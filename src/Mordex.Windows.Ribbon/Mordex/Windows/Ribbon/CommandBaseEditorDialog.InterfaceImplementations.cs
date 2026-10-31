using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		Control ICommandDesignTarget.OwnerControl => this;

		ImageList ICommandDesignTarget.DesignerImageList
		{
			get
			{
				if (GetRibbon() != null)
				{
					return GetRibbon()._designerImageList;
				}
				return null;
			}
		}

		AccessibleObject ICommandRenderHost.AccessibilityObject => base.AccessibilityObject;

		Control ICommandRenderHost.ParentControl => this;

		int ICommandRenderHost.SmallItemHeight => _owner.SmallItemHeight;

		Size ICommandRenderHost.SmallImageSize => _owner.SmallImageSize;

		Size ICommandRenderHost.LargeImageSize => _owner.LargeImageSize;

		Font ICommandRenderHost.Font => _owner.Font;

		RibbonColorScheme ICommandRenderHost.ColorScheme => _owner.ColorScheme;


		CommandBaseRenderer ICommandRenderHost.FocusItem
		{
			get
			{
				return FocusItem;
			}
			set
			{
				FocusItem = value;
			}
		}

		CommandBaseRenderer ICommandRenderHost.MouseDownItem
		{
			get
			{
				if (_owner != null)
				{
					return _owner.MouseDownItem;
				}
				return null;
			}
			set
			{
				_owner.MouseDownItem = value;
			}
		}

		int ICommandRenderHost.MaxItemRows => _owner.MaxItemRows;

		CommandBaseRenderer ICommandRenderHost.ActiveInputItem
		{
			get
			{
				return _owner.ActiveInputItem;
			}
			set
			{
				_owner.ActiveInputItem = value;
			}
		}

		ISite ICommandRenderHost.Site => _owner.Site;

		bool ICommandRenderHost.UseCompatibleTextRendering => _owner.UseCompatibleTextRendering;

		RightToLeft ICommandRenderHost.RightToLeft => _owner.RightToLeft;

		bool ICommandRenderHost.IsInDesignMode => _owner.IsInDesignMode;

		CommandBaseEditorDialog ICommandRenderHost.ChildMenu
		{
			get
			{
				return GetChildMenu();
			}
			set
			{
				SetChildMenu(value);
			}
		}

		CommandCollection ICommandRenderHost.Commands => _commands;

		Form ICommandRenderHost.ParentForm => _owner.ParentForm;

		CommandBaseRenderer ICommandRenderHost.SelectedItem
		{
			get
			{
				return GetSelectedItem();
			}
			set
			{
				SetSelectedItem(value);
			}
		}

		bool ICommandRenderHost.ShowToolTips => _owner.ShowToolTips;

		MxRibbon ICommandRenderHost.Ribbon => GetRibbon();

		bool ICommandDesignTarget.CanAddCommand(CommandType commandType)
		{
			return true;
		}

		void ICommandDesignTarget.AddNewCommand(CommandBase command, bool editItem)
		{
			ICommandContainer iAccessibleRibbonElement = GetParentMenuCommand() as ICommandContainer;
			if (iAccessibleRibbonElement == null && _owner is MxRibbonContextMenu)
			{
				iAccessibleRibbonElement = _owner as ICommandContainer;
			}
			AddCommandToContainer(command, iAccessibleRibbonElement, editItem);
		}

		Point ICommandDesignTarget.PointToScreen(Point clientPoint)
		{
			return PointToScreen(clientPoint);
		}

		ImageList ICommandRenderHost.GetImageList(CommandBase command)
		{
			return _owner.GetImageList(command);
		}

		ImageList ICommandRenderHost.GetLargeImageList(CommandBase command)
		{
			return _owner.GetLargeImageList(command);
		}

		Point ICommandRenderHost.PointToScreen(Point clientPoint)
		{
			return PointToScreen(clientPoint);
		}

		void ICommandRenderHost.ShowQuickCustomizeMenu(CommandBase command)
		{
			if (GetRibbon() != null)
			{
				GetRibbon().BuildRibbonContextMenu(command, GetRibbon()._isQuickAccessItemMouseUp);
			}
		}
	}
}
