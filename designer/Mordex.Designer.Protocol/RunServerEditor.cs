using System;
using System.Composition;
using Microsoft.DotNet.DesignTools.Protocol;
using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace Mordex.Designer.Protocol
{
	public static class EndpointNames
	{
		public const string RunServerEditor = nameof(RunServerEditor);
	}

	[Shared]
	[ExportEndpoint]
	public class RunServerEditorEndpoint : Endpoint<RunServerEditorRequest, RunServerEditorResponse>
	{
		public override string Name => EndpointNames.RunServerEditor;

		protected override RunServerEditorRequest CreateRequest(IDataPipeReader reader) => new RunServerEditorRequest(reader);

		protected override RunServerEditorResponse CreateResponse(IDataPipeReader reader) => new RunServerEditorResponse(reader);
	}

	public class RunServerEditorRequest : Request
	{
		public SessionId SessionId { get; private set; }

		public object? Instance { get; private set; }

		public string PropertyName { get; private set; } = string.Empty;

		public RunServerEditorRequest(SessionId sessionId, object? instance, string propertyName)
		{
			SessionId = sessionId.IsNull ? throw new ArgumentNullException(nameof(sessionId)) : sessionId;
			Instance = instance;
			PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		}

		public RunServerEditorRequest(IDataPipeReader reader)
			: base(reader)
		{
		}

		protected override void ReadProperties(IDataPipeReader reader)
		{
			SessionId = reader.ReadSessionId(nameof(SessionId));
			Instance = reader.ReadObject(nameof(Instance));
			PropertyName = reader.ReadString(nameof(PropertyName)) ?? string.Empty;
		}

		protected override void WriteProperties(IDataPipeWriter writer)
		{
			writer.Write(nameof(SessionId), SessionId);
			writer.WriteObject(nameof(Instance), Instance);
			writer.Write(nameof(PropertyName), PropertyName);
		}
	}

	public class RunServerEditorResponse : Response.Empty
	{
		public static new RunServerEditorResponse Empty { get; } = new RunServerEditorResponse();

		private RunServerEditorResponse()
		{
		}

		public RunServerEditorResponse(IDataPipeReader reader)
			: base(reader)
		{
		}
	}
}
