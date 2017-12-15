using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wallanguager.WallpaperEngine;

namespace Wallanguager.Windows
{
	/// <summary>
	/// Логика взаимодействия для DisplaySettings.xaml
	/// </summary>
	public partial class DisplaySettings : Window
	{
		private static SoundPlayer _player = new SoundPlayer();
		private static FileInfo[] _soundFiles;
		private const string SOUND_FILE_PATH = "Content/Sounds";
		
		public TimeSpan UpdateFrequency { get; private set; }
		public UpdateOrder WallpaperUpdateOrder { get; private set; }
		public UpdateOrder PhraseUpdateOrder { get; private set; }
		public FileInfo SoundFile { get; private set; }

		static DisplaySettings()
		{
			DirectoryInfo soundsDir = new DirectoryInfo(SOUND_FILE_PATH);
			if (soundsDir.Exists)
				_soundFiles = soundsDir.GetFiles().OrderBy(i => i.Name.Length).ToArray();
		}

		public DisplaySettings() : this(TimeSpan.Zero, null, UpdateOrder.Randomly, UpdateOrder.Randomly) { }

		public DisplaySettings(TimeSpan frequency, FileInfo soundInfo, UpdateOrder wallpaperUpd, UpdateOrder phraseUpd)
		{
			InitializeComponent();

			soundsComboBox.Items.Add("None");

			if (_soundFiles != null)
				foreach (var sound in _soundFiles)
					soundsComboBox.Items.Add(sound);

			if (soundInfo == null)
				soundsComboBox.SelectedItem = "None";
			else
				soundsComboBox.SelectedItem = soundInfo;

			Hours.Text = frequency.Hours.ToString();
			Minutes.Text = frequency.Minutes.ToString();
			Seconds.Text = frequency.Seconds.ToString();

			PhraseUpdateOrderComboBox.SelectedIndex = (byte)phraseUpd;
			WallpaperUpdateOrderComboBox.SelectedIndex = (byte)wallpaperUpd;
			UpdateFrequency = frequency;
		}


		private void SoundChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!(soundsComboBox.SelectedItem is FileInfo))
				return;

			_player.SoundLocation = (soundsComboBox.SelectedItem as FileInfo).FullName;
			_player.Play();
		}

		private void SaveClick(object sender, RoutedEventArgs e)
		{
			int hours, minutes, seconds;

			if (!int.TryParse(Hours.Text, out hours)     ||
			    !int.TryParse(Minutes.Text, out minutes) ||
			    !int.TryParse(Seconds.Text, out seconds))
			{
				MessageBox.Show("Incorrect update time", "Failed attempt", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			TimeSpan updFrequency = new TimeSpan(hours, minutes, seconds);
			if (updFrequency == TimeSpan.Zero)
			{
				MessageBox.Show("An update time cannot be zero", "Failed attempt", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if (WallpaperUpdateOrderComboBox.SelectedValue == null || PhraseUpdateOrderComboBox.SelectedValue == null)
				return;

			WallpaperUpdateOrder = (UpdateOrder) WallpaperUpdateOrderComboBox.SelectedIndex;
			PhraseUpdateOrder = (UpdateOrder) PhraseUpdateOrderComboBox.SelectedIndex;
			UpdateFrequency = updFrequency;

			if (soundsComboBox.SelectedItem is String)
				SoundFile = null;
			else
				SoundFile = (FileInfo)soundsComboBox.SelectedItem;

			DialogResult = true;
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void TimePreviewInput(object sender, TextCompositionEventArgs e)
		{
			int tmp;
			if(!int.TryParse((e.OriginalSource as TextBox).Text + e.TextComposition.Text, out tmp))
				e.Handled = true;
		}
	}
}
