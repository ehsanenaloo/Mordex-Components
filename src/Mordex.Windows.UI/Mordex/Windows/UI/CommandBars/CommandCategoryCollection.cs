using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.CommandBars
{
	public class CommandCategoryCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxCommandManager _commandManager;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "CommandCategory";

		public CommandCategory this[int index] => (CommandCategory)base.List[index];

		public CommandCategory this[string categoryName]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						CommandCategory uiCommandCategory = (CommandCategory)enumerator.Current;
						if (string.Compare(uiCommandCategory.CategoryName, categoryName, ignoreCase: true, invariantCulture) == 0)
						{
							return uiCommandCategory;
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

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			int categoryIndex = 0;
			PropertyValue propertyValue = reader["CommandCategory" + categoryIndex];
			ArrayList arrayList = new ArrayList();
			while (propertyValue != null)
			{
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				string text = (string)LayoutReader.GetPropertyValue(layoutReader, "CategoryName", typeof(string), "");
				CommandCategory uiCommandCategory = null;
				uiCommandCategory = ((!ContainsName(text)) ? new CommandCategory() : this[text]);
				propertyValue.DeserializeObject(uiCommandCategory);
				arrayList.Add(uiCommandCategory);
				categoryIndex++;
				propertyValue = reader["CommandCategory" + categoryIndex];
			}
			Clear();
			foreach (CommandCategory item in arrayList)
			{
				Add(item);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int categoryIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandCategory value = (CommandCategory)enumerator.Current;
					info.AddValue("CommandCategory" + categoryIndex, value);
					categoryIndex++;
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

		internal CommandCategoryCollection(MxCommandManager manager)
		{
			_commandManager = manager;
		}

		public void AddRange(CommandCategory[] categories)
		{
			foreach (CommandCategory category in categories)
			{
				Add(category);
			}
		}

		public void Add(CommandCategory category)
		{
			if (ContainsName(category.CategoryName))
			{
				throw new InvalidOperationException("The collection can not have two UICommandCategory object with the same CategoryName");
			}
			base.List.Add(category);
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				CommandCategory category = (CommandCategory)base.List[index];
				Remove(category);
				index--;
			}
		}

		public bool Contains(CommandCategory category)
		{
			return base.List.Contains(category);
		}

		internal bool ContainsName(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandCategory uiCommandCategory = (CommandCategory)enumerator.Current;
					if (string.Compare(uiCommandCategory.CategoryName, strB, ignoreCase: true, invariantCulture) == 0)
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

		public void Remove(CommandCategory category)
		{
			base.List.Remove(category);
		}
	}
}
