using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Wallanguager.WallpaperEngine
{
	public class WallpaperCollection : IList<Wallpaper>
	{
		public event EventHandler<WallpaperAddedEventArgs> WallpaperAdded;

		public event EventHandler<WallpaperRemovedEventArgs> WallpaperRemoved;

		private List<Wallpaper> _data = new List<Wallpaper>();

		public WallpaperCollection(IEnumerable<Wallpaper> wallpapers = null)
		{
			if (wallpapers != null)
				foreach (Wallpaper item in wallpapers)
					Add(item);
		}

		#region IList
		public Wallpaper this[int index]
		{
			get
			{
				return _data[index];
			}

			set
			{
				if (_data[index] == value)
					return;

				OnWallpaperRemoved(new WallpaperRemovedEventArgs(_data[index]));
				_data[index] = value;
				OnWallpaperAdded(new WallpaperAddedEventArgs(value));
			}
		}

		public int Count => _data.Count;

		public bool IsReadOnly => false;

		public void Add(Wallpaper item)
		{
			_data.Add(item);
			OnWallpaperAdded(new WallpaperAddedEventArgs(item));
		}

		public void Clear()
		{
			foreach(Wallpaper item in _data)
			{
				_data.Remove(item);
				OnWallpaperRemoved(new WallpaperRemovedEventArgs(item));
			}
		}

		public bool Contains(Wallpaper item)
		{
			return _data.Contains(item);
		}

		public void CopyTo(Wallpaper[] array, int arrayIndex)
		{
			_data.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Wallpaper> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		public int IndexOf(Wallpaper item)
		{
			return _data.IndexOf(item);
		}

		public void Insert(int index, Wallpaper item)
		{
			if (_data[index] != null)
				OnWallpaperRemoved(new WallpaperRemovedEventArgs(_data[index]));

			_data.Insert(index, item);
			OnWallpaperAdded(new WallpaperAddedEventArgs(item));
		}

		public bool Remove(Wallpaper item)
		{
			if (!_data.Remove(item))
				return false;

			OnWallpaperRemoved(new WallpaperRemovedEventArgs(item));
			return true;
		}

		public void RemoveAt(int index)
		{
			OnWallpaperRemoved(new WallpaperRemovedEventArgs(_data[index]));
			_data.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}
		#endregion

		protected virtual void OnWallpaperAdded(WallpaperAddedEventArgs e)
		{
			WallpaperAdded?.Invoke(this, e);
		}
		protected virtual void OnWallpaperRemoved(WallpaperRemovedEventArgs e)
		{
			WallpaperRemoved?.Invoke(this, e);
		}
	}
}
