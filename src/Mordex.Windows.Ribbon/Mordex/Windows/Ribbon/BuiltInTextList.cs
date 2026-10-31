using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[Localizable(true)]
	public class BuiltInTextList : IMordexXmlLayoutsSupport
	{
		private static readonly int s_textCount = Enum.GetNames(typeof(RibbonBuiltInText)).Length;

		private string[] _defaultTexts;

		internal MxRibbon _ribbon;

		private string[] _customTexts;

		public string this[RibbonBuiltInText textID]
		{
			get
			{
				if (!Enum.IsDefined(typeof(RibbonBuiltInText), textID))
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
				if (!Enum.IsDefined(typeof(RibbonBuiltInText), textID))
				{
					throw new InvalidEnumArgumentException();
				}
				_customTexts[(int)textID] = value;
				_ribbon.InvalidateFullLayout();
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "LocalizableStrings";

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal BuiltInTextList(MxRibbon ribbon)
		{
			_ribbon = ribbon;
			InitializeDefaultTexts();
			Reset();
		}

		internal string GetDefaultText(RibbonBuiltInText ribbonBuiltInText)
		{
			return _defaultTexts[(int)ribbonBuiltInText];
		}

		internal string GetCustomText(RibbonBuiltInText ribbonBuiltInText)
		{
			return _customTexts[(int)ribbonBuiltInText];
		}

		public void Reset(RibbonBuiltInText textID)
		{
			if (!Enum.IsDefined(typeof(RibbonBuiltInText), textID))
			{
				throw new InvalidEnumArgumentException();
			}
			_customTexts[(int)textID] = null;
		}

		private void InitializeDefaultTexts()
		{
			int textCount = s_textCount;
			_defaultTexts = new string[textCount];
			_defaultTexts[0] = "Minimize";
			_defaultTexts[1] = "Maximize";
			_defaultTexts[2] = "Close";
			_defaultTexts[3] = "Restore";
			_defaultTexts[4] = "Customize Quick Access Toolbar";
			_defaultTexts[5] = "&Remove from Quick Access Toolbar...";
			_defaultTexts[6] = "&Add to Quick Access Toolbar";
			_defaultTexts[7] = "&Show Below the Ribbon";
			_defaultTexts[8] = "&Show Above the Ribbon";
			_defaultTexts[9] = "Mi&nimize the Ribbon";
			_defaultTexts[10] = "";
		}

		public void Reset()
		{
			int textCount = s_textCount;
			_customTexts = new string[textCount];
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

		private string WriteLayoutXml()
		{
			PropertyValue root = new PropertyValue("LocalizableData", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		internal string GetCustomTextsXml()
		{
			if (!HasCustomTexts())
			{
				return "";
			}
			return WriteLayoutXml();
		}

		internal void LoadCustomTextsXml(string xmlString)
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
			int textCount = s_textCount;
			for (int i = 0; i < textCount; i++)
			{
				RibbonBuiltInText ribbonBuiltInText = (RibbonBuiltInText)i;
				_customTexts[i] = (string)LayoutReader.GetPropertyValue(reader, ribbonBuiltInText.ToString(), typeof(string), null);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int textCount = s_textCount;
			for (int i = 0; i < textCount; i++)
			{
				if (_customTexts[i] != null)
				{
					RibbonBuiltInText ribbonBuiltInText = (RibbonBuiltInText)i;
					info.AddValue(ribbonBuiltInText.ToString(), _customTexts[i]);
				}
			}
		}
	}
}
