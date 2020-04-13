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

var adapterOffset = "c";
var data = new List<string>();

var output = debugger.Execute("!dumpheap /d -mt 0be270e8 -short");
debugger.Output(output);

// Parse the output.
var regex = new Regex(@">(?<address>[a-fA-F0-9]{8})<", RegexOptions.Multiline);
var matches = regex.Matches(output);
foreach (Match match in matches)
{
	var groups = match.Groups;
	var address = groups["address"].Value;
	var dereferencedAddress = debugger.POI(address, adapterOffset);
	var adapter = dereferencedAddress.ToString("X");

	output = debugger.Execute("!wdo " + adapter);
	var delimiters = new char[2];
	delimiters[0] = '\u000D';
	delimiters[1] = '\u000A';
	var outputLines = output.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
	foreach (var line in outputLines)
	{
		if (line.Contains("System.String") || line.Contains("System.Boolean") || line.Contains("System.Int32"))
		{
			var rgx = new Regex(@"[\w]{8}\s*(?<type>\w*\.\w*)\s\+[\w]*\s*(?<property>\w*)\s(\<.*\<\/link\>\s|[a-fA-F0-9]*\s)(?<val>.*)");
			var mtch = rgx.Match(line);
			var grp = mtch.Groups;
			var formattedLine = string.Format("{0,40} | {1}", grp["property"].Value, grp["val"].Value);
			data.Add(formattedLine);
		}
	}
	data.Add("------------------------------------------------------");
}

// Save to file.
File.WriteAllLines(@"AdapterProperties.txt", data);
