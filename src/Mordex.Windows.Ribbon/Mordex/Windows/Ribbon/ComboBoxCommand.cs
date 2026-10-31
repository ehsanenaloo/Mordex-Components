using System;
using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.Ribbon
{
	public class ComboBoxCommand : ContainerControlCommand
	{
		private MxComboBox _comboBox;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the UIComboBox control associated with the command.")]
		public MxComboBox ComboBox
		{
			get
			{
				if (_comboBox == null)
				{
					_comboBox = new MxComboBox();
					((IRibbonEditControl)_comboBox).ActAsRibbonEditControl = true;
					_comboBox.Enabled = Enabled;
				}
				return _comboBox;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Control Control
		{
			get
			{
				return ComboBox;
			}
			set
			{
				throw new InvalidOperationException("It is not possible to set the control for this command. Use a ContainerControlCommand instead");
			}
		}

		public ComboBoxCommand()
		{
		}

		public ComboBoxCommand(string key)
			: base(key)
		{
		}

		public ComboBoxCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public ComboBoxCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}

		protected override void OnBeforePaint()
		{
			base.OnBeforePaint();
			if (ComboBox != null && base.Ribbon != null)
			{
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && _comboBox != null)
			{
				((IRibbonEditControl)_comboBox).ActAsRibbonEditControl = false;
				_comboBox.Dispose();
				_comboBox = null;
			}
		}
	}
}
