using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Docking;

namespace Mordex.Demo
{
	public sealed class FloatingAndScrollPage : DemoPage
	{
		private readonly MxToolBar _toolBar = new MxToolBar { Dock = DockStyle.Top };
		private readonly MxScrollPanel _scroll = new MxScrollPanel { Dock = DockStyle.Fill };
		private readonly MxDockManager _manager = new MxDockManager();
		private readonly MxDockPanel _notes = new MxDockPanel("Notes", PanelDockStyle.Right);
		private readonly Panel _dockHost = new Panel { Dock = DockStyle.Fill };
		private readonly ListBox _list = new ListBox { Dock = DockStyle.Fill, BorderStyle = System.Windows.Forms.BorderStyle.None, IntegralHeight = false };
		private readonly Label _clicks = PageKit.Value("No tool bar button pressed yet");
		private readonly Label _state = PageKit.Value("The Notes panel is docked");
		private readonly Panel _tall = new Panel { Size = new Size(380, 900) };
		private readonly System.Collections.Generic.List<Image> _images = new System.Collections.Generic.List<Image>();

		public FloatingAndScrollPage()
		{
			string[] names = { "New", "Open", "Save", "Print" };
			int[] glyphs = { DemoIcons.Add, DemoIcons.Folder, DemoIcons.Save, DemoIcons.Print };
			for (int i = 0; i < names.Length; i++)
			{
				Image image = DemoIcons.Glyph(glyphs[i], SystemColors.ControlText, 16);
				_images.Add(image);
				_toolBar.Buttons.Add(new MxToolBarButton(names[i]) { Image = image, ToolTipText = names[i] + " the sample document" });
			}
			_toolBar.Buttons.Add(new MxToolBarButton("Bold") { Style = MxToolBarButtonStyle.ToggleButton, ToolTipText = "A toggle button keeps its pressed state" });
			_toolBar.ButtonClick += (s, e) => _clicks.Text = "Pressed: " + e.Button.Text + (e.Button.Style == MxToolBarButtonStyle.ToggleButton ? (e.Button.Pushed ? " (on)" : " (off)") : "");

			for (int i = 0; i < 24; i++)
			{
				_tall.Controls.Add(new Label { Text = "Line " + (i + 1) + ": the scroll panel moves this content with its buttons.", AutoSize = false, Size = new Size(360, 24), Location = new Point(10, 10 + i * 36), BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle });
			}
			_scroll.ChildControl = _tall;

			for (int i = 1; i <= 8; i++)
			{
				_list.Items.Add("Note " + i);
			}
			_notes.InnerContainer.Controls.Add(_list);
			_notes.Size = new Size(200, 200);
			_manager.ContainerControl = _dockHost;
			_manager.Panels.Add(_notes);
			_manager.PanelDockStateChanged += (s, e) =>
			{
				MxFloatingWindow window = e.Panel.FloatingWindow;
				_state.Text = window == null ? "The Notes panel is " + e.Panel.DockState.ToString().ToLowerInvariant() : "The Notes panel floats in a window at " + window.Location.X + ", " + window.Location.Y;
			};

			TableLayoutPanel grid = PageKit.Grid(2, 100f);
			MxGroupBox toolGroup = PageKit.Group("Tool bar and scroll panel");
			Panel toolHost = new Panel { Dock = DockStyle.Fill };
			toolHost.Controls.Add(_scroll);
			toolHost.Controls.Add(_clicks);
			_clicks.Dock = DockStyle.Bottom;
			toolHost.Controls.Add(_toolBar);
			_scroll.BringToFront();
			toolGroup.Controls.Add(toolHost);

			MxGroupBox dockGroup = PageKit.Group("Docked and floating panel");
			Panel dockWrap = new Panel { Dock = DockStyle.Fill };
			_state.Dock = DockStyle.Bottom;
			dockWrap.Controls.Add(_dockHost);
			dockWrap.Controls.Add(_state);
			dockGroup.Controls.Add(dockWrap);

			grid.Controls.Add(toolGroup, 0, 0);
			grid.Controls.Add(dockGroup, 1, 0);

			OptionsPanel options = new OptionsPanel();
			options.AddCaption("Tool bar");
			options.AddChoice("Appearance", new[] { MxToolBarAppearance.Flat, MxToolBarAppearance.Normal }, MxToolBarAppearance.Flat, value => _toolBar.Appearance = value);
			options.AddChoice("Text", new[] { MxToolBarTextAlign.Right, MxToolBarTextAlign.Underneath }, MxToolBarTextAlign.Right, value => _toolBar.TextAlign = value);
			options.AddToggle("Show tool tips", true, on => _toolBar.ShowToolTips = on);
			options.AddCaption("Scroll panel");
			options.AddChoice("Scroll mode", new[] { ScrollMode.Vertical, ScrollMode.Horizontal, ScrollMode.Both }, ScrollMode.Vertical, value => _scroll.ScrollMode = value);
			options.AddCaption("Floating window");
			options.AddButton("Float the Notes panel", () =>
			{
				if (_notes.DockState == PanelDockState.Docked)
				{
					_notes.UndockPanel();
				}
			});
			options.AddButton("Dock the Notes panel", () =>
			{
				if (_notes.DockState != PanelDockState.Docked)
				{
					_notes.DockPanel();
				}
			});
			options.AddNote("A floating window has its own frame and caption. Drag its caption back onto the page to dock it.");
			Compose(grid, options);
			ApplyLook(false);
		}

		public override void ApplyLook(bool themed)
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			_toolBar.RenderMode = ToolStripRenderMode.Professional;
			_toolBar.Renderer = themed ? new ToolStripProfessionalRenderer(new ThemeColorTable(theme)) : new ToolStripProfessionalRenderer();
			_toolBar.BackColor = themed ? theme.GetColor(ThemeRole.ToolbarBack) : SystemColors.Control;
			_toolBar.ForeColor = themed ? theme.GetColor(ThemeRole.Text) : SystemColors.ControlText;
			Color glyph = _toolBar.ForeColor;
			int[] codes = { DemoIcons.Add, DemoIcons.Folder, DemoIcons.Save, DemoIcons.Print };
			for (int i = 0; i < codes.Length && i < _toolBar.Buttons.Count; i++)
			{
				Image old = _toolBar.Buttons[i].Image;
				Image image = DemoIcons.Glyph(codes[i], glyph, 16);
				_toolBar.Buttons[i].Image = image;
				_images.Add(image);
				if (old != null)
				{
					_images.Remove(old);
					old.Dispose();
				}
			}
			_list.BackColor = themed ? theme.GetColor(ThemeRole.InputBack) : SystemColors.Window;
			_list.ForeColor = themed ? theme.GetColor(ThemeRole.Text) : SystemColors.WindowText;
		}

		// Tool strip colors from the roles of a theme.
		private sealed class ThemeColorTable : ProfessionalColorTable
		{
			private readonly MxTheme _theme;

			public ThemeColorTable(MxTheme theme)
			{
				_theme = theme;
				UseSystemColors = false;
			}

			public override Color ToolStripGradientBegin => _theme.GetColor(ThemeRole.ToolbarBack);

			public override Color ToolStripGradientMiddle => _theme.GetColor(ThemeRole.ToolbarBack);

			public override Color ToolStripGradientEnd => _theme.GetColor(ThemeRole.ToolbarBack);

			public override Color ToolStripBorder => _theme.GetColor(ThemeRole.Border);

			public override Color ButtonSelectedHighlight => _theme.GetColor(ThemeRole.SurfaceHot);

			public override Color ButtonSelectedGradientBegin => _theme.GetColor(ThemeRole.SurfaceHot);

			public override Color ButtonSelectedGradientMiddle => _theme.GetColor(ThemeRole.SurfaceHot);

			public override Color ButtonSelectedGradientEnd => _theme.GetColor(ThemeRole.SurfaceHot);

			public override Color ButtonSelectedBorder => _theme.GetColor(ThemeRole.BorderHot);

			public override Color ButtonPressedGradientBegin => _theme.GetColor(ThemeRole.SurfacePressed);

			public override Color ButtonPressedGradientMiddle => _theme.GetColor(ThemeRole.SurfacePressed);

			public override Color ButtonPressedGradientEnd => _theme.GetColor(ThemeRole.SurfacePressed);

			public override Color ButtonPressedBorder => _theme.GetColor(ThemeRole.BorderPressed);

			public override Color ButtonCheckedGradientBegin => _theme.GetColor(ThemeRole.AccentSubtle);

			public override Color ButtonCheckedGradientMiddle => _theme.GetColor(ThemeRole.AccentSubtle);

			public override Color ButtonCheckedGradientEnd => _theme.GetColor(ThemeRole.AccentSubtle);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_manager.Dispose();
				foreach (Image image in _images)
				{
					image.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public override string Title => "Floating windows and scroll panels";

		public override string Group => "Layout";

		public override string Summary => "A tool bar, a scroll panel with scroll buttons, and a dock panel that floats in its own window.";

		public override int Order => 30;
	}
}
