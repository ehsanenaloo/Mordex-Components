using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class ControlBoxMenu : IMordexXmlLayoutsSupport, IImageListProvider
	{
		private CommandCollection _leftCommands;

		private CommandCollection _rightCommands;

		private CommandCollection _bottomCommands;

		private MxRibbon _ribbon;

		private SuperTipSettings _superTipSettings;

		private string _keyTip;

		private int _leftPanelWidth;

		private int _rightPanelWidth;

		[Browsable(false)]
		public MxRibbon Ribbon => _ribbon;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandCollection LeftCommands => _leftCommands;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection RightCommands => _rightCommands;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection BottomCommands => _bottomCommands;

		[DefaultValue("")]
		[Description("Represents the letter to press after the Alt key to activate the command.")]
		public string KeyTip
		{
			get
			{
				return _keyTip;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_keyTip != value)
				{
					_keyTip = value;
				}
			}
		}

		[Description("Specifies the width of the left panel.")]
		[DefaultValue(-1)]
		public int LeftPanelWidth
		{
			get
			{
				return _leftPanelWidth;
			}
			set
			{
				if (_leftPanelWidth != value)
				{
					_leftPanelWidth = value;
				}
			}
		}

		[DefaultValue(-1)]
		[Description("Specifies the width of the right panel.")]
		public int RightPanelWidth
		{
			get
			{
				return _rightPanelWidth;
			}
			set
			{
				if (_rightPanelWidth != value)
				{
					_rightPanelWidth = value;
				}
			}
		}

		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Represents the settings used to customize the supertip of the ControlBox's button.")]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
		public SuperTipSettings SuperTipSettings => _superTipSettings;

		[Browsable(false)]
		[Localizable(true)]
		public string SupertipSettingsData
		{
			get
			{
				PropertyValue root = new PropertyValue("SupertipSettings", _superTipSettings);
				LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
				return mordexLayoutWriter.ToString();
			}
			set
			{
				try
				{
					LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
					mordexLayoutReader["SupertipSettings"]?.DeserializeObject(_superTipSettings);
				}
				catch (Exception ex) when (ex is XmlException || ex is FormatException || ex is InvalidCastException || ex is ArgumentException || ex is InvalidOperationException)
				{
					// Invalid saved data keeps the current supertip settings.
				}
			}
		}

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		ImageList IImageListProvider.ImageList
		{
			get
			{
				if (Ribbon != null)
				{
					if (Ribbon.SuperTipComponent != null && Ribbon.SuperTipComponent.ImageList != null)
					{
						return Ribbon.SuperTipComponent.ImageList;
					}
					return Ribbon.ImageList;
				}
				return null;
			}
		}

		internal ControlBoxMenu(MxRibbon ribbon)
		{
			_ribbon = ribbon;
			_leftCommands = new CommandCollection(_ribbon, isQuickAccessCollection: false);
			_rightCommands = new CommandCollection(_ribbon, isQuickAccessCollection: false);
			_bottomCommands = new CommandCollection(_ribbon, isQuickAccessCollection: false);
			_leftPanelWidth = -1;
			_rightPanelWidth = -1;
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.ImageListProvider = this;
			_keyTip = "";
		}

		public bool ShouldSerializeSupertipSettingsData()
		{
			if (_superTipSettings.FooterImage != null)
			{
				return true;
			}
			if (_superTipSettings.FooterImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.FooterImageKey != null && _superTipSettings.FooterImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.FooterText != null && _superTipSettings.FooterText.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.HeaderImage != null)
			{
				return true;
			}
			if (_superTipSettings.HeaderImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.HeaderImageKey != null && _superTipSettings.HeaderImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.HeaderText != null && _superTipSettings.HeaderText.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.Image != null)
			{
				return true;
			}
			if (_superTipSettings.ImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.ImageKey != null && _superTipSettings.ImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.Text != null && _superTipSettings.Text.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.ToolTipStyle != ToolTipStyle.Office2007)
			{
				return true;
			}
			if (_superTipSettings.WordWrap)
			{
				return true;
			}
			return false;
		}

		public void ResetSupertipSettingsData()
		{
			_superTipSettings.FooterImage = null;
			_superTipSettings.FooterImageIndex = -1;
			_superTipSettings.FooterImageKey = "";
			_superTipSettings.FooterText = "";
			_superTipSettings.HeaderImage = null;
			_superTipSettings.HeaderImageIndex = -1;
			_superTipSettings.HeaderImageKey = "";
			_superTipSettings.HeaderText = "";
			_superTipSettings.Image = null;
			_superTipSettings.ImageIndex = -1;
			_superTipSettings.ImageKey = "";
			_superTipSettings.Text = "";
			_superTipSettings.ToolTipStyle = ToolTipStyle.Office2007;
			_superTipSettings.WordWrap = false;
		}

		internal void RemoveFromAllPanels(CommandBase command)
		{
			if (_leftCommands.Contains(command))
			{
				_leftCommands.Remove(command);
			}
			if (_rightCommands.Contains(command))
			{
				_rightCommands.Remove(command);
			}
			if (_bottomCommands.Contains(command))
			{
				_bottomCommands.Remove(command);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_leftPanelWidth = (int)LayoutReader.GetPropertyValue(reader, "LeftPanelWidth", typeof(int), -1);
			_rightPanelWidth = (int)LayoutReader.GetPropertyValue(reader, "RightPanelWidth", typeof(int), -1);
			_keyTip = (string)LayoutReader.GetPropertyValue(reader, "KeyTip", typeof(string), "");
			PropertyValue propertyValue = reader["SuperTipSettings"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(SuperTipSettings);
			}
			else
			{
				_superTipSettings = new SuperTipSettings();
				_superTipSettings.ImageListProvider = this;
			}
			propertyValue = reader["LeftCommands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_leftCommands);
			}
			else
			{
				_leftCommands.DisposeAll();
			}
			propertyValue = reader["RightCommands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_rightCommands);
			}
			else
			{
				_rightCommands.DisposeAll();
			}
			propertyValue = reader["BottomCommands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_bottomCommands);
				return;
			}
			_bottomCommands.DisposeAll();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter writer)
		{
			if (_superTipSettings != null && !_superTipSettings.IsEmpty)
			{
				writer.AddValue("SuperTipSettings", _superTipSettings);
			}
			if (_keyTip.Length > 0)
			{
				writer.AddValue("KeyTip", _keyTip);
			}
			if (_leftCommands != null && _leftCommands.Count > 0)
			{
				writer.AddValue("LeftCommands", _leftCommands);
			}
			if (_rightCommands != null && _rightCommands.Count > 0)
			{
				writer.AddValue("RightCommands", _rightCommands);
			}
			if (_bottomCommands != null && _bottomCommands.Count > 0)
			{
				writer.AddValue("BottomCommands", _bottomCommands);
			}
			if (_leftPanelWidth != -1)
			{
				writer.AddValue("LeftPanelWidth", _leftPanelWidth);
			}
			if (_rightPanelWidth != -1)
			{
				writer.AddValue("RightPanelWidth", _rightPanelWidth);
			}
		}
	}
}
