using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using GoogleTranslateFreeApi;


namespace Wallanguager.Learning
{
	[DataContract(IsReference = true)]
	public class PhrasesGroup : IEnumerable<Phrase>, INotifyPropertyChanged, ITranslatable
	{
		[DataMember]
		private ObservableCollection<Phrase> _phrases;
		[DataMember]
		private Language _toLanguage;
		[DataMember]
		private Language _fromLanguage;
		[DataMember]
		private string _groupTheme;
		[DataMember]
		private string _groupName;
		[DataMember]
		private bool _isEnabled = true;

		public bool TranslationUpdateRequired { get; private set; } = true;

		public ICollection<Phrase> Phrases => new ReadOnlyObservableCollection<Phrase>(_phrases);

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				_isEnabled = value;
				OnPropertyChanged();
			}
		}
		public Language ToLanguage
		{
			get { return _toLanguage; }
			set
			{
				_toLanguage = value;
				OnPropertyChanged();
				TranslationUpdateRequired = true;
			}
		}
		public Language FromLanguage
		{
			get { return _fromLanguage; }
			set
			{
				_fromLanguage = value; 
				OnPropertyChanged();
				TranslationUpdateRequired = true;
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

		public async Task TranslateRequireded(ITranslator translator)
		{
			List<Phrase> untranslatedPhrases = new List<Phrase>();
			string textToTranslate = String.Empty;

			if (TranslationUpdateRequired)
			{
				await TranslateAll(translator);
			}
			else
			{
				foreach (var phrase in this)
				{
					if (phrase.TranslatedText != null)
						continue;

					untranslatedPhrases.Add(phrase);
					textToTranslate += phrase.OriginalText + '\n';
				}

				TranslationResult result = await translator.TranslateLiteAsync(textToTranslate.Trim(), FromLanguage, ToLanguage);

				int i = 0;
				foreach (var untranslatedPhrase in untranslatedPhrases)
					untranslatedPhrase.TranslatedText = result.FragmentedTranslation[i++];
			}



			TranslationUpdateRequired = false;
		}

		public async Task TranslateAll(ITranslator translator)
		{
			var result = await translator.TranslateLiteAsync(this);
			var i = 0;

			foreach (var phrase in this)
				phrase.TranslatedText = result.FragmentedTranslation[i++];
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
