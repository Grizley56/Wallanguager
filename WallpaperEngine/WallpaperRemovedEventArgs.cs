using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallanguager.WallpaperEngine
{
	public class WallpaperRemovedEventArgs : EventArgs
	{
		public readonly Wallpaper RemovedItem;
		public WallpaperRemovedEventArgs(Wallpaper item)
		{
			RemovedItem = item;
		}
	}
}
