/// Attempts to extract the CommonConfig data from a dump of OpenSpan.Runtime.exe and save it to a file.

using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ExtCS.Debugger;

var sosPath = @"C:\Users\vegam\Desktop\storage\Tools\DebugTools_x32\sym\SOS_x86_x86_4.7.3468.00.dll\5D490E656ef000\SOS_x86_x86_4.7.3468.00.dll";
var debugger = Debugger.GetCurrentDebugger();
var sos = new Extension(sosPath);
var netext = new Extension("netext");

var output = sos.CallExtensionMethod("dumpheap", "-stat -type OpenSpan.Configuration.ConfigManifestHelper -short");

// Parse the output.
var regex = new Regex(@">(?<address>[a-fA-F0-9]{8})<", RegexOptions.Multiline);
var match = regex.Match(output);
var groups = match.Groups;
var address = groups["address"].Value;

// Calculate the address of the document.
var manifestOffset = "4";
var manifestDocumentOffset = "10";
var dereferencedAddress = debugger.POI(address, manifestOffset, manifestDocumentOffset);
var addressToDocument = dereferencedAddress.ToString("X");

// Execute !wxml {address}.
var document = debugger.Execute("!wxml " + addressToDocument);

// Clean the output.
document = WebUtility.HtmlDecode(document);
document = document.Trim();

// Save to file.
File.WriteAllText(@"CommonConfig.xml", document);
