using System;
using System.Text;
using Roslyn.Scripting.CSharp;
using Roslyn.Scripting;
using DotNetDbg;
using System.IO;
using System.Reflection;


namespace ExtCS.Debugger
{
	public class ManagedExtCS
	{
		#region Fields

		private static ManagedExtCS mManagedExtensionHost;

		private static readonly Assembly SYSTEM_DIAGNOSTICS_DEBUG_ASM = typeof(System.Diagnostics.Debug).Assembly;
		private static readonly Assembly SYSTEM_DYNAMIC_DYNAMICOBJECT_ASM = typeof(System.Dynamic.DynamicObject).Assembly;
		private static readonly Assembly EXTCS_DEBUGGER_ASM = Assembly.GetExecutingAssembly();

		//bool IsSessionPersisted = false;
		//static string mHistory;

		private static bool mIsScript;
		private static bool mIsDebugMode;
		private static string mCSScript;
		private static string mParsedPath;

		#endregion

		#region Static Properties

		public static Debugger CSDebugger { get; set; }

		public static OptionSet OptionSet { get; set; }

		public static string Output { get; set; }

		// TODO: Consider removing this and leaving the debugClient solely to the Debugger.
		public static unsafe IDebugClient ManagedDebugClient { get; set; }

		public static Session Session { get; set; }

		#endregion

		#region Constructor

		private ManagedExtCS() { }

		public static ManagedExtCS GetInstance()
		{
			mManagedExtensionHost = mManagedExtensionHost ?? new ManagedExtCS();
			return mManagedExtensionHost;
		}

		#endregion

		#region Public Static Methods

		public static string Execute(string args)
		{
			return Execute(args, null);
		}

		// This currently does not work because the debugControl does not exist.
		// Debugger constructor needs logic to query for the Com object.
		public static string Execute(string args, IDebugClient debugClient)
		{

			if (CSDebugger is null)
			{
				IDebugClient client;
				debugClient.CreateClient(out client);
				ManagedDebugClient = client;
				CSDebugger = new Debugger(client);
			}

			// Testing the execute function
			//string test = CSDebugger.Execute("k");
			//CSDebugger.Output(test);

			bool useExistingSession = true;

			//CSDebugger.OutputLine("starting {0} ", "Execute");

			Output = string.Empty;

			// TODO: separate each case into its own method.
			try
			{
				ArgumentsHelper arguments = new ArgumentsHelper(args);

				if (arguments.HasArgument("-help") == false)
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
						useExistingSession = true;

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

					Session session = CreateSession(CSDebugger, useExistingSession); 
					
					//session.Execute("using WindbgManagedExt;");
					//Submission<Object> CSession = session.CompileSubmission<Object>(mCSScript);
					//var references = CSession.Compilation.References;
					//foreach (MetadataReference  reference in references)
					//{
					//    if (reference.Display.Contains("ExtCS.Debugger"))
					//        CSession.Compilation.RemoveReferences(reference);
					//}

					if (mIsScript)
					{
						var result = session.Execute(mCSScript);

						// TODO: Remove after done debugging application.
						Debugger.GetCurrentDebugger().Output(result);
					}
					else
					{
						if (CSDebugger.Context.Debug)
						{
							DebuggerScriptEngine.Execute(session, mParsedPath);
						}
						else
						{
							session.ExecuteFile(mParsedPath);
						}
					}
				}
				else
				{
					ShowHelp(arguments["-help"]);
				}
			}
			catch (Exception ex)
			{
				Session = null;
				CSDebugger.OutputError("\n\nException while executing the script {0}", ex.Message);
				CSDebugger.OutputDebugInfo("\n Details: {0} \n", ex.ToString());
			}

			//CSDebugger.Output("ending Execute");

			CSDebugger.Output(Output);
			CSDebugger.Output("\n");
			Output = string.Empty;

			// What exactly are we returning here?
			return Output;
		}

		public static string Execute(string args, IDebugClient debugClient, IDebugControl4 debugControl)
		{

			if (debugClient != null && CSDebugger == null)
			{
				// Whats the purpose of creating a client here if it is already provided as a parameter?
				//IDebugClient client;
				//debugClient.CreateClient(out client);
				ManagedDebugClient = debugClient;
				CSDebugger = new Debugger(debugClient, debugControl);
			}

			bool useExistingSession = true;

			//CSDebugger.OutputLine("starting {0} ", "Execute");

			Output = string.Empty;

			// TODO: separate each case into its own method.
			try
			{
				ArgumentsHelper arguments = new ArgumentsHelper(args);

				if (arguments.HasArgument("-help") == false)
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
						useExistingSession = true;
					}
					else if (arguments.HasArgument("-debug"))
					{
						mIsDebugMode = !mIsDebugMode;
						string debugState = (mIsDebugMode ? "on" : "off");
						Debugger.GetCurrentDebugger().Output($"Script debug mode is {debugState}.\n");
						return "";
					}
					else if (arguments.HasArgument("-clear"))
					{
						Session = null;
						DebuggerScriptEngine.Clear();
						Debugger.GetCurrentDebugger().Output("Script session cleared.\n");
						Output = string.Empty;
						return "Session cleared";
					}
					else
					{
						mIsScript = true;
						mCSScript = arguments.Args;
					}

					Session session = CreateSession(CSDebugger, useExistingSession);

					if (mIsScript)
					{
						var result = session.Execute(mCSScript);
						Debugger.GetCurrentDebugger().Output(result);
					}
					else
					{
						if (CSDebugger.Context.Debug)
						{
							DebuggerScriptEngine.Execute(session, mParsedPath);
						}
						else
						{
							session.ExecuteFile(mParsedPath);
						}
					}
				}
				else
				{
					ShowHelp(arguments["-help"]);
				}
			}
			catch (Exception ex)
			{
				Session = null;
				CSDebugger.OutputError("\n\nException while executing the script {0}", ex.Message);
				CSDebugger.OutputDebugInfo("\n Details: {0} \n", ex.ToString());
			}

			//CSDebugger.Output("ending Execute");

			CSDebugger.Output(Output);
			CSDebugger.Output("\n");
			Output = string.Empty;

			// What exactly are we returning here? -mv
			return Output;
		}
		#endregion

		#region Private Static Methods

		private static void ShowHelp(string command)
		{
			string sCreditBy = "Developed by - Mitchell Vega (vegamm215@gmail.com)\n";
			string sStartText = " ExtCS - Extend your debugger using CSharp \n================================================\nHelp for ExtCS.dll\n";
			string sTextExecute = "<exec cmd=\"!extcs.help execute\">!execute</exec> -> Execute a csharp script file e.g. <b>!execute -file c:\\scripts\\sosheap.csx</b>\n";
			string sTextDebug = "<exec cmd=\"!extcs.help debug\">!debug</exec> -> Toggles debugging flag e.g. <b>!debug</b>\n";
			string sTextClearScriptSession = "<exec cmd=\"!extcs.help clearscriptsession\">!clearscriptsession</exec> -> Clears the current script context session. This is useful when using !execute as a REPL.\n";

			StringBuilder outStb = new StringBuilder();

			if (String.IsNullOrEmpty(command) == false)
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

		private static Session CreateSession(Debugger currentDebugger, bool useExistingSession)
		{
			if (useExistingSession && Session != null)
				return Session;

			var scriptEngine = new ScriptEngine();

			scriptEngine.ImportNamespace("System");
			scriptEngine.ImportNamespace("System.Collections");
			scriptEngine.ImportNamespace("System.Collections.Generic");
			scriptEngine.ImportNamespace("System.Text");
			scriptEngine.ImportNamespace("System.IO");

			//scriptEngine.SetReferenceSearchPaths(EXTCS_DEBUGGER_ASM.Location);
			scriptEngine.AddReference(SYSTEM_DIAGNOSTICS_DEBUG_ASM);
			scriptEngine.AddReference(SYSTEM_DYNAMIC_DYNAMICOBJECT_ASM);
			scriptEngine.AddReference(EXTCS_DEBUGGER_ASM);

			// This can only be imported after the reference has been added.
			scriptEngine.ImportNamespace("ExtCS.Debugger");

			Session = scriptEngine.CreateSession(CSDebugger, CSDebugger.GetType());

			return Session;
		}

		#endregion

	}
}
