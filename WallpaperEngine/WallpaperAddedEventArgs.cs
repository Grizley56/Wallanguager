using System;

namespace Wallanguager.WallpaperEngine
{
	public class WallpaperAddedEventArgs : EventArgs
	{
		public readonly Wallpaper AddedItem;
		public WallpaperAddedEventArgs(Wallpaper item)
		{
			AddedItem = item;
		}
	}
}