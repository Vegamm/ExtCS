using Roslyn.Scripting;
using System;
using System.Linq;
using System.IO;
using System.Reflection;

namespace ExtCS.Debugger
{
	public class DebuggerScriptEngine
	{
		//Error	1	'ExtCS.Debugger.DebuggerScriptEngine' 
		//does not implement inherited abstract member 
		//'Roslyn.Scripting.CommonScriptEngine.CreateCompilation(Roslyn.Compilers.IText, string, bool, Roslyn.Scripting.Session, 
		//System.Type, Roslyn.Compilers.DiagnosticBag)'	C:\tfssourcecode\ExtCS\ExtCS\WindbgManagedExt\DebuggerScriptEngineSession.cs
		//10	18	WindbgManagedExt

		#region Fields

		private const string COMPILED_SCRIPT_CLASS = "Submission#0";
		private const string COMPILED_SCRIPT_METHOD = "<Factory>";

		private static AppDomain mDebuggerDomain;

		#endregion

		#region Public Static Methods

		public static void Clear()
		{
			if (mDebuggerDomain != null)
			{
				if (!mDebuggerDomain.IsFinalizingForUnload())
				{
					AppDomain.Unload(mDebuggerDomain);
					mDebuggerDomain = null;
				}
			}
		}

		public static Object Execute(Session session, string path)
		{
			Submission<object> submission;
			object returnValue = null;
			string code = null;

			try
			{
				if (session is null)
				{
					Debugger.GetCurrentDebugger().OutputDebugInfo("Session is null!");
					return null;
				}

				code = File.ReadAllText(path);
				submission = session.CompileSubmission<object>(code);
			}
			catch (Exception compileException)
			{
				Debugger.GetCurrentDebugger().OutputDebugInfo("Exception on compile submission.");
				throw compileException;
			}

			byte[] exeBytes = new byte[0];
			byte[] pdbBytes = new byte[0];
			bool compileSuccess = false;

			using (var exeStream = new MemoryStream())
			using (var pdbStream = new MemoryStream())
			{
				var result = submission.Compilation.Emit(exeStream, pdbStream: pdbStream);

				compileSuccess = result.Success;

				//File.WriteAllBytes(@"c:\scripts\dynamic.dll", exeBytes.ToArray());

				if (result.Success)
				{
					Debugger.GetCurrentDebugger().OutputDebugInfo("Compilation was successful.");
					exeBytes = exeStream.ToArray();
					pdbBytes = pdbStream.ToArray();
				}
				else
				{
					var errors = String.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
					Debugger.GetCurrentDebugger().OutputDebugInfo("Error occurred when compiling: {0})", errors);
				}
			}

			if (compileSuccess)
			{
				Debugger.GetCurrentDebugger().OutputDebugInfo("Loading assembly into appdomain.");
				// if(mDebuggerDomain==null)
				//   mDebuggerDomain = AppDomain.CreateDomain("mDebuggerDomain");

				var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
				Debugger.GetCurrentDebugger().OutputDebugInfo("Retrieving compiled script class (reflection).");
				var type = assembly.GetType(COMPILED_SCRIPT_CLASS);
				Debugger.GetCurrentDebugger().OutputDebugInfo("Retrieving compiled script method (reflection).");
				var method = type.GetMethod(COMPILED_SCRIPT_METHOD, BindingFlags.Static | BindingFlags.Public);

				try
				{
					Debugger.GetCurrentDebugger().OutputDebugInfo("Invoking method.");
					returnValue = method.Invoke(null, new[] { session });
				}
				catch (Exception executeException)
				{
					var message = $"Exception Message: {executeException.InnerException?.Message}\nStack Trace:{executeException.InnerException?.StackTrace}";
					Debugger.GetCurrentDebugger().OutputDebugInfo("An error occurred when executing the scripts.\n");
					Debugger.GetCurrentDebugger().OutputDebugInfo(message);
					
					// AppDomain.Unload(mDebuggerDomain);
					mDebuggerDomain = null;
					throw executeException;
				}
			}

			return returnValue;
		}

		#endregion

	}
}
