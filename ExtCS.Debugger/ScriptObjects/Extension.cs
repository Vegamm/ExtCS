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

		public Extension(string extensionFilePath)
		{
			ExtensionDebugger = Debugger.GetCurrentDebugger();

			if (ExtensionDebugger is null)
			{
				throw new Exception("No debugger available.");
			}

            ulong extensionHandle;
            int hr = ExtensionDebugger.DebugControl.AddExtension(extensionFilePath, 0, out extensionHandle);
            if (hr == S_OK)
            {
                ExtensionHandle = extensionHandle;
                Extension.ExtensionLoadedEvent(this, new ExtensionLoadedEventArgs(extensionFilePath));
            }
            else
            {
                throw new Exception($"Failed to load extension with path: {extensionFilePath}");
            }
		}

		#endregion

		#region Properties

		public ulong ExtensionHandle { get; set; }

		public unsafe Debugger ExtensionDebugger { get; set; }

		#endregion

		#region Public Methods

		public string CallExtensionMethod(string method, string args)
		{
			IntPtr ptrOriginalOutputHandler;
			ExtensionDebugger.InstallCustomHandler(mOutputHandler, out ptrOriginalOutputHandler);

            if (method.StartsWith("!"))
            {
                method = method.TrimStart('!');
            }

			int hr = ExtensionDebugger.DebugControl.CallExtensionWide(ExtensionHandle, method, args);
			if (hr != S_OK)
			{
				ExtensionDebugger.Output($"Unable to call extension method [{method}] with args [{args}]");
				return null;
			}

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

        #region Events

        public delegate void ExtensionLoaded(Extension extension, ExtensionLoadedEventArgs args);

        public static event ExtensionLoaded ExtensionLoadedEvent;

        public class ExtensionLoadedEventArgs : EventArgs
        {
            public ExtensionLoadedEventArgs(string extensionName)
            {
                ExtensionFilePath = extensionName;
            }

            public string ExtensionFilePath { get; set; }
        }

        #endregion

    }
}
