using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	[CLSCompliant(false)]
	public class EditBaseDesigner : ControlDesigner
	{
		public override IList<SnapLine> SnapLines
		{
			get
			{
				IList<SnapLine> arrayList = base.SnapLines;
				if (Control is EditBase edit)
				{
					int snapLinesOffset = DesignHelper.GetSnapLinesOffset(edit);
					if (snapLinesOffset > 0)
					{
						arrayList.Add(new SnapLine(SnapLineType.Baseline, snapLinesOffset, SnapLinePriority.Medium));
					}
				}
				return arrayList;
			}
		}

		public EditBaseDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			base.AutoResizeHandles = true;
		}

		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);
			if (base.Component is EditBase editBase)
			{
				editBase.Text = "";
			}
		}
	}
}
