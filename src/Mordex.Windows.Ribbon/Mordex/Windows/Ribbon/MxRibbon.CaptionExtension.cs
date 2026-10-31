using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		private RibbonCaptionExtension _captionExtension;

		// Follows the form, the frame and the position of the ribbon while it has a handle. The orb is lifted only while the
		// ribbon lies at the top of the client area, so a move of the ribbon or of a parent can lift it or put it back.
		private FormBindingWatch _formWatch;

		private bool _captionLifted;

		// True after another owner took the caption from the ribbon. The ribbon then takes it back only on its own request: a
		// new handle, parent or form, or a change of ShowCaptionBar.
		private bool _captionYielded;
		private bool _reconcilingCaption;

		internal RibbonCaptionExtension CaptionExtension => _captionExtension;

		private FormBindingWatch FormWatch => _formWatch ?? (_formWatch = new FormBindingWatch(this, OnContainingFormChanged, () => ReconcileCaptionExtension(false), OnCaptionOriginChanged, () => ReconcileCaptionExtension(false)));

		// Office 2007 draws the application button and the quick access bar in the title bar, and Office 2010 draws the bar
		// there. The themed frame of the form does that for the ribbon when it paints its own caption and the ribbon does not
		// paint the caption itself. A claim takes the caption from another owner; without it the ribbon takes only a free
		// caption, and not after it gave the caption up. The caption of a browser frame belongs first to the tabs that a tab
		// control puts there, so there the ribbon takes only a free caption, also after it gave it up.
		private void ReconcileCaptionExtension(bool claim)
		{
			if (_reconcilingCaption)
			{
				return;
			}
			_reconcilingCaption = true;
			try
			{
				ReconcileCaptionExtensionCore(claim);
			}
			finally
			{
				_reconcilingCaption = false;
			}
		}

		private void ReconcileCaptionExtensionCore(bool claim)
		{
			Form form = IsDisposed || !IsHandleCreated || base.DesignMode || ShowCaptionBar ? null : GetHostForm();
			MxThemedFormFrame frame = form == null || !ReferenceEquals(FindForm(), form) ? null : MxThemedFormFrame.FromForm(form);
			bool want = frame != null && (RibbonThemeStyle.For(_colorScheme.Theme.Theme).AppButton == RibbonAppButtonKind.Orb || (frame.IsPainting && frame.CurrentStyle.IsCustom));
			bool browser = frame != null && frame.CurrentStyle.Kind == FormFrameKind.Browser;
			if (!want)
			{
				ReleaseCaptionExtension();
			}
			else if (_captionExtension == null || !ReferenceEquals(_captionExtension.Frame, frame) || !ReferenceEquals(frame.CaptionExtension, _captionExtension))
			{
				ReleaseCaptionExtension();
				if (frame.CaptionExtension == null ? claim || browser || !_captionYielded : claim && !browser)
				{
					_captionYielded = false;
					_captionExtension = new RibbonCaptionExtension(this, frame);
					frame.CaptionExtension = _captionExtension;
				}
			}
			SyncCaptionLift();
		}

		// Relayouts the ribbon when the orb moves between the title bar and the ribbon.
		private void SyncCaptionLift()
		{
			bool lifted = CaptionExtensionActive;
			if (lifted != _captionLifted)
			{
				_captionLifted = lifted;
				RelayoutForCaptionExtension();
			}
		}

		private void ReleaseCaptionExtension()
		{
			RibbonCaptionExtension extension = _captionExtension;
			if (extension == null)
			{
				return;
			}
			_captionExtension = null;
			if (ReferenceEquals(extension.Frame.CaptionExtension, extension))
			{
				extension.Frame.CaptionExtension = null;
			}
			extension.ClearState();
		}

		// The frame no longer uses the extension, because another control took the caption or the frame let the form go.
		internal void OnCaptionExtensionReleased(RibbonCaptionExtension extension)
		{
			if (!ReferenceEquals(_captionExtension, extension))
			{
				return;
			}
			_captionExtension = null;
			extension.ClearState();
			Form form = IsDisposed ? null : GetHostForm();
			// Giving the caption of a browser frame to tabs is not giving it up: the ribbon takes it back when it is free.
			if (form != null && ReferenceEquals(MxThemedFormFrame.FromForm(form), extension.Frame) && extension.Frame.CurrentStyle.Kind != FormFrameKind.Browser)
			{
				_captionYielded = true;
			}
			SyncCaptionLift();
		}

		private void StartCaptionWatch()
		{
			if (!base.DesignMode)
			{
				FormWatch.Start();
			}
			ReconcileCaptionExtension(true);
		}

		private void StopCaptionWatch()
		{
			_formWatch?.Stop();
			ReleaseCaptionExtension();
			_captionLifted = false;
		}

		private bool CaptionExtensionActive => _captionExtension != null && _captionExtension.IsActive;

		// The orb of Office 2007 in the coordinates of the ribbon: large, in the corner of the window, over the title bar and the
		// tab row. Empty when the title bar does not draw it; the ribbon then draws its ordinary orb.
		internal Rectangle GetCaptionOrbBounds()
		{
			if (_captionExtension == null || !_captionExtension.TryGetLift(out Rectangle orb, out Point origin))
			{
				return Rectangle.Empty;
			}
			return new Rectangle(orb.X - origin.X, orb.Y - origin.Y, orb.Width, orb.Height);
		}

		internal BackstageMenuTabRenderer GetBackstageTabRenderer()
		{
			return _tabStripRenderer == null || BackstageMenu == null ? null : _tabStripRenderer.FindTabRenderer(BackstageMenu) as BackstageMenuTabRenderer;
		}

		internal bool IsCaptionOrbHot()
		{
			return CaptionExtensionActive && _captionExtension.OrbHot;
		}

		// The two halves of the button are drawn by different windows, so a change of state draws both. The quick access bar in
		// the caption is drawn again with it.
		internal void RefreshCaptionOrb()
		{
			if (CaptionExtensionActive)
			{
				_captionExtension.Frame.InvalidateCaption();
			}
		}

		internal void RelayoutForCaptionExtension()
		{
			if (IsDisposed || !IsHandleCreated)
			{
				return;
			}
			RequestTabStripLayout(true);
			InvalidateFullLayout();
		}

		internal void SetCaptionOrbHot(bool hot)
		{
			if (_captionExtension != null && _captionExtension.OrbHot != hot)
			{
				_captionExtension.OrbHot = hot;
				Invalidate();
				_captionExtension.Frame.InvalidateCaption();
			}
		}

		private void OnEnabledChangedForCaption()
		{
			if (_captionExtension == null)
			{
				return;
			}
			if (!base.Enabled)
			{
				_captionExtension.CancelPress();
			}
			_captionExtension.Frame.InvalidateCaption();
		}

		private void OnCaptionOriginChanged(object sender, EventArgs e)
		{
			if (_captionExtension == null)
			{
				return;
			}
			SyncCaptionLift();
			_captionExtension?.Frame.InvalidateCaption();
		}
	}
}
