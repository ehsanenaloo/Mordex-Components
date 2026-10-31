using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Mordex.Windows.DataGrid;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	public sealed class MoreInputsPage : DemoPage
	{
		private readonly DataTable _departments = new DataTable("Departments");
		private readonly DataTable _people;
		private readonly MxCheckedComboBox _checked = new MxCheckedComboBox();
		private readonly MxMultiColumnComboBox _multi = new MxMultiColumnComboBox();
		private readonly MxIntegerUpDown _quantity = new MxIntegerUpDown { Minimum = 0, Maximum = 500, Increment = 5, Value = 25 };
		private readonly MxValueListUpDown _priority = new MxValueListUpDown();
		private readonly MxMaskedTextBox _phone = new MxMaskedTextBox { Mask = "(000) 000-0000", Text = "5195550142" };
		private readonly MxMaskedTextBox _code = new MxMaskedTextBox { Mask = ">LL-0000", Text = "AB1234" };
		private readonly MxMaskedTextBox _digits = new MxMaskedTextBox { Numeric = true, Text = "2026" };
		private readonly MxNumericBox _amount = new MxNumericBox { ValueType = NumericEditValueType.Decimal, FormatMask = NumericEditFormatMask.Currency, Value = 99.95m };
		private readonly MxFontPicker _font = new MxFontPicker { Text = "Segoe UI" };
		private readonly MxColorButton _color = new MxColorButton { SelectedColor = Color.FromArgb(0, 120, 212) };
		private readonly MxColorPicker _palette = new MxColorPicker { SelectedColor = Color.FromArgb(214, 109, 0), ShowMoreColorsButton = false, ShowAutomaticButton = false };
		private readonly Label _checkedValue = PageKit.Value();
		private readonly Label _multiValue = PageKit.Value();
		private readonly Label _upDownValue = PageKit.Value();
		private readonly Label _maskValue = PageKit.Value();
		private readonly Control[] _editors;

		public MoreInputsPage()
		{
			_departments.Columns.Add("Id", typeof(int));
			_departments.Columns.Add("Name", typeof(string));
			for (int i = 0; i < DemoData.Departments.Length; i++)
			{
				_departments.Rows.Add(i + 1, DemoData.Departments[i]);
			}
			_checked.SetDataBinding(_departments, "");
			_checked.DropDownValueMember = "Id";
			_checked.DropDownDisplayMember = "Name";
			_checked.RetrieveStructure();
			_checked.DropDownList.Columns["Id"].Visible = false;
			_checked.CheckedValues = new object[] { 1, 3 };
			_checked.CheckedValuesChanged += (s, e) => ShowChecked();

			DataTable all = DemoData.Employees();
			_people = all.AsEnumerable().Take(60).CopyToDataTable();
			all.Dispose();
			_multi.DataSource = _people;
			_multi.ValueMember = "Id";
			_multi.DisplayMember = "Name";
			_multi.DropDownList.RetrieveStructure();
			foreach (GridColumn column in _multi.DropDownList.Columns.Cast<GridColumn>().ToArray())
			{
				column.Visible = column.Key == "Name" || column.Key == "Department" || column.Key == "Title";
			}
			_multi.Value = 3;
			_multi.ValueChanged += (s, e) => _multiValue.Text = "Selected id: " + (_multi.Value ?? "none");

			_priority.ValueList.Add(1, "Low");
			_priority.ValueList.Add(2, "Normal");
			_priority.ValueList.Add(3, "High");
			_priority.ValueList.Add(4, "Urgent");
			_priority.Value = 2;

			_quantity.ValueChanged += (s, e) => ShowUpDown();
			_priority.ValueChanged += (s, e) => ShowUpDown();
			_phone.TextChanged += (s, e) => ShowMask();
			_code.TextChanged += (s, e) => ShowMask();
			_digits.TextChanged += (s, e) => ShowMask();

			TableLayoutPanel grid = PageKit.Grid(2, 100f);
			TableLayoutPanel left = PageKit.Rows(150f, 110f, 150f);
			TableLayoutPanel right = PageKit.Rows(160f, 150f, 170f);

			MxGroupBox lists = PageParts.FormGroup("Lists", out TableLayoutPanel listForm);
			PageParts.AddField(listForm, "Departments", _checked);
			PageParts.AddField(listForm, "", _checkedValue);
			PageParts.AddField(listForm, "Employee", _multi);
			PageParts.AddField(listForm, "", _multiValue);
			lists.Controls.Add(listForm);

			MxGroupBox spinners = PageParts.FormGroup("Up and down", out TableLayoutPanel spinForm);
			PageParts.AddField(spinForm, "Quantity", _quantity);
			PageParts.AddField(spinForm, "Priority", _priority);
			PageParts.AddField(spinForm, "", _upDownValue);

			MxGroupBox masks = PageParts.FormGroup("Masked input", out TableLayoutPanel maskForm);
			PageParts.AddField(maskForm, "Phone", _phone);
			PageParts.AddField(maskForm, "Code", _code);
			PageParts.AddField(maskForm, "Digits only", _digits);
			PageParts.AddField(maskForm, "", _maskValue);

			MxGroupBox numbers = PageParts.FormGroup("Number and appearance", out TableLayoutPanel numberForm);
			PageParts.AddField(numberForm, "Amount", _amount);
			PageParts.AddField(numberForm, "Font", _font);
			PageParts.AddField(numberForm, "Color", _color);

			MxGroupBox palette = PageKit.Group("Color palette");
			Panel paletteHost = new Panel { Dock = DockStyle.Fill };
			_palette.Location = new Point(0, 4);
			paletteHost.Controls.Add(_palette);
			palette.Controls.Add(paletteHost);

			left.Controls.Add(lists, 0, 0);
			left.Controls.Add(spinners, 0, 1);
			left.Controls.Add(masks, 0, 2);
			right.Controls.Add(numbers, 0, 0);
			right.Controls.Add(palette, 0, 1);
			right.Controls.Add(PageParts.CodeGroup("Code", "checked.SetDataBinding(table, \"\");\nchecked.DropDownValueMember = \"Id\";\nchecked.DropDownDisplayMember = \"Name\";\nchecked.RetrieveStructure();\nchecked.CheckedValues = new object[] { 1, 3 };\n\nmulti.DataSource = employees;\nmulti.ValueMember = \"Id\";\nmulti.DisplayMember = \"Name\";\n\nphone.Mask = \"(000) 000-0000\";"), 0, 2);
			grid.Controls.Add(left, 0, 0);
			grid.Controls.Add(right, 1, 0);

			_editors = new Control[] { _checked, _multi, _quantity, _priority, _phone, _code, _digits, _amount, _font, _color, _palette };
			OptionsPanel options = new OptionsPanel("Options");
			options.AddToggle("Controls enabled", true, on =>
			{
				foreach (Control c in _editors)
				{
					c.Enabled = on;
				}
			});
			options.AddToggle("Include mask literals in the value", true, on => { _phone.IncludeLiterals = on; ShowMask(); });
			options.AddButton("Check all departments", _checked.CheckAll);
			options.AddButton("Uncheck all departments", _checked.UncheckAll);
			options.AddNote("Open the lists, use the arrow keys on the up and down boxes and type into the masks.");
			Compose(grid, options);
			ShowChecked();
			ShowUpDown();
			ShowMask();
			_multiValue.Text = "Selected id: " + (_multi.Value ?? "none");
		}

		private void ShowChecked()
		{
			object[] values = _checked.CheckedValues;
			_checkedValue.Text = values == null ? "Nothing checked" : values.Length + " checked: " + string.Join(", ", values);
		}

		private void ShowUpDown()
		{
			_upDownValue.Text = _quantity.Value + " units, priority " + (_priority.Value ?? "none");
		}

		private void ShowMask()
		{
			_maskValue.Text = "Phone digits: " + _phone.UnmaskedText + ", code: " + _code.Text;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_departments.Dispose();
				_people.Dispose();
			}
			base.Dispose(disposing);
		}

		public override string Title => "More inputs";

		public override string Group => "Inputs";

		public override string Summary => "Checked and multi-column lists, integer and value list up and down boxes, masked text boxes, a numeric box, and the font and color pickers.";

		public override int Order => 30;
	}
}
