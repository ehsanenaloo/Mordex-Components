using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Navigation;

namespace Mordex.Demo
{
	public sealed class NavigationBarPage : DemoPage
	{
		private static readonly int[] Glyphs =
		{
			DemoIcons.Mail, DemoIcons.Document, DemoIcons.Send, DemoIcons.Delete, DemoIcons.Archive,
			DemoIcons.Calendar, DemoIcons.Task, DemoIcons.Contact, DemoIcons.Folder, DemoIcons.Tag,
			DemoIcons.Chart, DemoIcons.Settings, DemoIcons.Home, DemoIcons.Pin, DemoIcons.Search
		};

		private readonly MxNavigationBar _bar = new MxNavigationBar { Dock = DockStyle.Left, Width = 250 };
		private readonly ImageList _small = DemoIcons.List(18, Color.FromArgb(0, 99, 177), Glyphs);
		private readonly ImageList _large = DemoIcons.List(32, Color.FromArgb(0, 99, 177), Glyphs);
		private readonly Label _title = PageKit.Heading("", 18f);
		private readonly Label _subtitle = new Label { Dock = DockStyle.Top, Height = 28, ForeColor = SystemColors.GrayText };
		private readonly ListView _list = new ListView { Dock = DockStyle.Fill, View = System.Windows.Forms.View.Details, FullRowSelect = true, HideSelection = false, BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle };
		private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 28, ForeColor = SystemColors.GrayText, TextAlign = ContentAlignment.MiddleLeft };

		public NavigationBarPage()
		{
			_bar.SmallImageList = _small;
			_bar.LargeImageList = _large;

			AddGroup("Mail", 0, "Inbox", "Drafts", "Sent items", "Deleted items", "Archive");
			AddGroup("Calendar", 5, "Today", "This week", "Tasks");
			AddGroup("Contacts", 7, "People", "Teams", "Tags");
			AddGroup("Workspace", 10, "Reports", "Settings");

			_list.Columns.Add("Title", 280);
			_list.Columns.Add("Owner", 140);
			_list.Columns.Add("Date", 110);
			_list.Columns.Add("Status", 100);

			Panel content = new Panel { Padding = new Padding(20, 12, 20, 12) };
			content.Controls.Add(_list);
			content.Controls.Add(_subtitle);
			content.Controls.Add(_title);
			content.Controls.Add(_status);
			_list.BringToFront();

			Panel host = new Panel { Dock = DockStyle.Fill };
			host.Controls.Add(content);
			content.Dock = DockStyle.Fill;
			host.Controls.Add(_bar);
			_bar.Dock = DockStyle.Left;
			content.BringToFront();

			OptionsPanel options = new OptionsPanel("Options");
			options.AddChoice("Style", new[] { NavigationBarStyle.Default, NavigationBarStyle.Office2003, NavigationBarStyle.Office2007, NavigationBarStyle.Office2010, NavigationBarStyle.VS2005, NavigationBarStyle.VS2010, NavigationBarStyle.OfficeXP }, NavigationBarStyle.Default, style => _bar.SetNavigationBarStyle(style));
			options.AddChoice("Item icons", new[] { NavigationBarView.SmallIcons, NavigationBarView.LargeIcons }, NavigationBarView.SmallIcons, view =>
			{
				foreach (NavigationGroup group in _bar.Groups)
				{
					group.View = view;
				}
			});
			options.AddChoice("Group animation", new[] { UnfoldEffect.Slide, UnfoldEffect.None }, UnfoldEffect.Slide, effect => _bar.UnfoldEffect = effect);
			options.AddChoice("Selection area", new[] { SelectionArea.FullItem, SelectionArea.Image }, SelectionArea.FullItem, area => _bar.SelectionArea = area);
			options.AddToggle("Show group headers", true, on => _bar.HeaderGroupVisible = on);
			options.AddToggle("Show tooltips", true, on => _bar.ShowToolTips = on);
			options.AddToggle("Drag groups to reorder", true, on => _bar.AllowDragGroups = on);
			options.AddButton("Go to the first group", () => _bar.SelectedGroup = _bar.Groups[0]);
			options.AddNote("Click a group header to open it. Click an item to change the page on the right.");

			Compose(host, options);

			_bar.ItemSelected += (s, e) => ShowItem(_bar.SelectedItem);
			_bar.SelectedGroup = _bar.Groups[0];
			_bar.SelectedItem = _bar.Groups[0].Items[0];
			ShowItem(_bar.SelectedItem);
		}

		public override string Title => "Navigation bar";

		public override string Group => "Navigation";

		public override string Summary => "An Outlook-style navigation bar with groups and items. The area on the right follows the selected item.";

		public override int Order => 10;

		private void AddGroup(string text, int firstImage, params string[] items)
		{
			NavigationGroup group = new NavigationGroup(text) { ImageIndex = firstImage, ToolTipText = text };
			for (int i = 0; i < items.Length; i++)
			{
				group.Items.Add(new NavigationItem(items[i], firstImage + i, firstImage + i) { ToolTipText = items[i] });
			}
			_bar.Groups.Add(group);
		}

		private void ShowItem(NavigationItem item)
		{
			if (item == null)
			{
				return;
			}
			_title.Text = item.Text;
			_subtitle.Text = item.Group.Text + "  |  " + item.Group.Items.Count + " views in this group";
			string[] owners = DemoData.Staff;
			string[] states = { "New", "Open", "Done", "Waiting" };
			Random random = new Random(item.Text.GetHashCode() & 0xFFFF);
			_list.BeginUpdate();
			_list.Items.Clear();
			int count = 8 + random.Next(10);
			for (int i = 0; i < count; i++)
			{
				ListViewItem row = new ListViewItem(item.Text + " entry " + (i + 1));
				row.SubItems.Add(owners[random.Next(owners.Length)]);
				row.SubItems.Add(DateTime.Today.AddDays(-random.Next(30)).ToString("d"));
				row.SubItems.Add(states[random.Next(states.Length)]);
				_list.Items.Add(row);
			}
			_list.EndUpdate();
			_status.Text = count + " items";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_small.Dispose();
				_large.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
