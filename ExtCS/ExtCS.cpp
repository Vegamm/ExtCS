#include "ExtCS.h"
#include <msclr\auto_gcroot.h>


using namespace ExtCS::Debugger;
using namespace System;
using namespace System::Runtime::InteropServices; // Marshal

IDebugAdvanced2*  gAdvancedDebug2=NULL;
IDebugControl4*   gDebugControl4=NULL;
IDebugControl*    gExecuteCmd=NULL;
IDebugClient*     gDebugClient=NULL;

EXPORT HRESULT CALLBACK DebugExtensionInitialize(OUT PULONG Version, OUT PULONG Flags)
{
	HRESULT hr = S_OK; 
	hr = DebugCreate(__uuidof(IDebugClient), (void **)&gDebugClient); 
	if (hr != S_OK) {
		DbgPrintf(L"EXTCS: DebugExtensionInitialize failed to create DebugClient\n");
		return E_FAIL;
	}
	
	hr = gDebugClient->QueryInterface(__uuidof(IDebugControl4), (void **)&gDebugControl4); 
	if (hr != S_OK) {
		DbgPrintf(L"EXTCS: DebugExtensionInitialize failed to create DebugControl\n");
		return E_FAIL;
	}

	// Probably remove this.
	// Opens a log to save all output from the debugger.
	// gDebugControl4->OpenLogFile(filename, true);

	*Version = DEBUG_EXTENSION_VERSION(MAJOR_VERSION, MINOR_VERSION);
	*Flags = 0;

	return hr;
}

// Called on unload.
EXPORT void CALLBACK DebugExtensionUninitialize()
{
	DbgPrintf(L"EXTCS: DebugExtensionUninitialize\n");

	// Probably remove this.
	// gDebugControl4->CloseLogFile();
}

#pragma warning(disable:4793)

// Prints to the debugger
void DbgPrintf(const wchar_t* format, ...)
{
	wchar_t buffer[1024];

	va_list args;
	va_start(args, format);
	StringCchVPrintf(buffer, sizeof(buffer)/sizeof(buffer[0]), format, args);
	va_end(args);

	OutputDebugStringW(buffer);
}

#pragma warning(default:4793)

// This method calls into the managed code
// script is the argument passed from the debugger
HRESULT CallManagedCode(char * script)
{
	// Testing the Output is working
	// const char* outputText = "ExtCS calling managed code!";
	// gDebugControl4->Output(DEBUG_OUTCTL_ALL_CLIENTS, outputText);

	 // Calling managed debugger
 	 ManagedExtCS::Execute(gcnew System::String(script), 
		 (DotNetDbg::IDebugClient^)Marshal::GetObjectForIUnknown(IntPtr(gDebugClient)), 
		 (DotNetDbg::IDebugControl4^)Marshal::GetObjectForIUnknown(IntPtr(gDebugControl4)));
	 
	 // Clearing the global buffer of output callbacks
	 // this is never used but for safer side, it is cleared.
	 // in managed code, always a new output callback is installed to debugclient.
	 g_OutputCb.ClearOutPutBuffer();
	 return NULL;
}

//calling the MACRO to define execute method
DECLARE_API(execute)
//declaring the alias
DECLARE_API(ex)


HRESULT help(IDebugClient* debugClient, PCSTR args)
{
	char* sHelp="-help ";
	char* helpargs;
	if (args!=NULL && strlen(args)>0)
	{
		char result[100];   // array to hold the result.

		strcpy_s(result,sHelp); // copy string one into the result.
		strcat_s(result,args); // append string two to the result.	
		helpargs=result;
	}
	else
		helpargs=" -help all";
	
	return CallManagedCode(helpargs);
}

HRESULT clearscriptsession(IDebugClient* debugClient, PCSTR args)
{
	//passing a space after the -clear is important for the paramter regex  to parse the argument
	return CallManagedCode(" -clear ");
}

HRESULT debug(IDebugClient* debugClient, PCSTR args)
{
	//passing a space after the -clear is important for the paramter regex  to parse the argument
	return CallManagedCode(" -debug ");
}