using System;
using Mordex.Windows.Common.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.TaskPane.Design
{
	[CLSCompliant(false)]
	public class TaskPaneDesigner : ParentControlDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] verbs = new DesignerVerb[2]
				{
					new DesignerVerb("TaskPane Designer", OnDefineGroups),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(verbs);
			}
		}

		public TaskPaneDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			EnableDragDrop(value: true);
			DrawGrid = false;
			_behaviorService = (BehaviorService)GetService(typeof(BehaviorService));
			if (_behaviorService != null)
			{
				_selectAdorner = new Adorner();
				_behaviorService.Adorners.Add(_selectAdorner);
				MxTaskPane bar = (MxTaskPane)Control;
				_selectAdorner.Glyphs.Add(new MouseForwardGlyph(_behaviorService, bar, DesignHelper.GetScrollBar(bar), location => DesignHelper.GetScrollBarBounds(bar).Contains(location), null));
				_selectAdorner.Glyphs.Add(new ClickSelectGlyph(_behaviorService, (MxTaskPane)Control, CanToggleAt, RememberGroupAt, ToggleGroupAt));
				_selectAdorner.Enabled = true;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _behaviorService != null && _selectAdorner != null)
			{
				_behaviorService.Adorners.Remove(_selectAdorner);
			}
			base.Dispose(disposing);
		}

		private BehaviorService _behaviorService;

		private Adorner _selectAdorner;

		// The Design Tools Server sends mouse input to glyph behaviors, so expanding a group by click lives here instead of in WndProc.
		private bool CanToggleAt(Point location)
		{
			MxTaskPane taskPane = (MxTaskPane)Control;
			TaskPaneGroup group = taskPane.GetGroupAt(location);
			return group != null && group.Expandable && !DesignHelper.GetScrollBarBounds(taskPane).Contains(location);
		}

		private TaskPaneGroup _pressedGroup;

		private bool RememberGroupAt(Point location, MouseButtons button)
		{
			_pressedGroup = CanToggleAt(location) ? ((MxTaskPane)Control).GetGroupAt(location) : null;
			return false;
		}

		private bool ToggleGroupAt(Point location, MouseButtons button)
		{
			MxTaskPane taskPane = (MxTaskPane)Control;
			TaskPaneGroup group = taskPane.GetGroupAt(location);
			TaskPaneGroup pressed = _pressedGroup;
			_pressedGroup = null;
			if (group == null || group != pressed || !group.Expandable)
			{
				return false;
			}
			DesignerChange.Run(taskPane, "Groups", group.Expanded ? "Collapse Group" : "Expand Group", () => { group.Expanded = !group.Expanded; });
			return false;
		}

		private void OnDefineGroups(object sender, EventArgs eventArgs)
		{
			try
			{
				MxTaskPane taskPane = (MxTaskPane)Control;
				DesignerChange.Run(taskPane, "Groups", "TaskPane Designer", () => DesignHelper.ShowGroupsDesigner(taskPane) == DialogResult.OK);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxTaskPane)Control);
		}

		public override bool CanParent(Control control)
		{
			if (control.GetType() == typeof(TaskPaneContainerControl))
			{
				return true;
			}
			return base.CanParent(control);
		}
	}
}
