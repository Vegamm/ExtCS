/// Contents of sosheap.csx
using ExtCS.Debugger;

// Get an instance of the debugger.
var debugger = Debugger.GetCurrentDebugger();

// Load sos
var sos = new Extension("C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\sos.dll");

// Call '!dumpheap -stat' on sos
sos.CallExtensionMethod("dumpheap", "-stat");