using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class MinimizedRibbonPopup : CommandBaseEditorDialog
	{
		private RibbonTabContentRenderer _tabContentRenderer;

		public MinimizedRibbonPopup(MxRibbon ribbon)
			: base(ribbon, null, null, null)
		{
			_ribbon = ribbon;
			_tabContentRenderer = _ribbon._tabContentRenderer;
		}

		protected override Point GetPosition(Size winSize)
		{
			_adjustedAnchorBounds = _anchorScreenBounds;
			Rectangle workingArea = Screen.GetWorkingArea(_adjustedAnchorBounds);
			Point location = _adjustedAnchorBounds.Location;
			if (!_locationOverride.IsEmpty)
			{
				location = _locationOverride;
			}
			if (_isFlipped && !_opensVertically)
			{
				location.X -= _adjustedAnchorBounds.Width;
			}
			if (_isRightToLeft && _opensVertically)
			{
				location.X = _adjustedAnchorBounds.Right - winSize.Width;
				if (location.X < workingArea.X)
				{
					location.X = workingArea.X;
				}
			}
			if (_opensVertically)
			{
				_isFlipped = false;
				if (location.Y + winSize.Height + _adjustedAnchorBounds.Height > workingArea.Bottom && workingArea.Bottom - _adjustedAnchorBounds.Bottom < _adjustedAnchorBounds.Top - workingArea.Top)
				{
					_isFlipped = true;
				}
				if (!_isFlipped)
				{
					if (_sourceItem != null)
					{
						location.Y = _adjustedAnchorBounds.Bottom;
					}
					_maxPopupHeight = workingArea.Bottom - _adjustedAnchorBounds.Bottom;
				}
				else
				{
					if (_upwardOffset > 0)
					{
						_adjustedAnchorBounds.Y -= _upwardOffset;
					}
					if (_sourceItem != null)
					{
						location.Y = _adjustedAnchorBounds.Top - winSize.Height;
					}
					else
					{
						location.Y = _adjustedAnchorBounds.Top - winSize.Height - _anchorSize.Height;
					}
					if (location.Y < 0)
					{
						winSize.Height = location.Y;
						location.Y = 0;
					}
					if (location.Y < workingArea.Top)
					{
						location.Y = workingArea.Top;
					}
					_maxPopupHeight = _adjustedAnchorBounds.Top - workingArea.Top;
				}
			}
			else
			{
				if (location.X + winSize.Width + _adjustedAnchorBounds.Width > workingArea.Right && workingArea.Right - _adjustedAnchorBounds.Right < _adjustedAnchorBounds.X - workingArea.X)
				{
					_isFlipped = true;
				}
				if (!_isFlipped)
				{
					location.X = _adjustedAnchorBounds.Right;
				}
				else
				{
					location.X = _adjustedAnchorBounds.Left - winSize.Width;
					if (location.X < workingArea.Left)
					{
						location.X = workingArea.Left;
					}
				}
				if (_adjustedAnchorBounds.Top + winSize.Height > workingArea.Bottom)
				{
					location.Y = workingArea.Bottom - winSize.Height;
				}
				_maxPopupHeight = workingArea.Height;
				if (location.Y < workingArea.Top)
				{
					location.Y = workingArea.Top;
				}
			}
			return location;
		}

		public override void RebuildMenuAndBounds(Graphics graphics)
		{
			int rowHeight = _ribbon.SmallImageSize.Height;
			if (_ribbon.Font.Height + 1 > rowHeight)
			{
				rowHeight = Font.Height + 1;
			}
			rowHeight += 6;
			int groupLabelHeight = Font.Height + 1;
			int height = rowHeight * _ribbon.ItemRows + 1 + groupLabelHeight + 4 + 7;
			_adjustedAnchorBounds = _anchorScreenBounds;
			Rectangle workingArea = Screen.GetWorkingArea(_adjustedAnchorBounds);
			int width = _ribbon.ClientRectangle.Width;
			if (_adjustedAnchorBounds.Location.X + _ribbon.ClientRectangle.Width > workingArea.Right)
			{
				width = workingArea.Right - _adjustedAnchorBounds.Location.X;
			}
			Size size = new Size(width, height);
			Point position = GetPosition(size);
			base.Bounds = new Rectangle(position, size);
			LayoutMenuItems(graphics, null);
		}

		public override void LayoutMenuItems(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			_tabContentRenderer.RenderContent(graphics, base.ClientRectangle);
			_isLayoutDirty = false;
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			_tabContentRenderer.HandleMouseDown(eventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			_tabContentRenderer.HandleMouseUp(eventArgs);
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			_tabContentRenderer.HandleMouseMove(eventArgs);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (_isLayoutDirty)
			{
				using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
				LayoutMenuItems(eventArgs.Graphics, textRenderer);
			}
			_tabContentRenderer.DrawContent(eventArgs.Graphics, eventArgs.ClipRectangle);
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			_tabContentRenderer.ClearHotItems();
		}

		internal override IList<RibbonKeyTipEntry> GetKeyTips(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (GetChildMenu() != null)
			{
				return GetChildMenu().GetKeyTips(graphics, textRenderer);
			}
			IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
			foreach (RibbonGroupBoxRenderer item in _tabContentRenderer._groupRenderers)
			{
				((IKeyboardNavigable)item).AddKeyTips(graphics, textRenderer, Font, list, showToolTip: true);
			}
			return list;
		}

		public override void OpenDropDown(DropDownCommand cmd)
		{
			if (cmd.GetParentContainer() is DropDownCommand && cmd.GetParentContainer() != GetParentMenuCommand())
			{
				OpenDropDown(cmd.GetParentContainer() as DropDownCommand);
				if (GetChildMenu() != null)
				{
					GetChildMenu().OpenDropDown(cmd);
				}
				return;
			}
			foreach (RibbonGroupBoxRenderer item in _tabContentRenderer._groupRenderers)
			{
				foreach (CommandBaseRenderer commandItem in item._childRenderers)
				{
					if (commandItem.GetCommand() == cmd)
					{
						if (commandItem is IDropDownItem)
						{
							((IDropDownItem)commandItem).DropDown(selectFirstItem: true);
							return;
						}
						break;
					}
				}
			}
		}

		protected override void OnWMMouseDown(NativeMethods.NativeMessage msg)
		{
			Point position = Cursor.Position;
			Point pt = _ribbon.PointToClient(position);
			RibbonTabRenderer ribbonTabRenderer = _ribbon._tabStripRenderer.GetTabAt(pt.X, pt.Y);
			if (ribbonTabRenderer != null && ribbonTabRenderer.GetTab() != null && _ribbon.SelectedTab != ribbonTabRenderer.GetTab())
			{
				MarkLayoutDirty();
			}
			if (_ribbon.ClientRectangle.Contains(pt))
			{
				if (_ribbon._headerRenderer == null || !_ribbon._headerRenderer.GetArea().Contains(pt))
				{
					return;
				}
				base.OnWMMouseDown(msg);
			}
			base.OnWMMouseDown(msg);
		}

		protected override void OnWMMouseUp(NativeMethods.NativeMessage msg)
		{
			Point position = Cursor.Position;
			Point point = _ribbon.PointToClient(position);
			RibbonTabRenderer ribbonTabRenderer = _ribbon._tabStripRenderer.GetTabAt(point.X, point.Y);
			if (ribbonTabRenderer == null || ribbonTabRenderer.GetTab() == null)
			{
				base.OnWMMouseUp(msg);
			}
		}

		protected override ContainerControlCommand GetParentCommand(Control childControl, ref bool exitLoop)
		{
			ArrayList arrayList = new ArrayList();
			if (_tabContentRenderer != null)
			{
				foreach (RibbonGroupBoxRenderer item in _tabContentRenderer._groupRenderers)
				{
					arrayList.Add(item);
				}
			}
			return FindContainerControlCommand(childControl, ref exitLoop, arrayList);
		}
	}
}
