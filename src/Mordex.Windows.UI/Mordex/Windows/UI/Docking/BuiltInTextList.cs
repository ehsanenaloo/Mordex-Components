using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Docking
{
	[Localizable(true)]
	public class BuiltInTextList : IMordexXmlLayoutsSupport
	{
		private string[] _defaultTexts;

		private MxDockManager _manager;

		private string[] _customTexts;

		public string this[PanelsBuiltInText textID]
		{
			get
			{
				if (!Enum.IsDefined(typeof(PanelsBuiltInText), textID))
				{
					throw new InvalidEnumArgumentException();
				}
				string text = _customTexts[(int)textID];
				if (text == null)
				{
					return _defaultTexts[(int)textID];
				}
				return text;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelsBuiltInText), textID))
				{
					throw new InvalidEnumArgumentException();
				}
				_customTexts[(int)textID] = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "LocalizableStrings";

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal BuiltInTextList(MxDockManager manager)
		{
			_manager = manager;
			InitializeDefaultTexts();
			Reset();
		}

		internal string GetDefaultText(PanelsBuiltInText panelsBuiltInText)
		{
			return _defaultTexts[(int)panelsBuiltInText];
		}

		internal string GetCustomText(PanelsBuiltInText panelsBuiltInText)
		{
			return _customTexts[(int)panelsBuiltInText];
		}

		public void Reset(PanelsBuiltInText textID)
		{
			if (!Enum.IsDefined(typeof(PanelsBuiltInText), textID))
			{
				throw new InvalidEnumArgumentException();
			}
			_customTexts[(int)textID] = null;
		}

		private void InitializeDefaultTexts()
		{
			_defaultTexts = new string[10];
			_defaultTexts[8] = "Add or Remove Buttons";
			_defaultTexts[1] = "Auto Hide";
			_defaultTexts[3] = "Close";
			_defaultTexts[0] = "Close";
			_defaultTexts[4] = "Scroll Left";
			_defaultTexts[5] = "Scroll Right";
			_defaultTexts[6] = "Show More Buttons";
			_defaultTexts[7] = "Show Fewer Buttons";
			_defaultTexts[2] = "Other Task Panes";
			_defaultTexts[9] = "Active Files";
		}

		public void Reset()
		{
			_customTexts = new string[10];
		}

		internal bool HasCustomTexts()
		{
			for (int i = 0; i < _customTexts.Length; i++)
			{
				if (_customTexts[i] != null)
				{
					return true;
				}
			}
			return false;
		}

		private string SerializeToXml()
		{
			PropertyValue root = new PropertyValue("LocalizableData", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		internal string GetLayoutXml()
		{
			if (!HasCustomTexts())
			{
				return "";
			}
			return SerializeToXml();
		}

		internal void SetLayoutXml(string xmlString)
		{
			if (xmlString == null || xmlString.Length == 0)
			{
				Reset();
				return;
			}
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			mordexLayoutReader["LocalizableData"].DeserializeObject(this);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			for (int i = 0; i < 10; i++)
			{
				PanelsBuiltInText panelsBuiltInText = (PanelsBuiltInText)i;
				_customTexts[i] = (string)LayoutReader.GetPropertyValue(reader, panelsBuiltInText.ToString(), typeof(string), null);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			for (int i = 0; i < 10; i++)
			{
				if (_customTexts[i] != null)
				{
					PanelsBuiltInText panelsBuiltInText = (PanelsBuiltInText)i;
					info.AddValue(panelsBuiltInText.ToString(), _customTexts[i]);
				}
			}
		}
	}
}
