//
// Contents of sosheap.csx
//

// References
#r "ExtCS.Debugger.dll"
#r "System.Data"
#r "System.Xml"

// Namespaces
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExtCS.Debugger;
using System.Data;
using System.Xml;
using System.Text;

/* Script Code! */

// Get an instance of the debugger.
var debugger = Debugger.GetCurrentDebugger();

// Load SOS.dll
var sos = new Extension("sos.dll");

// Execute !DumpHeap from SOS
var heapstat = sos.Call("!dumpheap -stat");

// Print the output to the debugger.
debugger.Output(heapstat);