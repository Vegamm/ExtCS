using System;
using System.Globalization;

namespace ExtCS.Debugger
{
    public class Address
    {

		#region Fields

		private UInt64 mAddress;
		private string mHexAddress;

		#endregion

		#region Properties

		public bool HasValue
		{
			get
			{
				int chkVal;
				//changing the address to a integer value
				//if this anything other than zero,this is vakid address
				//possible values come like 00000000000,000000
				if (Int32.TryParse(mHexAddress, out chkVal))
				{
					if (chkVal == 0)
					{
						return false;
					}

					return true;
				}
				else
					return true;
			}
		}

		#endregion

		#region Constructors

		public Address(string address)
		{
			address = FormatAddress(address);
			mHexAddress = address;
			if (!UInt64.TryParse(address, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out mAddress))
				throw new Exception("invalid address: " + address);
		}

		public Address(string address, string offset)
		{
			address = FormatAddress(address);
			UInt64 off;
			if (!UInt64.TryParse(offset, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out off))
				throw new Exception("invalid address: " + offset);

			if (!UInt64.TryParse(address, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out mAddress))
				throw new Exception("invalid address: " + address);
			mAddress = mAddress + off;
			mHexAddress = mAddress.ToString("X");
		}

		public Address(UInt64 address)
		{
			mAddress = address;
			mHexAddress = address.ToString("X");
		}
		public Address(UInt64 address, UInt64 offset)
		{
			mAddress = address + offset;
			mHexAddress = address.ToString("X");
		}


		#endregion

		#region Public Methods

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

		public string ToHex()
		{
			return mHexAddress;
		}

		public uint GetInt32Value()
		{
			uint output;
			if (!SUCCEEDED(Debugger.GetCurrentDebugger().ReadVirtual32(mAddress, out output)))
				throw new Exception("unable to get the int32 from address " + mAddress);

			return output;

		}

		public Int16 GetInt16Value()
		{
			Int16 output;
			if (!SUCCEEDED(Debugger.GetCurrentDebugger().ReadVirtual16(mAddress, out output)))
				throw new Exception("unable to get the int16 from address " + mAddress);

			return output;

		}

		public Byte GetByte()
		{
			Byte output;
			if (!SUCCEEDED(Debugger.GetCurrentDebugger().ReadVirtual8(mAddress, out output)))
				throw new Exception("unable to get the byte from address " + mAddress);

			return output;
		}

		public string GetManagedString()
		{
			string strOut;
			ulong offset = Debugger.GetCurrentDebugger().IsPointer64Bit() ? 12UL : 8UL;
			if (SUCCEEDED(GetString(mAddress + offset, 2000, out strOut)))
			{
				return strOut;
			}
			throw new Exception("unable to get the string from address " + mAddress);
		}

		public string GetString()
		{
			string strOut;
			if (SUCCEEDED(GetString(mAddress, 2000, out strOut)))
			{
				return strOut;
			}
			throw new Exception("unable to get the string from address " + mAddress);
		}

		public string GetString(uint maxlength)
		{
			string strOut;
			if (SUCCEEDED(GetString(mAddress, maxlength, out strOut)))
			{
				return strOut;
			}
			throw new Exception("unable to get the string from address " + mAddress);
		}

		public override string ToString()
		{
			return ToHex();
		}

		#endregion

		#region Private Methods

		private string FormatAddress(string address)
		{
			if (address[0] == '0' && address[1] == 'x')
			{
				//removing 0x from address
				return address.Substring(2);
			}
			return address;
		}


		private int GetString(UInt64 address, UInt32 maxSize, out string output)
		{
			return Debugger.GetCurrentDebugger().GetUnicodeString(address, maxSize, out output);
		}

		#endregion

	}
}
