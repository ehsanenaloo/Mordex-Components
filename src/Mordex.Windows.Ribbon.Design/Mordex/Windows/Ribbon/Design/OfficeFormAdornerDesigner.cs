using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class OfficeFormAdornerDesigner : ComponentDesigner
	{
		private MxRibbonFormAdorner _formAdorner;

		private IDesignerHost _designerHost;

		public OfficeFormAdornerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			_designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			if (_designerHost != null)
			{
				_designerHost.LoadComplete += OnDesignerHostLoadComplete;
				_formAdorner = (MxRibbonFormAdorner)base.Component;
				if (_formAdorner.Form == null && _designerHost.RootComponent is Form)
				{
					_formAdorner.Form = (Form)_designerHost.RootComponent;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_designerHost != null)
			{
				_designerHost.LoadComplete -= OnDesignerHostLoadComplete;
			}
			base.Dispose(disposing);
		}

		private void OnDesignerHostLoadComplete(object sender, EventArgs eventArgs)
		{
			DesignHelper.PerformFormSubclassVisibleChanged(_formAdorner);
		}
	}
}
