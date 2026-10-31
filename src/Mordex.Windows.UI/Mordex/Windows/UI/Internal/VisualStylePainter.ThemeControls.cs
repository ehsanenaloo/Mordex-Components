using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal partial class VisualStylePainter
	{
		private bool TrySelectElement(VisualStyleElement element)
		{
			if (!VisualStyleRenderer.IsElementDefined(element))
			{
				return false;
			}
			if (_visualStyleRenderer == null)
			{
				_visualStyleRenderer = new VisualStyleRenderer(element);
			}
			else
			{
				_visualStyleRenderer.SetParameters(element);
			}
			return true;
		}

		public static void DrawCheckedToolBarButtonTheme(Graphics dc, Rectangle bounds, bool isHot)
		{
			VisualStyleElement element = ((!isHot) ? VisualStyleElement.ToolBar.Button.Checked : VisualStyleElement.ToolBar.Button.HotChecked);
			if (VisualStyleRenderer.IsElementDefined(element))
			{
				VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
				visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawTrackBarThumb(Graphics dc, Rectangle bounds, Orientation orientation, TickStyle tickStyle, bool isEnabled, bool isHot, bool isPressed, bool isFocused)
		{
			int state = VisualStyleElement.TrackBar.ThumbBottom.Normal.State;
			if (!isEnabled)
			{
				state = VisualStyleElement.TrackBar.ThumbBottom.Disabled.State;
			}
			else if (isPressed && isHot)
			{
				state = VisualStyleElement.TrackBar.ThumbBottom.Pressed.State;
			}
			else if (isHot || isPressed)
			{
				state = VisualStyleElement.TrackBar.ThumbBottom.Hot.State;
			}
			else if (isFocused)
			{
				state = VisualStyleElement.TrackBar.ThumbBottom.Focused.State;
			}
			int part = 0;
			switch (tickStyle)
			{
			case TickStyle.None:
			case TickStyle.BottomRight:
				part = ((orientation != Orientation.Horizontal) ? VisualStyleElement.TrackBar.ThumbRight.Normal.Part : VisualStyleElement.TrackBar.ThumbBottom.Normal.Part);
				break;
			case TickStyle.TopLeft:
				part = ((orientation != Orientation.Horizontal) ? VisualStyleElement.TrackBar.ThumbLeft.Normal.Part : VisualStyleElement.TrackBar.ThumbTop.Normal.Part);
				break;
			case TickStyle.Both:
				part = ((orientation != Orientation.Horizontal) ? VisualStyleElement.TrackBar.ThumbVertical.Normal.Part : VisualStyleElement.TrackBar.Thumb.Normal.Part);
				break;
			}
			VisualStyleElement visualStyleElement = VisualStyleElement.CreateElement("TrackBar", part, state);
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawTrackBar(Graphics dc, Rectangle controlBounds, Rectangle trackBounds, Rectangle thumbBounds, Orientation orientation, TickStyle tickStyle, bool isEnabled, bool isHot, bool isPressed, bool isFocused)
		{
			if (orientation == Orientation.Horizontal)
			{
				VisualStyleElement horizontalTrackElement = VisualStyleElement.TrackBar.Track.Normal;
				if (TrySelectElement(horizontalTrackElement))
				{
					_visualStyleRenderer.DrawBackground(dc, trackBounds);
				}
			}
			else
			{
				VisualStyleElement verticalTrackElement = VisualStyleElement.TrackBar.TrackVertical.Normal;
				if (TrySelectElement(verticalTrackElement))
				{
					_visualStyleRenderer.DrawBackground(dc, trackBounds);
				}
			}
			DrawTrackBarThumb(dc, thumbBounds, orientation, tickStyle, isEnabled, isHot, isPressed, isFocused);
		}

		public void DrawProgressBar(Graphics dc, Rectangle barBounds, int percent)
		{
			if (percent >= 0)
			{
				barBounds.Inflate(-2, -2);
				Rectangle chunkBounds = barBounds;
				chunkBounds.Inflate(-3, -3);
				chunkBounds.Width++;
				chunkBounds.Width = chunkBounds.Width * percent / 100;
				VisualStyleElement element = VisualStyleElement.ProgressBar.Bar.Normal;
				if (TrySelectElement(element))
				{
					_visualStyleRenderer.DrawBackground(dc, barBounds);
				}
				element = VisualStyleElement.ProgressBar.Chunk.Normal;
				if (TrySelectElement(element))
				{
					_visualStyleRenderer.DrawBackground(dc, chunkBounds);
				}
			}
		}

		public static void DrawThemeElement(Graphics dc, Rectangle bounds, VisualStyleElement element)
		{
			if (VisualStyleRenderer.IsElementDefined(element))
			{
				VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
				visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawPushButtonTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled, bool isDefault)
		{
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.Button.PushButton.Disabled : ((isPressed && isHot) ? VisualStyleElement.Button.PushButton.Pressed : ((isHot || isPressed) ? VisualStyleElement.Button.PushButton.Hot : ((!isDefault) ? VisualStyleElement.Button.PushButton.Normal : VisualStyleElement.Button.PushButton.Default))));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public static bool DrawComboDropDownTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			VisualStyleElement element = ((!isEnabled) ? VisualStyleElement.ComboBox.DropDownButton.Disabled : (isPressed ? VisualStyleElement.ComboBox.DropDownButton.Pressed : ((!isHot) ? VisualStyleElement.ComboBox.DropDownButton.Normal : VisualStyleElement.ComboBox.DropDownButton.Hot)));
			if (!VisualStyleRenderer.IsElementDefined(element))
			{
				return false;
			}
			VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
			visualStyleRenderer.DrawBackground(dc, bounds);
			return true;
		}

		public void DrawScrollDownArrowTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.ScrollBar.ArrowButton.DownDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.DownPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.DownNormal : VisualStyleElement.ScrollBar.ArrowButton.DownHot)));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawScrollRightArrowTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.ScrollBar.ArrowButton.RightDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.RightPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.RightNormal : VisualStyleElement.ScrollBar.ArrowButton.RightHot)));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawCloseButtonTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.Window.CloseButton.Disabled : (isPressed ? VisualStyleElement.Window.CloseButton.Pressed : ((!isHot) ? VisualStyleElement.Window.CloseButton.Normal : VisualStyleElement.Window.CloseButton.Hot)));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawScrollLeftArrowTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.ScrollBar.ArrowButton.LeftDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.LeftPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.LeftNormal : VisualStyleElement.ScrollBar.ArrowButton.LeftHot)));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawScrollUpArrowTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isEnabled)
		{
			VisualStyleElement visualStyleElement = ((!isEnabled) ? VisualStyleElement.ScrollBar.ArrowButton.UpDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.UpPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.UpNormal : VisualStyleElement.ScrollBar.ArrowButton.UpHot)));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawRadioButtonTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, bool isChecked, bool isEnabled)
		{
			VisualStyleElement visualStyleElement = (isChecked ? ((!isEnabled) ? VisualStyleElement.Button.RadioButton.CheckedDisabled : (isPressed ? VisualStyleElement.Button.RadioButton.CheckedPressed : ((!isHot) ? VisualStyleElement.Button.RadioButton.CheckedNormal : VisualStyleElement.Button.RadioButton.CheckedHot))) : ((!isEnabled) ? VisualStyleElement.Button.RadioButton.UncheckedDisabled : (isPressed ? VisualStyleElement.Button.RadioButton.UncheckedPressed : ((!isHot) ? VisualStyleElement.Button.RadioButton.UncheckedNormal : VisualStyleElement.Button.RadioButton.UncheckedHot))));
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawCheckBoxTheme(Graphics dc, Rectangle bounds, bool isHot, bool isPressed, CheckState checkState, bool isEnabled)
		{
			VisualStyleElement visualStyleElement;
			switch (checkState)
			{
			case CheckState.Checked:
				visualStyleElement = (isEnabled ? ((!isPressed) ? ((!isHot) ? VisualStyleElement.Button.CheckBox.CheckedNormal : VisualStyleElement.Button.CheckBox.CheckedHot) : VisualStyleElement.Button.CheckBox.CheckedPressed) : VisualStyleElement.Button.CheckBox.CheckedDisabled);
				break;
			case CheckState.Unchecked:
				visualStyleElement = (isEnabled ? ((!isPressed) ? ((!isHot) ? VisualStyleElement.Button.CheckBox.UncheckedNormal : VisualStyleElement.Button.CheckBox.UncheckedHot) : VisualStyleElement.Button.CheckBox.UncheckedPressed) : VisualStyleElement.Button.CheckBox.UncheckedDisabled);
				break;
			default:
				visualStyleElement = (isEnabled ? ((!isPressed) ? ((!isHot) ? VisualStyleElement.Button.CheckBox.MixedNormal : VisualStyleElement.Button.CheckBox.MixedHot) : VisualStyleElement.Button.CheckBox.MixedPressed) : VisualStyleElement.Button.CheckBox.MixedDisabled);
				break;
			}
			if (TrySelectElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}
	}
}
