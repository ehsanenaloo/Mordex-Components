using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Timeline.Design
{
	[CLSCompliant(false)]
	public class TimelineDesigner : ControlDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[3]
				{
					new DesignerVerb("Timeline Designer...", OnDesigner),
					new DesignerVerb("Retrieve Fields", OnRetrieveFields),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public TimelineDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxTimeline)Control);
		}

		private void OnRetrieveFields(object sender, EventArgs eventArgs)
		{
			if (Control is MxTimeline)
			{
				MxTimeline timeline = (MxTimeline)Control;
				if (timeline.Fields.Count <= 0 || MessageBox.Show("This operation will overwrite the fields you currently have. \n\r\n\rDo you want to continue?", "Mordex Timeline Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.No)
				{
					DesignerChange.Run(timeline, "LayoutData", "Retrieve Fields", () => timeline.RetrieveFields());
				}
			}
		}

		private void OnDesigner(object sender, EventArgs eventArgs)
		{
			try
			{
				MxTimeline timeline = (MxTimeline)Control;
				DesignerChange.Run(timeline, "LayoutData", "Timeline Designer", () => DesignHelper.ShowTimelineDesigner(timeline) == DialogResult.OK);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
