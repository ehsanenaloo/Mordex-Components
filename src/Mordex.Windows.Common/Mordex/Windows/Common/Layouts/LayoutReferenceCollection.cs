using System;
using System.Collections;

namespace Mordex.Windows.Common.Layouts
{
	public class LayoutReferenceCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Reference";

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		public LayoutReference this[int index] => (LayoutReference)base.InnerList[index];

		public LayoutReference this[string referenceId]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						LayoutReference mordexLayoutReference = (LayoutReference)enumerator.Current;
						if (string.Equals(mordexLayoutReference.ReferenceId, referenceId, StringComparison.OrdinalIgnoreCase))
						{
							return mordexLayoutReference;
						}
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			for (int index = 0; index < info.Count; index++)
			{
				PropertyValue propertyValue = info["Reference" + index];
				if (propertyValue == null)
				{
					break;
				}
				LayoutReference reference = new LayoutReference();
				propertyValue.DeserializeObject(reference);
				base.InnerList.Add(reference);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int referenceIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					LayoutReference value = (LayoutReference)enumerator.Current;
					info.AddValue("Reference" + referenceIndex, value);
					referenceIndex++;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public LayoutReference Add(object instance)
		{
			return Add(CreateUniqueReferenceId(), instance);
		}

		public LayoutReference Add(string referenceId, object instance)
		{
			LayoutReference mordexLayoutReference = new LayoutReference(referenceId, instance);
			Add(mordexLayoutReference);
			return mordexLayoutReference;
		}

		public void Add(LayoutReference reference)
		{
			base.InnerList.Add(reference);
		}

		public void AddRange(LayoutReference[] references)
		{
			foreach (LayoutReference value in references)
			{
				base.InnerList.Add(value);
			}
		}

		public bool Contains(string referenceId)
		{
			return this[referenceId] != null;
		}

		public bool Contains(LayoutReference reference)
		{
			return base.List.Contains(reference);
		}

		public int IndexOf(LayoutReference reference)
		{
			return base.List.IndexOf(reference);
		}

		private string CreateUniqueReferenceId()
		{
			int referenceNumber = 1;
			string text = "#reference" + referenceNumber + "#";
			while (Contains(text))
			{
				referenceNumber++;
				text = "#reference" + referenceNumber + "#";
			}
			return text;
		}

		public void Remove(string referenceId)
		{
			LayoutReference mordexLayoutReference = this[referenceId];
			if (mordexLayoutReference != null)
			{
				base.List.Remove(mordexLayoutReference);
			}
		}

		public void CopyTo(LayoutReference[] destination, int index)
		{
			base.List.CopyTo(destination, index);
		}

		public void Remove(LayoutReference reference)
		{
			if (base.List.Contains(reference))
			{
				base.List.Remove(reference);
			}
		}
	}
}
