#include "StdAfx.h"
#include "MessageProcessor.h"


MessageProcessor::MessageProcessor()
{
    GUID guid = {0};
    this->documentId = guid;

    InitializeCriticalSection(&this->criticalSection);

	//---- Create manually-reset events. Initial state is non-signaled.
    this->terminateEvent = CreateEvent(NULL, TRUE, FALSE, NULL); 
	DEBUG_ONLY(LogFileWriter::Assert((this->terminateEvent != 0), L"27171601 %d", GetLastError()));
    //---- Create the auto-reset signaling events.
    this->messageEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
	DEBUG_ONLY(LogFileWriter::Assert((this->messageEvent != 0), L"27171602 %d", GetLastError()));
    
    this->queue = new CAtlArray<MESSAGEITEM>;

    this->httpClient = new HttpClient();

    //---- Start the worker thread.
	unsigned int consumerThreadID;
	// Unlike the thread handle returned by _beginthread(), the thread handle
	// returned by _beginthreadex() can be used with the synchronization APIs.
	this->workerThread = (HANDLE)_beginthreadex( NULL,										// default security attributes
										        0,										// default stack size
										        MessageProcessor::ThreadEntryPoint,	    // thread function
										        this,									// thread function argument
										        CREATE_SUSPENDED,						// creation flags, so we can later call ResumeThread()
										        &consumerThreadID );					// receive thread identifier
    DEBUG_ONLY(LogFileWriter::Assert((this->workerThread != 0 ), L"27171603")); 

    // TODO Add supervising of the worker thread is alive and restart it on its failure.

    // terminate will be changed in ForceTerminate()
    this->terminate = (this->workerThread == 0);  

    if (! IsTerminating(L"4150"))
    {           
		DWORD  result, exitCode;
		result = GetExitCodeThread(this->workerThread, &exitCode);
        // exitCode should be STILL_ACTIVE = 0x00000103 = 259
        DEBUG_ONLY(LogFileWriter::Assert(((result != 0) && (exitCode == STILL_ACTIVE)), L"27171604 %d. %d", exitCode, GetLastError()));  

		result = ResumeThread(this->workerThread);
		DEBUG_ONLY(LogFileWriter::Assert((result != -1), L"28132800 %d", GetLastError()));
    }
}  // end of ctor


MessageProcessor::~MessageProcessor(void)
{
	//----- Command the working thread to finish explicitly.
	ForceTerminate();
	//----- Wait for the thread to finish.
	DWORD  waitResult = WaitForSingleObject(this->workerThread, TERMINATE_TIMEOUT);
	DEBUG_ONLY(LogFileWriter::Assert((waitResult == WAIT_OBJECT_0), L"27172801 %u. %d", waitResult, GetLastError()));
	//----- Check the thread's exit value. It is set in MessageProcessor::ThreadEntryPoint.
	DWORD  exitCode;
	GetExitCodeThread(this->workerThread, &exitCode);
	DEBUG_ONLY(LogFileWriter::Assert((exitCode == THREAD_EXIT_CODE), L"27172802 %d", exitCode));
	// The handle returned by _beginthreadex() has to be closed by the caller of _beginthreadex().
	CloseHandle(this->workerThread);

    //----- Release resources.
    CloseHandle(this->terminateEvent);
    CloseHandle(this->messageEvent);

    if (this->queue)
    {
        RemoveAllMessages();
        delete this->queue;
	    this->queue = NULL;
    }

    DeleteCriticalSection(&this->criticalSection);

    if (this->httpClient)
    {
        delete this->httpClient;
	    this->httpClient = NULL;
    }

    ////DEBUG_ONLY(LogFileWriter::Write(L"----- MessageProcessor finished"));
}  // end of dtor


void MessageProcessor::ForceTerminate()
{
    //----- Indicate to break the processing loop  
	this->terminate = true;
	//----- Wake up the thread.
	BOOL result = SetEvent(this->terminateEvent);
    DEBUG_ONLY(LogFileWriter::Assert((result != 0), L"27173201 %d", GetLastError()));

    //----- Abort an ongoing HTTP round-trip, if any.
    /* If a thread is blocking a call to Wininet.dll, another thread in the application can call InternetCloseHandle 
    * on the Internet handle being used by the first thread to cancel the operation and unblock the first thread. */
    if (this->httpClient)
    {
        this->httpClient->CloseRequest();
    }
}


bool MessageProcessor::IsTerminating(const wchar_t *questionSource)
{
    bool result = this->terminate;
    if (result) {
        // Marker is quasi-random. It is combination of two digits minute + two last digits code line number.
        DEBUG_ONLY(LogFileWriter::Write(L"24153900 %s", questionSource));
    }
    return result;
}


// The worker thread runs this static function as its thread function.
unsigned __stdcall MessageProcessor::ThreadEntryPoint(void *pThis)
{
    // Initializes the COM library on the current thread and identifies the concurrency model as single-thread apartment (STA).
    HRESULT hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    DEBUG_ONLY(LogFileWriter::Assert((hr == S_OK), L"22201401 %d", hr));

    MessageProcessor *processor = (MessageProcessor *)pThis;
    processor->Execute();  // calls Execute(), there is a "while(! this->terminate)" loop within it

    CoUninitialize();
    return THREAD_EXIT_CODE;   // the thread exit code. It is checked in dtor.
}


void MessageProcessor::Execute()
{
    HRESULT hr = this->httpClient->Initialize();
    if (FAILED(hr)) {
		ForceTerminate(); 
    }

    // Array used to pass event handles to WaitForMultipleObjects
    HANDLE events[2] = {this->terminateEvent, this->messageEvent};

	while (! this->terminate)   // flag variable used to control the worker thread's lifetime
    {
        // We now want to enter an "alertable wait state" so that
        // this consumer thread doesn't burn any cycles except
        // upon those occasions when the producer thread indicates
        // that a message waits for processing.
		DWORD  waitResult = WaitForMultipleObjects( sizeof(events)/sizeof(*events), // number of objects in array
													events,	                        // array of event objects
													FALSE,                          // wait for any object
													INFINITE);                      // wait until finished

		switch (waitResult)  
		{ 
			// events[0], Terminate event was signaled
			case WAIT_OBJECT_0 + 0:
				break; 
			// events[1], Message event was signaled. Do the job.
			case WAIT_OBJECT_0 + 1:
				ProcessMessages();
				break;
			// Returned value is invalid. WAIT_FAILED, WAIT_ABANDONED, WAIT_TIMEOUT
			default:
				DEBUG_ONLY(LogFileWriter::Write(L"27172901 %u. %d", waitResult, GetLastError()));
				ForceTerminate(); 
		}
    } 
}


HRESULT MessageProcessor::ProcessMessages()
{
    HRESULT hr = NOERROR;
    bool process = true;

    // If there are several messages in the queue, process them sequencially.
	while (process)
	{
        if (IsTerminating(L"4430")) {
            return E_ABORT;
        }    
    
        //----- Dequeue an item from the queue.
        MESSAGEITEM item = {NULL, NULL};

        // Prevent the message producer from blocking while waiting for the MessageProcessor thread to transfer a message via communication channel.
	    // Minimize period spent within the critical section. Process one message at a time.
        EnterCriticalSection(&this->criticalSection);
        process = ! this->queue->IsEmpty();
        if (process)
    	{
            item = this->queue->GetAt(0);
		    this->queue->RemoveAt(0);
	    }
	    LeaveCriticalSection(&this->criticalSection);

        // The queue may be empty. Nothing more to process in such a case.
        if (! process)
        {
            return hr;
        }

        if (! item.marshallingStream) {
            DEBUG_ONLY(LogFileWriter::Write(L"24160857 %d", item.marshallingStream));
            return E_FAIL;
        }

        //----- Deserialize the byte contents of the IStream object into an actual interface pointer to be used only within this thread.                                 */
        // Proxy pointer marshaled to the worker thread. All IE DOM objects are Single-threaded Apartment (STA) COM objects.
        // Release the proxy as soon as posible. If it is hold, it will freeze the thread at the destruction time.
        CComPtr<IHTMLDocument2> htmlDocument;  

        // The interface pointer will not be a direct pointer. It will be a proxy to the original pointer.
        // We need to call CoGetInterfaceAndReleaseStream as soon as possible in order to release the marshaling stream resources.
        HRESULT hr = CoGetInterfaceAndReleaseStream(item.marshallingStream,  __uuidof(IHTMLDocument2), (void**)&htmlDocument);
        if (FAILED(hr)) {
            // Even if the unmarshaling fails, the stream is still released.
            // Do not do marshalingStream->Release(), because the pointer is dungling at this point and it will crash the browser.
            DEBUG_ONLY(LogFileWriter::Write(L"23122801 %d", hr));
            return hr;
        }

        if ((item.message == NULL) || (! item.message.Length())) {
            DEBUG_ONLY(LogFileWriter::Write(L"24123501"));
            return E_FAIL;
        }
        
        if (IsTerminating(L"5961")) {
            return E_ABORT;
        }

        //----- Exchange messages with the server.
        CComBSTR script;
        hr = this->httpClient->ExchangeMessages(documentId, item.message, &script);
        if FAILED(hr) {
            // Error is reported within ExchangeMessages(). Don't duplicate error report.
            return hr;
        }

        if (IsTerminating(L"0507")) {
            return E_ABORT;
        }

        CComBSTR executionResult;
        hr = ScriptExecutor::ExecuteScript(htmlDocument, script, &executionResult);
        if FAILED(hr) {
            // Error is reported within ExecuteScript(). Don't duplicate error report.
            return hr;
        }

        ////CString str = executionResult;
        ////DEBUG_ONLY(LogFileWriter::Write(L"executionResult >>>%s<<<", str));

        // Ignore response.
        hr = this->httpClient->ExchangeMessages(documentId, executionResult, NULL);
        if FAILED(hr) {
            return hr;
        }
    }
    return hr;
}


HRESULT MessageProcessor::EnqueueMessage(const BSTR message, IWebBrowser2 *webBrowser)
{
    // Executed by a browser thread.
    
    if (IsTerminating(L"0654")) {
        return E_ABORT;
    }

    if ((message == NULL) || SysStringLen(message) == 0) {
        DEBUG_ONLY(LogFileWriter::Write(L"24112502"));
        return E_INVALIDARG;
    }

    //----- Marshal pointer to the HTML document object to the worker thread.
    // Query for an HTML document.
    CComPtr<IDispatch> document;
    HRESULT hr = webBrowser->get_Document(&document);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"24114301 %d", hr));
        return hr;
    }

    // CoMarshalInterThreadInterfaceInStream needs interface derived from IUnknown
    CComPtr<IUnknown> tempUnknown;
    hr = document->QueryInterface(IID_PPV_ARGS(&tempUnknown));
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"24114303 %d", hr));
        return hr;
    }

    // Declare an item to be added to the queue.
    MESSAGEITEM item = {NULL, NULL};

    // Once we have got the IUnknown pointer, we can serialize assosiated IHTMLDocument2 into a stream.
    hr = CoMarshalInterThreadInterfaceInStream(__uuidof(IHTMLDocument2), tempUnknown, &item.marshallingStream);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"24114304 %d", hr));
        return hr;
    }

    //----- Add message to the queue. Pass it to the processing procedure anyway since item.marshallingStream holds resources of a stream. 
    item.message = message;  // Implicitly creates CComBSTR
    this->queue->Add(item);

    //----- Since the queue holds now a message, signal to the waiting thread.
    BOOL res = SetEvent(this->messageEvent);
    if (res == FALSE) {
        DEBUG_ONLY(LogFileWriter::Write(L"24113002 %d", GetLastError()));
        return E_FAIL;
    }

    return hr;
}


void MessageProcessor::NewDocument()
{
    // Executed by a browser thread.
    // New document loaded. Abandon artefacts from the previous document.
    //----- Clear message queue.
    RemoveAllMessages();  
    //----- Abort receiving response for the previous document.
    this->httpClient->CloseRequest();
    //----- Generate new documentId.
    HRESULT hr = CoCreateGuid(&this->documentId);
    DEBUG_ONLY(LogFileWriter:: Assert(SUCCEEDED(hr), L"22180101 %d", hr));
}

void MessageProcessor::RemoveAllMessages()
{
    EnterCriticalSection(&this->criticalSection);
    this->queue->RemoveAll();
	LeaveCriticalSection(&this->criticalSection);
    // MessageEvent is an auto-reset event, so it is unable to reset manually.
}

