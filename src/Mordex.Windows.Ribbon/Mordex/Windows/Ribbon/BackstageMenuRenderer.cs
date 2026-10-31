using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageMenuRenderer : CommandContainerRenderer
	{
		private BackstageMenu _backStageMenu;

		internal bool _selectFirstOnLayout;

		public BackstageMenuRenderer(BackstageMenu backStageMenu, ICommandRenderHost owner)
			: base(null, owner, null)
		{
			_backStageMenu = backStageMenu;
			_commandBaseRendererRibbon = backStageMenu.Ribbon;
		}

		public CommandBaseRenderer GetSelectedRenderer()
		{
			if (GetRenderHost() != null)
			{
				return _commandBaseRendererOwner.SelectedItem;
			}
			return null;
		}

		public void SetSelectedRenderer(CommandBaseRenderer commandBaseRenderer)
		{
			if (GetRenderHost() != null)
			{
				GetRenderHost().SelectedItem = commandBaseRenderer;
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_childRenderers = new List<CommandBaseRenderer>();
			CommandBaseRenderer commandBaseRenderer = null;
			int maxItemWidth = 0;
			CommandBaseRenderer firstTabRenderer = null;
			foreach (CommandBase command in _backStageMenu.Commands)
			{
				if (command.Visible)
				{
					commandBaseRenderer = CreateLayoutItem(command);
					if (command is BackstageTab && firstTabRenderer == null)
					{
						firstTabRenderer = commandBaseRenderer;
					}
					commandBaseRenderer.CalculateFullItemWidth(graphics, textRenderer, font);
					maxItemWidth = Math.Max(commandBaseRenderer._fullItemWidth, maxItemWidth);
					_childRenderers.Add(commandBaseRenderer);
				}
			}
			if (base.GetHotRenderer() == null && _selectFirstOnLayout && _childRenderers.Count > 0)
			{
				base.SetHotRenderer(_childRenderers[0]);
				_selectFirstOnLayout = false;
			}
			if (_commandBaseRendererRibbon.IsInDesignMode())
			{
				BackstageMenuAddCommandRenderer ribbonContextMenuLayoutState = new BackstageMenuAddCommandRenderer(_commandBaseRendererRibbon, this);
				ribbonContextMenuLayoutState.CalculateFullItemWidth(graphics, textRenderer, font);
				maxItemWidth = Math.Max(ribbonContextMenuLayoutState._fullItemWidth, maxItemWidth);
				_childRenderers.Add(ribbonContextMenuLayoutState);
			}
			if (GetSelectedRenderer() == null)
			{
				SetSelectedRenderer(firstTabRenderer);
			}
			if (maxItemWidth == 0)
			{
				if (!_commandBaseRendererRibbon.IsInDesignMode() && _childRenderers.Count <= 0)
				{
					_fullItemWidth = 0;
					SetLayoutWidth(0);
					return;
				}
				maxItemWidth = 150;
			}
			_fullItemWidth = maxItemWidth + 12;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int barHeight)
		{
			int itemTop = top + 6;
			if (_childRenderers != null)
			{
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					int itemHeight = GetItemHeight(item, GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows) + 4;
					item.ArrangeItem(graphics, textRenderer, left, itemTop, width, itemHeight);
					if (itemHeight < GetRenderHost().SmallItemHeight + 4)
					{
						itemHeight = GetRenderHost().SmallItemHeight + 4;
					}
					itemTop += itemHeight + 2;
				}
			}
			_bounds = new Rectangle(left, top, _fullItemWidth, barHeight);
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			if (commandBaseRenderer != null && commandBaseRenderer.CommandType == CommandType.BackstageTab)
			{
				SetSelectedRenderer(commandBaseRenderer);
			}
			else
			{
				GetRenderHost().MouseDownItem = commandBaseRenderer;
			}
			if (commandBaseRenderer != null)
			{
				if (eventArgs.Button == MouseButtons.Left)
				{
					commandBaseRenderer.Pushed = true;
				}
				commandBaseRenderer.OnMouseDown(eventArgs);
				if (_commandBaseRendererRibbon != null && _commandBaseRendererRibbon.IsInDesignMode() && commandBaseRenderer.GetCommand() != null)
				{
					_commandBaseRendererRibbon.DesignerSelectComponent(commandBaseRenderer.GetCommand());
				}
			}
			else
			{
				GetRenderHost().MouseDownItem = this;
			}
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			FindRendererAt(eventArgs.X, eventArgs.Y)?.OnMouseUp(eventArgs);
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			if (eventArgs.Button != MouseButtons.None && commandBaseRenderer != GetRenderHost().MouseDownItem)
			{
				base.SetHotRenderer((CommandBaseRenderer)null);
				return;
			}
			base.SetHotRenderer(commandBaseRenderer);
			if (commandBaseRenderer != null)
			{
				commandBaseRenderer.OnMouseMove(eventArgs);
			}
			else if (_commandBaseRendererRibbon != null)
			{
				_commandBaseRendererRibbon.ShowSuperTip(null);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			ThemedRibbonPainter.BackstageSide(graphics, colorScheme.Theme, _bounds);
			foreColor = colorScheme.Theme.Color(ThemeRole.Text);
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				Rectangle rectangle = Rectangle.Intersect(clipRectangle, item._bounds);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					item.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
				}
			}
			if (_commandBaseRendererRibbon != null && _commandBaseRendererRibbon.IsInDesignMode())
			{
				RenderContent(graphics);
			}
		}

		protected void RenderContent(Graphics graphics)
		{
			if (_commandBaseRendererRibbon.Site == null)
			{
				return;
			}
			ISelectionService selectionService = (ISelectionService)_commandBaseRendererRibbon.Site.GetService(typeof(ISelectionService));
			if (selectionService == null)
			{
				return;
			}
			foreach (Component selectedComponent in selectionService.GetSelectedComponents())
			{
				if (!(selectedComponent is CommandBase))
				{
					continue;
				}
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					if (item._commandBaseRendererCommand == selectedComponent)
					{
						DrawSelectionBorder(graphics, item._bounds, DashStyle.Dot);
						break;
					}
				}
			}
		}

		private void DrawSelectionBorder(Graphics graphics, Rectangle rectangle, DashStyle dashStyle)
		{
			Pen pen = new Pen(Color.Black);
			pen.DashStyle = dashStyle;
			if (rectangle.Height <= 4)
			{
				rectangle.Y -= 2;
				rectangle.Height = 4;
			}
			RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, rectangle);
			pen.Dispose();
		}

		public override CommandBaseRenderer CreateLayoutItem(CommandBase command)
		{
			switch (command.CommandType)
			{
			case CommandType.LargeButton:
			case CommandType.SmallButton:
				return new BackstageMenuButtonRenderer((ButtonCommand)command, GetRenderHost(), this);
			case CommandType.BackstageTab:
				return new BackstageTabRenderer((BackstageTab)command, GetRenderHost(), this);
			default:
				return null;
			}
		}

		internal override void AddNewCommand(CommandBase command, bool editItem)
		{
			if (_backStageMenu != null)
			{
				using (DesignEditScope edit = CommandCollection.BeginDesignEdit(_backStageMenu.Commands, "Add Command"))
				{
					_backStageMenu.Commands.Add(command);
					edit.End();
				}
			}
		}

		public virtual void SelectPrevious()
		{
			if (_childRenderers == null)
			{
				return;
			}
			int previousIndex = -1;
			if (base.GetHotRenderer() != null)
			{
				previousIndex = _childRenderers.IndexOf(base.GetHotRenderer()) - 1;
			}
			else if (GetSelectedRenderer() != null)
			{
				previousIndex = _childRenderers.IndexOf(GetSelectedRenderer()) - 1;
			}
			int count = _childRenderers.Count;
			if (count <= 0)
			{
				return;
			}
			if (previousIndex < 0)
			{
				previousIndex = _childRenderers.Count - 1;
			}
			int targetIndex = 0;
			int scanIndex = previousIndex;
			while (scanIndex >= 0)
			{
				targetIndex = scanIndex;
				CommandBaseRenderer thisValue = _childRenderers[scanIndex];
				if (thisValue.CommandType == CommandType.Separator)
				{
					if (previousIndex != 0)
					{
						scanIndex--;
						continue;
					}
					thisValue = GetLastNonSeparatorItem();
					targetIndex = ((RibbonLayoutItem)thisValue).GetItemIndex();
					if (thisValue == null)
					{
						break;
					}
				}
				if (thisValue.CommandType == CommandType.BackstageTab)
				{
					SetSelectedRenderer(thisValue);
					if (GetRenderHost() != null && GetRenderHost().ParentControl != null)
					{
						RibbonDrawingUtils.TryFocusControl(GetRenderHost().ParentControl);
					}
				}
				base.SetHotRenderer(_childRenderers[targetIndex]);
				break;
			}
		}

		public virtual void SelectNext()
		{
			if (_childRenderers == null)
			{
				return;
			}
			int nextIndex = 0;
			if (base.GetHotRenderer() != null)
			{
				nextIndex = _childRenderers.IndexOf(base.GetHotRenderer()) + 1;
			}
			else if (GetSelectedRenderer() != null)
			{
				nextIndex = _childRenderers.IndexOf(GetSelectedRenderer()) + 1;
			}
			int itemCount = _childRenderers.Count;
			if (itemCount <= 0)
			{
				return;
			}
			CommandBaseRenderer thisValue = _childRenderers[itemCount - 1];
			if (thisValue.CommandType == CommandType.Separator)
			{
				itemCount--;
			}
			if (nextIndex > itemCount - 1)
			{
				nextIndex = 0;
			}
			for (int i = nextIndex; i < itemCount; i++)
			{
				CommandBaseRenderer candidate = _childRenderers[i];
				if (candidate.CommandType != CommandType.Separator)
				{
					if (candidate.CommandType == CommandType.Gallery && !((GalleryMenuRenderer)candidate).TrySelectFirstItem())
					{
						continue;
					}
					if (candidate.CommandType == CommandType.BackstageTab)
					{
						SetSelectedRenderer(candidate);
						if (GetRenderHost() != null && GetRenderHost().ParentControl != null)
						{
							RibbonDrawingUtils.TryFocusControl(GetRenderHost().ParentControl);
						}
					}
					base.SetHotRenderer(_childRenderers[i]);
					break;
				}
				if (nextIndex == itemCount - 1)
				{
					base.SetHotRenderer((CommandBaseRenderer)null);
					Invalidate();
				}
			}
		}

		private CommandBaseRenderer GetLastNonSeparatorItem()
		{
			for (int index = _childRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _childRenderers[index];
				if (thisValue.CommandType != CommandType.Separator)
				{
					return thisValue;
				}
			}
			return null;
		}
	}
}
