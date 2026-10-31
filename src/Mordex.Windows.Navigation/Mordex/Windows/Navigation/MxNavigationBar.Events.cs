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
using System.Runtime.CompilerServices;
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
		private string _settingsKey;

		private string _defaultGroupImageKey = "";

		private string _defaultItemSmallImageKey = "";

		private string _defaultItemLargeImageKey = "";

		private static readonly object SelectingGroupKey = new object();

		private static readonly object GroupSelectedKey = new object();

		private static readonly object SelectingItemKey = new object();

		private static readonly object ItemSelectedKey = new object();

		private static readonly object ItemClickKey = new object();

		private static readonly object GroupClickKey = new object();

		private static readonly object GroupRenamedKey = new object();

		private static readonly object ItemRenamedKey = new object();

		private static readonly object DrawAreaKey = new object();

		private static readonly object LayoutLoadKey = new object();

		private static readonly object CurrentLayoutChangedKey = new object();

		private static readonly object CurrentLayoutChangingKey = new object();

		[Description("Event fired when a new NavigationGroup is about to be selected by either a programmatic modification or user interaction.")]
		public event GroupCancelEventHandler SelectingGroup
		{
			add
			{
				base.Events.AddHandler(SelectingGroupKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(SelectingGroupKey, value);
			}
		}

		[Description("Event fired when a new NavigationGroup is selected by either a programmatic modification or user interaction.")]
		public event EventHandler GroupSelected
		{
			add
			{
				base.Events.AddHandler(GroupSelectedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupSelectedKey, value);
			}
		}

		[Description("Event fired when a new NavigationItem is about to be selected by either a programmatic modification or user interaction.")]
		public event ItemCancelEventHandler SelectingItem
		{
			add
			{
				base.Events.AddHandler(SelectingItemKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(SelectingItemKey, value);
			}
		}

		[Description("Event fired when a new NavigationItem is selected by either a programmatic modification or user interaction.")]
		public event EventHandler ItemSelected
		{
			add
			{
				base.Events.AddHandler(ItemSelectedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(ItemSelectedKey, value);
			}
		}

		[Description("Event fired when the user clicks on a NavigationItem.")]
		public event ItemEventHandler ItemClick
		{
			add
			{
				base.Events.AddHandler(ItemClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(ItemClickKey, value);
			}
		}

		[Description("Event fired when the user clicks on a NavigationGroup.")]
		public event GroupEventHandler GroupClick
		{
			add
			{
				base.Events.AddHandler(GroupClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupClickKey, value);
			}
		}

		[Description("Event fired when the user renamed a NavigationGroup.")]
		public event GroupEventHandler GroupRenamed
		{
			add
			{
				base.Events.AddHandler(GroupRenamedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupRenamedKey, value);
			}
		}

		[Description("Event fired when the user renamed a NavigationItem.")]
		public event ItemEventHandler ItemRenamed
		{
			add
			{
				base.Events.AddHandler(ItemRenamedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(ItemRenamedKey, value);
			}
		}

		public event DrawNavigationBarAreaEventHandler DrawNavigationBarArea
		{
			add
			{
				base.Events.AddHandler(DrawAreaKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(DrawAreaKey, value);
			}
		}

		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(LayoutLoadKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(LayoutLoadKey, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(CurrentLayoutChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(CurrentLayoutChangedKey, value);
			}
		}

		[Description("Occurs before the CurrentLayout property changes")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(CurrentLayoutChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(CurrentLayoutChangingKey, value);
			}
		}

		internal void OnItemClickCore(NavigationItem item)
		{
			ItemEventArgs e = new ItemEventArgs(item);
			OnItemClick(e);
		}

		internal void OnDrawNavigationBarAreaCore(DrawNavigationBarAreaEventArgs eventArgs)
		{
			OnDrawNavigationBarArea(eventArgs);
		}

		protected virtual void OnSelectingGroup(GroupCancelEventArgs e)
		{
			if (base.Events[SelectingGroupKey] is GroupCancelEventHandler groupCancelEventHandler)
			{
				groupCancelEventHandler(this, e);
			}
		}

		protected virtual void OnGroupSelected(EventArgs e)
		{
			if (base.Events[GroupSelectedKey] is EventHandler groupSelectedHandler)
			{
				groupSelectedHandler(this, e);
			}
		}

		protected virtual void OnSelectingItem(ItemCancelEventArgs e)
		{
			if (base.Events[SelectingItemKey] is ItemCancelEventHandler itemCancelEventHandler)
			{
				itemCancelEventHandler(this, e);
			}
		}

		protected virtual void OnItemSelected(EventArgs e)
		{
			if (base.Events[ItemSelectedKey] is EventHandler itemSelectedHandler)
			{
				itemSelectedHandler(this, e);
			}
		}

		protected virtual void OnItemClick(ItemEventArgs e)
		{
			if (base.Events[ItemClickKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		protected virtual void OnGroupClick(GroupEventArgs e)
		{
			if (base.Events[GroupClickKey] is GroupEventHandler groupEventHandler)
			{
				groupEventHandler(this, e);
			}
		}

		protected virtual void OnGroupRenamed(GroupEventArgs e)
		{
			if (base.Events[GroupRenamedKey] is GroupEventHandler groupEventHandler)
			{
				groupEventHandler(this, e);
			}
		}

		protected virtual void OnItemRenamed(ItemEventArgs e)
		{
			if (base.Events[ItemRenamedKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		protected virtual void OnDrawNavigationBarArea(DrawNavigationBarAreaEventArgs e)
		{
			if (base.Events[DrawAreaKey] is DrawNavigationBarAreaEventHandler drawAreaHandler)
			{
				drawAreaHandler(this, e);
			}
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[LayoutLoadKey] is EventHandler layoutLoadHandler)
			{
				layoutLoadHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[CurrentLayoutChangedKey] is EventHandler currentLayoutChangedHandler)
			{
				currentLayoutChangedHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[CurrentLayoutChangingKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}
	}
}
