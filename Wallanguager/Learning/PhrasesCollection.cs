using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallanguager.Learning
{
	public sealed class PhrasesCollection: ObservableCollection<PhrasesGroup>
	{
		public PhrasesGroup this[string key] => Items.FirstOrDefault(i => i.GroupName == key);

		public PhrasesCollection()
		{
			CollectionChanged += PhrasesCollection_CollectionChanged;
		}

		private void PhrasesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			//TODO: skip the method if it is possible

			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems)
				{
					((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
				}
			}
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems)
				{
					((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
				}
			}
		}

		public new bool Add(PhrasesGroup value)
		{
			if (Items.ToList().Exists(i => i.GroupName == value.GroupName))
				return false;

			base.Add(value);
			return true;
		}

		private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((PhrasesGroup)sender));

			OnCollectionChanged(args);
		}

	}
}
