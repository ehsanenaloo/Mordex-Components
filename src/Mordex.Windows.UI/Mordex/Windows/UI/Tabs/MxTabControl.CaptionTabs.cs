using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		private bool _tabsInCaption;

		private bool _showAddTabButton;

		private TabCaptionExtension _captionTabs;

		// True after another owner took the caption from this control. The control then shows its own strip and takes the
		// caption back only on its own request: TabsInCaption, a new handle or parent, or being shown again.
		private bool _captionYielded;

		// Follows the form of the control while it has a handle: a move to another form is a new request for its caption.
		private FormBindingWatch _formWatch;

		private FormBindingWatch FormWatch => _formWatch ?? (_formWatch = new FormBindingWatch(this, () => UpdateCaptionTabs(true), () => UpdateCaptionTabs(!_captionYielded)));

		/// <summary>
		/// Draws the tab strip into the title bar of the form when the theme has a window frame for it (Chrome and Firefox).
		/// The control itself then shows only the pages. The title bar has room for one such strip: when two controls ask for
		/// it, the one that asked last shows its tabs there and the other shows its own strip.
		/// </summary>
		[DefaultValue(false)]
		[Category("Appearance")]
		[Description("Indicates whether the tab strip is drawn in the title bar of the form when the theme allows it.")]
		public bool TabsInCaption
		{
			get => _tabsInCaption;
			set
			{
				if (_tabsInCaption != value)
				{
					_tabsInCaption = value;
					UpdateCaptionTabs(true);
					InvalidateLayout();
				}
			}
		}

		/// <summary>Shows a button after the last tab in the title bar. Use <see cref="AddTabClick"/> to add a page.</summary>
		[DefaultValue(false)]
		[Category("Appearance")]
		[Description("Indicates whether the tab strip in the title bar shows a button that adds a tab.")]
		public bool ShowAddTabButton
		{
			get => _showAddTabButton;
			set
			{
				if (_showAddTabButton != value)
				{
					_showAddTabButton = value;
					_captionTabs?.Refresh();
				}
			}
		}

		[Description("Event fired when the button for a new tab in the title bar is clicked.")]
		public event EventHandler AddTabClick
		{
			add => base.Events.AddHandler(_addTabClickEvent, value);
			remove => base.Events.RemoveHandler(_addTabClickEvent, value);
		}

		private static readonly object _addTabClickEvent = new object();

		/// <summary>True while the title bar of the form shows the tab strip.</summary>
		internal bool CaptionTabsActive => _captionTabs != null && _captionTabs.IsActive;

		internal TabCaptionExtension CaptionTabs => _captionTabs;

		private bool StripShown => _showTabs && !CaptionTabsActive;

		internal void RaiseAddTabClick()
		{
			if (base.Events[_addTabClickEvent] is EventHandler handler)
			{
				handler(this, EventArgs.Empty);
			}
		}

		// Connects the control to the frame of its form, or releases it. A claim takes the caption from another owner; without
		// it the control takes only a free caption, and not after it gave the caption up.
		private void UpdateCaptionTabs(bool claim)
		{
			Form form = base.IsHandleCreated && !base.DesignMode && !base.IsDisposed && _tabsInCaption ? FindForm() : null;
			MxThemedFormFrame frame = form == null ? null : MxThemedFormFrame.FromForm(form);
			if (frame == null || frame.CurrentStyle.Kind != FormFrameKind.Browser || !(base.Visible || !form.Visible))
			{
				ReleaseCaptionTabs();
				return;
			}
			IFrameCaptionExtension current = frame.CaptionExtension;
			if (_captionTabs != null && ReferenceEquals(current, _captionTabs))
			{
				return;
			}
			ReleaseCaptionTabs();
			if (!claim && (_captionYielded || current != null))
			{
				return;
			}
			_captionYielded = false;
			_captionTabs = new TabCaptionExtension(this, frame);
			frame.CaptionExtension = _captionTabs;
			InvalidateLayout();
			NotifyCaptionAccessibility(AccessibleEvents.Reorder, -1);
		}

		// The frame may take its new style after this control hears of the theme change, so look again when the message queue is
		// idle. The tabs take the caption of a browser frame from a ribbon that shows its quick access bar there, unless they gave
		// the caption up before.
		private void OnThemeChangedForCaption()
		{
			OnThemeChangedForTabClose();
			UpdateCaptionTabs(!_captionYielded);
			if (base.IsHandleCreated && !base.IsDisposed)
			{
				BeginInvoke(new Action(() => UpdateCaptionTabs(!_captionYielded)));
			}
		}

		private void ReleaseCaptionTabs()
		{
			TabCaptionExtension extension = _captionTabs;
			if (extension == null)
			{
				return;
			}
			_captionTabs = null;
			if (ReferenceEquals(extension.Frame.CaptionExtension, extension))
			{
				extension.Frame.CaptionExtension = null;
			}
			extension.Release();
			InvalidateLayout();
			NotifyCaptionAccessibility(AccessibleEvents.Reorder, -1);
		}

		// The frame no longer uses the extension: another owner took the caption, or the frame let the form go.
		internal void OnCaptionTabsReleased(TabCaptionExtension extension)
		{
			if (!ReferenceEquals(_captionTabs, extension))
			{
				return;
			}
			_captionTabs = null;
			extension.Release();
			Form form = base.IsDisposed ? null : FindForm();
			bool frameGone = form == null || !ReferenceEquals(MxThemedFormFrame.FromForm(form), extension.Frame);
			if (frameGone)
			{
				// A new frame of the form can take the strip once the old one is gone.
				if (base.IsHandleCreated && !base.IsDisposed)
				{
					BeginInvoke(new Action(() => UpdateCaptionTabs(false)));
				}
			}
			else
			{
				_captionYielded = true;
			}
			InvalidateLayout();
			NotifyCaptionAccessibility(AccessibleEvents.Reorder, -1);
		}

		// The form of the control was shown or hidden, so the control may now be shown or hidden as well.
		internal void OnCaptionEligibilityChanged()
		{
			UpdateCaptionTabs(false);
			InvalidateLayout();
		}

		// The frame took another style, so the strip moves between the title bar and the control.
		internal void OnCaptionFrameChanged()
		{
			if (!base.IsDisposed && base.IsHandleCreated)
			{
				InvalidateLayout();
			}
		}

		internal void InvalidateCaptionTabs()
		{
			_captionTabs?.Refresh();
		}

		internal void NotifyCaptionAccessibility(AccessibleEvents accEvent, int childId)
		{
			if (base.IsHandleCreated && !base.IsDisposed)
			{
				AccessibilityNotifyClients(accEvent, childId);
			}
		}

		// The accessible children of the tab strip while it lies in the title bar.
		internal List<AccessibleObject> GetCaptionAccessibleObjects()
		{
			return CaptionTabsActive ? _captionTabs.GetAccessibleObjects() : null;
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			_formWatch?.Update();
			UpdateCaptionTabs(true);
		}

		private void OnVisibleChangedForCaption()
		{
			if (!base.Visible)
			{
				_captionTabs?.CancelPress();
			}
			UpdateCaptionTabs(base.Visible);
		}

		private void OnEnabledChangedForCaption()
		{
			if (!base.Enabled)
			{
				_captionTabs?.CancelPress();
			}
			_captionTabs?.Refresh();
		}

		private void OnSelectedTabChangedForCaption()
		{
			if (CaptionTabsActive && SelectedTab != null)
			{
				int index = _captionTabs.IndexOfAccessibleTab(SelectedTab);
				if (index >= 0)
				{
					NotifyCaptionAccessibility(AccessibleEvents.Selection, index);
				}
			}
		}
	}
}
