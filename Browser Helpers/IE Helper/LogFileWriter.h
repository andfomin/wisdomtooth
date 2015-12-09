#pragma once
/* LogFileWriter.h : Declaration of the LogFileWriter.
** Writes log messages to a file on disk. 
** Since it is called from BHO in the Protected Mode, it writes to folder FOLDERID_LocalAppDataLow.
*/

class LogFileWriter
{
private:
	// Non-integral type must be defined outside the class body. Defined in LogFileWriter.cpp
	static const wchar_t DIRECTORY_NAME[]; 
	static const wchar_t FILE_NAME[];

	static void WriteEntry(CString text);  // Actual writing is done here

public:
    LogFileWriter(void);
    ~LogFileWriter(void);

	static void Write(const wchar_t *formatString, ...);  // Perform formating and write the data we are given.
	static bool Assert(bool condition, const wchar_t *formatString, ...);  // If the condition is false, write formatted GetLastError.
	////static bool Is_Vista_or_Later (); 

};

