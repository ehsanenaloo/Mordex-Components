using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		internal bool IsCaptionBarShown()
		{
			if (_showCaptionBar)
			{
				return true;
			}
			return false;
		}

		internal bool IsGlassCaptionActive()
		{
			if (_isGlassAvailable && _enableGlassEffect && HandlesTitleBar && !base.DesignMode && (GetHostForm() == null || !GetHostForm().IsMdiChild))
			{
				return true;
			}
			return false;
		}

		internal bool IsParentedToForm()
		{
			return _isParentedToForm;
		}

		internal void SetParentedToForm(bool isParentedToForm)
		{
			if (isParentedToForm != _isParentedToForm)
			{
				_isParentedToForm = isParentedToForm;
			}
		}

		internal bool IsFormActive()
		{
			if (_ribbonWindowMessageHandler != null)
			{
				return _ribbonWindowMessageHandler.IsActive();
			}
			return true;
		}

		internal Form GetHostForm()
		{
			return _form;
		}

		internal void AttachToForm(Form form)
		{
			if (_form == form)
			{
				return;
			}
			if (_form != null)
			{
				foreach (Control oldFormControl in _form.Controls)
				{
					if (oldFormControl is MdiClient)
					{
						RibbonMdiClientMessageHandler.Detach((MdiClient)oldFormControl);
					}
				}
				Application.RemoveMessageFilter(this);
				_form.MdiChildActivate -= OnFormMdiChildActivate;
				_form.ControlAdded -= OnFormControlAdded;
				_form.ControlRemoved -= OnFormControlRemoved;
				_form.Activated -= OnFormActivated;
				_form.Deactivate -= OnFormDeactivate;
				_form.HandleCreated -= OnFormHandleCreated;
				_form.HandleDestroyed -= OnFormHandleDestroyed;
				_form.Resize -= OnFormResize;
				_form.LocationChanged -= OnFormLocationChanged;
				_headerRenderer.SetFormHandle(IntPtr.Zero);
			}
			if (_ribbonWindowMessageHandler != null)
			{
				if (_form != null && _form.IsHandleCreated)
				{
					NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
				}
				_ribbonWindowMessageHandler.Dispose();
				_ribbonWindowMessageHandler = null;
				if (_form != null && (!_form.Disposing || _form.IsDisposed))
				{
					try
					{
						NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
					}
					catch (InvalidOperationException)
					{
						// The form handle cannot be created while the form is closing.
					}
				}
			}
			_form = form;
			SetParentedToForm(GetHostForm() == base.Parent && base.Parent != null);
			if (_form != null)
			{
				RecalculateLayout(true);
				Application.AddMessageFilter(this);
				if (IsParentedToForm())
				{
					if (!base.DesignMode)
					{
						foreach (Control newFormControl in _form.Controls)
						{
							if (newFormControl is MdiClient)
							{
								RibbonMdiClientMessageHandler.Attach((MdiClient)newFormControl);
								break;
							}
						}
					}
					CreateCaptionHandler(true, false);
					_form.MdiChildActivate += OnFormMdiChildActivate;
					_form.ControlAdded += OnFormControlAdded;
					_form.ControlRemoved += OnFormControlRemoved;
					_form.Activated += OnFormActivated;
					_form.Deactivate += OnFormDeactivate;
					_form.HandleCreated += OnFormHandleCreated;
					_form.HandleDestroyed += OnFormHandleDestroyed;
					_form.Resize += OnFormResize;
					_form.LocationChanged += OnFormLocationChanged;
					if (_ribbonWindowMessageHandler is RibbonGlassCaptionMessageHandler)
					{
						RecreateHandle();
					}
				}
			}
			RelayoutForFormState();
			InvalidateFullLayout();
			InvalidateWindowFrame();
		}

		internal bool IsFormMaximized()
		{
			if (GetHostForm() != null && IsParentedToForm() && GetHostForm().WindowState == FormWindowState.Maximized)
			{
				return true;
			}
			return false;
		}

		internal Size GetFrameBorderSize()
		{
			if (GetHostForm() != base.Parent)
			{
				return Size.Empty;
			}
			return _frameBorderSize;
		}

		private void CreateCaptionHandler(bool addOffset, bool runTimeForm)
		{
			if (IsParentedToForm() && Dock == DockStyle.Top && base.Location.Y == 0 && HandlesTitleBar && _form.IsHandleCreated)
			{
				if (_form != null)
				{
					NativeMethods.SetPropW(GetHostForm().Handle, "Mordex_Ribbon", new IntPtr(1));
				}
				if (IsGlassCaptionActive())
				{
					_ribbonWindowMessageHandler = new RibbonGlassCaptionMessageHandler(GetHostForm(), this);
				}
				else
				{
					_ribbonWindowMessageHandler = new RibbonCustomCaptionMessageHandler(GetHostForm(), this, addOffset, runTimeForm);
				}
				SyncHeaderFormHandle();
				if (_form != null)
				{
					_ribbonWindowMessageHandler.SetCaptionBounds(_form.ClientRectangle);
					_ribbonWindowMessageHandler.Refresh(update: false);
					InvalidateWindowFrame();
				}
				else
				{
					Rectangle rectangle = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, _captionHeight);
					_headerRenderer.SetArea(rectangle);
				}
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			RebuildColorScheme();
			RelayoutForFormState();
			StartCaptionWatch();
		}

		internal void RelayoutForFormState()
		{
			RecalculateLayout(IsFormMaximized());
		}

		internal void InvalidateWindowFrame()
		{
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.InvalidateNonClientArea();
			}
		}

		internal bool HasWindowMessageHandler()
		{
			return _ribbonWindowMessageHandler != null;
		}

		private void SyncHeaderFormHandle()
		{
			IntPtr formHandle = IntPtr.Zero;
			if (GetHostForm() != null && GetHostForm().IsHandleCreated)
			{
				formHandle = GetHostForm().Handle;
			}
			if (_headerRenderer == null)
			{
				return;
			}
			if (IsCaptionBarShown())
			{
				if (IsParentedToForm() && HandlesTitleBar)
				{
					_headerRenderer.SetFormHandle(formHandle);
				}
				else
				{
					_headerRenderer.SetFormHandle(IntPtr.Zero);
				}
			}
			else
			{
				_headerRenderer.SetFormHandle(formHandle);
			}
		}

		internal void RefreshInternalCaptionRow()
		{
			bool hasInternalCaptionRow = false;
			if (_showCaptionBar)
			{
				if (IsGlassCaptionActive())
				{
					hasInternalCaptionRow = true;
				}
				else if (!IsParentedToForm() || !HandlesTitleBar)
				{
					hasInternalCaptionRow = true;
				}
				else if (Dock != DockStyle.Top || base.Location.Y != 0)
				{
					hasInternalCaptionRow = true;
				}
			}
			if (hasInternalCaptionRow != _hasInternalCaptionRow)
			{
				if (_ribbonWindowMessageHandler == null)
				{
					CreateCaptionHandler(true, false);
				}
				RelayoutForFormState();
				InvalidateTabContent();
				InvalidateWindowFrame();
			}
			SyncHeaderFormHandle();
			if (_headerRenderer != null && hasInternalCaptionRow)
			{
				Rectangle rectangle = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, _captionHeight);
				_headerRenderer.SetArea(rectangle);
			}
			_hasInternalCaptionRow = hasInternalCaptionRow;
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			_formWatch?.Update();
			OnContainingFormChanged();
		}

		// The ribbon or one of its parents got a new parent, so the ribbon may now lie in another form.
		private void OnContainingFormChanged()
		{
			if (!base.DesignMode)
			{
				AttachToForm(RibbonDrawingUtils.FindParentForm(this));
				SetParentedToForm(base.Parent == GetHostForm() && base.Parent != null);
				RefreshInternalCaptionRow();
				RelayoutForFormState();
				ReconcileCaptionExtension(true);
			}
		}

		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);
			RefreshInternalCaptionRow();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (!_isAdjustingVisibility)
			{
				ApplyMinimumFormSize();
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.InvalidateLayout();
			}
			if (_ribbonWindowMessageHandler != null && GetHostForm() != null && !base.DesignMode)
			{
				if (IsGlassCaptionActive())
				{
					NativeMethods.SetWindowPos(GetHostForm().Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
					return;
				}
				GetHostForm().Width++;
				GetHostForm().Width--;
			}
		}

		private bool IsGlassAvailable()
		{
			bool compositionEnabled = false;
			NativeMethods.DwmIsCompositionEnabled(ref compositionEnabled);
			return compositionEnabled && !base.DesignMode;
		}

		protected override void SetVisibleCore(bool value)
		{
			if (!_isAdjustingVisibility)
			{
				_requestedVisible = value;
			}
			base.SetVisibleCore(value);
		}

		private void OnFormHandleDestroyed(object sender, EventArgs eventArgs)
		{
			if (_form != null)
			{
				NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
			}
		}

		private void OnFormHandleCreated(object sender, EventArgs eventArgs)
		{
			SyncHeaderFormHandle();
			if (_ribbonWindowMessageHandler != null)
			{
				if (IsInDesignMode())
				{
					_ribbonWindowMessageHandler.ReleaseHandle();
					_ribbonWindowMessageHandler.AssignHandle(GetHostForm().Handle);
					return;
				}
				NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
				_ribbonWindowMessageHandler.Dispose();
				_ribbonWindowMessageHandler = null;
			}
			CreateCaptionHandler(!IsInDesignMode(), true);
		}

		private void OnFormActivated(object sender, EventArgs eventArgs)
		{
			if (GetPopupMenu() == null)
			{
				SetKeyTipMode(false);
			}
		}

		private void OnFormDeactivate(object sender, EventArgs eventArgs)
		{
			StopKeyTipTimer();
			if (_ribbonWindowMessageHandler != null)
			{
				bool isWindowActive = false;
				_ribbonWindowMessageHandler.SetActive(isWindowActive);
			}
			if (GetPopupMenu() == null)
			{
				SetKeyTipMode(false);
			}
		}

		private void OnFormControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			if (eventArgs.Control is MdiClient)
			{
				RibbonMdiClientMessageHandler.Detach((MdiClient)eventArgs.Control);
			}
		}

		private void OnFormControlAdded(object sender, ControlEventArgs eventArgs)
		{
			if (!base.DesignMode && GetHostForm().IsHandleCreated && eventArgs.Control is MdiClient && IsParentedToForm())
			{
				RibbonMdiClientMessageHandler.Attach((MdiClient)eventArgs.Control);
			}
		}

		private void OnFormLocationChanged(object sender, EventArgs eventArgs)
		{
			if (_backstageMenuScrollableBase != null)
			{
				_backstageMenuScrollableBase.ApplyLayout();
			}
		}

		private void OnFormResize(object sender, EventArgs eventArgs)
		{
			ApplyMinimumFormSize();
			if (_backstageMenuScrollableBase != null)
			{
				_backstageMenuScrollableBase.ApplyLayout();
			}
		}

		private void ApplyMinimumFormSize()
		{
			_isAdjustingVisibility = true;
			if (base.DesignMode)
			{
				return;
			}
			if (GetHostForm() != null && GetHostForm().Visible && GetHostForm().WindowState != FormWindowState.Minimized)
			{
				if (MinimumFormSize.Width > 0 && MinimumFormSize.Width >= GetHostForm().Width)
				{
					base.Visible = false;
					return;
				}
				if (MinimumFormSize.Height > 0 && MinimumFormSize.Height >= GetHostForm().Height)
				{
					base.Visible = false;
					return;
				}
				base.Visible = _requestedVisible;
				if (GetHostForm().IsMdiChild && IsTabStripHidden() && base.Height > 0)
				{
					RelayoutForFormState();
				}
			}
			_isAdjustingVisibility = false;
		}

		internal bool IsShownOnVisibleForm()
		{
			if (base.Visible && GetHostForm() != null && GetHostForm().Visible)
			{
				return true;
			}
			return false;
		}

		private void OnFormSizeChanged(object sender, EventArgs eventArgs)
		{
			if (_activeMdiChild != null)
			{
				SetMdiChildMaximized(_activeMdiChild.WindowState == FormWindowState.Maximized);
			}
			else
			{
				SetMdiChildMaximized(false);
			}
		}
	}
}
