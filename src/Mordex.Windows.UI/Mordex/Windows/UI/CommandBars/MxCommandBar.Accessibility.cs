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
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new CommandBarAccessibleObject(this);
			}
			return _accessibleObject;
		}

		void ILayoutItemOwner.DoDefaultAction(FormatStyleLayoutState item)
		{
			if (_customizeItem == item)
			{
				ShowCustomizeMenu(_customizeItem, PointToScreen(_customizeItem._bounds.Location));
				return;
			}
			CommandLayoutState uiCommandLayoutState = item as CommandLayoutState;
			CommandLayoutState openedItem = null;
			if (GetChildMenu() != null)
			{
				openedItem = GetChildMenu()._parentItem;
			}
			if (((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled() && uiCommandLayoutState != null && uiCommandLayoutState.GetCommand() != null)
			{
				if (uiCommandLayoutState.GetCommand().CanBeClicked() && (_commandBarType != CommandBarType.Menu || !uiCommandLayoutState.GetCommand().IsParent))
				{
					uiCommandLayoutState.GetCommand().InvokeOnClick();
				}
				if (uiCommandLayoutState.GetCommand().IsParent && item.IsEnabled() && uiCommandLayoutState != openedItem)
				{
					OpenChildMenu(uiCommandLayoutState, false);
				}
			}
		}

		void ILayoutItemOwner.Invalidate(Rectangle rect)
		{
			Invalidate();
		}

		void ILayoutItemOwner.AccessibleStateChanged(FormatStyleLayoutState item)
		{
			int index = _layoutItems.IndexOfItem(item);
			if (index >= 0)
			{
				AccessibilityNotifyClients(AccessibleEvents.StateChange, index);
			}
			if (item != _customizeItem)
			{
				AccessibilityNotifyClients(AccessibleEvents.StateChange, _layoutItems.Count);
			}
		}

		AccessibleObject IAccessibleCommandBarsElement.AccessibilityObject
		{
			get { return base.AccessibilityObject; }
		}
	}
}
