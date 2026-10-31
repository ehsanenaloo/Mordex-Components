using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.UI.Controls
{
	public class ColorEntry
	{
		private Color _color = Color.Empty;

		private string _colorName;

		internal ColorEntryCollection _owner;

		// Marks a filler cell added by the layout. The next layout removes it.
		internal bool _isRemovable;

		internal Rectangle _bounds;

		internal bool _isPressed;

		internal bool _isHot;

		public Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				if (_color != value)
				{
					_color = value;
					InvalidateOwner();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		public string ColorName
		{
			get
			{
				if (_colorName == null)
				{
					if (_color.IsNamedColor)
					{
						return _color.Name;
					}
					return "";
				}
				return _colorName;
			}
			set
			{
				if (_colorName != value)
				{
					_colorName = value;
					InvalidateOwner();
				}
			}
		}

		public ColorEntry(Color color)
		{
			_color = color;
		}

		public ColorEntry(Color color, string colorName)
		{
			_color = color;
			_colorName = colorName;
		}

		private void InvalidateOwner()
		{
			if (_owner != null)
			{
				_owner.InvalidateColorPicker();
			}
		}
	}
}
