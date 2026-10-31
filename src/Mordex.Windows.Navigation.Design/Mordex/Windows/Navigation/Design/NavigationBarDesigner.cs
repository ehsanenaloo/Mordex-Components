using System;
using Mordex.Windows.Common.Design;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Navigation.Design
{
	[CLSCompliant(false)]
	public class NavigationBarDesigner : ParentControlDesigner
	{

		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] verbs = new DesignerVerb[5]
				{
					new DesignerVerb("NavigationBar Designer", OnDefineGroups),
					new DesignerVerb("Auto Format", OnAutoFormat),
					new DesignerVerb("Add Group", OnAddGroup),
					new DesignerVerb("Add Item", OnAddItem),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(verbs);
			}
		}

		public NavigationBarDesigner()
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
				_selectAdorner.Glyphs.Add(new ClickSelectGlyph(_behaviorService, (MxNavigationBar)Control, CanHandleAt, null, HandleMouseUp));
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

		// The Design Tools Server sends mouse input to glyph behaviors, so group selection and scrolling live here instead of in WndProc.
		private bool CanHandleAt(Point clientPoint)
		{
			MxNavigationBar navigationBar = (MxNavigationBar)Control;
			foreach (NavigationGroup group in navigationBar.Groups)
			{
				if (DesignHelper.GetGroupBounds(group).Contains(clientPoint))
				{
					return true;
				}
			}
			return DesignHelper.IsPointOverScrollDown(navigationBar, clientPoint) || DesignHelper.IsPointOverScrollUp(navigationBar, clientPoint);
		}

		private bool HandleMouseUp(Point clientPoint, MouseButtons button)
		{
			MxNavigationBar navigationBar = (MxNavigationBar)Control;
			foreach (NavigationGroup group in navigationBar.Groups)
			{
				if (DesignHelper.GetGroupBounds(group).Contains(clientPoint))
				{
					navigationBar.SelectedGroup = group;
				}
			}
			try
			{
				if (DesignHelper.IsPointOverScrollDown(navigationBar, clientPoint))
				{
					DesignHelper.ScrollItemsDown(navigationBar);
				}
				else if (DesignHelper.IsPointOverScrollUp(navigationBar, clientPoint))
				{
					DesignHelper.ScrollItemsUp(navigationBar);
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
			return false;
		}

		public override bool CanParent(Control control)
		{
			if (control.GetType() == typeof(NavigationBarContainerControl))
			{
				return true;
			}
			return false;
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxNavigationBar)Control);
		}

		private void OnDefineGroups(object sender, EventArgs eventArgs)
		{
			MxNavigationBar navigationBar = (MxNavigationBar)Control;
			DesignerChange.Run(navigationBar, "Groups", "NavigationBar Designer", () =>
			{
				using (GroupsDesigner groupsDesigner = new GroupsDesigner())
				{
					groupsDesigner.NavigationBar = navigationBar;
					groupsDesigner.ShowDialog();
					return groupsDesigner.DialogResult == DialogResult.OK;
				}
			});
		}

		private void OnAddItem(object sender, EventArgs eventArgs)
		{
			MxNavigationBar navigationBar = (MxNavigationBar)Control;
			DesignerChange.Run(navigationBar, "Groups", "Add Item", () => AddItem(navigationBar));
		}

		private static void AddItem(MxNavigationBar navigationBar)
		{
			if (navigationBar.SelectedGroup == null)
			{
				AddGroup(navigationBar);
			}
			NavigationItem navigationBarItem = new NavigationItem("New Item");
			ArrayList arrayList = new ArrayList(navigationBar.SelectedGroup.Items.Count);
			foreach (NavigationItem item in navigationBar.SelectedGroup.Items)
			{
				arrayList.Add(item);
			}
			navigationBarItem.Key = Utils.GetObjectKey(typeof(NavigationItem), "Item", 1, arrayList);
			navigationBar.SelectedGroup.Items.Add(navigationBarItem);
		}

		private void OnAddGroup(object sender, EventArgs eventArgs)
		{
			MxNavigationBar navigationBar = (MxNavigationBar)Control;
			DesignerChange.Run(navigationBar, "Groups", "Add Group", () => AddGroup(navigationBar));
		}

		private static void AddGroup(MxNavigationBar navigationBar)
		{
			NavigationGroup navigationBarGroup = new NavigationGroup("New Group");
			ArrayList arrayList = new ArrayList(navigationBar.Groups.Count);
			foreach (NavigationGroup group in navigationBar.Groups)
			{
				arrayList.Add(group);
			}
			navigationBarGroup.Key = Utils.GetObjectKey(typeof(NavigationGroup), "Group", 1, arrayList);
			navigationBar.Groups.Add(navigationBarGroup);
		}

		private void OnAutoFormat(object sender, EventArgs eventArgs)
		{
			MxNavigationBar navigationBar = (MxNavigationBar)Control;
			DesignerChange.Run(navigationBar, null, "Auto Format", () =>
			{
				using (AutoFormatDialog autoFormatDialog = new AutoFormatDialog())
				{
					autoFormatDialog.NavigationBar = navigationBar;
					autoFormatDialog.ShowDialog();
					return autoFormatDialog.DialogResult == DialogResult.OK;
				}
			});
		}

		protected override void WndProc(ref Message msg)
		{
			int messageId = msg.Msg;
			if (messageId == 514)
			{
				MxNavigationBar navigationBar = (MxNavigationBar)Control;
				Point clientPoint = DesignMouse.ClientPoint(navigationBar, msg);
				foreach (NavigationGroup group in navigationBar.Groups)
				{
					if (DesignHelper.GetGroupBounds(group).Contains(clientPoint))
					{
						navigationBar.SelectedGroup = group;
					}
				}
				try
				{
					if (DesignHelper.IsPointOverScrollDown(navigationBar, clientPoint))
					{
						DesignHelper.ScrollItemsDown(navigationBar);
					}
					else if (DesignHelper.IsPointOverScrollUp(navigationBar, clientPoint))
					{
						DesignHelper.ScrollItemsUp(navigationBar);
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
				}
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}
	}
}
