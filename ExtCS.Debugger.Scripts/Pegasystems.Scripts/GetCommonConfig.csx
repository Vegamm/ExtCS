/// Attempts to extract the CommonConfig data from a dump of OpenSpan.Runtime.exe and save it to a file.

// References
#r "System"
#r "ExtCS.Debugger.dll"

// Namespaces
using System;
using ExtCS.Debugger;

///
/// Script Code
///

//Chained method requests are a no-go
//Escaped characters are a no-go

Debugger debugger = Debugger.GetCurrentDebugger();

// Load necessary extensions
Extension sos = new Extension(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\sos.dll");
Extension netext = new Extension("netext.dll");

// Call sos !dumpheap
string output = sos.CallExtensionMethod("dumpheap", "-stat -type OpenSpan.Configuration.ConfigManifestHelper -short");

// Splits output.
string[] separator = new string[1];
separator[0] = Utilities.NEW_LINE;
StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries;
string[] manifests = output.Split(separator, options);

//var rgMt = new Regex(@"MethodTable:\W(?<address>\S*)", System.Text.RegularExpressions.RegexOptions.Multiline);

string address = manifests[0]; // CommonConfig is always the first address
string manifestOffset = "0x04";
string manifestDocumentOffset = "0x10";

debugger.Output(Utilities.NEW_LINE + address);

UInt64 manifest = debugger.ReadPointer(address, manifestOffset);

string value = manifest.ToString("X");
debugger.Output(Utilities.NEW_LINE + value);

//UInt64  = debugger.ReadPointer(manifest.ToString("X"), manifestDocumentOffset);