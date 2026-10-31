using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		private void StartDesignDragDrop()
		{
			_dragStartPoint = Point.Empty;
			AllowDrop = true;
			SetChildMenu(null);
			CommandBaseState data = new CommandBaseState(GetDesignSelectedItem());
			DoDragDrop(data, DragDropEffects.All);
			AllowDrop = false;
		}

		internal virtual bool OnComponentSelected(ISelectionService selectionService)
		{
			EndInPlaceEdit(false);
			if (selectionService != null && selectionService.SelectionCount > 0)
			{
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item is GalleryMenuRenderer)
					{
						((GalleryMenuRenderer)item).SetDesignSelectedCommand(null);
					}
				}
				foreach (Component selectedComponent in selectionService.GetSelectedComponents())
				{
					if (!(selectedComponent is CommandBase))
					{
						continue;
					}
					CommandBase commandBase = selectedComponent as CommandBase;
					if (GetParentMenuCommand() == selectedComponent)
					{
						SetDesignSelectedItem(null);
						return true;
					}
					bool isGalleryItem = commandBase is GalleryItem;
					foreach (CommandBaseRenderer layoutItem in _layoutItems)
					{
						if (isGalleryItem)
						{
							if (layoutItem.GetCommand() == commandBase.GetParentContainer())
							{
								if (layoutItem is GalleryMenuRenderer)
								{
									((GalleryMenuRenderer)layoutItem).SetDesignSelectedCommand(commandBase);
								}
								else
								{
									SetDesignSelectedItem(null);
								}
								return true;
							}
						}
						else if (layoutItem.GetCommand() == commandBase)
						{
							SetDesignSelectedItem(layoutItem);
							return true;
						}
					}
				}
				SetDesignSelectedItem(null);
				if (GetChildMenu() != null)
				{
					if (!GetChildMenu().OnComponentSelected(selectionService))
					{
						ClearOwnerChildMenu();
						return false;
					}
					return true;
				}
				ClearOwnerChildMenu();
			}
			else
			{
				SetDesignSelectedItem(null);
				if (GetChildMenu() != null)
				{
					GetChildMenu().OnComponentSelected(selectionService);
					return false;
				}
			}
			return false;
		}

		protected void DrawDesignSelectionBorders(Graphics graphics)
		{
			if (GetDesignHoverItem() != null && !(GetDesignHoverItem() is RibbonContextMenuRenderer) && GetDesignHoverItem().GetCommand() != _inPlaceEditCommand)
			{
				DrawSelectionBorder(graphics, GetDesignHoverItem()._bounds, DashStyle.Dot);
			}
			if (GetDesignSelectedItem() != null && GetDesignSelectedItem().GetCommand() != _inPlaceEditCommand)
			{
				DrawSelectionBorder(graphics, GetDesignSelectedItem()._bounds, DashStyle.Solid);
			}
			if (_owner.Site == null)
			{
				return;
			}
			ISelectionService selectionService = (ISelectionService)_owner.Site.GetService(typeof(ISelectionService));
			if (selectionService == null || selectionService.SelectionCount <= 1)
			{
				return;
			}
			foreach (Component selectedComponent in selectionService.GetSelectedComponents())
			{
				if (!(selectedComponent is CommandBase))
				{
					continue;
				}
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item._commandBaseRendererCommand == selectedComponent)
					{
						DrawSelectionBorder(graphics, item._bounds, DashStyle.Dot);
						break;
					}
				}
			}
		}

		internal CommandBaseRenderer GetDesignSelectedItem()
		{
			return _designSelectedItem;
		}

		internal void SetDesignSelectedItem(CommandBaseRenderer designSelectedItem)
		{
			if (_designSelectedItem == designSelectedItem)
			{
				return;
			}
			if (designSelectedItem != null && designSelectedItem.GetCommand().GetParentContainer() == null)
			{
				SetChildMenu(null);
				designSelectedItem = null;
			}
			_designSelectedItem = designSelectedItem;
			if (_designSelectedItem != null)
			{
				if (_designSelectedItem.CommandType != CommandType.Gallery && _designSelectedItem.GetCommand() is DropDownCommand)
				{
					ShowChildMenu(_designSelectedItem, selectItem: false);
				}
				else
				{
					SetChildMenu(null);
				}
			}
			Invalidate();
		}

		internal CommandBaseRenderer GetDesignHoverItem()
		{
			return _designHoverItem;
		}

		public RibbonContextMenuRenderer GetDesignContextMenuRenderer()
		{
			if (_designContextMenuRenderer == null)
			{
				_designContextMenuRenderer = new RibbonContextMenuRenderer(this, this, drawImageArea: true);
			}
			return _designContextMenuRenderer;
		}

		private bool CanEditInPlace()
		{
			if (_allowInPlaceEdit && _owner.Site != null)
			{
				ISelectionService selectionService = (ISelectionService)_owner.Site.GetService(typeof(ISelectionService));
				if (selectionService != null && selectionService.SelectionCount > 1)
				{
					return false;
				}
			}
			return _allowInPlaceEdit;
		}

		private void NotifyComponentChanging(Component component, string name)
		{
			if (component != null && component.Site != null)
			{
				IComponentChangeService componentChangeService = (IComponentChangeService)component.Site.GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					PropertyDescriptor member = TypeDescriptor.GetProperties(component).Find(name, ignoreCase: true);
					componentChangeService.OnComponentChanging(component, member);
				}
			}
		}

		private void NotifyComponentChanged(Component component, string name, object oldValue, object newValue)
		{
			if (component != null && component.Site != null)
			{
				IComponentChangeService componentChangeService = (IComponentChangeService)component.Site.GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					PropertyDescriptor member = TypeDescriptor.GetProperties(component).Find(name, ignoreCase: true);
					componentChangeService.OnComponentChanged(component, member, oldValue, newValue);
				}
			}
		}

		internal void AddCommandToCollection(CommandBase commandBaseEditorDialogCommandBase, CommandCollection commandCollection, bool startEditing)
		{
			EndInPlaceEdit(false);
			if (commandCollection != null)
			{
				_addedCommand = commandBaseEditorDialogCommandBase;
				_allowInPlaceEdit = true;
				if (CanEditInPlace() && startEditing)
				{
					_inPlaceEditCommand = commandBaseEditorDialogCommandBase;
				}
				_allowInPlaceEdit = false;
				SetDesignContextMenuRenderer(null);
				using (DesignEditScope edit = CommandCollection.BeginDesignEdit(commandCollection, "Add Command"))
				{
					commandCollection.Add(commandBaseEditorDialogCommandBase);
					edit.End();
				}
				MarkMenuBoundsDirty();
			}
		}

		internal void AddCommandToContainer(CommandBase commandBaseEditorDialogCommandBase, ICommandContainer iAccessibleRibbonElement, bool startEditing)
		{
			EndInPlaceEdit(false);
			if (iAccessibleRibbonElement != null)
			{
				_addedCommand = commandBaseEditorDialogCommandBase;
				_allowInPlaceEdit = true;
				if (CanEditInPlace() && startEditing)
				{
					_inPlaceEditCommand = commandBaseEditorDialogCommandBase;
				}
				_allowInPlaceEdit = false;
				SetDesignContextMenuRenderer(null);
				using (DesignEditScope edit = CommandCollection.BeginDesignEdit(iAccessibleRibbonElement.Commands, "Add Command"))
				{
					iAccessibleRibbonElement.Commands.Add(commandBaseEditorDialogCommandBase);
					edit.End();
				}
				MarkMenuBoundsDirty();
			}
		}

		internal void EndInPlaceEdit(bool cancel)
		{
			if (_ownerItemRenderer != null)
			{
				if (!cancel && _ownerItemRenderer.GetOrCreateTextBox().Text.Length == 0)
				{
					cancel = true;
				}
				string text = _ownerItemRenderer.GetOrCreateTextBox().Text;
				_ownerItemRenderer.Dispose();
				_ownerItemRenderer = null;
				if (!cancel)
				{
					if (_inPlaceEditCommand is ButtonCommand)
					{
						ButtonCommand buttonCommand = (ButtonCommand)_inPlaceEditCommand;
						if (buttonCommand.Text != text)
						{
							using (DesignEditScope ownerEdit = CommandCollection.BeginDesignEdit(buttonCommand.CommandsOwner ?? _commands, "Rename Command"))
							{
								NotifyComponentChanging(buttonCommand, "Text");
								object oldText = buttonCommand.Text;
								buttonCommand.Text = text;
								NotifyComponentChanged(buttonCommand, "Text", oldText, text);
								ownerEdit.End();
							}
						}
					}
				}
				else if (_addedCommand == _inPlaceEditCommand)
				{
					_addedCommand.Dispose();
				}
				_addedCommand = null;
				_inPlaceEditCommand = null;
				MarkMenuBoundsDirty();
			}
			else if (GetChildMenu() != null)
			{
				GetChildMenu().EndInPlaceEdit(cancel);
			}
		}

		private void ShowDesignerContextMenu(CommandBaseRenderer commandBaseRenderer)
		{
			if (commandBaseRenderer != null && commandBaseRenderer.GetCommand() != null)
			{
				MxRibbonContextMenu ribbonContextMenu = new MxRibbonContextMenu();
				if (GetRibbon() != null)
				{
					ribbonContextMenu.ImageList = GetRibbon()._designerImageList;
				}
				ButtonCommand buttonCommand = new ButtonCommand("Delete", "Delete");
				buttonCommand.ImageIndex = 8;
				ButtonCommand moveUpCommand = new ButtonCommand("MoveUp", "Move up");
				moveUpCommand.ImageIndex = 13;
				ButtonCommand moveDownCommand = new ButtonCommand("MoveDown", "Move down");
				moveDownCommand.ImageIndex = 12;
				ButtonCommand addToQuickAccessCommand = new ButtonCommand("AddQA", "Add to Quick Access Toolbar...");
				if (commandBaseRenderer.GetCommand().GetIndexInOwner() == 0)
				{
					moveUpCommand.Enabled = false;
				}
				if (commandBaseRenderer.GetCommand().GetIndexInOwner() == commandBaseRenderer.GetCommand().GetParentContainer().Commands.Count - 1)
				{
					moveDownCommand.Enabled = false;
				}
				if (GetRibbon() == null || GetRibbon().QuickAccessCommands.Contains(commandBaseRenderer.GetCommand()))
				{
					addToQuickAccessCommand.Visible = false;
				}
				buttonCommand.Tag = commandBaseRenderer.GetCommand();
				moveDownCommand.Tag = commandBaseRenderer.GetCommand();
				moveUpCommand.Tag = commandBaseRenderer.GetCommand();
				addToQuickAccessCommand.Tag = commandBaseRenderer.GetCommand();
				ribbonContextMenu.Commands.Add(buttonCommand);
				ribbonContextMenu.Commands.Add(moveUpCommand);
				ribbonContextMenu.Commands.Add(moveDownCommand);
				ribbonContextMenu.Commands.Add(new SeparatorCommand());
				ribbonContextMenu.Commands.Add(addToQuickAccessCommand);
				ribbonContextMenu.CommandClick += OnControlCommandClick;
				ribbonContextMenu.Show();
			}
		}

		private void SelectCommandInDesigner(CommandBase commandBase)
		{
			if (_owner.Site == null)
			{
				return;
			}
			ISelectionService selectionService = (ISelectionService)_owner.Site.GetService(typeof(ISelectionService));
			if (selectionService == null)
			{
				return;
			}
			if (selectionService.PrimarySelection == commandBase)
			{
				_allowInPlaceEdit = true;
			}
			else
			{
				_allowInPlaceEdit = false;
			}
			ArrayList arrayList = new ArrayList();
			bool removedFromSelection = false;
			if (Control.ModifierKeys == Keys.Control)
			{
				_allowInPlaceEdit = false;
				foreach (Component selectedComponent in selectionService.GetSelectedComponents())
				{
					if (selectedComponent != commandBase)
					{
						arrayList.Add(selectedComponent);
					}
					else
					{
						removedFromSelection = true;
					}
				}
			}
			if (!removedFromSelection)
			{
				arrayList.Add(commandBase);
			}
			selectionService.SetSelectedComponents(arrayList, SelectionTypes.Replace);
		}
	}
}
