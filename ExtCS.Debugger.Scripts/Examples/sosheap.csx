// Contents of sosheap.csx

var debugger = Debugger.GetCurrentDebugger();

// Add extension path to search path
//debugger.Execute(".extpath+\"C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\"");

// We must use the absolute path to load sos.
var sos = new Extension(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\sos.dll");

// Make sure to not use '!' for the method to execute on the extension.
sos.CallExtensionMethod("dumpheap", "-stat");