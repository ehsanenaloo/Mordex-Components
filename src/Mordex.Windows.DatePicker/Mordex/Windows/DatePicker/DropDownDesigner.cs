using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public class DropDownDesigner : Form
	{
		private Container _container = new Container();

		private PropertyGrid _propertyGrid;

		private Button _btnOk;

		private DatePickerCalendar _calendar;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public DatePickerCalendar Calendar
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
					_calendar.Visible = true;
					base.Controls.Add(_calendar);
					_calendar.ComputeDroppedControlSize(1, 1);
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
			_propertyGrid.LineColor = SystemColors.ScrollBar;
			_propertyGrid.Location = new Point(300, 8);
			_propertyGrid.Name = "propertyGrid1";
			_propertyGrid.Size = new Size(264, 284);
			_propertyGrid.TabIndex = 0;
			_btnOk.Location = new Point(480, 308);
			_btnOk.Name = "btnOk";
			_btnOk.Size = new Size(80, 20);
			_btnOk.TabIndex = 1;
			_btnOk.Text = "OK";
			_btnOk.Click += OnBtnOkClick;
			base.ClientSize = new Size(574, 336);
			base.Controls.Add(_btnOk);
			base.Controls.Add(_propertyGrid);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "DropDownDesigner";
			Text = "DropDownDesigner";
			ResumeLayout(performLayout: false);
		}

		private void OnBtnOkClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
