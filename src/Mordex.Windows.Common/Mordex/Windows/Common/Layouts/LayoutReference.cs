using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Mordex.Windows.Common.Layouts
{
	[ComVisible(false)]
	[TypeConverter(typeof(LayoutReferenceConverter))]
	public class LayoutReference : IMordexXmlLayoutsSupport
	{
		private string _referenceId;

		private object _instance;

		string IMordexXmlLayoutsSupport.Identifier => _referenceId;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string ReferenceId
		{
			get
			{
				return _referenceId;
			}
			set
			{
				_referenceId = value;
			}
		}

		public object Instance
		{
			get
			{
				return _instance;
			}
			set
			{
				_instance = value;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_referenceId = (string)LayoutReader.GetPropertyValue(info, "ReferenceId", typeof(string), "");
			PropertyValue propertyValue = info["Instance"];
			if (propertyValue != null)
			{
				_instance = propertyValue.ReadEmbeddedValue();
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_referenceId.Length > 0)
			{
				info.AddValue("ReferenceId", _referenceId);
			}
			if (_instance != null)
			{
				PropertyValue propertyValue = new PropertyValue("Instance", _instance);
				propertyValue._embedStreams = true;
				info.AddValue(propertyValue);
			}
		}

		internal LayoutReference()
		{
			_referenceId = "";
		}

		public LayoutReference(string referenceId)
		{
			_referenceId = referenceId;
		}

		public LayoutReference(string referenceId, object instance)
		{
			_referenceId = referenceId;
			_instance = instance;
		}
	}
}
