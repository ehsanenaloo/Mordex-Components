using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	/// <summary>Shows every registered theme: pick one to make it the default theme and see its colors and a themed button.</summary>
	public sealed class ThemeGalleryPage : DemoPage, IThemeListener
	{
		private readonly ListBox _list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, BorderStyle = BorderStyle.None };
		private readonly SwatchPanel _swatches = new SwatchPanel { Dock = DockStyle.Fill };
		private readonly Panel _buttons = new Panel { Dock = DockStyle.Top, Height = 104 };
		private bool _updating;

		public ThemeGalleryPage()
		{
			foreach (MxTheme theme in MxThemeManager.RegisteredThemes)
			{
				_list.Items.Add(theme);
			}
			_list.SelectedItem = MxThemeManager.DefaultTheme;
			_list.SelectedIndexChanged += (sender, e) =>
			{
				if (!_updating && _list.SelectedItem is MxTheme theme)
				{
					MxThemeManager.DefaultTheme = theme;
					Form form = FindForm();
					if (form != null)
					{
						ThemeWindow.Apply(form, theme);
					}
				}
			};
			AddButton("Save", false, 16);
			AddButton("Open", true, 128);
			AddButton("Disabled", false, 240).Enabled = false;
			_buttons.Controls.Add(new MxCheckBox { Text = "Check box", Checked = true, Location = new Point(360, 8), Size = new Size(110, 24) });
			_buttons.Controls.Add(new MxCheckBox { Text = "Off", Location = new Point(360, 34), Size = new Size(110, 24) });
			_buttons.Controls.Add(new MxRadioButton { Text = "Radio", Checked = true, Location = new Point(480, 8), Size = new Size(100, 24) });
			_buttons.Controls.Add(new MxRadioButton { Text = "Other", Location = new Point(480, 34), Size = new Size(100, 24) });
			_buttons.Controls.Add(new MxProgressBar { Value = 60, Location = new Point(16, 64), Size = new Size(210, 20) });
			_buttons.Controls.Add(new MxTrackBar { Value = 4, Location = new Point(240, 58), Size = new Size(220, 40) });
			Panel left = new Panel { Dock = DockStyle.Left, Width = 230 };
			left.Controls.Add(_list);
			Panel centre = new Panel { Dock = DockStyle.Fill };
			centre.Controls.Add(_swatches);
			centre.Controls.Add(_buttons);
			OptionsPanel options = new OptionsPanel("Theme");
			options.AddToggle("Follow the Windows light or dark setting", false, on => MxThemeManager.FollowWindows = on);
			options.AddNote("Faces, edges and colors come from the theme. Older looks use a face gradient or a 3D edge.");
			Controls.Add(centre);
			Controls.Add(left);
			Controls.Add(options);
			options.Dock = DockStyle.Right;
			ApplyBackground();
			MxThemeManager.AddListener(this);
		}

		public override string Title => "Theme gallery";

		public override string Group => "Themes";

		public override string Summary => "Every theme of the suite with all its colors. Pick one to make it the default theme.";

		// The page holds ordinary panels, so the theme color of the surface is given to the panel behind the themed controls.
		private void ApplyBackground()
		{
			_buttons.BackColor = MxThemeManager.DefaultTheme.GetColor(ThemeRole.Surface);
		}

		public override void OnShown()
		{
			ApplyBackground();
			_updating = true;
			_list.SelectedItem = MxThemeManager.DefaultTheme;
			_updating = false;
		}

		private MxButton AddButton(string text, bool isDefault, int x)
		{
			MxButton button = new MxButton { Text = text, Location = new Point(x, 14), Size = new Size(100, 34) };
			button.NotifyDefault(isDefault);
			_buttons.Controls.Add(button);
			return button;
		}

		void IThemeListener.OnThemeChanged()
		{
			if (IsHandleCreated && !IsDisposed)
			{
				BeginInvoke(new Action(() =>
				{
					_updating = true;
					_list.SelectedItem = MxThemeManager.DefaultTheme;
					_updating = false;
					ApplyBackground();
					_swatches.Invalidate();
					Invalidate(true);
				}));
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MxThemeManager.RemoveListener(this);
			}
			base.Dispose(disposing);
		}

		private sealed class SwatchPanel : Control
		{
			public SwatchPanel()
			{
				DoubleBuffered = true;
				ResizeRedraw = true;
				SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
				Graphics g = e.Graphics;
				g.FillRectangle(theme.Brush(ThemeRole.Surface), ClientRectangle);
				Array roles = Enum.GetValues(typeof(ThemeRole));
				int tileW = theme.Scaled(150);
				int tileH = theme.Scaled(46);
				int gap = theme.Scaled(8);
				int columns = Math.Max(1, (Width - gap) / (tileW + gap));
				for (int i = 0; i < roles.Length; i++)
				{
					ThemeRole role = (ThemeRole)roles.GetValue(i);
					int x = gap + (i % columns) * (tileW + gap);
					int y = gap + (i / columns) * (tileH + gap);
					Rectangle tile = new Rectangle(x, y, tileW, tileH);
					Color color = theme.Color(role);
					theme.FillRounded(g, tile, role, theme.Shape.CornerRadius);
					theme.DrawRounded(g, tile, ThemeRole.Border, theme.Shape.CornerRadius);
					Color ink = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0 > 0.55 ? Color.Black : Color.White;
					Rectangle text = Rectangle.Inflate(tile, -theme.Scaled(6), -theme.Scaled(4));
					TextRenderer.DrawText(g, role.ToString(), Font, text, ink, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding);
					TextRenderer.DrawText(g, "#" + (color.ToArgb() & 0xFFFFFF).ToString("X6"), Font, text, ink, TextFormatFlags.Left | TextFormatFlags.Bottom | TextFormatFlags.NoPadding);
				}
			}
		}
	}
}
