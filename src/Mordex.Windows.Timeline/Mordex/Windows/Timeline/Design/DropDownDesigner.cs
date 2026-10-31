using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design
{
	public class DropDownDesigner : Form
	{
		private Container _container = new Container();

		private PropertyGrid _propertyGrid;

		private Button _btnOk;

		private TimelineSurface _calendar;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public TimelineSurface Calendar
		{
			get
			{
				return _calendar;
			}
			set
			{
				_calendar = value;
				if (_calendar != null)
				{
					_calendar.Location = new Point(15, 35);
					base.Controls.Add(_calendar);
					_propertyGrid.PropertySort = PropertySort.Alphabetical;
					_propertyGrid.SelectedObject = _calendar;
				}
			}
		}

		public DropDownDesigner()
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

		private void InitializeControls()
		{
			_propertyGrid = new PropertyGrid();
			_btnOk = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_propertyGrid.CommandsVisibleIfAvailable = true;
			_propertyGrid.LargeButtons = false;
			_propertyGrid.LineColor = SystemColors.ScrollBar;
			_propertyGrid.Location = new Point(300, 8);
			_propertyGrid.Name = "propertyGrid1";
			_propertyGrid.Size = new Size(264, 284);
			_propertyGrid.TabIndex = 0;
			_propertyGrid.Text = "propertyGrid1";
			_propertyGrid.ViewBackColor = SystemColors.Window;
			_propertyGrid.ViewForeColor = SystemColors.WindowText;
			_btnOk.Location = new Point(480, 308);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(80, 20);
			_btnOk.TabIndex = 1;
			_btnOk.Text = "OK";
			_btnOk.Click += OnBtnOkClick;
			base.ClientSize = new Size(574, 336);
			base.Controls.AddRange(new Control[2] { _btnOk, _propertyGrid });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "DropDownDesigner";
			Text = "DropDownDesigner";
			base.FormClosed += OnControlClosed;
			ResumeLayout(performLayout: false);
		}

		private void OnControlClosed(object sender, EventArgs eventArgs)
		{
			if (base.Controls.Contains(_calendar))
			{
				base.Controls.Remove(_calendar);
			}
		}

		private void OnBtnOkClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
