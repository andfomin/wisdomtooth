// HelperObject.cpp : Implementation of CHelperObject

#include "stdafx.h"
#include "ScriptExecutor.h"
#include "HelperObject.h"


// CHelperObject

STDMETHODIMP CHelperObject::SetSite(IUnknown *site)
{
    if (site != NULL)
    {
        //----- Cache the pointer to IWebBrowser2.
        HRESULT hr = site->QueryInterface(IID_IWebBrowser2, (void **)&this->webBrowser);
        if (SUCCEEDED(hr))
        {
            //----- Get ready to work;
		    Initialize();
            //----- Register to sink events from DWebBrowserEvents2.
            hr = DispEventAdvise(this->webBrowser);
            if (SUCCEEDED(hr))
            {
                this->advised = true;
            }
        }
	}
    else
    {
        //----- Unregister event sink.
        if (this->advised)
        {
            DispEventUnadvise(this->webBrowser);
            this->advised = false;
        }
		//----- Clean up.
		Uninitialize();
        this->webBrowser.Release();  // The interface is released, and CComPtrBase::p is set to NULL.
    }

    //----- Call base class implementation.
    return IObjectWithSiteImpl<CHelperObject>::SetSite(site);
}


void CHelperObject::Initialize()
{
    this->messageProcessor = new MessageProcessor();
}


void CHelperObject::Uninitialize()
{
    if (this->messageProcessor) {
        delete this->messageProcessor;
        this->messageProcessor = NULL;
    }
}


bool CHelperObject::IsTopFrame(IDispatch *disp)
{
    // Query for the IWebBrowser2 interface.
    CComQIPtr<IWebBrowser2> tempWebBrowser = disp;
    // Is this event associated with the top-level browser?
    return tempWebBrowser && this->webBrowser && this->webBrowser.IsEqualObject(tempWebBrowser);
}

void STDMETHODCALLTYPE CHelperObject::OnDocumentComplete(IDispatch *disp, VARIANT *url)
{
    if (IsTopFrame(disp))
    {
        // TODO: Move this to OnBeforeNavigate()
        this->messageProcessor->NewDocument();

        //////----- Get the current document object from browser.

        CComPtr<IDispatch> dispatchDocument;
        HRESULT hr = this->webBrowser->get_Document(&dispatchDocument);
        if (FAILED(hr)) {
            DEBUG_ONLY(LogFileWriter::Write(L"14114301 %d", hr));
            return;
        }

        //----- Query for the HTML document.
        CComPtr<IHTMLDocument2> htmlDocument;
        hr = dispatchDocument->QueryInterface(IID_PPV_ARGS(&htmlDocument));
        if (FAILED(hr)) {
            DEBUG_ONLY(LogFileWriter::Write(L"14114302 %d", hr));
            return;
            /* WinError.h
            E_NOINTERFACE 0x80004002L (-2147467262)
            */
        }

        CComBSTR documentURL;
        hr = htmlDocument->get_URL(&documentURL);
        if (FAILED(hr)) {
            DEBUG_ONLY(LogFileWriter::Write(L"14114318 %d", hr));
            return;
        }

        CStringW tempStr;
        tempStr = (LPCWSTR)documentURL;
        tempStr.Replace(L"&", L"&amp;");
        documentURL = tempStr;

        ////CComBSTR request0(L"{\"messageType\":\"{DA9138A9-3C9D-407E-978D-CD03BF0062EA}\",\"url\":\"");  // JSON notation
        CComBSTR request(L"<message type=\"DA9138A9-3C9D-407E-978D-CD03BF0062EA\"><url>");  // XML notation
        request.AppendBSTR(documentURL);
        request.Append(L"</url></message>");

        this->messageProcessor->EnqueueMessage(request, this->webBrowser);             
    }
}
