using System;
using System.Dynamic;

namespace ExtCS.Debugger
{
   public class Extension : DynamicObject
    {

		#region Fields

		const int S_OK = 0;
		private OutputHandler mOutputHandler = new OutputHandler();

		#endregion

		#region Constructors

		public Extension(string extensionName)
		{
			ExtensionDebugger = Debugger.GetCurrentDebugger();

			if (ExtensionDebugger is null)
			{
				throw new Exception("No debugger available.");
			}

			ExtensionHandle = ExtensionDebugger.GetExtensionHandle(extensionName);
		}

		#endregion

		#region Properties

		public ulong ExtensionHandle { get; set; }

		public unsafe Debugger ExtensionDebugger { get; set; }

		#endregion

		#region Public Methods

		// This doesn't seem like a necessary method. Consider removing.
		public string Call(string command, params string[] args)
		{
			//return CallExtensionMethod(commandname, CombineArgs(args));
			return this.ExtensionDebugger.Execute(command + "" + CombineArgs(args));
		}

		public string CallExtensionMethod(string method, string args)
		{
			IntPtr ptrOriginalOutputHandler;
			ExtensionDebugger.InstallCustomHandler(mOutputHandler, out ptrOriginalOutputHandler);

			int hr = ExtensionDebugger.DebugControl.CallExtensionWide(ExtensionHandle, method, args);
			if (hr != S_OK)
			{
				ExtensionDebugger.OutputError("unable to call extension method {0} with args {1}", method, args);
				return null;
			}

			ExtensionDebugger.DebugClient.FlushCallbacks();
			ExtensionDebugger.RevertCallBacks(ptrOriginalOutputHandler);

			return mOutputHandler.ToString();

		}

		#endregion

		#region Private Methods

		#endregion

		#region Private Static Methods

		private static string CombineArgs(object[] args)
		{
			string[] arguments = null;

			string combinedArg = string.Empty;

			if (args.Length > 0)
				arguments = new string[args.Length];

			foreach (var item in args)
			{
				if (!string.IsNullOrEmpty(item.ToString()))
					combinedArg += " " + item.ToString();
			}

			return combinedArg;
		}

		#endregion

		#region DynamicObject Overrides

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = CallExtensionMethod(binder.Name, null);
			if (result == null)
			{
				return false;
			}
			return true;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			string combinedArg = CombineArgs(args);

			result = CallExtensionMethod(binder.Name, combinedArg);
			if (result == null)
				return false;

			return true;
		}

		#endregion

	}
}
