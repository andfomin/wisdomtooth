// HelperObject.h : Declaration of the CHelperObject

#pragma once
#include "resource.h"       // main symbols
#include "IEHelper_i.h"

#include "LogFileWriter.h" 
#include "MessageProcessor.h" 


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

using namespace ATL;


// CHelperObject

class ATL_NO_VTABLE CHelperObject :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CHelperObject, &CLSID_HelperObject>,
	public IObjectWithSiteImpl<CHelperObject>,
	public IDispatchImpl<IHelperObject, &IID_IHelperObject, &LIBID_IEHelperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
    	// IDispEventImpl provides an easy and safe alternative to Invoke for handling events.
	// We specify that we want to handle events defined by the DWebBrowserEvents2 
	public IDispEventImpl<1, CHelperObject, &DIID_DWebBrowserEvents2, &LIBID_SHDocVw, 1, 1>  

{
public:
	CHelperObject()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_HELPEROBJECT)

DECLARE_NOT_AGGREGATABLE(CHelperObject)

BEGIN_COM_MAP(CHelperObject)
	COM_INTERFACE_ENTRY(IHelperObject)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IObjectWithSite)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:

// Custom code

    
    STDMETHOD(SetSite)(IUnknown *site);  // Method of IObjectWithSite

	// Macros that route the event to a new OnDocumentComplete event handler method
	// The number supplied to the SINK_ENTRY_EX macro (1) refers to the first parameter of the IDispEventImpl class definition 
	// and is used to distinguish between events from different interfaces, if necessary. 
	BEGIN_SINK_MAP(CHelperObject)
		SINK_ENTRY_EX(1, DIID_DWebBrowserEvents2, DISPID_DOCUMENTCOMPLETE, OnDocumentComplete)
	END_SINK_MAP()

    // DWebBrowserEvents2
    void STDMETHODCALLTYPE OnDocumentComplete(IDispatch *disp, VARIANT *url);

private:

    CComPtr<IWebBrowser2> webBrowser;  // Store the browser site
    bool advised;  // Track whether the object has established a connection with the browser
    MessageProcessor *messageProcessor;

	void Initialize();  // Called from within IObjectWithSite.SetSite when site != NULL
	void Uninitialize();  // Called from within IObjectWithSite.SetSite when site == NULL

    bool IsTopFrame(IDispatch *disp); // Is this the top-level browser? Compare the object with _webBrowser.

// End custom code
};

OBJECT_ENTRY_AUTO(__uuidof(HelperObject), CHelperObject)
