using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Wallanguager.WallpaperEngine;
using WpfColorFontDialog;

namespace Wallanguager.Windows
{
	/// <summary>
	/// Логика взаимодействия для GeneralWallpaperSettings.xaml
	/// </summary>
	public partial class GeneralWallpaperSettings : Window
	{
		public FontInfo GeneralFontSetting { get; private set; }
		public SignFormat GeneralSignFormat { get; private set; }
		public WallpaperStyle GeneralWallpaperStyle { get; private set; }
		public GeneralWallpaperSettings(FontInfo settings, SignFormat signFormat, WallpaperStyle style)
		{
			InitializeComponent();
			generalWallpaperStyleComboBox.ItemsSource = Enum.GetValues(typeof(WallpaperStyle));

			GeneralFontSetting = settings;
			GeneralSignFormat = signFormat;
			GeneralWallpaperStyle = style;

			generalFontColorChooser.SelectedFont = settings;
			generalSignFormatTextBox.Text = signFormat.Pattern;
			generalWallpaperStyleComboBox.SelectedItem = style;
		}

		private void ButtonSaveClick(object sender, RoutedEventArgs e)
		{
			GeneralFontSetting = generalFontColorChooser.SelectedFont;
			GeneralSignFormat = new SignFormat(generalSignFormatTextBox.Text);
			GeneralWallpaperStyle = (WallpaperStyle) generalWallpaperStyleComboBox.SelectedItem;

			DialogResult = true;
		}

		private void ButtonCancelClick(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
