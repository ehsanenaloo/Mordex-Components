using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[CLSCompliant(false)]
	public class SchedulerDesigner : ControlDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[3]
				{
					new DesignerVerb("Schedule Designer...", OnDesigner),
					new DesignerVerb("Retrieve Fields", OnRetrieveFields),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public SchedulerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnRetrieveFields(object sender, EventArgs eventArgs)
		{
			if (Control is MxScheduler)
			{
				MxScheduler scheduler = (MxScheduler)Control;
				if (scheduler.Fields.Count <= 0 || MessageBox.Show("This operation will overwrite the fields you currently have.\nDo you want to continue?", "Mordex Schedule Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.No)
				{
					DesignerChange.Run(scheduler, "LayoutData", "Retrieve Fields", () => scheduler.RetrieveFields());
				}
			}
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxScheduler)Control);
		}

		private void OnDesigner(object sender, EventArgs eventArgs)
		{
			try
			{
				MxScheduler scheduler = (MxScheduler)Control;
				DesignerChange.Run(scheduler, "LayoutData", "Schedule Designer", () => DesignHelper.ShowSchedulerDesigner(scheduler) == DialogResult.OK);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
