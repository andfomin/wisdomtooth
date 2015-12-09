#pragma once
/* Based on +http://code.google.com/p/selenium/source/browse/trunk/cpp/IEDriver/Script.cpp 
which is Copyright 2011 Software Freedom Conservatory and licensed under the Apache License, Version 2.0
*/

#include "LogFileWriter.h"

class ScriptExecutor
{
public:
    HRESULT static ExecuteScript(IHTMLDocument2 *document, const BSTR script, /*out*/ BSTR *result);
};

