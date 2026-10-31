using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	internal class CommandContainerRenderer : CommandBaseRenderer, ICommandDesignTarget
	{
		public IList<CommandBaseRenderer> _childRenderers;

		private CommandBaseRenderer _hotRenderer;

		public override AccessibleObject AccessibilityObject
		{
			get
			{
				if (_cachedAccessibilityObject == null)
				{
					_cachedAccessibilityObject = new CommandContainerAccessibleObject(this);
				}
				return _cachedAccessibilityObject;
			}
		}

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (value != base.Hot)
				{
					if (!value)
					{
						SetHotRenderer(null);
					}
					base.Hot = value;
				}
			}
		}

		Control ICommandDesignTarget.OwnerControl => GetCommand().Ribbon;

		ImageList ICommandDesignTarget.DesignerImageList
		{
			get
			{
				if (GetCommand() != null && _commandBaseRendererCommand.Ribbon != null)
				{
					return _commandBaseRendererCommand.Ribbon._designerImageList;
				}
				return null;
			}
		}

		internal CommandContainerRenderer(CommandBase command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
		}

		public override void StopAnimation()
		{
			if (_childRenderers != null)
			{
				foreach (CommandBaseRenderer child in _childRenderers)
				{
					child.StopAnimation();
				}
			}
			base.StopAnimation();
		}

		internal CommandBaseRenderer GetHotRenderer()
		{
			return _hotRenderer;
		}

		internal void SetHotRenderer(CommandBaseRenderer commandBaseRenderer)
		{
			if (_hotRenderer != commandBaseRenderer)
			{
				if (_hotRenderer != null)
				{
					_hotRenderer.Hot = false;
					if (_hotRenderer.GetCommand() is IMouseEventInvoker)
					{
						((IMouseEventInvoker)_hotRenderer.GetCommand()).InvokeMouseLeave();
					}
				}
				if (commandBaseRenderer != null && commandBaseRenderer.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)commandBaseRenderer.GetCommand()).InvokeMouseEnter();
				}
				_hotRenderer = commandBaseRenderer;
				if (_hotRenderer != null)
				{
					_hotRenderer.Hot = true;
				}
				if (_hotRenderer != null && _hotRenderer.GetRenderHost() != null)
				{
					if (_hotRenderer.GetRenderHost().Ribbon != null)
					{
						_hotRenderer.GetRenderHost().Ribbon.ShowSuperTip(_hotRenderer);
					}
					else if (_hotRenderer.GetRenderHost() is MxRibbonStatusBar)
					{
						((MxRibbonStatusBar)_hotRenderer.GetRenderHost()).ShowSuperTipFor(_hotRenderer);
					}
				}
			}
			else if (_hotRenderer != null && !_hotRenderer.Hot)
			{
				_hotRenderer.Hot = true;
			}
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			FindRendererAt(eventArgs.X, eventArgs.Y)?.OnMouseUp(eventArgs);
			if (eventArgs.Button == MouseButtons.Right && GetRenderHost() != null && this == GetRenderHost().MouseDownItem)
			{
				GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
			}
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			if (commandBaseRenderer != null && commandBaseRenderer.CommandType != CommandType.Separator)
			{
				GetRenderHost().MouseDownItem = commandBaseRenderer;
			}
			else
			{
				GetRenderHost().MouseDownItem = this;
			}
			if (commandBaseRenderer != null && eventArgs.Button == MouseButtons.Left)
			{
				commandBaseRenderer.Pushed = true;
			}
			if (commandBaseRenderer != null)
			{
				commandBaseRenderer.OnMouseDown(eventArgs);
			}
			else
			{
				GetRenderHost().MouseDownItem = this;
			}
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			CommandBaseRenderer commandBaseRenderer = FindRendererAt(eventArgs.X, eventArgs.Y);
			if (eventArgs.Button != MouseButtons.None && commandBaseRenderer != GetRenderHost().MouseDownItem)
			{
				SetHotRenderer(null);
				return;
			}
			SetHotRenderer(commandBaseRenderer);
			commandBaseRenderer?.OnMouseMove(eventArgs);
		}

		internal CommandBaseRenderer FindRendererAt(int x, int y)
		{
			if (_childRenderers != null)
			{
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					if (item._bounds.Contains(x, y))
					{
						return item;
					}
				}
			}
			return null;
		}

		public void RemoveHostedControls()
		{
			if (GetRenderHost() == null || GetRenderHost().ParentControl == null)
			{
				return;
			}
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (item is ContainerControlCommandRenderer)
				{
					Control control = ((ContainerControlCommandRenderer)item).GetHostedControl();
					if (GetRenderHost().ParentControl.Controls.Contains(control))
					{
						GetRenderHost().ParentControl.Controls.Remove(control);
					}
				}
				else if (item is IKeyboardNavigable)
				{
					((IKeyboardNavigable)item).RemoveControls();
				}
			}
		}

		public virtual CommandBaseRenderer CreateLayoutItem(CommandBase command)
		{
			switch (command.CommandType)
			{
			case CommandType.Gallery:
			{
				if (GetRenderHost() is CollapsedGroupPopup)
				{
					Gallery gallery = command as Gallery;
					int popupColumnCount = gallery.MinGalleryColumns;
					if (popupColumnCount <= 0)
					{
						popupColumnCount = gallery.MaxGalleryColumns;
					}
					return new GalleryRenderer(command as Gallery, GetRenderHost(), this, popupColumnCount);
				}
				Gallery galleryCommand = command as Gallery;
				int columnCount = galleryCommand.MinGalleryColumns;
				if (columnCount <= 0)
				{
					columnCount = galleryCommand.MaxGalleryColumns;
				}
				return new GalleryRenderer(command as Gallery, GetRenderHost(), this, columnCount);
			}
			case CommandType.Separator:
				return new RibbonVerticalSeparatorRenderer(command, GetRenderHost(), this);
			case CommandType.Box:
				return new CommandBoxRenderer(command as CommandBox, GetRenderHost(), this);
			case CommandType.ButtonGroup:
				return new ButtonGroupRenderer(command, GetRenderHost(), this);
			case CommandType.ContainerControl:
				return new ContainerControlCommandRenderer((ContainerControlCommand)command, GetRenderHost(), this);
			case CommandType.DropDownButton:
				return new RibbonDropDownButtonRenderer((DropDownCommand)command, GetRenderHost(), this);
			case CommandType.LargeButton:
			case CommandType.SmallButton:
				return new RibbonButtonRenderer((ButtonCommand)command, GetRenderHost(), this);
			case CommandType.Label:
				return new RibbonLabelRenderer(command as LabelCommand, GetRenderHost(), this);
			default:
				return null;
			}
		}

		public int GetItemHeight(CommandBaseRenderer commandBaseRenderer, int maxHeight)
		{
			switch (commandBaseRenderer.CommandType)
			{
			case CommandType.LargeButton:
			case CommandType.Gallery:
			case CommandType.Separator:
				return maxHeight;
			case CommandType.SmallButton:
				return GetRenderHost().SmallItemHeight;
			case CommandType.BackstageTab:
			{
				BackstageTab backstageTab = commandBaseRenderer.GetCommand() as BackstageTab;
				int tabHeight = GetRenderHost().SmallItemHeight + 11;
				if (backstageTab != null)
				{
					int fontHeight = ((backstageTab.Font != null) ? (backstageTab.Font.Height + 7) : 0);
					if (fontHeight > tabHeight)
					{
						tabHeight = fontHeight + 8;
					}
				}
				return tabHeight;
			}
			case CommandType.ContainerControl:
				if (commandBaseRenderer is ContainerControlCommandRenderer && ((ContainerControlCommandRenderer)commandBaseRenderer).GetHostedControl() != null)
				{
					int controlHeight = ((ContainerControlCommandRenderer)commandBaseRenderer).GetHostedControl().Height;
					if (controlHeight < GetRenderHost().SmallItemHeight)
					{
						return GetRenderHost().SmallItemHeight;
					}
					if (controlHeight > maxHeight)
					{
						controlHeight = maxHeight;
					}
					return controlHeight;
				}
				return GetRenderHost().SmallItemHeight;
			case CommandType.Box:
				return ((CommandBoxRenderer)commandBaseRenderer).GetPreferredItemHeight(maxHeight);
			case CommandType.ButtonGroup:
				return ((ButtonGroupRenderer)commandBaseRenderer).GetPreferredItemHeight(maxHeight);
			default:
				if (commandBaseRenderer.GetCommand() is LabelCommand && ((LabelCommand)commandBaseRenderer._commandBaseRendererCommand).SizeStyle == CommandSizeStyle.Large)
				{
					return maxHeight;
				}
				return GetRenderHost().SmallItemHeight;
			}
		}

		internal virtual CommandDropInfo GetDropInfo(Point pt, CommandBase command)
		{
			return null;
		}

		internal virtual bool CanAddCommand(CommandType commandType)
		{
			return true;
		}

		internal virtual void AddNewCommand(CommandBase command, bool editItem)
		{
			if (GetCommand() is ICommandContainer iAccessibleRibbonElement)
			{
				using (DesignEditScope edit = CommandCollection.BeginDesignEdit(iAccessibleRibbonElement.Commands, "Add Command"))
				{
					iAccessibleRibbonElement.Commands.Add(command);
					edit.End();
				}
			}
		}

		bool ICommandDesignTarget.CanAddCommand(CommandType commandType)
		{
			return CanAddCommand(commandType);
		}

		void ICommandDesignTarget.AddNewCommand(CommandBase command, bool editItem)
		{
			AddNewCommand(command, editItem);
		}

		Point ICommandDesignTarget.PointToScreen(Point clientPoint)
		{
			return GetRenderHost().PointToScreen(clientPoint);
		}
	}
}
