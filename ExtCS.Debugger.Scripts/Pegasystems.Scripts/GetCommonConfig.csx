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

///
/// Script Code
///

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

ulong addr;
UInt64.TryParse(address, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr);
debugger.Output(Utilities.NEW_LINE + "addr: " + addr.ToString());

ulong offset;
UInt64.TryParse(manifestOffset, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset);
debugger.Output(Utilities.NEW_LINE + "offset: " + offset.ToString());

ulong result = addr + offset;
debugger.Output(Utilities.NEW_LINE + "result: " + result.ToString("X"));

UInt64 manifest = Convert.ToUInt64(result);
UInt64 destAddress = debugger.ReadPointer(manifest);

string dest = destAddress.ToString("X");
debugger.Output(Utilities.NEW_LINE + "address to -> " + dest + Utilities.NEW_LINE);

// Calculate the second pointer!
string manifestDocumentOffset = "10";

ulong addr1;
UInt64.TryParse(dest, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr1);
debugger.Output(Utilities.NEW_LINE + "addr1: " + addr1.ToString());

ulong offset1;
UInt64.TryParse(manifestDocumentOffset, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset1);
debugger.Output(Utilities.NEW_LINE + "offset1: " + offset1.ToString());

ulong result1 = addr1 + offset1;
debugger.Output(Utilities.NEW_LINE + "result1: " + result1.ToString("X"));

UInt64 manifest1 = Convert.ToUInt64(result1);
UInt64 destAddress1 = debugger.ReadPointer(manifest1);

string commonConfig = destAddress1.ToString("X");
debugger.Output(Utilities.NEW_LINE + "address1 to -> " + commonConfig + Utilities.NEW_LINE);

// Execute !wxml {address}
string document = netext.CallExtensionMethod("wxml", commonConfig);

// Save the output to a file
document = WebUtility.HtmlDecode(document);
document = document.Trim();
File.WriteAllText(@"CommonConfig.xml", document);
