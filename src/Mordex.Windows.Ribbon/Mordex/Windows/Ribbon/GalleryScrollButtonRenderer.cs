using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryScrollButtonRenderer : RibbonLayoutItem
	{
		internal enum ScrollButtonType
		{
			ScrollUp,
			ScrollDown,
			Expand
		}

		private MxRibbon _ribbon;

		private ScrollButtonType _buttonType;

		private Gallery _gallery;

		public override string AccessibleName
		{
			get
			{
				switch (_buttonType)
				{
				case ScrollButtonType.Expand:
					if (_gallery != null)
					{
						string text = _gallery.Text;
						if (text == null || text.Length == 0)
						{
							text = _gallery.Name;
						}
						return text;
					}
					break;
				case ScrollButtonType.ScrollDown:
					return "Row down";
				case ScrollButtonType.ScrollUp:
					return "Row up";
				}
				return base.AccessibleName;
			}
		}

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (_buttonType == ScrollButtonType.Expand)
				{
					return AccessibleRole.ButtonDropDownGrid;
				}
				return AccessibleRole.PushButton;
			}
		}

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (Hot != value)
				{
					base.Hot = value;
					InvalidateBounds();
				}
			}
		}

		public override bool Pushed
		{
			get
			{
				return base.Pushed;
			}
			set
			{
				if (Pushed != value)
				{
					base.Pushed = value;
					InvalidateBounds();
				}
			}
		}

		public GalleryScrollButtonRenderer(MxRibbon ribbon, Gallery gallery, ScrollButtonType buttonType)
			: base(ribbon)
		{
			_ribbon = ribbon;
			_buttonType = buttonType;
			_gallery = gallery;
		}

		public ScrollButtonType GetButtonType()
		{
			return _buttonType;
		}

		private void InvalidateBounds()
		{
			if (_ribbon != null)
			{
				_ribbon.Invalidate(_bounds);
			}
		}

		private void DrawThemedButton(Graphics graphics, ResolvedTheme theme)
		{
			ThemedRibbonPainter.ScrollButton(graphics, theme, _bounds, Hot, Pushed, _isEnabled, _buttonType != ScrollButtonType.Expand);
			Rectangle glyph = Rectangle.Inflate(_bounds, -1, -1);
			switch (_buttonType)
			{
			case ScrollButtonType.Expand:
				ThemedRibbonPainter.CustomizeGlyph(graphics, theme, glyph, _isEnabled);
				break;
			case ScrollButtonType.ScrollDown:
				ThemedRibbonPainter.Arrow(graphics, theme, glyph, glyph.Width, 1, true, _isEnabled);
				break;
			default:
				ThemedRibbonPainter.Arrow(graphics, theme, glyph, glyph.Width, -1, true, _isEnabled);
				break;
			}
		}

		public void DrawButton(Graphics graphics, RibbonColorScheme colorScheme)
		{
			DrawThemedButton(graphics, colorScheme.Theme);
			return;
		}
	}
}
