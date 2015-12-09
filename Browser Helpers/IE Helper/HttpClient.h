#pragma once

#include "LogFileWriter.h"

class HttpClient
{
public:
    HttpClient(void);
    ~HttpClient(void);

    // Deferred initialization
    HRESULT Initialize();

    /* If a thread is blocking a call to Wininet.dll, another thread in the application can call InternetCloseHandle 
     * on the Internet handle being used by the first thread to cancel the operation and unblock the first thread. */
    void CloseRequest();

    /* When you implement a function that returns a BSTR, allocate the string but do not free it. The receiving the function releases the memory.
     * (Source: Allocating and Releasing Memory for a BSTR. +http://msdn.microsoft.com/en-us/library/xda6xzx7%28v=VS.100%29.aspx) */
    /* Beware! Passing the address of an initialized CComBSTR to a function as an [out] parameter causes a memory leak.
     * (Source: Programming with CComBSTR. +http://msdn.microsoft.com/en-us/library/bdyd6xz6.aspx#programmingwithccombstr_memoryleaks) */
    // Instance method
    HRESULT ExchangeMessages(REFGUID documentId, const BSTR requestText, /*[out]*/ BSTR *responseText);
    // Static
    static HRESULT StaticExchangeMessages(REFGUID documentId, const BSTR requestText, /*[out]*/ BSTR *responseText);

private:
    HINTERNET internet;
    HINTERNET session;
    HINTERNET request;
    CRITICAL_SECTION criticalSection; 

    HRESULT InitializeNewRequest();

    static void SetSessionAttributes(HINTERNET session);
    static HRESULT SetRequestAttributes(HINTERNET request, REFGUID instanceId);
    static HRESULT SendRequest(HINTERNET request, const BSTR requestText);
    static HRESULT ReadResponse(HINTERNET request, /*[out]*/ BSTR *responseText);
};

