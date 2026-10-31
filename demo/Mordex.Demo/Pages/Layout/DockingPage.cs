using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Demo
{
	public sealed class DockingPage : DemoPage
	{
		private readonly Panel _host = new Panel { BackColor = SystemColors.AppWorkspace };
		private readonly MxDockManager _manager = new MxDockManager();
		private readonly MxDockPanelGroup _left;
		private readonly MxDockPanel _explorer = new MxDockPanel("Explorer", PanelDockStyle.Left);
		private readonly MxDockPanel _toolbox = new MxDockPanel("Toolbox", PanelDockStyle.Left);
		private readonly MxDockPanel _properties = new MxDockPanel("Properties", PanelDockStyle.Right);
		private readonly MxDockPanel _output = new MxDockPanel("Output", PanelDockStyle.Bottom);
		private readonly MxDockPanel _documents = new MxDockPanel("Documents", PanelDockStyle.Fill);
		private readonly MxTabControl _tabs = new MxTabControl { Dock = DockStyle.Fill, ShowCloseButton = true };
		private readonly TreeView _tree = new TreeView { Dock = DockStyle.Fill, BorderStyle = System.Windows.Forms.BorderStyle.None, HideSelection = false };
		private readonly PropertyGrid _grid = new PropertyGrid { Dock = DockStyle.Fill, HelpVisible = false, ToolbarVisible = false };
		private readonly ListBox _log = new ListBox { Dock = DockStyle.Fill, BorderStyle = System.Windows.Forms.BorderStyle.None, IntegralHeight = false };

		public DockingPage()
		{
			BuildTree();
			BuildToolbox();
			foreach (string file in new[] { "Program.cs", "Orders.cs", "Readme.txt" })
			{
				_tabs.TabPages.Add(CreateDocument(file));
			}
			_grid.SelectedObject = new FileInfoView("Program.cs", 4180, "C# source");
			_tree.AfterSelect += (s, e) =>
			{
				if (e.Node?.Tag is FileInfoView view)
				{
					_grid.SelectedObject = view;
				}
			};
			_tree.NodeMouseDoubleClick += (s, e) => OpenDocument(e.Node);

			_documents.InnerContainer.Controls.Add(_tabs);
			_explorer.InnerContainer.Controls.Add(_tree);
			_properties.InnerContainer.Controls.Add(_grid);
			_output.InnerContainer.Controls.Add(_log);

			_explorer.Size = new Size(230, 300);
			_toolbox.Size = new Size(230, 300);
			_properties.Size = new Size(260, 300);
			_output.Size = new Size(300, 170);

			_manager.ContainerControl = _host;
			_left = new MxDockPanelGroup(PanelGroupStyle.Tab) { DockStyle = PanelDockStyle.Left };
			_manager.Panels.Add(_documents);
			_manager.Panels.Add(_left);
			_left.Panels.Add(_toolbox);
			_left.Panels.Add(_explorer);
			_manager.Panels.Add(_properties);
			_manager.Panels.Add(_output);
			_left.Size = new Size(230, 300);

			_manager.PanelAutoHideChanged += (s, e) => Write(e.Panel.Text + (e.Panel.AutoHide ? " hidden at the edge" : " pinned"));
			_manager.PanelDockStateChanged += (s, e) => Write(e.Panel.Text + " is now " + e.Panel.DockState.ToString().ToLowerInvariant());
			_manager.PanelClosedChanged += (s, e) => Write(e.Panel.Text + (e.Panel.Closed ? " closed" : " opened"));
			_manager.PanelActivated += (s, e) => Write(e.Panel.Text + " activated");

			OptionsPanel options = new OptionsPanel("Options");
			options.AddChoice("Docking guides", new[] { DockingStyleMode.Standard, DockingStyleMode.VS2005 }, DockingStyleMode.Standard, mode => _manager.DockingStyleMode = mode);
			options.AddToggle("Drag panels to move them", true, on => _manager.AllowPanelDrag = on);
			options.AddToggle("Drop panels onto others", true, on => _manager.AllowPanelDrop = on);
			options.AddToggle("Resize with the splitter", true, on => _manager.AllowPanelResize = on);
			options.AddToggle("Animate auto-hide", true, on => _manager.AllowAutoHideAnimation = on);
			options.AddButton("Auto-hide Properties", () => _properties.AutoHide = !_properties.AutoHide);
			options.AddButton("Float Output", () =>
			{
				if (_output.DockState == PanelDockState.Docked)
				{
					_output.UndockPanel();
				}
				else
				{
					_output.DockPanel();
				}
			});
			options.AddButton("Show or hide Output", () => _output.Closed = !_output.Closed);
			options.AddNote("Drag a panel caption to dock it somewhere else. Use the pin button to hide a panel at the edge.");

			Compose(_host, options);
			Write("Layout ready");
		}

		public override string Title => "Docking";

		public override string Group => "Layout";

		public override string Summary => "An editor-style layout: Explorer and Toolbox in tabs, Properties on the right, Output at the bottom and documents in the middle. Panels dock, float and auto-hide.";

		public override int Order => 20;

		private void BuildTree()
		{
			TreeNode project = new TreeNode("Demo project") { Tag = new FileInfoView("Demo project", 0, "Folder") };
			TreeNode source = project.Nodes.Add("Source");
			source.Tag = new FileInfoView("Source", 0, "Folder");
			foreach ((string name, int size, string kind) file in new[] { ("Program.cs", 4180, "C# source"), ("Orders.cs", 12750, "C# source"), ("Customers.cs", 9320, "C# source"), ("Readme.txt", 860, "Text file") })
			{
				TreeNode node = source.Nodes.Add(file.name);
				node.Tag = new FileInfoView(file.name, file.size, file.kind);
			}
			TreeNode data = project.Nodes.Add("Data");
			data.Tag = new FileInfoView("Data", 0, "Folder");
			foreach ((string name, int size, string kind) file in new[] { ("orders.csv", 52400, "CSV file"), ("settings.json", 1210, "JSON file") })
			{
				TreeNode node = data.Nodes.Add(file.name);
				node.Tag = new FileInfoView(file.name, file.size, file.kind);
			}
			_tree.Nodes.Add(project);
			project.ExpandAll();
		}

		private void BuildToolbox()
		{
			ListBox tools = new ListBox { Dock = DockStyle.Fill, BorderStyle = System.Windows.Forms.BorderStyle.None, IntegralHeight = false };
			tools.Items.AddRange(new object[] { "Pointer", "Label", "Button", "Text box", "Check box", "Combo box", "List box", "Tab control", "Grid" });
			_toolbox.InnerContainer.Controls.Add(tools);
		}

		private static MxTabPage CreateDocument(string name)
		{
			MxTabPage page = new MxTabPage(name) { AllowClose = true };
			TextBox editor = new TextBox
			{
				Dock = DockStyle.Fill,
				Multiline = true,
				ScrollBars = ScrollBars.Both,
				BorderStyle = System.Windows.Forms.BorderStyle.None,
				Font = new Font(FontFamily.GenericMonospace, 10f),
				WordWrap = false,
				Text = "// " + name + "\r\nnamespace Demo\r\n{\r\n\tpublic static class Program\r\n\t{\r\n\t\tpublic static void Main()\r\n\t\t{\r\n\t\t\tConsole.WriteLine(\"Hello\");\r\n\t\t}\r\n\t}\r\n}"
			};
			editor.Disposed += (sender, e) => editor.Font.Dispose();
			page.Controls.Add(editor);
			return page;
		}

		private void OpenDocument(TreeNode node)
		{
			if (node == null || node.Nodes.Count > 0)
			{
				return;
			}
			foreach (MxTabPage page in _tabs.TabPages)
			{
				if (page.Text == node.Text)
				{
					_tabs.SelectedTab = page;
					return;
				}
			}
			MxTabPage opened = CreateDocument(node.Text);
			_tabs.TabPages.Add(opened);
			_tabs.SelectedTab = opened;
		}

		private void Write(string text)
		{
			_log.Items.Add(DateTime.Now.ToString("HH:mm:ss") + "  " + text);
			_log.TopIndex = Math.Max(0, _log.Items.Count - 1);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_manager.Dispose();
			}
			base.Dispose(disposing);
		}

		private sealed class FileInfoView
		{
			public FileInfoView(string name, int size, string kind)
			{
				Name = name;
				Size = size;
				Kind = kind;
			}

			[Category("General")]
			public string Name { get; }

			[Category("General")]
			public string Kind { get; }

			[Category("General")]
			public int Size { get; }

			[Category("General")]
			public DateTime Modified { get; } = DateTime.Today.AddDays(-3);
		}
	}
}
