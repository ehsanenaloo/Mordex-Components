using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Design;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class CommandManagerDesigner : ComponentDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[2]
				{
					new DesignerVerb("CommandManager Designer...", OnCustomize),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		private IDesignerHost _designerHost;

		public CommandManagerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_designerHost != null)
				{
					_designerHost.Deactivated -= OnDeactivated;
					_designerHost.Activated -= OnActivated;
					_designerHost = null;
				}
			}
			base.Dispose(disposing);
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			if (designerHost != null)
			{
				MxCommandManager uiCommandManager = (MxCommandManager)base.Component;
				designerHost.Deactivated += OnDeactivated;
				designerHost.Activated += OnActivated;
				_designerHost = designerHost;
				if (uiCommandManager.ContainerControl == null && designerHost.RootComponent is ContainerControl)
				{
					uiCommandManager.ContainerControl = (ContainerControl)designerHost.RootComponent;
				}
			}
		}

		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);
			if (base.Component is MxCommandManager commandManager)
			{
				DesignHelper.OnCommandManagerInitialize(commandManager);
			}
		}

		private void OnDeactivated(object sender, EventArgs eventArgs)
		{
			if (base.Component != null)
			{
				MxCommandManager commandManager = (MxCommandManager)base.Component;
				DesignHelper.SetFloatingBarsVisibility(commandManager, visible: false);
			}
		}

		private void OnActivated(object sender, EventArgs eventArgs)
		{
			if (base.Component != null)
			{
				MxCommandManager commandManager = (MxCommandManager)base.Component;
				DesignHelper.SetFloatingBarsVisibility(commandManager, visible: true);
			}
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxCommandManager)base.Component);
		}

		private void OnCustomize(object sender, EventArgs eventArgs)
		{
			if (base.Component is MxCommandManager commandManager)
			{
				DesignerChange.Run(commandManager, null, "CommandManager Designer", () => DesignHelper.ShowCommandBarsDesignerDialog(commandManager) == DialogResult.OK);
			}
		}
	}
}
