#include "StdAfx.h"
#include "HttpClient.h"

static const wchar_t USER_AGENT[] = L"Media Curator IE Helper/1.0";
// TODO : Remove condition
#if defined( _DEBUG )
    static const wchar_t SERVER_NAME[] = L"miner";
#else
    static const wchar_t SERVER_NAME[] = L"localhost";
#endif

static const wchar_t OBJECT_NAME[] = L"/mediacurator/helper/";
static const wchar_t CUSTOMHEADER[] = L"X-MediaCurator-DocumentId: %s\r\n";
static const int SEND_TIMEOUT = 1000; // msec
static const int RECEIVE_TIMEOUT = 2000; // msec


HttpClient::HttpClient(void)
{
    InitializeCriticalSection(&this->criticalSection);
    this->internet = NULL;
    this->session = NULL;
    this->request = NULL;
}


HttpClient::~HttpClient(void)
{
    EnterCriticalSection(&this->criticalSection);
    if (this->request) {
        if (! InternetCloseHandle(this->request)) {
            DEBUG_ONLY(LogFileWriter::Write(L"22113801 %d", GetLastError()));
        }
        this->request = NULL;
    }
    if (this->session) {
        if (! InternetCloseHandle(this->session)) {
            DEBUG_ONLY(LogFileWriter::Write(L"22113802 %d", GetLastError()));
        }
        this->session = NULL;
    }
    if (this->internet) {
        if (! InternetCloseHandle(this->internet)) {
            DEBUG_ONLY(LogFileWriter::Write(L"22113803 %d", GetLastError()));
        }
        this->internet = NULL;
    }
	LeaveCriticalSection(&this->criticalSection);
    DeleteCriticalSection(&this->criticalSection);
}


HRESULT HttpClient::Initialize()
{
    // Use synchronous mode. Sample code +http://code.google.com/p/google-breakpad/source/browse/trunk/src/common/windows/http_upload.cc

    // TODO : Remove condition
#if defined( _DEBUG )
    this->internet = InternetOpen(USER_AGENT, INTERNET_OPEN_TYPE_PRECONFIG, NULL, NULL, 0);
#else  // !_DEBUG
    this->internet = InternetOpen(USER_AGENT, INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
#endif

    if (! this->internet)
    { 
        DEBUG_ONLY(LogFileWriter::Write(L"14123801 %d", GetLastError()));
        return E_FAIL;
    }

    // For HTTP InternetConnect returns synchronously because it does not actually make the connection.
    this->session = InternetConnect(this->internet, SERVER_NAME, INTERNET_DEFAULT_HTTP_PORT, NULL, NULL, INTERNET_SERVICE_HTTP, NULL, INTERNET_NO_CALLBACK);
    if (! this->session)
    { 
        DEBUG_ONLY(LogFileWriter::Write(L"14113401 %d", GetLastError()));
        return E_FAIL;
    }
    
    SetSessionAttributes(session);

    return NOERROR;
}


HRESULT HttpClient::InitializeNewRequest()
{
    // It seems redundant, however it does not hurt. 
    CloseRequest();

    // Request handle holds a request to be sent to an HTTP server.
    DWORD flags = INTERNET_FLAG_NO_CACHE_WRITE || INTERNET_FLAG_NO_COOKIES || INTERNET_FLAG_NO_UI 
                    || INTERNET_FLAG_PRAGMA_NOCACHE || INTERNET_FLAG_RELOAD;
        
    EnterCriticalSection(&this->criticalSection);
    this->request = HttpOpenRequest(this->session, L"POST", OBJECT_NAME, NULL, NULL, NULL, flags, 0);
    LeaveCriticalSection(&this->criticalSection);

    if (! this->request)
    { 
        DEBUG_ONLY(LogFileWriter::Write(L"14121401 %d", GetLastError()));
        return E_FAIL;
    }
    return NOERROR;
}


void HttpClient::CloseRequest()
{
    EnterCriticalSection(&this->criticalSection);
    if (this->request) 
    {
        if (! InternetCloseHandle(this->request)) {
            DEBUG_ONLY(LogFileWriter::Write(L"22114701 %d", GetLastError()));  // ERROR_INVALID_HANDLE = 6
        }
        this->request = NULL;
    }
	LeaveCriticalSection(&this->criticalSection);
}


HRESULT HttpClient::ExchangeMessages(REFGUID documentId, const BSTR requestText, /*[out]*/ BSTR *responseText)
{
    HRESULT hr = InitializeNewRequest();
    if (SUCCEEDED(hr)) 
    {
        hr = SetRequestAttributes(this->request, documentId);
        if (SUCCEEDED(hr)) 
        {
            hr = SendRequest(this->request, requestText);
            if (SUCCEEDED(hr))
            {
                hr = ReadResponse(this->request, responseText);
            }
        }
        CloseRequest();
    }
    return hr;
}


// static
HRESULT HttpClient::StaticExchangeMessages(REFGUID documentId, const BSTR requestText, /*[out]*/ BSTR *responseText)
{
    HRESULT result = E_UNEXPECTED;

    // Use synchronous mode. Sample code +http://code.google.com/p/google-breakpad/source/browse/trunk/src/common/windows/http_upload.cc

// TODO : Remove condition
#if defined( _DEBUG )
    HINTERNET internet = InternetOpen(USER_AGENT, INTERNET_OPEN_TYPE_PRECONFIG, NULL, NULL, 0);
#else  // !_DEBUG
    HINTERNET internet = InternetOpen(USER_AGENT, INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
#endif

    DEBUG_ONLY(LogFileWriter::Assert((internet != NULL), L"14123801 %d", GetLastError() ));

    if (internet)
    {
        // For HTTP InternetConnect returns synchronously because it does not actually make the connection.
        HINTERNET session = InternetConnect(internet, SERVER_NAME, INTERNET_DEFAULT_HTTP_PORT, NULL, NULL, INTERNET_SERVICE_HTTP, NULL, INTERNET_NO_CALLBACK);
        DEBUG_ONLY(LogFileWriter::Assert((session != NULL), L"14113301 %d", GetLastError()));

        if (session)
        {
            SetSessionAttributes(session);

            // Request handle holds a request to be sent to an HTTP server.
            DWORD flags = INTERNET_FLAG_NO_CACHE_WRITE || INTERNET_FLAG_NO_COOKIES || INTERNET_FLAG_NO_UI 
                            || INTERNET_FLAG_PRAGMA_NOCACHE || INTERNET_FLAG_RELOAD;
            HINTERNET request = HttpOpenRequest(session, L"POST", OBJECT_NAME, NULL, NULL, NULL, flags, 0);
            DEBUG_ONLY(LogFileWriter::Assert((request != NULL), L"14121401 %d", GetLastError()));

            if (request)
            { 
                if (SUCCEEDED(SetRequestAttributes(request, documentId)))
                {
                    if (SUCCEEDED(SendRequest(request, requestText)))
                    {
                        result = ReadResponse(request, responseText);
                    }
                }

                if (! InternetCloseHandle(request)) 
                {
                    DEBUG_ONLY(LogFileWriter::Write(L"15221101 %d", GetLastError()));
                }
            }

            if (! InternetCloseHandle(session)) 
            {
                DEBUG_ONLY(LogFileWriter::Write(L"15221102 %d", GetLastError()));
            }
        }
        
        if (! InternetCloseHandle(internet)) 
        {
            DEBUG_ONLY(LogFileWriter::Write(L"15221103 %d", GetLastError()));
        }
    }

    return result;
}


//static
void HttpClient::SetSessionAttributes(HINTERNET session)
{
    //----- Since we work with localhost, set shorter timeouts. 
    // Failure in these methods is not critical.
    BOOL res = InternetSetOption(session, INTERNET_OPTION_SEND_TIMEOUT, (LPVOID)&SEND_TIMEOUT, sizeof(SEND_TIMEOUT));
    DEBUG_ONLY(LogFileWriter::Assert((res == TRUE), L"15220101 %d", GetLastError()));

    res = InternetSetOption(session, INTERNET_OPTION_RECEIVE_TIMEOUT, (LPVOID)&RECEIVE_TIMEOUT, sizeof(RECEIVE_TIMEOUT));
    DEBUG_ONLY(LogFileWriter::Assert((res == TRUE), L"15220102 %d", GetLastError()));
}


// static
HRESULT HttpClient::SetRequestAttributes(HINTERNET request, REFGUID documentId)
{
    //----- Construct custom header to report instance Id.
    CComBSTR id(documentId);  // CComBSTR constructor can convert GUID
    CString header;
    header.Format(CUSTOMHEADER, id);

    if (! HttpAddRequestHeaders(request, header, header.GetLength(), HTTP_ADDREQ_FLAG_ADD))
    {
        DEBUG_ONLY(LogFileWriter::Write(L"16154601 %d", GetLastError()));
        return E_FAIL;
    }

    return NOERROR;
}


// static
HRESULT HttpClient::SendRequest(HINTERNET request, const BSTR requestText)
{
    //----- Convert Unicode to UTF-8
    int srcLength = SysStringLen(requestText);
    // Get the required length
    int dstLength = WideCharToMultiByte(CP_UTF8, 0, requestText, srcLength, 0, 0, 0, 0);
    if (!dstLength) 
    {
        DEBUG_ONLY(LogFileWriter::Write(L"17192101 %d", GetLastError()));
        return E_FAIL;
    }

    CStringA requestBody;
    // nMinBufferLength - The minimum number of characters that the character buffer can hold. This value does not include space for a null terminator.
    LPSTR buffer = requestBody.GetBuffer(dstLength);

    if (WideCharToMultiByte(CP_UTF8, 0, requestText, srcLength, buffer, dstLength, 0, 0) != dstLength) 
    {
        DEBUG_ONLY(LogFileWriter::Write(L"17192102 %d", GetLastError()));
        return E_FAIL;
    }

    ////CString debugStr(buffer);
    ////DEBUG_ONLY(LogFileWriter::Write(L"130503 buffer %s %d", debugStr, dstLength));

    // Buffer length should be in bytes.
    if (! HttpSendRequest(request, NULL, 0, (LPVOID)buffer, dstLength))
    {
        DEBUG_ONLY(LogFileWriter::Write(L"15204301 %d", GetLastError()));
        return E_FAIL;
        /* WinInet.h
        ERROR_INTERNET_TIMEOUT 12002
        ERROR_INTERNET_OPERATION_CANCELLED 12017
        ERROR_INTERNET_CANNOT_CONNECT 12029
        ERROR_INTERNET_CONNECTION_ABORTED 12030
        */
    }

    return NOERROR;
}


// static
HRESULT HttpClient::ReadResponse(HINTERNET request, /*[out]*/ BSTR *responseText)
{
    // Passing the address of an initialized CComBSTR to a function as an [out] parameter causes a memory leak.
    // If bstr is NULL, the function simply returns.
    if (responseText) 
    {
        SysFreeString(*responseText);
    }

    // After the request is sent, the status code and response headers from the HTTP server are read. 
    // These headers are maintained internally and are available to client applications through the HttpQueryInfo function.
    
    DWORD httpStatus, contentLength;
    DWORD numberSize = sizeof(httpStatus);
    DWORD headerIndex = 0;

    // Execution blocks here waiting for the server response. May be interrupted by another thread by calling CloseRequest().
    if (! HttpQueryInfo(request, HTTP_QUERY_STATUS_CODE | HTTP_QUERY_FLAG_NUMBER, &httpStatus, &numberSize, &headerIndex))
    {
        DEBUG_ONLY(LogFileWriter::Write(L"16213702 %d", GetLastError()));
        return E_FAIL;
    }

    if (httpStatus != HTTP_STATUS_OK)
    {
        DEBUG_ONLY(LogFileWriter::Write(L"16215901 %d", httpStatus));
        return E_FAIL;
    }

    BOOL contentLengthKnown = HttpQueryInfo(request, HTTP_QUERY_CONTENT_LENGTH | HTTP_QUERY_FLAG_NUMBER, &contentLength, &numberSize, &headerIndex);
    DEBUG_ONLY(LogFileWriter::Assert((contentLengthKnown == TRUE), L"16221101 %d", GetLastError()));

    CStringA buffer, responseBody; // UTF-8 expected
    DWORD bytesAvailable = 0;
    DWORD totalRead = 0;
    BOOL returnCode;

    // Read response body in chunks
    while (((returnCode = InternetQueryDataAvailable(request, &bytesAvailable, 0, 0)) == TRUE) && (bytesAvailable > 0)) 
    {
        DWORD sizeRead;
        returnCode = InternetReadFile(request, buffer.GetBuffer(bytesAvailable), bytesAvailable, &sizeRead);

        if (returnCode && (sizeRead > 0)) 
        {
            totalRead += sizeRead;
            responseBody.Append(buffer, sizeRead);
        } 
        else
        {
            break;
        }
    }

    if (!returnCode || (contentLengthKnown && (totalRead != contentLength))) 
    {
        DEBUG_ONLY(LogFileWriter::Write(L"17183501 %d %d %d %d", returnCode, contentLengthKnown, totalRead, contentLength));
        return E_FAIL;
    }

    if (contentLengthKnown && (contentLength == 0))
    {
        // Server has nothing to say.
        return NOERROR;
    }

    //----- Convert UTF-8 to Unicode
    // The return value is the required size, in characters, for the buffer indicated by lpWideCharStr.
    // Since we provide exact length in chars, the responseTextLength will not include a terminating null character.
    int srcLength = responseBody.GetLength();
    int dstLength = MultiByteToWideChar(CP_UTF8, 0, responseBody, srcLength, 0, 0);
    if (!dstLength) 
    {
        DEBUG_ONLY(LogFileWriter::Write(L"17174401 %d", GetLastError()));
        return E_FAIL;
    }
        
    // When you implement a function that returns a BSTR, allocate the string but do not free it. The receiving the function releases the memory.
    // Allocating and Releasing Memory for a BSTR +http://msdn.microsoft.com/en-us/library/xda6xzx7%28v=VS.100%29.aspx
    // Allocates a total of responseTextLength plus one characters.
    BSTR temp = SysAllocStringLen(NULL, dstLength); 
    if (temp == NULL) 
    {
        DEBUG_ONLY(LogFileWriter::Write(L"17174501 %d", GetLastError()));
        return E_FAIL;
    }

    if (MultiByteToWideChar(CP_UTF8, 0, responseBody, srcLength, temp, dstLength) != dstLength) 
    {
        DEBUG_ONLY(LogFileWriter::Write(L"17175501 %d, %d", dstLength, GetLastError()));
        return E_FAIL;
    }

    if (responseText) 
    {
        *responseText = temp;
    }

    return NOERROR;
}







