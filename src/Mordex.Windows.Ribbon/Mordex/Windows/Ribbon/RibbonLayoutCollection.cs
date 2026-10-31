using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class RibbonLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private RibbonLayout _parentLayout;

		internal MxRibbon _ribbon;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public MxRibbon Ribbon => _ribbon;

		public RibbonLayout this[int index] => (RibbonLayout)base.List[index];

		public RibbonLayout this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						RibbonLayout ribbonLayout = (RibbonLayout)enumerator.Current;
						if (string.Compare(ribbonLayout.Key, key, ignoreCase: true, invariantCulture) == 0)
						{
							return ribbonLayout;
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
				throw new ArgumentOutOfRangeException();
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int layoutIndex = 0;
			for (PropertyValue propertyValue = info["Layout" + layoutIndex]; propertyValue != null; propertyValue = info["Layout" + layoutIndex])
			{
				RibbonLayout ribbonLayout = new RibbonLayout();
				propertyValue.DeserializeObject(ribbonLayout);
				Add(ribbonLayout);
				layoutIndex++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonLayout loadedLayout = (RibbonLayout)enumerator.Current;
					if (loadedLayout._isCurrentLayout && ResolveRibbon() != null)
					{
						ResolveRibbon().CurrentLayout = loadedLayout;
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
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int layoutIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonLayout value = (RibbonLayout)enumerator.Current;
					info.AddValue("Layout" + layoutIndex, value);
					layoutIndex++;
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
		}

		internal RibbonLayoutCollection(MxRibbon ribbon)
		{
			_ribbon = ribbon;
		}

		internal RibbonLayoutCollection(RibbonLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal RibbonLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxRibbon ResolveRibbon()
		{
			if (_parentLayout == null)
			{
				return _ribbon;
			}
			return _parentLayout.FindRibbon();
		}

		public void Add(RibbonLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(RibbonLayout[] layouts)
		{
			foreach (RibbonLayout layout in layouts)
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
			if (value.GetType() != typeof(RibbonLayout))
			{
				throw new InvalidCastException();
			}
			RibbonLayout ribbonLayout = (RibbonLayout)value;
			if (ribbonLayout.GetOwningCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !ribbonLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			RibbonLayout ribbonLayout = (RibbonLayout)value;
			ribbonLayout.SetOwningCollection(this);
			ribbonLayout.EnsureIncrementalLayout();
		}

		public void Remove(RibbonLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxRibbon ribbon = ResolveRibbon();
			if (ribbon != null && ribbon.CurrentLayout == (RibbonLayout)value)
			{
				ribbon.CurrentLayout = null;
			}
			((RibbonLayout)value).SetOwningCollection(null);
		}

		protected override void OnClear()
		{
			MxRibbon ribbon = ResolveRibbon();
			if (ribbon != null && ribbon.CurrentLayout != null && Contains(ribbon.CurrentLayout))
			{
				ribbon.CurrentLayout = null;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonLayout ribbonLayout = (RibbonLayout)enumerator.Current;
					ribbonLayout.SetOwningCollection(null);
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
		}

		public int IndexOf(RibbonLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(RibbonLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonLayout ribbonLayout = (RibbonLayout)enumerator.Current;
					if (string.Compare(ribbonLayout.Key, layoutKey, ignoreCase: true, invariantCulture) == 0)
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

		internal RibbonLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonLayout ribbonLayout = (RibbonLayout)enumerator.Current;
					RibbonLayout currentLayout = ribbonLayout.FindCurrentLayout();
					if (currentLayout != null)
					{
						return currentLayout;
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
			return null;
		}
	}
}
