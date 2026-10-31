using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	public class ContainerControlCommand : ButtonCommand
	{
		private int _controlWidth;

		private Control _control;

		[DefaultValue(null)]
		[Editor(typeof(ControlEditor), typeof(UITypeEditor))]
		[Description("Represents the System.Windows.Forms.Control associated with this command.")]
		public virtual Control Control
		{
			get
			{
				return _control;
			}
			set
			{
				if (_control != value)
				{
					_control = value;
					if (_control != null)
					{
						_control.Enabled = Enabled;
					}
					NotifyTextChanged("Control");
				}
			}
		}

		internal override CommandType CommandType => CommandType.ContainerControl;

		[Description("Specifies how the command is displayed in the Ribbon control.")]
		[DefaultValue(CommandSizeStyle.Small)]
		public override CommandSizeStyle SizeStyle
		{
			get
			{
				return CommandSizeStyle.Small;
			}
			set
			{
				if (value == CommandSizeStyle.Large)
				{
					throw new InvalidOperationException("Large SizeStyle is not supported for ContainerControlCommands");
				}
			}
		}

		[Description("Represents the width of the control associated with this command.")]
		[DefaultValue(100)]
		public int ControlWidth
		{
			get
			{
				return _controlWidth;
			}
			set
			{
				if (_controlWidth != value)
				{
					_controlWidth = value;
					NotifyTextChanged("ControlWidth");
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the Command is enabled.")]
		public override bool Enabled
		{
			get
			{
				return base.Enabled;
			}
			set
			{
				base.Enabled = value;
				if (Control != null)
				{
					Control.Enabled = Enabled;
				}
			}
		}

		public ContainerControlCommand()
		{
		}

		public ContainerControlCommand(string key)
			: base(key)
		{
		}

		public ContainerControlCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public ContainerControlCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_controlWidth = 100;
		}

		public override object Clone()
		{
			ButtonCommand buttonCommand = (ButtonCommand)base.Clone();
			ContainerControlCommand containerControlCommand = new ContainerControlCommand();
			containerControlCommand.Description = buttonCommand.Description;
			containerControlCommand.Enabled = buttonCommand.Enabled;
			containerControlCommand.Icon = buttonCommand.Icon;
			containerControlCommand.Image = buttonCommand.Image;
			containerControlCommand.ImageIndex = buttonCommand.ImageIndex;
			containerControlCommand.ImageKey = buttonCommand.ImageKey;
			containerControlCommand.Key = buttonCommand.Key;
			containerControlCommand.KeyTip = buttonCommand.KeyTip;
			containerControlCommand.LargeIcon = buttonCommand.LargeIcon;
			containerControlCommand.LargeImage = buttonCommand.LargeImage;
			containerControlCommand.LargeImageIndex = buttonCommand.LargeImageIndex;
			containerControlCommand.LargeImageKey = buttonCommand.LargeImageKey;
			containerControlCommand.Shortcut = buttonCommand.Shortcut;
			containerControlCommand.SizeStyle = buttonCommand.SizeStyle;
			containerControlCommand.Style = buttonCommand.Style;
			if (buttonCommand._superTipSettings != null)
			{
				containerControlCommand._superTipSettings = buttonCommand.SuperTipSettings.Clone();
				containerControlCommand._superTipSettings.ImageListProvider = containerControlCommand;
			}
			containerControlCommand.Text = buttonCommand.Text;
			containerControlCommand.Visible = buttonCommand.Visible;
			containerControlCommand.ControlWidth = ControlWidth;
			return containerControlCommand;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_controlWidth != 100)
			{
				writer.AddValue("ControlWidth", _controlWidth);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			ControlWidth = (int)LayoutReader.GetPropertyValue(reader, "ControlWidth", typeof(int), 100);
		}

		internal void InvokeBeforePaint()
		{
			OnBeforePaint();
		}

		protected virtual void OnBeforePaint()
		{
			if (Control is IRibbonEditControl)
			{
				((IRibbonEditControl)Control).ActAsRibbonEditControl = true;
			}
			if (Control != null)
			{
				Control.Enabled = IsEnabled;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_control = null;
			}
		}
	}
}
