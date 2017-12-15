using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;

namespace Wallanguager.Learning
{
	public class Phrase : ITranslatable
	{
		private readonly PhrasesGroup _ownedGroup;

		public string OriginalText { get; }

		public string TranslatedText { get; set; }

		public Language FromLanguage => _ownedGroup.FromLanguage;

		public Language ToLanguage => _ownedGroup.ToLanguage;

		public Phrase(string text, PhrasesGroup ownedGroup)
		{
			if (ownedGroup == null)
				throw new ArgumentNullException(nameof(ownedGroup));

			_ownedGroup = ownedGroup;
			OriginalText = text;
			TranslatedText = null;
		}

		public bool Equals(Phrase other)
		{
			return string.Equals(OriginalText, other.OriginalText);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Phrase && Equals((Phrase) obj);
		}

		public override int GetHashCode()
		{
			return (OriginalText != null ? OriginalText.GetHashCode() : 0);
		}

		public override string ToString() { return $"{OriginalText}"; }

		public static bool operator==(Phrase phrase1, Phrase phrase2)
		{
			return String.Equals(phrase1.OriginalText, phrase2.OriginalText, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool operator !=(Phrase phrase1, Phrase phrase2)
		{
			return !(phrase1 == phrase2);
		}
	}
}
