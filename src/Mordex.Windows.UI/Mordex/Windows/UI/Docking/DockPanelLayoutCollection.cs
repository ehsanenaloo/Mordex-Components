using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Docking
{
	public class DockPanelLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private DockPanelLayout _parentLayout;

		private MxDockManager _panelManager;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public MxDockManager PanelManager
		{
			get
			{
				if (_parentLayout == null)
				{
					return _panelManager;
				}
				return _parentLayout.GetPanelManager();
			}
		}

		public DockPanelLayout this[int index] => (DockPanelLayout)base.List[index];

		public DockPanelLayout this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (DockPanelLayout uiPanelLayout in this)
				{
					if (string.Compare(uiPanelLayout.Key, key, ignoreCase: true, invariantCulture) == 0)
					{
						return uiPanelLayout;
					}
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["Layout" + index]; propertyValue != null; propertyValue = info["Layout" + index])
			{
				DockPanelLayout uiPanelLayout = new DockPanelLayout();
				propertyValue.DeserializeObject(uiPanelLayout);
				Add(uiPanelLayout);
				index++;
			}
			foreach (DockPanelLayout loadedLayout in this)
			{
				if (loadedLayout._isCurrentLayout && PanelManager != null)
				{
					PanelManager.CurrentLayout = loadedLayout;
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			foreach (DockPanelLayout value in this)
			{
				info.AddValue("Layout" + index, value);
				index++;
			}
		}

		internal DockPanelLayoutCollection(MxDockManager manager)
		{
			_panelManager = manager;
		}

		internal DockPanelLayoutCollection(DockPanelLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal DockPanelLayout GetParentLayout()
		{
			return _parentLayout;
		}

		public void Add(DockPanelLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(DockPanelLayout[] layouts)
		{
			foreach (DockPanelLayout layout in layouts)
			{
				Add(layout);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.GetType() != typeof(DockPanelLayout))
			{
				throw new InvalidCastException();
			}
			DockPanelLayout uiPanelLayout = (DockPanelLayout)value;
			if (uiPanelLayout.GetOwningCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !uiPanelLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			DockPanelLayout uiPanelLayout = (DockPanelLayout)value;
			uiPanelLayout.SetOwningCollection(this);
			uiPanelLayout.EnsureIncrementalLayoutString();
		}

		public void Remove(DockPanelLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxDockManager panelManager = PanelManager;
			if (panelManager != null && panelManager.CurrentLayout == (DockPanelLayout)value)
			{
				panelManager.CurrentLayout = null;
			}
			((DockPanelLayout)value).SetOwningCollection(null);
		}

		protected override void OnClear()
		{
			foreach (DockPanelLayout uiPanelLayout in this)
			{
				uiPanelLayout.SetOwningCollection(null);
			}
		}

		public int IndexOf(DockPanelLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(DockPanelLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (DockPanelLayout uiPanelLayout in this)
			{
				if (string.Compare(uiPanelLayout.Key, layoutKey, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		internal DockPanelLayout FindCurrentLayout()
		{
			foreach (DockPanelLayout uiPanelLayout in this)
			{
				DockPanelLayout currentLayout = uiPanelLayout.FindCurrentLayout();
				if (currentLayout != null)
				{
					return currentLayout;
				}
			}
			return null;
		}
	}
}
