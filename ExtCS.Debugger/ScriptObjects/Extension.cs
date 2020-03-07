using System;
using System.Dynamic;

using DotNetDbg;

namespace ExtCS.Debugger
{
	public class Extension : DynamicObject
	{

		#region Fields

		private OutputHandler mOutputHandler = new OutputHandler();

		#endregion

		#region Constructors

		/// <summary>
		/// Gets an instance of the debugger and attempts to load the extension
		/// from the provided path.
		/// </summary>
		/// <param name="extensionFilePath">Path to the extension.</param>
		public Extension(string extensionFilePath)
		{
			ExtensionDebugger = Debugger.GetCurrentDebugger();

			if (ExtensionDebugger is null)
			{
				throw new Exception("No debugger available.");
			}

			// TODO: This needs logic to check if the extensionHandle is an invalid address (meaning that the loading of the module failed because of a bad path). We could also intercept the outputhandler and check for certain output.

			// If the extension has already been loaded,
			// the existing extension handle is returned.
			ulong extensionHandle;
			int hr = ExtensionDebugger.DebugControl.AddExtensionWide(extensionFilePath, 0, out extensionHandle);
			if (hr == (int)HRESULT.S_OK)
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

		public ulong ExtensionHandle { get; private set; }

		public Debugger ExtensionDebugger { get; private set; }

		#endregion

		#region Public Methods

		public string CallExtensionMethod(string method, string args)
		{
			IntPtr ptrOriginalOutputHandler;
			ExtensionDebugger.InstallCustomHandler(mOutputHandler, out ptrOriginalOutputHandler);

			// The bang isn't needed to call extension methods but some people may
			// add the input out of habit, so we will trim this off if it happens.
			if (method.StartsWith("!"))
			{
				method = method.TrimStart('!');
			}

			int hr = ExtensionDebugger.DebugControl.CallExtensionWide(ExtensionHandle, method, args);
			if (hr != (int)HRESULT.S_OK)
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
			public ExtensionLoadedEventArgs(string extensionFilePath)
			{
				ExtensionFilePath = extensionFilePath;
			}

			public string ExtensionFilePath { get; }
		}

		#endregion

	}
}
