using System.Collections.Generic;
using System.Collections;
using System;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ExtCS.Debugger;

Extension sos = new Extension(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\sos.dll");
Extension netext = new Extension("netext.dll");

List<string> data = new List<string>();
string output = sos.CallExtensionMethod("!dumpheap", "-stat -type Pega.Remoting.OpenSpanIPCProxy -short");

// Parse the output
Regex regex = new Regex(@">(?<address>[a-fA-F0-9]{8})<", RegexOptions.Multiline);
MatchCollection matches = regex.Matches(output);
string address = "";

foreach (Match match in matches)
{

	GroupCollection groups = match.Groups;
	address = groups["address"].Value;
	string dumpObject = netext.CallExtensionMethod("!wdo", address);

	// Want to change the type or value to search for? Change it here.
	if (dumpObject.Contains("IVirtualHtmlCollection"))
	{
		data.Add(address);
	}

}

string fileName = "_FindAddressWithType.txt";
int prefix = 1;
bool fileExists = File.Exists(fileName);

while (fileExists)
{
	fileName = Convert.ToString(prefix) + fileName;
	fileExists = File.Exists(fileName);
	prefix++;
}

File.WriteAllLines(fileName, data);