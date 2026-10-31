using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	internal class NewCommandDialog : Form
	{
		private ImageList _smallImageList;

		private ImageList _largeImageList;

		private MxCommandManager _commandManager;

		private ComboBox _cboCommandType;

		private Label _lblCommandType;

		private Label _lblText;

		private Label _lblKey;

		private Label _lblCategory;

		private ComboBox _cboCategory;

		private TextBox _txtText;

		private TextBox _txtKey;

		private ComboBox _cboSmallImage;

		private Label _lblSmallImage;

		private Label _lblLargeImage;

		private ComboBox _cboLargeImage;

		private Button _btnCancel;

		private Button _btnOK;

		private Label _lblShortcut;

		private ComboBox _cboShortcut;

		private Container _components = new Container();

		public NewCommandDialog()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(NewCommandDialog));
			_cboCommandType = new ComboBox();
			_lblCommandType = new Label();
			_lblText = new Label();
			_txtText = new TextBox();
			_lblKey = new Label();
			_txtKey = new TextBox();
			_lblCategory = new Label();
			_cboCategory = new ComboBox();
			_cboSmallImage = new ComboBox();
			_lblSmallImage = new Label();
			_lblLargeImage = new Label();
			_cboLargeImage = new ComboBox();
			_btnCancel = new Button();
			_btnOK = new Button();
			_lblShortcut = new Label();
			_cboShortcut = new ComboBox();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_cboCommandType.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboCommandType.FormattingEnabled = true;
			_cboCommandType.Location = new Point(104, 86);
			_cboCommandType.Name = "cboCommandType";
			_cboCommandType.Size = new Size(180, 21);
			_cboCommandType.TabIndex = 2;
			_lblCommandType.AutoSize = true;
			_lblCommandType.Location = new Point(8, 90);
			_lblCommandType.Name = "label1";
			_lblCommandType.Size = new Size(81, 13);
			_lblCommandType.TabIndex = 1;
			_lblCommandType.Text = "Command Type:";
			_lblText.AutoSize = true;
			_lblText.Location = new Point(8, 53);
			_lblText.Name = "label2";
			_lblText.Size = new Size(29, 13);
			_lblText.TabIndex = 2;
			_lblText.Text = "Text:";
			_txtText.Location = new Point(48, 49);
			_txtText.Name = "txtText";
			_txtText.Size = new Size(236, 21);
			_txtText.TabIndex = 1;
			_lblKey.AutoSize = true;
			_lblKey.Location = new Point(8, 16);
			_lblKey.Name = "label3";
			_lblKey.Size = new Size(25, 13);
			_lblKey.TabIndex = 4;
			_lblKey.Text = "Key:";
			_txtKey.Location = new Point(48, 12);
			_txtKey.Name = "txtKey";
			_txtKey.Size = new Size(236, 21);
			_txtKey.TabIndex = 0;
			_lblCategory.AutoSize = true;
			_lblCategory.Location = new Point(8, 127);
			_lblCategory.Name = "label4";
			_lblCategory.Size = new Size(52, 13);
			_lblCategory.TabIndex = 6;
			_lblCategory.Text = "Category:";
			_cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboCategory.FormattingEnabled = true;
			_cboCategory.Location = new Point(76, 123);
			_cboCategory.Name = "cboCategory";
			_cboCategory.Size = new Size(208, 21);
			_cboCategory.TabIndex = 3;
			_cboSmallImage.DrawMode = DrawMode.OwnerDrawFixed;
			_cboSmallImage.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboSmallImage.FormattingEnabled = true;
			_cboSmallImage.Location = new Point(112, 192);
			_cboSmallImage.Name = "cboSmallImage";
			_cboSmallImage.Size = new Size(172, 22);
			_cboSmallImage.TabIndex = 5;
			_cboSmallImage.DrawItem += OnCboSmallImageDrawItem;
			_lblSmallImage.AutoSize = true;
			_lblSmallImage.Location = new Point(8, 196);
			_lblSmallImage.Name = "label7";
			_lblSmallImage.Size = new Size(89, 13);
			_lblSmallImage.TabIndex = 11;
			_lblSmallImage.Text = "SmallImageIndex:";
			_lblLargeImage.AutoSize = true;
			_lblLargeImage.Location = new Point(8, 232);
			_lblLargeImage.Name = "label8";
			_lblLargeImage.Size = new Size(92, 13);
			_lblLargeImage.TabIndex = 11;
			_lblLargeImage.Text = "LargeImageIndex:";
			_cboLargeImage.DrawMode = DrawMode.OwnerDrawFixed;
			_cboLargeImage.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboLargeImage.FormattingEnabled = true;
			_cboLargeImage.Location = new Point(112, 228);
			_cboLargeImage.Name = "cboLargeImage";
			_cboLargeImage.Size = new Size(172, 22);
			_cboLargeImage.TabIndex = 6;
			_cboLargeImage.DrawItem += OnCboLargeImageDrawItem;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(204, 260);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(76, 24);
			_btnCancel.TabIndex = 8;
			_btnCancel.Text = "Cancel";
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(120, 260);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(76, 24);
			_btnOK.TabIndex = 7;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_lblShortcut.AutoSize = true;
			_lblShortcut.Location = new Point(8, 160);
			_lblShortcut.Name = "lblShortcut";
			_lblShortcut.Size = new Size(48, 13);
			_lblShortcut.TabIndex = 11;
			_lblShortcut.Text = "Shortcut:";
			_cboShortcut.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboShortcut.FormattingEnabled = true;
			_cboShortcut.Location = new Point(76, 156);
			_cboShortcut.Name = "cboShortcut";
			_cboShortcut.Size = new Size(208, 21);
			_cboShortcut.TabIndex = 4;
			base.AcceptButton = _btnOK;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(294, 292);
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_lblSmallImage);
			base.Controls.Add(_lblCategory);
			base.Controls.Add(_lblKey);
			base.Controls.Add(_lblText);
			base.Controls.Add(_lblCommandType);
			base.Controls.Add(_lblLargeImage);
			base.Controls.Add(_cboSmallImage);
			base.Controls.Add(_cboCategory);
			base.Controls.Add(_txtKey);
			base.Controls.Add(_txtText);
			base.Controls.Add(_cboCommandType);
			base.Controls.Add(_cboLargeImage);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_lblShortcut);
			base.Controls.Add(_cboShortcut);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "NewBaseCommandForm";
			Text = "New Command";
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private string FindFreeCommandKey(int commandNumber)
		{
			string text = "Command" + commandNumber;
			foreach (MxCommand command in _commandManager.Commands)
			{
				if (string.Compare(command.Key, text, ignoreCase: true) == 0)
				{
					return FindFreeCommandKey(commandNumber + 1);
				}
			}
			return text;
		}

		internal MxCommand ShowAndCreateCommand(MxCommandManager uiCommandManager, ImageList imageList, ImageList newCommandDialogImageList)
		{
			_commandManager = uiCommandManager;
			_smallImageList = imageList;
			_largeImageList = newCommandDialogImageList;
			ShowDialog();
			if (base.DialogResult == DialogResult.OK)
			{
				MxCommand uiCommand = new MxCommand(_txtKey.Text);
				uiCommand.Text = _txtText.Text;
				uiCommand.CommandType = (CommandType)_cboCommandType.SelectedItem;
				ON oN = (ON)_cboSmallImage.SelectedItem;
				uiCommand.ImageIndex = (int)oN.Object;
				oN = (ON)_cboLargeImage.SelectedItem;
				uiCommand.LargeImageIndex = (int)oN.Object;
				uiCommand.Shortcut = (Shortcut)_cboShortcut.SelectedItem;
				if (_cboCategory.SelectedItem != null)
				{
					uiCommand.CategoryName = ((CommandCategory)((ON)_cboCategory.SelectedItem).Object).CategoryName;
				}
				return uiCommand;
			}
			return null;
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			_cboCommandType.Items.Add(CommandType.ColorPickerCommand);
			_cboCommandType.Items.Add(CommandType.ComboBoxCommand);
			_cboCommandType.Items.Add(CommandType.Command);
			_cboCommandType.Items.Add(CommandType.ControlContainer);
			_cboCommandType.Items.Add(CommandType.ControlPopup);
			_cboCommandType.Items.Add(CommandType.FontPickerCommand);
			_cboCommandType.Items.Add(CommandType.Label);
			_cboCommandType.Items.Add(CommandType.MdiList);
			_cboCommandType.Items.Add(CommandType.Separator);
			_cboCommandType.Items.Add(CommandType.TextBoxCommand);
			_cboCommandType.Items.Add(CommandType.ToggleButton);
			_cboCommandType.SelectedIndex = 2;
			Array values = Enum.GetValues(typeof(Shortcut));
			foreach (object item in values)
			{
				_cboShortcut.Items.Add(item);
			}
			_cboShortcut.SelectedItem = Shortcut.None;
			FillSmallImageCombo();
			FillLargeImageCombo();
			if (_commandManager == null)
			{
				return;
			}
			_txtKey.Text = FindFreeCommandKey(0);
			foreach (CommandCategory category in _commandManager.Categories)
			{
				_cboCategory.Items.Add(new ON(category, category.CategoryName));
			}
			_cboCategory.DisplayMember = "Name";
			_cboCategory.ValueMember = "Object";
		}

		private void FillLargeImageCombo()
		{
			_cboLargeImage.Items.Add(new ON(-1, "(none)"));
			if (_largeImageList != null)
			{
				for (int i = 0; i < _largeImageList.Images.Count; i++)
				{
					_cboLargeImage.Items.Add(new ON(i, "Image[" + i + "]"));
				}
			}
			_cboLargeImage.DisplayMember = "Name";
			_cboLargeImage.ValueMember = "Object";
			_cboLargeImage.SelectedIndex = 0;
		}

		private void FillSmallImageCombo()
		{
			_cboSmallImage.Items.Add(new ON(-1, "(none)"));
			if (_smallImageList != null)
			{
				for (int i = 0; i < _smallImageList.Images.Count; i++)
				{
					_cboSmallImage.Items.Add(new ON(i, "Image[" + i + "]"));
				}
			}
			_cboSmallImage.DisplayMember = "Name";
			_cboSmallImage.ValueMember = "Object";
			_cboSmallImage.SelectedIndex = 0;
		}

		private void OnCboSmallImageDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			Rectangle bounds = eventArgs.Bounds;
			Rectangle rectangle = new Rectangle(bounds.Left + 1, bounds.Top + 1, 18, bounds.Height - 2);
			Brush brush = new SolidBrush(eventArgs.BackColor);
			eventArgs.Graphics.FillRectangle(brush, eventArgs.Bounds);
			eventArgs.Graphics.FillRectangle(Brushes.White, rectangle);
			brush.Dispose();
			brush = null;
			rectangle = RenderingHelper.DrawRectangleOutline(eventArgs.Graphics, SystemPens.WindowFrame, rectangle);
			string text = "";
			ON oN = (ON)((ComboBox)sender).Items[eventArgs.Index];
			int imageIndex = (int)oN.Object;
			if (imageIndex < 0)
			{
				text = "(none)";
			}
			else
			{
				if (_smallImageList != null)
				{
					eventArgs.Graphics.DrawImage(_smallImageList.Images[imageIndex], rectangle, 0, 0, _smallImageList.ImageSize.Width, _smallImageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
				text = "Image[" + imageIndex + "]";
			}
			if (text.Length > 0)
			{
				rectangle = new Rectangle(rectangle.Right + 1, bounds.Top, bounds.Width - rectangle.Width - 2, bounds.Height);
				StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap);
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				Brush textBrush = new SolidBrush(eventArgs.ForeColor);
				eventArgs.Graphics.DrawString(text, eventArgs.Font, textBrush, rectangle, stringFormat);
				textBrush.Dispose();
				textBrush = null;
			}
		}

		private void OnCboLargeImageDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			Rectangle bounds = eventArgs.Bounds;
			Rectangle rectangle = new Rectangle(bounds.Left + 1, bounds.Top + 1, 18, bounds.Height - 2);
			Brush brush = new SolidBrush(eventArgs.BackColor);
			eventArgs.Graphics.FillRectangle(brush, eventArgs.Bounds);
			eventArgs.Graphics.FillRectangle(Brushes.White, rectangle);
			brush.Dispose();
			brush = null;
			rectangle = RenderingHelper.DrawRectangleOutline(eventArgs.Graphics, SystemPens.WindowFrame, rectangle);
			string text = "";
			ON oN = (ON)((ComboBox)sender).Items[eventArgs.Index];
			int imageIndex = (int)oN.Object;
			if (imageIndex < 0)
			{
				text = "(none)";
			}
			else
			{
				if (_largeImageList != null)
				{
					eventArgs.Graphics.DrawImage(_largeImageList.Images[imageIndex], rectangle, 0, 0, _largeImageList.ImageSize.Width, _largeImageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
				text = "Image[" + imageIndex + "]";
			}
			if (text.Length > 0)
			{
				rectangle = new Rectangle(rectangle.Right + 1, bounds.Top, bounds.Width - rectangle.Width - 2, bounds.Height);
				StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap);
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				Brush textBrush = new SolidBrush(eventArgs.ForeColor);
				eventArgs.Graphics.DrawString(text, eventArgs.Font, textBrush, rectangle, stringFormat);
				textBrush.Dispose();
				textBrush = null;
			}
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			string text = _txtKey.Text;
			if (text.Length <= 0)
			{
				MessageBox.Show(this, "You must specify a key for the command", "Invalid Key", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				_txtKey.Focus();
				base.DialogResult = DialogResult.None;
			}
			else if (!_commandManager.IsCommandKeyUnique(null, text))
			{
				string message = string.Format("A command with key '{0}' already exist in the Commands collection of the UICommandManager.", text);
				MessageBox.Show(this, message, "Invalid Key", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				_txtKey.Focus();
				base.DialogResult = DialogResult.None;
			}
			else
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
