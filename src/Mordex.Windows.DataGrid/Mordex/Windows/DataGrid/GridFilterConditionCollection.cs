using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridFilterConditionCollection : MordexConditionCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "FilterCondition";

		public GridFilterCondition this[int index] => (GridFilterCondition)BaseGet(index);

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int value = 0;
			for (PropertyValue propertyValue = info["FilterCondition" + value]; propertyValue != null; propertyValue = info["FilterCondition" + value])
			{
				GridFilterCondition gridFilterCondition = new GridFilterCondition();
				gridFilterCondition._owner = GetOwnerCondition()._owner;
				propertyValue.DeserializeObject(gridFilterCondition);
				BaseAdd(gridFilterCondition);
				value++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int otherValue = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridFilterCondition value = (GridFilterCondition)enumerator.Current;
					info.AddValue("FilterCondition" + otherValue, value);
					otherValue++;
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

		internal GridFilterConditionCollection(GridFilterCondition owner)
			: base(owner)
		{
		}

		internal GridFilterCondition GetOwner()
		{
			return (GridFilterCondition)owner;
		}

		public int IndexOf(GridFilterCondition condition)
		{
			return ((IList)this).IndexOf((object)condition);
		}

		internal GridFilterCondition GetOwnerCondition()
		{
			return GetOwner();
		}
	}
}
