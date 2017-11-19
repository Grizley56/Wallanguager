using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Wallanguager.Learning
{
	public class PhrasesGroup : IEnumerable<Phrase>
	{
		public Language ToLanguage { get; set; }
		public Language FromLanguage { get; set; }
		public string GroupTheme { get; set; }
		public string GroupName { get; set; }

		public Phrase this[int index]
		{
			get { return _phrases[index]; }
			set { _phrases[index] = value; }
		}

		public int PhrasesCount => _phrases.Count;

		private List<Phrase> _phrases;

		public PhrasesGroup(string groupName, string groupTheme, Language toLanguage, 
			Language fromLanguage = null, IEnumerable<Phrase> phrases = null)
		{
			GroupName = groupName;
			GroupTheme = groupTheme;

			ToLanguage = toLanguage;
			FromLanguage = fromLanguage ?? Language.Auto;
			_phrases = new List<Phrase>();


			if (phrases != null)
				_phrases.AddRange(phrases);
		}

		public IEnumerator<Phrase> GetEnumerator()
		{
			return ((IEnumerable<Phrase>)_phrases).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Phrase>)_phrases).GetEnumerator();
		}

		public override string ToString()
		{
			return $"Group name: {GroupName}\nGroup theme: {GroupTheme}\nFrom " +
			       $"language: {FromLanguage}\nTo language: {ToLanguage}\n" +
			       $"Phrases count: {PhrasesCount}";
		}

	}
}
