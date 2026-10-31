using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	[Designer("Mordex.Windows.Ribbon.Design.BackstageMenuDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class BackstageMenu : RibbonTab, ICommandContainer, IInvalidatable
	{
		private Color _backColor;

		private CommandCollection _commands = default;

		private BackstageTab _selectedTab;

		private Color _blendedColor;

		private Color _backstageMenuColor;

		private AccessibleObject _cachedAccessibleObject;

		internal CommandBaseRenderer _owner;

		internal bool _deserializeCommands = true;

		private static readonly object s_selectedTabChangedEventKey = new object();

		private static readonly object s_changingSelectedTabEventKey = new object();

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new RibbonGroupCollection Groups => base.Groups;

		[Description("The background color of this backstage menu tab.")]
		public Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				if (_backColor != value)
				{
					_backColor = value;
					RecalculateColors();
					Invalidate();
				}
			}
		}

		[Description("Represents the text displayed on the tab.")]
		[Localizable(true)]
		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (base.Text != value)
				{
					base.Text = value;
					RequestRelayout(false);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public CommandCollection Commands
		{
			get
			{
				if (_standaloneRibbon != null)
				{
					return _standaloneRibbon._commands;
				}
				return _commands;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		[Browsable(false)]
		public BackstageTab SelectedTab
		{
			get
			{
				return _selectedTab;
			}
			set
			{
				if (_selectedTab == value)
				{
					return;
				}
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				CommandCancelEventArgs e = new CommandCancelEventArgs(_selectedTab);
				OnChangingSelectedTab(e);
				if (!e.Cancel || _selectedTab == null || value == null)
				{
					if (_selectedTab != null)
					{
						_selectedTab.SetSelectedState(false);
					}
					CommandEventArgs e2 = new CommandEventArgs(value);
					_selectedTab = value;
					if (_selectedTab != null)
					{
						_selectedTab.SetSelectedState(true);
					}
					Invalidate();
					OnSelectedTabChanged(e2);
				}
			}
		}

		CommandCollection ICommandContainer.Commands => Commands;

		MxRibbon ICommandContainer.Ribbon => _standaloneRibbon;

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu => null;

		CommandType ICommandContainer.CommandType => CommandType.BackstageMenu;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Small;

		bool ICommandContainer.IsEnabled => true;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Vertical;

		AccessibleObject ICommandContainer.AccessibilityObject => GetAccessibleObject();

		[Description("Event fired when the SelectedTab property has changed.")]
		public event CommandEventHandler SelectedTabChanged
		{
			add
			{
				base.Events.AddHandler(s_selectedTabChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectedTabChangedEventKey, value);
			}
		}

		[Description("Event fired when a new BackstageTab is about to be selected")]
		public event CommandCancelEventHandler ChangingSelectedTab
		{
			add
			{
				base.Events.AddHandler(s_changingSelectedTabEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_changingSelectedTabEventKey, value);
			}
		}

		internal BackstageMenu(MxRibbon ribbon)
		{
			_standaloneRibbon = ribbon;
			InitializeFields();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeText()
		{
			return string.Compare("File", Text) != 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetText()
		{
			Text = "File";
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(Color.FromArgb(252, 161, 18));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColor()
		{
			BackColor = Color.FromArgb(252, 161, 18);
		}

		internal Color GetBackstageMenuColor()
		{
			return _backstageMenuColor;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_cachedAccessibleObject == null)
			{
				_cachedAccessibleObject = new BackstageMenuAccessibleObject(this);
			}
			return _cachedAccessibleObject;
		}

		public void DropDown()
		{
			if (base.Ribbon != null)
			{
				base.Ribbon.ShowBackstageMenu();
			}
		}

		public void Hide()
		{
			if (base.Ribbon != null)
			{
				base.Ribbon.CloseBackstage();
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			BackColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), Color.FromArgb(252, 161, 18));
			if (!_deserializeCommands)
			{
				return;
			}
			PropertyValue propertyValue = reader["Commands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(Commands);
				return;
			}
			Commands.DisposeAll();
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (ShouldSerializeBackColor())
			{
				writer.AddValue("BackColor", _backColor);
			}
			if (Commands.Count > 0)
			{
				writer.AddValue("Commands", Commands);
			}
		}

		internal void ResetToDefaults()
		{
			BackColor = Color.FromArgb(252, 161, 18);
			base.AllowMerge = true;
			base.Enabled = true;
			base.ForegroundStyle.ResetAll();
			base.Icon = null;
			base.Image = null;
			base.ImageIndex = -1;
			base.ImageKey = null;
			base.Key = "";
			base.KeyTip = "";
			base.MergeType = CommandMergeType.MergeItems;
			base.MergeOrder = -1;
			base.Name = null;
			base.Tag = null;
			Text = "File";
			base.Visible = true;
		}

		internal bool IsModifiedFromDefaults()
		{
			if (ShouldSerializeBackColor())
			{
				return true;
			}
			if (!base.AllowMerge)
			{
				return true;
			}
			if (!base.Enabled)
			{
				return true;
			}
			if (base.ForegroundStyle.HasCustomStyle())
			{
				return true;
			}
			if (base.Icon != null)
			{
				return true;
			}
			if (base.Image != null)
			{
				return true;
			}
			if (base.ImageIndex != -1)
			{
				return true;
			}
			if (base.ImageKey != null)
			{
				return true;
			}
			if (base.Key != null)
			{
				return true;
			}
			if (base.KeyTip.Length > 0)
			{
				return true;
			}
			if (base.MergeType != CommandMergeType.MergeItems)
			{
				return true;
			}
			if (base.MergeOrder != -1)
			{
				return true;
			}
			if (base.Name != null && base.Name.Length > 0)
			{
				return true;
			}
			if (base.Tag != null)
			{
				return true;
			}
			if (Text != "File")
			{
				return true;
			}
			if (!base.Visible)
			{
				return true;
			}
			return false;
		}

		public new string ToString()
		{
			PropertyValue root = new PropertyValue("BackstageMenu", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
			return mordexLayoutWriter.ToString();
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			Text = "File";
			BackColor = Color.FromArgb(252, 161, 18);
		}

		internal override void Invalidate()
		{
			if (base.Ribbon != null)
			{
				base.Ribbon.InvalidateBackstage();
			}
		}

		private void RecalculateColors()
		{
			int backColorBrightness = RibbonDrawingUtils.GetLightness(_backColor);
			double redScale = 1f - (1f - (float)(int)_backColor.R / 255f);
			double greenScale = 1f - (1f - (float)(int)_backColor.G / 255f);
			double blueScale = 1f - (1f - (float)(int)_backColor.B / 255f);
			double redValue = Math.Min((double)(int)_backColor.R * redScale, 255.0);
			double greenValue = Math.Min((double)(int)_backColor.G * greenScale, 255.0);
			double blueValue = Math.Min((double)(int)_backColor.B * blueScale, 255.0);
			Color color = Color.FromArgb((int)redValue, (int)greenValue, (int)blueValue);
			_blendedColor = RibbonDrawingUtils.BlendColors(BackColor, color, 0.65f);
			if (backColorBrightness > 120)
			{
				redScale = 1f + (1f - (float)(int)_backColor.R / 255f);
				greenScale = 1f + (1f - (float)(int)_backColor.G / 255f);
				blueScale = 1f + (1f - (float)(int)_backColor.B / 255f);
			}
			else
			{
				redScale = 2.0;
				greenScale = 2.0;
				blueScale = 2.0;
			}
			redValue = Math.Min((double)(int)_backColor.R * redScale, 255.0);
			greenValue = Math.Min((double)(int)_backColor.G * greenScale, 255.0);
			blueValue = Math.Min((double)(int)_backColor.B * blueScale, 255.0);
			_backstageMenuColor = Color.FromArgb(255, (int)redValue, (int)greenValue, (int)blueValue);
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			if (commandType == CommandType.SmallButton || commandType == CommandType.LargeButton || commandType == CommandType.BackstageTab)
			{
				return true;
			}
			return false;
		}

		void ICommandContainer.CommandClicked(CommandEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandClicked(eventArgs);
			}
		}

		void ICommandContainer.CommandAdded(CommandBase command)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandAdded(command);
			}
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandRemoved(command);
			}
		}

		bool ICommandContainer.CommandDropDownClosing(DropDownClosingEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandDropDownClosing(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDropDownClosed(CommandEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandDropDownClosed(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownOpening(CommandCancelEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandDropDownOpening(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDropDownOpened(CommandEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandDropDownOpened(eventArgs);
			}
		}

		void ICommandContainer.CommandDisposed(CommandEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandDisposed(eventArgs);
			}
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
			if (_standaloneRibbon != null)
			{
				((ICommandContainer)_standaloneRibbon).CommandCheckedChanged(eventArgs);
			}
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			Commands.RemoveFromInnerListAndClearOwner(command);
		}

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
			Commands.Insert(index, command);
		}

		protected virtual void OnSelectedTabChanged(CommandEventArgs e)
		{
			if (base.Events[s_selectedTabChangedEventKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnChangingSelectedTab(CommandCancelEventArgs e)
		{
			if (base.Events[s_changingSelectedTabEventKey] is CommandCancelEventHandler commandCancelEventHandler)
			{
				commandCancelEventHandler(this, e);
			}
		}
	}
}
