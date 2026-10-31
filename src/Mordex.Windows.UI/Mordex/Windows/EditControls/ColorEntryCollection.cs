using System;
using System.Collections;
using System.Drawing;
using Mordex.Windows.UI;

namespace Mordex.Windows.UI.Controls
{
	public class ColorEntryCollection : CollectionBase
	{
		private MxColorPicker _colorPicker;

		public ColorEntry this[int index] => (ColorEntry)base.List[index];

		public ColorEntry this[Color color]
		{
			get
			{
				foreach (ColorEntry item in base.List)
				{
					if (RenderingHelper.AreColorsEqual(item.Color, color))
					{
						return item;
					}
				}
				return null;
			}
		}

		internal ColorEntryCollection(MxColorPicker colorPicker)
		{
			_colorPicker = colorPicker;
		}

		internal void InvalidateColorPicker()
		{
			_colorPicker.Invalidate();
		}

		internal void InvalidateColorPickerLayout()
		{
			_colorPicker.InvalidateLayout();
		}

		public void Add(ColorEntry colorEntry)
		{
			if (colorEntry._owner != null)
			{
				throw new InvalidOperationException("The item you are trying to add, already belongs to a UIColorEntryCollection");
			}
			colorEntry._owner = this;
			base.List.Add(colorEntry);
			InvalidateColorPickerLayout();
		}

		public ColorEntry Add(Color color)
		{
			ColorEntry uiColorEntry = new ColorEntry(color);
			Add(uiColorEntry);
			return uiColorEntry;
		}

		public new void Clear()
		{
			foreach (ColorEntry item in base.List)
			{
				item._owner = null;
			}
			base.Clear();
			InvalidateColorPickerLayout();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			((ColorEntry)value)._owner = null;
			InvalidateColorPickerLayout();
		}

		public bool Contains(ColorEntry colorEntry)
		{
			return base.List.Contains(colorEntry);
		}

		public void Remove(ColorEntry colorEntry)
		{
			if (base.List.Contains(colorEntry))
			{
				base.List.Remove(colorEntry);
			}
		}
	}
}
