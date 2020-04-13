using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using DotNetDbg;

namespace ExtCS.Debugger
{
	public unsafe partial class Debugger
	{
		#region Fields

		const int E_FAIL = unchecked((int)0x80004005);
		const int ERROR_INVALID_PARAMETER = unchecked((int)0x80070057);

		private static Debugger sCurrent;
		private static OutputHandler sOutHandler;
		private static Dictionary<string, Extension> sLoadedExtensions = new Dictionary<string, Extension>(10);

		StringBuilder mDebugOutput = new StringBuilder(10);

		private bool mFirstCommand = false;
		private DEBUG_OUTCTL mOutCtl = DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.DML;

		#endregion Fields

		#region Constructors

		// TODO: Add logic to get the IDebugControl4 in this constructor.
		public Debugger(IDebugClient debugClient)
		{
			DebugClient = debugClient as IDebugClient5;
			sCurrent = this;
		}

		public Debugger(IDebugClient debugClient, IDebugControl4 debugControl)
		{
			DebugClient = debugClient as IDebugClient5;
			DebugControl = debugControl;
			sCurrent = this;
		}

		public Debugger(IDebugClient debugClient, IDebugControl4 debugControl, IDebugDataSpaces4 debugDataSpaces)
		{
			DebugClient = debugClient as IDebugClient5;
			DebugControl = debugControl;
			DebugDataSpaces = debugDataSpaces;
			sCurrent = this;

			Extension.ExtensionLoadedEvent += OnExtensionLoaded;
		}

		public Debugger(IDebugClient debugClient, ScriptContext context)
		{
			DebugClient = debugClient as IDebugClient5;
			sCurrent = this;
			this.Context = context;
		}

		#endregion

		#region Static Methods

		public static Debugger GetCurrentDebugger()
		{
			return sCurrent;
		}

		#endregion

		#region Properties

		/// <summary>IDebugClient5</summary>
		public IDebugClient5 DebugClient { get; internal set; }

		/// <summary>IDebugControl4</summary>
		public IDebugControl4 DebugControl { get; internal set; }

		/// <summary>IDebugControl5</summary>
		public IDebugControl6 DebugControl6 { get; internal set; }

		/// <summary>IDebugDataSpaces4</summary>
		public IDebugDataSpaces4 DebugDataSpaces { get; internal set; }

		/// <summary>IDebugRegisters2</summary>
		public IDebugRegisters2 DebugRegisters { get; internal set; }

		/// <summary>IDebugSymbols3</summary>
		public IDebugSymbols3 DebugSymbols { get; internal set; }

		/// <summary>IDebugSymbols5</summary>
		public IDebugSymbols5 DebugSymbols5 { get; internal set; }

		/// <summary>IDebugSystemObjects2</summary>
		public IDebugSystemObjects2 DebugSystemObjects { get; internal set; }

		/// <summary>IDebugAdvanced3</summary>
		public IDebugAdvanced3 DebugAdvanced { get; internal set; }

		public ScriptContext Context { get; internal set; }

		public bool IsFirstCommand
		{
			get { return mFirstCommand; }
			set
			{
				mFirstCommand = value;
				mOutCtl = (mFirstCommand == false) ?
						DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.NOT_LOGGED :
						DEBUG_OUTCTL.THIS_CLIENT;
			}
		}

		#endregion

		#region Public Methods

		public string Execute(string command)
		{
			//create a new debug control and assign the new output handler
			IntPtr outputCallbacks;
			IntPtr previousCallbacks;

			string output = null;

			// This doesn't do anything
			//GCHandle dbgClient = GCHandle.Alloc(DebugControl);
			//IntPtr ptrClient = (IntPtr)dbgClient;
			//IDebugControl4 debugControl = (IDebugControl4) Marshal.GetObjectForIUnknown(ptrClient);

			// Save previous callbacks
			OutputDebugInfo($"executing command {command}\n");
			previousCallbacks = SavePreviousCallbacks();

			// Sets up output callback
			int hrInstallation = InitializeOutputHandler();
			if (FAILED(hrInstallation))
			{
				this.OutputVerboseLine("Failed installing a new output callbacks client for execution");
				outputCallbacks = IntPtr.Zero;
				return null;
			}

			// Executes the command and sends output to this client's output callback only.
			int hrExecution = this.DebugControl.Execute(DEBUG_OUTCTL.THIS_CLIENT, command, DEBUG_EXECUTE.DEFAULT | DEBUG_EXECUTE.NO_REPEAT);
			if (FAILED(hrExecution))
			{
				this.OutputVerboseLine("Failed creating a new debug client for execution.");
				outputCallbacks = IntPtr.Zero;
				return null;
			}

			// Revert previous callbacks
			hrInstallation = RevertCallBacks(previousCallbacks);
			if (FAILED(hrInstallation))
			{
				this.OutputVerboseLine("Failed reverting callbacks for client.");
			}

			//getting the output from the buffer.
			output = sOutHandler.ToString();
			OutputDebugInfo($"command output:\n{output}");
			sOutHandler.mStbOutput.Length = 0;

			//releasing the COM object.
			//Marshal.ReleaseComObject(sOutHandler);
			return output;
		}

		public void OutputDebugInfo(string format, params object[] args)
		{
			if (this.Context.Debug)
			{
				Output(string.Format("\ndebuginfo:" + format, args));
			}
		}

		public void Output(object output)
		{
			if (output != null)
			{
				OutputHelper(output.ToString(), DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.VERBOSE);
			}
		}

		public void Output(string output)
		{
			if (string.IsNullOrEmpty(output) == false)
			{
				OutputHelper(output, DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.VERBOSE);
			}
		}

		public string GetString(UInt64 address)
		{
			string strOut;
			if (SUCCEEDED(GetString(address, 2000, out strOut)))
			{
				return strOut;
			}
			throw new Exception("unable to get the string from address " + address);
		}

		public string GetString(object objAddress)
		{
			UInt64 address;
			if (UInt64.TryParse(objAddress.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address))
			{
				return GetString(address);
			}

			throw new Exception("unable to get the string from address " + address);
		}

		/// <summary>
		/// Reads a null-terminated ANSI or Multi-byte string from the target.
		/// </summary>
		/// <param name="address">Address of the string</param>
		/// <param name="maxSize">Maximum number of bytes to read</param>
		/// <param name="output">The string</param>
		/// <returns>Last HRESULT received while retrieving the string</returns>
		public int GetString(UInt64 address, UInt32 maxSize, out string output)
		{
			return GetUnicodeString(address, maxSize, out output);
		}

		public UInt64 POI(Address address)
		{
			UInt64 addr;
			if (UInt64.TryParse(address.ToHex(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr))
			{
				return ReadPointer(addr);
			}

			throw new Exception($"Unable to convert address: {address}");
		}

		public UInt64 POI(string address)
		{
			UInt64 addr;
			if (UInt64.TryParse(address, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr))
			{
				return ReadPointer(addr);
			}

			throw new Exception($"Unable to convert address: {address}");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>UInt64 is interchangeable with ulong</remarks>
		/// <param name="address"></param>
		/// <returns></returns>
		public UInt64 POI(UInt64 address)
		{
			return ReadPointer(address);
		}

		public UInt64 POI(string address, string offset)
		{
			UInt64 addr;
			if (UInt64.TryParse(address, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr) == false)
			{
				throw new Exception($"Unable to convert address: {address}");
			}

			UInt64 offs;
			if (UInt64.TryParse(offset, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offs) == false)
			{
				throw new Exception($"Unable to convert offset: {offset}");
			}

			return ReadPointer(addr + offs);
		}

		public UInt64 POI(string address, UInt64 offset)
		{
			UInt64 addr;
			if (UInt64.TryParse(address, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out addr) == false)
			{
				throw new Exception($"Unable to convert address: {address}");
			}

			return ReadPointer(addr + offset);
		}

		public UInt64 POI(UInt64 address, UInt64 offset)
		{
			return ReadPointer(address + offset);
		}

		public UInt64 POI(UInt64 address, string offset)
		{
			UInt64 offs;
			if (UInt64.TryParse(offset, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offs) == false)
			{
				throw new Exception($"Unable to convert offset: {offset}");
			}

			return ReadPointer(address + offs);
		}

		// Roslyn doesn't like the 'params' keyword and fails to compile scripts that use a method with it.
		// Since that's the case, we've just created this overloaded method even though it's ugly...
		// TODO: investigate how we can get methods with params to work in scripts.
		public UInt64 POI(string address, string offset1, string offset2, string offset3 = "")
		{
			UInt64 addr = 0;
			string[] offsets;

			if (String.IsNullOrEmpty(offset3))
			{
				offsets = new[] { offset1, offset2 };
			}
			else
			{
				offsets = new[] { offset1, offset2, offset3 };
			}

			foreach (string offset in offsets)
			{
				addr = POI(address, offset);
				address = addr.ToString("X");
			}

			return addr;
		}

		public UInt64 POI(string address, string[] offsets)
		{
			UInt64 addr = 0;
			foreach (string offset in offsets)
			{
				addr = POI(address, offset);
				address = addr.ToString("X");
			}

			return addr;
		}

		/// <summary>
		/// Used to determine whether the debug target has a 64-bit pointer size
		/// </summary>
		/// <returns>True if 64-bit, otherwise false</returns>
		public bool IsPointer64Bit()
		{
			return (DebugControl.IsPointer64Bit() == (int)HRESULT.S_OK);
		}

		/// <summary>
		/// Reads a 32-bit value from the target's virtual address space.
		/// </summary>
		/// <param name="address">Address to read from</param>
		/// <param name="value">UInt32 to receive the value</param>
		/// <returns>HRESULT</returns>
		public unsafe int ReadVirtual32(UInt64 address, out UInt32 value)
		{
			UInt32 tempValue;
			int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), (uint)sizeof(UInt32), null);
			value = SUCCEEDED(hr) ? tempValue : 0;
			return hr;
		}

		/// <summary>
		/// Reads a 8-bit value from the target's virtual address space.
		/// </summary>
		/// <param name="address">Address to read from</param>
		/// <param name="value">Byte to receive the value</param>
		/// <returns>HRESULT</returns>
		public unsafe int ReadVirtual8(UInt64 address, out Byte value)
		{
			Byte tempValue;
			int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), (uint)sizeof(Byte), null);
			value = SUCCEEDED(hr) ? tempValue : ((Byte)0);
			return hr;
		}

		/// <summary>
		/// Reads a 16-bit value from the target's virtual address space.
		/// </summary>
		/// <param name="address">Address to read from</param>
		/// <param name="value">Int16 to receive the value</param>
		/// <returns>HRESULT</returns>
		public unsafe int ReadVirtual16(UInt64 address, out Int16 value)
		{
			Int16 tempValue;
			int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), (uint)sizeof(Int16), null);
			value = SUCCEEDED(hr) ? tempValue : ((Int16)0);
			return hr;
		}

		/// <summary>
		/// Reads a 64-bit value from the target's virtual address space.
		/// </summary>
		/// <param name="address">Address to read from</param>
		/// <param name="value">Int64 to receive the value</param>
		/// <returns>HRESULT</returns>
		public unsafe int ReadVirtual64(UInt64 address, out Int64 value)
		{
			Int64 tempValue;
			int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), (uint)sizeof(Int64), null);
			value = SUCCEEDED(hr) ? tempValue : 0;
			return hr;
		}

		/// <summary>
		/// Reads a null-terminated Unicode string from the target.
		/// </summary>
		/// <param name="address">Address of the Unicode string</param>
		/// <param name="maxSize">Maximum number of bytes to read</param>
		/// <param name="output">The string</param>
		/// <returns>Last HRESULT received while retrieving the string</returns>
		public int GetUnicodeString(UInt64 address, UInt32 maxSize, out string output)
		{
			StringBuilder sb = new StringBuilder((int)maxSize + 1);
			uint bytesRead;
			int hr = DebugDataSpaces.ReadUnicodeStringVirtualWide(address, (maxSize * 2), sb, maxSize, &bytesRead);
			if (SUCCEEDED(hr))
			{
				if ((bytesRead / 2) > maxSize)
				{
					sb.Length = (int)maxSize;
				}
				output = sb.ToString();
			}
			else if (ERROR_INVALID_PARAMETER == hr)
			{
				sb.Length = (int)maxSize;
				output = sb.ToString();
			}
			else
			{
				output = null;
			}
			return hr;
		}

		public void InstallCustomHandler(OutputHandler handler, out IntPtr previousCallbacks)
		{
			previousCallbacks = SavePreviousCallbacks();
			IntPtr ptrDebugOutputCallbacks2 = Marshal.GetComInterfaceForObject(handler, typeof(IDebugOutputCallbacks2));
			DebugClient.SetOutputCallbacks(ptrDebugOutputCallbacks2);
		}

		public void OutputError(string p, params object[] args)
		{
			string formatted = String.Format(p, args);
			OutputHelper(formatted, DEBUG_OUTPUT.ERROR);
		}

		/// <summary>
		/// Tries to load an extension. 
		/// </summary>
		/// <remarks>This is equivalent to instantiating a new Extension object</remarks>
		/// <param name="extensionFilePath"></param>
		/// <returns>True if extension is successfully loaded, false otherwise</returns>
		public bool Require(string extensionFilePath)
		{
			if (extensionFilePath == null)
			{
				throw new ArgumentNullException("Extension file path cannot be null.");
			}

			extensionFilePath = extensionFilePath.ToLowerInvariant();
			string extensionName = Path.GetFileNameWithoutExtension(extensionFilePath);
			if (sLoadedExtensions.ContainsKey(extensionName))
			{
				return true;
			}

			Extension extension = new Extension(extensionFilePath);
			return true;
		}

		/// <summary>
		/// Sets the output callbacks to the previous callbacks.
		/// </summary>
		/// <param name="ptrPreviousCallbacks"></param>
		/// <returns></returns>
		public int RevertCallBacks(IntPtr ptrPreviousCallbacks)
		{
			this.DebugClient.FlushCallbacks();
			var hrInstallation = this.DebugClient.SetOutputCallbacks(ptrPreviousCallbacks);
			return hrInstallation;
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns true if a HRESULT indicates failure.
		/// </summary>
		/// <param name="hr">HRESULT</param>
		/// <returns>True if hr indicates failure</returns>
		public static bool FAILED(int hr)
		{
			return (hr < 0);
		}

		/// <summary>
		/// Returns true if a HRESULT indicates success.
		/// </summary>
		/// <param name="hr">HRESULT</param>
		/// <returns>True if hr indicates success</returns>
		public static bool SUCCEEDED(int hr)
		{
			return (hr >= 0);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Installs our implementation of the DebugOutputCallbacks2 with our DebugClient
		/// </summary>
		/// <returns></returns>
		private int InitializeOutputHandler()
		{
			//creating new output handler to redirect output.
			if (sOutHandler == null)
			{
				sOutHandler = new OutputHandler();
			}

			IntPtr ptrDebugOutputCallbacks2 = Marshal.GetComInterfaceForObject(sOutHandler, typeof(IDebugOutputCallbacks2));
			int hrInstallation = this.DebugClient.SetOutputCallbacks(ptrDebugOutputCallbacks2);
			return hrInstallation;
		}

		// TODO: Switch parameter order so that it matches with the native call.
		private int OutputHelper(string formattedString, DEBUG_OUTPUT outputType)
		{
			return DebugControl.ControlledOutput(DEBUG_OUTCTL.ALL_OTHER_CLIENTS | DEBUG_OUTCTL.DML, outputType, formattedString);
		}

		private void OutputVerboseLine(string p)
		{
			OutputHelper(p, DEBUG_OUTPUT.VERBOSE);
		}

		/// <summary>
		/// Flushes the DebugClient and gets the current OutputCallbacks on the DebugClient.
		/// </summary>
		/// <returns></returns>
		private IntPtr SavePreviousCallbacks()
		{
			// TODO: Rename method to save CURRENT output callbacks.
			IntPtr ptrPreviousCallbacks;
			this.DebugClient.FlushCallbacks();

			//get previous callbacks
			//saving the previous callbacks
			this.DebugClient.GetOutputCallbacks(out ptrPreviousCallbacks); /* We will need to release this */

			return ptrPreviousCallbacks;
		}

		/// <summary>
		/// Reads a single pointer from the target.
		/// NOTE: POINTER VALUE IS SIGN EXTENDED TO 64-BITS WHEN NECESSARY!
		/// </summary>
		/// <param name="offset">The address to read the pointer from</param>
		/// <returns>The pointer</returns>
		private UInt64 ReadPointer(UInt64 offset)
		{
			UInt64[] pointerArray = new UInt64[1];
			int hr = DebugDataSpaces.ReadPointersVirtual(1, offset, pointerArray);
			if (FAILED(hr))
				ThrowExceptionHere(hr);

			return pointerArray[0];
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// If the extension is loaded in the debugger, the Extension is returned.
		/// Otherwise, null is returned.
		/// </summary>
		/// <param name="extensionName">The name of the extension to retrieve</param>
		internal Extension GetExtension(string extensionName)
		{
			extensionName = extensionName.ToLowerInvariant();
			extensionName = Path.GetFileNameWithoutExtension(extensionName);
			if (sLoadedExtensions.ContainsKey(extensionName))
			{
				return sLoadedExtensions[extensionName];
			}
			return null;
		}

		internal void OutputError(string message, string method, string args)
		{
			string formatted = $"Error | [{method} {args}] : {message}";
			OutputHelper(formatted, DEBUG_OUTPUT.ERROR);
		}

		/// <summary>
		/// Throws a new exception with the name of the parent function and value of the HR
		/// </summary>
		/// <param name="hr">Error # to include</param>
		internal void ThrowExceptionHere(int hr)
		{
			StackTrace stackTrace = new StackTrace();
			System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(1);
			throw new Exception(String.Format("Error in {0}: {1}", stackFrame.GetMethod().Name, hr));
		}

		#endregion

		#region Event Handlers

		private void OnExtensionLoaded(Extension extension, Extension.ExtensionLoadedEventArgs args)
		{
			string extensionFilePath = args?.ExtensionFilePath;
			if (extensionFilePath is null)
			{
				return;
			}

			extensionFilePath = extensionFilePath.ToLowerInvariant();
			string extensionName = Path.GetFileNameWithoutExtension(extensionFilePath);
			if (sLoadedExtensions.ContainsKey(extensionName) == false)
			{
				sLoadedExtensions.Add(extensionName, extension);
			}
		}

		#endregion
	}
}
