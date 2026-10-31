using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class HeaderQuickAccessToolbarRenderer : QuickAccessToolbarRenderer
	{
		private RibbonHeaderRenderer _headerRenderer;

		private Rectangle _itemsBounds;

		public HeaderQuickAccessToolbarRenderer(MxRibbon ribbon, RibbonHeaderRenderer headerLayoutHandler)
			: base(ribbon)
		{
			_headerRenderer = headerLayoutHandler;
		}

		public override void Shrink(Graphics graphics)
		{
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				_customizeButton._isVisible = false;
			}
			if (_overflowButton == null)
			{
				_overflowButton = new QuickAccessOverflowButtonRenderer(_ribbon, this);
			}
			for (int index = _itemRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _itemRenderers[index];
				if (thisValue._isVisible)
				{
					thisValue._isVisible = false;
					break;
				}
			}
			int totalWidth = 0;
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item._isVisible)
				{
					totalWidth += item._fullItemWidth;
				}
			}
			_itemsBounds = new Rectangle(_rectangle.X, _rectangle.Y, totalWidth, _rectangle.Height);
			if (totalWidth > 0)
			{
				totalWidth += 14;
			}
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				totalWidth += _customizeButton._fullItemWidth;
			}
			if (_overflowButton != null)
			{
				_overflowButton.CalculateFullItemWidth(graphics, null, null);
				_overflowButton.ArrangeItem(graphics, null, _rectangle.X + totalWidth, _rectangle.Y, _overflowButton._fullItemWidth, _rectangle.Height);
				totalWidth += _overflowButton._fullItemWidth;
			}
			_rectangle.Width = totalWidth;
		}

		public override void ArrangeToolbar(Graphics graphics, int left, int top)
		{
			if (_itemsDirty)
			{
				RebuildItems();
			}
			if (!_ribbon.IsGlassCaptionActive())
			{
				if (top > 4)
				{
					top -= 4;
					left += 4;
				}
			}
			else
			{
				top -= 2;
			}
			int itemPadding = 5;
			if (_itemRenderers.Count > 0 || _ribbon.ShowCustomizeButton)
			{
				int totalWidth = 0;
				int toolbarHeight = 22;
				_overflowButton = null;
				using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					item.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
					item._isEnabled = !_ribbon.IsBackstageOpen() && item._isEnabled;
					totalWidth += item._fullItemWidth;
				}
				_itemsBounds = new Rectangle(left, top, totalWidth, toolbarHeight);
				if (totalWidth > 0 || _customizeButton != null)
				{
					totalWidth += 10;
				}
				if (_customizeButton != null)
				{
					_customizeButton.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
					_customizeButton._isEnabled = !_ribbon.IsBackstageOpen();
					totalWidth += _customizeButton._fullItemWidth;
				}
				_rectangle = new Rectangle(left, top, totalWidth, toolbarHeight);
				left += itemPadding;
				foreach (CommandBaseRenderer placedItem in _itemRenderers)
				{
					placedItem.ArrangeItem(graphics, null, left, top, placedItem._fullItemWidth, toolbarHeight);
					placedItem._isVisible = true;
					left += placedItem._bounds.Width;
				}
				left = ((_ribbon.GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton) ? (left + itemPadding) : (left + 14));
				if (_customizeButton != null)
				{
					_customizeButton.ArrangeItem(graphics, null, left, top, _customizeButton._fullItemWidth, toolbarHeight);
					_customizeButton._isVisible = true;
				}
				int count = _itemRenderers.Count;
				if (_customizeButton != null)
				{
					count++;
				}
			}
			else
			{
				_rectangle = Rectangle.Empty;
			}
		}

		internal override CommandBaseRenderer GetItemAt(Point pt)
		{
			if (_customizeButton != null && _customizeButton._isVisible && _customizeButton._bounds.Contains(pt))
			{
				return _customizeButton;
			}
			if (_overflowButton != null && _overflowButton._bounds.Contains(pt))
			{
				return _overflowButton;
			}
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._isVisible && item._bounds.Contains(pt))
					{
						return item;
					}
				}
			}
			return null;
		}

		private GraphicsPath CreateToolbarOutlinePath(Rectangle rectangle, int width)
		{
			Rectangle rect = new Rectangle(rectangle.X - width + 1, 0, width, width);
			Point point = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
			float startAngle = -90f;
			GraphicsPath graphicsPath = new GraphicsPath();
			Point bottomPoint;
			Point topPoint;
			if (rectangle.Height < width / 2)
			{
				double edgeAngle = Math.Asin((double)(rectangle.Bottom - point.Y) / ((double)width / 2.0));
				bottomPoint = new Point(0);
				bottomPoint.X = point.X + Convert.ToInt32(Math.Cos(edgeAngle) * (double)width / 2.0);
				bottomPoint.Y = point.Y + Convert.ToInt32(Math.Sin(edgeAngle) * (double)width / 2.0);
				edgeAngle = Math.Asin((double)(rectangle.Y - point.Y) / ((double)width / 2.0));
				if (double.IsNaN(edgeAngle))
				{
					edgeAngle = 0.0;
				}
				topPoint = new Point(0);
				topPoint.X = point.X + Convert.ToInt32(Math.Cos(edgeAngle) * (double)width / 2.0);
				topPoint.Y = point.Y + Convert.ToInt32(Math.Sin(edgeAngle) * (double)width / 2.0);
				startAngle = (float)(edgeAngle / (Math.PI * 2.0));
				startAngle *= 360f;
			}
			else
			{
				topPoint = new Point(point.X, rectangle.Y);
				bottomPoint = new Point(rectangle.X, rectangle.Bottom);
				graphicsPath.AddLine(point.X, rectangle.Y, point.X, rectangle.Bottom - width / 2);
			}
			graphicsPath.AddArc(rect, startAngle, 0f - startAngle);
			graphicsPath.AddLine(bottomPoint.X, bottomPoint.Y, rectangle.Right - 1, bottomPoint.Y);
			graphicsPath.AddArc(rectangle.Right - rectangle.Height / 2, rectangle.Top, rectangle.Height, rectangle.Height, 90f, -180f);
			graphicsPath.AddLine(rectangle.Right - 1, rectangle.Y, topPoint.X, rectangle.Y);
			return graphicsPath;
		}

		public override void OnPaint(Graphics graphics, RibbonColorScheme colorScheme, Rectangle clipRectangle)
		{
			if (_rectangle.Width <= 0 || _rectangle.Height <= 0)
			{
				return;
			}
			int currentX = _rectangle.X;
			DrawSeparator(graphics, colorScheme, currentX);
			currentX += 5;
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			foreach (CommandBaseRenderer item in _itemRenderers)
			{
				if (item._isVisible)
				{
					item.Paint(graphics, textRenderer, _ribbon._colorScheme, _ribbon.Font, _ribbon.ForeColorDark, clipRectangle);
					currentX += item._bounds.Width;
				}
			}
			DrawSeparator(graphics, colorScheme, currentX + 2);
			if (_customizeButton != null && _customizeButton._isVisible)
			{
				_customizeButton.Paint(graphics, null, _ribbon._colorScheme, null, Color.Empty, clipRectangle);
			}
			if (_overflowButton != null)
			{
				_overflowButton.Paint(graphics, null, _ribbon._colorScheme, null, Color.Empty, clipRectangle);
			}
		}

		private void DrawSeparator(Graphics graphics, RibbonColorScheme colorScheme, int separatorX)
		{
			graphics.DrawLine(colorScheme.Theme.Pen(ThemeRole.Border), separatorX + 1, _rectangle.Y + 5, separatorX + 1, _rectangle.Bottom - 5);
			return;
		}

		private void DrawBackground(Graphics graphics, RibbonColorScheme colorScheme)
		{
			bool isFormActive = _ribbon.IsFormActive();
			if (_itemsBounds.Width > 0 && _itemsBounds.Height > 0)
			{
				SmoothingMode smoothingMode = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				Rectangle rectangle = _itemsBounds;
				rectangle.Inflate(0, 1);
				rectangle.Height--;
				Pen pen = GdiCache.Pen(colorScheme._headerQuickAccessBorder);
				rectangle.X--;
				rectangle.Width++;
				using (GraphicsPath path = CreateToolbarOutlinePath(rectangle, _itemsBounds.X))
				{
					graphics.DrawPath(pen, path);
				}
				rectangle.X++;
				rectangle.Width--;
				rectangle = _itemsBounds;
				rectangle.Height--;
				using GraphicsPath graphicsPath = CreateToolbarOutlinePath(rectangle, _itemsBounds.X);
				Blend blend = new Blend();
				LinearGradientBrush linearGradientBrush;
				if (isFormActive)
				{
					linearGradientBrush = new LinearGradientBrush(_itemsBounds, colorScheme._headerQuickAccessActiveFillStart, colorScheme._headerQuickAccessActiveFillEnd, LinearGradientMode.Vertical);
					blend.Positions = new float[3] { 0f, 0.5f, 1f };
					blend.Factors = new float[3] { 0f, 1f, 0.75f };
					linearGradientBrush.Blend = blend;
				}
				else
				{
					linearGradientBrush = new LinearGradientBrush(_itemsBounds, colorScheme._headerQuickAccessInactiveFillStart, colorScheme._headerQuickAccessInactiveFillEnd, LinearGradientMode.Vertical);
					blend.Positions = new float[3] { 0f, 0.75f, 1f };
					blend.Factors = new float[3] { 0f, 1f, 0.25f };
					linearGradientBrush.Blend = blend;
				}
				graphics.FillPath(linearGradientBrush, graphicsPath);
				linearGradientBrush.Dispose();
				pen = GdiCache.Pen(Color.FromArgb(64, colorScheme._headerQuickAccessBorder));
				graphics.DrawLine(pen, rectangle.X - _itemsBounds.X / 7, rectangle.Y + 2, rectangle.Right + _itemsBounds.X / 8, rectangle.Y + 2);
				using Brush brush = new LinearGradientBrush(_itemsBounds, colorScheme._quickAccessOutlineStart, colorScheme._quickAccessOutlineEnd, LinearGradientMode.Vertical);
				using Pen outlinePen = new Pen(brush);
				graphics.DrawPath(outlinePen, graphicsPath);
				graphics.SmoothingMode = smoothingMode;
			}
		}

		internal override void Invalidate(bool update)
		{
			if (_headerRenderer != null && !_ribbon._isMergeInProgress)
			{
				_headerRenderer.RefreshHeader(update);
			}
		}

		internal override Point PointToScreen(CommandBaseRenderer item, Point clientPt)
		{
			if (item.GetRenderHost() is CommandBaseEditorDialog)
			{
				return item.GetRenderHost().PointToScreen(clientPt);
			}
			if (!_ribbon.IsCaptionBarShown())
			{
				clientPt.Y -= _headerRenderer.GetArea().Height;
				clientPt.X -= _headerRenderer.GetBorderSize().Width;
			}
			Point result = base.PointToScreen(item, clientPt);
			if (_ribbon.IsGlassCaptionActive())
			{
				result = new Point(result.X - _headerRenderer.GetBorderSize().Width, result.Y);
			}
			return result;
		}
	}
}
