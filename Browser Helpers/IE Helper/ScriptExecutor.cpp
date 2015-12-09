/* Based on +http://code.google.com/p/selenium/source/browse/trunk/cpp/IEDriver/Script.cpp 
which is Copyright 2011 Software Freedom Conservatory and licensed under the Apache License, Version 2.0
*/
#include "StdAfx.h"
#include "ScriptExecutor.h"

// static
HRESULT ScriptExecutor::ExecuteScript(IHTMLDocument2 *document, const BSTR script, /*out*/ BSTR *result)
{
    HRESULT hr = NOERROR;

    if (result) 
    {
        SysFreeString(*result);
    }

    OLECHAR* functionObjectName = L"_media_curator_script";

    CComBSTR code(L"window.document.");
    code.Append(functionObjectName);
    code.Append(L" = ");
    code.AppendBSTR(script);

    CComBSTR language(L"JScript");

    CComPtr<IHTMLWindow2> window;
    hr = document->get_parentWindow(&window);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"26191801 %d", hr));
        return hr;
    }

    //----- Inject anonymous function into the document.
    // The script have to be an Immediately-Invoked Function Expression (IIFE) which returns a function which in turn returns a string.
    // For example, "(function(){return function(){return 'abc';};})();"
    CComVariant execScriptReturn;
    hr = window->execScript(code, language, &execScriptReturn);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"26214977 %d", hr));  
        return hr;
        /* WinError.h
        E_ACCESSDENIED 0x80070005L (-2147024891)
        Error 80020101 (-2147352319) - something is wrong with the script being exected, any sintactical/logical/somewhat error.
        */
    }

    // It is documented that execScript() does not return anything back, execScriptReturn is a dummy parameter. 
    // We will invoke the function as a COM object through IDispatch .

    //----- Get the pointer to the function.
    DISPID functionDispId;
    hr = document->GetIDsOfNames(IID_NULL, &functionObjectName, 1, LOCALE_USER_DEFAULT, &functionDispId);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"26214985 %d", hr));
        return hr;
    }

    DISPPARAMS noParams = {NULL, NULL, 0, 0};
    CComVariant function;
    
    hr = document->Invoke(functionDispId, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_PROPERTYGET, &noParams, &function, NULL, NULL);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"26215296 %d", hr));
        return hr;
    }

    if (function.vt != VT_DISPATCH) {    
        DEBUG_ONLY(LogFileWriter::Write(L"22192801 SUCCESS"));  // No return value
        return E_INVALIDARG;
    }

    //----- Get the "call" method out of the returned function! This is the trick!
    DISPID methodDispId;
    OLECHAR* methodName = L"call";
    
    hr = function.pdispVal->GetIDsOfNames(IID_NULL, &methodName, 1, LOCALE_USER_DEFAULT, &methodDispId);
    if (FAILED(hr)) {
        DEBUG_ONLY(LogFileWriter::Write(L"25091157 %d", hr));  // Cannot locate call method on anonymous function
        return hr;
    }

    //----- Execute the function
    EXCEPINFO exception;
    memset(&exception, 0, sizeof(exception));

    CComVariant functionResult;

    hr = function.pdispVal->Invoke(methodDispId, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &noParams, &functionResult, &exception, NULL);
    if (FAILED(hr)) {
        if (hr == DISP_E_EXCEPTION && exception.bstrDescription) {
            DEBUG_ONLY(LogFileWriter::Write(L"261042961 %d %s", hr, exception.bstrDescription));
        }
        else {
            // If error in JavaScript: 0x80020101 (-2147352319) SCRIPT_E_REPORTED
            DEBUG_ONLY(LogFileWriter::Write(L"25059102 %d", hr));
        }
        return hr;
    }

    if (functionResult.vt != VT_BSTR) {
        DEBUG_ONLY(LogFileWriter::Write(L"26095212 %d", functionResult.vt));
        return E_INVALIDARG;
    }

    if (result) 
    {
      *result = SysAllocString(functionResult.bstrVal);
    }
    functionResult.Clear();

    return hr;
}
