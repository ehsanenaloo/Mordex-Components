using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	/// <summary>Saves the layout of the grid to a file or to memory, shows the XML, and loads or resets it.</summary>
	public sealed class LayoutFilesPage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly System.Data.DataTable _employees;
		private readonly ListBox _snapshots = new ListBox { Dock = DockStyle.Fill, BorderStyle = System.Windows.Forms.BorderStyle.None, IntegralHeight = false };
		private readonly TextBox _xml = PageParts.CodeBox("");
		private readonly Label _status = PageKit.Value("Change the grid, then save the layout.");
		private readonly List<byte[]> _saved = new List<byte[]>();
		private readonly byte[] _defaultLayout;

		public LayoutFilesPage()
		{
			_employees = DemoData.Employees();
			_grid = GridDemo.CreateGrid(_employees);
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.GroupByBoxVisible = true;
			_grid.RootTable.SortKeys.Add("Name");
			_grid.Dock = DockStyle.Fill;
			_defaultLayout = CaptureLayout();

			TableLayoutPanel layout = PageKit.Grid(3, 58f, 42f);
			layout.Controls.Add(_grid, 0, 0);
			layout.SetColumnSpan(_grid, 3);
			Mordex.Windows.UI.Controls.MxGroupBox snapshotGroup = PageKit.Group("Layouts kept in memory");
			_status.Dock = DockStyle.Bottom;
			snapshotGroup.Controls.Add(_snapshots);
			snapshotGroup.Controls.Add(_status);
			_snapshots.BringToFront();
			Mordex.Windows.UI.Controls.MxGroupBox xmlGroup = PageKit.Group("The layout as XML");
			xmlGroup.Controls.Add(_xml);
			layout.Controls.Add(snapshotGroup, 0, 1);
			layout.Controls.Add(xmlGroup, 1, 1);
			layout.Controls.Add(PageParts.CodeGroup("Code", "using (var stream = File.Create(path))\n    grid.SaveLayoutFile(stream);\n\nusing (var stream = File.OpenRead(path))\n    grid.LoadLayoutFile(stream);\n\nGridLayout layout = grid.GetLayout();\ngrid.LoadLayout(layout);"), 2, 1);

			OptionsPanel options = new OptionsPanel();
			options.AddNote("Move, resize, sort or group the columns, then save the layout.");
			options.AddCaption("In memory");
			options.AddButton("Keep this layout", () =>
			{
				_saved.Add(CaptureLayout());
				_snapshots.Items.Add("Layout " + _saved.Count + " at " + DateTime.Now.ToLongTimeString());
				_snapshots.SelectedIndex = _snapshots.Items.Count - 1;
				_status.Text = "Kept layout " + _saved.Count + ".";
			});
			options.AddButton("Restore the selected layout", () =>
			{
				if (_snapshots.SelectedIndex < 0)
				{
					_status.Text = "Select a kept layout first.";
					return;
				}
				Restore(_saved[_snapshots.SelectedIndex]);
				_status.Text = "Restored " + _snapshots.SelectedItem + ".";
			});
			options.AddButton("Reset to the default layout", () =>
			{
				Restore(_defaultLayout);
				_status.Text = "The default layout is back.";
			});
			options.AddCaption("In a file");
			options.AddButton("Save to a file...", SaveFile);
			options.AddButton("Load from a file...", LoadFile);
			options.AddButton("Show the layout as XML", ShowXml);
			Compose(layout, options);
			ShowXml();
		}

		private byte[] CaptureLayout()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				_grid.SaveLayoutFile(stream);
				return stream.ToArray();
			}
		}

		private void Restore(byte[] layout)
		{
			using (MemoryStream stream = new MemoryStream(layout))
			{
				_grid.LoadLayoutFile(stream);
			}
			ShowXml();
		}

		private void ShowXml()
		{
			string text = Encoding.UTF8.GetString(CaptureLayout());
			_xml.Text = text.Length > 20000 ? text.Substring(0, 20000) + "\r\n..." : text;
		}

		private void SaveFile()
		{
			using (SaveFileDialog dialog = new SaveFileDialog { Filter = "Grid layout (*.xml)|*.xml", FileName = "employees-layout.xml" })
			{
				if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
				{
					return;
				}
				try
				{
					File.WriteAllBytes(dialog.FileName, CaptureLayout());
					_status.Text = "Saved " + Path.GetFileName(dialog.FileName);
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
				{
					MessageBox.Show(FindForm(), "The file could not be written: " + ex.Message, "Layout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		private void LoadFile()
		{
			using (OpenFileDialog dialog = new OpenFileDialog { Filter = "Grid layout (*.xml)|*.xml" })
			{
				if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
				{
					return;
				}
				try
				{
					Restore(File.ReadAllBytes(dialog.FileName));
					_status.Text = "Loaded " + Path.GetFileName(dialog.FileName);
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Xml.XmlException)
				{
					MessageBox.Show(FindForm(), "The layout could not be read: " + ex.Message, "Layout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_employees.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Saving and loading layouts";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Save the column layout to an XML file or to memory, read the XML, load it again, or reset the grid to its first layout.";

		public override int Order => 72;
	}
}
