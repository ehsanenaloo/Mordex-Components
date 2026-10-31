using System;
using Mordex.Windows.DataGrid.Internal;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;
using Microsoft.Win32;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal class DropDownDesignerDialog : Form
	{
		private Container _container = new Container();

		private Panel _panel;

		private Button _btnOK;

		private Button _btnCancel;

		private Button _btnPreview;

		private MxMultiColumnComboBox _previewCombo;

		private MxMultiColumnComboBox _multiColumnCombo;

		private MxCheckedComboBox _checkedComboBox;

		private MultiColumnComboEditorControl _multiColumnComboEditorControl;

		internal DropDownDesignerDialog()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		internal DialogResult ShowForCombo(MxMultiColumnComboBox multiColumnCombo)
		{
			if (multiColumnCombo == null)
			{
				throw new ArgumentNullException();
			}
			_multiColumnCombo = multiColumnCombo;
			Text = "DropDown Control Designer - " + multiColumnCombo.Name;
			UpdateMultiColumnComboEditorControl();
			return ShowDialog();
		}

		internal DialogResult ComputeDialogResult(MxCheckedComboBox checkedComboBox)
		{
			if (checkedComboBox == null)
			{
				throw new ArgumentNullException();
			}
			_checkedComboBox = checkedComboBox;
			Text = "DropDown Control Designer - " + _checkedComboBox.Name;
			UpdateMultiColumnComboEditorControl();
			return ShowDialog();
		}

		private void UpdateMultiColumnComboEditorControl()
		{
			_multiColumnComboEditorControl = new MultiColumnComboEditorControl();
			_multiColumnComboEditorControl.Dock = DockStyle.Fill;
			if (_multiColumnCombo != null)
			{
				((IDesignerPage)_multiColumnComboEditorControl).DataGrid = _multiColumnCombo.DropDownList;
			}
			else if (_checkedComboBox != null)
			{
				((IDesignerPage)_multiColumnComboEditorControl).DataGrid = _checkedComboBox.DropDownList;
			}
			_panel.Controls.Add(_multiColumnComboEditorControl);
		}

		private void InitializeControls()
		{
			GridLayout gridLayout = new GridLayout();
			_panel = new Panel();
			_btnOK = new Button();
			_btnCancel = new Button();
			_btnPreview = new Button();
			_previewCombo = new MxMultiColumnComboBox();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_panel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_panel.Location = new Point(4, 4);
			_panel.Name = "panel1";
			_panel.Size = new Size(620, 343);
			_panel.TabIndex = 0;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(460, 355);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 28);
			_btnOK.TabIndex = 2;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(544, 355);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 28);
			_btnCancel.TabIndex = 3;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			_btnPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnPreview.Location = new Point(376, 355);
			_btnPreview.Name = "btnPreview";
			_btnPreview.Size = new Size(80, 28);
			_btnPreview.TabIndex = 4;
			_btnPreview.Text = "Preview...";
			_btnPreview.Click += OnBtnPreviewClick;
			_btnPreview.MouseDown += OnBtnPreviewMouseDown;
			_previewCombo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_previewCombo.BackColor = SystemColors.Window;
			_previewCombo.ControlStyle.ButtonAppearance = ButtonAppearance.Regular;
			gridLayout.LayoutString = "<GridLayoutData><RootTable><GroupCondition ID=\"\" /></RootTable><WatermarkImage /><BackColor>ControlLight</BackColor><AllowEdit>False</AllowEdit><ExpandableGroups>False</ExpandableGroups><GroupByBoxVisible>False</GroupByBoxVisible><HideSelection>Highlight</HideSelection></GridLayoutData>";
			_previewCombo.DesignTimeLayout = gridLayout;
			_previewCombo.DisplayMember = "";
			_previewCombo.ForeColor = SystemColors.WindowText;
			_previewCombo.ImageVerticalAlignment = ImageVerticalAlignment.Center;
			_previewCombo.Location = new Point(376, 360);
			_previewCombo.Name = "multiColumnCombo1";
			_previewCombo.Size = new Size(72, 20);
			_previewCombo.TabIndex = 5;
			_previewCombo.ValueMember = "";
			_previewCombo.Enter += OnMultiColumnCombo1Enter;
			_previewCombo.CloseUp += OnMultiColumnCombo1CloseUp;
			base.AcceptButton = _btnOK;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(628, 389);
			base.Controls.AddRange(new Control[5] { _btnCancel, _btnOK, _panel, _btnPreview, _previewCombo });
			base.Name = "DropDownDesignerDialog";
			Text = "Drop Down Designer";
			ResumeLayout(performLayout: false);
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			((IDesignerPage)_multiColumnComboEditorControl).CommitChanges();
			Close();
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnBtnPreviewClick(object sender, EventArgs eventArgs)
		{
			((IDesignerPage)_multiColumnComboEditorControl).CommitChanges();
			if (_multiColumnCombo != null)
			{
				_previewCombo.DesignTimeLayout = _multiColumnCombo.DesignTimeLayout;
			}
			else if (_checkedComboBox.DesignTimeLayout != null)
			{
				_previewCombo.DesignTimeLayout = _checkedComboBox.DesignTimeLayout;
			}
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new object());
			_previewCombo.DataSource = arrayList;
			_previewCombo.DroppedDown = !_previewCombo.DroppedDown;
			if (_previewCombo.DroppedDown)
			{
				_previewCombo.DropDownList.Capture = true;
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			base.OnFormClosing(eventArgs);
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\MultiColumnCombo Control");
				_ = Rectangle.Empty;
				registryKey.SetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5));
				registryKey.SetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5));
				registryKey.SetValue("Top", base.Top);
				registryKey.SetValue("Left", base.Left);
				registryKey.SetValue("Window State", base.WindowState);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\MultiColumnCombo Control");
				base.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), (string)registryKey.GetValue("Window State", "Normal"));
				if (base.WindowState == FormWindowState.Minimized)
				{
					base.WindowState = FormWindowState.Normal;
				}
				if (base.WindowState != FormWindowState.Minimized)
				{
					Rectangle empty = Rectangle.Empty;
					empty.Width = LogicalToDeviceUnits((int)registryKey.GetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5)));
					empty.Height = LogicalToDeviceUnits((int)registryKey.GetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5)));
					empty.Y = (int)registryKey.GetValue("Top", base.Top);
					empty.X = (int)registryKey.GetValue("Left", base.Left);
					if (empty.Width > SystemInformation.VirtualScreen.Width)
					{
						empty.Width = SystemInformation.VirtualScreen.Width;
					}
					if (empty.Height > SystemInformation.VirtualScreen.Height)
					{
						empty.Height = SystemInformation.VirtualScreen.Height;
					}
					if (empty.Right > SystemInformation.VirtualScreen.Right)
					{
						empty.X = SystemInformation.VirtualScreen.Right - empty.Width;
					}
					if (empty.X < SystemInformation.VirtualScreen.Left)
					{
						empty.X = SystemInformation.VirtualScreen.Left;
					}
					if (empty.Bottom > SystemInformation.VirtualScreen.Bottom)
					{
						empty.Y = SystemInformation.VirtualScreen.Bottom - empty.Height;
					}
					if (empty.Y < SystemInformation.VirtualScreen.Top)
					{
						empty.Y = SystemInformation.VirtualScreen.Top;
					}
					base.Bounds = empty;
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void OnBtnPreviewMouseDown(object sender, MouseEventArgs eventArgs)
		{
		}

		private void OnMultiColumnCombo1Enter(object sender, EventArgs eventArgs)
		{
		}

		private void OnMultiColumnCombo1CloseUp(object sender, EventArgs eventArgs)
		{
			_btnPreview.Focus();
		}
	}
}
