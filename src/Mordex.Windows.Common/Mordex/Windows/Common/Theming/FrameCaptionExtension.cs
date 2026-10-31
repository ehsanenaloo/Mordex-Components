using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// Content that a control draws into the caption of a themed window frame, for example the application button and the
	/// quick access bar of a ribbon. All points and rectangles are in window coordinates.
	/// </summary>
	/// <remarks>
	/// A press on the extension starts a gesture that the frame owns. The frame takes the mouse capture, sends the moves to
	/// <see cref="MouseMove"/> wherever the pointer is, and ends the gesture with exactly one call: <see cref="MouseUp"/> when the
	/// button is released anywhere, or <see cref="GestureCanceled"/> when the capture is lost, the window is deactivated or
	/// destroyed, or the extension is removed.
	/// </remarks>
	public interface IFrameCaptionExtension
	{
		/// <summary>The width at the left of the caption that the extension uses, in device pixels. The icon is not drawn and the title starts after it.</summary>
		int LeftInset { get; }

		/// <summary>True when the extension fills the caption between its left inset and the buttons, so the title is not drawn.</summary>
		bool HidesTitle => false;

		/// <summary>Draws the extension. The graphics is clipped to the caption.</summary>
		void Paint(Graphics graphics, Rectangle caption, bool active);

		/// <summary>True when the point lies on a part that the extension handles itself, so it is not a part of the caption that moves the window.</summary>
		bool HitTest(Point point);

		/// <summary>The frame was applied or changed, so the places and sizes of its parts may be new.</summary>
		void FrameChanged();

		void MouseMove(Point point);

		void MouseLeave();

		void MouseDown(Point point);

		/// <summary>The button was released. The point can lie outside the caption and outside the window.</summary>
		void MouseUp(Point point);

		/// <summary>The press ended without a release. The extension clears its pressed and drag state and runs no action.</summary>
		void GestureCanceled()
		{
		}

		/// <summary>The frame no longer uses this extension: another extension took its place, it was removed, or the frame let the form go.</summary>
		void CaptionReleased()
		{
		}
	}

	/// <summary>An extension that leaves the icon of the window in its place: its left inset counts from the left edge of the window and includes the icon.</summary>
	internal interface ICaptionIconKeeper
	{
		bool KeepsIcon { get; }
	}
}
