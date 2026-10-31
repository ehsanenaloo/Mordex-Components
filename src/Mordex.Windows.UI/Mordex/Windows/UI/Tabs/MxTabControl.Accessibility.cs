using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		void ILayoutItemOwner.DoDefaultAction(FormatStyleLayoutState item)
		{
			if (item == _scrollBackButton)
			{
				ScrollBack();
			}
			if (item == _scrollForwardButton)
			{
				ScrollForward();
			}
			if (item == _closeButton && SelectedTab != null)
			{
				SelectedTab.TabVisible = false;
			}
			if (item is TabPageRenderer && item.IsEnabled() && item is TabPageRenderer uiTabPageRenderer && uiTabPageRenderer.GetTabPage() != null)
			{
				uiTabPageRenderer.GetTabPage().Selected = true;
			}
		}

		void ILayoutItemOwner.AccessibleStateChanged(FormatStyleLayoutState item)
		{
			int childIndex = _pageRenderers.IndexOfItem(item);
			if (childIndex >= 0)
			{
				AccessibilityNotifyClients(AccessibleEvents.StateChange, childIndex);
				return;
			}
			childIndex = _pageRenderers.Count;
			if (item == _scrollBackButton)
			{
				AccessibilityNotifyClients(AccessibleEvents.StateChange, childIndex);
			}
			else if (_scrollBackButton != null)
			{
				childIndex++;
			}
			if (item == _scrollForwardButton)
			{
				AccessibilityNotifyClients(AccessibleEvents.StateChange, childIndex);
			}
			else
			{
				childIndex++;
			}
			if (item == _closeButton)
			{
				AccessibilityNotifyClients(AccessibleEvents.StateChange, childIndex);
			}
		}

		void ILayoutItemOwner.Invalidate(Rectangle rect)
		{
			Invalidate(rect);
		}
	}
}
