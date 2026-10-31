using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	internal class MxFilterBuilderDialog : Form, IDesignDialog
	{
		private FilterConditionsEditor _conditionsEditor;

		private Button _btnOk;

		private Button _btnCancel;

		private Container _container = new Container();

		private GridTable _filterTable;

		object IDesignDialog.Value => GetFilterCondition();

		internal MxFilterBuilderDialog()
		{
			InitializeControls();
			base.StartPosition = FormStartPosition.CenterScreen;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		public GridFilterCondition GetFilterCondition()
		{
			return _conditionsEditor.GetFilterCondition();
		}

		public void SetFilterCondition(GridFilterCondition gridFilterCondition)
		{
			_conditionsEditor.SetFilterCondition(gridFilterCondition);
		}

		internal void SetGridTable(GridTable gridTable)
		{
			_filterTable = gridTable;
			_conditionsEditor.SetTable(_filterTable);
		}

		private void InitializeControls()
		{
			ResourceManager resourceManager = new ResourceManager(typeof(MxFilterBuilderDialog));
			_conditionsEditor = new FilterConditionsEditor();
			_btnOk = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_conditionsEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_conditionsEditor.DockPadding.All = 1;
			_conditionsEditor.SetFilterCondition(null);
			_conditionsEditor.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			_conditionsEditor.Location = new Point(0, 2);
			_conditionsEditor.Name = "gridFilterConditionsEditor1";
			_conditionsEditor.Size = new Size(559, 293);
			_conditionsEditor.TabIndex = 0;
			_conditionsEditor.SetTable(null);
			_btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOk.DialogResult = DialogResult.OK;
			_btnOk.Location = new Point(388, 297);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(80, 24);
			_btnOk.TabIndex = 1;
			_btnOk.Text = "OK";
			_btnOk.Click += OnBtnOkClick;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(473, 297);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 2;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			base.AcceptButton = _btnOk;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(560, 326);
			base.Controls.AddRange(new Control[3] { _btnCancel, _btnOk, _conditionsEditor });
			base.DockPadding.Top = 4;
			base.Icon = (Icon)resourceManager.GetObject("$this.Icon");
			base.Name = "FilterBuilderDialog";
			Text = "FilterBuilderDialog";
			ResumeLayout(performLayout: false);
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnBtnOkClick(object sender, EventArgs eventArgs)
		{
			if (GetFilterCondition() != null)
			{
				GetFilterCondition().ConvertValuesToColumnType();
			}
			Close();
		}
	}
}
