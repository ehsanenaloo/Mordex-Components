using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon.Design
{
	public class ObjectTypeDialog : Form
	{
		private Label _lblType;

		private ComboBox _cboType;

		private Label _lblValue;

		private TextBox _txtValue;

		private Button _btnOK;

		private Button _btnCancel;

		private static readonly TypeCode[] s_allowedTypeCodes =
		{
			TypeCode.String, TypeCode.Boolean, TypeCode.Char, TypeCode.SByte, TypeCode.Byte,
			TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
			TypeCode.Single, TypeCode.Double, TypeCode.Decimal, TypeCode.DateTime
		};

		private object _value;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public ObjectTypeDialog()
		{
			InitializeControls();
		}

		private void InitializeControls()
		{
			_cboType = new ComboBox();
			_lblType = new Label();
			_lblValue = new Label();
			_txtValue = new TextBox();
			_btnOK = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_cboType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_cboType.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboType.FormattingEnabled = true;
			_cboType.Location = new Point(56, 9);
			_cboType.Name = "cboType";
			_cboType.Size = new Size(172, 21);
			_cboType.TabIndex = 0;
			_lblType.AutoSize = true;
			_lblType.Location = new Point(8, 12);
			_lblType.Name = "lblType";
			_lblType.Size = new Size(31, 13);
			_lblType.TabIndex = 1;
			_lblType.Text = "Type:";
			_lblValue.AutoSize = true;
			_lblValue.Location = new Point(8, 40);
			_lblValue.Name = "lblValue";
			_lblValue.Size = new Size(33, 13);
			_lblValue.TabIndex = 1;
			_lblValue.Text = "Value:";
			_txtValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_txtValue.Location = new Point(56, 37);
			_txtValue.Name = "txtValue";
			_txtValue.Size = new Size(172, 21);
			_txtValue.TabIndex = 2;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(76, 72);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(72, 24);
			_btnOK.TabIndex = 3;
			_btnOK.Text = "OK";
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(156, 72);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(72, 24);
			_btnCancel.TabIndex = 4;
			_btnCancel.Text = "Cancel";
			base.ClientSize = new Size(236, 106);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_txtValue);
			base.Controls.Add(_lblType);
			base.Controls.Add(_lblValue);
			base.Controls.Add(_cboType);
			base.Controls.Add(_btnCancel);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "ObjectTypeDialog";
			base.StartPosition = FormStartPosition.CenterParent;
			Text = "Object Editor:";
			base.Load += OnControlLoad;
			base.FormClosing += OnControlClosing;
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private object ParseValue(string strA)
		{
			if (strA == null)
			{
				return null;
			}
			TypeCode typeCode = _cboType.SelectedItem is TypeCode selected ? selected : TypeCode.String;
			if (typeCode == TypeCode.Boolean)
			{
				return string.Equals(strA, bool.TrueString, StringComparison.OrdinalIgnoreCase);
			}
			return Convert.ChangeType(strA, typeCode, CultureInfo.InvariantCulture);
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
			foreach (TypeCode item in s_allowedTypeCodes)
			{
				_cboType.Items.Add(item);
			}
			TypeCode current = Value == null ? TypeCode.String : Type.GetTypeCode(Value.GetType());
			if (Value == null || Array.IndexOf(s_allowedTypeCodes, current) < 0)
			{
				_cboType.SelectedItem = TypeCode.String;
				return;
			}
			_cboType.SelectedItem = current;
			try
			{
				_txtValue.Text = PropertyValue.ConvertObjectToString(Value);
			}
			catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is ArgumentException || ex is NotSupportedException)
			{
				// The value has no text form the layout writer understands, so start with an empty box.
				_cboType.SelectedItem = TypeCode.String;
				_txtValue.Text = "";
			}
		}

		private void OnControlClosing(object sender, CancelEventArgs eventArgs)
		{
			if (base.DialogResult == DialogResult.OK)
			{
				try
				{
					Value = ParseValue(_txtValue.Text);
				}
				catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
				{
					MessageBox.Show(this, "Can't convert '" + _txtValue.Text + "' to: " + _cboType.Text, "Error");
					eventArgs.Cancel = true;
				}
			}
		}
	}
}
