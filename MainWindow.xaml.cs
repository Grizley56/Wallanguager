using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Wallanguager.WallpaperEngine;
using WpfColorFontDialog;

namespace Wallanguager
{
	public partial class MainWindow : Window
	{
		private readonly Dictionary<DependencyProperty, Binding> _bindings = new Dictionary<DependencyProperty, Binding>();

		private readonly WallpaperController _wallpaperController = new WallpaperController(
			new Uri(Environment.CurrentDirectory + "/Images/"));

		private GeneralWallpaperSettings _settings;

		private Wallpaper _selectedWallpaper;

		private Signature _defaultSignature = new Signature("Example", "Пример");

		private readonly OpenFileDialog _fileDialog = new OpenFileDialog
		{
			Filter = "Image Files(*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG;*.JPEG",
			Multiselect = true
		};

		public MainWindow()
		{
			InitializeComponent();
			InitializeBindings();
			InitializeController();
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

		private void Wallpapers_WallpaperRemoved(object sender, WallpaperRemovedEventArgs e)
		{
			images.Children.Remove(e.RemovedItem.UIElementImage);
			e.RemovedItem.UIElementImage.MouseLeftButtonDown -= Image_MouseLeftButtonDown; // GC optimize
		}

		private void Wallpapers_WallpaperAdded(object sender, WallpaperAddedEventArgs e)
		{
			images.Children.Add(e.AddedItem.UIElementImage);
			e.AddedItem.UIElementImage.MouseLeftButtonDown += Image_MouseLeftButtonDown;

			foreach (var pair in _bindings)
				e.AddedItem.UIElementImage.SetBinding(pair.Key, pair.Value);
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

		private void UpdateWallpaperZoomedImage(Point? position = null)
		{
			if (_selectedWallpaper == null)
				throw new NullReferenceException();

			if (!_selectedWallpaper.IsFixed && !position.HasValue)
				throw new ArgumentNullException(nameof(position));

			if (_selectedWallpaper.IsFixed)
				WallpaperZoomed.Source = _selectedWallpaper.GetFixedSignedImage(_defaultSignature);
			else
				WallpaperZoomed.Source = _selectedWallpaper.GetSignedImage(position.Value, _defaultSignature);
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

	}
}