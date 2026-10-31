using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		private void DisposeRenderers()
		{
			_mordexColorSchemeRenderer.DisposeAll();
		}

		internal bool IsOwnerDrawn(OwnerDrawnArea ownerDrawnArea)
		{
			if ((_ownerDrawnArea & ownerDrawnArea) != OwnerDrawnArea.None)
			{
				return true;
			}
			return false;
		}

		internal BorderStyle GetEffectiveBorderStyle()
		{
			if (BorderStyle == BorderStyle.None)
			{
				return BorderStyle;
			}
			return BorderStyle.Flat;
		}

		private NavigationBarFormatStyle GetAreaFormatStyle(NavigationBarArea navigationBarArea, object areaTarget)
		{
			switch (navigationBarArea)
			{
			case NavigationBarArea.GroupHeader:
			{
				NavigationGroup headerGroup = (NavigationGroup)areaTarget;
				return GetGroupFormatStyle(headerGroup);
			}
			case NavigationBarArea.Item:
			{
				NavigationItem navigationBarItem = (NavigationItem)areaTarget;
				NavigationGroup itemGroup = navigationBarItem.Group;
				NavigationBarStateStyles navigationBarStateStyles = _itemsStateStyles.MergeWith(itemGroup.ItemsStateStyles);
				return GetItemFormatStyle(navigationBarItem, navigationBarStateStyles);
			}
			default:
				if (areaTarget is NavigationGroup navigationBarGroup)
				{
					return _groupsBackgroundFormatStyle.MergeWith(navigationBarGroup.BackgroundFormatStyle);
				}
				return CreateBaseFormatStyle();
			}
		}

		internal Brush CreateAreaBackBrush(NavigationBarArea navigationBarArea, object areaTarget)
		{
			switch (navigationBarArea)
			{
			case NavigationBarArea.Background:
			{
				NavigationBarFormatStyle backgroundStyle = CreateBaseFormatStyle().MergeWith(_groupsBackgroundFormatStyle);
				return backgroundStyle.GetCachedBackBrush(_itemsAreaBounds);
			}
			case NavigationBarArea.GroupHeader:
			{
				NavigationBarFormatStyle headerStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
				NavigationGroup navigationBarGroup = areaTarget as NavigationGroup;
				return headerStyle?.CreateBackBrush(navigationBarGroup.Bounds);
			}
			case NavigationBarArea.Item:
			{
				NavigationItem navigationBarItem = (NavigationItem)areaTarget;
				NavigationBarFormatStyle itemStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
				return itemStyle.CreateBackBrush(navigationBarItem.GetBounds());
			}
			default:
			{
				NavigationBarFormatStyle navigationBarFormatStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
				return navigationBarFormatStyle.CreateBackBrush(_itemsAreaBounds);
			}
			}
		}

		internal Color GetAreaBackColor(NavigationBarArea navigationBarArea, object areaTarget)
		{
			if (navigationBarArea == NavigationBarArea.Background)
			{
				return GetEffectiveBackColor();
			}
			NavigationBarFormatStyle navigationBarFormatStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
			return navigationBarFormatStyle.BackColor;
		}

		internal Brush CreateAreaTextBrush(NavigationBarArea navigationBarArea, object areaTarget)
		{
			NavigationBarFormatStyle navigationBarFormatStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
			return navigationBarFormatStyle.GetForeBrush(GetEffectiveForeColor());
		}

		internal Color GetAreaTextColor(NavigationBarArea navigationBarArea, object areaTarget)
		{
			NavigationBarFormatStyle navigationBarFormatStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
			return navigationBarFormatStyle.GetForeColorOrDefault(GetEffectiveForeColor());
		}

		private NavigationBarFormatStyle GetGroupFormatStyle(NavigationGroup navigationBarGroup)
		{
			NavigationBarStateStyles navigationBarStateStyles = GroupsStateStyles.MergeWith(navigationBarGroup.StateStyles);
			NavigationBarFormatStyle navigationBarFormatStyle = CreateBaseFormatStyle();
			navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(navigationBarStateStyles.FormatStyle);
			if (!navigationBarGroup.Enabled || !base.Enabled)
			{
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.DisabledFormatStyle);
			}
			switch (navigationBarGroup._itemState)
			{
			case ItemState.SelectedFocus:
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.PressedFormatStyle);
			case ItemState.Selected:
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.SelectedFormatStyle);
			case ItemState.Focus:
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.HotFormatStyle);
			default:
				return navigationBarFormatStyle;
			}
		}

		private NavigationBarFormatStyle GetItemFormatStyle(NavigationItem navigationBarItem, NavigationBarStateStyles defaultStateStyles)
		{
			NavigationBarStateStyles navigationBarStateStyles = defaultStateStyles.MergeWith(navigationBarItem.StateStyles);
			NavigationBarFormatStyle navigationBarFormatStyle = new NavigationBarFormatStyle(navigationBarStateStyles.FormatStyle);
			if (!navigationBarItem.Enabled || !navigationBarItem.Group.Enabled || !base.Enabled)
			{
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.DisabledFormatStyle);
			}
			switch (navigationBarItem._itemState)
			{
			case ItemState.SelectedFocus:
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.PressedFormatStyle);
			case ItemState.Selected:
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.SelectedFormatStyle);
			case ItemState.Focus:
				return navigationBarFormatStyle.MergeWith(navigationBarStateStyles.HotFormatStyle);
			default:
				return navigationBarFormatStyle;
			}
		}

		private void RefreshFormatStyles()
		{
			ApplyThemedStyle(_groupsBackgroundFormatStyle);
			ApplyThemedStyle(GroupsStateStyles.FormatStyle);
			ApplyThemedStyle(GroupsStateStyles.HotFormatStyle);
			ApplyThemedStyle(GroupsStateStyles.SelectedFormatStyle);
			ApplyThemedStyle(GroupsStateStyles.PressedFormatStyle);
			ApplyThemedStyle(GroupsStateStyles.DisabledFormatStyle);
			ApplyThemedStyle(ItemsStateStyles.FormatStyle);
			ApplyThemedStyle(ItemsStateStyles.HotFormatStyle);
			ApplyThemedStyle(ItemsStateStyles.SelectedFormatStyle);
			ApplyThemedStyle(ItemsStateStyles.PressedFormatStyle);
			ApplyThemedStyle(ItemsStateStyles.DisabledFormatStyle);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			RefreshFormatStyles();
			Invalidate(invalidateChildren: true);
		}

		internal NavigationBarFormatStyle CreateBaseFormatStyle()
		{
			NavigationBarFormatStyle navigationBarFormatStyle = new NavigationBarFormatStyle();
			navigationBarFormatStyle.SetAlphaModes(_alpha, GetBackColorAlphaMode(), GetForeColorAlphaMode());
			navigationBarFormatStyle.BackColor = GetEffectiveBackColor();
			navigationBarFormatStyle.ForeColor = GetEffectiveForeColor();
			return navigationBarFormatStyle;
		}

		internal Color GetEffectiveForeColor()
		{
			if (ForeColor == SystemColors.ControlText)
			{
				return GetTheme().Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		internal Color GetEffectiveBackColor()
		{
			if (BackColor == SystemColors.Control)
			{
				return GetTheme().Color(ThemeRole.Surface);
			}
			return BackColor;
		}

		private AlphaMode GetBackColorAlphaMode()
		{
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return _backColorAlphaMode;
			}
			return _alphaMode;
		}

		private AlphaMode GetForeColorAlphaMode()
		{
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return _foreColorAlphaMode;
			}
			return _alphaMode;
		}
	}
}
