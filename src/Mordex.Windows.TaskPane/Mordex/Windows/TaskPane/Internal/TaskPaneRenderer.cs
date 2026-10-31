using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Mordex.Windows.TaskPane.Internal
{
	internal class TaskPaneRenderer
	{
		private VisualStyleRenderer _visualStyleRenderer;

		internal static bool IsThemingActive()
		{
			try
			{
				return VisualStyleInformation.IsEnabledByUser && VisualStyleInformation.IsSupportedByOS && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled);
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return false;
			}
		}

		private bool TrySetElement(VisualStyleElement element)
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

		public void DrawUpArrow(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.ArrowButton.UpDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.UpPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.UpNormal : VisualStyleElement.ScrollBar.ArrowButton.UpHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawDownArrow(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.ArrowButton.DownDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.DownPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.DownNormal : VisualStyleElement.ScrollBar.ArrowButton.DownHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawVerticalThumb(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed)
		{
			VisualStyleElement visualStyleElement = (isPressed ? VisualStyleElement.ScrollBar.ThumbButtonVertical.Pressed : ((!isHot) ? VisualStyleElement.ScrollBar.ThumbButtonVertical.Normal : VisualStyleElement.ScrollBar.ThumbButtonVertical.Hot));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
			if (TrySetElement(VisualStyleElement.ScrollBar.GripperVertical.Normal))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawLowerTrack(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.LowerTrackVertical.Disabled : (isPressed ? VisualStyleElement.ScrollBar.LowerTrackVertical.Pressed : ((!isHot) ? VisualStyleElement.ScrollBar.LowerTrackVertical.Normal : VisualStyleElement.ScrollBar.LowerTrackVertical.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawUpperTrack(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.UpperTrackVertical.Disabled : (isPressed ? VisualStyleElement.ScrollBar.UpperTrackVertical.Pressed : ((!isHot) ? VisualStyleElement.ScrollBar.UpperTrackVertical.Normal : VisualStyleElement.ScrollBar.UpperTrackVertical.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawLeftArrow(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.ArrowButton.LeftDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.LeftPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.LeftNormal : VisualStyleElement.ScrollBar.ArrowButton.LeftHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawRightArrow(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.ArrowButton.RightDisabled : (isPressed ? VisualStyleElement.ScrollBar.ArrowButton.RightPressed : ((!isHot) ? VisualStyleElement.ScrollBar.ArrowButton.RightNormal : VisualStyleElement.ScrollBar.ArrowButton.RightHot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawHorizontalThumb(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed)
		{
			VisualStyleElement visualStyleElement = (isPressed ? VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Pressed : ((!isHot) ? VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Normal : VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Hot));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
			if (TrySetElement(VisualStyleElement.ScrollBar.GripperHorizontal.Normal))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawLeftTrack(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.LeftTrackHorizontal.Disabled : (isPressed ? VisualStyleElement.ScrollBar.LeftTrackHorizontal.Pressed : ((!isHot) ? VisualStyleElement.ScrollBar.LeftTrackHorizontal.Normal : VisualStyleElement.ScrollBar.LeftTrackHorizontal.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}

		public void DrawRightTrack(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed, bool isDisabled)
		{
			VisualStyleElement visualStyleElement = (isDisabled ? VisualStyleElement.ScrollBar.RightTrackHorizontal.Disabled : (isPressed ? VisualStyleElement.ScrollBar.RightTrackHorizontal.Pressed : ((!isHot) ? VisualStyleElement.ScrollBar.RightTrackHorizontal.Normal : VisualStyleElement.ScrollBar.RightTrackHorizontal.Hot)));
			if (TrySetElement(visualStyleElement))
			{
				_visualStyleRenderer.DrawBackground(graphics, bounds);
			}
		}
	}
}
