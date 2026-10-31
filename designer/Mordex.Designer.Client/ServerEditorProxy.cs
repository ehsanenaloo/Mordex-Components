using System;
using System.ComponentModel;
using System.Drawing.Design;
using Microsoft.DotNet.DesignTools.Client;
using Mordex.Designer.Protocol;

namespace Mordex.Designer.Client
{
	// Runs the real editor inside the Design Tools Server, where the control assemblies live.
	internal sealed class ServerEditorProxy : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider? provider, object? value)
		{
			if (provider == null || context == null || context.PropertyDescriptor == null)
			{
				return value;
			}
			DesignerSession session = (DesignerSession)provider.GetService(typeof(DesignerSession));
			IDesignToolsClient client = (IDesignToolsClient)provider.GetService(typeof(IDesignToolsClient));
			RunServerEditorEndpoint endpoint = client.Protocol.GetEndpoint<RunServerEditorEndpoint>();
			endpoint.GetSender(client).SendRequest(new RunServerEditorRequest(session.Id, context.Instance, context.PropertyDescriptor.Name));
			return value;
		}
	}
}
