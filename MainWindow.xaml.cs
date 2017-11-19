using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

using Wallanguager.WallpaperEngine;
using Wallanguager.Learning;
using WpfColorFontDialog;
using Easy.Logger;

namespace Wallanguager
{
	public partial class MainWindow : Window
	{
		private readonly Dictionary<DependencyProperty, Binding> _bindings = new Dictionary<DependencyProperty, Binding>();

		private readonly WallpaperController _wallpaperController = new WallpaperController(
			new Uri(Environment.CurrentDirectory));

		private GeneralWallpaperSettings _settings;

		private GroupAddWindow _groupWindow;

		private Wallpaper _selectedWallpaper;

		private readonly Signature _defaultSignature = new Signature("Example", "Пример");


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

			// For tests
			Language a = (Application.Current.Properties["Languages"] as Language[])[0];
			Language b = (Application.Current.Properties["Languages"] as Language[])[1];
			Language c = (Application.Current.Properties["Languages"] as Language[])[2];

			_wallpaperController.Phrases.AddGroup(new PhrasesGroup("Test #1", "Mixed", a, b));
			_wallpaperController.Phrases.AddGroup(new PhrasesGroup("Test #2", "Body", b, c));
			_wallpaperController.Phrases.AddGroup(new PhrasesGroup("Test #3", "Sport", c, a));
			// ---------

			GroupListView.ItemsSource = _wallpaperController.Phrases;
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

		private void UpdateWallpaperZoomedImage(Point? position = null)
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
					       $"with position as null parameter [as UnFixedWallaper]");
				throw new ArgumentNullException(nameof(position));
			}

			try
			{
				WallpaperZoomed.Source = _selectedWallpaper.IsFixed
					? _selectedWallpaper.GetFixedSignedImage(_defaultSignature)
					: _selectedWallpaper.GetSignedImage(position.Value, _defaultSignature);
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
				.Debug($"Wallaper was removed \"{e.RemovedItem.SourceImage.UriSource}\"");
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
				.Debug($"Wallaper was added \"{e.AddedItem.SourceImage.UriSource}\"");
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
				_defaultSignature.Format, Wallpaper.GeneralDefaultStyle)
			{
				Owner = this,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			bool? result = _settings.ShowDialog();

			if (result.HasValue && result.Value)
			{
				Wallpaper.GeneralDefaultFont = _settings.GeneralFontSetting;
				Wallpaper.GeneralDefaultStyle = _settings.GeneralWallpaperStyle;
				_defaultSignature.Format = _settings.GeneralSignFormat;

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
				=> GroupAddUpdateRequirement(newGroup));

			bool? result = _groupWindow.ShowDialog();

			if (result != null && result.Value)
				_wallpaperController.Phrases.AddGroup(new PhrasesGroup(_groupWindow.Group.GroupName,
					_groupWindow.Group.GroupTheme, _groupWindow.Group.ToLanguage, _groupWindow.Group.ToLanguage));
		}

		private void RemoveGroupClick(object sender, RoutedEventArgs e)
		{
			if ((GroupListView.SelectedItem as PhrasesGroup) == null)
			{
				MessageBox.Show("Select any group", "Fail");
				return;
			}

			if ((MessageBox.Show("Are you sure you want to delete this group?", "Confirm action",
				     MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes))
				_wallpaperController.Phrases.RemoveGroup((PhrasesGroup)GroupListView.SelectedItem);
		}

		private void UpdateGroupClick(object sender, RoutedEventArgs e)
		{
			if((GroupListView.SelectedItem as PhrasesGroup) == null)
			{
				MessageBox.Show("Select any group", "Fail");
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
		}

		private bool GroupAddUpdateRequirement(PhrasesGroup newGroup, PhrasesGroup oldGroup = null)
		{
			return _wallpaperController.Phrases.All<PhrasesGroup>(delegate (PhrasesGroup j)
			{
				if (j.GroupName != newGroup.GroupName || j == oldGroup)
					return true;

				MessageBox.Show($"Group {newGroup.GroupName} already exist!", "Fail");
				return false;
			});
		}
	}
}