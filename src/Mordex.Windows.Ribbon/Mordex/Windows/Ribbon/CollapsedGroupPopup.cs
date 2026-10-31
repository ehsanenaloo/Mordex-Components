using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class CollapsedGroupPopup : CommandBaseEditorDialog
	{
		private RibbonGroup _group;

		private RibbonGroupBoxRenderer _ribbonRenderer;

		public CollapsedGroupPopup(ICommandRenderHost owner, CommandCollection commands, CommandBaseRenderer item, RibbonGroup group)
			: base(owner, commands, item, null)
		{
			_group = group;
		}

		public override void RebuildMenuAndBounds(Graphics graphics)
		{
			_layoutItems = new ArrayList();
			if (GetRibbon() != null)
			{
				using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
				int popupWidth = 2;
				int popupHeight = 0;
				_ribbonRenderer = new RibbonGroupBoxRenderer(_group, GetRibbon(), this);
				_ribbonRenderer.SetDrawsBackground(false);
				_ribbonRenderer.CalculateFullItemWidth(graphics, textRenderer, Font);
				_layoutItems.Add(_ribbonRenderer);
				popupWidth += _ribbonRenderer._fullItemWidth;
				int itemRowHeight = GetRibbon().GetSmallItemHeight();
				int captionHeight = Font.Height;
				if (captionHeight < 14)
				{
					captionHeight = 14;
				}
				int groupHeight = itemRowHeight * GetRibbon().ItemRows + captionHeight + GetRibbon().GetGroupsAreaPadding().Top + GetRibbon().GetGroupsAreaPadding().Bottom;
				groupHeight += 1 + GetRibbon().GetGroupBoxPadding().Top + GetRibbon().GetGroupBoxPadding().Bottom;
				popupHeight += groupHeight - GetRibbon().GetGroupsAreaPadding().Bottom;
				Rectangle workingArea = Screen.GetWorkingArea(_adjustedAnchorBounds);
				if (workingArea.Width < popupWidth)
				{
					popupWidth = workingArea.Width;
				}
				Size size = new Size(popupWidth, popupHeight);
				Point position = GetPosition(size);
				base.Bounds = new Rectangle(position, size);
				LayoutMenuItems(graphics, textRenderer);
			}
		}

		public override void LayoutMenuItems(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			_contentBounds = base.ClientRectangle;
			_contentBounds.Inflate(-2, -2);
			int width = _contentBounds.Width;
			_ribbonRenderer.ArrangeItem(graphics, textRenderer, _contentBounds.Left, _contentBounds.Top, width, _contentBounds.Height);
			Rectangle clientRectangle = base.ClientRectangle;
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
			}
			else
			{
				if (_ribbonRenderer == null)
				{
					return;
				}
				foreach (CommandBaseRenderer item in _ribbonRenderer._childRenderers)
				{
					if (item.GetCommand() == cmd && item.HasChildren)
					{
						ShowChildMenu(item, selectItem: true);
					}
				}
			}
		}

		internal override IList<RibbonKeyTipEntry> GetKeyTips(Graphics graphics, TextLayoutRenderer textRenderer)
		{
			if (GetChildMenu() != null)
			{
				return GetChildMenu().GetKeyTips(graphics, textRenderer);
			}
			IList<RibbonKeyTipEntry> list = new List<RibbonKeyTipEntry>();
			if (GetRibbon() != null && _ribbonRenderer != null)
			{
				GetRibbon().SetKeyTipOwner(_ribbonRenderer.GetRibbonGroup());
				foreach (CommandBaseRenderer itemRenderer in _ribbonRenderer._childRenderers)
				{
					if (itemRenderer.GetCommand() is IKeyTipProvider)
					{
						IKeyTipProvider keyTipProvider = (IKeyTipProvider)itemRenderer.GetCommand();
						if (keyTipProvider.HasKeyTip)
						{
							Point keyTipLocation = itemRenderer.GetKeyTipLocation(graphics, textRenderer, Font, keyTipProvider.KeyTip);
							SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(Font, _owner.UseCompatibleTextRendering, keyTipProvider.KeyTip, GetRibbon()._colorScheme);
							superTipSettingsWindowMessageHandler.SetCompactMode(true);
							superTipSettingsWindowMessageHandler.SetSticky(true);
							superTipSettingsWindowMessageHandler._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
							superTipSettingsWindowMessageHandler.ShowAt(_owner.ParentControl, PointToScreen(keyTipLocation));
							RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipProvider.KeyTip, itemRenderer.GetCommand(), itemRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
							list.Add(item);
						}
					}
				}
			}
			return list;
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (GetRibbon() != null)
			{
				GetRibbon().SetKeyTipMode(false);
			}
			if (_ribbonRenderer != null)
			{
				_ribbonRenderer.OnMouseDown(eventArgs);
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (_ribbonRenderer != null)
			{
				_ribbonRenderer.OnMouseUp(eventArgs);
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			if (_ribbonRenderer != null)
			{
				_ribbonRenderer.OnMouseMove(eventArgs);
			}
		}

		private void DrawOffice2010Background(Graphics graphics, Rectangle bounds, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Popup(graphics, colorScheme.Theme, bounds, ThemeRole.ToolbarBack);
			return;
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (base.ClientRectangle.Width == 0 || base.ClientRectangle.Height == 0)
			{
				return;
			}
			DrawOffice2010Background(eventArgs.Graphics, base.ClientRectangle, _popupColorScheme);
			if (_layoutItems == null)
			{
				return;
			}
			using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
			if (_isLayoutDirty)
			{
				LayoutMenuItems(eventArgs.Graphics, textRenderer);
			}
			if (_ribbonRenderer != null)
			{
				Color foreColor = Color.Black;
				if (GetRibbon() != null)
				{
					foreColor = GetRibbon().ForeColorDark;
				}
				_ribbonRenderer.Paint(eventArgs.Graphics, textRenderer, _owner.ColorScheme, _owner.Font, foreColor, eventArgs.ClipRectangle);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_sourceItem is RibbonGroupBoxRenderer)
			{
				((RibbonGroupBoxRenderer)_sourceItem).SetPopupOpen(false);
			}
			base.Dispose(disposing);
		}
	}
}
