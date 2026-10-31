using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxColorPicker
	{
		private void UpdateBorderWidth()
		{
			switch (_borderStyle)
			{
			case Mordex.Windows.UI.BorderStyle.Flat:
			case Mordex.Windows.UI.BorderStyle.RaisedLight3D:
			case Mordex.Windows.UI.BorderStyle.SunkenLight3D:
				_borderWidth = FlatBorderWidth;
				break;
			case Mordex.Windows.UI.BorderStyle.Raised:
			case Mordex.Windows.UI.BorderStyle.Sunken:
				_borderWidth = ThickBorderWidth;
				break;
			default:
				_borderWidth = 0;
				break;
			}
		}

		internal void RecalculateLayout()
		{
			TextLayoutRenderer textLayoutRenderer = CreateTextLayoutRenderer();
			Graphics graphics = CreateGraphics();
			LayoutContent(graphics, textLayoutRenderer);
			textLayoutRenderer.ResetCache();
			graphics.Dispose();
		}

		private void LayoutContent(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			int layoutY = OuterPadding + _borderWidth;
			int layoutX = OuterPadding + _borderWidth;
			int buttonHeight = (int)Font.GetHeight(graphics) + ButtonHeightPadding;
			for (int i = 0; i < _colorEntries.Count; i++)
			{
				ColorEntry uiColorEntry = _colorEntries[i];
				if (uiColorEntry._isRemovable)
				{
					_colorEntries.Remove(uiColorEntry);
					i--;
				}
			}
			if (_showAutomaticButton)
			{
				if (_automaticButtonState == null)
				{
					_automaticButtonState = new FormatStyleLayoutState(_automaticButtonText, this);
				}
				else
				{
					_automaticButtonState._caption = _automaticButtonText;
				}
				_automaticButtonState._bounds = new Rectangle(layoutX, layoutY, base.ClientRectangle.Width - layoutX * 2, buttonHeight);
				layoutY += buttonHeight + Spacing;
			}
			else if (_automaticButtonState != null)
			{
				_automaticButtonState = null;
			}
			int cellWidth = _colorItemSize.Width + CellPadding;
			int cellHeight = _colorItemSize.Height + CellPadding;
			int columnIndex = 1;
			int contentWidth = 0;
			int rowCount = 1;
			foreach (ColorEntry item in _colorEntries)
			{
				if (columnIndex > Columns)
				{
					layoutY += cellHeight;
					contentWidth = layoutX;
					layoutX = OuterPadding + _borderWidth;
					columnIndex = 1;
					rowCount++;
				}
				item._bounds = new Rectangle(layoutX, layoutY, cellWidth, cellHeight);
				layoutX += cellWidth;
				columnIndex++;
			}
			int slotCount = rowCount * _columns;
			if (_colorEntries.Count > 0 && _colorEntries.Count < slotCount)
			{
				slotCount -= _colorEntries.Count;
				for (int j = 0; j < slotCount; j++)
				{
					ColorEntry padEntry = new ColorEntry(Color.White);
					padEntry._isRemovable = true;
					_colorEntries.Add(padEntry);
					padEntry._bounds = new Rectangle(layoutX, layoutY, cellWidth, cellHeight);
					layoutX += cellWidth;
				}
			}
			contentWidth = ((contentWidth != 0) ? (contentWidth + (_borderWidth + OuterPadding)) : (layoutX + (_borderWidth + OuterPadding) * 2));
			layoutY += cellHeight + Spacing;
			if (_showMoreColorsButton)
			{
				if (_moreColorsButtonState == null)
				{
					_moreColorsButtonState = new FormatStyleLayoutState(_moreColorsButtonText, this);
				}
				else
				{
					_moreColorsButtonState._caption = _moreColorsButtonText;
				}
				_moreColorsButtonState._bounds = new Rectangle(_borderWidth + OuterPadding, layoutY, contentWidth - (_borderWidth + OuterPadding) * 2, buttonHeight);
				layoutY += buttonHeight + Spacing;
			}
			else if (_moreColorsButtonState != null)
			{
				_moreColorsButtonState = null;
			}
			if (_automaticButtonState != null)
			{
				_automaticButtonState._bounds.Width = contentWidth - (_borderWidth + OuterPadding) * 2;
				int minButtonWidth = (int)textLayoutRenderer.MeasureText(graphics, _automaticButtonText, Font, 0).Width + CellPadding;
				minButtonWidth += cellWidth + CellPadding;
				if (minButtonWidth >= _automaticButtonState._bounds.Width)
				{
					_automaticButtonState._imageBounds = Rectangle.Empty;
				}
				else
				{
					int y = _automaticButtonState._bounds.Top + (_automaticButtonState._bounds.Height - _colorItemSize.Height) / 2;
					_automaticButtonState._imageBounds = new Rectangle(_automaticButtonState._bounds.Left + AutomaticSwatchLeft, y, _colorItemSize.Width, _colorItemSize.Height);
				}
			}
			base.Bounds = new Rectangle(base.Location, new Size(contentWidth, layoutY));
			_isLayoutDirty = false;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InvalidateLayout();
		}

		private void RefreshColorScheme()
		{
			if (ShouldSerializeBackColor())
			{
				ResetBackColor();
			}
			_windowsAppearanceState.ApplyVisualStyle();
			Invalidate();
		}

		public void LoadCustomPalette(ColorPalette customPalette)
		{
			if (customPalette == null)
			{
				throw new ArgumentNullException();
			}
			_colorEntries.Clear();
			Color[] entries = customPalette.Entries;
			foreach (Color color in entries)
			{
				ColorEntry colorEntry = new ColorEntry(color);
				_colorEntries.Add(colorEntry);
			}
		}

		public ColorEntry GetColorEntryAt(int x, int y)
		{
			foreach (ColorEntry item in _colorEntries)
			{
				if (item._bounds.Contains(x, y))
				{
					return item;
				}
			}
			return null;
		}

		public ColorEntry GetColorEntryAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetColorEntryAt(point.X, point.Y);
		}

		public ColorEntry GetColorEntryAt(Point pt)
		{
			return GetColorEntryAt(pt.X, pt.Y);
		}

		private void AddDefaultPalette()
		{
			Color black = Color.Black;
			ColorEntry colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.Brown;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(51, 51, 0);
			colorEntry = new ColorEntry(black, "Olive Green");
			_colorEntries.Add(colorEntry);
			black = Color.DarkGreen;
			colorEntry = new ColorEntry(black, "Dark Green");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(0, 51, 102);
			colorEntry = new ColorEntry(black, "Dark Teal");
			_colorEntries.Add(colorEntry);
			black = Color.DarkBlue;
			colorEntry = new ColorEntry(black, "Dark Blue");
			_colorEntries.Add(colorEntry);
			black = Color.Indigo;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(51, 51, 51);
			colorEntry = new ColorEntry(black, "Gray-80%");
			_colorEntries.Add(colorEntry);
			black = Color.DarkRed;
			colorEntry = new ColorEntry(black, "Dark Red");
			_colorEntries.Add(colorEntry);
			black = Color.Orange;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(128, 128, 0);
			colorEntry = new ColorEntry(black, "Dark Yellow");
			_colorEntries.Add(colorEntry);
			black = Color.Green;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.Teal;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.Blue;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(102, 102, 153);
			colorEntry = new ColorEntry(black, "Blue-Gray");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(128, 128, 128);
			colorEntry = new ColorEntry(black, "Gray-50%");
			_colorEntries.Add(colorEntry);
			black = Color.Red;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(255, 153, 0);
			colorEntry = new ColorEntry(black, "Light Orange");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(153, 204, 0);
			colorEntry = new ColorEntry(black, "Lime");
			_colorEntries.Add(colorEntry);
			black = Color.SeaGreen;
			colorEntry = new ColorEntry(black, "Sea Green");
			_colorEntries.Add(colorEntry);
			black = Color.Aqua;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.LightBlue;
			colorEntry = new ColorEntry(black, "Light Blue");
			_colorEntries.Add(colorEntry);
			black = Color.Violet;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(153, 153, 153);
			colorEntry = new ColorEntry(black, "Gray-40%");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(255, 0, 255);
			colorEntry = new ColorEntry(black, "Magenta");
			_colorEntries.Add(colorEntry);
			black = Color.Gold;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.Yellow;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(0, 255, 0);
			colorEntry = new ColorEntry(black, "Bright Green");
			_colorEntries.Add(colorEntry);
			black = Color.Turquoise;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.SkyBlue;
			colorEntry = new ColorEntry(black, "Sky Blue");
			_colorEntries.Add(colorEntry);
			black = Color.Plum;
			colorEntry = new ColorEntry(black);
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(192, 192, 192);
			colorEntry = new ColorEntry(black, "Gray-25%");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(255, 153, 204);
			colorEntry = new ColorEntry(black, "Rose");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(255, 204, 153);
			colorEntry = new ColorEntry(black, "Tan");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(255, 255, 153);
			colorEntry = new ColorEntry(black, "Light Yellow");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(204, 255, 204);
			colorEntry = new ColorEntry(black, "Light Green");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(204, 255, 255);
			colorEntry = new ColorEntry(black, "Light Turquoise");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(153, 204, 255);
			colorEntry = new ColorEntry(black, "Pale Blue");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(204, 153, 255);
			colorEntry = new ColorEntry(black, "Lavender");
			_colorEntries.Add(colorEntry);
			black = Color.FromArgb(255, 255, 255);
			colorEntry = new ColorEntry(black, "White");
			_colorEntries.Add(colorEntry);
		}
	}
}
