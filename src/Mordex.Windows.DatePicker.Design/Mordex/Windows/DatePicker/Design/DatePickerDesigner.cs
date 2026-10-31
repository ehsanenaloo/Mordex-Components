using System;
using System.Collections.Generic;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.DatePicker.Design
{
	[CLSCompliant(false)]
	public class DatePickerDesigner : ControlDesigner
	{
		public override IList<SnapLine> SnapLines
		{
			get
			{
				IList<SnapLine> arrayList = base.SnapLines;
				if (Control is MxDatePicker combo)
				{
					int baseLineOffset = DesignHelper.GetBaseLineOffset(combo);
					if (baseLineOffset > 0)
					{
						arrayList.Add(new SnapLine(SnapLineType.Baseline, baseLineOffset, SnapLinePriority.Medium));
					}
				}
				return arrayList;
			}
		}

		public override SelectionRules SelectionRules => SelectionRules.AllSizeable | SelectionRules.Moveable | SelectionRules.Visible;

		public DatePickerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			base.AutoResizeHandles = true;
		}
	}
}
