using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			Control control = RenderingHelper.GetParentControl(this);
			if (control != null)
			{
				if (!(control is MxCommandBarHost) && !(control is CommandBarEditorDialog))
				{
					throw new InvalidOperationException();
				}
				if (control != null && control.Visible)
				{
					_isClosed = !base.Visible;
				}
			}
			Form form = RenderingHelper.FindForm(this);
			SetParentForm(form);
		}

		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);
			if (_isInitializing && _dockStyle != BarsDockStyle.Floating)
			{
				SetPosition(Location);
			}
			if (_host != null)
			{
				_host.InvalidateBarLayouts();
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (_floatingWindow != null)
			{
				_floatingWindow.InvalidateLayout();
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (_isHorizontal)
			{
				if (base.Width < GetMinimumLength())
				{
					base.Width = GetMinimumLength();
					return;
				}
			}
			else if (base.Height < GetMinimumLength())
			{
				base.Height = GetMinimumLength();
				return;
			}
			if (GetFloatingWindow() != null)
			{
				_floatingWindow.InvalidateLayout();
			}
			else
			{
				InvalidateLayout();
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			ClearHotTracking();
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			if (Controls.Count != 0)
			{
				return;
			}
			foreach (MxCommand item in _commands)
			{
				if (item.Control is MxComboBox)
				{
					item.Control.BindingContext = BindingContext;
				}
			}
		}

		internal void ClearHotTracking()
		{
			if (_mdiButtons != null)
			{
				_mdiButtons.SetHoveredButton(null);
			}
			SetHotItem(null);
			if (!base.ClientRectangle.Contains(PointToClient(Cursor.Position)))
			{
				HideToolTip();
			}
			if (_gripItem != null)
			{
				_gripItem.SetHot(false);
			}
			if (_customizeItem != null)
			{
				_customizeItem.SetHot(false);
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			InvalidateLayout();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			if (GetFloatingWindow() != null)
			{
				_floatingWindow.InvalidateLayout();
			}
			else
			{
				InvalidateLayout();
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (_isCreatingFloatingWindow)
			{
				return;
			}
			Control control = RenderingHelper.GetParentControl(this);
			if (control != null && control.Visible)
			{
				_isClosed = !base.Visible;
			}
			else if (control == null && _dockStyle == BarsDockStyle.Floating)
			{
				_isClosed = !base.Visible;
			}
			if (_dockStyle == BarsDockStyle.Floating && !_isLoadingLayout)
			{
				if (base.Visible && control == null)
				{
					if (CommandManager != null && _floatingWindow == null)
					{
						_isCreatingFloatingWindow = true;
						CommandBarEditorDialog uiCommandBarEditorDialog = new CommandBarEditorDialog(this);
						Size = FloatingSize;
						_floatingWindow = uiCommandBarEditorDialog;
						uiCommandBarEditorDialog.ShowAt(FloatingLocation);
						_isCreatingFloatingWindow = false;
						RecalculateLayout();
					}
				}
				else if (!base.Visible && control != null && control.Visible && GetFloatingWindow() != null)
				{
					SetFloatingWindow(null);
				}
			}
			else if (GetHost() != null)
			{
				GetHost().LayoutRows();
				if (base.Visible)
				{
					RecalculateLayout(true);
				}
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			SetHotItem(null);
			if (GetPressedItem() != null)
			{
				GetPressedItem().SetPressed(false);
				GetPressedItem().SetHot(false);
			}
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			SetHotItem(null);
			if (GetPressedItem() != null)
			{
				GetPressedItem().SetPressed(false);
				GetPressedItem().SetHot(false);
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (GetHotItem() != null || _layoutItems == null || _layoutItems.Count <= 0)
			{
				return;
			}
			foreach (FormatStyleLayoutState item in _layoutItems)
			{
				if (item is CommandLayoutState uiCommandLayoutState)
				{
					SetHotItem(uiCommandLayoutState);
					CommandManager.SetActiveBar(this);
					break;
				}
			}
		}
	}
}
