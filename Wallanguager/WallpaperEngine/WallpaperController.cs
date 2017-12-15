using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GoogleTranslateFreeApi;
using Wallanguager.Learning;
using Image = System.Windows.Controls.Image;

namespace Wallanguager.WallpaperEngine
{
	public class WallpaperController
	{
		private GoogleTranslator _translator = new GoogleTranslator();
		private Timer _timer;
		private Phrase[] _tempPhrases;
		private Wallpaper[] _tempWallpapers;
		private Action<Wallpaper, Phrase> _callBack;
		private Random _random = new Random();
		private string _userWallpaper;
		private WallpaperStyle _userStyle;

		public Signature DefaultSignature { get; set; } = new Signature("Example", "Пример");
		public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromSeconds(3);
		public UpdateOrder WallpaperUpdateOrder { get; set; } = UpdateOrder.Randomly;
		public UpdateOrder PhraseUpdateOrder { get; set; } = UpdateOrder.Randomly;

		public bool IsRunning { get; private set; }

		public WallpaperCollection Wallpapers { get; private set; } = new WallpaperCollection();

		public PhrasesCollection PhrasesGroups { get; private set; } = new PhrasesCollection();

		public Uri DefaultWallpapersPath { get; private set; }

		public string WallpaperTempPath { get; set; }
		public const string TEMP_FILE_NAME = "wallpaper_temp.jpeg";

		public WallpaperController(Uri defaultWallpapersPath)
		{
			DefaultWallpapersPath = defaultWallpapersPath;
			WallpaperTempPath = Path.Combine(Path.GetTempPath(), "Wallanguager_tmp");

			_timer = new Timer(WallpaperChanging, null, Timeout.Infinite, Timeout.Infinite);
		}

		private int _currentWallpaperIndex;
		private int _currentPhraseIndex;

		private async void WallpaperChanging(object state)
		{
			Wallpaper currentWallpaper = _tempWallpapers[_currentWallpaperIndex++];
			Phrase phrase = _tempPhrases[_currentPhraseIndex++];
			
			var image = await currentWallpaper.GetFixedSignedImage(new Signature(phrase.OriginalText, phrase.TranslatedText,
				DefaultSignature.Format));

			if (_currentWallpaperIndex >= Wallpapers.Count)
				_currentWallpaperIndex = 0;
			if (_currentPhraseIndex >= _tempPhrases.Length)
				_currentPhraseIndex = 0;

			if (!Directory.Exists(WallpaperTempPath))
				Directory.CreateDirectory(WallpaperTempPath);

			string wallpaperPath = Path.Combine(WallpaperTempPath, TEMP_FILE_NAME);

			image.Dispatcher.Invoke(() => currentWallpaper.SaveToFile(image, wallpaperPath));


			WinAPI.SetWallpaper(wallpaperPath);
			WinAPI.SetWallpaperStyle(currentWallpaper.Style);

			_callBack?.Invoke(currentWallpaper, phrase);
		}

		public void AddWallpaper(Uri url)
		{
			Wallpapers.Add(GetNewWallpaperByUri(url));
		}

		public void AddWallpaper(Wallpaper wallpaper)
		{
			Wallpapers.Add(wallpaper);
		}

		public void RemoveWallpaper(Wallpaper wallpaper)
		{
			Wallpapers.Remove(wallpaper);
		}

		public void LoadDefaultWallpapers()
		{
			DirectoryInfo wallpaperDir = new DirectoryInfo(DefaultWallpapersPath.AbsolutePath);

			if (!wallpaperDir.Exists)
			{
				Easy.Logger.Log4NetService.Instance.GetLogger<Wallpaper>()
					.Warn("Default directory dont exist " + wallpaperDir.FullName);
				return;
			}

			foreach(FileInfo file in wallpaperDir.GetFiles())
			{
				if (file.Extension == ".jpg" || file.Extension == ".png" || file.Extension == ".gif" || file.Extension == ".jpeg")
					Wallpapers.Add(GetNewWallpaperByUri(new Uri(file.FullName)));
			}
		}

		private static Wallpaper GetNewWallpaperByUri(Uri uri)
		{
			Image img = new Image() { Source = new BitmapImage(uri) };
			return new Wallpaper(img, (BitmapImage) img.Source);
		}

		public async Task Start(Action<Wallpaper, Phrase> callBack = null)
		{
			if (IsRunning)
				return;

			_userWallpaper = WinAPI.GetWallpaper();
			_userStyle = WinAPI.GetWallpaperStyle();

			if (Wallpapers.Count == 0)
			{
				MessageBox.Show("List of wallpapers is empty", "Failed attempt", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			if (Wallpapers.All(i => i.IsFixed == false))
			{
				MessageBox.Show("Select a phrase display point of any wallpaper", "Failed attempt",
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			if (PhrasesGroups.Count == 0 || PhrasesGroups.All(i => i.Phrases.Count == 0) || !PhrasesGroups.Any(i => i.IsEnabled))
			{
				MessageBox.Show("List of phrases is empty", "Failed attempt", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			_currentWallpaperIndex = 0;
			_currentPhraseIndex = 0;

			PhrasesGroup phrasesGroup = null;

			try
			{
				foreach (PhrasesGroup group in PhrasesGroups)
				{
					if (!group.IsEnabled)
						continue;

					phrasesGroup = group;
					await phrasesGroup.TranslateRequireded(_translator);
				}
			}
			catch (GoogleTranslateIPBannedException e)
			{
				MessageBox.Show($"You have been banned on https://translate.google.{_translator.Domain}.\n" +
				                $"Try to use a proxy or wait a few hours.", "Oops!", MessageBoxButton.OK, MessageBoxImage.Error);
				Easy.Logger.Log4NetService.Instance.GetLogger<Wallpaper>().Info(e);
				//TODO: PROXY
			}
			catch (Exception e)
			{
				Easy.Logger.Log4NetService.Instance.GetLogger<WallpaperController>().Error(e);
				MessageBox.Show($"An error occurred while trying to translate a group \"{phrasesGroup.GroupName}\"\n" +
												"Group was disabled.", "Translation Fail", 
					MessageBoxButton.OK, MessageBoxImage.Error);
				phrasesGroup.IsEnabled = false;
			}


			_tempPhrases = PhraseUpdateOrder == UpdateOrder.Randomly 
				? PhrasesGroups.GetShuffledPhrases() 
				: (from grp in PhrasesGroups where grp.IsEnabled from phrase in grp.Phrases select phrase).ToArray();

			if (_tempPhrases.Length == 0)
			{
				MessageBox.Show("Run is stopped", "Failed attempt", MessageBoxButton.OK, MessageBoxImage.Stop);
				return;
			}

			_tempWallpapers = WallpaperUpdateOrder == UpdateOrder.Randomly
				? Wallpapers.OrderBy(i => _random.Next()).ToArray()
				: (from wallpaper in Wallpapers where wallpaper.IsFixed select wallpaper).ToArray();

			_callBack = callBack;
			_timer.Change(TimeSpan.Zero, UpdateFrequency);
			IsRunning = true;
		}

		public void Stop()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			IsRunning = false;

			WinAPI.SetWallpaper(_userWallpaper);
			WinAPI.SetWallpaperStyle(_userStyle);
		}

	}
}
