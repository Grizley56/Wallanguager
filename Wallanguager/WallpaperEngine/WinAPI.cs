using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Wallanguager.WallpaperEngine
{
	public static class WinAPI
	{
		private const int SPI_SETDESKWALLPAPER = 0x0014;
		private const int SPI_GETDESKWALLPAPER = 0x0073;

		private const int SPIF_UPDATEINIFILE = 0x01;
		private const int SPIF_SENDWININICHANGE = 0x02;
		private const int MAX_PATH = 260;

		private static string _buffer;
		private const string WALLPAPER_STYLE_PATH = @"Control Panel\Desktop";

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

		public static string GetWallpaper()
		{
			_buffer = new string('\0', 260);
			SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, _buffer, 0);
			return _buffer.TrimEnd();
		}

		public static bool SetWallpaper(string path)
		{
			return SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE) != 0;
		}

		public static WallpaperStyle GetWallpaperStyle()
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(WALLPAPER_STYLE_PATH, false);
			var value = (string)key.GetValue("WallpaperStyle");

			if (value != "0")
				return (WallpaperStyle) int.Parse(value);

			var style = (string)key.GetValue("TileWallpaper") == "1" ? WallpaperStyle.Tile : WallpaperStyle.Center;
			return style;
		}

		public static void SetWallpaperStyle(WallpaperStyle userStyle)
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(WALLPAPER_STYLE_PATH, true);
			var value = (string)key.GetValue("WallpaperStyle");

			if (userStyle == WallpaperStyle.Tile || userStyle == WallpaperStyle.Center)
			{
				key.SetValue("WallpaperStyle", "0");
				key.SetValue("TileWallpaper", ((int)userStyle).ToString());
				return;
			}

			key.SetValue("WallpaperStyle", ((int) userStyle).ToString());
			key.SetValue("TileWallpaper", "0");
		}
	}
}
