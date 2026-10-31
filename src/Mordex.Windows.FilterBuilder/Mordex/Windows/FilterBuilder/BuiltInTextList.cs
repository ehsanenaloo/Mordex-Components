using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.FilterBuilder
{
	[Localizable(true)]
	public class BuiltInTextList : IMordexXmlLayoutsSupport
	{
		private const string LayoutRootName = "LocalizableData";

		// Same order as the BuiltInText enum.
		private static readonly string[] DefaultTexts =
		{
			"Click here to add filter criteria",
			"Begins with",
			"Between",
			"Contains",
			"Ends with",
			"Equal",
			"Greater than",
			"Greater than or equal to",
			"Any of",
			"Is empty",
			"Is null",
			"Less than",
			"Less than or equal to",
			"Not between",
			"Not contains",
			"Not equal",
			"None of",
			"Not is empty",
			"Not is null",
			"And",
			"And not",
			"Or",
			"Or not",
			"Xor",
			"Xor not",
			"Add Criteria",
			"Remove Criteria",
			"Choose Field",
			"Not",
			"True",
			"False",
			"Show Fields to Compare",
			"Negate this Criteria",
			"Assert this Criteria",
			"Add Criteria",
			"Remove Criteria",
			"Today",
			""
		};

		private static readonly int TextCount = Enum.GetValues(typeof(BuiltInText)).Length;

		private readonly MxFilterBuilder _filterBuilder;

		private string[] _customTexts = new string[TextCount];

		public string this[BuiltInText textID]
		{
			get
			{
				EnsureDefined(textID);
				return _customTexts[(int)textID] ?? DefaultTexts[(int)textID];
			}
			set
			{
				EnsureDefined(textID);
				if (_filterBuilder.HasFilterCondition && !_filterBuilder.IsInDesignMode)
				{
					throw new InvalidOperationException("The built-in texts need to be set before a FilterCondition is applied to the FilterBuilder.");
				}
				_customTexts[(int)textID] = value;
				_filterBuilder.OnBuiltInTextChanged(textID);
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "LocalizableStrings";

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal BuiltInTextList(MxFilterBuilder filterBuilder)
		{
			_filterBuilder = filterBuilder;
		}

		internal static string DefaultText(BuiltInText textID)
		{
			return DefaultTexts[(int)textID];
		}

		internal string CustomText(BuiltInText textID)
		{
			return _customTexts[(int)textID];
		}

		public void Reset(BuiltInText textID)
		{
			EnsureDefined(textID);
			_customTexts[(int)textID] = null;
		}

		public void Reset()
		{
			Array.Clear(_customTexts, 0, _customTexts.Length);
		}

		internal bool HasCustomTexts()
		{
			foreach (string text in _customTexts)
			{
				if (text != null)
				{
					return true;
				}
			}
			return false;
		}

		internal string ToXml()
		{
			if (!HasCustomTexts())
			{
				return "";
			}
			PropertyValue root = new PropertyValue(LayoutRootName, this);
			return new LayoutWriter(root, isRoot: false).ToString();
		}

		internal void LoadFromXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				Reset();
				return;
			}
			LayoutReader.FromString(xml)[LayoutRootName].DeserializeObject(this);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			for (int i = 0; i < _customTexts.Length; i++)
			{
				BuiltInText textID = (BuiltInText)i;
				_customTexts[i] = (string)LayoutReader.GetPropertyValue(reader, textID.ToString(), typeof(string), null);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			for (int i = 0; i < _customTexts.Length; i++)
			{
				if (_customTexts[i] != null)
				{
					BuiltInText textID = (BuiltInText)i;
					info.AddValue(textID.ToString(), _customTexts[i]);
				}
			}
		}

		private static void EnsureDefined(BuiltInText textID)
		{
			if (!Enum.IsDefined(typeof(BuiltInText), textID))
			{
				throw new InvalidEnumArgumentException();
			}
		}
	}
}
