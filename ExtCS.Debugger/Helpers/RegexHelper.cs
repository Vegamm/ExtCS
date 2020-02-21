using ExtCS.Debugger;
using System.Text;

namespace ExtCS.Helpers
{
	public class RegexHelper
	{

		#region Fields

		private StringBuilder mPattern;

		#endregion

		#region Constructor

		public RegexHelper()
		{
			mPattern = new StringBuilder();
		}

		#endregion

		#region Public Methods

		public RegexHelper String(string pattern)
		{
			mPattern.Append(pattern);
			return this;
		}

		public RegexHelper Spaces(int count)
		{
			mPattern.Append(Utilities.GetPaddedString(' ', count));
			return this;
		}

		public RegexHelper Numbers(int count)
		{
			return this;
		}

		public RegexHelper AnyChar(int count)
		{
			return this;
		}

		#endregion

	}
}
