using System;
using System.Collections;
using System.Globalization;

namespace Mordex.Windows.UI.StatusBar
{
	public class StatusBarPanelCollection : CollectionBase
	{
		internal MxStatusBar _statusBar;

		public MxStatusBarPanel this[int index] => (MxStatusBarPanel)base.List[index];

		public MxStatusBarPanel this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (MxStatusBarPanel item in base.List)
				{
					if (string.Compare(item.Key, key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal StatusBarPanelCollection(MxStatusBar statusBar)
		{
			_statusBar = statusBar;
		}

		public bool Contains(MxStatusBarPanel panel)
		{
			return base.List.Contains(panel);
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxStatusBarPanel uiStatusBarPanel = (MxStatusBarPanel)enumerator.Current;
					if (string.Compare(uiStatusBarPanel.Key, key, ignoreCase: true, invariantCulture) == 0)
					{
						return true;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return false;
		}

		public int IndexOf(MxStatusBarPanel panel)
		{
			return base.List.IndexOf(panel);
		}

		public void AddRange(MxStatusBarPanel[] panels)
		{
			foreach (MxStatusBarPanel panel in panels)
			{
				Add(panel);
			}
		}

		public void Insert(int index, MxStatusBarPanel panel)
		{
			base.List.Insert(index, panel);
		}

		public void Add(MxStatusBarPanel panel)
		{
			base.List.Add(panel);
		}

		public void Remove(MxStatusBarPanel panel)
		{
			base.List.Remove(panel);
		}

		public new void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			MxStatusBarPanel uiStatusBarPanel = (MxStatusBarPanel)value;
			uiStatusBarPanel._owner = this;
			NotifyChanged();
		}

		protected override void OnClear()
		{
			NotifyChanged();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxStatusBarPanel uiStatusBarPanel = (MxStatusBarPanel)value;
			if (uiStatusBarPanel.Control != null)
			{
				_statusBar.RemovePanelControl(uiStatusBarPanel);
			}
			uiStatusBarPanel._owner = null;
			NotifyChanged();
		}

		public new void Clear()
		{
			while (base.List.Count > 0)
			{
				Remove((MxStatusBarPanel)base.List[0]);
			}
		}

		internal void NotifyChanged()
		{
			if (_statusBar != null)
			{
				_statusBar.InvalidateLayout();
			}
		}
	}
}
