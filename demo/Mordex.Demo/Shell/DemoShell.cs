using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Navigation;
using Mordex.Windows.Ribbon;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Demo
{
	/// <summary>
	/// The main window of the demo: a ribbon with theme choices, a navigation bar with one entry for each demo page, the
	/// page itself under a title, and a status bar.
	/// </summary>
	public sealed class DemoShell : Form
	{
		private readonly MxRibbon _ribbon = new MxRibbon();
		private readonly MxNavigationBar _navigation = new MxNavigationBar();
		private readonly Panel _header = new Panel { Dock = DockStyle.Top, Height = 64 };
		private readonly Label _title = new Label { AutoSize = false, Dock = DockStyle.Top, Height = 34, Padding = new Padding(16, 6, 0, 0) };
		private readonly Label _summary = new Label { AutoSize = false, Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 0) };
		private readonly Panel _host = new Panel { Dock = DockStyle.Fill };
		private readonly MxStatusBar _status = new MxStatusBar();
		private readonly MxStatusBarPanel _statusMessage = new MxStatusBarPanel();
		private readonly MxStatusBarPanel _statusTheme = new MxStatusBarPanel();
		private readonly IList<DemoPage> _pages = DemoPage.CreateAll();
		private readonly Dictionary<NavigationItem, DemoPage> _pageByItem = new Dictionary<NavigationItem, DemoPage>();
		private readonly ThemeStrip _themeStrip = new ThemeStrip();
		private DemoPage _current;
		private ButtonCommand _showNavigation;
		private readonly MxThemedFormFrame _frame = new MxThemedFormFrame();
		private ButtonCommand _frameFollow;
		private ButtonCommand _frameSystem;
		private ButtonCommand _followWindows;
		private bool _changing;

		public DemoShell()
		{
			Text = "Mordex Components Demo";
			Size = new Size(1440, 900);
			MinimumSize = new Size(1024, 680);
			StartPosition = FormStartPosition.CenterScreen;
			Icon = LoadIcon();
			_frame.Form = this;
			BuildRibbon();
			BuildNavigation();
			BuildHeader();
			BuildStatusBar();
			_host.Padding = new Padding(0);
			Controls.Add(_host);
			Controls.Add(_header);
			Controls.Add(_navigation);
			Controls.Add(_status);
			Controls.Add(_ribbon);
			_themeStrip.ThemeChosen += (sender, theme) => SetThemedLook(theme != null, theme);
			_themeStrip.FollowChanged += (sender, on) => SetFollowWindows(on);
			MxThemeManager.DefaultThemeChanged += OnDefaultThemeChanged;
			_statusTheme.Text = "Classic";
			if (_navigation.Groups.Count > 0 && _navigation.Groups[0].Items.Count > 0)
			{
				_navigation.SelectedItem = _navigation.Groups[0].Items[0];
			}
		}

		public int PageCount => _pages.Count;

		public string PageTitle(int index)
		{
			return _pages[index].Title;
		}

		/// <summary>Opens the page at the position in the navigation order.</summary>
		public int IndexOfPage(string title)
		{
			for (int i = 0; i < _pages.Count; i++)
			{
				if (_pages[i].Title == title)
				{
					return i;
				}
			}
			return -1;
		}

		public void OpenPage(int index)
		{
			if (index < 0 || index >= _pages.Count)
			{
				return;
			}
			foreach (KeyValuePair<NavigationItem, DemoPage> entry in _pageByItem)
			{
				if (entry.Value == _pages[index])
				{
					_navigation.SelectedItem = entry.Key;
					return;
				}
			}
		}

		/// <summary>Shows every page in turn, to check that each one opens. Returns the number that failed.</summary>
		public int ShowEachPage(System.Text.StringBuilder log)
		{
			int failures = ShowEachPage(log, "");
			// The same pages in a theme, then back in the classic look, so a page that cannot take a theme is found here.
			MxTheme original = MxThemeManager.DefaultTheme;
			try
			{
				UseThemedLook(MxThemeManager.Find("Windows 11 Dark"));
				failures += ShowEachPage(log, "[Windows 11 Dark] ");
				// Following Windows switches the theme to the matching light or dark one and the pages must still open.
				SetFollowWindows(true);
				failures += ShowEachPage(log, "[follow Windows] ");
				SetFollowWindows(false);
				SetThemedLook(false, null);
				failures += ShowEachPage(log, "[classic again] ");
			}
			finally
			{
				MxThemeManager.FollowWindows = false;
				MxThemeManager.DefaultTheme = original;
			}
			return failures;
		}

		private int ShowEachPage(System.Text.StringBuilder log, string prefix)
		{
			int failures = 0;
			_current = null;
			foreach (KeyValuePair<NavigationItem, DemoPage> entry in _pageByItem)
			{
				try
				{
					Show(entry.Value);
					for (int i = 0; i < 3; i++)
					{
						Application.DoEvents();
					}
					log.AppendLine("OK   " + prefix + entry.Value.Group + " / " + entry.Value.Title);
				}
				catch (Exception ex)
				{
					failures++;
					log.AppendLine("FAIL " + prefix + entry.Value.Group + " / " + entry.Value.Title + ": " + ex.GetType().Name + ": " + ex.Message);
				}
			}
			return failures;
		}

		private static Icon LoadIcon()
		{
			try
			{
				return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void BuildRibbon()
		{
			_ribbon.HandlesTitleBar = false;
			_ribbon.ShowCaptionBar = false;
			_ribbon.ApplicationName = "Mordex Components";
			_ribbon.Dock = DockStyle.Top;
			RibbonTab home = new RibbonTab("Home");
			RibbonGroup navigate = new RibbonGroup("navigate", "Navigate");
			ButtonCommand previous = Large(new ButtonCommand("previous", "Previous page"), "nav");
			previous.Click += (sender, e) => Step(-1);
			ButtonCommand next = Large(new ButtonCommand("next", "Next page"), "nav");
			next.Click += (sender, e) => Step(1);
			_showNavigation = Large(new ButtonCommand("navigation", "Navigation") { CheckOnClick = true, Checked = true }, "tree");
			_showNavigation.CheckedChanged += (sender, e) => _navigation.Visible = _showNavigation.Checked;
			navigate.Commands.Add(previous);
			navigate.Commands.Add(next);
			navigate.Commands.Add(_showNavigation);
			_ribbon.QuickAccessCommands.Add(previous);
			_ribbon.QuickAccessCommands.Add(next);
			home.Groups.Add(navigate);
			RibbonTab help = new RibbonTab("Help");
			RibbonGroup about = new RibbonGroup("about", "About");
			ButtonCommand aboutButton = Large(new ButtonCommand("about", "About this demo"), "default");
			aboutButton.Click += (sender, e) => ShowAbout();
			about.Commands.Add(aboutButton);
			help.Groups.Add(about);
			_ribbon.Tabs.Add(home);
			_ribbon.Tabs.Add(BuildThemesTab());
			_ribbon.Tabs.Add(help);
			_ribbon.SelectedTab = home;
		}

		private static ButtonCommand Large(ButtonCommand button, string kind)
		{
			button.LargeImage = ShellIcons.Create(kind, 32);
			button.Image = ShellIcons.Create(kind, 16);
			button.SizeStyle = CommandSizeStyle.Large;
			return button;
		}

		// One group for each theme family; the first group switches back to the classic look.
		private RibbonTab BuildThemesTab()
		{
			RibbonTab tab = new RibbonTab("Themes");
			RibbonGroup classic = new RibbonGroup("themeclassic", "Classic") { Image = ShellIcons.Create("default", 32) };
			ButtonCommand classicButton = Large(new ButtonCommand("themeclassicbutton", "Classic look"), "default");
			classicButton.Click += (sender, e) => SetThemedLook(false, null);
			classic.Commands.Add(classicButton);
			tab.Groups.Add(classic);
			tab.Groups.Add(BuildFrameGroup());
			RibbonGroup windows = new RibbonGroup("themewindows", "Windows") { Image = ShellIcons.Create("default", 32) };
			_followWindows = Large(new ButtonCommand("themefollowwindows", "Follow Windows light/dark") { CheckOnClick = true }, "default");
			_followWindows.CheckedChanged += (sender, e) => SetFollowWindows(_followWindows.Checked);
			windows.Commands.Add(_followWindows);
			tab.Groups.Add(windows);
			System.Collections.Generic.Dictionary<string, RibbonGroup> groups = new System.Collections.Generic.Dictionary<string, RibbonGroup>();
			foreach (MxTheme theme in MxThemeManager.RegisteredThemes)
			{
				if (theme.Family == "Standard")
				{
					continue;
				}
				if (!groups.TryGetValue(theme.Family, out RibbonGroup group))
				{
					group = new RibbonGroup("family" + groups.Count, theme.Family) { Image = ShellIcons.CreateThemeSwatch(theme, 32) };
					groups[theme.Family] = group;
					tab.Groups.Add(group);
				}
				MxTheme captured = theme;
				string text = theme.Name.StartsWith(theme.Family) ? theme.Name.Substring(theme.Family.Length).Trim() : theme.Name;
				ButtonCommand button = new ButtonCommand("theme" + groups.Count + captured.Name, text.Length == 0 ? captured.Name : text);
				button.LargeImage = ShellIcons.CreateThemeSwatch(captured, 32);
				button.Image = ShellIcons.CreateThemeSwatch(captured, 16);
				button.SizeStyle = CommandSizeStyle.Large;
				button.Click += (sender, e) => SetThemedLook(true, captured);
				group.Commands.Add(button);
			}
			return tab;
		}

		// Frame style: the caption and the border of the window follow the theme, or stay as Windows draws them.
		private RibbonGroup BuildFrameGroup()
		{
			RibbonGroup group = new RibbonGroup("frame", "Frame style") { Image = ShellIcons.Create("layout", 32) };
			_frameFollow = Large(new ButtonCommand("framefollow", "Follow theme") { CheckOnClick = false, Checked = true }, "layout");
			_frameSystem = Large(new ButtonCommand("framesystem", "System") { CheckOnClick = false }, "default");
			_frameFollow.Click += (sender, e) => SetFrameMode(FormFrameMode.FollowTheme);
			_frameSystem.Click += (sender, e) => SetFrameMode(FormFrameMode.System);
			group.Commands.Add(_frameFollow);
			group.Commands.Add(_frameSystem);
			return group;
		}

		/// <summary>Sets whether the window frame follows the theme.</summary>
		public void SetFrameMode(FormFrameMode mode)
		{
			_frame.Mode = mode;
			_frameFollow.Checked = mode == FormFrameMode.FollowTheme;
			_frameSystem.Checked = mode == FormFrameMode.System;
		}

		private bool _themedLook;

		/// <summary>Switches the whole window to the look of a theme.</summary>
		public void UseThemedLook(MxTheme theme)
		{
			SetThemedLook(true, theme);
		}

		private void SetThemedLook(bool themed, MxTheme theme)
		{
			_changing = true;
			try
			{
				_themedLook = themed;
				if (theme != null)
				{
					MxThemeManager.DefaultTheme = theme;
				}
				else if (!themed)
				{
					MxThemeManager.DefaultTheme = MxThemeManager.Find("Standard") ?? MxThemeManager.DefaultTheme;
				}
				ApplyLook();
				_statusTheme.Text = themed ? MxThemeManager.DefaultTheme.Name : "Classic";
				ShowThemeState();
			}
			finally
			{
				_changing = false;
			}
		}

		private void ShowThemeState()
		{
			_themeStrip.ShowState(_themedLook, MxThemeManager.DefaultTheme, MxThemeManager.FollowWindows);
			if (_followWindows != null && _followWindows.Checked != MxThemeManager.FollowWindows)
			{
				_followWindows.Checked = MxThemeManager.FollowWindows;
			}
		}

		// Following Windows needs a theme of a family with a light and a dark variant, so the classic look moves to Windows 11 first.
		private void SetFollowWindows(bool on)
		{
			if (on && !_themedLook)
			{
				SetThemedLook(true, MxThemeManager.Find("Windows 11 Light"));
			}
			MxThemeManager.FollowWindows = on;
			ShowThemeState();
		}

		// The theme changed from outside this window, for example because Windows switched between light and dark.
		private void OnDefaultThemeChanged(object sender, EventArgs e)
		{
			if (_changing || IsDisposed)
			{
				return;
			}
			if (InvokeRequired)
			{
				if (IsHandleCreated)
				{
					BeginInvoke(new Action(() => OnDefaultThemeChanged(sender, e)));
				}
				return;
			}
			SetThemedLook(true, MxThemeManager.DefaultTheme);
		}

		private void ApplyLook()
		{
			ThemeApplier.Apply(this, MxThemeManager.DefaultTheme, _themedLook);
			_current?.OnLookChanged();
			Invalidate(true);
		}

		private void BuildNavigation()
		{
			_navigation.Dock = DockStyle.Left;
			_navigation.Width = 250;
			ImageList images = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
			string[] kinds = { "grid", "calendar", "nav", "input", "layout", "default" };
			foreach (string kind in kinds)
			{
				images.Images.Add(kind, ShellIcons.Create(kind, 16));
			}
			_navigation.SmallImageList = images;
			Disposed += (sender, e) => images.Dispose();
			string currentGroup = null;
			NavigationGroup group = null;
			foreach (DemoPage page in _pages)
			{
				if (page.Group != currentGroup)
				{
					currentGroup = page.Group;
					group = new NavigationGroup(currentGroup);
					group.View = NavigationBarView.SmallIcons;
					group.ImageIndex = Math.Max(0, Array.IndexOf(kinds, ShellIcons.KindForGroup(currentGroup)));
					_navigation.Groups.Add(group);
				}
				NavigationItem item = new NavigationItem { Text = page.Title, SmallImageIndex = group.ImageIndex };
				group.Items.Add(item);
				_pageByItem[item] = page;
			}
			_navigation.ItemSelected += (sender, e) =>
			{
				if (_navigation.SelectedItem != null && _pageByItem.TryGetValue(_navigation.SelectedItem, out DemoPage page))
				{
					Show(page);
				}
			};
		}

		private void BuildHeader()
		{
			_title.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 15f, FontStyle.Bold);
			_summary.ForeColor = SystemColors.GrayText;
			_themeStrip.Dock = DockStyle.Right;
			_header.Controls.Add(_summary);
			_header.Controls.Add(_title);
			_header.Controls.Add(_themeStrip);
			_summary.BringToFront();
		}

		private void BuildStatusBar()
		{
			_status.Dock = DockStyle.Bottom;
			_status.Height = 24;
			_statusMessage.Text = "Ready";
			_statusMessage.AutoSize = Mordex.Windows.UI.StatusBar.StatusBarPanelAutoSize.Spring;
			_statusTheme.AutoSize = Mordex.Windows.UI.StatusBar.StatusBarPanelAutoSize.Contents;
			_statusTheme.Text = "";
			_status.Panels.Add(_statusMessage);
			_status.Panels.Add(_statusTheme);
		}

		private void Show(DemoPage page)
		{
			if (_current == page)
			{
				return;
			}
			_host.SuspendLayout();
			if (_current != null)
			{
				_host.Controls.Remove(_current);
			}
			_current = page;
			_host.Controls.Add(page);
			_host.ResumeLayout(true);
			// A page that was shown before may hold the other look, so the look is set each time.
			ThemeApplier.Apply(page, MxThemeManager.DefaultTheme, _themedLook);
			page.OnLookChanged();
			_title.Text = page.Title;
			_summary.Text = page.Summary;
			_statusMessage.Text = page.Group + " / " + page.Title;
			page.OnShown();
		}

		private void Step(int offset)
		{
			int index = _current == null ? 0 : _pages.IndexOf(_current) + offset;
			if (index < 0 || index >= _pages.Count)
			{
				return;
			}
			foreach (KeyValuePair<NavigationItem, DemoPage> entry in _pageByItem)
			{
				if (entry.Value == _pages[index])
				{
					_navigation.SelectedItem = entry.Key;
					return;
				}
			}
		}

		private void ShowAbout()
		{
			Version version = typeof(MxRibbon).Assembly.GetName().Version;
			MessageBox.Show(this, "Mordex Components " + version.ToString(3) + Environment.NewLine + "A demo of the data grid, scheduler, timeline, ribbon, navigation, task pane, date picker and the shared controls." + Environment.NewLine + "All data in this demo is invented.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MxThemeManager.DefaultThemeChanged -= OnDefaultThemeChanged;
				MxThemeManager.FollowWindows = false;
				foreach (DemoPage page in _pages)
				{
					if (page != _current)
					{
						page.Dispose();
					}
				}
			}
			base.Dispose(disposing);
		}
	}
}
