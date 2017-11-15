using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Wallanguager.WallpaperEngine
{
	public class WallpaperController
	{
		//TBD : Translate object
		//TBD : List of supported words

		public WallpaperCollection Wallpapers { get; private set; } = new WallpaperCollection();

		public Uri DefaultWallpapersPath { get; private set; }

		public WallpaperController(Uri defaultWallpapersPath)
		{
			DefaultWallpapersPath = defaultWallpapersPath;
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
			Image img = new Image();
			BitmapImage bitmap = new BitmapImage(uri);
			img.Source = bitmap;
			return new Wallpaper(img, bitmap);
		}
	}
}
