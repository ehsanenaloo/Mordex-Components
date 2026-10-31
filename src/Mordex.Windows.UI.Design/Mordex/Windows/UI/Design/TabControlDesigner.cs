using System;
using Mordex.Windows.Common.Design;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class TabControlDesigner : ControlDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[3]
				{
					new DesignerVerb("Add Tab", OnTabAdded),
					new DesignerVerb("Remove Tab", OnTabRemoved),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public TabControlDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxTabControl)Control);
		}

		private void OnTabAdded(object sender, EventArgs eventArgs)
		{
			if (!(Control is MxTabControl uiTab))
			{
				return;
			}
			IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
			DesignerChange.Run(uiTab, "TabPages", "Add Tab", () =>
			{
				MxTabPage page = host != null ? (MxTabPage)host.CreateComponent(typeof(MxTabPage)) : new MxTabPage();
				page.Text = "New Tab";
				uiTab.TabPages.Add(page);
			});
		}

		private void OnTabRemoved(object sender, EventArgs eventArgs)
		{
			if (!(Control is MxTabControl uiTab) || uiTab.SelectedTab == null)
			{
				return;
			}
			MxTabPage page = uiTab.SelectedTab;
			IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
			try
			{
				DesignerChange.Run(uiTab, "TabPages", "Remove Tab", () =>
				{
					uiTab.TabPages.Remove(page);
					if (host != null)
					{
						DestroyWithChildren(host, page);
					}
				});
			}
			catch (Exception ex) when (ex is CheckoutException || ex is InvalidOperationException || ex is ArgumentException)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// Destroys the sited controls inside a removed page first, so none of them stays in the designer container.
		private static void DestroyWithChildren(IDesignerHost host, Control control)
		{
			Control[] children = new Control[control.Controls.Count];
			control.Controls.CopyTo(children, 0);
			foreach (Control child in children)
			{
				DestroyWithChildren(host, child);
			}
			if (control.Site != null && control.Site.Container == host.Container)
			{
				host.DestroyComponent(control);
			}
		}

		protected override void WndProc(ref Message msg)
		{
			MxTabControl uiTab = (MxTabControl)Control;
			int message = msg.Msg;
			if (message == NativeMethods.WM_LBUTTONDOWN)
			{
				Point position = DesignMouse.ClientPoint(uiTab, msg);
				if (DesignHelper.TryTabScroll(uiTab))
				{
					return;
				}
				MxTabPage tabPage = null;
				if (DesignHelper.TabPageFromPoint(uiTab, position, ref tabPage))
				{
					DefWndProc(ref msg);
					if (tabPage != null)
					{
						uiTab.Update();
						ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
						if (selectionService != null)
						{
							ArrayList arrayList = new ArrayList();
							arrayList.Add(tabPage);
							selectionService.SetSelectedComponents(arrayList);
						}
					}
				}
				else
				{
					base.WndProc(ref msg);
				}
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		private BehaviorService _behaviorService;

		private Adorner _selectAdorner;

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			_behaviorService = (BehaviorService)GetService(typeof(BehaviorService));
			if (_behaviorService != null)
			{
				_selectAdorner = new Adorner();
				_behaviorService.Adorners.Add(_selectAdorner);
				_selectAdorner.Glyphs.Add(new ClickSelectGlyph(_behaviorService, (MxTabControl)Control, CanSelectPageAt, SelectPageAt));
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

		// The Design Tools Server sends mouse input to glyph behaviors, so tab pages are selected here instead of in WndProc.
		private bool CanSelectPageAt(Point clientPoint)
		{
			MxTabPage tabPage = null;
			return DesignHelper.TabPageFromPoint((MxTabControl)Control, clientPoint, ref tabPage) && tabPage != null;
		}

		private bool SelectPageAt(Point clientPoint, MouseButtons button)
		{
			MxTabControl uiTab = (MxTabControl)Control;
			MxTabPage tabPage = null;
			if (!DesignHelper.TabPageFromPoint(uiTab, clientPoint, ref tabPage) || tabPage == null)
			{
				return false;
			}
			uiTab.SelectedTab = tabPage;
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService == null)
			{
				return false;
			}
			selectionService.SetSelectedComponents(new ArrayList { tabPage });
			return true;
		}
	}
}
