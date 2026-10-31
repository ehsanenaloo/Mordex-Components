using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon;
using Mordex.Windows.UI.Controls;

namespace Mordex.Demo
{
	public sealed class TooltipsAndAboutPage : DemoPage
	{
		private readonly MxSuperTip _superTip = new MxSuperTip { InitialDelay = 300, AutoPopDelay = 8000, ShowAlways = true };
		private readonly ThemedToolTip _simpleTip = new ThemedToolTip { InitialDelay = 300, ShowAlways = true };
		private readonly MxButton _rich = new MxButton { Text = "Hover for a super tip", Width = 190, Height = 30 };
		private readonly MxButton _plain = new MxButton { Text = "Hover for a plain tip", Width = 190, Height = 30 };
		private readonly Label _clickLabel = new Label { Text = "Right click here for a context menu", BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 54 };
		private readonly MxRibbonContextMenu _menu = new MxRibbonContextMenu();
		private readonly MxRibbonStatusBar _statusBar = new MxRibbonStatusBar { ShowSizeGrip = false, Height = 26 };
		private readonly SuperTipSettings _settings = new SuperTipSettings();
		private readonly Label _last = PageKit.Value("Nothing chosen yet");
		private readonly List<Image> _images = new List<Image>();
		private readonly LinkLabel _link = new LinkLabel { Text = "Read the guide for super tips", AutoSize = true, Margin = new Padding(0, 6, 0, 0) };

		public TooltipsAndAboutPage()
		{
			Image picture = DemoIcons.Glyph(DemoIcons.Help, Color.FromArgb(0, 120, 212), 32);
			Image footer = DemoIcons.Glyph(DemoIcons.Document, Color.FromArgb(96, 96, 96), 16);
			_images.Add(picture);
			_images.Add(footer);
			_settings.HeaderText = "Save the document";
			_settings.HeaderImage = footer;
			_settings.Text = "Writes the open document to disk.\nThe file keeps its name and its folder.";
			_settings.Image = picture;
			_settings.FooterText = "Press F1 for more help";
			_settings.FooterImage = footer;
			_superTip.UseDefaultTheme = true;
			_superTip.SetSuperTip(_rich, _settings);
			_simpleTip.SetToolTip(_plain, "A plain tip draws itself with the theme");
			_link.LinkClicked += (s, e) => _last.Text = "The link was clicked: a super tip can sit next to a link that opens a guide.";

			BuildMenu();
			BuildStatusBar();

			TableLayoutPanel grid = PageKit.Grid(2, 100f);
			TableLayoutPanel left = PageKit.Rows(200f, 150f);
			MxGroupBox tips = PageKit.Group("Tool tips");
			FlowLayoutPanel tipFlow = PageKit.Stack();
			tipFlow.Controls.Add(_rich);
			tipFlow.Controls.Add(_plain);
			tipFlow.Controls.Add(_link);
			tipFlow.Controls.Add(PageKit.Caption("Both tips take their colors, border and font size from the theme of the window."));
			tips.Controls.Add(tipFlow);
			MxGroupBox menuGroup = PageKit.Group("Context menu");
			Panel menuHost = new Panel { Dock = DockStyle.Fill };
			_last.Dock = DockStyle.Bottom;
			menuHost.Controls.Add(_last);
			menuHost.Controls.Add(_clickLabel);
			menuGroup.Controls.Add(menuHost);
			left.Controls.Add(tips, 0, 0);
			left.Controls.Add(menuGroup, 0, 1);

			TableLayoutPanel right = PageKit.Rows(200f, 150f);
			MxGroupBox status = PageKit.Group("Status bar");
			Panel statusHost = new Panel { Dock = DockStyle.Fill };
			_statusBar.Dock = DockStyle.Top;
			statusHost.Controls.Add(_statusBar);
			status.Controls.Add(statusHost);
			right.Controls.Add(PageParts.CodeGroup("Code", "superTip.SetSuperTip(button, new SuperTipSettings\n{\n    HeaderText = \"Save the document\",\n    Text = \"Writes the open document to disk.\",\n    Image = picture,\n    FooterText = \"Press F1 for more help\",\n    ToolTipStyle = ToolTipStyle.Office2007\n});\nsuperTip.UseDefaultTheme = true;\n\nvar tip = new ThemedToolTip();\ntip.SetToolTip(other, \"A plain tip\");\n\nusing (var about = new MxAboutDialog(assembly))\n    about.ShowDialog(this);"), 0, 0);
			right.Controls.Add(status, 0, 1);
			grid.Controls.Add(left, 0, 0);
			grid.Controls.Add(right, 1, 0);

			OptionsPanel options = new OptionsPanel();
			options.AddToggle("Use the colors of the theme", true, on => _superTip.UseDefaultTheme = on);
			options.AddChoice("Super tip style when not themed", new[] { ToolTipStyle.Standard, ToolTipStyle.Office2007, ToolTipStyle.Office2010 }, ToolTipStyle.Office2007, style =>
			{
				_settings.ToolTipStyle = style;
				_superTip.SetSuperTip(_rich, _settings);
			});
			options.AddToggle("Word wrap in the super tip", false, on =>
			{
				_settings.WordWrap = on;
				_superTip.SetSuperTip(_rich, _settings);
			});
			options.AddButton("Show the super tip now", () => _superTip.Show(_settings, _rich, new Point(0, _rich.Height + 4)));
			options.AddButton("About this suite", ShowAbout);
			options.AddNote("Hover the buttons and wait for the tip. The About dialog follows the theme while it is open.");
			Compose(grid, options);
		}

		private void BuildMenu()
		{
			_menu.Commands.Add(new ButtonCommand("cut", "Cut"));
			_menu.Commands.Add(new ButtonCommand("copy", "Copy"));
			_menu.Commands.Add(new ButtonCommand("paste", "Paste"));
			_menu.Commands.Add(new SeparatorCommand());
			_menu.Commands.Add(new ButtonCommand("properties", "Properties"));
			_menu.CommandClick += (s, e) => _last.Text = "You chose: " + e.Command.Key;
			_clickLabel.MouseUp += (s, e) =>
			{
				if (e.Button == MouseButtons.Right)
				{
					_menu.Show(_clickLabel, e.Location);
				}
			};
		}

		private void BuildStatusBar()
		{
			_statusBar.LeftPanelCommands.Add(new LabelCommand("ready", "Ready"));
			_statusBar.LeftPanelCommands.Add(new LabelCommand("words", "Words: 128"));
			_statusBar.RightPanelCommands.Add(new LabelCommand("zoom", "Zoom 100%"));
			ButtonCommand view = new ButtonCommand("view", "Print layout");
			view.Click += (s, e) => _last.Text = "The view button of the status bar was clicked.";
			_statusBar.RightPanelCommands.Add(view);
		}

		private void ShowAbout()
		{
			using (MxAboutDialog dialog = new MxAboutDialog(typeof(MxRibbon).Assembly))
			{
				dialog.ShowDialog(FindForm());
			}
		}

		public override void ApplyLook(bool themed)
		{
			_link.LinkColor = themed ? MxThemeManager.DefaultTheme.GetColor(ThemeRole.TextLink) : SystemColors.HotTrack;
			_link.ActiveLinkColor = _link.LinkColor;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_superTip.Dispose();
				_simpleTip.Dispose();
				_menu.Dispose();
				foreach (Image image in _images)
				{
					image.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public override string Title => "Tooltips and about";

		public override string Group => "Output and help";

		public override string Summary => "Super tips with images and a footer, themed plain tips, the About dialog, a ribbon context menu and a ribbon status bar.";

		public override int Order => 20;
	}
}
