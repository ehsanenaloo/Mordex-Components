using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class PopupMenuAccessibleObject : Control.ControlAccessibleObject
	{
		private CommandBaseEditorDialog _popupMenu;

		public override string Name
		{
			get
			{
				string text = _popupMenu.AccessibleName;
				if (text == null)
				{
					text = "DropDown";
					if (_popupMenu._sourceItem != null && _popupMenu._sourceItem.AccessibilityObject.Name != null)
					{
						text = _popupMenu._sourceItem.AccessibilityObject.Name + text;
					}
				}
				return text;
			}
			set
			{
				_popupMenu.AccessibleName = value;
			}
		}

		public override AccessibleRole Role => AccessibleRole.MenuPopup;

		public PopupMenuAccessibleObject(CommandBaseEditorDialog puMenu)
			: base(puMenu)
		{
			_popupMenu = puMenu;
		}

		public override int GetChildCount()
		{
			if (_popupMenu._layoutItems != null)
			{
				return _popupMenu._layoutItems.Count;
			}
			return 0;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_popupMenu._layoutItems != null && index >= 0 && index < _popupMenu._layoutItems.Count && _popupMenu._layoutItems[index] is RibbonLayoutItem layoutItem)
			{
				return layoutItem.AccessibilityObject;
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_popupMenu != null)
			{
				Point point = _popupMenu.PointToClient(new Point(screenX, screenY));
				CommandBaseRenderer commandBaseRenderer = _popupMenu.FindRendererAt(point.X, point.Y);
				if (commandBaseRenderer != null)
				{
					return commandBaseRenderer.AccessibilityObject;
				}
			}
			return base.HitTest(screenX, screenY);
		}
	}
}
