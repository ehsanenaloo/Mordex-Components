using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	internal class StatusBarCommandsRenderer : ICommandDesignTarget, ISupportsDesignerMouse
	{
		private CommandCollection _commands;

		private bool _areItemsStale;

		internal IList<CommandBaseRenderer> _itemRenderers;

		private MxRibbonStatusBar _statusBar;

		internal Rectangle _bounds;

		private StatusBarPanelType _panelType;

		private bool _isMousePressed;

		internal Rectangle _sizeGripBounds;

		private bool _isDesignerMouseIn;

		Control ICommandDesignTarget.OwnerControl => _statusBar;

		ImageList ICommandDesignTarget.DesignerImageList
		{
			get
			{
				if (_statusBar != null)
				{
					return _statusBar._designerImageList;
				}
				return null;
			}
		}

		bool ISupportsDesignerMouse.DesignerMouseIn
		{
			get
			{
				return IsDesignerMouseIn();
			}
			set
			{
				SetDesignerMouseIn(value);
			}
		}

		internal StatusBarCommandsRenderer(CommandCollection commands, MxRibbonStatusBar statusBar, StatusBarPanelType panelType)
		{
			_commands = commands;
			_areItemsStale = true;
			_statusBar = statusBar;
			_panelType = panelType;
		}

		public bool IsDesignerMouseIn()
		{
			return _isDesignerMouseIn;
		}

		public void SetDesignerMouseIn(bool isDesignerMouseIn)
		{
			if (_statusBar != null && _statusBar._designer != null && _statusBar.GetChildMenu() != null)
			{
				isDesignerMouseIn = false;
			}
			if (_isDesignerMouseIn != isDesignerMouseIn)
			{
				_isDesignerMouseIn = isDesignerMouseIn;
				_statusBar.RebuildPanelLayouts();
			}
		}

		internal void InvalidateItems()
		{
			_areItemsStale = true;
		}

		internal CommandBaseRenderer CreateRendererForCommand(CommandBase command)
		{
			switch (command.CommandType)
			{
			case CommandType.Separator:
				return new RibbonVerticalSeparatorRenderer(command, _statusBar, null);
			case CommandType.ButtonGroup:
				return new ButtonGroupRenderer(command, _statusBar, null);
			case CommandType.ContainerControl:
				return new ContainerControlCommandRenderer((ContainerControlCommand)command, _statusBar, null);
			case CommandType.DropDownButton:
				return new RibbonDropDownButtonRenderer((DropDownCommand)command, _statusBar, null);
			case CommandType.LargeButton:
			case CommandType.SmallButton:
				return new RibbonButtonRenderer((ButtonCommand)command, _statusBar, null);
			case CommandType.Label:
				return new RibbonLabelRenderer(command as LabelCommand, _statusBar, null);
			case CommandType.StatusBarPanel:
				return new StatusBarPanelRenderer(command as StatusBarPanel, _statusBar, null);
			default:
				return null;
			}
		}

		internal void RebuildItemRenderers()
		{
			if (_itemRenderers != null)
			{
				_statusBar.Controls.Clear();
			}
			_itemRenderers = new List<CommandBaseRenderer>();
			foreach (CommandBase command in _commands)
			{
				if (command.Visible)
				{
					CommandBaseRenderer commandBaseRenderer = CreateRendererForCommand(command);
					if (commandBaseRenderer != null)
					{
						_itemRenderers.Add(commandBaseRenderer);
					}
				}
			}
			if (IsDesignerMouseIn())
			{
				StatusBarAddCommandRenderer item = new StatusBarAddCommandRenderer(_statusBar, this);
				_itemRenderers.Add(item);
			}
			_areItemsStale = false;
		}

		internal int MeasureItems(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (_areItemsStale)
			{
				RebuildItemRenderers();
			}
			int totalWidth = 0;
			int buttonSpacing = 3;
			for (int i = 0; i < _itemRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _itemRenderers[i];
				thisValue.CalculateFullItemWidth(graphics, textRenderer, font);
				totalWidth += thisValue._fullItemWidth;
				if (thisValue is RibbonButtonRenderer && i < _itemRenderers.Count - 1)
				{
					totalWidth += buttonSpacing;
				}
			}
			if (_panelType == StatusBarPanelType.RightPanel)
			{
				if (_statusBar.CanShowSizeGrip())
				{
					totalWidth += 18;
				}
				if (totalWidth > 0)
				{
					totalWidth += buttonSpacing;
				}
			}
			return totalWidth;
		}

		internal int GetTotalWidth()
		{
			int totalWidth = 0;
			int buttonSpacing = 3;
			for (int i = 0; i < _itemRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _itemRenderers[i];
				totalWidth += thisValue._fullItemWidth;
				if (thisValue is RibbonButtonRenderer && i < _itemRenderers.Count - 1)
				{
					totalWidth += buttonSpacing;
				}
			}
			if (_panelType == StatusBarPanelType.RightPanel)
			{
				if (_statusBar.CanShowSizeGrip())
				{
					totalWidth += 18;
				}
				if (totalWidth > 0)
				{
					totalWidth += buttonSpacing;
				}
			}
			return totalWidth;
		}

		internal void RenderContent(Graphics graphics, TextLayoutRenderer textRenderer, Rectangle rectangle)
		{
			_bounds = rectangle;
			int currentX = rectangle.X;
			if (_panelType == StatusBarPanelType.RightPanel)
			{
				currentX += 2;
			}
			int topOffset = 0;
			int buttonSpacing = 3;
			for (int i = 0; i < _itemRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _itemRenderers[i];
				if (currentX + thisValue._fullItemWidth > rectangle.Right || currentX < 0)
				{
					thisValue._isVisible = false;
					currentX += thisValue._fullItemWidth;
					continue;
				}
				thisValue._isVisible = true;
				thisValue.ArrangeItem(graphics, textRenderer, currentX, rectangle.Top + topOffset, thisValue._fullItemWidth, rectangle.Height - topOffset - 1);
				currentX += thisValue._bounds.Width;
				if (thisValue is RibbonButtonRenderer && i < _itemRenderers.Count - 1)
				{
					currentX += buttonSpacing;
				}
			}
			if (_panelType == StatusBarPanelType.RightPanel)
			{
				if (_statusBar.CanShowSizeGrip())
				{
					_sizeGripBounds = new Rectangle(rectangle.Right - 21, rectangle.Y, 20, rectangle.Height);
				}
				else
				{
					_sizeGripBounds = Rectangle.Empty;
				}
				_statusBar._sizeGripBounds = _sizeGripBounds;
			}
			else
			{
				_sizeGripBounds = Rectangle.Empty;
			}
		}

		internal CommandBaseRenderer FindItemRendererAt(int x, int y)
		{
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._isVisible && item._bounds.Contains(x, y))
					{
						return item;
					}
				}
			}
			return null;
		}

		internal CommandBase FindCommandAt(int x, int y)
		{
			return FindItemRendererAt(x, y)?.GetCommand();
		}

		internal CommandBaseRenderer GetItemRendererForCommand(CommandBase commandBase)
		{
			if (_itemRenderers != null)
			{
				foreach (CommandBaseRenderer item in _itemRenderers)
				{
					if (item._commandBaseRendererCommand == commandBase)
					{
						return item;
					}
				}
			}
			return null;
		}

		public bool HandleMouseUp(MouseEventArgs eventArgs)
		{
			if (_isMousePressed)
			{
				_isMousePressed = false;
				FindItemRendererAt(eventArgs.X, eventArgs.Y)?.OnMouseUp(eventArgs);
				return true;
			}
			return false;
		}

		internal bool HandleMouseDown(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindItemRendererAt(eventArgs.X, eventArgs.Y);
			if (commandBaseRenderer != null)
			{
				_isMousePressed = true;
				_statusBar.SetPushedRenderer(commandBaseRenderer);
				commandBaseRenderer.OnMouseDown(eventArgs);
				return true;
			}
			return false;
		}

		internal bool HandleMouseMove(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindItemRendererAt(eventArgs.X, eventArgs.Y);
			if (commandBaseRenderer != null)
			{
				_statusBar.SetHotRenderer(commandBaseRenderer);
				commandBaseRenderer.OnMouseMove(eventArgs);
				return true;
			}
			return false;
		}

		internal void PaintPanel(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Rectangle clipRectangle)
		{
			if (_bounds.Width <= 0 || _bounds.Height <= 0)
			{
				return;
			}
			DrawPanelGradientBackground(graphics, colorScheme, _bounds);
			if (_panelType == StatusBarPanelType.RightPanel)
			{
				DrawSeparator(graphics, _bounds.X, colorScheme);
			}
			for (int i = 0; i < _itemRenderers.Count; i++)
			{
				CommandBaseRenderer thisValue = _itemRenderers[i];
				if (!thisValue._isVisible)
				{
					continue;
				}
				Rectangle rectangle = Rectangle.Intersect(clipRectangle, thisValue._bounds);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					if ((thisValue.Hot || thisValue.Pushed) && thisValue.Focusable)
					{
						thisValue.Paint(graphics, textRenderer, _statusBar._colorScheme, font, colorScheme._foreColorDark, clipRectangle);
					}
					else
					{
						thisValue.Paint(graphics, textRenderer, _statusBar._colorScheme, font, _statusBar.ForeColor, clipRectangle);
					}
				}
				if (thisValue is RibbonButtonRenderer && i < _itemRenderers.Count - 1)
				{
					DrawSeparator(graphics, thisValue._bounds.Right, colorScheme);
				}
			}
			if (!_sizeGripBounds.IsEmpty)
			{
				DrawBackground(graphics, colorScheme);
			}
		}

		private void DrawBackground(Graphics graphics, RibbonColorScheme colorScheme)
		{
			Brush brush = null;
			Brush gripShadowBrush = null;
			brush = new SolidBrush(colorScheme._statusBarGripDot);
			gripShadowBrush = new SolidBrush(colorScheme._statusBarGripShadow);
			int rightInset = 1;
			int bottomInset = 2;
			Rectangle rectangle = _sizeGripBounds;
			rectangle.Width -= rightInset;
			Rectangle rect;
			for (int i = 0; i < 3; i++)
			{
				rect = new Rectangle(rectangle.Right, rectangle.Bottom - bottomInset, 2, 2);
				graphics.FillRectangle(gripShadowBrush, rect);
				rect.Offset(-1, -1);
				graphics.FillRectangle(brush, rect);
				rectangle.Width -= 4;
			}
			rectangle.Width = _sizeGripBounds.Width - rightInset;
			rectangle.Height -= 4;
			for (int j = 0; j < 2; j++)
			{
				rect = new Rectangle(rectangle.Right, rectangle.Bottom - bottomInset, 2, 2);
				graphics.FillRectangle(gripShadowBrush, rect);
				rect.Offset(-1, -1);
				graphics.FillRectangle(brush, rect);
				rectangle.Width -= 4;
			}
			rectangle.Width = _sizeGripBounds.Width - rightInset;
			rect = new Rectangle(rectangle.Right, rectangle.Bottom - 4 - bottomInset, 2, 2);
			graphics.FillRectangle(gripShadowBrush, rect);
			rect.Offset(-1, -1);
			graphics.FillRectangle(brush, rect);
			brush.Dispose();
			brush = null;
			gripShadowBrush.Dispose();
			gripShadowBrush = null;
		}

		private void DrawSeparator(Graphics graphics, int x1, RibbonColorScheme colorScheme)
		{
			if (_bounds.Height < 3)
			{
				return;
			}
			graphics.DrawLine(colorScheme.Theme.Pen(ThemeRole.Border), x1, _bounds.Top + 3, x1, _bounds.Bottom - 4);
			return;
		}

		internal void FillPanelBackground(Graphics graphics, RibbonColorScheme colorScheme, Rectangle rect)
		{
			try
			{
				if (_panelType == StatusBarPanelType.LeftPanel)
				{
					Brush brush = GdiCache.Brush(colorScheme._statusBarLeftBottomEnd);
					graphics.FillRectangle(brush, rect);
					return;
				}
				Brush rightPanelBrush = GdiCache.Brush(colorScheme._statusBarRightBottomEnd);
				graphics.FillRectangle(rightPanelBrush, rect);
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				// GDI+ fails when the target surface is no longer valid; skip the paint.
			}
		}

		internal void DrawPanelGradientBackground(Graphics graphics, RibbonColorScheme colorScheme, Rectangle bounds)
		{
			graphics.FillRectangle(GdiCache.Brush(colorScheme._statusBarLeftTopStart), bounds);
		}

		bool ICommandDesignTarget.CanAddCommand(CommandType commandType)
		{
			return true;
		}

		void ICommandDesignTarget.AddNewCommand(CommandBase command, bool editItem)
		{
			if (_panelType == StatusBarPanelType.LeftPanel)
			{
				_statusBar.LeftPanelCommands.Add(command);
			}
			else if (_panelType == StatusBarPanelType.RightPanel)
			{
				_statusBar.RightPanelCommands.Add(command);
			}
		}

		Point ICommandDesignTarget.PointToScreen(Point clientPoint)
		{
			return _statusBar.PointToScreen(clientPoint);
		}
	}
}
