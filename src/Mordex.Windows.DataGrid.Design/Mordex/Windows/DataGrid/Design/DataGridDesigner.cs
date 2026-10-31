using System;
using Mordex.Windows.Common.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.DataGrid.Design
{
	[CLSCompliant(false)]
	public class DataGridDesigner : ControlDesigner
	{
		private bool mouseDown;

		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[5]
				{
					new DesignerVerb("DataGrid Designer...", OnDesigner),
					new DesignerVerb("Retrieve Structure", OnRetrieveStructure),
					new DesignerVerb("Retrieve Hierarchical Structure", OnRetrieveHierarchicalStructure),
					new DesignerVerb("Reset Defaults", OnClearProperties),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		private const string LayoutMember = "DesignTimeLayout";

		private bool gestureChanging;

		internal bool Locked => DesignerChange.IsReadOnly(Control);

		public DataGridDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private BehaviorService behaviorService;

		private Adorner forwardAdorner;

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			behaviorService = (BehaviorService)GetService(typeof(BehaviorService));
			if (behaviorService != null)
			{
				forwardAdorner = new Adorner();
				behaviorService.Adorners.Add(forwardAdorner);
				forwardAdorner.Glyphs.Add(new MouseForwardGlyph(behaviorService, (MxDataGrid)Control, CanForwardAt, BeginGesture, EndGesture));
				forwardAdorner.Enabled = true;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && behaviorService != null && forwardAdorner != null)
			{
				behaviorService.Adorners.Remove(forwardAdorner);
			}
			base.Dispose(disposing);
		}

		// The Design Tools Server sends mouse input to glyph behaviors, so header and group-by-box gestures are forwarded to the grid from here.
		private bool CanForwardAt(Point pt)
		{
			try
			{
				MxDataGrid dataGrid = (MxDataGrid)Control;
				switch (dataGrid.HitTest(pt.X, pt.Y))
				{
				case GridArea.GroupByBoxHeader:
				case GridArea.ColumnHeader:
				case GridArea.ColumnSetHeader:
				case GridArea.ExpandBox:
					return !Locked;
				case GridArea.None:
					return !Locked && Control.GetChildAtPoint(pt) != null;
				default:
					return false;
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return false;
			}
		}

		private void OnRetrieveStructure(object sender, EventArgs eventArgs)
		{
			if (Control is MxDataGrid)
			{
				MxDataGrid dataGrid = (MxDataGrid)Control;
				if (dataGrid.BoundMode == BoundMode.Unbound)
				{
					MessageBox.Show("Can not retrieve structure in an unbound grid.", "Mordex DataGrid Control", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return;
				}
				if (dataGrid.RootTable == null || MessageBox.Show("This operation will overwrite the Table-Column layout you currently have. \n\r\n\rDo you want to continue?", "Mordex DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.No)
				{
					DesignerChange.Run(dataGrid, LayoutMember, "Retrieve Structure", () => dataGrid.RetrieveStructure(retrieveChildTables: false));
				}
			}
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxDataGrid)Control);
		}

		private void OnRetrieveHierarchicalStructure(object sender, EventArgs eventArgs)
		{
			if (Control is MxDataGrid)
			{
				MxDataGrid dataGrid = (MxDataGrid)Control;
				if (dataGrid.BoundMode == BoundMode.Unbound)
				{
					MessageBox.Show("Can not retrieve structure in an unbound grid.", "Mordex DataGrid Control", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					return;
				}
				if (dataGrid.RootTable == null || MessageBox.Show("This operation will overwrite the Table-Column layout you currently have. \n\r\n\rDo you want to continue?", "Mordex DataGrid Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.No)
				{
					DesignerChange.Run(dataGrid, LayoutMember, "Retrieve Hierarchical Structure", () => dataGrid.RetrieveStructure(retrieveChildTables: true));
				}
			}
		}

		private void OnDesigner(object sender, EventArgs eventArgs)
		{
			MxDataGrid dataGrid = (MxDataGrid)Control;
			DesignerChange.Run(dataGrid, LayoutMember, "DataGrid Designer", () => DesignHelper.ShowGridDesigner(dataGrid) == DialogResult.OK);
		}

		private void OnClearProperties(object sender, EventArgs eventArgs)
		{
			if (MessageBox.Show("Are you sure you want to lose all your settings in the control?", "DataGrid Control", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				MxDataGrid dataGrid = (MxDataGrid)Control;
				DesignerChange.Run(dataGrid, LayoutMember, "Reset Defaults", () => dataGrid.Reset());
			}
		}

		// A header or group-by-box gesture changes the layout, so ComponentChanging is raised before the grid sees the mouse.
		private bool BeginGesture()
		{
			if (gestureChanging)
			{
				return true;
			}
			if (Locked)
			{
				return false;
			}
			IComponentChangeService changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
			try
			{
				changeService?.OnComponentChanging(Control, TypeDescriptor.GetProperties(Control)[LayoutMember]);
			}
			catch (CheckoutException)
			{
				return false;
			}
			gestureChanging = true;
			return true;
		}

		private void EndGesture()
		{
			if (!gestureChanging)
			{
				return;
			}
			gestureChanging = false;
			IComponentChangeService changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
			changeService?.OnComponentChanged(Control, TypeDescriptor.GetProperties(Control)[LayoutMember], null, null);
		}

		protected override void WndProc(ref Message msg)
		{
			switch (msg.Msg)
			{
			case 512:
				if (mouseDown)
				{
					DefWndProc(ref msg);
					return;
				}
				break;
			case 32:
				DefWndProc(ref msg);
				return;
			case 514:
				if (mouseDown)
				{
					mouseDown = false;
					if (gestureChanging)
					{
						DefWndProc(ref msg);
						EndGesture();
					}
					else
					{
						((MxDataGrid)Control).Refresh();
					}
					return;
				}
				break;
			case 513:
				try
				{
					MxDataGrid dataGrid = (MxDataGrid)Control;
					Point pt = DesignMouse.ClientPoint(dataGrid, msg);
					switch (dataGrid.HitTest(pt.X, pt.Y))
					{
					case GridArea.GroupByBoxHeader:
					case GridArea.ColumnHeader:
					case GridArea.ColumnSetHeader:
					case GridArea.ExpandBox:
						if (!BeginGesture())
						{
							break;
						}
						DefWndProc(ref msg);
						mouseDown = true;
						return;
					case GridArea.None:
					{
						Control childAtPoint = Control.GetChildAtPoint(pt);
						if (childAtPoint != null && BeginGesture())
						{
							DefWndProc(ref msg);
							mouseDown = true;
							return;
						}
						break;
					}
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				break;
			}
			base.WndProc(ref msg);
		}
	}
}
