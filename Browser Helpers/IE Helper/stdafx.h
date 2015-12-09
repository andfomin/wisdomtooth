// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#include "targetver.h"

#define _ATL_APARTMENT_THREADED

#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit


#define ATL_NO_ASSERT_ON_DESTROY_NONEXISTENT_WINDOW

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>
#include <atlctl.h>

// Custom code

#pragma comment( lib, "wininet.lib" )  // for InternetOpen and etc.

#include <atlstr.h>  // for CStringW
#include <shlobj.h>  // for SHGetKnownFolderPath
#include <comutil.h>  // for _variant_t
#include "atlcoll.h"  // for CAtlArray
//#include <shlguid.h>  // IID_IWebBrowser2, DIID_DWebBrowserEvents2, etc.
#include <exdispid.h>  // for DISPID_DOCUMENTCOMPLETE, etc.
#include <wininet.h>  // for InternetOpen and so on

// Definition of DEBUG_ONLY copied from afx.h. "#include <afx.h>" causes a trouble on compile. _DEBUG is defined by the compiler.
#if defined( MC_WRITE_LOG ) // _DEBUG
#define DEBUG_ONLY(f)      (f)
#else
#define DEBUG_ONLY(f)      ((void)0)
#endif 

using namespace ATL; // for CStringW, CComBSTR, CComVariant, CComPtr
