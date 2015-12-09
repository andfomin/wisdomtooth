#include "StdAfx.h"
#include "LogFileWriter.h"


LogFileWriter::LogFileWriter(void)
{
}


LogFileWriter::~LogFileWriter(void)
{
}


// Declared within the LogFileWriter class in LogFileWriter.h. Non-integral type must be defined outside the class body.
const wchar_t LogFileWriter::DIRECTORY_NAME[] = L"\\MediaCurator"; // For instance, "C:\Users\John\AppData\LocalLow\MediaCurator"
const wchar_t LogFileWriter::FILE_NAME[] = L"\\MediaCurator.log";

// If an unhandled exception occurs within this procedure, IE reports the exception and falls. 
void LogFileWriter::WriteEntry(CString text)
{
	HRESULT hr;
	CString rootDir;

	////if ( Is_Vista_or_Later() )
	////{
		/* IEIsProtectedModeProcess is only supported in Windows Vista or later. 
        // ////#pragma comment( lib, "iepmapi.lib" )  // for IEIsProtectedModeProcess and IEGetWriteableFolderPath
        // ////#include <iepmapi.h>  // for IEIsProtectedModeProcess and IEGetWriteableFolderPath 
		// If called from earlier versions of Microsoft Windows, this function returns E_NOTIMPL?
		BOOL bProtectedMode = FALSE;
		hr = IEIsProtectedModeProcess ( &bProtectedMode ); 
		if ( SUCCEEDED( hr ) && bProtectedMode )  // IE is running in protected mode
		{ }
		else  // IE isn't running in protected mode. It may one of the following cases: is started as "Run As Administrator", or the page is a local file, or in Intranet zone, or in Trusted Sites (default setting is off), (or on XP, or it is IE6).
		{ } */

	//----- Get the path to a dir that we're allowed to write to in the protected mode.
		// On Vista, SHGetFolderPath is simply a wrapper for SHGetKnownFolderPath. SHGetFolderPathEx is undocumented. 
		/* IEGetWriteableFolderPath returns E_ACCESSDENIED for FOLDERID_LocalAppDataLow, although we can actually write to it by specifying the path explicitly.
		   For FOLDERID_InternetCache it reports S_OK, but a following writting fails. 
		   hr = IEGetWriteableFolderPath(FOLDERID_LocalAppDataLow, &pwszRootDir); */

	LPWSTR buffer = NULL;
	hr = SHGetKnownFolderPath(FOLDERID_LocalAppDataLow, 0, NULL, &buffer);
	if (SUCCEEDED(hr))  
	{
		rootDir = buffer;
		CoTaskMemFree(buffer);

	}
	////}
	////else
	////{
	////	wchar_t szPath[MAX_PATH];
	////	hr = SHGetFolderPath(NULL, CSIDL_LOCAL_APPDATA,  NULL, SHGFP_TYPE_CURRENT, szPath);
	////	if ( SUCCEEDED(hr) )  
	////		rootDir = szPath;
	////}

	// If we have found the root directory.
	if (SUCCEEDED(hr))
	{
		//----- Test the existence of the custom directory.
		CString customDir = rootDir + DIRECTORY_NAME;
		if (GetFileAttributes(customDir) != INVALID_FILE_ATTRIBUTES)
		{
            // Since we log for debug purposes, we intentionally want the directory to be created manually as a means for preventing 
            // writing log file on the user machine in an unfortunate case this code is mistakenly included into a production version.
			////if (! CreateDirectory(customDir, NULL)) 
			////	customDir = rootDir;  // If the directory creation failed, resort to the root dir.
		
		    CString filePath = customDir + FILE_NAME;

		    SYSTEMTIME time;  // Contains date/time. It is impossible to get milliseconds using CTime.
		    GetSystemTime(&time);  // UTC

            //----- Compose entry text
		    CString entryText = L"";  
		    entryText.Format(L"%4d/%02d/%02d %02d:%02d:%02d.%03d %s",
			                time.wYear, time.wMonth, time.wDay, time.wHour, time.wMinute, time.wSecond, time.wMilliseconds,
			                text);

		    //----- Write to file.
            FILE* file;
            errno_t error = -1;
            // "a" means open for writing at the end of the file (appending) without removing the EOF marker before writing new data to the file; creates the file first if it doesn't exist.
            error = _wfopen_s(&file, filePath , L"a");
		    // Files opened by _wfopen_s are not sharable. If another process has opened the file, wait, but not forever, since writing is synchronous.
            if (error != 0) 
            {
                Sleep(100);
                error = _wfopen_s(&file, filePath , L"a");
            }

            if (error == 0)
            {
		        fwprintf_s(file, L"%s\r\n", entryText);
		        fclose(file);
            }
        }
	}
}


////bool LogFileWriter::Is_Vista_or_Later ()
////{
////   // Initialize the OSVERSIONINFOEX structure.
////   OSVERSIONINFOEX osvi;
////   ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
////   osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
////   osvi.dwMajorVersion = 6; // for Vista and later
////   //osvi.wProductType = VER_NT_SERVER;  // is Server
////
////   // Initialize the condition mask.
////   DWORDLONG dwlConditionMask = 0;
////   VER_SET_CONDITION( dwlConditionMask, VER_MAJORVERSION, VER_GREATER_EQUAL );
////   //VER_SET_CONDITION( dwlConditionMask, VER_PRODUCT_TYPE, VER_EQUAL );
////
////   // Perform the test.
////   return (VerifyVersionInfoW( &osvi, VER_MAJORVERSION, dwlConditionMask) != 0);  // VER_PRODUCT_TYPE
////}


// Format and write the data we are given.
void LogFileWriter::Write(const wchar_t *formatString, ...)
{
	try  // defence in the case of a wrong format string
	{
        CString str;
		va_list args;
	    va_start(args, formatString);
		str.FormatV(formatString, args);
		va_end( args ); 
		WriteEntry( str );
	}
	catch (...)
	{
	    CString strErr;
		strErr.Format(L"Formatting error. %s", formatString);
		WriteEntry(strErr);
	}
}


bool LogFileWriter::Assert(bool condition, const wchar_t *formatString, ...)
{
	if (! condition) 
	{
		va_list args;
	    va_start(args, formatString);
		Write(formatString, args);
		va_end(args); 
	}
	return condition;
}