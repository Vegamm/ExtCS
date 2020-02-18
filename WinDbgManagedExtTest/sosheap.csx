//contents of heapstat.csx
#r "C:\Users\Mitchell\Desktop\windbg\ExtCS.Debugger.dll"
#r "System.Data"
#r "System.Xml"
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExtCS.Debugger;
using System.Data;
using System.Xml;
using System.Text;

var d = Debugger.GetCurrentDebugger();
//load the sos.dll
var sos=new Extension("sos.dll");
//call the command on sos.dll and get tehe output to variable heapstat
var heapstat=sos.Call("!dumpheap -stat");
//output heapstat to debugger
d.Output(heapstat);




