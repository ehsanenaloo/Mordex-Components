using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Design.Designer;

namespace Mordex.Windows.DataGrid.Design
{
	internal class CustomGroupRowsDialog : Form
	{
		private GridCustomGroupRowCollection _gridCustomGroupRowCollection;

		private IContainer _iContainer = default;

		private GridEditorControl _collectionEditor;

		private Button _btnOK;

		private Button _btnCancel;

		internal CustomGroupRowsDialog()
		{
			InitializeControls();
		}

		private void OnControlLoad(object sender, EventArgs eventArgs)
		{
		}

		internal void SetCustomGroupRows(GridCustomGroupRowCollection gridCustomGroupRowCollection)
		{
			if (_gridCustomGroupRowCollection != gridCustomGroupRowCollection)
			{
				_gridCustomGroupRowCollection = gridCustomGroupRowCollection;
				((IGridElement)_collectionEditor).SelectedObject = _gridCustomGroupRowCollection;
			}
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			if (!((IGridElement)_collectionEditor).CommitChanges())
			{
				base.DialogResult = DialogResult.None;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_collectionEditor = new GridEditorControl();
			_btnOK = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			_collectionEditor.Location = new Point(6, 0);
			_collectionEditor.Name = "collectionPage1";
			_collectionEditor.Size = new Size(576, 310);
			_collectionEditor.TabIndex = 0;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(416, 316);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 28);
			_btnOK.TabIndex = 1;
			_btnOK.Text = "OK";
			_btnOK.UseVisualStyleBackColor = true;
			_btnOK.Click += OnBtnOKClick;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(502, 316);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 28);
			_btnCancel.TabIndex = 2;
			_btnCancel.Text = "Cancel";
			_btnCancel.UseVisualStyleBackColor = true;
			base.AcceptButton = _btnOK;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(586, 351);
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_collectionEditor);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.Name = "CustomGroupRowsDialog";
			Text = "Custom Group Rows";
			base.Load += OnControlLoad;
			ResumeLayout(performLayout: false);
		}
	}
}
