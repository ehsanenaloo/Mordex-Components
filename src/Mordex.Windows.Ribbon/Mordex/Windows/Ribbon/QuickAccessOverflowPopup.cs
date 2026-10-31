using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessOverflowPopup : CommandBaseEditorDialog
	{
		private int _itemHeight;

		internal override CommandBaseRenderer FocusItem
		{
			get
			{
				return _focusedItem;
			}
			set
			{
				if (_focusedItem == value)
				{
					return;
				}
				if (_focusedItem != null)
				{
					if (value == null && GetChildMenu() != null && _focusedItem == GetChildMenu()._sourceItem)
					{
						return;
					}
					_focusedItem.Hot = false;
					if (_focusedItem.GetCommand() is IMouseEventInvoker)
					{
						((IMouseEventInvoker)_focusedItem.GetCommand()).InvokeMouseLeave();
					}
				}
				if (value != null)
				{
					if (value.GetCommand() is IMouseEventInvoker)
					{
						((IMouseEventInvoker)value.GetCommand()).InvokeMouseEnter();
					}
					if (!value.Focusable)
					{
						value = null;
					}
				}
				_focusedItem = value;
				if (_focusedItem != null)
				{
					if (_owner is CommandBaseEditorDialog)
					{
						((CommandBaseEditorDialog)_owner).FocusItem = null;
					}
					if (_focusedItem != GetSelectedItem())
					{
						if (GetChildMenu() != null && GetChildMenu()._sourceItem == _focusedItem)
						{
							SetSelectedItem(_focusedItem);
						}
						else
						{
							SetSelectedItem(null);
						}
					}
					_focusedItem.Hot = true;
				}
				UpdateSuperTip(_focusedItem);
				Invalidate();
			}
		}

		public QuickAccessOverflowPopup(MxRibbon ribbon, ICommandRenderHost owner, ArrayList layoutItems, CommandBaseRenderer item)
			: base(owner, null, item, null)
		{
			_ribbon = ribbon;
			_layoutItems = layoutItems;
			foreach (CommandBaseRenderer overflowItem in _layoutItems)
			{
				overflowItem.SetRenderHost(this);
			}
			_itemHeight = item._bounds.Height;
		}

		public override void RebuildMenuAndBounds(Graphics graphics)
		{
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
			int popupWidth = 4;
			int popupHeight = 4;
			popupWidth += 2;
			popupHeight += 2;
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				item.CalculateFullItemWidth(graphics, textRenderer, _ribbon.Font);
				popupWidth += item._fullItemWidth;
			}
			popupHeight += _itemHeight;
			Size size = new Size(popupWidth, popupHeight);
			Point position = GetPosition(size);
			base.Bounds = new Rectangle(position, size);
			LayoutMenuItems(graphics, textRenderer);
		}

		public override void LayoutMenuItems(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-3, -3);
			int itemLeft = 2;
			int itemTop = 2;
			itemLeft++;
			itemTop++;
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				item.ArrangeItem(graphics, null, itemLeft, itemTop, item._fullItemWidth, _itemHeight);
				itemLeft += item._bounds.Width;
			}
			if (FocusItem == null && _selectFirstItem && _layoutItems.Count > 0)
			{
				FocusItem = (CommandBaseRenderer)_layoutItems[0];
				_selectFirstItem = false;
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			if (_isDesignMode)
			{
				if (commandBaseRenderer is RibbonContextMenuRenderer)
				{
					commandBaseRenderer.OnMouseMove(eventArgs);
				}
				SetDesignHoverItem(commandBaseRenderer);
				return;
			}
			commandBaseRenderer?.OnMouseMove(eventArgs);
			if (commandBaseRenderer != FocusItem)
			{
				_showToolTipOnFocus = true;
				FocusItem = commandBaseRenderer;
				_showToolTipOnFocus = false;
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			_owner.MouseDownItem = commandBaseRenderer;
			if (commandBaseRenderer != null && eventArgs.Button == MouseButtons.Left)
			{
				commandBaseRenderer.Pushed = true;
			}
			commandBaseRenderer?.OnMouseDown(eventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			FindRendererAt(eventArgs.X, eventArgs.Y)?.OnMouseUp(eventArgs);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			DrawOffice2010Background(eventArgs.Graphics, _ribbon._colorScheme, base.ClientRectangle);
			if (_layoutItems == null || _layoutItems.Count <= 0)
			{
				return;
			}
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
			if (_isMenuBoundsDirty)
			{
				RebuildMenuAndBounds(eventArgs.Graphics);
			}
			if (_isLayoutDirty)
			{
				LayoutMenuItems(eventArgs.Graphics, textRenderer);
			}
			Color foreColor = ForeColor;
			if (GetRibbon() != null)
			{
				foreColor = GetRibbon().ForeColorDark;
			}
			foreach (CommandBaseRenderer item in _layoutItems)
			{
				item.Paint(eventArgs.Graphics, textRenderer, _owner.ColorScheme, Font, foreColor, eventArgs.ClipRectangle);
			}
		}

		private void DrawOffice2010Background(Graphics graphics, RibbonColorScheme colorScheme, Rectangle bounds)
		{
			ThemedRibbonPainter.Popup(graphics, colorScheme.Theme, bounds, ThemeRole.SurfaceRaised);
			return;
		}

		internal override IList<RibbonKeyTipEntry> GetKeyTips(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (GetChildMenu() != null)
			{
				return GetChildMenu().GetKeyTips(graphics, textRenderer);
			}
			IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
			int tipNumber = 1;
			string text = "";
			bool useLetters = false;
			string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			foreach (CommandBaseRenderer itemRenderer in _ribbon._quickAccessToolbarRenderer._itemRenderers)
			{
				if (_layoutItems.Contains(itemRenderer))
				{
					string tipText = (useLetters ? (text + letters.Substring(tipNumber, 1)) : (text + tipNumber));
					SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(_ribbon.Font, _ribbon.UseCompatibleTextRendering, tipText, _popupColorScheme);
					superTipSettingsWindowMessageHandler.SetCompactMode(true);
					superTipSettingsWindowMessageHandler.SetSticky(true);
					superTipSettingsWindowMessageHandler._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
					superTipSettingsWindowMessageHandler.ShowAt(_ribbon, PointToScreen(itemRenderer.GetKeyTipLocation(graphics, textRenderer, _ribbon.Font, tipText)));
					RibbonKeyTipEntry item = new RibbonKeyTipEntry(tipText, itemRenderer.GetCommand(), itemRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
					list.Add(item);
				}
				if (!useLetters && text.Length > 0)
				{
					tipNumber--;
					if (tipNumber <= 0)
					{
						useLetters = true;
					}
				}
				else
				{
					tipNumber++;
				}
				if (!useLetters && tipNumber > 9)
				{
					text = "0";
					tipNumber = 9;
				}
			}
			return list;
		}

		public override void HandleKeyUp()
		{
			if (FocusItem == null)
			{
				SelectNext();
			}
		}

		public override void HandleKeyDown()
		{
			if (FocusItem == null)
			{
				SelectNext();
			}
		}

		public override void HandleKeyLeft()
		{
			SelectPrevious();
		}

		public override bool HandleKeyRight(NativeMethods.NativeMessage msg)
		{
			SelectNext();
			return false;
		}
	}
}
