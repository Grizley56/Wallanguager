using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Easy.Logger;
using Microsoft.Win32;
using Wallanguager.Learning;
using Wallanguager.WallpaperEngine;
using WpfColorFontDialog;

namespace Wallanguager.Windows
{
	public partial class MainWindow : Window
	{
		private readonly Dictionary<DependencyProperty, Binding> _bindings = new Dictionary<DependencyProperty, Binding>();

		private readonly WallpaperController _wallpaperController = new WallpaperController(
			new Uri(Environment.CurrentDirectory));

		private GeneralWallpaperSettings _settings;

		private DisplaySettings _displaySettings;

		private GroupAddWindow _groupWindow;

		private Wallpaper _selectedWallpaper;

		private FileInfo _selectedSound;

		private SoundPlayer _player = new SoundPlayer();

		private readonly OpenFileDialog _fileDialog = new OpenFileDialog
		{
			Filter = "Image Files(*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG;*.JPEG",
			Multiselect = true
		};

		
		public MainWindow()
		{
			InitializeComponent();
			InitializeBindings();

			try
			{
				InitializeController();
			}
			catch (Exception e)
			{
				Log4NetService.Instance.GetLogger<MainWindow>().Fatal(e);
				throw;
			}

			LoadData();

		}

		private void LoadData()
		{
			//get data from .bin file
			//create WallpaperCollection
			_wallpaperController.Deserialize();
		}

		private void InitializeBindings()
		{
			var offsetBinding = new Binding();
			var sizeBinding = new Binding();

			offsetBinding.Source = wallpaperOffset;
			offsetBinding.Path = new PropertyPath("Value");

			sizeBinding.Source = wallpaperSize;
			sizeBinding.Path = new PropertyPath("Value");

			_bindings.Add(MarginProperty, offsetBinding);
			_bindings.Add(MaxWidthProperty, sizeBinding);
			_bindings.Add(MaxHeightProperty, sizeBinding);

			GroupListView.ItemsSource = _wallpaperController.PhrasesGroups;
		}

		private void InitializeController()
		{
			_wallpaperController.Wallpapers.WallpaperAdded += Wallpapers_WallpaperAdded;
			_wallpaperController.Wallpapers.WallpaperRemoved += Wallpapers_WallpaperRemoved;
			_wallpaperController.LoadDefaultWallpapers();
		}

		private void ToggleCheckboxes()
		{
			isFontByDefault.IsChecked = _selectedWallpaper.IsFontByDefault;
			fontColorChooser.IsEnabled = !_selectedWallpaper.IsFontByDefault;

			isStyleByDefault.IsChecked = _selectedWallpaper.IsStyleByDefault;
			styleChooser.IsEnabled = !_selectedWallpaper.IsStyleByDefault;
		}

		private Point ScreenToWallpaperPosition(Point screenPosition)
		{
			var sourceSize = new Size(_selectedWallpaper.SourceImage.Width, _selectedWallpaper.SourceImage.Height);
			var destinationSize = WallpaperZoomed.RenderSize;

			var scale = sourceSize.Width / destinationSize.Width;
			var scaleMatrix = new Matrix { M11 = scale, M22 = scale };
			return screenPosition * scaleMatrix;
		}

		private async void UpdateWallpaperZoomedImage(Point? position = null)
		{
			if (_selectedWallpaper == null)
			{
				Log4NetService.Instance.GetLogger<MainWindow>()
					.Error($"{nameof(UpdateWallpaperZoomedImage)} was called, " +
								 $"but {nameof(_selectedWallpaper)} is not defined");
				return;
			}

			if (!_selectedWallpaper.IsFixed && !position.HasValue)
			{
				Log4NetService.Instance.GetLogger<MainWindow>()
					.Fatal($"call {nameof(UpdateWallpaperZoomedImage)} " +
					       $"with position as null parameter [as UnFixedWallзaper]");
				throw new ArgumentNullException(nameof(position));
			}

			try
			{
				WallpaperZoomed.Source = _selectedWallpaper.IsFixed
					? await _selectedWallpaper.GetFixedSignedImage(_wallpaperController.DefaultSignature)
					: await _selectedWallpaper.GetSignedImage(position.Value, _wallpaperController.DefaultSignature);
			}
			catch (Exception e)
			{
				Log4NetService.Instance.GetLogger<MainWindow>().Fatal(e);
				throw;
			}
		}

		private void Wallpapers_WallpaperRemoved(object sender, WallpaperRemovedEventArgs e)
		{
			images.Children.Remove(e.RemovedItem.UIElementImage);
			e.RemovedItem.UIElementImage.MouseLeftButtonDown -= Image_MouseLeftButtonDown; // GC optimize

#if DEBUG
			Easy.Logger.Log4NetService.Instance.GetLogger<Wallpaper>()
				.Debug($"Wallpaper was removed \"{e.RemovedItem.SourceImage.UriSource}\"");
#endif
		}

		private void Wallpapers_WallpaperAdded(object sender, WallpaperAddedEventArgs e)
		{
			images.Children.Add(e.AddedItem.UIElementImage);
			e.AddedItem.UIElementImage.MouseLeftButtonDown += Image_MouseLeftButtonDown;

			foreach (var pair in _bindings)
				e.AddedItem.UIElementImage.SetBinding(pair.Key, pair.Value);

#if DEBUG
			Easy.Logger.Log4NetService.Instance.GetLogger<Wallpaper>()
				.Debug($"Wallpaper was added \"{e.AddedItem.SourceImage.UriSource}\"");
#endif
		}

		private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_selectedWallpaper = _wallpaperController.Wallpapers.First(i => 
				ReferenceEquals(i.UIElementImage, sender as Image));
			
			if (_selectedWallpaper == null)
				return;

			ToggleCheckboxes();

			fontColorChooser.SelectedFont = _selectedWallpaper.Font;
			styleChooser.SelectedIndex = (int) _selectedWallpaper.Style;

			if (_selectedWallpaper.IsFixed)
				UpdateWallpaperZoomedImage();
			else
				WallpaperZoomed.Source = _selectedWallpaper.SourceImage;
		}

		private void GeneralWallpaperSettingsOpen(object sender, RoutedEventArgs e)
		{
			_settings = new GeneralWallpaperSettings(Wallpaper.GeneralDefaultFont,
				_wallpaperController.DefaultSignature.Format, Wallpaper.GeneralDefaultStyle)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			bool? result = _settings.ShowDialog();

			if (result.HasValue && result.Value)
			{
				Wallpaper.GeneralDefaultFont = _settings.GeneralFontSetting;
				Wallpaper.GeneralDefaultStyle = _settings.GeneralWallpaperStyle;
				_wallpaperController.DefaultSignature.Format = _settings.GeneralSignFormat;

				if (_selectedWallpaper == null)
					return;

				fontColorChooser.SelectedFont = _selectedWallpaper.Font;
				styleChooser.SelectedIndex = (int) _selectedWallpaper.Style;
				if (_selectedWallpaper.IsFixed)
					UpdateWallpaperZoomedImage();
			}
		}

		private void ZoomedImageMouseMove(object sender, MouseEventArgs e)
		{
			if (!_selectedWallpaper.IsFixed)
				UpdateWallpaperZoomedImage(ScreenToWallpaperPosition(e.GetPosition(WallpaperZoomed)));
		}

		private void ZoomedImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_selectedWallpaper.IsFixed)
				return;

			_selectedWallpaper.Fix(ScreenToWallpaperPosition(e.GetPosition(WallpaperZoomed)));
			UpdateWallpaperZoomedImage();
		}

		private void ZoomedImageMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!_selectedWallpaper.IsFixed)
				return;

			_selectedWallpaper.UnFix();
			UpdateWallpaperZoomedImage(ScreenToWallpaperPosition(e.GetPosition(WallpaperZoomed)));
		}

		private void FontColorChooserColorFontChanged(object sender, ColorFontSelectChangedEventArgs e)
		{
			if (_selectedWallpaper == null)
				return;

			_selectedWallpaper.Font = fontColorChooser.SelectedFont;

			if (_selectedWallpaper.IsFixed)
				UpdateWallpaperZoomedImage();
		}

		private void StyleChooserSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedWallpaper.Style = (WallpaperStyle) (sender as ComboBox).SelectedIndex;
		}

		private void IsFontByDefaultClick(object sender, RoutedEventArgs e)
		{
			CheckBox checkbox = sender as CheckBox;
			if (checkbox == null)
				return;

			_selectedWallpaper.IsFontByDefault = checkbox.IsChecked.Value;
			fontColorChooser.IsEnabled = !checkbox.IsChecked.Value;

			if (!checkbox.IsChecked.Value)
				fontColorChooser.SelectedFont = _selectedWallpaper.Font;

			if(_selectedWallpaper.IsFixed)
				UpdateWallpaperZoomedImage();
		}

		private void IsStyleByDefaultClick(object sender, RoutedEventArgs e)
		{
			CheckBox checkBox = sender as CheckBox;

			_selectedWallpaper.IsStyleByDefault = checkBox.IsChecked.Value;
			styleChooser.IsEnabled = !checkBox.IsChecked.Value;

			styleChooser.SelectedIndex = (int)_selectedWallpaper.Style;
		}

		private void AddWallpaperClick(object sender, RoutedEventArgs e)
		{
			if (_fileDialog.ShowDialog() != true)
				return;

			IEnumerable<Uri> uriList = _fileDialog.FileNames.Select(i => new Uri(i) );
			foreach (Uri uri in uriList)
				_wallpaperController.AddWallpaper(uri);
		}

		private void RemoveWallpaperClick(object sender, RoutedEventArgs e)
		{
			if (_selectedWallpaper == null)
				return;

			_wallpaperController.RemoveWallpaper(_selectedWallpaper);
			WallpaperZoomed.Source = null;
		}


		private void AddGroupClick(object sender, RoutedEventArgs e)
		{
			_groupWindow = new GroupAddWindow((newGroup)
				=> GroupAddUpdateRequirement(newGroup))
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			bool? result = _groupWindow.ShowDialog();

			if (result == null || !result.Value)
				return;

			_wallpaperController.PhrasesGroups.Add(new PhrasesGroup(_groupWindow.Group.GroupName,
				_groupWindow.Group.GroupTheme, _groupWindow.Group.ToLanguage, _groupWindow.Group.FromLanguage));

			UpdateGridViewColumnsSize(GroupGridViewColumns);
		}

		private void RemoveGroupClick(object sender, RoutedEventArgs e)
		{
			if ((GroupListView.SelectedItem as PhrasesGroup) == null)
			{
				MessageBox.Show("Select any group", "Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if ((MessageBox.Show("Are you sure you want to delete this group?", "Confirm action",
				     MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes))
			{
				_wallpaperController.PhrasesGroups.Remove((PhrasesGroup)GroupListView.SelectedItem);
				UpdateGridViewColumnsSize(GroupGridViewColumns);
			}

		}

		private void UpdateGroupClick(object sender, RoutedEventArgs e)
		{
			if((GroupListView.SelectedItem as PhrasesGroup) == null)
			{
				MessageBox.Show("Select any group", "Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			PhrasesGroup oldGroup = (PhrasesGroup) GroupListView.SelectedItem;

			_groupWindow = new GroupAddWindow((newGroup)
				=> GroupAddUpdateRequirement(newGroup, oldGroup), oldGroup);

			bool? result = _groupWindow.ShowDialog();

			if (result == null || !result.Value)
				return;

			oldGroup.GroupName = _groupWindow.Group.GroupName;
			oldGroup.GroupTheme = _groupWindow.Group.GroupTheme;
			oldGroup.FromLanguage = _groupWindow.Group.FromLanguage;
			oldGroup.ToLanguage = _groupWindow.Group.ToLanguage;

			UpdateGridViewColumnsSize(GroupGridViewColumns);
		}

		private bool GroupAddUpdateRequirement(PhrasesGroup newGroup, PhrasesGroup oldGroup = null)
		{
			if (newGroup.FromLanguage.Equals(newGroup.ToLanguage))
			{
				MessageBox.Show("The languages are the same", "Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}

			return _wallpaperController.PhrasesGroups.All<PhrasesGroup>(delegate (PhrasesGroup j)
			{
				if (j.GroupName != newGroup.GroupName || j == oldGroup)
					return true;

				MessageBox.Show($"Group {newGroup.GroupName} already exist!", "Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			});
		}


		private static void UpdateGridViewColumnsSize(GridView gridView)
		{
			foreach (GridViewColumn c in gridView.Columns)
			{
				if (double.IsNaN(c.Width))
				{
					c.Width = c.ActualWidth;
				}
				c.Width = double.NaN;
			}
		}
		
		private void AddPhraseClick(object sender, RoutedEventArgs e)
		{
			PhrasesGroup selectedGroup = GroupListView.SelectedItem as PhrasesGroup;

			if (selectedGroup == null)
				return;

			string phrasesFullText = new TextRange(PhrasesRichTextBox.Document.ContentStart,
																		  PhrasesRichTextBox.Document.ContentEnd)
																		  .Text;

			string[] phrases = phrasesFullText.Split('\n');
			foreach (var phrase in phrases)
				if(phrase.Trim() != String.Empty)
					selectedGroup.AddPhrase(phrase.Trim());

			PhrasesRichTextBox.Document.Blocks.Clear();
		}

		private void RemovePhraseClick(object sender, RoutedEventArgs e)
		{
			PhrasesGroup phrasesGroup = (PhrasesGroup) GroupListView.SelectedItem;
			//Check is not necessary, because the button is not clickable while the selected one is null
			if (PhrasesListBox.SelectedItems.Count == 0)
			{
				MessageBox.Show("Select phrase for remove", "Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			Phrase[] phrases = PhrasesListBox.SelectedItems.Cast<Phrase>().ToArray();

			foreach (Phrase phrase in phrases)
				phrasesGroup.RemovePhrase(phrase);
		}

		private async void StartButtonClick(object sender, RoutedEventArgs e)
		{
			await _wallpaperController.Start((_, __) =>
			{
				if (_selectedSound == null)
					return;

				_player.SoundLocation = _selectedSound.FullName;
				_player.Play();
			});
		}

		private void DisplaySettingsClick(object sender, RoutedEventArgs e)
		{
			_displaySettings = new DisplaySettings(_wallpaperController.UpdateFrequency, 
				_selectedSound, _wallpaperController.WallpaperUpdateOrder, _wallpaperController.PhraseUpdateOrder )
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};
			if (_displaySettings.ShowDialog() == true)
			{
				_wallpaperController.PhraseUpdateOrder = _displaySettings.PhraseUpdateOrder;
				_wallpaperController.WallpaperUpdateOrder = _displaySettings.WallpaperUpdateOrder;
				_wallpaperController.UpdateFrequency = _displaySettings.UpdateFrequency;
				_selectedSound = _displaySettings.SoundFile;
			}
			
		}

		private void StopButtonClick(object sender, RoutedEventArgs e)
		{
			_wallpaperController.Stop();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_wallpaperController.Stop();
			base.OnClosing(e);
		}

		private void GroupListViewItemDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var phraseGroup = (sender as ListViewItem).Content as PhrasesGroup;
			phraseGroup.IsEnabled = !phraseGroup.IsEnabled;
		}
	}
}