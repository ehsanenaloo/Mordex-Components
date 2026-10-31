using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.TaskPane;

namespace Mordex.Demo
{
	public sealed class TaskPanePage : DemoPage
	{
		private static readonly int[] Glyphs =
		{
			DemoIcons.Add, DemoIcons.Folder, DemoIcons.Save, DemoIcons.Print,
			DemoIcons.Cut, DemoIcons.Copy, DemoIcons.Paste, DemoIcons.Undo,
			DemoIcons.Search, DemoIcons.Refresh, DemoIcons.Download, DemoIcons.Send,
			DemoIcons.Help, DemoIcons.Settings, DemoIcons.Home
		};

		private readonly MxTaskPane _pane = new MxTaskPane { Dock = DockStyle.Left, Width = 270 };
		private readonly ImageList _icons = DemoIcons.List(16, Color.FromArgb(0, 99, 177), Glyphs);
		private readonly Label _command = PageKit.Heading("No task chosen yet", 18f);
		private readonly Label _hint = new Label { Dock = DockStyle.Top, Height = 40, ForeColor = SystemColors.GrayText, Text = "Choose a task in the pane on the left. Each click is added to the history below." };
		private readonly ListView _history = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle, HeaderStyle = ColumnHeaderStyle.Nonclickable };

		public TaskPanePage()
		{
			_pane.ImageList = _icons;
			_pane.ImageSize = new Size(16, 16);

			TaskPaneGroup file = AddGroup("File tasks", true, false);
			AddItem(file, "New document", 0);
			AddItem(file, "Open a folder", 1);
			AddItem(file, "Save a copy", 2);
			AddItem(file, "Print", 3);

			TaskPaneGroup edit = AddGroup("Edit tasks", true, false);
			AddItem(edit, "Cut", 4);
			AddItem(edit, "Copy", 5);
			AddItem(edit, "Paste", 6);
			AddItem(edit, "Undo last change", 7);

			TaskPaneGroup tools = AddGroup("Tools", true, false);
			AddItem(tools, "Find and replace", 8);
			AddItem(tools, "Refresh the list", 9);
			AddItem(tools, "Download updates", 10);
			AddItem(tools, "Send by e-mail", 11);

			TaskPaneGroup details = AddGroup("Details", true, true);
			details.Items.Add(new TaskPaneItem("Quarterly report.docx", ItemType.Description));
			details.Items.Add(new TaskPaneItem("Word document, 148 KB", ItemType.Description));
			details.Items.Add(new TaskPaneItem("Changed today at 09:12", ItemType.Description));

			_history.Columns.Add("Time", 90);
			_history.Columns.Add("Task", 220);
			_history.Columns.Add("Group", 160);

			Panel content = new Panel { Padding = new Padding(20, 12, 20, 12) };
			content.Controls.Add(_history);
			content.Controls.Add(_hint);
			content.Controls.Add(_command);
			_history.BringToFront();

			Panel host = new Panel { Dock = DockStyle.Fill };
			host.Controls.Add(content);
			content.Dock = DockStyle.Fill;
			host.Controls.Add(_pane);
			_pane.Dock = DockStyle.Left;
			content.BringToFront();

			OptionsPanel options = new OptionsPanel("Options");
			options.AddChoice("Group animation", new[] { Effect.Fade, Effect.Scroll, Effect.None }, Effect.Scroll, effect => _pane.Effect = effect);
			options.AddToggle("Show group borders", true, on => _pane.ShowGroupBorder = on);
			options.AddToggle("Show tooltips", true, on => _pane.ShowToolTips = on);
			options.AddToggle("Close buttons on groups", false, on =>
			{
				foreach (TaskPaneGroup group in _pane.Groups)
				{
					group.ShowCloseButton = on;
				}
			});
			options.AddToggle("Groups can collapse", true, on =>
			{
				foreach (TaskPaneGroup group in _pane.Groups)
				{
					group.Expandable = on;
				}
			});
			options.AddButton("Collapse all groups", () => SetExpanded(false));
			options.AddButton("Expand all groups", () => SetExpanded(true));
			options.AddNote("Groups open and close with an animation. The last group only shows information.");

			Compose(host, options);

			_pane.ItemClick += (s, e) => Log(e.Item);
			foreach (TaskPaneItem seed in new[] { file.Items[1], file.Items[0] })
			{
				Log(seed);
			}
		}

		public override string Title => "Task pane";

		public override string Group => "Navigation";

		public override string Summary => "A task pane with groups of commands next to a content area. Groups collapse, close and change style.";

		public override int Order => 20;

		private TaskPaneGroup AddGroup(string text, bool expanded, bool special)
		{
			TaskPaneGroup group = new TaskPaneGroup(text) { Expanded = expanded, SpecialGroup = special, ToolTipText = text };
			_pane.Groups.Add(group);
			return group;
		}

		private static void AddItem(TaskPaneGroup group, string text, int image)
		{
			group.Items.Add(new TaskPaneItem(text, image) { ToolTipText = text });
		}

		private void SetExpanded(bool expanded)
		{
			foreach (TaskPaneGroup group in _pane.Groups)
			{
				group.Expanded = expanded;
			}
		}

		private void Log(TaskPaneItem item)
		{
			if (item == null || item.ItemType != ItemType.LinkButton)
			{
				return;
			}
			_command.Text = item.Text;
			_hint.Text = "Run from the " + item.Group.Text.ToLowerInvariant() + " group. A real application would start the task here.";
			ListViewItem row = new ListViewItem(DateTime.Now.ToString("T"));
			row.SubItems.Add(item.Text);
			row.SubItems.Add(item.Group.Text);
			_history.Items.Insert(0, row);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_icons.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
