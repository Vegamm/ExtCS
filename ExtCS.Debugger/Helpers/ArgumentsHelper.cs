using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ExtCS
{
	public class ArgumentsHelper
	{

		#region Fields

		Dictionary<string, string> mArgsList;
		Regex mRegex = new Regex(
			@"(\s|^)(?<argname>\-\w+\s?)(?<argvalue>\s[^-]\S+)?",
			RegexOptions.IgnoreCase
			| RegexOptions.Multiline
			| RegexOptions.Singleline
			| RegexOptions.RightToLeft
			| RegexOptions.IgnorePatternWhitespace
			| RegexOptions.Compiled
		);

		#endregion

		#region Properties

		public string this[string argName]
		{
			get
			{
				if (mArgsList == null)
				{
					return string.Empty;
				}
				else
				{
					return mArgsList[argName.ToUpperInvariant()];
				}
			}

			private set { mArgsList[argName.ToUpperInvariant()] = value; }
		}

		public IDictionary<string, string> ArgsCollection
		{
			get { return mArgsList; }
		}

		public string Args { get; internal set; }
		#endregion

		#region Constructor

		public ArgumentsHelper(string args)
		{
			Args = args.Trim('"');
			IntArgs();
		}

		#endregion

		#region Public Methods

		public bool HasArgument(string argName)
		{
			return mArgsList.ContainsKey(argName.ToUpperInvariant());
		}

		public bool IsNullOrEmpty(string argName)
		{
			if (HasArgument(argName))
			{
				return string.IsNullOrEmpty(mArgsList[argName]);
			}

			return false;
		}

		#endregion

		#region Private Methods

		//
		private void IntArgs()
		{
			if (string.IsNullOrEmpty(Args))
			{
				return;
			}

			mArgsList = new Dictionary<string, string>();
			foreach (Match item in mRegex.Matches(Args))
			{
				string key = item.Groups["argname"].Value.Trim().ToUpperInvariant();
				string value = item.Groups["argvalue"].Value.Trim();
				mArgsList.Add(key, value);
			}

			AddFileExtensionIfNecessary();
		}

		/// <summary>
		/// Adds ".csx" to the file path if it was not was included with
		/// the argument for '-f' or '-file'.
		/// </summary>
		private void AddFileExtensionIfNecessary()
		{
			string fileArg;
			if (this.HasArgument("-file"))
			{
				fileArg = "-file";
			}
			else if (this.HasArgument("-f"))
			{
				fileArg = "-f";
			}
			else
			{
				return;
			}

			string filePath = this[fileArg];
			if (Path.HasExtension(filePath) == false)
			{
				filePath = Path.ChangeExtension(filePath, "csx");
				this[fileArg] = filePath;
			}
		}

		#endregion

	}
}
