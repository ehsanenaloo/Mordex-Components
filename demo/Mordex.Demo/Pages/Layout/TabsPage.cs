using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Demo
{
	public sealed class TabsPage : DemoPage
	{
		private static readonly string[] Names = { "Overview", "Customers", "Orders", "Reports", "Messages", "Settings" };

		private static readonly int[] Glyphs = { DemoIcons.Home, DemoIcons.Contact, DemoIcons.Task, DemoIcons.Chart, DemoIcons.Mail, DemoIcons.Settings };

		private readonly MxTabControl _tabs = new MxTabControl { Dock = DockStyle.Fill };
		private readonly ImageList _icons = DemoIcons.List(16, Color.FromArgb(0, 99, 177), Glyphs);
		private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 30, TextAlign = ContentAlignment.MiddleLeft, ForeColor = SystemColors.GrayText, Padding = new Padding(4, 0, 0, 0) };
		private int _added;

		public TabsPage()
		{
			_tabs.ImageList = _icons;
			_tabs.ImageSize = new Size(16, 16);
			for (int i = 0; i < Names.Length; i++)
			{
				_tabs.TabPages.Add(CreatePage(Names[i], i));
			}
			_tabs.ShowCloseButton = true;
			_tabs.ShowToolTips = true;

			Panel host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 16, 16, 8) };
			host.Controls.Add(_tabs);
			host.Controls.Add(_status);
			_tabs.BringToFront();

			OptionsPanel options = new OptionsPanel("Options");
			options.AddChoice("Tab position", new[] { TabStripAlignment.Top, TabStripAlignment.Bottom, TabStripAlignment.Left, TabStripAlignment.Right }, TabStripAlignment.Top, position => _tabs.TabStripAlignment = position);
			options.AddChoice("Tab content", new[] { TabDisplay.ImageAndText, TabDisplay.Text, TabDisplay.Image, TabDisplay.ImageAndTextOnSelected }, TabDisplay.ImageAndText, display => _tabs.TabDisplay = display);
			options.AddChoice("Page border", new[] { PageBorder.All, PageBorder.StripLine, PageBorder.None }, PageBorder.All, border => _tabs.PageBorder = border);
			options.AddChoice("Tab size", new[] { TabSizeMode.Normal, TabSizeMode.Fixed, TabSizeMode.FillToRight }, TabSizeMode.Normal, mode => _tabs.SizeMode = mode);
			options.AddToggle("Close buttons on tabs", true, on => _tabs.ShowCloseButton = on);
			options.AddToggle("Several rows of tabs", false, on => _tabs.MultiLine = on);
			options.AddToggle("Show tooltips", true, on => _tabs.ShowToolTips = on);
			options.AddToggle("Show the tab strip", true, on => _tabs.ShowTabs = on);
			options.AddToggle("Tabs in the title bar", false, on =>
			{
				_tabs.ShowAddTabButton = on;
				_tabs.TabsInCaption = on;
			});
			options.AddButton("Add a tab", AddTab);
			options.AddButton("Close the selected tab", () =>
			{
				if (_tabs.SelectedTab != null && _tabs.SelectedTab.AllowClose)
				{
					MxTabPage page = _tabs.SelectedTab;
					_tabs.TabPages.Remove(page);
					page.Dispose();
					UpdateStatus();
				}
			});

			Compose(host, options);

			_tabs.AddTabClick += (s, e) => AddTab();
			_tabs.SelectedTabChanged += (s, e) => UpdateStatus();
			_tabs.TabClosed += (s, e) => UpdateStatus();
			UpdateStatus();
		}

		public override string Title => "Tabs";

		public override string Group => "Layout";

		public override string Summary => "A tab control with several visual styles, strip positions and display modes. Tabs can show a close button and be added at run time.";

		public override int Order => 10;

		private MxTabPage CreatePage(string name, int index)
		{
			MxTabPage page = new MxTabPage(name, index % Glyphs.Length) { ToolTipText = name + " page", AllowClose = true };
			Label title = PageKit.Heading(name);
			Label text = new Label { Text = "Each page is a normal container. This one holds a list of sample rows for " + name.ToLowerInvariant() + ".", Dock = DockStyle.Top, Height = 34, ForeColor = SystemColors.GrayText };
			ListView list = new ListView { Dock = DockStyle.Fill, View = System.Windows.Forms.View.Details, FullRowSelect = true, BorderStyle = System.Windows.Forms.BorderStyle.None };
			list.Columns.Add("Item", 220);
			list.Columns.Add("Owner", 140);
			list.Columns.Add("Updated", 110);
			Random random = new Random(index * 7 + 3);
			for (int i = 1; i <= 14; i++)
			{
				ListViewItem row = new ListViewItem(name + " " + i);
				row.SubItems.Add(DemoData.Staff[random.Next(DemoData.Staff.Length)]);
				row.SubItems.Add(DateTime.Today.AddDays(-random.Next(40)).ToString("d"));
				list.Items.Add(row);
			}
			page.Padding = new Padding(14, 8, 14, 8);
			page.Controls.Add(list);
			page.Controls.Add(text);
			page.Controls.Add(title);
			list.BringToFront();
			return page;
		}

		private void AddTab()
		{
			_added++;
			MxTabPage page = CreatePage("New tab " + _added, _added);
			_tabs.TabPages.Add(page);
			_tabs.SelectedTab = page;
			UpdateStatus();
		}

		private void UpdateStatus()
		{
			_status.Text = "Selected tab: " + (_tabs.SelectedTab == null ? "none" : _tabs.SelectedTab.Text) + "   |   " + _tabs.TabPages.Count + " tabs";
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
