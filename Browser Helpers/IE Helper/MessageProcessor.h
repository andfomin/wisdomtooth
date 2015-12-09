#pragma once

#include "LogFileWriter.h"
#include "HttpClient.h"
#include "ScriptExecutor.h"

class MessageProcessor
{
public:
    MessageProcessor();
    ~MessageProcessor(void);

private:

	static const int TERMINATE_TIMEOUT = 5000;  // The time-out interval for waiting while the sender thread terminates, in milliseconds. 
    static const int THREAD_EXIT_CODE = 1;  // The thread's exit value. It is set in MessageProcessor::ThreadEntryPoint. 

    struct MESSAGEITEM
    {
        CComBSTR message;
        LPSTREAM marshallingStream;
    };

    GUID documentId;
    bool terminate;  // Flag variable used to control the worker thread lifetime
	HANDLE workerThread;  // Worker thread
    HANDLE terminateEvent;  // Used to wake up the thread on termination.
    HANDLE messageEvent;
    CAtlArray<MESSAGEITEM> *queue;
    CRITICAL_SECTION criticalSection;
    HttpClient *httpClient;
	
	static unsigned __stdcall ThreadEntryPoint(void *pThis);  // Used as the worker thread's thread function. Thread function should be static.

    void Execute();  // Called from ThreadEntryPoint()
	void ForceTerminate();
	HRESULT ProcessMessages();
    bool IsTerminating(const wchar_t *questionSource);
    void RemoveAllMessages();

public:

    void NewDocument();  // Exected by the browser thread.
    HRESULT EnqueueMessage(const BSTR message, IWebBrowser2 *webBrowser);
};

