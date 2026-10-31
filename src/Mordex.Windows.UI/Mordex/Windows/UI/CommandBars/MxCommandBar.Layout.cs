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
		internal void SaveLayoutState()
		{
			_savedWrappable = _commandBarWrappable;
			if (_savedRowIndex == -1)
			{
				_savedRowIndex = RowIndex;
			}
		}

		internal bool RestoreLayoutState()
		{
			bool result = false;
			if (_savedWrappable != _commandBarWrappable)
			{
				Wrappable = _savedWrappable;
				result = true;
			}
			if (_savedRowIndex >= 0)
			{
				if (_savedRowIndex != RowIndex)
				{
					UpdateRowIndex(_savedRowIndex);
					result = true;
				}
				_savedRowIndex = -1;
			}
			return result;
		}

		internal void LayoutVertical(Graphics graphics, TextLayoutRenderer textLayoutRenderer, StateStyles uiStateStyles, bool useMaxExtent)
		{
			if (_hostRow._bounds.Height <= 0)
			{
				return;
			}
			int itemTop = base.ClientRectangle.Top + 1;
			int left = base.ClientRectangle.Left;
			int rowBottom = GetHostRow()._bounds.Height;
			int extraLength = 2;
			if (useMaxExtent)
			{
				rowBottom = GetHostRow().GetMaxAvailableExtent(this);
			}
			else if (!GetHostRow().HasDraggedBar())
			{
				rowBottom = Location.Y + base.Height;
			}
			if (base.Top + GetMinimumLength() > rowBottom)
			{
				int newTop = rowBottom - GetMinimumLength();
				int minTop = GetHostRow().ComputeMinOffsetY(this);
				if (newTop >= minTop)
				{
					MoveToTop(rowBottom - GetMinimumLength());
					return;
				}
			}
			rowBottom--;
			_layoutItems.ClearItems();
			if (CanMove())
			{
				if (_gripItem == null)
				{
					_gripItem = new FormatStyleLayoutState(this);
				}
				_gripItem._bounds = new Rectangle(left, itemTop, base.ClientRectangle.Width - 2, GripSize);
				itemTop += GripSize;
				extraLength += GripSize;
			}
			else
			{
				_gripItem = null;
			}
			int customizeLength = GetCustomizeButtonLength();
			if (ShouldShowCustomizeButton())
			{
				rowBottom -= customizeLength;
				extraLength += customizeLength;
			}
			_isFullShown = true;
			LayoutVerticalCommands(graphics, textLayoutRenderer, uiStateStyles, left, itemTop, rowBottom - base.Top, extraLength, customizeLength, useMaxExtent);
			_isLayoutDirty = false;
			if (_gripItem != null)
			{
				_gripItem._bounds.Width = GetThickness() - 2;
			}
			if (GetHostRow() != null && !GetHostRow().IsRowFilled())
			{
				if (base.Top + base.Height > GetHostRow()._bounds.Bottom)
				{
					MoveToTop(GetHostRow()._bounds.Bottom - base.Height);
				}
				else if (!_isFullShown && base.Top + GetFullLength() > GetHostRow()._bounds.Bottom)
				{
					MoveToTop(base.Top);
				}
			}
			if (GetHotItem() != null)
			{
				SetHotItem(_layoutItems.FindCommandItem(GetHotItem().GetCommand()));
			}
		}

		private void LayoutPopupControl()
		{
			_gripItem = null;
			Control control = GetSourceCommand().Control;
			_isLayoutDirty = false;
			if (control is MxColorPicker)
			{
				MxColorPicker uiColorPicker = (MxColorPicker)control;
				uiColorPicker.BackColor = Color.Transparent;
				uiColorPicker.RecalculateLayout();
			}
			if (!Controls.Contains(control))
			{
				Controls.Add(control);
				control.Resize += OnControlResize;
			}
			control.Top = base.ClientRectangle.Top;
			control.Left = base.ClientRectangle.Left;
			_isLayoutDirty = true;
			Size = new Size(control.Width, control.Height);
			_isHostingPopupControl = true;
		}

		private void PlaceHostedControl(CommandLayoutState uiCommandLayoutState)
		{
			Control control = uiCommandLayoutState.GetCommand().Control;
			int x = uiCommandLayoutState._bounds.Right - control.Width - 2;
			int y = uiCommandLayoutState._bounds.Y + (uiCommandLayoutState._bounds.Height - control.Height) / 2;
			Rectangle bounds = new Rectangle(x, y, control.Width, control.Height);
			control.Enabled = ((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled();
			control.Bounds = bounds;
			if (!Controls.Contains(control))
			{
				HookHostedControl(control);
				Controls.Add(control);
			}
			if (control is MxFontPicker)
			{
				((MxFontPicker)control).UseFontInFontNames = CommandManager.UseFontInFontNames;
			}
		}

		private void LayoutHorizontalCommands(Graphics graphics, TextLayoutRenderer textLayoutRenderer, StateStyles uiStateStyles, int itemLeft, int top, int endPosition, int width, int customizeWidth, bool useMaxExtent)
		{
			int maxHeight = GetDefaultThickness();
			int spaceLeft = endPosition - itemLeft;
			int nearEdge = itemLeft;
			int farEdge = base.Width - 1;
			if (ShouldShowCustomizeButton())
			{
				farEdge -= customizeWidth;
			}
			int usedWidth = 0;
			if (_commands.Count > 0)
			{
				FormatStyle formatStyle = uiStateStyles.FormatStyle;
				FormatStyle uiFormatStyle = null;
				bool isFitting = true;
				bool canAddSeparator = false;
				foreach (MxCommand item in _commands)
				{
					if (item.IsEffectivelyVisible())
					{
						if (item.IsSeparator())
						{
							if (!canAddSeparator)
							{
								continue;
							}
							canAddSeparator = false;
						}
						else
						{
							canAddSeparator = true;
						}
						CommandLayoutState uiCommandLayoutState = new CommandLayoutState(item, item.GetEffectiveText());
						((FormatStyleLayoutState)uiCommandLayoutState).SetEnabled((item.IsEnabled && base.Enabled) || CommandManager._isCustomizing);
						uiFormatStyle = formatStyle;
						if (!item.GetEffectiveStateStyles().FormatStyle.IsDefaultStyle())
						{
							uiFormatStyle = uiFormatStyle.MergeStyle(item.GetEffectiveStateStyles().FormatStyle);
						}
						Size size = LayoutHorizontalItem(graphics, uiCommandLayoutState, ref nearEdge, ref farEdge, top, uiCommandLayoutState.GetCommand().GetEffectiveAlignment(), textLayoutRenderer, uiFormatStyle);
						width += size.Width;
						if (isFitting)
						{
							spaceLeft -= size.Width;
							if (spaceLeft < 0 && (_layoutItems.Count > 0 || _hostRow._bounds.Width < size.Width))
							{
								usedWidth = endPosition - itemLeft;
								if (!uiCommandLayoutState.GetCommand().IsSeparator())
								{
									_overflowItems.AddItem(uiCommandLayoutState);
								}
								isFitting = false;
								if (uiCommandLayoutState.GetCommand().HasEmbeddedControl())
								{
									RemoveHostedControl(uiCommandLayoutState.GetCommand());
								}
								continue;
							}
							usedWidth += size.Width;
							if (uiCommandLayoutState._hostsControl)
							{
								PlaceHostedControl(uiCommandLayoutState);
							}
							if (size.Height > maxHeight)
							{
								maxHeight = size.Height;
							}
							uiCommandLayoutState.SetHasDropDown(item.IsParent);
							_layoutItems.AddItem(uiCommandLayoutState);
						}
						else if (!uiCommandLayoutState.GetCommand().IsSeparator())
						{
							if (uiCommandLayoutState.GetCommand().HasEmbeddedControl())
							{
								RemoveHostedControl(uiCommandLayoutState.GetCommand());
							}
							uiCommandLayoutState.SetHasDropDown(item.IsParent);
							_overflowItems.AddItem(uiCommandLayoutState);
						}
						uiFormatStyle.DisposeResources();
					}
					else if (item.HasEmbeddedControl())
					{
						RemoveHostedControl(item);
					}
				}
				if (_layoutItems.Count > 0)
				{
					CommandLayoutState lastItem = (CommandLayoutState)_layoutItems.FindItemByIndex(_layoutItems.Count - 1);
					if (lastItem.GetCommand().IsSeparator())
					{
						_layoutItems.RemoveItem(lastItem);
					}
				}
				if (_overflowItems != null && _overflowItems.Count > 0)
				{
					_isFullShown = false;
				}
				if ((Size.Height < maxHeight + 2 || _layoutItems.Count > 0) && GetThickness() != maxHeight + 2)
				{
					SetThickness(maxHeight + 2);
					return;
				}
				itemLeft = (IsFullRowLayout() ? endPosition : (itemLeft + usedWidth));
				if (ShouldShowCustomizeButton())
				{
					if (_customizeItem == null)
					{
						_customizeItem = new CommandLayoutState(GetCustomizeCommand(), "", this);
					}
					if (_layoutItems.Count > 0)
					{
						if (GetHostRow() != null)
						{
							int minimumWidth = GetHostRow().GetDesiredBarExtent(this);
							int margin = 2;
							if (itemLeft + maxHeight + margin < minimumWidth)
							{
								itemLeft = minimumWidth - maxHeight - margin;
							}
						}
					}
					else
					{
						itemLeft = GetMinimumLength() - customizeWidth;
					}
					_customizeItem._bounds = new Rectangle(itemLeft, top + 2, customizeWidth, GetThickness() - 4);
					itemLeft += customizeWidth;
				}
				else
				{
					_customizeItem = null;
				}
				int left = base.Left;
				SetFullLength(width);
				if (GetThickness() != Size.Height && !IsWrapping())
				{
					Size = new Size(itemLeft + 1, GetThickness());
					return;
				}
				Size = new Size(itemLeft + 1, GetThickness());
				if (base.Left != left && Size.Width != itemLeft + 1)
				{
					LayoutBar(graphics, textLayoutRenderer, uiStateStyles, useMaxExtent);
					return;
				}
				_isLayoutDirty = false;
				CenterItemsInRow(GetThickness(), _layoutItems, GetEffectiveTextAlign());
				formatStyle.DisposeResources();
				return;
			}
			SetThickness(GetDefaultThickness());
			if (ShouldShowCustomizeButton())
			{
				if (_customizeItem == null)
				{
					_customizeItem = new CommandLayoutState(GetCustomizeCommand(), "", this);
				}
				itemLeft = GetMinimumLength() - customizeWidth;
				_customizeItem._bounds = new Rectangle(itemLeft, top + 2, customizeWidth, GetThickness() - 4);
			}
			else
			{
				_customizeItem = null;
			}
			width = GetMinimumLength();
			SetFullLength(width);
			Size = new Size(width, GetThickness());
		}

		private void LayoutVerticalCommands(Graphics graphics, TextLayoutRenderer textLayoutRenderer, StateStyles uiStateStyles, int left, int itemTop, int endPosition, int height, int customizeHeight, bool useMaxExtent)
		{
			int maxWidth = GetDefaultThickness();
			int spaceLeft = endPosition - itemTop;
			int nearEdge = itemTop;
			int farEdge = base.Height - 1;
			if (ShouldShowCustomizeButton())
			{
				farEdge -= customizeHeight;
			}
			int usedHeight = 0;
			if (_commands.Count > 0)
			{
				FormatStyle formatStyle = uiStateStyles.FormatStyle;
				FormatStyle uiFormatStyle = null;
				bool isFitting = true;
				bool canAddSeparator = false;
				foreach (MxCommand item in _commands)
				{
					if (!item.IsEffectivelyVisible())
					{
						continue;
					}
					if (item.IsSeparator())
					{
						if (!canAddSeparator)
						{
							continue;
						}
						canAddSeparator = false;
					}
					else
					{
						canAddSeparator = true;
					}
					CommandLayoutState uiCommandLayoutState = new CommandLayoutState(item, item.GetEffectiveText(), this);
					((FormatStyleLayoutState)uiCommandLayoutState).SetEnabled((item.IsEnabled && base.Enabled) || CommandManager._isCustomizing);
					uiFormatStyle = formatStyle;
					if (!item.GetEffectiveStateStyles().FormatStyle.IsDefaultStyle())
					{
						uiFormatStyle = uiFormatStyle.MergeStyle(item.GetEffectiveStateStyles().FormatStyle);
					}
					Size size = LayoutVerticalItem(graphics, uiCommandLayoutState, ref nearEdge, ref farEdge, left, item.GetEffectiveAlignment(), textLayoutRenderer, uiFormatStyle);
					height += size.Height;
					if (isFitting)
					{
						spaceLeft -= size.Height;
						if (spaceLeft < 0 && (_layoutItems.Count > 0 || _hostRow._bounds.Height < size.Height))
						{
							usedHeight = endPosition - itemTop;
							if (!uiCommandLayoutState.GetCommand().IsSeparator())
							{
								_overflowItems.AddItem(uiCommandLayoutState);
							}
							isFitting = false;
							continue;
						}
						usedHeight += size.Height;
						if (size.Width > maxWidth)
						{
							maxWidth = size.Width;
						}
						uiCommandLayoutState.SetHasDropDown(item.IsParent);
						_layoutItems.AddItem(uiCommandLayoutState);
					}
					else if (!uiCommandLayoutState.GetCommand().IsSeparator())
					{
						if (uiCommandLayoutState.GetCommand().HasEmbeddedControl())
						{
							RemoveHostedControl(uiCommandLayoutState.GetCommand());
						}
						uiCommandLayoutState.SetHasDropDown(item.IsParent);
						_overflowItems.AddItem(uiCommandLayoutState);
					}
					uiFormatStyle.DisposeResources();
				}
				if (_layoutItems.Count > 0)
				{
					CommandLayoutState lastItem = (CommandLayoutState)_layoutItems.FindItemByIndex(_layoutItems.Count - 1);
					if (lastItem.GetCommand().IsSeparator())
					{
						_layoutItems.RemoveItem(lastItem);
					}
				}
				if (_overflowItems != null && _overflowItems.Count > 0)
				{
					_isFullShown = false;
				}
				if (Size.Width < maxWidth + 2 || _layoutItems.Count > 0)
				{
					SetThickness(maxWidth + 2);
				}
				itemTop = (IsFullRowLayout() ? endPosition : (itemTop + usedHeight));
				if (ShouldShowCustomizeButton())
				{
					if (_customizeItem == null)
					{
						_customizeItem = new CommandLayoutState(GetCustomizeCommand(), "", this);
					}
					if (_layoutItems.Count > 0)
					{
						if (GetHostRow() != null)
						{
							int minimumHeight = GetHostRow().GetDesiredBarExtent(this);
							int margin = 2;
							if (itemTop + customizeHeight + margin < minimumHeight)
							{
								itemTop = minimumHeight - customizeHeight - margin;
							}
						}
					}
					else
					{
						itemTop = GetMinimumLength() - customizeHeight;
					}
					_customizeItem._bounds = new Rectangle(left + 2, itemTop, GetThickness() - 4, customizeHeight);
					itemTop += customizeHeight;
				}
				else
				{
					_customizeItem = null;
				}
				int top = base.Top;
				SetFullLength(height);
				Size = new Size(GetThickness(), itemTop + 1);
				if (base.Top != top && Size.Height != itemTop + 1)
				{
					LayoutVertical(graphics, textLayoutRenderer, uiStateStyles, useMaxExtent);
					return;
				}
				_isLayoutDirty = false;
				CenterItemsInColumn(GetThickness(), _layoutItems, GetEffectiveTextAlign());
				formatStyle.DisposeResources();
				return;
			}
			SetThickness(GetDefaultThickness());
			if (ShouldShowCustomizeButton())
			{
				if (_customizeItem == null)
				{
					_customizeItem = new CommandLayoutState(GetCustomizeCommand(), "", this);
				}
				itemTop = GetMinimumLength() - customizeHeight;
				_customizeItem._bounds = new Rectangle(left + 2, itemTop, GetThickness() - 4, customizeHeight);
			}
			else
			{
				_customizeItem = null;
			}
			height = GetMinimumLength();
			SetFullLength(height);
			Size = new Size(GetThickness(), height);
		}

		private void LayoutBar(Graphics graphics, TextLayoutRenderer textLayoutRenderer, StateStyles uiStateStyles, bool useMaxExtent)
		{
			_isLayoutDirty = false;
			_rows = null;
			_overflowItems = new MxCollection(this);
			_isHostingPopupControl = false;
			if (_sourceCommand != null && _host == null && GetSourceCommand().Control != null && (GetSourceCommand().CommandType == CommandType.ColorPickerCommand || GetSourceCommand().CommandType == CommandType.ControlPopup))
			{
				LayoutPopupControl();
				_isLayoutDirty = false;
			}
			else if (GetHostRow() == null || IsWrapping())
			{
				LayoutWrapped(graphics, textLayoutRenderer, uiStateStyles);
				_isLayoutDirty = false;
			}
			else if (!IsHorizontalLayout())
			{
				LayoutVertical(graphics, textLayoutRenderer, uiStateStyles, useMaxExtent);
				_isLayoutDirty = false;
			}
			else
			{
				if (_hostRow._bounds.Width <= 0)
				{
					return;
				}
				int itemLeft = base.ClientRectangle.Left + 1;
				int top = base.ClientRectangle.Top;
				int rowRight = GetHostRow()._bounds.Width;
				int extraLength = 2;
				if (useMaxExtent)
				{
					rowRight = GetHostRow().GetMaxAvailableExtent(this);
				}
				else if (!GetHostRow().HasDraggedBar())
				{
					rowRight = Location.X + base.Width;
				}
				if (base.Left + GetMinimumLength() > rowRight)
				{
					int newLeft = rowRight - GetMinimumLength();
					int minLeft = GetHostRow().ComputeMinOffsetX(this);
					if (newLeft >= minLeft)
					{
						MoveToLeft(rowRight - GetMinimumLength());
						return;
					}
				}
				rowRight--;
				_layoutItems.ClearItems();
				if (CanMove())
				{
					if (_gripItem == null)
					{
						_gripItem = new FormatStyleLayoutState(this);
					}
					_gripItem._bounds = new Rectangle(itemLeft, top, GripSize, base.ClientRectangle.Height - 2);
					itemLeft += GripSize;
					extraLength += GripSize;
				}
				else
				{
					_gripItem = null;
				}
				int customizeLength = GetCustomizeButtonLength();
				if (ShouldShowCustomizeButton())
				{
					rowRight -= customizeLength;
					extraLength += customizeLength;
				}
				_isFullShown = true;
				LayoutHorizontalCommands(graphics, textLayoutRenderer, uiStateStyles, itemLeft, top, rowRight - base.Left, extraLength, customizeLength, useMaxExtent);
				_isLayoutDirty = false;
				if (_gripItem != null)
				{
					_gripItem._bounds.Height = GetThickness() - 2;
				}
				if (GetHostRow() != null && !GetHostRow().IsRowFilled())
				{
					if (base.Left + base.Width > GetHostRow()._bounds.Right)
					{
						MoveToLeft(GetHostRow()._bounds.Right - base.Width);
					}
					else if (!_isFullShown && base.Left + GetFullLength() > GetHostRow()._bounds.Right)
					{
						MoveToLeft(base.Left);
					}
				}
				if (GetHotItem() != null)
				{
					SetHotItem(_layoutItems.FindCommandItem(GetHotItem().GetCommand()));
				}
			}
		}

		internal void RemoveHostedControls()
		{
			foreach (Control control in Controls)
			{
				UnhookHostedControl(control);
			}
			Controls.Clear();
		}

		private void HookHostedControl(Control control)
		{
			control.Resize += OnControlResize;
			control.MouseEnter += OnControlMouseEnter;
			control.MouseLeave += OnControlMouseLeave;
			control.GotFocus += OnControlGotFocus;
			control.LostFocus += OnControlLostFocus;
		}

		private void UnhookHostedControl(Control control)
		{
			control.Resize -= OnControlResize;
			control.MouseEnter -= OnControlMouseEnter;
			control.MouseLeave -= OnControlMouseLeave;
			control.GotFocus -= OnControlGotFocus;
			control.LostFocus -= OnControlLostFocus;
		}

		private void RemoveHostedControl(MxCommand uiCommand)
		{
			Control control = uiCommand.Control;
			if (Controls.Contains(control))
			{
				UnhookHostedControl(control);
				try
				{
					Controls.Remove(control);
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// The hosted control may already be disposed; removing it is best effort.
				}
			}
		}
	}
}
