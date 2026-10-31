using System;
using System.Xml;

namespace Mordex.Windows.Common.Layouts
{
	public class LayoutMerger
	{
		private const string UseDefaultAttribute = "UseDefault";

		private const string IdAttribute = "ID";

		private const string CollectionAttribute = "Collection";

		private const string NonInheritedAttribute = "NonInherited";

		private const string IncrementalAttribute = "Incremental";

		private const string HandlerVersionAttribute = "IncrementalHandlerVersion";

		private const string CurrentHandlerVersion = "2.0";

		private const string TrueValue = "true";

		public void MergeLayouts(IMordexLayout mergedLayout, IMordexLayout parentLayout, IMordexLayout childLayout)
		{
			mergedLayout.LayoutString = MergeXml(parentLayout.LayoutString, childLayout.LayoutString);
			LayoutReferenceCollection references = CopyReferences(childLayout.LayoutReferences);
			foreach (LayoutReference parentReference in parentLayout.LayoutReferences)
			{
				if (!references.Contains(parentReference.ReferenceId))
				{
					references.Add(new LayoutReference(parentReference.ReferenceId, parentReference.Instance));
				}
			}
			ReplaceReferences(mergedLayout, references);
		}

		private static LayoutReferenceCollection CopyReferences(LayoutReferenceCollection source)
		{
			LayoutReferenceCollection copy = new LayoutReferenceCollection();
			foreach (LayoutReference reference in source)
			{
				copy.Add(new LayoutReference(reference.ReferenceId, reference.Instance));
			}
			return copy;
		}

		private static void ReplaceReferences(IMordexLayout layout, LayoutReferenceCollection references)
		{
			layout.LayoutReferences.Clear();
			foreach (LayoutReference reference in references)
			{
				layout.LayoutReferences.Add(reference);
			}
		}

		private static string MergeXml(string parentXml, string childXml)
		{
			if (string.IsNullOrEmpty(parentXml))
			{
				return childXml;
			}
			if (string.IsNullOrEmpty(childXml))
			{
				return parentXml;
			}
			XmlDocument parent = LayoutXml.LoadString(parentXml);
			XmlDocument child = LayoutXml.LoadString(childXml);
			return MergeDocuments(parent, child).OuterXml;
		}

		internal static XmlNode MergeDocuments(XmlDocument parent, XmlDocument child)
		{
			if (parent.DocumentElement.Name != child.DocumentElement.Name)
			{
				throw new InvalidOperationException("Merge can not be done with different types of layouts");
			}
			if (GetAttributeValue(child.DocumentElement, HandlerVersionAttribute) == CurrentHandlerVersion)
			{
				return MergeIncremental(parent.DocumentElement, child.DocumentElement, null, isCollection: false, useOverrideShell: false);
			}
			return MergeBasic(parent.DocumentElement, child.DocumentElement, null, useOverrideShell: false);
		}

		// Merge used by layouts written before incremental handler 2.0.
		private static XmlNode MergeBasic(XmlNode baseNode, XmlNode overrideNode, XmlDocument targetDocument, bool useOverrideShell)
		{
			XmlNode result = CloneShell(baseNode, overrideNode, targetDocument, useOverrideShell);
			foreach (XmlNode baseChild in baseNode.ChildNodes)
			{
				if (FindChild(overrideNode, baseChild.Name, GetAttributeValue(baseChild, IdAttribute)) == null)
				{
					result.AppendChild(result.OwnerDocument.ImportNode(baseChild, deep: true));
				}
			}
			foreach (XmlNode overrideChild in overrideNode.ChildNodes)
			{
				XmlNode baseChild = FindChild(baseNode, overrideChild.Name, GetAttributeValue(overrideChild, IdAttribute));
				if (baseChild == null)
				{
					result.AppendChild(result.OwnerDocument.ImportNode(overrideChild, deep: true));
				}
				else if (GetAttributeValue(overrideChild, UseDefaultAttribute) != TrueValue)
				{
					result.AppendChild(MergeBasic(baseChild, overrideChild, result.OwnerDocument, useOverrideShell: true));
				}
			}
			return result;
		}

		private static XmlNode CloneShell(XmlNode baseNode, XmlNode overrideNode, XmlDocument targetDocument, bool useOverrideShell)
		{
			XmlNode source = useOverrideShell ? overrideNode : baseNode;
			return targetDocument != null ? targetDocument.ImportNode(source, deep: false) : source.CloneNode(deep: false);
		}

		public static bool IsIncrementalLayout(string layoutString)
		{
			if (layoutString.Length == 0)
			{
				return false;
			}
			XmlDocument document = LayoutXml.LoadString(layoutString);
			return GetAttributeValue(document.DocumentElement, IncrementalAttribute) == TrueValue;
		}

		public void CreateIncrementalLayout(IMordexLayout newLayout, IMordexLayout parentLayout, IMordexLayout childLayout)
		{
			newLayout.LayoutString = CreateIncrementalXml(parentLayout.LayoutString, childLayout.LayoutString);
			LayoutReferenceCollection references = CopyReferences(childLayout.LayoutReferences);
			foreach (LayoutReference parentReference in parentLayout.LayoutReferences)
			{
				if (references.Contains(parentReference.ReferenceId))
				{
					LayoutWriter parentWriter = new LayoutWriter();
					parentWriter.AddValue("LayoutReference", parentReference);
					LayoutWriter childWriter = new LayoutWriter();
					childWriter.AddValue("LayoutReference", references[parentReference.ReferenceId]);
					if (parentWriter.ToString() == childWriter.ToString())
					{
						references.Remove(parentReference.ReferenceId);
					}
				}
			}
			ReplaceReferences(newLayout, references);
		}

		private static string CreateIncrementalXml(string parentXml, string childXml)
		{
			if (parentXml.Length == 0)
			{
				return childXml;
			}
			XmlDocument parent = LayoutXml.LoadString(parentXml);
			XmlNode result;
			if (childXml.Length == 0)
			{
				result = parent.DocumentElement.CloneNode(deep: false);
			}
			else
			{
				result = CreateIncrementalDocument(parent, LayoutXml.LoadString(childXml));
			}
			return result.OuterXml;
		}

		private static XmlNode CreateIncrementalDocument(XmlDocument parent, XmlDocument child)
		{
			if (parent.DocumentElement.Name != child.DocumentElement.Name)
			{
				throw new InvalidOperationException("Incremental layouts can not be done in two different types of layouts");
			}
			XmlNode result = AreSameXml(parent.DocumentElement, child.DocumentElement)
				? parent.DocumentElement.CloneNode(deep: false)
				: CreateIncrementalNode(parent.DocumentElement, child.DocumentElement, null);
			AppendAttribute(result, IncrementalAttribute, TrueValue);
			AppendAttribute(result, HandlerVersionAttribute, CurrentHandlerVersion);
			return result;
		}

		private static void AppendAttribute(XmlNode node, string name, string value)
		{
			XmlAttribute attribute = node.OwnerDocument.CreateAttribute(name);
			attribute.AppendChild(node.OwnerDocument.CreateTextNode(value));
			node.Attributes.Append(attribute);
		}

		private static XmlNode CreateUseDefaultMarker(XmlDocument document, string name)
		{
			XmlNode marker = document.CreateNode(XmlNodeType.Element, name, null);
			AppendAttribute(marker, UseDefaultAttribute, TrueValue);
			return marker;
		}

		// Builds the difference between a parent node and the child node derived from it.
		private static XmlNode CreateIncrementalNode(XmlNode parentNode, XmlNode childNode, XmlDocument targetDocument)
		{
			if (AreSameXml(parentNode, childNode))
			{
				return null;
			}
			XmlNode result = targetDocument != null ? targetDocument.ImportNode(childNode, deep: false) : childNode.CloneNode(deep: false);
			XmlDocument document = result.OwnerDocument;
			if (GetAttributeValue(parentNode, CollectionAttribute) == TrueValue)
			{
				foreach (XmlNode childItem in childNode.ChildNodes)
				{
					string id = GetAttributeValue(childItem, IdAttribute);
					XmlNode parentItem = FindChild(parentNode, childItem.Name, id);
					if (parentItem == null)
					{
						XmlNode added = document.ImportNode(childItem, deep: true);
						added.Attributes.Append(document.CreateAttribute(NonInheritedAttribute));
						result.AppendChild(added);
					}
					else if (parentItem.InnerXml != childItem.InnerXml)
					{
						if (id != null)
						{
							XmlNode changed = CreateIncrementalNode(parentItem, childItem, document);
							if (changed != null)
							{
								result.AppendChild(changed);
							}
						}
						else
						{
							result.AppendChild(document.ImportNode(childItem, deep: true));
						}
					}
					else if (id != null)
					{
						result.AppendChild(document.ImportNode(childItem, deep: true));
					}
				}
				int deletedCount = 0;
				foreach (XmlNode parentItem in parentNode.ChildNodes)
				{
					string id = GetAttributeValue(parentItem, IdAttribute);
					if (id != null && FindChild(childNode, parentItem.Name, id) == null)
					{
						XmlNode deleted = CreateUseDefaultMarker(document, "Deleted" + deletedCount);
						AppendAttribute(deleted, IdAttribute, id);
						result.AppendChild(deleted);
						deletedCount++;
					}
				}
			}
			else
			{
				foreach (XmlNode parentItem in parentNode.ChildNodes)
				{
					string id = GetAttributeValue(parentItem, IdAttribute);
					XmlNode childItem = FindChild(childNode, parentItem.Name, id);
					if (childItem != null)
					{
						XmlNode changed = CreateIncrementalNode(parentItem, childItem, document);
						if (changed != null)
						{
							result.AppendChild(changed);
						}
					}
					else if (parentItem.NodeType != XmlNodeType.Text && id == null)
					{
						result.AppendChild(CreateUseDefaultMarker(document, parentItem.Name));
					}
				}
				foreach (XmlNode childItem in childNode.ChildNodes)
				{
					if (FindChild(parentNode, childItem.Name, GetAttributeValue(childItem, IdAttribute)) == null)
					{
						result.AppendChild(document.ImportNode(childItem, deep: true));
					}
				}
			}
			return result;
		}

		private static string GetAttributeValue(XmlNode node, string name)
		{
			return node?.Attributes?.GetNamedItem(name)?.Value;
		}

		internal static XmlNode FindChild(XmlNode parent, string name, string id)
		{
			if (parent == null)
			{
				return null;
			}
			foreach (XmlNode child in parent.ChildNodes)
			{
				if (id == null)
				{
					if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
					{
						return child;
					}
				}
				else
				{
					string childId = GetAttributeValue(child, IdAttribute);
					if (childId != null && string.Equals(childId, id, StringComparison.OrdinalIgnoreCase))
					{
						return child;
					}
				}
			}
			return null;
		}

		private static bool AreSameXml(XmlNode first, XmlNode second)
		{
			return string.Equals(first.OuterXml, second.OuterXml, StringComparison.OrdinalIgnoreCase);
		}

		// Merges a child layout written with incremental handler 2.0 over its parent.
		private static XmlNode MergeIncremental(XmlNode baseNode, XmlNode overrideNode, XmlDocument targetDocument, bool isCollection, bool useOverrideShell)
		{
			XmlNode result = CloneShell(baseNode, overrideNode, targetDocument, useOverrideShell);
			XmlDocument document = result.OwnerDocument;
			if (!isCollection)
			{
				MergeIncrementalChildren(baseNode, overrideNode, result, document);
			}
			else
			{
				MergeIncrementalCollection(baseNode, overrideNode, result, document);
			}
			return result;
		}

		private static void MergeIncrementalChildren(XmlNode baseNode, XmlNode overrideNode, XmlNode result, XmlDocument document)
		{
			foreach (XmlNode baseChild in baseNode.ChildNodes)
			{
				XmlNode overrideChild = FindChild(overrideNode, baseChild.Name, GetAttributeValue(baseChild, IdAttribute));
				if (overrideChild == null)
				{
					result.AppendChild(document.ImportNode(baseChild, deep: true));
				}
				else if (GetAttributeValue(overrideChild, UseDefaultAttribute) != TrueValue)
				{
					XmlNode merged;
					if (GetAttributeValue(baseChild, CollectionAttribute) == TrueValue)
					{
						merged = MergeIncremental(baseChild, overrideChild, document, isCollection: true, useOverrideShell: true);
					}
					else if (baseChild.NodeType == XmlNodeType.Text)
					{
						merged = document.ImportNode(overrideChild, deep: true);
					}
					else
					{
						merged = MergeIncremental(baseChild, overrideChild, document, isCollection: false, useOverrideShell: true);
					}
					result.AppendChild(merged);
				}
			}
			foreach (XmlNode overrideChild in overrideNode.ChildNodes)
			{
				if (overrideChild.NodeType != XmlNodeType.Text
					&& FindChild(baseNode, overrideChild.Name, GetAttributeValue(overrideChild, IdAttribute)) == null)
				{
					result.AppendChild(document.ImportNode(overrideChild, deep: true));
				}
			}
		}

		private static void MergeIncrementalCollection(XmlNode baseNode, XmlNode overrideNode, XmlNode result, XmlDocument document)
		{
			string elementName = GetCollectionElementName(overrideNode);
			int itemNumber = 0;
			foreach (XmlNode overrideChild in overrideNode.ChildNodes)
			{
				if (overrideChild.NodeType == XmlNodeType.Text)
				{
					continue;
				}
				XmlNode baseChild = FindChild(baseNode, overrideChild.Name, GetAttributeValue(overrideChild, IdAttribute));
				if (baseChild == null)
				{
					if (GetAttributeValue(overrideChild, NonInheritedAttribute) == null)
					{
						continue;
					}
					if (StartsWithElementName(overrideChild, elementName))
					{
						result.AppendChild(CreateRenamedCopy(document, overrideChild, elementName + itemNumber));
						itemNumber++;
					}
					else
					{
						result.AppendChild(document.ImportNode(overrideChild, deep: true));
					}
				}
				else if (GetAttributeValue(overrideChild, UseDefaultAttribute) != TrueValue)
				{
					XmlNode merged = MergeIncremental(baseChild, overrideChild, document, isCollection: false, useOverrideShell: true);
					if (elementName != null)
					{
						result.AppendChild(CreateRenamedCopy(document, merged, elementName + itemNumber));
						itemNumber++;
					}
					else
					{
						result.AppendChild(merged);
					}
				}
			}
			foreach (XmlNode baseChild in baseNode.ChildNodes)
			{
				if (baseChild.NodeType != XmlNodeType.Element
					|| FindChild(overrideNode, baseChild.Name, GetAttributeValue(baseChild, IdAttribute)) != null)
				{
					continue;
				}
				if (StartsWithElementName(baseChild, elementName))
				{
					result.AppendChild(CreateRenamedCopy(document, baseChild, elementName + itemNumber));
					itemNumber++;
				}
				else
				{
					result.AppendChild(document.ImportNode(baseChild, deep: true));
				}
			}
		}

		private static bool StartsWithElementName(XmlNode node, string elementName)
		{
			return elementName != null && node.Name.StartsWith(elementName, StringComparison.OrdinalIgnoreCase);
		}

		private static XmlNode CreateRenamedCopy(XmlDocument document, XmlNode source, string name)
		{
			XmlNode copy = document.CreateElement(name);
			if (source.Attributes != null)
			{
				foreach (XmlAttribute attribute in source.Attributes)
				{
					copy.Attributes.Append((XmlAttribute)document.ImportNode(attribute, deep: true));
				}
			}
			copy.InnerXml = source.InnerXml;
			return copy;
		}

		private static string GetCollectionElementName(XmlNode node)
		{
			if (!string.Equals(GetAttributeValue(node, CollectionAttribute), TrueValue, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			string elementName = GetAttributeValue(node, "ElementName");
			return string.IsNullOrEmpty(elementName) ? null : elementName;
		}
	}
}
