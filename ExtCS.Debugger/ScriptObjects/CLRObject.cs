using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtCS.Debugger
{

	public class CLRObject
	{

		#region Fields

		private static Extension mSOSExtension;

		private Address mAddress;
		private Dictionary<string, CLRObject> mFields;

		//private static Regex expression = new Regex(".*", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

		private string mOffset;
		private string mDumpObjOutput;
		private string[] mProperties;

		private bool mIsPointer;
		private bool mInitialized;
		private object mValue;

		//why is this public? -mv
		public bool mHasValue;

		#endregion

		#region Properties

		public Address Address
		{
			get
			{
				InitIfPointer();
				return mAddress;
			}
		}

		public CLRObject[] Children
		{
			get
			{
				if (mFields != null)
				{
					return mFields.Values.ToArray<CLRObject>();
				}
				else
					return null;
			}
		}

		public string EEclass { get; private set; }

		public string Field { get; private set; }

		public bool IsValueType { get; private set; }

		public string MT { get; private set; }

		public string Name { get; private set; }

		public string Offset { get; private set; }

		public CLRObject Parent { private set; get; }

		public string Type { get; private set; }

		public bool HasValue
		{
			get
			{
				if (IsValueType)
				{
					return true;
				}

				InitIfPointer();
				return mAddress.HasValue;

			}
		}

		public object Value
		{
			get
			{
				try
				{
					if (IsValueType)
					{
						Debugger.GetCurrentDebugger().OutputDebugInfo("Reading value of CLRObject of ValueType with Name:{2} and Type:{0} and Value {1}\n", Type, mValue, Name);
						switch (Type)
						{
							case "System.Boolean":
								return (bool)mValue;
							case "System.Int32":
								if (mValue == null)
								{
									mValue = mAddress.GetInt32Value();
								}
								return mValue;
							default:
								return mValue;
						}
					}

					InitIfPointer();
					Debugger.GetCurrentDebugger().OutputDebugInfo("Reading field {2} value of CLRObject with and Type:{0} and Value {1}\n", Type, mValue, Name);
					switch (Type)
					{
						case "System.String":
							mValue = mAddress.GetManagedString();
							Debugger.GetCurrentDebugger().OutputDebugInfo("Reading string value {0}\n", mValue);
							break;
						default:
							Debugger.GetCurrentDebugger().OutputDebugInfo("Reading string value {0}\n", mValue);
							mValue = this;
							break;
					}

					return mValue;
				}

				catch (Exception ex)
				{
					Debugger.GetCurrentDebugger().OutputDebugInfo("Could not read field '{0}' of CLRObject with type:{1} and MT {2}", Name, Type, MT);
					Debugger.GetCurrentDebugger().OutputDebugInfo(ex.Message);
					EmitParentInfo();
					throw ex;
				}
			}
		}

		#endregion

		#region Constructors

		public CLRObject(bool isValueType, string typeName, object value)
		{
			this.IsValueType = isValueType;
			Name = typeName;
			mValue = value;
			mInitialized = false;
			// Debugger.GetCurrentDebugger().OutputDebugInfo("created CLRObject {2} of valuetype with name:{0} and value:{1}\n", typeName, value,Name); 
		}

		/// <summary>
		/// This constuctor is used a s lazy initialization technique.
		/// we will not be reading the original address unitl any request comes for any field access.
		/// 
		/// </summary>
		/// <param name="isPointer"></param>
		/// <param name="offset"></param>
		public CLRObject(bool isPointer, string offset)
		{
			mIsPointer = isPointer;
			mOffset = offset;
			mInitialized = false;
			// Debugger.GetCurrentDebugger().OutputDebugInfo("created CLRObject's {1} with offset:{0}\n",mOffset,Name ); 
		}

		public CLRObject(Address address)
		{
			mAddress = address;
			mInitialized = false;
			Debugger.GetCurrentDebugger().OutputDebugInfo("created CLRObject from address object ,address:{0}\n", address.ToHex());
		}

		public CLRObject(string address)
		{
			mAddress = new Address(address);
			mInitialized = false;
			Debugger.GetCurrentDebugger().OutputDebugInfo("created CLRObject with address:{0}\n", address);
		}

		public CLRObject(UInt64 address)
		{
			mAddress = new Address(address);
			mInitialized = false;
		}

		#endregion

		#region Private Methods

		private void Init()
		{
			if (!mInitialized)
			{
				if (mSOSExtension == null)
					mSOSExtension = new Extension("sos.dll");

				InitIfPointer();
				mDumpObjOutput = mSOSExtension.Call("!do", mAddress.ToHex());
				mProperties = mDumpObjOutput.Split('\n', '\r');
				mInitialized = true;
				Name = string.Empty;
			}
		}

		private void InitIfPointer()
		{
			if (mIsPointer)
			{
				Debugger.GetCurrentDebugger().OutputDebugInfo("Reading pointer of CLRObject's {2} with name: {1} and offset:{0}\n", mOffset, Type, Name);
				mAddress = new Address(Debugger.GetCurrentDebugger().POI(mOffset));
				mIsPointer = false;
			}
		}

		private void InitFields()
		{

			if (mFields != null)
			{
				return;
			}

			mFields = new Dictionary<string, CLRObject>(mProperties.Length);

			//chose to do string split instead of regular expressions.
			//this may be faster than regular expressions
			//regular expression has lot of edge cases when i comes to matching geneic's notation
			//containing special charcters


			//name can be populated from the feild property.so this is not necessary
			if (string.IsNullOrEmpty(Name) && mProperties[0].Contains("Name:"))
				Name = mProperties[0].Substring(13);
			//getting the method table from the substring.
			if (string.IsNullOrEmpty(this.MT) && mProperties[1].Contains("MethodTable:"))
				MT = mProperties[1].Substring(13);
			if (mProperties[2].Contains("EEClass:"))
				EEclass = mProperties[2].Substring(13);

			Debugger.GetCurrentDebugger().OutputDebugInfo("Initializing fields of CLR Object Name:{0} and Type {1}\n", Name, Type);
			EmitParentInfo();

			Debugger.GetCurrentDebugger().OutputDebugInfo("MT \t\t Type \t\t valueType \t\t Name\n");
			for (int i = 7; i < mProperties.Length; i++)
			{
				var strCurrentLine = mProperties[i];
				//only if the filed contains instance,try to parse it.
				//we are skipping shared and static variable instances.
				if (strCurrentLine.Contains("instance"))
				{
					string[] arrFields = strCurrentLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					//saw that some times the type field is empty string
					//we are skipping that field as well b taking more thn or equal to 8 strings.
					//0-MT
					//1-Field
					//2-offset
					//3-Type
					//4-VT
					//5-Attr
					//6-Value
					//7 =>Name
					if (arrFields.Length >= 8)
					{

						CLRObject ObjCLR;
						//indexing from back to avoid problems when the type contains spaces.
						//if type contains spaces,it will cause length to be changed
						if (arrFields[arrFields.Length - 4].Trim() == "1")
						{

							//instantiaing value type.
							//no need to check for address and values.
							ObjCLR = new CLRObject(true, arrFields[3].Trim(), arrFields[arrFields.Length - 2].Trim());
							ObjCLR.Name = arrFields[arrFields.Length - 1];
							ObjCLR.Parent = this;
							ObjCLR.MT = arrFields[0].Trim();
							ObjCLR.Type = arrFields[3].Trim();

						}
						else
						{
							string fieldAddress = new Address(mAddress.ToHex(), arrFields[2]).ToHex();
							//var pointer = Debugger.GetCurrentDebugger().ReadPointer(fieldAddress);
							ObjCLR = new CLRObject(true, fieldAddress);
							ObjCLR.Name = arrFields[arrFields.Length - 1];
							ObjCLR.Parent = this;
							//ObjCLR.MT = arrFields[0];
							ObjCLR.MT = arrFields[0].Trim();
							ObjCLR.Type = arrFields[3].Trim();

						}
						Debugger.GetCurrentDebugger().OutputDebugInfo("{0} \t\t {1} \t\t {2} \t\t {3} \n", ObjCLR.MT, ObjCLR.Type, ObjCLR.IsValueType, ObjCLR.Name);
						mFields.Add(arrFields[arrFields.Length - 1].Trim().ToUpperInvariant(), ObjCLR);
					}
				}

			}
		}

		private void EmitParentInfo()
		{

			if (this.Parent != null)
			{
				Debugger.GetCurrentDebugger().OutputDebugInfo("Parent CLRObject's  Name:{0} Type:{1} MT:{2}\n", this.Parent.Name, this.Parent.Type, this.Parent.MT);
				if (this.Parent.Parent != null)
				{
					Debugger.GetCurrentDebugger().OutputDebugInfo("GrandParent CLRObject's  Name:{0} Type:{1} MT:{2}\n", this.Parent.Parent.Name, this.Parent.Parent.Type, this.Parent.Parent.MT);
				}
			}
		}

		#endregion

		#region Public Methods

		public bool HasField(string field)
		{
			Init();
			InitFields();
			if (mFields.ContainsKey(field.ToUpperInvariant()))
			{
				return true;
			}

			return false;
		}

		// what is this? Looks like a property. You should probably move this to a different region -mv
		public CLRObject this[string fieldName]
		{
			get
			{
				try
				{
					if (IsValueType)
					{
						throw new Exception(string.Format("value type {0} does not support field access", Name));
					}
					Debugger.GetCurrentDebugger().OutputDebugInfo("Beginning to read field of CLRObject's {0} fieldname {1}\n", Name, fieldName);
					fieldName = fieldName.ToUpperInvariant();
					if (mFields != null && mFields.ContainsKey(fieldName))
					{
						return mFields[fieldName];
					}
					else
					{

						Init();
						InitFields();
						return mFields[fieldName];
					}
				}
				catch (Exception ex)
				{

					Debugger.GetCurrentDebugger().OutputDebugInfo("Error:Could not find a field '{2}' for CLRObject with Name {0} and Type{1}\n", Name, Type, fieldName);
					Debugger.GetCurrentDebugger().OutputDebugInfo(ex.Message);
					throw ex;

				}
			}
		}

		#endregion




	}
}
