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
		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(_layoutLoadEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_layoutLoadEvent, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentLayoutChangedEvent, value);
			}
		}

		[Description("Occurs before the CurrentLayout property has changed")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentLayoutChangingEvent, value);
			}
		}

		[Description("Event fired when the SelectedTab is about to be closed.")]
		public event TabCancelEventHandler ClosingTab
		{
			add
			{
				base.Events.AddHandler(_closingTabEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_closingTabEvent, value);
			}
		}

		[Description("Event fired when the SelectedTab is closed.")]
		public event TabEventHandler TabClosed
		{
			add
			{
				base.Events.AddHandler(_tabClosedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_tabClosedEvent, value);
			}
		}

		[Description("Event fired when the SelectedTab property has changed.")]
		public event TabEventHandler SelectedTabChanged
		{
			add
			{
				base.Events.AddHandler(_selectedTabChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedTabChangedEvent, value);
			}
		}

		[Description("Event fired when a new UITabPage is about to be selected")]
		public event TabCancelEventHandler ChangingSelectedTab
		{
			add
			{
				base.Events.AddHandler(_changingSelectedTabEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_changingSelectedTabEvent, value);
			}
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[_layoutLoadEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[_currentLayoutChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[_currentLayoutChangingEvent] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnClosingTab(TabCancelEventArgs e)
		{
			if (base.Events[_closingTabEvent] is TabCancelEventHandler tabCancelEventHandler)
			{
				tabCancelEventHandler(this, e);
			}
		}

		protected virtual void OnTabClosed(TabEventArgs e)
		{
			if (base.Events[_tabClosedEvent] is TabEventHandler tabEventHandler)
			{
				tabEventHandler(this, e);
			}
		}

		protected virtual void OnSelectedTabChanged(TabEventArgs e)
		{
			InvalidateCaptionTabs();
			OnSelectedTabChangedForCaption();
			if (base.Events[_selectedTabChangedEvent] is TabEventHandler tabEventHandler)
			{
				tabEventHandler(this, e);
			}
		}

		protected virtual void OnChangingSelectedTab(TabCancelEventArgs e)
		{
			if (base.Events[_changingSelectedTabEvent] is TabCancelEventHandler tabCancelEventHandler)
			{
				tabCancelEventHandler(this, e);
			}
		}
	}
}
