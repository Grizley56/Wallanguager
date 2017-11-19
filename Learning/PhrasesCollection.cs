using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallanguager.Learning
{
	public class PhrasesCollection: IEnumerable<PhrasesGroup>
	{
		public PhrasesGroup this[string key] => _phrasesGroups.FirstOrDefault(i => i.GroupName == key);
		public PhrasesGroup this[int index] => _phrasesGroups[index];

		private List<PhrasesGroup> _phrasesGroups = new List<PhrasesGroup>();

		public bool AddGroup(PhrasesGroup value)
		{
			if (_phrasesGroups.Exists(i => i.GroupName == value.GroupName))
				return false;
			
			_phrasesGroups.Add(value);
			return true;
		}

		public bool RemoveGroup(PhrasesGroup group)
		{
			return _phrasesGroups.Remove(group);
		}

		public IEnumerator<PhrasesGroup> GetEnumerator()
		{
			return ((IEnumerable<PhrasesGroup>)_phrasesGroups).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<PhrasesGroup>)_phrasesGroups).GetEnumerator();
		}
	}
}
