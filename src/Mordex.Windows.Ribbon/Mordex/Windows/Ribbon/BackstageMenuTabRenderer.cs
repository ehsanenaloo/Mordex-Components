using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageMenuTabRenderer : RibbonTabRenderer, IDropDownItem
	{
		private BackstageMenu _backstageMenu;

		public override AccessibleObject AccessibilityObject
		{
			get
			{
				if (_backstageMenu != null)
				{
					return _backstageMenu.GetAccessibleObject();
				}
				return base.AccessibilityObject;
			}
		}

		public BackstageMenuTabRenderer(BackstageMenu backStageMenu)
			: base(backStageMenu, backStageMenu.Ribbon)
		{
			_backstageMenu = backStageMenu;
			backStageMenu._owner = this;
		}

		private bool IsOrb()
		{
			return _commandBaseRendererRibbon != null && RibbonThemeStyle.For(_commandBaseRendererRibbon._colorScheme.Theme.Theme).AppButton == RibbonAppButtonKind.Orb;
		}

		public BackstageMenu GetBackstageMenu()
		{
			return _backstageMenu;
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_backstageMenu != null)
			{
				_backstageMenu._owner = null;
			}
			_backstageMenu = null;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRender, Font font)
		{
			if (IsOrb())
			{
				_hasImageContent = false;
				_fullItemWidth = (int)System.Math.Round((_commandBaseRendererRibbon.GetCaptionOrbBounds().IsEmpty ? 38 : 52) * _commandBaseRendererRibbon.DeviceDpi / 96f);
				_extraWidth = 0;
				SetLayoutWidth(_fullItemWidth);
				return;
			}
			_hasImageContent = RibbonDrawingUtils.HasImageSource(_backstageMenu.Icon, _backstageMenu.Image, GetRenderHost().GetImageList(GetCommand()), _backstageMenu.ImageIndex, _backstageMenu.ImageKey);
			_fullItemWidth = (int)textRender.MeasureStyledText(graphics, _backstageMenu.Text, _backstageMenu.ForegroundStyle, font, 0, 0).Width + 1;
			if (_hasImageContent)
			{
				_fullItemWidth += GetRenderHost().SmallImageSize.Width + 4;
			}
			_fullItemWidth += 30;
			_extraWidth = 0;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			int imageOffset = 0;
			if (_hasImageContent)
			{
				_imageBounds = new Rectangle(left, top + (itemHeight - GetRenderHost().SmallImageSize.Height) / 2, GetRenderHost().SmallImageSize.Width, GetRenderHost().SmallImageSize.Height);
				imageOffset += GetRenderHost().SmallImageSize.Width + 4;
			}
			else
			{
				_imageBounds = Rectangle.Empty;
			}
			int maxTextWidth = width + _extraWidth + 8;
			int textWidth = (int)textRenderer.MeasureStyledText(graphics, _backstageMenu.Text, _backstageMenu.ForegroundStyle, GetRenderHost().Font, 0, 0).Width + 1;
			if (textWidth > maxTextWidth)
			{
				_textBounds = new Rectangle(left + imageOffset, top, maxTextWidth, itemHeight);
			}
			else
			{
				_textBounds = new Rectangle(left + imageOffset, top, textWidth, itemHeight);
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
			AlignContentInBounds();
		}

		internal override GraphicsPath GetTabPath(Rectangle rect)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddLine(rect.X, rect.Bottom - 1, rect.X, rect.Y + 2);
			graphicsPath.AddArc(rect.X, rect.Y, 4, 4, -180f, 90f);
			graphicsPath.AddLine(rect.X + 3, rect.Y, rect.Right - 3, rect.Y);
			graphicsPath.AddArc(rect.Right - 5, rect.Y, 4, 4, -90f, 90f);
			graphicsPath.AddLine(rect.Right - 1, rect.Y + 2, rect.Right - 1, rect.Bottom - 1);
			return graphicsPath;
		}

		// The orb in the coordinates of the ribbon. With the caption extension it starts in the title bar, so only its lower part lies in the ribbon.
		internal Rectangle GetOrbBounds()
		{
			Rectangle lifted = _commandBaseRendererRibbon.GetCaptionOrbBounds();
			if (!lifted.IsEmpty)
			{
				return lifted;
			}
			int size = System.Math.Min(_bounds.Width, _bounds.Height + 6);
			return new Rectangle(_bounds.X + (_bounds.Width - size) / 2, _bounds.Y + (_bounds.Height - size) / 2 + 1, size, size);
		}

		internal void PaintOrb(Graphics graphics, ResolvedTheme theme, Rectangle orb, bool open, bool hot)
		{
			PaintOrb(graphics, _commandBaseRendererRibbon, theme, orb, open, hot);
		}

		// The frame can draw the lifted orb before the ribbon has made its tab renderers, so this needs only the ribbon.
		internal static void PaintOrb(Graphics graphics, MxRibbon ribbon, ResolvedTheme theme, Rectangle orb, bool open, bool hot)
		{
			ThemedRibbonPainter.AppOrb(graphics, theme, orb, open, hot || ribbon.IsCaptionOrbHot());
			if (ribbon.GetHostForm() != null)
			{
				Rectangle icon = Rectangle.Inflate(orb, -orb.Width / 5, -orb.Height / 5);
				RibbonDrawingUtils.DrawIcon(graphics, ribbon.ResolveControlBoxIcon(), icon, true, SystemColors.Window, false);
			}
		}

		public override bool Hot
		{
			get => base.Hot;
			set
			{
				if (base.Hot != value)
				{
					base.Hot = value;
					_commandBaseRendererRibbon.RefreshCaptionOrb();
				}
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			bool open = Selected || _commandBaseRendererRibbon.IsBackstageOpen();
			if (IsOrb())
			{
				PaintOrb(graphics, colorScheme.Theme, GetOrbBounds(), open, Hot);
				return;
			}
			ThemedRibbonPainter.FileTab(graphics, colorScheme.Theme, _bounds, open, Hot);
			Color textColor = RibbonThemeStyle.For(colorScheme.Theme.Theme).GetAppButtonTextColor(colorScheme.Theme.Theme);
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawImageFromSources(graphics, _backstageMenu.Icon, _backstageMenu.Image, _backstageMenu.ImageIndex, _backstageMenu.ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, true, false, colorScheme);
			}
			if (IsOnGlassCaption())
			{
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
				textLayoutRenderer.RenderContent(graphics, _backstageMenu.Text, _textBounds, _backstageMenu.ForegroundStyle, font, textColor, colorScheme._disabledTextColor, true);
			}
			else
			{
				textRenderer.RenderContent(graphics, _backstageMenu.Text, _textBounds, _backstageMenu.ForegroundStyle, font, textColor, colorScheme._disabledTextColor, true);
			}
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			if (_commandBaseRendererRibbon != null)
			{
				_commandBaseRendererRibbon.ToggleBackstageMenu(this);
			}
		}
	}
}
