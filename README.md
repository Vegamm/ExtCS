# ExtCS

C# Extension to access the windows DebugEngine.

This repo was forked to make this code usable in newer versions of Visual Studio and Windows Debugging Tools. The Web/ScriptApi has been removed and further improvements have been made to provide more functionality against WinDbg.

## Purpose

This extension allows you to write C# scripts against WinDbg.

### Why do I need this?

If you want to automate the debugger but dislike the WinDbg built-in scripting, now you can use C# language and all the framework libraries.

## Getting Started

* Install Debugging Tools for Windows 10 - https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/debugger-download-tools
* Download and extract the extension dlls to WinDbg directory.
* Load the extension in WinDbg with the following command: .load extcs
* Try executing a pre-defined script: !execute -file HelloWorld.csx

### Example Script
```cs
/// Contents of sosheap.csx

// Get an instance of the debugger
var debugger = Debugger.GetCurrentDebugger();

// Load the sos extension
var sos = new Extension(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\sos.dll");

// Call '!dumpheap -stat' on sos
sos.CallExtensionMethod("dumpheap", "-stat");
```

## Supported Features

* Full intellisense support on Visual Studio Code when writing scripts and partial support in Visual Studio.
* Supports all the Windows Debugging Tools that utilize the debug engine.
* Complete debugging support. Add a breakpoint in the C# script and attach Visual Studio to windbg to do debugging.
* REPL support – You can use your debugger to execute C# statements directly in the command window.

## License

This project is licensed under the MIT License - see [LICENSE.md](LICENSE.md).
