using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Demo
{
	public sealed class CommandsAndStatusPage : DemoPage
	{
		private static readonly int[] Glyphs =
		{
			DemoIcons.Add, DemoIcons.Folder, DemoIcons.Save, DemoIcons.Print, DemoIcons.Cut, DemoIcons.Copy,
			DemoIcons.Paste, DemoIcons.Undo, DemoIcons.Redo, DemoIcons.Edit, DemoIcons.Search, DemoIcons.Refresh,
			DemoIcons.Help, DemoIcons.Play
		};

		private readonly Panel _host = new Panel();
		private readonly MxCommandManager _manager = new MxCommandManager();
		private readonly ImageList _small = DemoIcons.List(16, Color.FromArgb(0, 99, 177), Glyphs);
		private readonly ImageList _large = DemoIcons.List(24, Color.FromArgb(0, 99, 177), Glyphs);
		private readonly RichTextBox _editor = new RichTextBox { Dock = DockStyle.Fill, BorderStyle = System.Windows.Forms.BorderStyle.None, Font = new Font("Segoe UI", 11f), Margin = new Padding(0) };
		private readonly MxStatusBar _status = new MxStatusBar { Dock = DockStyle.Bottom, Height = 26 };
		private readonly MxStatusBarPanel _message = new MxStatusBarPanel("Ready") { AutoSize = Mordex.Windows.UI.StatusBar.StatusBarPanelAutoSize.Spring, Key = "message" };
		private readonly MxStatusBarPanel _progress = new MxStatusBarPanel { PanelType = StatusBarPanelType.ProgressBar, Width = 150, Key = "progress", ProgressBarMinValue = 0, ProgressBarMaxValue = 100 };
		private readonly MxStatusBarPanel _zoomText = new MxStatusBarPanel("100%") { Width = 56, Alignment = HorizontalAlignment.Center, Key = "zoomText" };
		private readonly MxStatusBarPanel _zoomSlider = new MxStatusBarPanel { PanelType = StatusBarPanelType.ControlContainer, Width = 130, Key = "zoomSlider" };
		private readonly MxStatusBarPanel _clock = new MxStatusBarPanel { Width = 80, Alignment = HorizontalAlignment.Center, Key = "clock" };
		private readonly MxTrackBar _zoom = new MxTrackBar { Minimum = 50, Maximum = 300, Value = 100, SmallChange = 10, LargeChange = 25, TickStyle = System.Windows.Forms.TickStyle.None, Height = 22 };
		private readonly Timer _clockTimer = new Timer { Interval = 1000 };
		private readonly Timer _taskTimer = new Timer { Interval = 40 };
		private MxCommand _cut;
		private MxCommand _copy;
		private MxCommand _paste;
		private MxCommand _bold;
		private MxCommand _italic;
		private MxCommand _underline;

		public CommandsAndStatusPage()
		{
			_editor.Text = "Quarterly summary\n\nSales grew in every region this quarter. The largest gain came from the northern group, where new accounts doubled.\n\nSelect some text, then use the toolbar, the menu or the right-click menu. All three run the same commands.\n\nBold, italic and underline are toggle commands. Their buttons and menu items stay checked while the selection has that style.\n\nChoose Save to see the progress panel in the status bar. Move the zoom slider at the right to change the size of the text.";
			_editor.Select(0, 17);
			using (Font heading = new Font("Segoe UI", 16f, FontStyle.Bold))
			{
				_editor.SelectionFont = heading;
			}
			_editor.Select(0, 0);

			_manager.ImageList = _small;
			_manager.LargeImageList = _large;
			_manager.ContainerControl = _host;

			CreateCommands(out MxCommand[] fileGroup, out MxCommand[] editGroup, out MxCommand[] formatGroup, out MxCommand[] viewGroup);

			MxCommandBar menuBar = new MxCommandBar(_manager, "MenuBar") { CommandBarType = CommandBarType.Menu, DockStyle = BarsDockStyle.Top, FullRow = true, CommandsStyle = CommandStyle.Text };
			menuBar.Commands.Add(ParentCommand("File", "&File", fileGroup));
			menuBar.Commands.Add(ParentCommand("Edit", "&Edit", editGroup));
			menuBar.Commands.Add(ParentCommand("Format", "F&ormat", formatGroup));
			menuBar.Commands.Add(ParentCommand("View", "&View", viewGroup));

			MxCommandBar toolBar = new MxCommandBar(_manager, "ToolBar") { CommandBarType = CommandBarType.ToolBar, DockStyle = BarsDockStyle.Top, Text = "Standard" };
			foreach (MxCommand command in new[] { fileGroup[0], fileGroup[1], fileGroup[2], fileGroup[3] })
			{
				toolBar.Commands.Add(new MxCommand(command));
			}
			toolBar.Commands.AddSeparator();
			foreach (MxCommand command in new[] { editGroup[0], editGroup[1], editGroup[2], editGroup[4], editGroup[5] })
			{
				toolBar.Commands.Add(new MxCommand(command));
			}
			toolBar.Commands.AddSeparator();
			foreach (MxCommand command in formatGroup)
			{
				toolBar.Commands.Add(new MxCommand(command));
			}
			toolBar.Commands.AddSeparator();
			foreach (MxCommand command in viewGroup)
			{
				toolBar.Commands.Add(new MxCommand(command));
			}

			MxContextMenu menu = new MxContextMenu(_manager, "EditorMenu");
			foreach (MxCommand command in new[] { editGroup[0], editGroup[1], editGroup[2], editGroup[4] })
			{
				menu.Commands.Add(new MxCommand(command));
			}
			menu.Commands.AddSeparator();
			foreach (MxCommand command in formatGroup)
			{
				menu.Commands.Add(new MxCommand(command));
			}
			_manager.SetContextMenu(_editor, menu);

			BuildStatusBar();

			_host.Controls.Add(_editor);
			_host.Controls.Add(_status);
			_editor.BringToFront();

			OptionsPanel options = BuildOptions();
			Compose(_host, options);

			_editor.SelectionChanged += (s, e) => UpdateStates();
			_zoom.ValueChanged += (s, e) => SetZoom(_zoom.Value);
			_clockTimer.Tick += (s, e) => _clock.Text = DateTime.Now.ToString("T");
			_taskTimer.Tick += (s, e) => StepTask();
			_clock.Text = DateTime.Now.ToString("T");
			_clockTimer.Start();
			UpdateStates();
		}

		public override string Title => "Commands and status bar";

		public override string Group => "Layout";

		public override string Summary => "A menu bar, a toolbar and a context menu that share one set of commands, with a status bar that shows progress, zoom and the time.";

		public override int Order => 30;

		private void CreateCommands(out MxCommand[] file, out MxCommand[] edit, out MxCommand[] format, out MxCommand[] view)
		{
			MxCommand fresh = Define("New", "&New", 0, Shortcut.CtrlN, () => { _editor.Clear(); Say("New document"); });
			MxCommand open = Define("Open", "&Open...", 1, Shortcut.CtrlO, () => Say("Open was chosen"));
			MxCommand save = Define("Save", "&Save", 2, Shortcut.CtrlS, StartTask);
			MxCommand print = Define("Print", "&Print", 3, Shortcut.CtrlP, () => Say("Printing is not part of this demo"));
			file = new[] { fresh, open, save, print };

			_cut = Define("Cut", "Cu&t", 4, Shortcut.CtrlX, () => _editor.Cut());
			_copy = Define("Copy", "&Copy", 5, Shortcut.CtrlC, () => _editor.Copy());
			_paste = Define("Paste", "&Paste", 6, Shortcut.CtrlV, () => _editor.Paste());
			MxCommand undo = Define("Undo", "&Undo", 7, Shortcut.CtrlZ, () => _editor.Undo());
			MxCommand redo = Define("Redo", "&Redo", 8, Shortcut.CtrlY, () => _editor.Redo());
			MxCommand all = Define("SelectAll", "Select &all", -1, Shortcut.CtrlA, () => _editor.SelectAll());
			edit = new[] { _cut, _copy, _paste, all, undo, redo };

			_bold = Define("Bold", "&Bold", -1, Shortcut.CtrlB, () => ToggleStyle(FontStyle.Bold), CommandType.ToggleButton);
			_italic = Define("Italic", "&Italic", -1, Shortcut.CtrlI, () => ToggleStyle(FontStyle.Italic), CommandType.ToggleButton);
			_underline = Define("Underline", "&Underline", -1, Shortcut.CtrlU, () => ToggleStyle(FontStyle.Underline), CommandType.ToggleButton);
			format = new[] { _bold, _italic, _underline };

			MxCommand zoomIn = Define("ZoomIn", "Zoom &in", 10, Shortcut.None, () => _zoom.Value = Math.Min(_zoom.Maximum, _zoom.Value + 25));
			MxCommand zoomOut = Define("ZoomOut", "Zoom &out", 11, Shortcut.None, () => _zoom.Value = Math.Max(_zoom.Minimum, _zoom.Value - 25));
			MxCommand reset = Define("ZoomReset", "&Actual size", 12, Shortcut.None, () => _zoom.Value = 100);
			view = new[] { zoomIn, zoomOut, reset };
		}

		private MxCommand Define(string key, string text, int image, Shortcut shortcut, Action run, CommandType type = CommandType.Command)
		{
			MxCommand command = new MxCommand(key, text, image, shortcut) { CommandType = type };
			if (image >= 0)
			{
				command.LargeImageIndex = image;
			}
			if (image < 0)
			{
				command.CommandStyle = CommandStyle.Text;
			}
			command.ToolTipText = text.Replace("&", string.Empty);
			command.Click += (s, e) => run();
			_manager.Commands.Add(command);
			return command;
		}

		private static MxCommand ParentCommand(string key, string text, MxCommand[] children)
		{
			MxCommand parent = new MxCommand(key, text);
			foreach (MxCommand child in children)
			{
				parent.Commands.Add(new MxCommand(child));
			}
			return parent;
		}

		private void BuildStatusBar()
		{
			_zoomSlider.Control = _zoom;
			_status.Panels.Add(_message);
			_status.Panels.Add(_progress);
			_status.Panels.Add(_zoomSlider);
			_status.Panels.Add(_zoomText);
			_status.Panels.Add(_clock);
			_message.ToolTipText = "The last action";
			_clock.ToolTipText = "The time of day";
		}

		private OptionsPanel BuildOptions()
		{
			OptionsPanel options = new OptionsPanel("Options");
			options.AddChoice("Toolbar buttons", new[] { CommandStyle.Image, CommandStyle.TextImage, CommandStyle.Text }, CommandStyle.Image, style => _manager.CommandsStyle = style);
			options.AddChoice("Icon size", new[] { Mordex.Windows.UI.CommandBars.View.SmallIcons, Mordex.Windows.UI.CommandBars.View.LargeIcons }, Mordex.Windows.UI.CommandBars.View.SmallIcons, view => _manager.View = view);
			options.AddToggle("Show tooltips", true, on => _manager.ShowToolTips = on);
			options.AddToggle("Show shortcuts in tooltips", true, on => _manager.ShowShortcutInToolTips = on);
			options.AddToggle("Lock the toolbars", false, on => _manager.LockCommandBars = on);
			options.AddToggle("Clipboard commands enabled", true, on =>
			{
				InheritableBoolean state = on ? InheritableBoolean.True : InheritableBoolean.False;
				_cut.Enabled = state;
				_copy.Enabled = state;
				_paste.Enabled = state;
			});
			options.AddButton("Save (runs the progress bar)", StartTask);
			options.AddNote("Disable the clipboard commands to see the menu, toolbar and context menu change together.");
			return options;
		}

		private void ToggleStyle(FontStyle style)
		{
			using (Font current = _editor.SelectionFont)
			{
				if (current == null)
				{
					return;
				}
				using (Font next = new Font(current, current.Style ^ style))
				{
					_editor.SelectionFont = next;
				}
			}
			UpdateStates();
		}

		private void UpdateStates()
		{
			using (Font current = _editor.SelectionFont)
			{
				FontStyle style = current == null ? FontStyle.Regular : current.Style;
				_bold.Checked = (style & FontStyle.Bold) != 0 ? InheritableBoolean.True : InheritableBoolean.False;
				_italic.Checked = (style & FontStyle.Italic) != 0 ? InheritableBoolean.True : InheritableBoolean.False;
				_underline.Checked = (style & FontStyle.Underline) != 0 ? InheritableBoolean.True : InheritableBoolean.False;
			}
		}

		private void SetZoom(int percent)
		{
			_editor.ZoomFactor = percent / 100f;
			_zoomText.Text = percent + "%";
		}

		private void Say(string text)
		{
			_message.Text = text;
		}

		private void StartTask()
		{
			_progress.ProgressBarValue = 0;
			Say("Saving...");
			_taskTimer.Start();
		}

		private void StepTask()
		{
			_progress.ProgressBarValue = Math.Min(100, _progress.ProgressBarValue + 4);
			if (_progress.ProgressBarValue >= 100)
			{
				_taskTimer.Stop();
				Say("Saved at " + DateTime.Now.ToString("T"));
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_clockTimer.Stop();
				_clockTimer.Dispose();
				_taskTimer.Stop();
				_taskTimer.Dispose();
				_manager.Dispose();
				_small.Dispose();
				_large.Dispose();
				_editor.Font.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
