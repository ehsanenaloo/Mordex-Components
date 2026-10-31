using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal class TabPageRenderer : FormatStyleLayoutState
	{
		private const int SlantWidth = 5;

		private const float InactiveTabBlend = 0.3f;

		private MxTabPage _tabPage;

		public bool _isFirstInRow;

		public bool _isLaidOut;

		private bool _isSelectedExtended;

		private bool _isSlanted = true;

		public TabStripAlignment _tabAlignment = TabStripAlignment.Bottom;

		public int _rowIndex = -1;

		public override AccessibleRole AccessibleRole => AccessibleRole.PageTab;

		public override AccessibleStates AccessibleStates
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Focusable;
				accessibleStates = ((!base.IsEnabled()) ? (accessibleStates | AccessibleStates.Unavailable) : (accessibleStates | AccessibleStates.Selectable));
				if (IsPressed())
				{
					accessibleStates |= AccessibleStates.Selected;
				}
				return accessibleStates;
			}
		}

		public override string AccessibleDefaultActionDescription
		{
			get
			{
				if (GetTabPage() != null && GetTabPage().AccessibleDefaultActionDescription != null)
				{
					return GetTabPage().AccessibleDefaultActionDescription;
				}
				return "Switch";
			}
		}

		public override string AccessibleName
		{
			get
			{
				if (GetTabPage() != null)
				{
					if (GetTabPage().AccessibleName != null)
					{
						return GetTabPage().AccessibleName;
					}
					string text = GetTabPage().Text;
					text = text.Replace("&", "");
					if (text.Length == 0)
					{
						text = GetTabPage().Name;
					}
					return text;
				}
				return null;
			}
		}

		public TabPageRenderer()
		{
		}

		public TabPageRenderer(Control parentControl)
		{
			_parentControl = parentControl;
		}

		public TabPageRenderer(string text, MxTabPage tabPage, TabStripAlignment tabAlignment)
		{
			_caption = text;
			SetTabPage(tabPage);
			_tabAlignment = tabAlignment;
			_parentControl = tabPage.TabControl;
		}

		public MxTabPage GetTabPage()
		{
			return _tabPage;
		}

		public void SetTabPage(MxTabPage tabPage)
		{
			_tabPage = tabPage;
		}

		public bool HitTest(int x, int y, bool useSlantedShape)
		{
			if (useSlantedShape)
			{
				Rectangle rectangle = Rectangle.Empty;
				int slantWidth = 0;
				switch (_tabAlignment)
				{
				case TabStripAlignment.Top:
				case TabStripAlignment.Bottom:
				{
					slantWidth = _bounds.Height;
					if (!IsSlanted())
					{
						slantWidth = SlantWidth;
					}
					Rectangle tailBounds;
					if (!_isLaidOut || (_isFirstInRow && useSlantedShape))
					{
						tailBounds = new Rectangle(_bounds.X, _bounds.Y, slantWidth, _bounds.Height);
						rectangle = new Rectangle(_bounds.X + slantWidth, _bounds.Y, _bounds.Width - slantWidth, _bounds.Height);
					}
					else
					{
						tailBounds = new Rectangle(_bounds.X - slantWidth, _bounds.Y, slantWidth, _bounds.Height);
						rectangle = _bounds;
					}
					if (!IsSlanted())
					{
						if (tailBounds.Contains(x, y))
						{
							return true;
						}
					}
					else
					{
						if (x < tailBounds.Left || x > tailBounds.Right)
						{
							break;
						}
						if (_tabAlignment == TabStripAlignment.Top)
						{
							if (x - tailBounds.X + (y - _bounds.Y) >= tailBounds.Height)
							{
								return true;
							}
						}
						else if (x - tailBounds.X + (_bounds.Bottom - y) >= _bounds.Height)
						{
							return true;
						}
					}
					break;
				}
				case TabStripAlignment.Left:
				case TabStripAlignment.Right:
				{
					slantWidth = _bounds.Width;
					if (!IsSlanted())
					{
						slantWidth = SlantWidth;
					}
					Rectangle tailBounds;
					if (!_isLaidOut || (_isFirstInRow && useSlantedShape))
					{
						tailBounds = new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, slantWidth);
						rectangle = new Rectangle(_bounds.X, _bounds.Y + slantWidth, _bounds.Width, _bounds.Height - slantWidth);
					}
					else
					{
						tailBounds = new Rectangle(_bounds.X, _bounds.Y - slantWidth, _bounds.Width, slantWidth);
						rectangle = _bounds;
					}
					if (!IsSlanted())
					{
						if (tailBounds.Contains(x, y))
						{
							return true;
						}
					}
					else if (y >= tailBounds.Top && y <= tailBounds.Bottom && y - tailBounds.Top + (_bounds.Right - x) >= _bounds.Width)
					{
						return true;
					}
					break;
				}
				}
				if (rectangle.Contains(x, y))
				{
					return true;
				}
			}
			else if (_bounds.Contains(x, y))
			{
				return true;
			}
			return false;
		}

		public bool IsSelectedExtended()
		{
			return _isSelectedExtended;
		}

		public void SetSelectedExtended(bool isSelectedExtended)
		{
			_isSelectedExtended = isSelectedExtended;
		}

		public bool IsSlanted()
		{
			return _isSlanted;
		}

		public void SetSlanted(bool isSlanted)
		{
			_isSlanted = isSlanted;
		}

		public void DrawBackground(Graphics graphics, TabStripAlignment tabStripAlignment, Color borderColor, FormatStyle formatStyle, Color defaultBackColor, VisualStylePainter painter, bool isSelected, bool isHot)
		{
			if (_bounds.Width <= 0 || _bounds.Height <= 0)
			{
				return;
			}
			Color backColor = formatStyle.GetBlendedBackColor();
			Color backGradientColor = formatStyle.GetBlendedGradientColor();
			Color startColor;
			Color endColor;
			if (isHot && painter != null)
			{
				startColor = painter._hotColor;
				endColor = painter._hotGradientColor;
			}
			else
			{
				if (backColor.IsEmpty)
				{
					backColor = painter?._tabColor ?? defaultBackColor;
				}
				if (isHot)
				{
					startColor = backColor;
					endColor = ((!formatStyle.BackColorGradient.IsEmpty) ? formatStyle.BackColorGradient : startColor);
				}
				else
				{
					startColor = RenderingHelper.BlendColors(backColor, SystemColors.Window, InactiveTabBlend);
					endColor = ((formatStyle.BackgroundGradientMode == BackgroundGradientMode.Solid) ? startColor : ((!formatStyle.BackColorGradient.IsEmpty) ? formatStyle.GetBlendedGradientColor() : backColor));
				}
			}
			Brush brush = null;
			Pen pen = ((painter == null) ? new Pen(borderColor) : painter.GetBorderPen());
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			GraphicsPath graphicsPath = new GraphicsPath();
			Rectangle rectangle = _bounds;
			switch (tabStripAlignment)
			{
			case TabStripAlignment.Top:
			{
				int tabStartX = ((!IsSlanted() || (_isLaidOut && !_isSelectedExtended)) ? (rectangle.Left + SlantWidth) : (rectangle.Left + rectangle.Height));
				tabStartX--;
				if (isSelected)
				{
					rectangle.Height++;
				}
				graphicsPath.AddLine(tabStartX, rectangle.Top, rectangle.Right - 3, rectangle.Top);
				graphicsPath.AddLine(rectangle.Right - 3, rectangle.Top, rectangle.Right - 1, rectangle.Top + 2);
				graphicsPath.AddLine(rectangle.Right - 1, rectangle.Top + 2, rectangle.Right - 1, rectangle.Bottom);
				if (_isSelectedExtended || (isSelected && _isSlanted))
				{
					graphicsPath.AddLine(rectangle.Right - 1, rectangle.Bottom, tabStartX - rectangle.Height, rectangle.Bottom);
					graphicsPath.AddLine(tabStartX - rectangle.Height, rectangle.Bottom - 1, tabStartX - SlantWidth, rectangle.Top + 3);
					graphicsPath.AddLine(tabStartX - SlantWidth, rectangle.Top + 3, tabStartX, rectangle.Top);
				}
				else
				{
					graphicsPath.AddLine(rectangle.Right - 1, rectangle.Bottom, tabStartX - SlantWidth, rectangle.Bottom);
					graphicsPath.AddLine(tabStartX - SlantWidth, rectangle.Bottom, tabStartX - SlantWidth, rectangle.Top + 3);
					graphicsPath.AddLine(tabStartX - SlantWidth, rectangle.Top + 3, tabStartX, rectangle.Top);
				}
				brush = new LinearGradientBrush(graphicsPath.GetBounds(), startColor, endColor, LinearGradientMode.Vertical);
				break;
			}
			case TabStripAlignment.Bottom:
			{
				rectangle.Y--;
				int tabStartX = ((!IsSlanted() || (_isLaidOut && !_isSelectedExtended)) ? (rectangle.Left + SlantWidth) : (rectangle.Left + rectangle.Height));
				int bottomY = rectangle.Bottom - 1;
				tabStartX--;
				if (isSelected)
				{
					rectangle.Y--;
					rectangle.Height++;
				}
				graphicsPath.AddLine(tabStartX, bottomY, rectangle.Right - 3, bottomY);
				graphicsPath.AddLine(rectangle.Right - 3, bottomY, rectangle.Right - 1, bottomY - 2);
				graphicsPath.AddLine(rectangle.Right - 1, bottomY - 2, rectangle.Right - 1, rectangle.Top);
				if (_isSelectedExtended || (isSelected && _isSlanted))
				{
					graphicsPath.AddLine(rectangle.Right - 1, rectangle.Top, tabStartX - rectangle.Height, rectangle.Top);
					graphicsPath.AddLine(tabStartX - rectangle.Height, rectangle.Top + 1, tabStartX - SlantWidth, bottomY - 3);
					graphicsPath.AddLine(tabStartX - SlantWidth, bottomY - 3, tabStartX, bottomY);
				}
				else
				{
					graphicsPath.AddLine(rectangle.Right - 1, rectangle.Top, tabStartX - SlantWidth, rectangle.Top);
					graphicsPath.AddLine(tabStartX - SlantWidth, rectangle.Top, tabStartX - SlantWidth, bottomY - 3);
					graphicsPath.AddLine(tabStartX - SlantWidth, bottomY - 3, tabStartX, bottomY);
				}
				RectangleF bounds = graphicsPath.GetBounds();
				bounds.Y -= 1f;
				bounds.Height += 1f;
				brush = new LinearGradientBrush(bounds, startColor, endColor, -90f);
				break;
			}
			case TabStripAlignment.Left:
			{
				int tabStartY = ((!IsSlanted() || (_isLaidOut && !_isSelectedExtended)) ? (rectangle.Top + SlantWidth) : (rectangle.Top + rectangle.Width));
				tabStartY--;
				if (isSelected)
				{
					rectangle.Width++;
				}
				graphicsPath.AddLine(rectangle.Left, tabStartY, rectangle.Left, rectangle.Bottom - 3);
				graphicsPath.AddLine(rectangle.Left, rectangle.Bottom - 3, rectangle.Left + 2, rectangle.Bottom - 1);
				graphicsPath.AddLine(rectangle.Left + 2, rectangle.Bottom - 1, rectangle.Right, rectangle.Bottom - 1);
				if (_isSelectedExtended || (isSelected && _isSlanted))
				{
					graphicsPath.AddLine(rectangle.Right, rectangle.Bottom, rectangle.Right, tabStartY - rectangle.Width);
					graphicsPath.AddLine(rectangle.Right, tabStartY - rectangle.Width, rectangle.Left + 3, tabStartY - SlantWidth);
					graphicsPath.AddLine(rectangle.Left + 3, tabStartY - SlantWidth, rectangle.Left, tabStartY);
				}
				else
				{
					graphicsPath.AddLine(rectangle.Right, rectangle.Bottom, rectangle.Right, tabStartY - SlantWidth);
					graphicsPath.AddLine(rectangle.Right, tabStartY - SlantWidth, rectangle.Left + 3, tabStartY - SlantWidth);
					graphicsPath.AddLine(rectangle.Left + 3, tabStartY - SlantWidth, rectangle.Left, tabStartY);
				}
				brush = new LinearGradientBrush(graphicsPath.GetBounds(), endColor, startColor, 180f);
				break;
			}
			case TabStripAlignment.Right:
			{
				rectangle.X--;
				int rightX = rectangle.Right - 1;
				int tabStartY = ((!IsSlanted() || (_isLaidOut && !_isSelectedExtended)) ? (rectangle.Top + SlantWidth) : (rectangle.Top + rectangle.Width));
				tabStartY--;
				if (isSelected)
				{
					rectangle.X--;
					rectangle.Width++;
				}
				graphicsPath.AddLine(rightX, tabStartY, rightX, rectangle.Bottom - 3);
				graphicsPath.AddLine(rightX, rectangle.Bottom - 3, rightX - 2, rectangle.Bottom - 1);
				graphicsPath.AddLine(rightX - 2, rectangle.Bottom - 1, rectangle.Left, rectangle.Bottom - 1);
				if (_isSelectedExtended || (isSelected && _isSlanted))
				{
					graphicsPath.AddLine(rectangle.Left, rectangle.Bottom, rectangle.Left, tabStartY - rectangle.Width);
					graphicsPath.AddLine(rectangle.Left, tabStartY - rectangle.Width, rightX - 3, tabStartY - SlantWidth);
					graphicsPath.AddLine(rightX - 3, tabStartY - SlantWidth, rightX, tabStartY);
				}
				else
				{
					graphicsPath.AddLine(rectangle.Left, rectangle.Bottom, rectangle.Left, tabStartY - SlantWidth);
					graphicsPath.AddLine(rectangle.Left, tabStartY - SlantWidth, rightX - 3, tabStartY - SlantWidth);
					graphicsPath.AddLine(rightX - 3, tabStartY - SlantWidth, rightX, tabStartY);
				}
				RectangleF bounds = graphicsPath.GetBounds();
				bounds.X -= 1f;
				bounds.Width += 1f;
				brush = new LinearGradientBrush(bounds, startColor, endColor, 180f);
				break;
			}
			}
			graphics.FillPath(brush, graphicsPath);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawPath(pen, graphicsPath);
			graphics.SmoothingMode = smoothingMode;
			brush?.Dispose();
			graphicsPath.Dispose();
			if (painter == null)
			{
				pen.Dispose();
			}
		}
	}
}
