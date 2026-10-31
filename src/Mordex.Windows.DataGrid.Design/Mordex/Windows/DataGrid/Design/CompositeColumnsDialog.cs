using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	internal class CompositeColumnsDialog : Form
	{
		private IContainer components = default;

		private Button btnOk;

		private Button btnCancel;

		private CompositeColumnsChooser _columnsChooser;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal GridTable TargetTable
		{
			get
			{
				return _columnsChooser.TargetTable;
			}
			set
			{
				_columnsChooser.TargetTable = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public GridColumn[] Columns
		{
			get
			{
				return _columnsChooser.Columns;
			}
			set
			{
				_columnsChooser.Columns = value;
			}
		}

		internal CompositeColumnsDialog()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this._columnsChooser = new Mordex.Windows.DataGrid.Design.CompositeColumnsChooser();
			base.SuspendLayout();
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(174, 262);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(80, 28);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(260, 261);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 28);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this._columnsChooser.Columns = new Mordex.Windows.DataGrid.GridColumn[0];
			this._columnsChooser.Location = new System.Drawing.Point(2, 34);
			this._columnsChooser.Name = "compositeColumnsChooser1";
			this._columnsChooser.Size = new System.Drawing.Size(511, 221);
			this._columnsChooser.TabIndex = 3;
			base.AcceptButton = this.btnOk;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new System.Drawing.Size(514, 299);
			base.Controls.Add(this._columnsChooser);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnOk);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Name = "CompositeColumnsDialog";
			this.Text = "Composite Columns";
			base.ResumeLayout(false);
		}
	}
}
