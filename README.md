## EXTCS - C# Extension to access windows Debug Engine
=====

### Purpose

This extension allows you to write C# scripts against windbg (or other debugging tools like cdb.exe, ntsd.exe, etc). With the help of this extension, you can write windbg scripts in C# utilizing the vast array of .NET libraries available.

Why do I need this?

If you want to automate the debugger but dislike the WinDbg built-in scripting, now you can use C# language and all the framework libraries. Even if you don't want to create your own script, maybe some existing scripts will be of interest to you.

### Supported Features

- Full intellisense support on Visual Studio Code when writing scripts and partial support in Visual Studio.
- Supports all the windows debugging tools that utilize the debug engine.
- Complete debugging support. Add a breakpoint in the C# script and attach Visual Studio to windbg to do debugging.
- REPL support – You can use your debugger to execute C# statements directly in the command window.

### Quick start

* Install Debugging Tools for Windows 10 - https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/debugger-download-tools
* Download and extract the extension dlls to WinDbg directory.
* Load the extension in WinDbg with the following command: .load extcs
* Now try executing a script: !execute -file HelloWorld.csx

Here’s a simple example of a script that is executed with the extension:

!execute –file sosheap.csx

```cs
/// Contents of sosheap.csx

// Get an instance of the debugger.
var debugger = Debugger.GetCurrentDebugger();

// Load sos
var sos = new Extension("C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\sos.dll");

// Call '!dumpheap -stat' on sos
sos.CallExtensionMethod("dumpheap", "-stat");
```
