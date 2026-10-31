using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class HeaderCellRendererBase : CellRendererBase
	{
		internal bool _gridRenderer16Boolean;

		private Pen _gridRenderer16Pen;

		internal HeaderCellRendererBase(IGridEnvironment owner, MxColorSchemeAppearanceState drawingStyle)
			: base(owner, drawingStyle)
		{
		}

		internal override void OnDispose()
		{
			if (_gridRenderer16Pen != null)
			{
				_gridRenderer16Pen.Dispose();
				_gridRenderer16Pen = null;
			}
		}

		internal Pen GetHeaderBorderPen()
		{
			if (_gridRenderer16Pen == null)
			{
				_gridRenderer16Pen = new Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderBorder));
			}
			return _gridRenderer16Pen;
		}

		internal override void PaintGridlines(Graphics graphics, GridGroupLayoutState item, Rectangle rect)
		{
		}

		// After its text a header paints only a one pixel ring at the edge of its bounds: the bottom and right border lines of
		// the flat look (the top line is above the bounds), and a ring of the themed look. Raised and sunken headers, and the
		// look with gradient borders, paint more and keep drawing at once.
		internal override bool TextMayBeDeferred(GridFormatStyle style)
		{
			if (_drawingStyle._isThemed)
			{
				return false;
			}
			return _drawingStyle.IsThemedLook || style.Appearance == Appearance.Flat || style.Appearance == Appearance.Empty;
		}

		internal override Rectangle GetTextFreeArea(GridGroupLayoutState item, Rectangle bounds, Rectangle gridlineFreeArea)
		{
			if (gridlineFreeArea.IsEmpty)
			{
				return gridlineFreeArea;
			}
			return _drawingStyle.IsThemedLook ? Rectangle.Inflate(bounds, -1, -1) : new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
		}
		internal override Brush GetBackBrush(Rectangle rect, GridGroupLayoutState item, GridFormatStyle style, bool selected, bool focused)
		{
			Color color = Color.Empty;
			Color gridColor = Color.Empty;
			if (_drawingStyle.IsThemedLook)
			{
				Blend blend = null;
				bool condition = _drawingStyle.GetResolvedAppearance().GetThemeColorName() == "SILVER";
				if (item._isPressed)
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElement);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElementGradient);
					condition = false;
				}
				else if (item._isHot)
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElement);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient);
					condition = false;
				}
				if (_drawingStyle._isThemed)
				{
					if (!item._isPressed && !item._isHot)
					{
						color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.Header);
						gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderGradient);
					}
					if (blend == null && MxThemeManager.DefaultTheme.Style.Modern.FaceKind == ThemeFaceKind.Glass)
					{
						blend = new Blend(4);
						blend.Positions = new float[4] { 0f, 0.49f, 0.5f, 1f };
						blend.Factors = new float[4] { 0f, 0.35f, 0.75f, 1f };
					}
					if (condition && blend == null)
					{
						blend = new Blend(4);
						blend.Positions = new float[4] { 0f, 0.35f, 0.5f, 1f };
						blend.Factors = new float[4] { 0f, 0.17f, 0.19f, 1f };
					}
				}
				else if (!item._isPressed && !item._isHot)
				{
					color = style.BackColor;
					gridColor = style.BackColorGradient;
					if (gridColor.IsEmpty)
					{
						gridColor = GridHelper.BlendColors(color, Color.Black, 0.5f);
					}
				}
				rect.Y--;
				rect.Height++;
				LinearGradientBrush otherLinearGradientBrush = new LinearGradientBrush(rect, color, gridColor, LinearGradientMode.Vertical);
				if (blend != null)
				{
					otherLinearGradientBrush.Blend = blend;
				}
				return otherLinearGradientBrush;
			}
			if (_drawingStyle._isThemed)
			{
				if (item._isPressed)
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElement);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.PressedElementGradient);
				}
				else
				{
					color = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.Header);
					gridColor = _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderGradient);
				}
				return new LinearGradientBrush(Rectangle.Inflate(rect, 0, 1), color, gridColor, 90f);
			}
			return base.GetBackBrush(rect, item, style, selected, focused);
		}

		internal override void PaintBackgroundImage(Graphics graphics, Rectangle rect, GridFormatStyle style)
		{
			if (!_drawingStyle._isThemed && !_drawingStyle.IsThemedLook)
			{
				base.PaintBackgroundImage(graphics, rect, style);
			}
		}

		internal override Rectangle GetForegroundRect(Rectangle rect, GridGroupLayoutState item, GridFormatStyle style)
		{
			Rectangle rectangle;
			if (_drawingStyle.IsThemedLook)
			{
				rectangle = Rectangle.Inflate(rect, -1, -1);
			}
			else
			{
				if (!_drawingStyle._isThemed)
				{
					return base.GetForegroundRect(rect, item, style);
				}
				rectangle = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
			}
			return item.CalculateContentRect(rectangle, item.IsRightToLeft(_owner.RightToLeft));
		}

		internal override void PaintForeground(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect, bool selected, bool focused)
		{
			base.PaintForeground(graphics, item, style, rect, selected, focused);
		}

		internal override void PaintAddornments(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle rect)
		{
			if (_drawingStyle.IsThemedLook)
			{
				ResolvedTheme look = MxThemeManager.ResolveDefault(System.Math.Max(96, (int)graphics.DpiX));
				if (look.Style.HeaderBorder == ThemeBorder.Raised && rect.Width > 3 && rect.Height > 3)
				{
					look.DrawBevel(graphics, rect, item._isPressed);
					return;
				}
				if (item._isPressed || item._isHot)
				{
					Pen pen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementBorder));
					if (item.HasTopBorder())
					{
						rect.Y--;
						rect.Height++;
					}
					GridHelper.DrawFlatBorder(graphics, pen, rect);
					pen = null;
					return;
				}
				Pen otherPen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderBorder));
				if ((item._gridGroupLayoutStateBorderFlags & BorderFlags.ConnectorRight) == BorderFlags.ConnectorRight)
				{
					graphics.DrawLine(otherPen, rect.Right - 1, rect.Y + 3, rect.Right - 1, rect.Bottom - 5);
				}
				if ((item._gridGroupLayoutStateBorderFlags & BorderFlags.ConnectorBottom) == BorderFlags.ConnectorBottom)
				{
					graphics.DrawLine(otherPen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				}
				if ((item._gridGroupLayoutStateBorderFlags & BorderFlags.Left) == BorderFlags.Left)
				{
					graphics.DrawLine(otherPen, rect.X, rect.Y, rect.X, rect.Bottom - 1);
				}
				if ((item._gridGroupLayoutStateBorderFlags & BorderFlags.Top) == BorderFlags.Top)
				{
					graphics.DrawLine(otherPen, rect.X, rect.Y, rect.Right - 1, rect.Y);
				}
				if ((item._gridGroupLayoutStateBorderFlags & BorderFlags.Right) == BorderFlags.Right)
				{
					graphics.DrawLine(otherPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
				}
				if ((item._gridGroupLayoutStateBorderFlags & BorderFlags.Bottom) == BorderFlags.Bottom)
				{
					graphics.DrawLine(otherPen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				}
				if (item.HasTopBorder())
				{
					graphics.DrawLine(otherPen, rect.X, rect.Y - 1, rect.Right - 1, rect.Y - 1);
				}
			}
			else if (_drawingStyle._isThemed)
			{
				if (item._isPressed)
				{
					graphics.DrawLine(GetHeaderBorderPen(), rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
					graphics.DrawLine(GetHeaderBorderPen(), rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
					rect.Inflate(-2, -2);
					return;
				}
				if (item._isHot)
				{
					graphics.DrawLine(GetHeaderBorderPen(), rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
					graphics.DrawLine(GetHeaderBorderPen(), rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
					rect.Width--;
					rect.Height--;
					LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rect, 1, 1), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElement), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient), 90f);
					Pen nextPen = new Pen(linearGradientBrush);
					GridHelper.DrawFlatBorder(graphics, nextPen, rect);
					nextPen.Dispose();
					linearGradientBrush.Dispose();
					rect.Inflate(-1, -1);
					linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rect, 1, 1), GridHelper.BlendColors(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElement), _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient), 0.5f), GridHelper.BlendColors(Color.White, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementGradient), 0f), 90f);
					nextPen = new Pen(linearGradientBrush);
					GridHelper.DrawFlatBorder(graphics, nextPen, rect);
					nextPen.Dispose();
					linearGradientBrush.Dispose();
					return;
				}
				graphics.DrawLine(SystemPens.ControlLightLight, rect.Left, rect.Bottom - 1, rect.Left, rect.Top);
				graphics.DrawLine(SystemPens.ControlLightLight, rect.Left, rect.Top, rect.Right - 1, rect.Top);
				Pen previousPen = GdiCache.Pen(_drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderBorder));
				graphics.DrawLine(previousPen, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine(previousPen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				rect.Inflate(-1, -1);
				previousPen = GdiCache.Pen(GridHelper.BlendColors(SystemColors.ControlDark, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderGradient), 0.35f));
				graphics.DrawLine(previousPen, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine(previousPen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				if (item.HasTopBorder())
				{
					graphics.DrawLine(previousPen, rect.Left - 1, rect.Y - 1, rect.Right - 1, rect.Y - 1);
				}
				rect.Width--;
				rect.Height--;
				previousPen = GdiCache.Pen(GridHelper.BlendColors(SystemColors.ControlDark, _drawingStyle.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderGradient), 0.1f));
				graphics.DrawLine(previousPen, rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine(previousPen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
				rect.Width--;
				rect.Height--;
			}
			else if (_gridRenderer16Boolean && style.Appearance == Appearance.Flat)
			{
				Pen flatBorderPen = null;
				if (_owner is ControlStyle)
				{
					flatBorderPen = ((ControlStyle)_owner).GetFlatBorderPen();
				}
				else
				{
					flatBorderPen = GdiCache.Pen(GridHelper.BlendColors(style.GetEffectiveBackColor(), Color.Black, 0.5f));
				}
				if (item.HasTopBorder())
				{
					graphics.DrawLine(flatBorderPen, rect.X, rect.Y - 1, rect.Right - 1, rect.Y - 1);
				}
				graphics.DrawLine(flatBorderPen, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
				graphics.DrawLine(flatBorderPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
			}
			else
			{
				base.PaintAddornments(graphics, item, style, rect);
			}
		}
	}
}
