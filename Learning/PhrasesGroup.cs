using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Wallanguager.Annotations;

namespace Wallanguager.Learning
{
	public class PhrasesGroup : IEnumerable<Phrase>, INotifyPropertyChanged
	{
		private List<Phrase> _phrases;
		private Language _toLanguage;
		private Language _fromLanguage;
		private string _groupTheme;
		private string _groupName;

		public Language ToLanguage
		{
			get { return _toLanguage; }
			set
			{
				_toLanguage = value;
				OnPropertyChanged();
			}
		}

		public Language FromLanguage
		{
			get { return _fromLanguage; }
			set
			{
				_fromLanguage = value; 
				OnPropertyChanged();
			}
		}

		public string GroupTheme
		{
			get { return _groupTheme; }
			set
			{
				_groupTheme = value; 
				OnPropertyChanged();
			}
		}

		public string GroupName
		{
			get { return _groupName; }
			set
			{
				_groupName = value; 
				OnPropertyChanged();
			}
		}

		public Phrase this[int index]
		{
			get { return _phrases[index]; }
			set { _phrases[index] = value; }
		}

		public int PhrasesCount => _phrases.Count;

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


		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
