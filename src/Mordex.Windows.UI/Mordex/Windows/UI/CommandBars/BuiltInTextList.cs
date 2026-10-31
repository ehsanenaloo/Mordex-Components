using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.CommandBars
{
	[Localizable(true)]
	public class BuiltInTextList : IMordexXmlLayoutsSupport
	{
		private static readonly int TextCount = Enum.GetNames(typeof(CommandsBuiltInText)).Length;

		private string[] _defaultTexts;

		internal MxCommandManager _commandManager;

		private string[] _customTexts;

		public string this[CommandsBuiltInText textID]
		{
			get
			{
				if (!Enum.IsDefined(typeof(CommandsBuiltInText), textID))
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
				if (!Enum.IsDefined(typeof(CommandsBuiltInText), textID))
				{
					throw new InvalidEnumArgumentException();
				}
				_customTexts[(int)textID] = value;
				_commandManager.RefreshRebars();
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => "LocalizableStrings";

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal BuiltInTextList(MxCommandManager manager)
		{
			_commandManager = manager;
			FillDefaultTexts();
			Reset();
		}

		internal string GetDefaultText(CommandsBuiltInText commandsBuiltInText)
		{
			return _defaultTexts[(int)commandsBuiltInText];
		}

		internal string GetCustomText(CommandsBuiltInText commandsBuiltInText)
		{
			return _customTexts[(int)commandsBuiltInText];
		}

		public void Reset(CommandsBuiltInText textID)
		{
			if (!Enum.IsDefined(typeof(CommandsBuiltInText), textID))
			{
				throw new InvalidEnumArgumentException();
			}
			_customTexts[(int)textID] = null;
		}

		private void FillDefaultTexts()
		{
			_defaultTexts = new string[TextCount];
			_defaultTexts[(int)CommandsBuiltInText.Minimize] = "Minimize";
			_defaultTexts[(int)CommandsBuiltInText.Maximize] = "Maximize";
			_defaultTexts[(int)CommandsBuiltInText.Close] = "Close";
			_defaultTexts[(int)CommandsBuiltInText.Restore] = "Restore";
			_defaultTexts[(int)CommandsBuiltInText.Size] = "Size";
			_defaultTexts[(int)CommandsBuiltInText.Move] = "Move";
			_defaultTexts[(int)CommandsBuiltInText.Next] = "Next";
			_defaultTexts[(int)CommandsBuiltInText.CustomizeButtonToolTip] = "Toolbar Options";
			_defaultTexts[(int)CommandsBuiltInText.Reset] = "&Reset";
			_defaultTexts[(int)CommandsBuiltInText.Delete] = "&Delete";
			_defaultTexts[(int)CommandsBuiltInText.Name] = "&Name";
			_defaultTexts[(int)CommandsBuiltInText.DefaultStyle] = "Defa&ult Style";
			_defaultTexts[(int)CommandsBuiltInText.TextAlways] = "&Text Only";
			_defaultTexts[(int)CommandsBuiltInText.ImageAndText] = "Image &and Text";
			_defaultTexts[(int)CommandsBuiltInText.BeginGroup] = "Begin a &Group";
			_defaultTexts[(int)CommandsBuiltInText.Customize] = "&Customize...";
			_defaultTexts[(int)CommandsBuiltInText.NoDescriptionAvailable] = "No Help topic is associated with this item.";
			_defaultTexts[(int)CommandsBuiltInText.ResetToolBarWarning] = "Are you sure you want to reset the changes made to the '{0}' toolbar?";
			_defaultTexts[(int)CommandsBuiltInText.DuplicatedNameErrorMessage] = "A toolbar named '{0}' already exist. Type another name.";
			_defaultTexts[(int)CommandsBuiltInText.EmptyNameErrorMessage] = "The toolbar name cannot be blank. Type a name";
			_defaultTexts[(int)CommandsBuiltInText.ResetUsageDataMessage] = "This will delete the record of commands you've used in this application and restore the default set of visible commands to the menus and toolbars.\nIt will not undo any explicit customizations. Are you sure you want to do this?";
			_defaultTexts[(int)CommandsBuiltInText.ExpandToolTipText] = "Expand";
			_defaultTexts[(int)CommandsBuiltInText.AddRemoveButtons] = "&Add or Remove Buttons";
			_defaultTexts[(int)CommandsBuiltInText.CutText] = "Cu&t";
			_defaultTexts[(int)CommandsBuiltInText.CopyText] = "&Copy";
			_defaultTexts[(int)CommandsBuiltInText.PasteText] = "&Paste";
			_defaultTexts[(int)CommandsBuiltInText.UndoText] = "&Undo";
			_defaultTexts[(int)CommandsBuiltInText.SelectAllText] = "Select A&ll";
		}

		public void Reset()
		{
			_customTexts = new string[TextCount];
		}

		internal bool HasCustomText()
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

		private string SerializeLayout()
		{
			PropertyValue root = new PropertyValue("LocalizableData", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: false);
			return mordexLayoutWriter.ToString();
		}

		internal string GetLayoutXml()
		{
			if (!HasCustomText())
			{
				return "";
			}
			return SerializeLayout();
		}

		internal void LoadLayoutXml(string xmlString)
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
			for (int i = 0; i < TextCount; i++)
			{
				CommandsBuiltInText commandsBuiltInText = (CommandsBuiltInText)i;
				_customTexts[i] = (string)LayoutReader.GetPropertyValue(reader, commandsBuiltInText.ToString(), typeof(string), null);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			for (int i = 0; i < TextCount; i++)
			{
				if (_customTexts[i] != null)
				{
					CommandsBuiltInText commandsBuiltInText = (CommandsBuiltInText)i;
					info.AddValue(commandsBuiltInText.ToString(), _customTexts[i]);
				}
			}
		}
	}
}
