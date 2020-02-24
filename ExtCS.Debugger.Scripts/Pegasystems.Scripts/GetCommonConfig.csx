/// Attempts to extract the CommonConfig data from a dump of OpenSpan.Runtime.exe and save it to a file.

// References
#r "ExtCS.Debugger.dll"

// Namespaces
using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ExtCS.Debugger;

Debugger debugger = Debugger.GetCurrentDebugger();

// Load extensions
Extension sos = new Extension(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\sos.dll");
Extension netext = new Extension("netext.dll");

// Execute windbg command for !dumpheap
string output = sos.CallExtensionMethod("dumpheap", "-stat -type OpenSpan.Configuration.ConfigManifestHelper -short");

// Parse the output
Regex regex = new Regex(@">(?<address>[a-fA-F0-9]{8})<", RegexOptions.Multiline);
MatchCollection matches = regex.Matches(output);
string address = "";

foreach (Match match in matches)
{
	GroupCollection groups = match.Groups;
	address = groups["address"].Value;
	break; // CommonConfig is always the first address. We can break out here.
}

// Calculate the first pointer!
string manifestOffset = "4";

UInt64 dereferencedAddress = debugger.POI(address, manifestOffset);
string addressToManifest = dereferencedAddress.ToString("X");
debugger.Output($"{Environment.NewLine} address dereferenced to -> {addressToManifest}{Environment.NewLine}");

// Calculate the second pointer!
string manifestDocumentOffset = "10";

dereferencedAddress = debugger.POI(addressToManifest, manifestDocumentOffset);
string addressToDocument = dereferencedAddress.ToString("X");
debugger.Output($"{Environment.NewLine} address dereferenced to -> {addressToDocument}{Environment.NewLine}");

// Execute !wxml {address}
string document = netext.CallExtensionMethod("wxml", addressToDocument);

// Save the output to a file
document = WebUtility.HtmlDecode(document);
document = document.Trim();
File.WriteAllText(@"CommonConfig.xml", document);