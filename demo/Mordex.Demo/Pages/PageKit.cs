using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	/// <summary>Small builders that keep the spacing of the demo pages the same.</summary>
	internal static class PageKit
	{
		public const int Gap = 10;

		public static MxGroupBox Group(string title)
		{
			return new MxGroupBox { Text = title, Dock = DockStyle.Fill, Margin = new Padding(0, 0, Gap, Gap), Padding = new Padding(12, 6, 12, 10) };
		}

		public static FlowLayoutPanel Stack(bool horizontal = false)
		{
			return new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				FlowDirection = horizontal ? FlowDirection.LeftToRight : FlowDirection.TopDown,
				WrapContents = horizontal,
				BackColor = Color.Transparent
			};
		}

		/// <summary>One column whose rows have the height of their control and the width of the column.</summary>
		public static TableLayoutPanel Column()
		{
			TableLayoutPanel column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1, BackColor = Color.Transparent };
			column.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			column.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			return column;
		}

		/// <summary>One column of fixed-height rows followed by free space.</summary>
		public static TableLayoutPanel Rows(params float[] heights)
		{
			TableLayoutPanel column = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = heights.Length + 1, BackColor = Color.Transparent };
			column.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			foreach (float height in heights)
			{
				column.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
			}
			column.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			return column;
		}

		public static T AddRow<T>(this TableLayoutPanel column, T control) where T : Control
		{
			control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			int row = column.RowCount - 1;
			column.RowStyles.Insert(row, new RowStyle(SizeType.AutoSize));
			column.RowCount++;
			column.Controls.Add(control, 0, row);
			return control;
		}

		public static Label Caption(string text, bool muted = true)
		{
			return new Label
			{
				Text = text,
				AutoSize = true,
				Margin = new Padding(0, 6, 0, 3),
				ForeColor = muted ? SystemColors.GrayText : SystemColors.ControlText
			};
		}

		/// <summary>A large title label. It releases its font when it is disposed.</summary>
		public static Label Heading(string text, float size = 16f)
		{
			Label label = new Label { Text = text, Dock = DockStyle.Top, Height = (int)(size * 2.4f), Padding = new Padding(0, 6, 0, 0) };
			Font font = new Font(SystemFonts.MessageBoxFont.FontFamily, size);
			label.Font = font;
			label.Disposed += (sender, e) => font.Dispose();
			return label;
		}

		public static Label Value(string text = "")
		{
			Label label = new Label { Text = text, AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
			Font font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
			label.Font = font;
			label.Disposed += (sender, e) => font.Dispose();
			return label;
		}

		/// <summary>A grid of equal columns. Each row is a percentage, or a fixed height when the value is above 100.</summary>
		public static TableLayoutPanel Grid(int columns, params float[] rows)
		{
			TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rows.Length, Padding = new Padding(Gap + 6, Gap + 6, Gap + 6 - Gap, Gap + 6 - Gap) };
			for (int c = 0; c < columns; c++)
			{
				grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
			}
			foreach (float row in rows)
			{
				grid.RowStyles.Add(row > 100f ? new RowStyle(SizeType.Absolute, row) : new RowStyle(SizeType.Percent, row));
			}
			return grid;
		}
	}
}
