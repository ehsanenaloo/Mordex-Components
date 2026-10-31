using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal partial class ScrollBarControl
	{
		protected override void OnPaint(PaintEventArgs pe)
		{
			if (_style == null)
			{
				pe.Graphics.FillRectangle(SystemBrushes.Control, base.ClientRectangle);
			}
			else
			{
				DrawScrollBar(pe.Graphics);
			}
			base.OnPaint(pe);
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
		}

		internal void RelayoutAndInvalidate()
		{
			try
			{
				LayoutAreas();
				Invalidate();
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void DrawBackground(Graphics graphics, int value)
		{
			ControlStyleControl2Data a = _controlStyleControl2DataArray[value];
			Rectangle mirrorForRightToLeft = MirrorForRightToLeft(a._bounds);
			if (a._isVisible)
			{
				if (a._isPressed)
				{
					Brush brush = GdiCache.Brush(Color.Black);
					graphics.FillRectangle(brush, mirrorForRightToLeft);
					brush = null;
				}
				else
				{
					graphics.FillRectangle(_style.GetScrollBarBrush(), mirrorForRightToLeft);
				}
			}
		}

		private void DrawScrollBar(Graphics graphics)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _style.ResolveAppearanceState(GridStyleArea.ScrollBars);
			if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				DrawThemedScrollBar(graphics);
			}
			else if (mordexColorSchemeAppearanceState._isThemed && VisualStyleDrawer.IsThemingAvailable())
			{
				if (!TryDrawThemedScrollBar(graphics))
				{
					DrawClassicScrollBar(graphics);
				}
			}
			else
			{
				DrawClassicScrollBar(graphics);
			}
		}

		private void DrawClassicScrollBar(Graphics graphics)
		{
			DrawClassicArea(graphics, 0);
			DrawClassicArea(graphics, 4);
			DrawClassicArea(graphics, 2);
			DrawBackground(graphics, 1);
			DrawBackground(graphics, 3);
		}

		private bool TryDrawThemedScrollBar(Graphics graphics)
		{
			bool condition = !base.Enabled;
			if (_type == ScrollBarType.Vertical)
			{
				ControlStyleControl2Data a = _controlStyleControl2DataArray[0];
				if (a._isVisible && !VisualStyleDrawer.DrawScrollUpArrow(graphics, a._bounds, a._controlStyleControl2DataBoolean, a._isPressed, condition))
				{
					return false;
				}
				a = _controlStyleControl2DataArray[4];
				if (a._isVisible && !VisualStyleDrawer.DrawScrollDownArrow(graphics, a._bounds, a._controlStyleControl2DataBoolean, a._isPressed, condition))
				{
					return false;
				}
				a = _controlStyleControl2DataArray[2];
				if (a._isVisible && !condition && !VisualStyleDrawer.RenderContent(graphics, a._bounds, a._controlStyleControl2DataBoolean, a._isPressed))
				{
					return false;
				}
				a = _controlStyleControl2DataArray[1];
				if (a._isVisible && !VisualStyleDrawer.DrawVerticalUpperTrack(graphics, a._bounds, a._controlStyleControl2DataBoolean, a._isPressed, condition))
				{
					return false;
				}
				a = _controlStyleControl2DataArray[3];
				if (a._isVisible && !VisualStyleDrawer.DrawVerticalLowerTrack(graphics, a._bounds, a._controlStyleControl2DataBoolean, a._isPressed, condition))
				{
					return false;
				}
			}
			else
			{
				ControlStyleControl2Data a = _controlStyleControl2DataArray[0];
				Rectangle rectangle = MirrorForRightToLeft(a._bounds);
				if (a._isVisible)
				{
					if (RightToLeft == RightToLeft.No)
					{
						if (!VisualStyleDrawer.DrawScrollLeftArrow(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed, condition))
						{
							return false;
						}
					}
					else if (!VisualStyleDrawer.DrawScrollRightArrow(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed, condition))
					{
						return false;
					}
				}
				a = _controlStyleControl2DataArray[4];
				rectangle = MirrorForRightToLeft(a._bounds);
				if (a._isVisible)
				{
					if (RightToLeft == RightToLeft.No)
					{
						if (!VisualStyleDrawer.DrawScrollRightArrow(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed, condition))
						{
							return false;
						}
					}
					else if (!VisualStyleDrawer.DrawScrollLeftArrow(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed, condition))
					{
						return false;
					}
				}
				a = _controlStyleControl2DataArray[2];
				rectangle = MirrorForRightToLeft(a._bounds);
				if (a._isVisible && !condition && !VisualStyleDrawer.DrawHorizontalThumb(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed))
				{
					return false;
				}
				a = _controlStyleControl2DataArray[1];
				rectangle = MirrorForRightToLeft(a._bounds);
				if (a._isVisible && !VisualStyleDrawer.DrawHorizontalRightTrack(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed, condition))
				{
					return false;
				}
				a = _controlStyleControl2DataArray[3];
				rectangle = MirrorForRightToLeft(a._bounds);
				if (a._isVisible && !VisualStyleDrawer.DrawHorizontalLeftTrack(graphics, rectangle, a._controlStyleControl2DataBoolean, a._isPressed, condition))
				{
					return false;
				}
			}
			return true;
		}

		private void DrawClassicArea(Graphics graphics, int value)
		{
			ControlStyleControl2Data a = _controlStyleControl2DataArray[value];
			_style.GetControlBrush();
			Pen pen = _style.GetControlTextPen();
			bool condition = false;
			if (!a._isVisible)
			{
				return;
			}
			Rectangle rectangle = MirrorForRightToLeft(a._bounds);
			bool otherCondition = a._isPressed;
			if (otherCondition && a._type == ScrollAreaType.Thumb && _style.ButtonAppearance == ButtonAppearance.PopUp)
			{
				otherCondition = false;
			}
			_style.DrawBackground(graphics, rectangle, a._controlStyleControl2DataBoolean, otherCondition);
			switch (value)
			{
			case 0:
				if (_type == ScrollBarType.Vertical)
				{
					_style.DrawVerticalArrow(graphics, a._bounds, -1, a._isPressed, base.Enabled);
				}
				else if (RightToLeft == RightToLeft.No)
				{
					_style.DrawHorizontalArrow(graphics, rectangle, -1, a._controlStyleControl2DataBoolean, a._isPressed, base.Enabled);
				}
				else
				{
					_style.DrawHorizontalArrow(graphics, rectangle, 1, a._controlStyleControl2DataBoolean, a._isPressed, base.Enabled);
				}
				break;
			case 4:
				if (_type == ScrollBarType.Vertical)
				{
					_style.DrawVerticalArrow(graphics, a._bounds, 1, a._isPressed, base.Enabled);
				}
				else if (RightToLeft == RightToLeft.No)
				{
					_style.DrawHorizontalArrow(graphics, rectangle, 1, a._controlStyleControl2DataBoolean, a._isPressed, base.Enabled);
				}
				else
				{
					_style.DrawHorizontalArrow(graphics, rectangle, -1, a._controlStyleControl2DataBoolean, a._isPressed, base.Enabled);
				}
				break;
			}
			if (condition)
			{
				pen.Dispose();
				pen = null;
			}
		}
	}
}
