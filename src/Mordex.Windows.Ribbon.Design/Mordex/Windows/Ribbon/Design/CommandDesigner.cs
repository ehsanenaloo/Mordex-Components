using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class CommandDesigner : ComponentDesigner, ITreeDesigner, IDesigner, IDisposable
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb designerVerb = CreateSelectParentVerb();
				if (designerVerb != null)
				{
					DesignerVerb[] value = new DesignerVerb[1] { designerVerb };
					return new DesignerVerbCollection(value);
				}
				return base.Verbs;
			}
		}

		public CommandDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private DesignerVerb CreateSelectParentVerb()
		{
			string text = null;
			Component parentComponent = GetParentComponent();
			if (parentComponent != null && parentComponent.Site != null)
			{
				text = parentComponent.Site.Name;
			}
			if (text != null)
			{
				return new DesignerVerb("Select '" + text + "'", OnSelectParent);
			}
			return null;
		}

		private void OnSelectParent(object sender, EventArgs eventArgs)
		{
			Component parentComponent = GetParentComponent();
			if (parentComponent != null)
			{
				DesignerDragHelper.SelectOnly(GetService(typeof(ISelectionService)) as ISelectionService, parentComponent);
			}
		}

		private Component GetParentComponent()
		{
			if (base.Component is CommandBase command)
			{
				Component owner = DesignHelper.GetOwner(command);
				if (owner != null && owner.Site != null)
				{
					return owner;
				}
			}
			return null;
		}
	}
}
