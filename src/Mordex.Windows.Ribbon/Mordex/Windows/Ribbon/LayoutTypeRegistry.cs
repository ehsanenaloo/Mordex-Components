using System;
using System.Collections.Generic;
using System.Xml;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	// Layout XML is data from outside the process. Types named in it are looked up here
	// instead of being resolved and constructed by name.
	internal static class LayoutTypeRegistry
	{
		private static readonly Dictionary<string, Type> s_commandTypes = BuildCommandTypes();

		private static Dictionary<string, Type> BuildCommandTypes()
		{
			Dictionary<string, Type> types = new Dictionary<string, Type>(StringComparer.Ordinal);
			foreach (Type type in typeof(CommandBase).Assembly.GetTypes())
			{
				if (type.IsPublic && !type.IsAbstract && typeof(CommandBase).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
				{
					types[type.FullName] = type;
				}
			}
			return types;
		}

		// Returns a new command of the named type, or a plain CommandBase when the name is not a known command type.
		internal static CommandBase CreateCommand(string typeName)
		{
			if (typeName != null && s_commandTypes.TryGetValue(typeName, out Type type))
			{
				return (CommandBase)Activator.CreateInstance(type);
			}
			return new CommandBase();
		}

		// Reads the Tag and TagTypeName values. Anything that cannot be converted safely stays a string.
		internal static object ReadTag(LayoutReader reader)
		{
			string text = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			return LayoutTypePolicy.ConvertOrKeepText(text, (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null));
		}

		// Loads a layout document and rejects anything that carries a DTD.
		internal static XmlDocument LoadDocument(XmlReader xmlReader)
		{
			if (xmlReader == null)
			{
				throw new ArgumentNullException(nameof(xmlReader));
			}
			return LayoutImportLimits.LoadDocument(xmlReader);
		}
	}
}
