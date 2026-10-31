using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Controls.Design
{
	[CLSCompliant(false)]
	public class BaseEditControlDesigner : ControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.AllSizeable | SelectionRules.Moveable | SelectionRules.Visible;

		public override IList<SnapLine> SnapLines
		{
			get
			{
				IList<SnapLine> arrayList = base.SnapLines;
				if (Control is IBaseLineControl)
				{
					IBaseLineControl baseLineControl = Control as IBaseLineControl;
					if (baseLineControl.Offset > 0)
					{
						arrayList.Add(new SnapLine(SnapLineType.Baseline, baseLineControl.Offset, SnapLinePriority.Medium));
					}
				}
				return arrayList;
			}
		}

		public BaseEditControlDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			base.AutoResizeHandles = true;
		}
	}
}
