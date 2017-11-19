using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallanguager.Learning
{
	public class Language
	{
		public static Language Auto = new Language("Automatic", "auto");
		public string FullName { get; }
		public string ISO639 { get; }

		public Language(string fullName, string iso639)
		{
			FullName = fullName;
			ISO639 = iso639;
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
