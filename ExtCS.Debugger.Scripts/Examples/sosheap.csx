// Contents of sosheap.csx


// Get the instance of the Debugger.
var debugger = Debugger.GetCurrentDebugger();

// We must use the absolute path to load sos.
var sos = new Extension(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\sos.dll");

// Call the extension method: !dumpheap -stat
sos.CallExtensionMethod("dumpheap", "-stat");