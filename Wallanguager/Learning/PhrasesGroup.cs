using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using GoogleTranslateFreeApi;


namespace Wallanguager.Learning
{
	public class PhrasesGroup : IEnumerable<Phrase>, INotifyPropertyChanged, ITranslatable
	{
		private ObservableCollection<Phrase> _phrases;

		private Language _toLanguage;
		private Language _fromLanguage;
		private string _groupTheme;
		private string _groupName;

		public ICollection<Phrase> Phrases => new ReadOnlyObservableCollection<Phrase>(_phrases);

		public Language ToLanguage
		{
			get { return _toLanguage; }
			set
			{
				_toLanguage = value;
				OnPropertyChanged();
			}
		}

		public string OriginalText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach (Phrase phrase in _phrases)
					sb.AppendLine(phrase.OriginalText);
				return sb.ToString();
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

		public Phrase this[int index] => _phrases[index];

		public PhrasesGroup(string groupName, string groupTheme, Language toLanguage, 
			Language fromLanguage = null)
		{
			GroupName = groupName;
			GroupTheme = groupTheme;

			ToLanguage = toLanguage;
			FromLanguage = fromLanguage ?? Language.Auto;
			_phrases = new ObservableCollection<Phrase>();
		}


		public void AddPhrase(string phrase)
		{
			_phrases.Add(new Phrase(phrase, this));
		}

		public bool RemovePhrase(Phrase phrase)
		{
			return _phrases.Remove(phrase);
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
			       $"Phrases count: {_phrases.Count}";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
