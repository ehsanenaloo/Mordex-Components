using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	public sealed class PickersPage : DemoPage
	{
		private readonly MxTextBox _name = new MxTextBox { Text = "Quarterly report" };
		private readonly MxTextBox _notes = new MxTextBox { Multiline = true, Text = "Review the figures before the Friday meeting.\r\nSend the summary to the finance team.", Height = 66 };
		private readonly MxComboBox _language = new MxComboBox { ComboStyle = Mordex.Windows.UI.Controls.ComboStyle.DropDownList };
		private readonly MxNumericBox _pageSize = new MxNumericBox { ValueType = NumericEditValueType.Int32, Value = 25 };
		private readonly MxNumericBox _price = new MxNumericBox { ValueType = NumericEditValueType.Decimal, FormatMask = NumericEditFormatMask.Currency, Value = 1249.5m };
		private readonly MxNumericBox _discount = new MxNumericBox { ValueType = NumericEditValueType.Double, FormatMask = NumericEditFormatMask.Percent, Value = 0.15 };
		private readonly MxTextBox _user = new MxTextBox { Text = "demo.user" };
		private readonly MxTextBox _password = new MxTextBox { PasswordChar = '*', Text = "secret-value" };
		private readonly MxComboBox _country = new MxComboBox { AutoComplete = true, Text = "Canada" };
		private readonly MxFontPicker _font = new MxFontPicker();
		private readonly MxColorButton _textColor = new MxColorButton { SelectedColor = Color.FromArgb(31, 78, 121) };
		private readonly MxColorButton _backColor = new MxColorButton { SelectedColor = Color.FromArgb(255, 244, 214) };
		private readonly MxColorPicker _palette = new MxColorPicker { SelectedColor = Color.FromArgb(0, 120, 212), ShowMoreColorsButton = false, ShowAutomaticButton = false };
		private readonly Label _preview = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Text = "The quick brown fox jumps over the lazy dog", BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle };
		private readonly Label _summary = PageKit.Value();
		private readonly Control[] _editors;
		private Font _previewFont;

		public PickersPage()
		{
			foreach (string language in new[] { "English", "French", "German", "Spanish", "Portuguese", "Italian", "Dutch" })
			{
				_language.Items.Add(language, language);
			}
			_language.SelectedIndex = 0;
			foreach (string country in new[] { "Canada", "Brazil", "France", "Germany", "India", "Japan", "Mexico", "Spain", "United Kingdom", "United States" })
			{
				_country.Items.Add(country, country);
			}
			_font.Text = "Segoe UI";
			_palette.ColorItemSize = new Size(32, 32);

			TableLayoutPanel grid = PageKit.Grid(2, 100f);
			TableLayoutPanel left = PageKit.Rows(250f, 130f, 150f);
			TableLayoutPanel right = PageKit.Rows(150f, 120f, 280f);

			MxGroupBox general = PageKit.Group("General");
			TableLayoutPanel generalForm = Form();
			AddField(generalForm, "Name", _name);
			AddField(generalForm, "Notes", _notes);
			AddField(generalForm, "Language", _language);
			AddField(generalForm, "Rows per page", _pageSize);
			general.Controls.Add(generalForm);

			MxGroupBox pricing = PageKit.Group("Numbers");
			TableLayoutPanel pricingForm = Form();
			AddField(pricingForm, "Price", _price);
			AddField(pricingForm, "Discount", _discount);
			AddField(pricingForm, "Total", _summary);
			pricing.Controls.Add(pricingForm);

			MxGroupBox account = PageKit.Group("Account");
			TableLayoutPanel accountForm = Form();
			AddField(accountForm, "User name", _user);
			AddField(accountForm, "Password", _password);
			AddField(accountForm, "Country", _country);
			account.Controls.Add(accountForm);

			MxGroupBox appearance = PageKit.Group("Appearance");
			TableLayoutPanel appearanceForm = Form();
			AddField(appearanceForm, "Font", _font);
			AddField(appearanceForm, "Text color", _textColor);
			AddField(appearanceForm, "Background", _backColor);
			appearance.Controls.Add(appearanceForm);

			MxGroupBox preview = PageKit.Group("Preview");
			Panel previewHost = new Panel { Dock = DockStyle.Fill };
			previewHost.Controls.Add(_preview);
			preview.Controls.Add(previewHost);

			MxGroupBox palette = PageKit.Group("Palette");
			Panel paletteHost = new Panel { Dock = DockStyle.Fill };
			_palette.Location = new Point(0, 4);
			paletteHost.Controls.Add(_palette);
			palette.Controls.Add(paletteHost);

			left.Controls.Add(general, 0, 0);
			left.Controls.Add(pricing, 0, 1);
			left.Controls.Add(account, 0, 2);
			right.Controls.Add(appearance, 0, 0);
			right.Controls.Add(preview, 0, 1);
			right.Controls.Add(palette, 0, 2);
			grid.Controls.Add(left, 0, 0);
			grid.Controls.Add(right, 1, 0);

			_editors = new Control[] { _name, _notes, _language, _pageSize, _price, _discount, _user, _password, _country, _font, _textColor, _backColor, _palette };

			OptionsPanel options = new OptionsPanel("Options");
			options.AddToggle("Controls enabled", true, on =>
			{
				foreach (Control c in _editors)
				{
					c.Enabled = on;
				}
			});
			options.AddToggle("Text boxes read-only", false, on =>
			{
				_name.ReadOnly = on;
				_notes.ReadOnly = on;
				_user.ReadOnly = on;
			});
			options.AddToggle("Show fonts in their own style", true, on => _font.UseFontInFontNames = on);
			options.AddToggle("Bold preview", false, on => UpdatePreview(on));
			options.AddNote("Pick a font or a color to change the preview. The palette sets the text color too.");

			Compose(grid, options);

			_font.SelectedValueChanged += (s, e) =>
			{
				_fontChosen |= !_settingFont;
				UpdatePreview(_boldPreview);
			};
			_textColor.SelectedColorChanged += (s, e) => UpdatePreview(_boldPreview);
			_backColor.SelectedColorChanged += (s, e) => UpdatePreview(_boldPreview);
			_palette.SelectedColorChanged += (s, e) =>
			{
				_textColor.SelectedColor = _palette.SelectedColor;
			};
			_price.ValueChanged += (s, e) => UpdateTotal();
			_discount.ValueChanged += (s, e) => UpdateTotal();
			UpdateTotal();
			UpdatePreview(false);
		}

		private bool _boldPreview;

		public override string Title => "Pickers and editors";

		public override string Group => "Inputs";

		public override string Summary => "A settings form built from combo boxes, color and font pickers, text boxes and numeric boxes. The preview follows your font and color choices.";

		public override int Order => 20;

		private static TableLayoutPanel Form()
		{
			TableLayoutPanel form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
			form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
			form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			form.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			return form;
		}

		private static void AddField(TableLayoutPanel form, string caption, Control editor)
		{
			int row = form.RowCount - 1;
			form.RowStyles.Insert(row, new RowStyle(SizeType.AutoSize));
			form.RowCount++;
			Label label = new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 8), ForeColor = SystemColors.GrayText };
			if (editor is MxTextBox box && box.Multiline)
			{
				label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			}
			editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			editor.Margin = new Padding(0, 4, 0, 4);
			form.Controls.Add(label, 0, row);
			form.Controls.Add(editor, 1, row);
		}

		private void UpdateTotal()
		{
			decimal price = Convert.ToDecimal(_price.Value ?? 0m);
			decimal discount = Convert.ToDecimal(_discount.Value ?? 0.0);
			_summary.Text = (price * (1 - discount)).ToString("C2");
		}

		public override void OnLookChanged()
		{
			base.OnLookChanged();
			// The font picker starts at the font of the page, which follows the theme.
			if (_font.Text != Font.Name && !_fontChosen)
			{
				_settingFont = true;
				_font.Text = Font.Name;
				_settingFont = false;
				UpdatePreview(_boldPreview);
			}
		}

		private bool _fontChosen;

		private bool _settingFont;

		private void UpdatePreview(bool bold)
		{
			_boldPreview = bold;
			FontFamily family = FindFamily(_font.Text);
			Font next = family == null ? new Font(Font, bold ? FontStyle.Bold : FontStyle.Regular) : new Font(family, 18f, bold && family.IsStyleAvailable(FontStyle.Bold) ? FontStyle.Bold : FontStyle.Regular);
			_preview.Font = next;
			_previewFont?.Dispose();
			_previewFont = next;
			_preview.ForeColor = _textColor.SelectedColor;
			_preview.BackColor = _backColor.SelectedColor;
		}

		private static FontFamily FindFamily(string name)
		{
			foreach (FontFamily family in FontFamily.Families)
			{
				if (string.Equals(family.Name, name, StringComparison.OrdinalIgnoreCase) && family.IsStyleAvailable(FontStyle.Regular))
				{
					return family;
				}
			}
			return null;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_previewFont?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
