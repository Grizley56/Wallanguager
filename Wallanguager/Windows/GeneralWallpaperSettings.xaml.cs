using System.Windows;
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

			GeneralFontSetting = settings;
			GeneralSignFormat = signFormat;
			GeneralWallpaperStyle = style;

			generalFontColorChooser.SelectedFont = settings;
			generalSignFormatTextBox.Text = signFormat.Pattern;
			generalWallpaperStyleComboBox.SelectedIndex = (int) style;
		}

		private void ButtonSaveClick(object sender, RoutedEventArgs e)
		{
			GeneralFontSetting = generalFontColorChooser.SelectedFont;
			GeneralSignFormat = new SignFormat(generalSignFormatTextBox.Text);
			GeneralWallpaperStyle = (WallpaperStyle)generalWallpaperStyleComboBox.SelectedIndex;
			DialogResult = true;
		}

		private void ButtonCancelClick(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
