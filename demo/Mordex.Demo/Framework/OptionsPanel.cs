using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Demo
{
	/// <summary>
	/// A narrow panel at the side of a demo page. It holds the switches and choices that change the demo control, so a
	/// visitor can try a feature without reading code.
	/// </summary>
	public sealed class OptionsPanel : Panel
	{
		private readonly FlowLayoutPanel _flow = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoScroll = true,
			Padding = new Padding(12, 8, 12, 8)
		};

		public OptionsPanel(string title = "Options")
		{
			Width = 250;
			Padding = new Padding(0);
			BackColor = SystemColors.Control;
			Label heading = new Label { Text = title, Dock = DockStyle.Top, Height = 34, Padding = new Padding(12, 10, 0, 0), Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold) };
			Panel line = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = SystemColors.ControlDark };
			Controls.Add(_flow);
			Controls.Add(heading);
			Controls.Add(line);
		}

		private int InnerWidth => Width - 12 * 2 - SystemInformation.VerticalScrollBarWidth - 2;

		public CheckBox AddToggle(string text, bool initial, Action<bool> apply)
		{
			CheckBox box = new CheckBox { Text = text, Checked = initial, AutoSize = false, Width = InnerWidth, Height = 24, Margin = new Padding(0, 2, 0, 2) };
			box.CheckedChanged += (sender, e) => apply(box.Checked);
			_flow.Controls.Add(box);
			apply(initial);
			return box;
		}

		public ComboBox AddChoice<T>(string caption, T[] values, T initial, Action<T> apply)
		{
			AddCaption(caption);
			ComboBox combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = InnerWidth, Margin = new Padding(0, 0, 0, 6) };
			foreach (T value in values)
			{
				combo.Items.Add(value);
			}
			combo.SelectedItem = initial;
			combo.SelectedIndexChanged += (sender, e) =>
			{
				if (combo.SelectedItem is T selected)
				{
					apply(selected);
				}
			};
			_flow.Controls.Add(combo);
			if (combo.SelectedItem is T first)
			{
				apply(first);
			}
			return combo;
		}

		public Button AddButton(string text, Action click)
		{
			Button button = new Button { Text = text, Width = InnerWidth, Height = 28, Margin = new Padding(0, 6, 0, 2) };
			button.Click += (sender, e) => click();
			_flow.Controls.Add(button);
			return button;
		}

		public Label AddCaption(string text)
		{
			Label label = new Label { Text = text, AutoSize = false, Width = InnerWidth, Height = 20, Margin = new Padding(0, 8, 0, 0), ForeColor = SystemColors.GrayText };
			_flow.Controls.Add(label);
			return label;
		}

		public Label AddNote(string text)
		{
			Label label = new Label { Text = text, AutoSize = false, Width = InnerWidth, Height = 64, Margin = new Padding(0, 8, 0, 0), ForeColor = SystemColors.GrayText };
			_flow.Controls.Add(label);
			return label;
		}
	}
}
