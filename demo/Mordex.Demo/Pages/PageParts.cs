using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	/// <summary>Builders shared by the newer pages: form rows, code samples and result boxes.</summary>
	internal static class PageParts
	{
		/// <summary>A read-only box with a monospaced font for a code sample or a result text.</summary>
		public static TextBox CodeBox(string text)
		{
			TextBox box = new TextBox
			{
				Multiline = true,
				ReadOnly = true,
				ScrollBars = System.Windows.Forms.ScrollBars.Both,
				WordWrap = false,
				Dock = DockStyle.Fill,
				BorderStyle = System.Windows.Forms.BorderStyle.None,
				Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n")
			};
			Font font = new Font("Consolas", 9f);
			box.Font = font;
			box.Disposed += (sender, e) => font.Dispose();
			return box;
		}

		public static MxGroupBox CodeGroup(string title, string code)
		{
			MxGroupBox group = PageKit.Group(title);
			group.Controls.Add(CodeBox(code));
			return group;
		}

		/// <summary>A two column form: a caption column and an editor column.</summary>
		public static TableLayoutPanel Form(int captionWidth = 120)
		{
			TableLayoutPanel form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
			form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, captionWidth));
			form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			form.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			return form;
		}

		public static void AddField(TableLayoutPanel form, string caption, Control editor)
		{
			int row = form.RowCount - 1;
			form.RowStyles.Insert(row, new RowStyle(SizeType.AutoSize));
			form.RowCount++;
			Label label = new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 8), ForeColor = SystemColors.GrayText };
			editor.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			editor.Margin = new Padding(0, 4, 0, 4);
			form.Controls.Add(label, 0, row);
			form.Controls.Add(editor, 1, row);
		}

		/// <summary>A group box that holds one form.</summary>
		public static MxGroupBox FormGroup(string title, out TableLayoutPanel form, int captionWidth = 120)
		{
			MxGroupBox group = PageKit.Group(title);
			form = Form(captionWidth);
			group.Controls.Add(form);
			return group;
		}
	}
}
