using System.Collections.Generic;
using Microsoft.DotNet.DesignTools.Client.TypeRouting;

namespace Mordex.Designer.Client
{
	[ExportTypeRoutingDefinitionProvider]
	internal sealed class TypeRoutingProvider : TypeRoutingDefinitionProvider
	{
		public override IEnumerable<TypeRoutingDefinition> GetDefinitions()
		{
			foreach (string name in EditorRoutes.Names)
			{
				yield return new TypeRoutingDefinition(TypeRoutingKinds.Editor, name, typeof(ServerEditorProxy));
			}
		}
	}
}
