using System;
using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public class TextBoxCommand : ContainerControlCommand
	{
		private TextBox _textBox;

		private ThemeKind? _nativeKind;

		[Description("Represents the System.Windows.Forms.TextBox control associated with the command.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TextBox TextBox
		{
			get
			{
				if (_textBox == null)
				{
					_textBox = new TextBox();
					_textBox.Enabled = Enabled;
				}
				return _textBox;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Control Control
		{
			get
			{
				return TextBox;
			}
			set
			{
				throw new InvalidOperationException("It is not possible to set the control for this command. Use a ContainerControlCommand instead");
			}
		}

		protected override void OnBeforePaint()
		{
			base.OnBeforePaint();
			if (_textBox != null && base.Ribbon != null)
			{
				ResolvedTheme theme = MxThemeManager.ResolveDefault(base.Ribbon.DeviceDpi);
				_textBox.BackColor = theme.Color(ThemeRole.InputBack);
				_textBox.ForeColor = theme.Color(ThemeRole.Text);
				if (_textBox.BorderStyle == BorderStyle.Fixed3D)
				{
					_textBox.BorderStyle = BorderStyle.FixedSingle;
				}
				if (_nativeKind != theme.Theme.Kind)
				{
					_nativeKind = theme.Theme.Kind;
					ThemeWindow.ApplyNativeLook(_textBox, theme.Theme);
				}
			}
		}

		public TextBoxCommand()
		{
		}

		public TextBoxCommand(string key)
			: base(key)
		{
		}

		public TextBoxCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public TextBoxCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}

		public override object Clone()
		{
			ButtonCommand buttonCommand = (ButtonCommand)base.Clone();
			TextBoxCommand textBoxCommand = new TextBoxCommand();
			textBoxCommand.Description = buttonCommand.Description;
			textBoxCommand.Enabled = buttonCommand.Enabled;
			textBoxCommand.Icon = buttonCommand.Icon;
			textBoxCommand.Image = buttonCommand.Image;
			textBoxCommand.ImageIndex = buttonCommand.ImageIndex;
			textBoxCommand.Key = buttonCommand.Key;
			textBoxCommand.KeyTip = buttonCommand.KeyTip;
			textBoxCommand.LargeIcon = buttonCommand.LargeIcon;
			textBoxCommand.LargeImage = buttonCommand.LargeImage;
			textBoxCommand.LargeImageIndex = buttonCommand.LargeImageIndex;
			textBoxCommand.Shortcut = buttonCommand.Shortcut;
			textBoxCommand.SizeStyle = buttonCommand.SizeStyle;
			textBoxCommand.Style = buttonCommand.Style;
			if (buttonCommand._superTipSettings != null)
			{
				textBoxCommand._superTipSettings = buttonCommand.SuperTipSettings.Clone();
				textBoxCommand._superTipSettings.ImageListProvider = textBoxCommand;
			}
			textBoxCommand.Text = buttonCommand.Text;
			textBoxCommand.Visible = buttonCommand.Visible;
			textBoxCommand.ControlWidth = base.ControlWidth;
			return textBoxCommand;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && _textBox != null)
			{
				_textBox.Dispose();
				_textBox = null;
			}
		}
	}
}
