using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallanguager.Learning
{
	public struct Phrase
	{
		public readonly string Text;

		public Phrase(string text)
		{
			Text = text;
		}

		public bool Equals(Phrase other)
		{
			return string.Equals(Text, other.Text);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Phrase && Equals((Phrase) obj);
		}

		public override int GetHashCode()
		{
			return (Text != null ? Text.GetHashCode() : 0);
		}

		public override string ToString() { return $"{Text}"; }

		public static bool operator==(Phrase phrase1, Phrase phrase2)
		{
			return String.Equals(phrase1.Text, phrase2.Text, StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool operator !=(Phrase phrase1, Phrase phrase2)
		{
			return !(phrase1 == phrase2);
		}
	}
}
