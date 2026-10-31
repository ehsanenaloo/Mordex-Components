using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class ControlStyle
	{
		internal readonly GridTextBatch _textBatch = new GridTextBatch();

		GridTextBatch IGridEnvironment.PaintTextBatch => _textBatch;

		internal Pen GetFlatBorderPen()
		{
			if (_flatBorderPen == null)
			{
				_flatBorderPen = new Pen(_flatBorderColor);
			}
			return _flatBorderPen;
		}

		internal Brush GetControlBrush()
		{
			if (_controlBrush == null)
			{
				_controlBrush = new SolidBrush(ControlColor);
			}
			return _controlBrush;
		}

		internal Brush GetControlTextBrush()
		{
			if (_controlStyleBrush == null)
			{
				_controlStyleBrush = new SolidBrush(ControlTextColor);
			}
			return _controlStyleBrush;
		}

		internal Pen GetControlTextPen()
		{
			if (_controlStylePen == null)
			{
				_controlStylePen = new Pen(ControlTextColor);
			}
			return _controlStylePen;
		}

		internal Brush GetScrollBarBrush()
		{
			if (_scrollBarBrush == null)
			{
				if (ScrollBarColor.ToArgb() == ControlColor.ToArgb() || ScrollBarColor.Equals(SystemColors.ScrollBar))
				{
					Bitmap bitmap = new Bitmap(8, 8);
					for (int i = 0; i < bitmap.Width; i += 2)
					{
						for (int j = 0; j < bitmap.Height; j++)
						{
							try
							{
								if (j % 2 == 0)
								{
									bitmap.SetPixel(i, j, ControlColor);
								}
								else
								{
									bitmap.SetPixel(i, j, _controlLightLightColor);
								}
							}
							catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
							{
								GridExceptions.Report(caught);
							}
						}
					}
					for (int i = 1; i < bitmap.Width; i += 2)
					{
						for (int j = 0; j < bitmap.Height; j++)
						{
							try
							{
								if (j % 2 == 0)
								{
									bitmap.SetPixel(i, j, _controlLightLightColor);
								}
								else
								{
									bitmap.SetPixel(i, j, ControlColor);
								}
							}
							catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
							{
								GridExceptions.Report(caught);
							}
						}
					}
					_scrollBarBrush = new TextureBrush(bitmap);
					bitmap.Dispose();
					bitmap = null;
				}
				else
				{
					_scrollBarBrush = new SolidBrush(ScrollBarColor);
				}
			}
			return _scrollBarBrush;
		}

		private void ResetScrollBarBrush()
		{
			DisposeScrollBarBrush();
			NotifyChanged();
		}

		internal void DisposeScrollBarBrush()
		{
			if (_scrollBarBrush != null)
			{
				_scrollBarBrush.Dispose();
				_scrollBarBrush = null;
			}
		}

		internal Pen GetControlLightPen()
		{
			if (_controlLightPen == null)
			{
				_controlLightPen = new Pen(_controlLightColor);
			}
			return _controlLightPen;
		}

		private void DisposeControlLightPen()
		{
			if (_controlLightPen != null)
			{
				_controlLightPen.Dispose();
				_controlLightPen = null;
			}
			NotifyChanged();
		}

		internal Pen GetControlLightLightPen()
		{
			if (_controlLightLightPen == null)
			{
				_controlLightLightPen = new Pen(_controlLightLightColor);
			}
			return _controlLightLightPen;
		}

		internal Brush GetControlLightLightBrush()
		{
			if (_controlLightLightBrush == null)
			{
				_controlLightLightBrush = new SolidBrush(_controlLightLightColor);
			}
			return _controlLightLightBrush;
		}

		internal Pen GetControlDarkPen()
		{
			if (_controlDarkPen == null)
			{
				_controlDarkPen = new Pen(ResolveControlDarkColor());
			}
			return _controlDarkPen;
		}

		internal Brush GetControlDarkBrush()
		{
			if (_controlDarkBrush == null)
			{
				_controlDarkBrush = new SolidBrush(ResolveControlDarkColor());
			}
			return _controlDarkBrush;
		}

		private void DisposeControlDarkPen()
		{
			if (_controlDarkPen != null)
			{
				_controlDarkPen.Dispose();
				_controlDarkPen = null;
			}
			NotifyChanged();
		}

		internal Pen GetControlDarkDarkPen()
		{
			if (_controlDarkDarkPen == null)
			{
				_controlDarkDarkPen = new Pen(_controlDarkDarkColor);
			}
			return _controlDarkDarkPen;
		}

		private void DisposeControlDarkDarkPen()
		{
			if (_controlDarkDarkPen != null)
			{
				_controlDarkDarkPen.Dispose();
				_controlDarkDarkPen = null;
			}
			ResetScrollBarBrush();
			NotifyChanged();
		}

		internal Brush GetWindowBrush()
		{
			if (_windowBrush == null)
			{
				_windowBrush = new SolidBrush(WindowColor);
			}
			return _windowBrush;
		}

		private void DisposeWindowBrush()
		{
			if (_windowBrush != null)
			{
				_windowBrush.Dispose();
				_windowBrush = null;
			}
			NotifyChanged();
		}

		internal Brush GetWindowTextBrush()
		{
			if (_windowTextBrush == null)
			{
				_windowTextBrush = new SolidBrush(WindowTextColor);
			}
			return _windowTextBrush;
		}

		internal Pen GetWindowTextPen()
		{
			if (_windowTextPen == null)
			{
				_windowTextPen = new Pen(WindowTextColor);
			}
			return _windowTextPen;
		}

		internal Brush GetHoverBrush()
		{
			if (_hoverBrush == null)
			{
				_hoverBrush = new SolidBrush(BlendColors(_hoverBlendColor, _hoverBaseColor, _hoverBlend));
			}
			return _hoverBrush;
		}

		internal Brush GetHoverSelectedBrush()
		{
			if (_hoverSelectedBrush == null)
			{
				_hoverSelectedBrush = new SolidBrush(BlendColors(_hoverBlendColor, _hoverBaseColor, _hoverSelectedBlend));
			}
			return _hoverSelectedBrush;
		}

		internal Pen GetHoverPen()
		{
			if (_hoverPen == null)
			{
				_hoverPen = new Pen(_hoverBaseColor);
			}
			return _hoverPen;
		}

		internal void DrawBackground(Graphics graphics, Rectangle rectangle, bool condition, bool otherCondition)
		{
			Brush brush = GetControlBrush();
			GetControlTextPen();
			if (otherCondition)
			{
				switch (_buttonAppearance)
				{
				case ButtonAppearance.Flat:
				case ButtonAppearance.FlatBorderless:
					brush = GetHoverSelectedBrush();
					rectangle = GridHelper.DrawFlatBorder(graphics, GetHoverPen(), rectangle);
					break;
				case ButtonAppearance.Regular:
					rectangle = GridHelper.DrawFlatBorder(graphics, GetControlDarkPen(), rectangle);
					break;
				case ButtonAppearance.Light3D:
				case ButtonAppearance.PopUp:
					rectangle = GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (_buttonAppearance)
				{
				case ButtonAppearance.Regular:
					rectangle = GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.Raised);
					break;
				case ButtonAppearance.Light3D:
					rectangle = GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.RaisedLight3D);
					break;
				case ButtonAppearance.FlatBorderless:
					if (condition)
					{
						rectangle = GridHelper.DrawFlatBorder(graphics, GetHoverPen(), rectangle);
						brush = GetHoverBrush();
					}
					break;
				case ButtonAppearance.Flat:
					if (condition)
					{
						rectangle = GridHelper.DrawFlatBorder(graphics, GetHoverPen(), rectangle);
						brush = GetHoverBrush();
					}
					else
					{
						rectangle = ((_printDocument == null) ? GridHelper.DrawFlatBorder(graphics, GetControlDarkPen(), rectangle) : GridHelper.DrawFlatBorder(graphics, GetFlatBorderPen(), rectangle));
					}
					break;
				case ButtonAppearance.PopUp:
					rectangle = ((!condition) ? ((_printDocument == null) ? GridHelper.DrawFlatBorder(graphics, GetControlDarkPen(), rectangle) : GridHelper.DrawFlatBorder(graphics, GetFlatBorderPen(), rectangle)) : GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.RaisedLight3D));
					break;
				}
			}
			graphics.FillRectangle(brush, rectangle);
		}

		internal void FillButtonBackground(Graphics graphics, Rectangle bounds, bool condition, bool otherCondition)
		{
			Brush brush = GetControlBrush();
			if (_buttonAppearance == ButtonAppearance.Flat || _buttonAppearance == ButtonAppearance.FlatBorderless)
			{
				if (otherCondition)
				{
					brush = GetHoverSelectedBrush();
				}
				else if (condition)
				{
					brush = GetHoverBrush();
				}
			}
			graphics.FillRectangle(brush, bounds);
		}

		internal void DrawButtonBorder(Graphics graphics, Rectangle rectangle, bool condition, bool otherCondition)
		{
			if (otherCondition)
			{
				switch (_buttonAppearance)
				{
				case ButtonAppearance.Flat:
				case ButtonAppearance.FlatBorderless:
					GridHelper.DrawFlatBorder(graphics, GetHoverPen(), rectangle);
					break;
				case ButtonAppearance.Regular:
					GridHelper.DrawFlatBorder(graphics, GetControlDarkPen(), rectangle);
					break;
				case ButtonAppearance.Light3D:
				case ButtonAppearance.PopUp:
					GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.SunkenLight3D);
					break;
				}
				return;
			}
			switch (_buttonAppearance)
			{
			case ButtonAppearance.Regular:
				GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.Raised);
				break;
			case ButtonAppearance.Light3D:
				GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.RaisedLight3D);
				break;
			case ButtonAppearance.FlatBorderless:
				if (condition)
				{
					GridHelper.DrawFlatBorder(graphics, GetHoverPen(), rectangle);
				}
				break;
			case ButtonAppearance.Flat:
				if (condition)
				{
					GridHelper.DrawFlatBorder(graphics, GetHoverPen(), rectangle);
				}
				else if (_printDocument != null)
				{
					GridHelper.DrawFlatBorder(graphics, GetFlatBorderPen(), rectangle);
				}
				else
				{
					GridHelper.DrawFlatBorder(graphics, GetControlDarkPen(), rectangle);
				}
				break;
			case ButtonAppearance.PopUp:
				if (condition)
				{
					GridHelper.DrawBorderForStyle(graphics, this, rectangle, BorderStyle.RaisedLight3D);
				}
				else if (_printDocument != null)
				{
					GridHelper.DrawFlatBorder(graphics, GetFlatBorderPen(), rectangle);
				}
				else
				{
					GridHelper.DrawFlatBorder(graphics, GetControlDarkPen(), rectangle);
				}
				break;
			}
		}

		internal void DrawVerticalArrow(Graphics graphics, Rectangle rectangle, int value, bool condition, bool otherCondition)
		{
			if (condition && !IsFlatAppearance())
			{
				rectangle.Offset(1, 1);
			}
			if (!otherCondition)
			{
				if (IsFlatAppearance())
				{
					DrawVerticalTriangle(graphics, rectangle.Width, rectangle, value, GetControlDarkPen());
					return;
				}
				rectangle.Offset(1, 1);
				DrawVerticalTriangle(graphics, rectangle.Width, rectangle, value, GetControlLightLightPen());
				rectangle.Offset(-1, -1);
				DrawVerticalTriangle(graphics, rectangle.Width, rectangle, value, GetControlDarkPen());
			}
			else
			{
				DrawVerticalTriangle(graphics, rectangle.Width, rectangle, value, GetControlTextPen());
			}
		}

		internal void DrawHorizontalArrow(Graphics graphics, Rectangle rectangle, int value, bool condition, bool otherCondition, bool nextCondition)
		{
			if (otherCondition && !IsFlatAppearance())
			{
				rectangle.Offset(1, 1);
			}
			Pen pen;
			if (!nextCondition)
			{
				pen = GetControlDarkPen();
				if (IsFlatAppearance())
				{
					DrawHorizontalTriangle(graphics, rectangle.Height, rectangle, value, pen);
				}
				else
				{
					rectangle.Offset(1, 1);
					DrawHorizontalTriangle(graphics, rectangle.Height, rectangle, value, GetControlLightLightPen());
					rectangle.Offset(-1, -1);
					DrawHorizontalTriangle(graphics, rectangle.Height, rectangle, value, pen);
				}
			}
			else
			{
				pen = GetControlTextPen();
				DrawHorizontalTriangle(graphics, rectangle.Height, rectangle, value, pen);
			}
		}

		internal void DrawArrowWithAsterisk(Graphics graphics, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			int value = bounds.Height / 2;
			if (value % 2 == 0)
			{
				value--;
			}
			value -= 2;
			int width = value / 2 + 1;
			int otherValue = bounds.Top + (bounds.Height - value) / 2 - 1;
			if (otherCondition && !IsFlatAppearance())
			{
				bounds.Offset(1, 1);
			}
			Rectangle rectangle = new Rectangle(bounds.Left + (bounds.Width - (value + width + 1)) / 2, bounds.Top, width, bounds.Height);
			if (!nextCondition)
			{
				if (IsFlatAppearance())
				{
					Pen pen = GetControlDarkPen();
					DrawHorizontalTriangle(graphics, bounds.Height, rectangle, 1, pen);
					RenderContent(graphics, rectangle.Right + 1, otherValue, value, pen);
				}
				else
				{
					rectangle.Offset(1, 1);
					DrawHorizontalTriangle(graphics, bounds.Height, rectangle, 1, GetControlLightLightPen());
					RenderContent(graphics, rectangle.Right + 1, otherValue + 1, value, GetControlLightLightPen());
					rectangle.Offset(-1, -1);
					DrawHorizontalTriangle(graphics, bounds.Height, rectangle, 1, GetControlDarkPen());
					RenderContent(graphics, rectangle.Right + 1, otherValue, value, GetControlDarkPen());
				}
			}
			else
			{
				Pen pen = GetControlTextPen();
				DrawHorizontalTriangle(graphics, bounds.Height, rectangle, 1, pen);
				RenderContent(graphics, rectangle.Right + 1, otherValue, value, pen);
			}
		}

		internal void DrawGrip(Graphics graphics, Rectangle rectangle, bool condition, bool otherCondition)
		{
			if (condition && !IsFlatAppearance())
			{
				rectangle.Offset(1, 1);
			}
			if (!otherCondition)
			{
				if (IsFlatAppearance())
				{
					FillGripDots(graphics, rectangle, GetControlDarkBrush());
					return;
				}
				rectangle.Offset(1, 1);
				FillGripDots(graphics, rectangle, GetControlLightLightBrush());
				rectangle.Offset(-1, -1);
				FillGripDots(graphics, rectangle, GetControlDarkBrush());
			}
			else
			{
				FillGripDots(graphics, rectangle, GetControlTextBrush());
			}
		}

		internal static void FillGripDots(Graphics graphics, Rectangle rectangle, Brush brush)
		{
			rectangle = new Rectangle(rectangle.Right - 5, rectangle.Bottom - 5, 2, 2);
			graphics.FillRectangle(brush, rectangle);
			rectangle.X -= 3;
			graphics.FillRectangle(brush, rectangle);
			rectangle.X -= 3;
			graphics.FillRectangle(brush, rectangle);
		}

		internal void DrawArrowWithBar(Graphics graphics, Rectangle bounds, int value, bool condition, bool otherCondition, bool nextCondition)
		{
			int otherValue = bounds.Height / 2;
			if (otherValue % 2 == 0)
			{
				otherValue--;
			}
			int width = otherValue / 2 + 1;
			int nextValue = bounds.Top + (bounds.Height - otherValue) / 2;
			int previousValue = width / 2;
			int firstValue = previousValue * 2 + width;
			int secondValue = ((value != -1) ? (bounds.Right - (bounds.Width - firstValue) / 2 - previousValue) : (bounds.Left + (bounds.Width - firstValue) / 2));
			if (otherCondition && !IsFlatAppearance())
			{
				bounds.Offset(1, 1);
			}
			Rectangle rectangle = ((value != -1) ? new Rectangle(secondValue - previousValue - width, bounds.Top, width, bounds.Height) : new Rectangle(secondValue + previousValue * 2, bounds.Top, width, bounds.Height));
			if (!nextCondition)
			{
				if (IsFlatAppearance())
				{
					Pen pen = GetControlDarkPen();
					DrawHorizontalTriangle(graphics, bounds.Height, rectangle, value, pen);
					DrawVerticalLines(graphics, secondValue, nextValue, otherValue, previousValue, pen);
				}
				else
				{
					rectangle.Offset(1, 1);
					DrawHorizontalTriangle(graphics, bounds.Height, rectangle, value, GetControlLightLightPen());
					DrawVerticalLines(graphics, secondValue + 1, nextValue + 1, otherValue, previousValue, GetControlLightLightPen());
					rectangle.Offset(-1, -1);
					DrawHorizontalTriangle(graphics, bounds.Height, rectangle, value, GetControlDarkPen());
					DrawVerticalLines(graphics, secondValue, nextValue, otherValue, previousValue, GetControlDarkPen());
				}
			}
			else
			{
				Pen pen = GetControlTextPen();
				DrawHorizontalTriangle(graphics, bounds.Height, rectangle, value, pen);
				DrawVerticalLines(graphics, secondValue, nextValue, otherValue, previousValue, pen);
			}
		}

		private static void DrawVerticalLines(Graphics graphics, int value, int y1, int otherValue, int nextValue, Pen pen)
		{
			for (int i = 0; i < nextValue; i++)
			{
				graphics.DrawLine(pen, value + i, y1, value + i, y1 + otherValue - 1);
			}
		}

		private static void RenderContent(Graphics graphics, int value, int otherValue, int nextValue, Pen pen)
		{
			Rectangle rectangle = new Rectangle(value + 1, otherValue + 1, nextValue - 1, nextValue - 1);
			graphics.DrawLine(pen, rectangle.Left + 1, rectangle.Top + 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Left + 1, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Top + 1);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top + rectangle.Height / 2, rectangle.Right, rectangle.Top + rectangle.Height / 2);
			graphics.DrawLine(pen, rectangle.Left + rectangle.Width / 2, rectangle.Top, rectangle.Left + rectangle.Width / 2, rectangle.Bottom);
		}

		internal static void DrawVerticalTriangle(Graphics graphics, int value, Rectangle rectangle, int otherValue, Pen pen)
		{
			int nextValue = value / 2;
			if (nextValue % 2 == 0)
			{
				nextValue--;
			}
			int previousValue = nextValue / 2 + 1;
			int firstValue = ((otherValue <= 0) ? (rectangle.Top + (rectangle.Height - previousValue) / 2 + previousValue - 1) : (rectangle.Top + (rectangle.Height - previousValue) / 2));
			int secondValue = rectangle.Left + (rectangle.Width - nextValue) / 2;
			while (nextValue >= 0)
			{
				if (nextValue == 1)
				{
					graphics.DrawLine(pen, secondValue, firstValue - otherValue, secondValue, firstValue);
				}
				else
				{
					graphics.DrawLine(pen, secondValue, firstValue, secondValue + nextValue - 1, firstValue);
				}
				secondValue++;
				nextValue -= 2;
				firstValue += otherValue;
			}
		}

		private static void DrawHorizontalTriangle(Graphics graphics, int value, Rectangle rectangle, int otherValue, Pen pen)
		{
			int nextValue = value / 2;
			if (nextValue % 2 == 0)
			{
				nextValue--;
			}
			int previousValue = nextValue / 2 + 1;
			int firstValue = ((otherValue <= 0) ? (rectangle.Left + (rectangle.Width - previousValue) / 2 + previousValue - 1) : (rectangle.Left + (rectangle.Width - previousValue) / 2));
			int secondValue = rectangle.Top + (rectangle.Height - nextValue) / 2;
			while (nextValue >= 0)
			{
				if (nextValue == 1)
				{
					graphics.DrawLine(pen, firstValue - otherValue, secondValue, firstValue, secondValue);
				}
				else
				{
					graphics.DrawLine(pen, firstValue, secondValue, firstValue, secondValue + nextValue - 1);
				}
				firstValue += otherValue;
				nextValue -= 2;
				secondValue++;
			}
		}

		internal bool ShouldDrawHeaderBorder()
		{
			if (_printDocument is MxDataGrid)
			{
				return ((MxDataGrid)_printDocument).FlatBorderInHeaders;
			}
			if (_printDocument is MxDataGridPrintDocument)
			{
				MxDataGridPrintDocument gridPrintDocument = (MxDataGridPrintDocument)_printDocument;
				if (gridPrintDocument.DataGrid != null)
				{
					if (gridPrintDocument.DataGrid.FlatBorderInHeaders)
					{
						return true;
					}
					return gridPrintDocument.DataGrid.HeaderFormatStyle.Appearance != Appearance.Flat;
				}
			}
			return true;
		}

		internal void DrawBorder(Graphics graphics, Rectangle bounds, BorderStyle borderStyle, bool condition)
		{
			if (borderStyle == BorderStyle.None)
			{
				return;
			}
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetLayoutState().GetControlBorderStyle();
			Rectangle rectangle = bounds;
			if (mordexColorSchemeAppearanceState != null && mordexColorSchemeAppearanceState.IsThemedLook)
			{
				if (borderStyle != BorderStyle.None)
				{
					Pen pen = ((!condition) ? GdiCache.Pen(mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledBorder)) : GdiCache.Pen(mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder)));
					rectangle = GridHelper.DrawFlatBorder(graphics, pen, rectangle);
				}
			}
			else if (mordexColorSchemeAppearanceState != null && mordexColorSchemeAppearanceState._isThemed)
			{
				Pen otherPen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(SystemColors.ControlDark, mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.Control), 0.35f)) : GdiCache.Pen(mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder)));
				GridHelper.DrawFlatBorder(graphics, otherPen, rectangle);
				rectangle.Inflate(-1, -1);
				otherPen = ((!condition) ? GdiCache.Pen(GridHelper.BlendColors(SystemColors.Window, SystemColors.ControlDark, 0.85f)) : GdiCache.Pen(SystemColors.Window));
				GridHelper.DrawFlatBorder(graphics, otherPen, rectangle);
			}
			else
			{
				GridHelper.DrawBorderForStyle(graphics, this, rectangle, borderStyle);
			}
		}

		internal MxColorSchemeAppearanceState ResolveAppearanceState(GridStyleArea styleArea)
		{
			if (_printDocument != null)
			{
				return _printDocument.GetDrawingStyle(styleArea);
			}
			return new MxColorSchemeAppearanceState(useTextRenderer: true);
		}
	}
}
