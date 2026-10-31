using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.CommandBars
{
	public class CommandBarsLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private CommandBarsLayout _parentLayout;

		internal MxCommandManager _commandManager;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public MxCommandManager CommandManager => _commandManager;

		public CommandBarsLayout this[int index] => (CommandBarsLayout)base.List[index];

		public CommandBarsLayout this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)enumerator.Current;
						if (string.Compare(uiCommandBarsLayout.Key, key, ignoreCase: true, invariantCulture) == 0)
						{
							return uiCommandBarsLayout;
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
				CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
				propertyValue.DeserializeObject(uiCommandBarsLayout);
				Add(uiCommandBarsLayout);
				layoutIndex++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandBarsLayout currentLayout = (CommandBarsLayout)enumerator.Current;
					if (currentLayout._isCurrentLayout && CommandManager != null)
					{
						CommandManager.CurrentLayout = currentLayout;
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
					CommandBarsLayout value = (CommandBarsLayout)enumerator.Current;
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

		internal CommandBarsLayoutCollection(MxCommandManager manager)
		{
			_commandManager = manager;
		}

		internal CommandBarsLayoutCollection(CommandBarsLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal CommandBarsLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxCommandManager FindCommandManager()
		{
			if (_parentLayout == null)
			{
				return _commandManager;
			}
			return _parentLayout.FindCommandManager();
		}

		public void Add(CommandBarsLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(CommandBarsLayout[] layouts)
		{
			foreach (CommandBarsLayout layout in layouts)
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
			if (value.GetType() != typeof(CommandBarsLayout))
			{
				throw new InvalidCastException();
			}
			CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)value;
			if (uiCommandBarsLayout.GetOwningCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !uiCommandBarsLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)value;
			uiCommandBarsLayout.SetOwningCollection(this);
			uiCommandBarsLayout.CreateIncrementalLayoutIfMissing();
		}

		public void Remove(CommandBarsLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxCommandManager uiCommandManager = FindCommandManager();
			if (uiCommandManager != null && uiCommandManager.CurrentLayout == (CommandBarsLayout)value)
			{
				uiCommandManager.CurrentLayout = null;
			}
			((CommandBarsLayout)value).SetOwningCollection(null);
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)enumerator.Current;
					uiCommandBarsLayout.SetOwningCollection(null);
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

		public int IndexOf(CommandBarsLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(CommandBarsLayout layout)
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
					CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)enumerator.Current;
					if (string.Compare(uiCommandBarsLayout.Key, layoutKey, ignoreCase: true, invariantCulture) == 0)
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

		internal CommandBarsLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandBarsLayout uiCommandBarsLayout = (CommandBarsLayout)enumerator.Current;
					CommandBarsLayout currentChild = uiCommandBarsLayout.FindCurrentLayout();
					if (currentChild != null)
					{
						return currentChild;
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
