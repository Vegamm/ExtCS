using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Scripting.CSharp;
using Roslyn.Scripting;
using DotNetDbg;
using System.IO;


namespace ExtCS.Debugger
{
	public class ManagedExtCS
	{
		#region Fields

		private static ManagedExtCS mManagedExtensionHost;

		// should these be private? -mv
		bool IsSessionPersisted = false;
		static bool mIsScript = false;
		static bool mIsDebugMode = false;
		static string mCSScript;
		static string mHistory;
		static string mParsedPath = null;

		#endregion

		#region Static Properties

		public static Debugger CSDebugger { get; set; }

		public static OptionSet OptionSet { get; set; }

		public static string Output { get; set; }

		public static unsafe IDebugClient ManagedDebugClient { get; set; }

		public static Session Session { get; set; }

		#endregion

		#region Constructor

		private ManagedExtCS() { }

		public static ManagedExtCS GetInstance()
		{
			return mManagedExtensionHost ?? new ManagedExtCS();
		}

		#endregion

		#region Public Static Methods

		public static string Execute(string args)
		{
			return Execute(args, null);
		}

		public static string Execute(string args, IDebugClient debugClient)
		{

			if (CSDebugger == null)
			{
				IDebugClient client;
				debugClient.CreateClient(out client);
				ManagedDebugClient = client;

				var windbgDebugger = new Debugger(client);
				CSDebugger = windbgDebugger;
			}
			//testig the execute function
			//string interoutput = windbgDebugger.Execute("k");
			//windbgDebugger.OutputLine(interoutput);
			bool persistSession = true;

			//windbgDebugger.OutputLine("starting {0} ", "Execute");
			Output = string.Empty;
			try
			{
				ArgumentsHelper arguments = new ArgumentsHelper(args);

				if (!arguments.HasArgument("-help"))
				{
					ScriptContext context = new ScriptContext();
					Debugger.GetCurrentDebugger().Context = context;
					context.Debug = mIsDebugMode;
					if (arguments.HasArgument("-file"))
					{
						mIsScript = false;
						mParsedPath = arguments["-file"];
						context.Args = arguments;
						context.ScriptLocation = Path.GetDirectoryName(mParsedPath);
						persistSession = true;

					}
					else if (arguments.HasArgument("-debug"))
					{
						if (mIsDebugMode)
						{
							mIsDebugMode = false;
							Debugger.GetCurrentDebugger().Output("Script debug mode is off\n");
						}
						else
						{
							mIsDebugMode = true;
							Debugger.GetCurrentDebugger().Output("Script debug mode is on\n");
						}
						return "";
					}
					else if (arguments.HasArgument("-clear"))
					{
						Session = null;
						DebuggerScriptEngine.Clear();
						Debugger.GetCurrentDebugger().Output("Script session cleared\n");
						Output = string.Empty;
						return "Session cleared";
					}
					else
					{
						mIsScript = true;
						mCSScript = args;
					}

					var session = CreateSession(CSDebugger, persistSession); //session.Execute("using WindbgManagedExt;");

					//Submission<Object> CSession = session.CompileSubmission<Object>(mCSScript);
					//var references = CSession.Compilation.References;
					//foreach (MetadataReference  reference in references)
					//{
					//    if (reference.Display.Contains("ExtCS.Debugger"))
					//        CSession.Compilation.RemoveReferences(reference);
					//}

					if (mIsScript)
						session.Execute(mCSScript);
					else
					{
						if (CSDebugger.Context.Debug)
							DebuggerScriptEngine.Execute(session, mParsedPath);
						else
							session.ExecuteFile(mParsedPath);
					}
				}
				else
					ShowHelp(arguments["-help"]);

			}
			catch (Exception ex)
			{

				Session = null;
				CSDebugger.OutputError("\n\nException while executing the script {0}", ex.Message);
				CSDebugger.OutputDebugInfo("\n Details: {0} \n", ex.ToString());

			}
			//windbgDebugger.OutputLine("ending Execute");

			CSDebugger.Output(Output);
			CSDebugger.Output("\n");
			Output = "";

			return Output;
		}

		#endregion

		#region Private Static Methods

		private static void ShowHelp(string command)
		{
			string sCreditBy = "Developed by   - Rohith Rajan (rohitra@microsoft.com)\n";
			string sStartText = " ExtCS - Extend your debugger using CSharp \n================================================\nHelp for ExtCS.dll\n";
			string sTextExecute = "<exec cmd=\"!extcs.help execute\">!execute</exec>\t\t ->execute a csharp script file e.g. <b>!execute -file c:\\scripts\\heapstat.csx </b>\n";
			string sTextDebug = "<exec cmd=\"!extcs.help debug\">!debug</exec>\t\t ->Toggles debugging flag e.g. <b>!debug</b> \n";
			string sTextClearScriptSession = "<exec cmd=\"!extcs.help clearscriptsession\">!clearscriptsession</exec>\t\t ->Clears the current script context session .This is useful when using !execute as a REPL \n";

			StringBuilder outStb = new StringBuilder();

			if (!String.IsNullOrEmpty(command))
			{
				command = command.Trim().ToUpperInvariant();
				switch (command)
				{
					case "EXECUTE":
					case "!EXECUTE":
					case "EX":
					case "!EX":
						outStb.Append("\n!execute (!ex)\n Execute a script a REPL C# statement\n");
						outStb.Append("Usage Details:\n");
						outStb.Append("\t !execute -file c:\\scripts\\heapdetails.csx:\n");
						outStb.Append("\t heapdetails.csx contains c# scripts to execute \n");
						break;
					case "CLEARSCRIPTSESSION":
					case "!CLEARSCRIPTSESSION":
						outStb.Append(sTextClearScriptSession);
						break;
					case "DEBUG":
					case "!DEBUG":
						outStb.Append("\n!debug (!ex)\n help to debug script better\n");
						outStb.Append("if this flag is enabled, When Executing script,it will emit extra details about internal commands running\n");
						outStb.Append(sTextDebug);
						break;
					case "ALL":
					case "all":
						outStb.Append(sStartText);
						outStb.AppendLine(sTextExecute).AppendLine(sTextDebug).AppendLine(sTextClearScriptSession);
						outStb.Append(sCreditBy);
						break;
					default:
						outStb.AppendFormat("\nunable to find help for command:{0} \n", command.ToLower());
						break;
				}
			}
			else
			{
				outStb.AppendLine(sTextExecute).AppendLine(sTextDebug).AppendLine(sTextClearScriptSession);
				outStb.Append(sCreditBy);
			}
			CSDebugger.Output(outStb.ToString());

		}

		private static Session CreateSession(Debugger currentDebugger, bool getOldSession)
		{
			//mCSScript = script;
			//var s = new CommonScriptEngine();

			if (getOldSession && Session != null)
				return Session;

			var asm = System.Reflection.Assembly.GetExecutingAssembly();

			var csharpEngine = new ScriptEngine(null, null);

			//csharpEngine.AddReference("System.Diagnostics");
			csharpEngine.ImportNamespace("System");
			csharpEngine.ImportNamespace("System.Collections.Generic");
			//csharpEngine.ImportNamespace("System.Linq");
			csharpEngine.ImportNamespace("System.Text");
			csharpEngine.ImportNamespace("System.IO");
			//csharpEngine.ImportNamespace("System.Diagnostics");

			csharpEngine.SetReferenceSearchPaths(asm.Location);
			csharpEngine.AddReference(typeof(System.Diagnostics.Debug).Assembly);
			csharpEngine.AddReference(typeof(System.Dynamic.DynamicObject).Assembly);
			csharpEngine.AddReference(asm);
			csharpEngine.ImportNamespace("ExtCS.Debugger");

			Session = csharpEngine.CreateSession(currentDebugger);

			return Session;
		}

		#endregion

	}
}
