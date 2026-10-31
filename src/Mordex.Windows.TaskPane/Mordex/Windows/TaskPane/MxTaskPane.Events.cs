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
using System.Runtime.CompilerServices;
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
		private string _settingsKey;

		private string _defaultItemImageKey = "";

		private string _defaultGroupImageKey = "";

		private static readonly object GroupExpandingKey = new object();

		private static readonly object GroupClosingKey = new object();

		private static readonly object GroupClosedKey = new object();

		private static readonly object GroupCollapsingKey = new object();

		private static readonly object GroupExpandedKey = new object();

		private static readonly object GroupCollapsedKey = new object();

		private static readonly object LayoutLoadKey = new object();

		private static readonly object CurrentLayoutChangedKey = new object();

		private static readonly object CurrentLayoutChangingKey = new object();

		private static readonly object ItemClickKey = new object();

		private static readonly object DrawTaskPaneAreaKey = new object();

		private static readonly object GroupClickKey = new object();

		[Description("Event fired when an TaskPaneGroup is about to be expanded by either a programmatic modification or user interaction.")]
		public event GroupCancelEventHandler GroupExpanding
		{
			add
			{
				base.Events.AddHandler(GroupExpandingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupExpandingKey, value);
			}
		}

		[Description("Event fired when an TaskPaneGroup is about to be closed by user interaction.")]
		public event GroupCancelEventHandler GroupClosing
		{
			add
			{
				base.Events.AddHandler(GroupClosingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupClosingKey, value);
			}
		}

		[Description("Event fired when an TaskPaneGroup is closed by user interaction.")]
		public event GroupEventHandler GroupClosed
		{
			add
			{
				base.Events.AddHandler(GroupClosedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupClosedKey, value);
			}
		}

		[Description("Event fired when an TaskPaneGroup is about to be collapsed by either a programmatic modification or user interaction.")]
		public event GroupCancelEventHandler GroupCollapsing
		{
			add
			{
				base.Events.AddHandler(GroupCollapsingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupCollapsingKey, value);
			}
		}

		[Description("Event fired when an TaskPaneGroup was expanded by either a programmatic modification or user interaction.")]
		public event GroupEventHandler GroupExpanded
		{
			add
			{
				base.Events.AddHandler(GroupExpandedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupExpandedKey, value);
			}
		}

		[Description("Event fired when an TaskPaneGroup was collapsed by either a programmatic modification or user interaction.")]
		public event GroupEventHandler GroupCollapsed
		{
			add
			{
				base.Events.AddHandler(GroupCollapsedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(GroupCollapsedKey, value);
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

		[Description("Event fired when the user clicks on an TaskPaneItem.")]
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

		public event DrawTaskPaneAreaEventHandler DrawTaskPaneArea
		{
			add
			{
				base.Events.AddHandler(DrawTaskPaneAreaKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(DrawTaskPaneAreaKey, value);
			}
		}

		[Description("Event fired when the user clicks on a TaskPaneGroup.")]
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

		internal void OnDrawTaskPaneAreaCore(DrawTaskPaneAreaEventArgs eventArgs)
		{
			OnDrawTaskPaneArea(eventArgs);
		}

		internal void OnItemClickCore(TaskPaneItem item)
		{
			ItemEventArgs e = new ItemEventArgs(item);
			OnItemClick(e);
		}

		protected virtual void OnGroupExpanding(GroupCancelEventArgs e)
		{
			if (base.Events[GroupExpandingKey] is GroupCancelEventHandler groupCancelEventHandler)
			{
				groupCancelEventHandler(this, e);
			}
		}

		protected virtual void OnGroupClosing(GroupCancelEventArgs e)
		{
			if (base.Events[GroupClosingKey] is GroupCancelEventHandler groupCancelEventHandler)
			{
				groupCancelEventHandler(this, e);
			}
		}

		protected virtual void OnGroupClosed(GroupEventArgs e)
		{
			if (base.Events[GroupClosedKey] is GroupEventHandler groupEventHandler)
			{
				groupEventHandler(this, e);
			}
		}

		protected virtual void OnGroupCollapsing(GroupCancelEventArgs e)
		{
			if (base.Events[GroupCollapsingKey] is GroupCancelEventHandler groupCancelEventHandler)
			{
				groupCancelEventHandler(this, e);
			}
		}

		protected virtual void OnGroupExpanded(GroupEventArgs e)
		{
			if (base.Events[GroupExpandedKey] is GroupEventHandler groupEventHandler)
			{
				groupEventHandler(this, e);
			}
		}

		protected virtual void OnGroupCollapsed(GroupEventArgs e)
		{
			if (base.Events[GroupCollapsedKey] is GroupEventHandler groupEventHandler)
			{
				groupEventHandler(this, e);
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
			if (base.Events[CurrentLayoutChangedKey] is EventHandler layoutChangedHandler)
			{
				layoutChangedHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[CurrentLayoutChangingKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnItemClick(ItemEventArgs e)
		{
			if (base.Events[ItemClickKey] is ItemEventHandler itemEventHandler)
			{
				itemEventHandler(this, e);
			}
		}

		protected virtual void OnDrawTaskPaneArea(DrawTaskPaneAreaEventArgs e)
		{
			if (base.Events[DrawTaskPaneAreaKey] is DrawTaskPaneAreaEventHandler drawAreaHandler)
			{
				drawAreaHandler(this, e);
			}
		}

		protected virtual void OnGroupClick(GroupEventArgs e)
		{
			if (base.Events[GroupClickKey] is GroupEventHandler groupEventHandler)
			{
				groupEventHandler(this, e);
			}
		}
	}
}
