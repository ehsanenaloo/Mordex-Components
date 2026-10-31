using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class VisualStyleDrawer
	{
		internal static VisualStyleRenderer _visualStyleRenderer;

		internal static bool TrySetElement(VisualStyleElement element)
		{
			if (!IsThemingAvailable())
			{
				return false;
			}
			if (!VisualStyleRenderer.IsElementDefined(element))
			{
				return false;
			}
			try
			{
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
			catch (InvalidOperationException)
			{
				// Windows can refuse a visual style handle while the style changes.
				_visualStyleRenderer = null;
				return false;
			}
		}

		public static bool IsThemingAvailable()
		{
			try
			{
				if (VisualStyleInformation.IsEnabledByUser && VisualStyleInformation.IsSupportedByOS && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled))
				{
					return VisualStyleRenderer.IsSupported;
				}
				return false;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return false;
			}
		}

		public static bool DrawPushButton(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			try
			{
				VisualStyleElement visualStyleElement = VisualStyleElement.Button.PushButton.Normal;
				if (nextCondition)
				{
					visualStyleElement = VisualStyleElement.Button.PushButton.Disabled;
				}
				else if (otherCondition)
				{
					visualStyleElement = VisualStyleElement.Button.PushButton.Pressed;
				}
				else if (condition)
				{
					visualStyleElement = VisualStyleElement.Button.PushButton.Hot;
				}
				if (TrySetElement(visualStyleElement))
				{
					_visualStyleRenderer.DrawBackground(dc, bounds);
					return true;
				}
				return false;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return false;
			}
		}

		public static bool DrawCheckBox(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition, bool previousCondition, bool firstCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? ((!firstCondition) ? VisualStyleElement.Button.CheckBox.MixedDisabled : (otherCondition ? VisualStyleElement.Button.CheckBox.MixedPressed : ((!condition) ? VisualStyleElement.Button.CheckBox.MixedNormal : VisualStyleElement.Button.CheckBox.MixedHot))) : (previousCondition ? ((!firstCondition) ? VisualStyleElement.Button.CheckBox.CheckedDisabled : (otherCondition ? VisualStyleElement.Button.CheckBox.CheckedPressed : ((!condition) ? VisualStyleElement.Button.CheckBox.CheckedNormal : VisualStyleElement.Button.CheckBox.CheckedHot))) : ((!firstCondition) ? VisualStyleElement.Button.CheckBox.UncheckedDisabled : (otherCondition ? VisualStyleElement.Button.CheckBox.UncheckedPressed : ((!condition) ? VisualStyleElement.Button.CheckBox.UncheckedNormal : VisualStyleElement.Button.CheckBox.UncheckedHot)))));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawThemedCheckBox(Graphics graphics, Rectangle rectangle, bool condition, bool otherCondition, bool nextCondition, bool previousCondition, bool firstCondition)
		{
			return DrawCheckBox(graphics, rectangle, condition, otherCondition, nextCondition, previousCondition, firstCondition);
		}

		public static bool DrawComboDropDownButton(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = ((!nextCondition) ? VisualStyleElement.ComboBox.DropDownButton.Disabled : (otherCondition ? VisualStyleElement.ComboBox.DropDownButton.Pressed : ((!condition) ? VisualStyleElement.ComboBox.DropDownButton.Normal : VisualStyleElement.ComboBox.DropDownButton.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawScrollUpArrow(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.ArrowButton.UpDisabled : (otherCondition ? VisualStyleElement.ScrollBar.ArrowButton.UpPressed : ((!condition) ? VisualStyleElement.ScrollBar.ArrowButton.UpNormal : VisualStyleElement.ScrollBar.ArrowButton.UpHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawScrollDownArrow(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.ArrowButton.DownDisabled : (otherCondition ? VisualStyleElement.ScrollBar.ArrowButton.DownPressed : ((!condition) ? VisualStyleElement.ScrollBar.ArrowButton.DownNormal : VisualStyleElement.ScrollBar.ArrowButton.DownHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool RenderContent(Graphics dc, Rectangle bounds, bool condition, bool otherCondition)
		{
			VisualStyleElement visualStyleElement = (otherCondition ? VisualStyleElement.ScrollBar.ThumbButtonVertical.Pressed : ((!condition) ? VisualStyleElement.ScrollBar.ThumbButtonVertical.Normal : VisualStyleElement.ScrollBar.ThumbButtonVertical.Hot));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				if (TrySetElement(VisualStyleElement.ScrollBar.GripperVertical.Normal))
				{
					_visualStyleRenderer.DrawBackground(dc, bounds);
				}
				return true;
			}
			return false;
		}

		public static bool DrawVerticalLowerTrack(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled : (otherCondition ? VisualStyleElement.ScrollBar.LowerTrackVertical.Pressed : ((!condition) ? VisualStyleElement.ScrollBar.LowerTrackVertical.Normal : VisualStyleElement.ScrollBar.LowerTrackVertical.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawVerticalUpperTrack(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.UpperTrackVertical.Disabled : (otherCondition ? VisualStyleElement.ScrollBar.UpperTrackVertical.Pressed : ((!condition) ? VisualStyleElement.ScrollBar.UpperTrackVertical.Normal : VisualStyleElement.ScrollBar.UpperTrackVertical.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawScrollLeftArrow(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.ArrowButton.LeftDisabled : (otherCondition ? VisualStyleElement.ScrollBar.ArrowButton.LeftPressed : ((!condition) ? VisualStyleElement.ScrollBar.ArrowButton.LeftNormal : VisualStyleElement.ScrollBar.ArrowButton.LeftHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawScrollRightArrow(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.ArrowButton.RightDisabled : (otherCondition ? VisualStyleElement.ScrollBar.ArrowButton.RightPressed : ((!condition) ? VisualStyleElement.ScrollBar.ArrowButton.RightNormal : VisualStyleElement.ScrollBar.ArrowButton.RightHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawHorizontalThumb(Graphics dc, Rectangle bounds, bool condition, bool otherCondition)
		{
			VisualStyleElement visualStyleElement = (otherCondition ? VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Pressed : ((!condition) ? VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Normal : VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Hot));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				if (TrySetElement(VisualStyleElement.ScrollBar.GripperHorizontal.Normal))
				{
					_visualStyleRenderer.DrawBackground(dc, bounds);
				}
				return true;
			}
			return false;
		}

		public static bool DrawHorizontalLeftTrack(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.LeftTrackHorizontal.Disabled : (otherCondition ? VisualStyleElement.ScrollBar.LeftTrackHorizontal.Pressed : ((!condition) ? VisualStyleElement.ScrollBar.LeftTrackHorizontal.Normal : VisualStyleElement.ScrollBar.LeftTrackHorizontal.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawHorizontalRightTrack(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = (nextCondition ? VisualStyleElement.ScrollBar.RightTrackHorizontal.Disabled : (otherCondition ? VisualStyleElement.ScrollBar.RightTrackHorizontal.Pressed : ((!condition) ? VisualStyleElement.ScrollBar.RightTrackHorizontal.Normal : VisualStyleElement.ScrollBar.RightTrackHorizontal.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawSpinUpButton(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			bounds.Height++;
			VisualStyleElement visualStyleElement = ((!nextCondition) ? VisualStyleElement.Spin.Up.Disabled : (otherCondition ? VisualStyleElement.Spin.Up.Pressed : ((!condition) ? VisualStyleElement.Spin.Up.Normal : VisualStyleElement.Spin.Up.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}

		public static bool DrawSpinDownButton(Graphics dc, Rectangle bounds, bool condition, bool otherCondition, bool nextCondition)
		{
			VisualStyleElement visualStyleElement = ((!nextCondition) ? VisualStyleElement.Spin.Down.Disabled : (otherCondition ? VisualStyleElement.Spin.Down.Pressed : ((!condition) ? VisualStyleElement.Spin.Down.Normal : VisualStyleElement.Spin.Down.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
				return true;
			}
			return false;
		}
	}
}
