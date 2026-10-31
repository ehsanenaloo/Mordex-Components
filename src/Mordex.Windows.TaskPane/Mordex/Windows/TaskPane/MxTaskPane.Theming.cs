using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		internal BorderStyle ComputeBorderStyle()
		{
			if (BorderStyle == BorderStyle.None)
			{
				return _borderStyle;
			}
			return BorderStyle.Flat;
		}

		internal void ApplyExplorerAreaChange(ExplorerArea explorerArea)
		{
			if (explorerArea == ExplorerArea.Border)
			{
				InvalidateLayout();
				return;
			}
			if (explorerArea == ExplorerArea.GroupHeader)
			{
				InvalidateLayout();
			}
			RefreshThemeResources();
		}

		internal bool IsAreaOwnerDrawn(OwnerDrawnArea ownerDrawnArea)
		{
			if ((_ownerDrawnArea & ownerDrawnArea) != OwnerDrawnArea.None)
			{
				return true;
			}
			return false;
		}

		internal TaskPaneFormatStyle CreateDefaultFormatStyle()
		{
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle();
			taskPaneFormatStyle.SetAlphaSettings(_alpha, GetBackColorAlphaMode(), GetForeColorAlphaMode());
			if (taskPaneFormatStyle.BackColor.IsEmpty)
			{
				taskPaneFormatStyle.BackColor = GetEffectiveBackColor();
			}
			taskPaneFormatStyle.ForeColor = GetEffectiveForeColor();
			return taskPaneFormatStyle;
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			InvalidateLayout();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			RefreshThemeResources();
		}

		private void RefreshThemeResources()
		{
			ApplyThemedStyles();
		}

		internal void PrepareContainerStyle(TaskPaneContainerControl taskPaneContainerControl, TaskPaneGroup taskPaneGroup, TaskPaneItem taskPaneItem)
		{
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle(_itemsStateStyles.FormatStyle);
			taskPaneFormatStyle = taskPaneFormatStyle.MergeWith(taskPaneGroup.ItemsStateStyles.FormatStyle);
			if (taskPaneItem != null)
			{
				taskPaneFormatStyle = taskPaneFormatStyle.MergeWith(taskPaneItem.StateStyles.FormatStyle);
			}
			taskPaneContainerControl.SetContainerStyle(taskPaneFormatStyle);
		}

		internal bool ShouldShowGroupBorder(TaskPaneGroup taskPaneGroup)
		{
			if (taskPaneGroup.ShowGroupBorder == TriState.Empty)
			{
				return _showGroupBorder;
			}
			if (taskPaneGroup.ShowGroupBorder == TriState.True)
			{
				return true;
			}
			return false;
		}

		private TaskPaneFormatStyle ComputeItemFormatStyle(TaskPaneItem taskPaneItem, TaskPaneStateStyles baseStateStyles)
		{
			TaskPaneStateStyles taskPaneStateStyles = baseStateStyles.MergeWith(taskPaneItem.StateStyles);
			taskPaneStateStyles.FormatStyle.BackColor = taskPaneItem.StateStyles.FormatStyle.BackColor;
			taskPaneStateStyles.FormatStyle.BackColorGradient = taskPaneItem.StateStyles.FormatStyle.BackColorGradient;
			taskPaneStateStyles.FormatStyle.BackgroundGradientMode = taskPaneItem.StateStyles.FormatStyle.BackgroundGradientMode;
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle(taskPaneStateStyles.FormatStyle);
			if (taskPaneItem.Enabled && base.Enabled && taskPaneItem.Group.Enabled)
			{
				bool isSelected = false;
				if ((_keepSelection && taskPaneItem == _selectedItem) || (taskPaneItem == _pressedItem && taskPaneItem._itemState == ItemState.Focus))
				{
					isSelected = true;
				}
				if (isSelected && taskPaneItem._itemState == ItemState.Focus)
				{
					return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.PressedFormatStyle);
				}
				if (taskPaneItem._itemState == ItemState.Focus)
				{
					return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.HotFormatStyle);
				}
				if (isSelected)
				{
					return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.SelectedFormatStyle);
				}
				return taskPaneFormatStyle;
			}
			return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.DisabledFormatStyle);
		}

		private TaskPaneFormatStyle ComputeGroupFormatStyle(TaskPaneGroup taskPaneGroup)
		{
			TaskPaneStateStyles taskPaneStateStyles = ((!taskPaneGroup.SpecialGroup) ? new TaskPaneStateStyles(_groupsStateStyles) : new TaskPaneStateStyles(_specialGroupsStateStyles));
			taskPaneStateStyles = taskPaneStateStyles.MergeWith(taskPaneGroup.StateStyles);
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle(taskPaneStateStyles.FormatStyle);
			if (taskPaneGroup.Enabled && base.Enabled)
			{
				if (taskPaneGroup.GetItemState() == ItemState.SelectedFocus)
				{
					return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.PressedFormatStyle);
				}
				if (taskPaneGroup.GetItemState() == ItemState.Focus)
				{
					return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.HotFormatStyle);
				}
				if (taskPaneGroup.GetItemState() == ItemState.Selected)
				{
					return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.SelectedFormatStyle);
				}
				return taskPaneFormatStyle;
			}
			return taskPaneFormatStyle.MergeWith(taskPaneStateStyles.DisabledFormatStyle);
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
			if (!ShouldSerializeBackColor())
			{
				return GetTheme().Color(ThemeRole.Surface);
			}
			return BackColor;
		}

		private TaskPaneFormatStyle ResolveAreaFormatStyle(ExplorerArea explorerArea, object areaTarget)
		{
			if (explorerArea == ExplorerArea.GroupHeader)
			{
				TaskPaneGroup taskPaneGroup = (TaskPaneGroup)areaTarget;
				TaskPaneFormatStyle taskPaneFormatStyle = null;
				taskPaneFormatStyle = ((!taskPaneGroup.SpecialGroup) ? new TaskPaneFormatStyle(_groupsFormatStyle) : new TaskPaneFormatStyle(_specialGroupsFormatStyle));
				TaskPaneFormatStyle groupStyle = taskPaneFormatStyle.MergeWith(taskPaneGroup.StateStyles.FormatStyle);
				bool isGroupEnabled = base.Enabled && taskPaneGroup.Enabled;
				if ((taskPaneGroup.GetItemState() == ItemState.Focus || taskPaneGroup.GetItemState() == ItemState.SelectedFocus) && isGroupEnabled)
				{
					groupStyle = ((!taskPaneGroup.SpecialGroup) ? groupStyle.MergeWith(_hoverGroupFormatStyle) : groupStyle.MergeWith(_hoverSpecialGroupFormatStyle));
				}
				return groupStyle;
			}
			TaskPaneGroup ownerGroup = null;
			TaskPaneItem taskPaneItem = null;
			if (explorerArea == ExplorerArea.Items)
			{
				taskPaneItem = (TaskPaneItem)areaTarget;
				ownerGroup = taskPaneItem.Group;
			}
			else
			{
				ownerGroup = (TaskPaneGroup)areaTarget;
			}
			TaskPaneFormatStyle itemsStyle = new TaskPaneFormatStyle(_itemsFormatStyle);
			itemsStyle = itemsStyle.MergeWith(ownerGroup.ItemsStateStyles.FormatStyle);
			if (explorerArea == ExplorerArea.Items)
			{
				itemsStyle = itemsStyle.MergeWith(taskPaneItem.StateStyles.FormatStyle);
				bool isItemEnabled = base.Enabled && ownerGroup.Enabled && taskPaneItem.Enabled;
				if ((taskPaneItem._itemState == ItemState.Focus || taskPaneItem._itemState == ItemState.SelectedFocus) && isItemEnabled)
				{
					itemsStyle = itemsStyle.MergeWith(_hoverItemFormatStyle);
				}
				return itemsStyle;
			}
			return itemsStyle;
		}

		internal Brush GetAreaBackBrush(ExplorerArea explorerArea, object areaTarget)
		{
			switch (explorerArea)
			{
			case ExplorerArea.Background:
			{
				TaskPaneFormatStyle backgroundStyle = CreateDefaultFormatStyle();
				backgroundStyle = backgroundStyle.MergeWith(_backgroundFormatStyle);
				return backgroundStyle.GetBackBrush(base.ClientRectangle);
			}
			case ExplorerArea.GroupHeader:
			{
				TaskPaneFormatStyle headerStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				TaskPaneGroup headerGroup = (TaskPaneGroup)areaTarget;
				return headerStyle?.CreateBackBrush(headerGroup.GetHeaderBounds());
			}
			default:
			{
				TaskPaneGroup taskPaneGroup = null;
				TaskPaneItem taskPaneItem = null;
				if (explorerArea == ExplorerArea.Items)
				{
					taskPaneItem = (TaskPaneItem)areaTarget;
					TaskPaneFormatStyle taskPaneFormatStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
					return taskPaneFormatStyle.CreateBackBrush(taskPaneItem.Bounds);
				}
				taskPaneGroup = (TaskPaneGroup)areaTarget;
				TaskPaneFormatStyle groupStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				return groupStyle.CreateBackBrush(taskPaneGroup.GetItemsBounds());
			}
			}
		}

		internal Color GetAreaBackColor(ExplorerArea explorerArea, object areaTarget)
		{
			switch (explorerArea)
			{
			case ExplorerArea.Background:
				return GetEffectiveBackColor();
			case ExplorerArea.GroupHeader:
			{
				TaskPaneFormatStyle headerStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				_ = (TaskPaneGroup)areaTarget;
				return headerStyle?.BackColor ?? Color.Empty;
			}
			case ExplorerArea.Items:
			{
				_ = (TaskPaneItem)areaTarget;
				TaskPaneFormatStyle itemStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				return itemStyle.BackColor;
			}
			default:
			{
				_ = (TaskPaneGroup)areaTarget;
				TaskPaneFormatStyle taskPaneFormatStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				return taskPaneFormatStyle.BackColor;
			}
			}
		}

		internal Brush GetAreaForeBrush(ExplorerArea explorerArea, object areaTarget)
		{
			if (explorerArea == ExplorerArea.GroupHeader)
			{
				_ = (TaskPaneGroup)areaTarget;
				TaskPaneFormatStyle taskPaneFormatStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				return taskPaneFormatStyle.CreateForeBrush(GetEffectiveForeColor());
			}
			TaskPaneItem taskPaneItem = (TaskPaneItem)areaTarget;
			if (taskPaneItem.ItemType == ItemType.Separator)
			{
				return null;
			}
			TaskPaneFormatStyle itemStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
			return itemStyle.CreateForeBrush(GetEffectiveForeColor());
		}

		internal Color GetAreaForeColor(ExplorerArea explorerArea, object areaTarget)
		{
			if (explorerArea == ExplorerArea.GroupHeader)
			{
				_ = (TaskPaneGroup)areaTarget;
				TaskPaneFormatStyle taskPaneFormatStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
				return taskPaneFormatStyle.ForeColor;
			}
			TaskPaneItem taskPaneItem = (TaskPaneItem)areaTarget;
			if (taskPaneItem.ItemType == ItemType.Separator)
			{
				return Color.Empty;
			}
			TaskPaneFormatStyle itemStyle = ResolveAreaFormatStyle(explorerArea, areaTarget);
			return itemStyle.ForeColor;
		}

	}
}
