using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public abstract partial class DockPanelBase
	{
		public virtual Size GetMaximumSize()
		{
			Size result = _maximumSize;
			if (result.Width == -1)
			{
				result.Width = 0;
			}
			if (result.Height == -1)
			{
				result.Height = 0;
			}
			return result;
		}

		private Size GetMaximumSizeWithSplitter()
		{
			Size maximumSize = GetMaximumSize();
			if (IsSideDocked())
			{
				if (!_splitterBounds.IsEmpty && maximumSize.Width > 0)
				{
					maximumSize.Width += GetSplitterSize();
				}
			}
			else if (!_splitterBounds.IsEmpty && maximumSize.Height > 0)
			{
				maximumSize.Height += GetSplitterSize();
			}
			return maximumSize;
		}

		public virtual Size GetMinimumSize()
		{
			Size result = _minimumSize;
			if (result.Width == -1)
			{
				result.Width = DefaultMinimumPanelSize;
			}
			if (result.Height == -1)
			{
				result.Height = DefaultMinimumPanelSize;
			}
			return result;
		}

		private Size GetMinimumSizeWithSplitter()
		{
			Size minimumSize = GetMinimumSize();
			if (IsSideDocked())
			{
				if (!_splitterBounds.IsEmpty)
				{
					minimumSize.Width += GetSplitterSize();
				}
			}
			else if (!_splitterBounds.IsEmpty)
			{
				minimumSize.Height += GetSplitterSize();
			}
			return minimumSize;
		}

		internal void ApplySizeLimits()
		{
			if (!IsRootPanel() || DockStyle == PanelDockStyle.Fill)
			{
				return;
			}
			Size maxSize = GetMaximumSizeWithSplitter();
			Size minSize = GetMinimumSizeWithSplitter();
			Size newSize = Size;
			if (IsFloating() || IsSideDocked())
			{
				if (maxSize.Width > 0 && newSize.Width > maxSize.Width)
				{
					newSize.Width = maxSize.Width;
				}
				if (newSize.Width < minSize.Width)
				{
					newSize.Width = minSize.Width;
				}
			}
			if (IsFloating() || !IsSideDocked())
			{
				if (maxSize.Height > 0 && newSize.Height > maxSize.Height)
				{
					newSize.Height = maxSize.Height;
				}
				if (newSize.Height < minSize.Height)
				{
					newSize.Height = minSize.Height;
				}
			}
			Size = newSize;
		}

		internal void RecalculateLayout()
		{
			if (IsUpdateSuspended() || _isUpdateSuspended)
			{
				return;
			}
			if (GetAutoHideStrip() != null && GetAutoHidePanel() == null && _autoHideStrip != null)
			{
				GetAutoHideStrip().UpdateLayout();
			}
			if (!base.DesignMode)
			{
				Form form = RenderingHelper.FindForm(this);
				if (form != null && !form.IsHandleCreated)
				{
					return;
				}
			}
			_displayBounds = base.ClientRectangle;
			if (IsRootPanel() || AutoHide)
			{
				int splitterSize = GetSplitterSize();
				if (_dockState == PanelDockState.Floating && !AutoHide)
				{
					_splitterBounds = Rectangle.Empty;
				}
				else
				{
					switch (DockStyle)
					{
					case PanelDockStyle.Top:
						_displayBounds.Height -= splitterSize;
						_splitterBounds = new Rectangle(_displayBounds.X, _displayBounds.Bottom, _displayBounds.Width, splitterSize);
						break;
					case PanelDockStyle.Fill:
						_splitterBounds = Rectangle.Empty;
						break;
					case PanelDockStyle.Right:
						_splitterBounds = new Rectangle(_displayBounds.X, _displayBounds.Y, splitterSize, _displayBounds.Height);
						_displayBounds.X += splitterSize;
						_displayBounds.Width -= splitterSize;
						break;
					case PanelDockStyle.Left:
						_displayBounds.Width -= splitterSize;
						_splitterBounds = new Rectangle(_displayBounds.Right, _displayBounds.Y, splitterSize, _displayBounds.Height);
						break;
					case PanelDockStyle.Bottom:
						_splitterBounds = new Rectangle(_displayBounds.X, _displayBounds.Y, _displayBounds.Width, splitterSize);
						_displayBounds.Y += splitterSize;
						_displayBounds.Height -= splitterSize;
						break;
					}
				}
			}
			else
			{
				_splitterBounds = Rectangle.Empty;
			}
			if (HasBorderPanel)
			{
				_displayBounds.Inflate(-1, -1);
			}
			RefreshLayout();
		}

		protected virtual void RefreshLayout()
		{
			if (PanelManager == null || !base.Visible)
			{
				return;
			}
			_innerAreaBounds = DisplayRectangle;
			if (_useContainerCaption)
			{
				AttachContainerCaption();
			}
			else
			{
				DetachContainerCaption();
			}
			if (HasCaption)
			{
				_captionRenderer = new DockCaptionRenderer(this);
				_innerAreaBounds.Y += _captionRenderer.GetCaptionBounds().Height - 1;
				_innerAreaBounds.Height -= _captionRenderer.GetCaptionBounds().Height - 1;
				if (_containerCaptionControl != null && _useContainerCaption)
				{
					_containerCaptionControl._isSettingBounds = true;
					_containerCaptionControl.Bounds = _captionRenderer.GetCaptionBounds();
					_containerCaptionControl._isSettingBounds = false;
					_containerCaptionControl.Invalidate();
				}
			}
			else
			{
				_captionRenderer = null;
			}
			InvalidateNonChildArea();
		}

		internal Rectangle GetInnerAreaBounds()
		{
			return _innerAreaBounds;
		}

		internal int GetTileSize()
		{
			return GetCurrentLayoutState().GetAncestorUnder(GetParentContainer().GetDockLayoutState(_dockState))?.GetTileSize() ?? GetCurrentLayoutState().GetTileSize();
		}

		internal void SetTileSize(int tileSize)
		{
			DockLayoutState dockLayoutState = GetCurrentLayoutState().GetAncestorUnder(GetParentContainer().GetDockLayoutState(_dockState));
			if (dockLayoutState != null)
			{
				dockLayoutState.SetTileSize(tileSize);
			}
			else
			{
				GetCurrentLayoutState().SetTileSize(tileSize);
			}
		}

		internal void InvalidateNonChildArea()
		{
			Region region = new Region(base.ClientRectangle);
			foreach (Control control in Controls)
			{
				region.Exclude(control.Bounds);
			}
			Invalidate(region, invalidateChildren: false);
			region.Dispose();
		}

		public virtual Size CalculateSize(Size clientSize)
		{
			Size result = clientSize;
			if (IsRootPanel() && IsFloating())
			{
				switch (DockStyle)
				{
				case PanelDockStyle.Top:
				case PanelDockStyle.Bottom:
					result.Height += GetSplitterSize();
					break;
				case PanelDockStyle.Left:
				case PanelDockStyle.Right:
					result.Width += GetSplitterSize();
					break;
				}
			}
			if (HasBorderPanel)
			{
				result.Width += 2;
				result.Height += 2;
			}
			if (_captionRenderer != null)
			{
				result.Height += _captionRenderer.GetCaptionBounds().Height - 1;
			}
			return result;
		}
	}
}
