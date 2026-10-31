using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	public sealed class ButtonsAndChoicesPage : DemoPage
	{
		private readonly Timer _timer = new Timer { Interval = 40 };
		private readonly MxProgressBar _progress = new MxProgressBar { Height = 22, Margin = new Padding(0, 0, 0, 6), ShowPercentage = true };
		private readonly MxProgressBar _slim = new MxProgressBar { Height = 12, Margin = new Padding(0, 0, 0, 6) };
		private readonly MxTrackBar _speed = new MxTrackBar { Minimum = 1, Maximum = 10, Value = 3, Width = 300, Height = 40, TickStyle = TickStyle.BottomRight };
		private readonly Label _speedValue = PageKit.Value("3");
		private readonly Label _volumeValue = PageKit.Value("40");
		private readonly Label _clicks = PageKit.Value("No clicks yet");
		private readonly Label _choice = PageKit.Value("Plan: Standard");
		private readonly Control[] _inputs;
		private int _clickCount;

		public ButtonsAndChoicesPage()
		{
			TableLayoutPanel grid = PageKit.Grid(6, 215f, 320f, 100f);

			MxGroupBox buttons = PageKit.Group("Buttons");
			TableLayoutPanel buttonColumn = PageKit.Column();
			FlowLayoutPanel buttonRow = PageKit.Stack(true);
			buttonRow.Dock = DockStyle.None;
			buttonRow.AutoSize = true;
			MxButton save = Button("Save", true);
			MxButton cancel = Button("Cancel", false);
			MxButton browse = Button("Browse...", false);
			browse.ButtonStyle = ButtonStyle.Ellipsis;
			MxButton off = Button("Disabled", false);
			off.Enabled = false;
			save.Click += (s, e) => _clicks.Text = "Save clicked " + (++_clickCount) + " times";
			cancel.Click += (s, e) => _clicks.Text = "Cancel clicked";
			buttonRow.Controls.AddRange(new Control[] { save, cancel, browse, off });
			buttonColumn.AddRow(PageKit.Caption("Default, standard, ellipsis and disabled"));
			buttonColumn.AddRow(buttonRow);
			buttonColumn.AddRow(PageKit.Caption("Last action"));
			buttonColumn.AddRow(_clicks);
			buttons.Controls.Add(buttonColumn);

			MxGroupBox checks = PageKit.Group("Check boxes");
			TableLayoutPanel checkColumn = PageKit.Column();
			MxCheckBox notify = Check("Send e-mail notifications", CheckState.Checked);
			MxCheckBox sync = Check("Synchronize on start", CheckState.Unchecked);
			MxCheckBox partial = Check("Install all components", CheckState.Indeterminate);
			partial.ThreeState = true;
			MxCheckBox locked = Check("Locked by the administrator", CheckState.Checked);
			locked.Enabled = false;
			foreach (MxCheckBox box in new[] { notify, sync, partial, locked })
			{
				checkColumn.AddRow(box);
			}
			checks.Controls.Add(checkColumn);

			MxGroupBox radios = PageKit.Group("Radio buttons");
			TableLayoutPanel radioColumn = PageKit.Column();
			MxRadioButton basic = Radio("Basic");
			MxRadioButton standard = Radio("Standard");
			MxRadioButton premium = Radio("Premium");
			MxRadioButton enterprise = Radio("Enterprise (contact sales)");
			enterprise.Enabled = false;
			standard.Checked = true;
			foreach (MxRadioButton radio in new[] { basic, standard, premium })
			{
				MxRadioButton captured = radio;
				radio.CheckedChanged += (s, e) =>
				{
					if (captured.Checked)
					{
						_choice.Text = "Plan: " + captured.Text;
					}
				};
			}
			foreach (MxRadioButton radio in new[] { basic, standard, premium, enterprise })
			{
				radioColumn.AddRow(radio);
			}
			radioColumn.AddRow(_choice);
			radios.Controls.Add(radioColumn);

			MxGroupBox sliders = PageKit.Group("Track bars");
			TableLayoutPanel sliderColumn = PageKit.Column();
			MxTrackBar volume = new MxTrackBar { Minimum = 0, Maximum = 100, Value = 40, LargeChange = 10, SmallChange = 1, Height = 40, TickStyle = TickStyle.BottomRight };
			volume.ValueChanged += (s, e) => _volumeValue.Text = volume.Value.ToString();
			MxTrackBar flat = new MxTrackBar { Minimum = 0, Maximum = 20, Value = 12, Height = 32, TickStyle = TickStyle.None };
			MxTrackBar locked2 = new MxTrackBar { Minimum = 0, Maximum = 20, Value = 6, Height = 32, TickStyle = TickStyle.None, Enabled = false };
			sliderColumn.AddRow(PageKit.Caption("Volume"));
			sliderColumn.AddRow(volume);
			sliderColumn.AddRow(_volumeValue);
			sliderColumn.AddRow(PageKit.Caption("Without ticks, and disabled"));
			sliderColumn.AddRow(flat);
			sliderColumn.AddRow(locked2);
			MxTrackBar vertical = new MxTrackBar { Orientation = Orientation.Vertical, Minimum = 0, Maximum = 10, Value = 4, Dock = DockStyle.Right, Width = 56 };
			sliders.Controls.Add(sliderColumn);
			sliders.Controls.Add(vertical);

			MxGroupBox progressGroup = PageKit.Group("Progress");
			TableLayoutPanel progressColumn = PageKit.Column();
			MxProgressBar upload = new MxProgressBar { Height = 22, Value = 65, Enabled = false, Margin = new Padding(0, 0, 0, 6) };
			_speed.ValueChanged += (s, e) => _speedValue.Text = _speed.Value.ToString();
			_speed.Width = 300;
			FlowLayoutPanel speedRow = PageKit.Stack(true);
			speedRow.Dock = DockStyle.None;
			speedRow.AutoSize = true;
			speedRow.Controls.AddRange(new Control[] { _speed, _speedValue });
			progressColumn.AddRow(PageKit.Caption("Download"));
			progressColumn.AddRow(_progress);
			progressColumn.AddRow(PageKit.Caption("Index rebuild"));
			progressColumn.AddRow(_slim);
			progressColumn.AddRow(PageKit.Caption("Upload (disabled)"));
			progressColumn.AddRow(upload);
			progressColumn.AddRow(PageKit.Caption("Animation speed"));
			progressColumn.AddRow(speedRow);
			progressGroup.Controls.Add(progressColumn);

			grid.Controls.Add(buttons, 0, 0);
			grid.SetColumnSpan(buttons, 2);
			grid.Controls.Add(checks, 2, 0);
			grid.SetColumnSpan(checks, 2);
			grid.Controls.Add(radios, 4, 0);
			grid.SetColumnSpan(radios, 2);
			grid.Controls.Add(sliders, 0, 1);
			grid.SetColumnSpan(sliders, 3);
			grid.Controls.Add(progressGroup, 3, 1);
			grid.SetColumnSpan(progressGroup, 3);

			_inputs = new Control[] { save, cancel, browse, notify, sync, partial, basic, standard, premium, volume, flat, vertical, _speed };

			OptionsPanel options = new OptionsPanel("Options");
			options.AddToggle("Controls enabled", true, on =>
			{
				foreach (Control c in _inputs)
				{
					c.Enabled = on;
				}
			});
			options.AddToggle("Run the progress animation", true, on => _timer.Enabled = on);
			options.AddToggle("Show percentage", true, on => _progress.ShowPercentage = on);
			options.AddButton("Restart progress", () => _progress.Value = 0);
			options.AddNote("The speed slider sets how fast the bars move.");

			Compose(grid, options);

			_timer.Tick += (s, e) =>
			{
				int next = _progress.Value + _speed.Value;
				_progress.Value = next > _progress.Maximum ? 0 : next;
				_slim.Value = _progress.Maximum - _progress.Value;
			};
		}

		public override string Title => "Buttons and choices";

		public override string Group => "Inputs";

		public override string Summary => "Buttons, check boxes, radio buttons, track bars and progress bars, with enabled and disabled states and a live progress animation.";

		public override int Order => 10;

		private static MxButton Button(string text, bool primary)
		{
			return new MxButton { Text = text, Size = new Size(96, 30), Margin = new Padding(0, 4, 8, 4), ButtonStyle = primary ? ButtonStyle.Default : ButtonStyle.Button };
		}

		private static MxCheckBox Check(string text, CheckState state)
		{
			return new MxCheckBox { Text = text, CheckState = state, AutoSize = false, Height = 24, Margin = new Padding(0, 2, 0, 2) };
		}

		private static MxRadioButton Radio(string text)
		{
			return new MxRadioButton { Text = text, AutoSize = false, Height = 24, Margin = new Padding(0, 2, 0, 2) };
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer.Stop();
				_timer.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
