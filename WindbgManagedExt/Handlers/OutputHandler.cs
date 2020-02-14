using DotNetDbg;
using System;
using System.Text;

namespace ExtCS.Debugger
{
	public class OutputHandler : IDebugOutputCallbacks2, IDisposable
	{
		//this should probably be a centralize property (along the the fail/succeed methods) -mv
		const int S_OK = 0;

		//why public... -mv
		public StringBuilder mStbOutput = new StringBuilder();

		private bool mReEnter = false;

		private readonly DEBUG_OUTCBI INTEREST_MASK = DEBUG_OUTCBI.ANY_FORMAT | DEBUG_OUTCBI.EXPLICIT_FLUSH;

		#region Public Methods

		/// <summary>
		/// Outputs a line of text.
		/// </summary>
		/// <param name="Mask">Flags describing the output.</param>
		/// <param name="Text">The text to output.</param>
		/// <returns>HRESULT which is almost always S_OK since errors are ignored by the debugger engine unless they signal an RPC error.</returns>
		public int Output(DEBUG_OUTPUT Mask, string Text)
		{
			//mStbOutput.Append(Text);
			//return S_OK;
			return Output2(DEBUG_OUTCB.TEXT, 0, (UInt64)Mask, Text);
		}

		/// <summary>
		/// Implements IDebugOutputCallbacks2::GetInterestMask
		/// </summary>
		public int GetInterestMask(out DEBUG_OUTCBI Mask)
		{
			Mask = INTEREST_MASK;
			return S_OK;
		}

		/// <summary>
		/// Implements IDebugOutputCallbacks2::Output2
		/// </summary>
		public int Output2(DEBUG_OUTCB Which, DEBUG_OUTCBF Flags, UInt64 Arg, string Text)
		{
			DEBUG_OUTPUT Mask = (DEBUG_OUTPUT)Arg;

			if (Which == DEBUG_OUTCB.EXPLICIT_FLUSH)
			{
				//Flush();
				return S_OK;
			}
			else if ((Text == null) || (Text.Length == 0))
			{
				return S_OK;
			}
			bool textIsDml = (Which == DEBUG_OUTCB.DML);

			mStbOutput.Append(Text);

			return S_OK;
		}

		int IDebugOutputCallbacks2.Output(DEBUG_OUTPUT Mask, string Text)
		{
			return Output(Mask, Text);
		}

		int IDebugOutputCallbacks2.GetInterestMask(out DEBUG_OUTCBI Mask)
		{
			return GetInterestMask(out Mask);
		}

		int IDebugOutputCallbacks2.Output2(DEBUG_OUTCB Which, DEBUG_OUTCBF Flags, ulong Arg, string Text)
		{
			return Output2(Which, Flags, Arg, Text);
		}

		int IDebugOutputCallbacks.Output(DEBUG_OUTPUT Mask, string Text)
		{
			return Output(Mask, Text);
		}

		public override string ToString()
		{
			return mStbOutput.ToString();
		}

		#endregion

		#region Private Methods

		private static bool FAILED(int hr)
		{
			return (hr < 0);
		}

		private static bool SUCCEEDED(int hr)
		{
			return (hr >= 0);
		}

		#endregion

		#region IDisposable Methods

		public void Dispose()
		{

		}

		#endregion
	}
}
