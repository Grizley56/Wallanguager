using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wallanguager.WallpaperEngine
{
	public class Signature
	{
		public string Original { get; private set; }
		public string Translated { get; private set; }

		public SignFormat Format { get; set; }

		public Signature(string original, string translated, SignFormat format = null)
		{
			Format = format ?? SignFormat.Default;

			Original = original;
			Translated = translated;
		}

		public override string ToString()
		{
			return Format.GetFormattedString(Original, Translated);
		}
	}
}
