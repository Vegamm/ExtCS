﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExtCS
{
	public class ArgumentsHelper
	{

		#region Fields

		//old mRegex @"(?<argName>\-\w+)\s(?<argvalue>\S+)?",
		private string mArgs;
		Dictionary<string, string> mArgsList;
		Regex mRegex = new Regex(
			@"(\s|^)(?<argName>\-\w+\s?)(?<argvalue>\s[^-]\S+)?",
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
		}

		public IDictionary<string, string> ArgsCollection
		{
			get { return mArgsList; }
		}

		#endregion

		#region Constructor

		public ArgumentsHelper(string args)
		{
			mArgs = args;
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

		private void IntArgs()
		{
			if (string.IsNullOrEmpty(mArgs))
			{
				return;
			}
			mArgsList = new Dictionary<string, string>();
			foreach (Match item in mRegex.Matches(mArgs))
			{
				mArgsList.Add(item.Groups["argname"].Value.Trim().ToUpperInvariant(), item.Groups["argvalue"].Value);
			}
		}

		#endregion

	}
}
