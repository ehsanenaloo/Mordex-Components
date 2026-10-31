using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	[Localizable(true)]
	public class GridBuiltInTextList : IMordexXmlLayoutsSupport
	{
		private string[] _defaultTexts;

		private MxDataGrid _gridOwner;

		private string[] _customTexts;

		public string this[GridBuiltInText textID]
		{
			get
			{
				if (!Enum.IsDefined(typeof(GridBuiltInText), textID))
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
				if (!Enum.IsDefined(typeof(GridBuiltInText), textID))
				{
					throw new InvalidEnumArgumentException();
				}
				_customTexts[(int)textID] = value;
				_gridOwner.ApplyRecordNavigatorBuiltInText();
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "LocalizableStrings";

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal GridBuiltInTextList(MxDataGrid gridOwner)
		{
			_gridOwner = gridOwner;
			UpdateTextArray();
			Reset();
		}

		internal string GetDefaultText(GridBuiltInText gridBuiltInText)
		{
			return _defaultTexts[(int)gridBuiltInText];
		}

		internal string GetCustomText(GridBuiltInText gridBuiltInText)
		{
			return _customTexts[(int)gridBuiltInText];
		}

		public void Reset(GridBuiltInText textID)
		{
			if (!Enum.IsDefined(typeof(GridBuiltInText), textID))
			{
				throw new InvalidEnumArgumentException();
			}
			_customTexts[(int)textID] = null;
			_gridOwner.ApplyRecordNavigatorBuiltInText();
		}

		private void UpdateTextArray()
		{
			_defaultTexts = new string[38];
			_defaultTexts[0] = "Drag a column header here to group by that column.";
			_defaultTexts[1] = "None";
			_defaultTexts[2] = "Today";
			_defaultTexts[3] = "Record:|Of";
			_defaultTexts[4] = "You can't go to the specified record";
			_defaultTexts[5] = string.Empty;
			_defaultTexts[21] = "Beyond Next Month";
			_defaultTexts[9] = "Earlier This Month";
			_defaultTexts[6] = "None";
			_defaultTexts[8] = "Last Month";
			_defaultTexts[12] = "Last Week";
			_defaultTexts[19] = "Later This Month";
			_defaultTexts[20] = "Next Month";
			_defaultTexts[16] = "Next Week";
			_defaultTexts[7] = "Older";
			_defaultTexts[10] = "Three Weeks Ago";
			_defaultTexts[18] = "Three Weeks Away";
			_defaultTexts[14] = "Today";
			_defaultTexts[15] = "Tomorrow";
			_defaultTexts[11] = "Two Weeks Ago";
			_defaultTexts[17] = "Two Weeks Away";
			_defaultTexts[13] = "Yesterday";
			_defaultTexts[22] = "OK";
			_defaultTexts[23] = "Cancel";
			_defaultTexts[24] = string.Empty;
			_defaultTexts[34] = "Begins With";
			_defaultTexts[25] = "Equal";
			_defaultTexts[27] = "Greater Than";
			_defaultTexts[28] = "Greater Than Or Equal";
			_defaultTexts[29] = "Less Than";
			_defaultTexts[26] = "Not Equal";
			_defaultTexts[31] = "Contains";
			_defaultTexts[33] = "Ends With";
			_defaultTexts[35] = "{Empty}";
			_defaultTexts[30] = "Less Than Or Equal";
			_defaultTexts[29] = "Less Than";
			_defaultTexts[32] = "Does Not Contain";
			_defaultTexts[36] = "{Not Empty}";
			_defaultTexts[37] = "Clear";
		}

		public void Reset()
		{
			_customTexts = new string[38];
			_gridOwner.ApplyRecordNavigatorBuiltInText();
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

		internal string GetCustomTextsXml()
		{
			if (!HasCustomTexts())
			{
				return "";
			}
			return SerializeToXml();
		}

		internal void SetCustomTextsXml(string xmlString)
		{
			if (xmlString == null || xmlString.Length == 0)
			{
				Reset();
				return;
			}
			LayoutReader mordexLayoutReader = LayoutReader.FromString(xmlString);
			mordexLayoutReader["LocalizableData"].DeserializeObject(this);
			_gridOwner.ApplyRecordNavigatorBuiltInText();
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			for (int i = 0; i < 38; i++)
			{
				GridBuiltInText gridBuiltInText = (GridBuiltInText)i;
				_customTexts[i] = (string)LayoutReader.GetPropertyValue(reader, gridBuiltInText.ToString(), typeof(string), null);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			for (int i = 0; i < 38; i++)
			{
				if (_customTexts[i] != null)
				{
					GridBuiltInText gridBuiltInText = (GridBuiltInText)i;
					info.AddValue(gridBuiltInText.ToString(), _customTexts[i]);
				}
			}
		}
	}
}
