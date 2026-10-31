using System;
using Microsoft.DotNet.DesignTools.Designers;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Controls.Design
{
	[CLSCompliant(false)]
	public class ColorPickerDesigner : ControlDesigner
	{
		public ColorPickerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
