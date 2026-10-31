using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using Mordex.Designer.Protocol;

namespace Mordex.Designer.Server
{
	[ExportRequestHandler(EndpointNames.RunServerEditor)]
	internal sealed class RunServerEditorHandler : RequestHandler<RunServerEditorRequest, RunServerEditorResponse>
	{
		public override RunServerEditorResponse HandleRequest(RunServerEditorRequest request)
		{
			IDesignerHost host = GetDesignerHost(request.SessionId);
			object? instance = request.Instance;
			if (instance == null)
			{
				return RunServerEditorResponse.Empty;
			}
			PropertyDescriptor? property = TypeDescriptor.GetProperties(instance)[request.PropertyName];
			UITypeEditor? editor = property?.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
			if (property == null || editor == null)
			{
				return RunServerEditorResponse.Empty;
			}
			EditorServices services = new EditorServices(host, instance, property);
			object? current = property.GetValue(instance);
			using (DesignerTransaction transaction = host.CreateTransaction("Edit " + property.DisplayName))
			{
				object? edited = editor.EditValue(services, services, current);
				if (!Equals(edited, current) && !property.IsReadOnly)
				{
					services.OnComponentChanging();
					property.SetValue(instance, edited);
					services.OnComponentChanged();
				}
				transaction.Commit();
			}
			return RunServerEditorResponse.Empty;
		}
	}
}
