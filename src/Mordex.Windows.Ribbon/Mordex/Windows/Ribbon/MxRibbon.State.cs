using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		public static string GetRibbonVersionString()
		{
			string localPath = typeof(MxRibbon).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private BackstageMenuScrollableBase GetBackstageMenuScrollableBase()
		{
			return _backstageMenuScrollableBase;
		}

		internal bool IsContextualTabSelected()
		{
			if (_selectedTab != null && _selectedTab.GetTabOwner() is RibbonContextualTab)
			{
				return true;
			}
			return false;
		}

		internal CommandBaseRenderer GetKeyboardFocusRenderer()
		{
			return _keyboardFocusRenderer;
		}

		internal void SetKeyboardFocusRenderer(CommandBaseRenderer focusRenderer)
		{
			if (_keyboardFocusRenderer != focusRenderer)
			{
				if (_keyboardFocusRenderer != null)
				{
					_keyboardFocusRenderer.Active = false;
				}
				_keyboardFocusRenderer = focusRenderer;
				if (_keyboardFocusRenderer != null)
				{
					_keyboardFocusRenderer.Active = true;
				}
			}
		}

		internal bool ShouldShowAddNewItems()
		{
			return _showAddNewItems;
		}

		internal void SetShowAddNewItems(bool showAddNewItems)
		{
			if (_designer != null && GetPopupMenu() != null)
			{
				showAddNewItems = false;
			}
			if (_showAddNewItems != showAddNewItems && !_designerItemPressed)
			{
				_showAddNewItems = showAddNewItems;
				InvalidateFullLayout();
			}
		}

		internal ControlBoxMode GetEffectiveControlBoxMode()
		{
			if (_controlBoxMode == ControlBoxMode.Default)
			{
				return ControlBoxMode.BackstageMenu;
			}
			return _controlBoxMode;
		}

		internal bool IsBackstageOpen()
		{
			return _backstageMenuScrollableBase != null;
		}

		internal Icon ResolveControlBoxIcon()
		{
			if (_controlBoxIcon != null)
			{
				return _controlBoxIcon;
			}
			if (GetHostForm() != null && GetHostForm().ShowIcon)
			{
				return GetHostForm().Icon;
			}
			return null;
		}

		internal List<RibbonContextualTab> GetContextualTabList()
		{
			return _contextualTabList;
		}

		internal bool IsDarkColorScheme()
		{
			return _colorScheme.Theme.Theme.Kind == ThemeKind.Dark;
		}

		internal bool IsInDesignMode()
		{
			return base.DesignMode;
		}

		internal void SetFrameBorderSize(Size size)
		{
			_frameBorderSize = size;
		}

		internal bool IsMinimizedPopupOpen()
		{
			return _isMinimizedPopupOpen;
		}

		internal void SetMinimizedPopupOpen(bool isOpen)
		{
			if (_isMinimizedPopupOpen != isOpen)
			{
				_isMinimizedPopupOpen = isOpen;
				RefreshWindowFrame(false);
				Invalidate();
			}
		}

		internal int GetCaptionAreaWidth()
		{
			return _captionAreaWidth;
		}

		internal void SetCaptionAreaWidth(int captionAreaWidth)
		{
			_captionAreaWidth = captionAreaWidth;
		}

		internal int GetSmallItemHeight()
		{
			return _smallItemHeight;
		}

		internal object GetKeyTipOwner()
		{
			return _keyTipOwner;
		}

		internal void SetKeyTipOwner(object owner)
		{
			_keyTipOwner = owner;
		}

		internal CommandBaseRenderer GetMouseDownItem()
		{
			return _pressedRenderer;
		}

		internal CommandBaseRenderer GetFocusItem()
		{
			return _hoveredRenderer;
		}

		internal void HandleContextualTabAdded(RibbonContextualTab component, bool addToContainer)
		{
			if (addToContainer && base.Container != null && component.Container == null)
			{
				if (component.Name != null && component.Name.Length > 0)
				{
					base.Container.Add(component, component.Name);
				}
				else
				{
					base.Container.Add(component);
				}
				if (component.Key.Length == 0)
				{
					component.Key = component.Name;
				}
			}
			component.HookAllControlEvents();
			if (!component.Active && !base.DesignMode)
			{
				if (GetHostForm() == null)
				{
					return;
				}
				if (component.Controls.Contains(GetHostForm()))
				{
					component.Active = true;
					return;
				}
				for (Control control = GetHostForm().ActiveControl; control != null; control = RibbonDrawingUtils.GetParentControl(control))
				{
					if (component.Controls.Contains(control))
					{
						component.Active = true;
						break;
					}
				}
			}
			else
			{
				SyncActiveContextualTab(component);
			}
		}

		internal void HandleContextualTabRemoved(RibbonContextualTab component, bool removeFromContainer)
		{
			if (removeFromContainer && component.Container != null)
			{
				component.Container.Remove(component);
			}
			component.UnhookAllControlEvents();
			if (!component.IsMerged())
			{
				component.Active = false;
			}
			if (_contextualTabList.Contains(component))
			{
				_contextualTabList.Remove(component);
				_tabStripRenderer.MarkTabRenderersDirty();
				_headerRenderer.InvalidateLayout();
				RequestTabStripLayout(false);
				if (component.Tabs.Contains(_selectedTab))
				{
					SelectFirstVisibleTab();
				}
			}
			_tabStripRenderer.MarkTabRenderersDirty();
			if (_headerRenderer != null)
			{
				_headerRenderer.InvalidateLayout();
			}
			InvalidateFullLayout();
			RefreshWindowFrame(true);
		}

		internal bool ShouldKeepContextualTabActive(RibbonContextualTab ribbonContextualTab)
		{
			if (base.ContainsFocus)
			{
				return true;
			}
			if (GetHostForm() != null && !GetHostForm().ContainsFocus)
			{
				return true;
			}
			foreach (Control control in ribbonContextualTab.Controls)
			{
				if (control.ContainsFocus)
				{
					return true;
				}
			}
			return false;
		}

		private MxRibbon FindRibbonInForm(Form form)
		{
			if (form == null)
			{
				return null;
			}
			foreach (Control control in form.Controls)
			{
				if (control is MxRibbon ribbon)
				{
					return ribbon;
				}
			}
			return null;
		}

		internal MxRibbon GetMergedRibbon()
		{
			return _mergedRibbon;
		}

		internal void SetMergedRibbon(MxRibbon ribbon)
		{
			if (_mergedRibbon == ribbon)
			{
				return;
			}
			if (_mergeChangingHandler != null)
			{
				_mergeChangingHandler(this, EventArgs.Empty);
			}
			_isMergeInProgress = true;
			try
			{
				if (_mergedRibbon != null)
				{
					_mergedRibbon.RemoveMergeChangingHandler(OnMergedRibbonChanging);
					UndoMergedContent();
				}
				_mergedRibbon = ribbon;
				if (_mergedRibbon != null)
				{
					if (_mergedRibbon._mergeHost != null)
					{
						_mergedRibbon._mergeHost.UndoMergedContent();
					}
					ImportSourceRibbonContent(ribbon);
					_mergedRibbon._mergeHost = this;
					_mergedRibbon.AddMergeChangingHandler(OnMergedRibbonChanging);
				}
			}
			finally
			{
				_isMergeInProgress = false;
			}
			RelayoutForFormState();
			if (_headerRenderer != null)
			{
				_headerRenderer.RefreshHeader(false);
			}
			RefreshWindowFrame(false);
			if (_mergeChangedHandler != null)
			{
				_mergeChangedHandler(this, EventArgs.Empty);
			}
		}

		internal bool IsTabStripHidden()
		{
			return _isTabStripHidden;
		}

		internal void SetTabStripHidden(bool isTabStripHidden)
		{
			if (_isTabStripHidden == isTabStripHidden)
			{
				return;
			}
			_isTabStripHidden = isTabStripHidden;
			if (!_isTabStripHidden)
			{
				if (_ribbonWindowMessageHandler == null)
				{
					RelayoutForFormState();
					CreateCaptionHandler(false, false);
					RelayoutForFormState();
					if (_form != null && !_form.IsDisposed && !_form.Disposing)
					{
						NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
					}
				}
			}
			else if (IsGlassCaptionActive())
			{
				if (_form != null && !_form.IsDisposed && !_form.Disposing)
				{
					NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, RdwInvalidateFrame);
				}
				if (_ribbonWindowMessageHandler != null)
				{
					if (_form != null && _form.IsHandleCreated)
					{
						NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
					}
					_ribbonWindowMessageHandler.Dispose();
					_ribbonWindowMessageHandler = null;
				}
				if (_form != null && !_form.IsDisposed && !_form.Disposing)
				{
					NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, RdwInvalidateFrame);
				}
			}
			InvalidateFullLayout();
			RefreshWindowFrame(false);
		}
	}
}
