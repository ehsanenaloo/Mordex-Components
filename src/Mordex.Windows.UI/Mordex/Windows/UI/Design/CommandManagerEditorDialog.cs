using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Microsoft.Win32;

namespace Mordex.Windows.UI.Design
{
	[ToolboxItem(false)]
	internal class CommandManagerEditorDialog : Form
	{
		private const string SettingsKeyPath = "Software\\Mordex Systems\\CommandManager Control";

		private const double ReferenceDpi = 96.0;

		private IContainer _components;

		private bool _hasChanges;

		private MemoryStream _snapshotStream;

		private Panel _buttonPanel;

		private TabControl _tabControl;

		private TabPage _tabGeneral;

		private TabPage _tabLayouts;

		private Button _btnOK;

		private Button _btnCancel;

		private ImageList _imageList;

		private MxCommandManager _commandManager;

		private MxDockManager _panelManager;

		private TabPage _activeTabPage;

		private IManagerEditorControl _pageEditor;

		internal CommandManagerEditorDialog()
		{
			InitializeControls();
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(CommandManagerEditorDialog));
			_buttonPanel = new Panel();
			_btnCancel = new Button();
			_btnOK = new Button();
			_tabControl = new TabControl();
			_tabGeneral = new TabPage();
			_tabLayouts = new TabPage();
			_imageList = new ImageList(_components);
			_buttonPanel.SuspendLayout();
			_tabControl.SuspendLayout();
			SuspendLayout();
			_buttonPanel.Controls.Add(_btnCancel);
			_buttonPanel.Controls.Add(_btnOK);
			_buttonPanel.Dock = DockStyle.Bottom;
			_buttonPanel.Location = new Point(4, 350);
			_buttonPanel.Name = "panel1";
			_buttonPanel.Size = new Size(640, 44);
			_buttonPanel.TabIndex = 2;
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(544, 12);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 1;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(456, 12);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 0;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_tabControl.Controls.Add(_tabGeneral);
			_tabControl.Controls.Add(_tabLayouts);
			_tabControl.Dock = DockStyle.Fill;
			_tabControl.ImageList = _imageList;
			_tabControl.Location = new Point(4, 4);
			_tabControl.Multiline = true;
			_tabControl.Name = "tabControl1";
			_tabControl.SelectedIndex = 0;
			_tabControl.Size = new Size(640, 346);
			_tabControl.TabIndex = 3;
			_tabControl.SelectedIndexChanged += OnTabControlSelectedIndexChanged;
			_tabGeneral.ImageIndex = 0;
			_tabGeneral.Location = new Point(4, 23);
			_tabGeneral.Name = "tabGeneral";
			_tabGeneral.Size = new Size(632, 319);
			_tabGeneral.TabIndex = 0;
			_tabGeneral.Text = "General Properties";
			_tabLayouts.ImageIndex = 1;
			_tabLayouts.Location = new Point(4, 23);
			_tabLayouts.Name = "tabLayouts";
			_tabLayouts.Size = new Size(632, 319);
			_tabLayouts.TabIndex = 1;
			_tabLayouts.Text = "Layout Manager";
			_tabLayouts.Visible = false;
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(648, 398);
			base.Controls.Add(_tabControl);
			base.Controls.Add(_buttonPanel);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "CommandBarsDesignerDialog";
			base.Padding = new Padding(4);
			base.ShowInTaskbar = false;
			_buttonPanel.ResumeLayout(performLayout: false);
			_tabControl.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		internal DialogResult ShowPanelManagerEditor(MxDockManager uiPanelManager)
		{
			try
			{
				if (uiPanelManager == null)
				{
					throw new ArgumentNullException();
				}
				_panelManager = uiPanelManager;
				Text = "PanelManager Designer - " + uiPanelManager.Site.Name;
				ActivateTabPage(_tabControl.SelectedTab);
				SavePanelSnapshot();
				return ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return DialogResult.None;
			}
		}

		internal void ShowCommandManagerEditor(MxCommandManager uiCommandManager)
		{
			if (uiCommandManager == null)
			{
				throw new ArgumentNullException();
			}
			_commandManager = uiCommandManager;
			Text = "CommandManager Designer - " + uiCommandManager.Site.Name;
			ActivateTabPage(_tabControl.SelectedTab);
			SaveCommandSnapshot();
			ShowDialog();
		}

		private void SaveCommandSnapshot()
		{
			_snapshotStream = new MemoryStream();
			_commandManager.SaveLayoutFile(_snapshotStream);
			_snapshotStream.Position = 0L;
			_hasChanges = false;
		}

		private void SavePanelSnapshot()
		{
			_snapshotStream = new MemoryStream();
			_panelManager.SaveLayoutFile(_snapshotStream);
			_snapshotStream.Position = 0L;
			_hasChanges = false;
		}

		private void RestoreSnapshot()
		{
			if (_snapshotStream != null)
			{
				_snapshotStream.Position = 0L;
				if (_panelManager != null)
				{
					_panelManager.LoadLayoutFile(_snapshotStream);
					_snapshotStream.Close();
					_snapshotStream = null;
					SavePanelSnapshot();
				}
				else
				{
					_commandManager.LoadLayoutFile(_snapshotStream);
					_snapshotStream.Close();
					_snapshotStream = null;
					SaveCommandSnapshot();
				}
			}
		}

		private void ActivateTabPage(TabPage tabPage)
		{
			if (_activeTabPage == tabPage)
			{
				return;
			}
			if (_pageEditor != null && !_pageEditor.CommitChanges())
			{
				_tabControl.SelectedTab = _activeTabPage;
				return;
			}
			_activeTabPage = tabPage;
			string name;
			if ((name = _activeTabPage.Name) != null)
			{
				if (name == "tabLayouts")
				{
					SetPageEditor(new LayoutEditorControl());
					return;
				}
				if (name == "tabGeneral")
				{
					SetPageEditor(new DockManagerEditorControl());
					return;
				}
			}
			SetPageEditor(null);
		}

		private void OnTabControlSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			ActivateTabPage(_tabControl.SelectedTab);
		}

		private void SetPageEditor(IManagerEditorControl pageEditor)
		{
			if (_pageEditor == pageEditor)
			{
				return;
			}
			if (_pageEditor != null)
			{
				_pageEditor.DisableCancel -= OnPageEditorDisableCancel;
				_pageEditor.SelectTab -= OnPageEditorSelectTab;
				_pageEditor.DataChanged -= OnPageEditorDataChanged;
				((Control)_pageEditor).Dispose();
			}
			_pageEditor = pageEditor;
			if (_pageEditor != null)
			{
				((Control)_pageEditor).Dock = DockStyle.Fill;
				if (_panelManager != null)
				{
					_pageEditor.PanelManager = _panelManager;
				}
				else
				{
					_pageEditor.CommandManager = _commandManager;
				}
				_pageEditor.DisableCancel += OnPageEditorDisableCancel;
				_pageEditor.SelectTab += OnPageEditorSelectTab;
				_pageEditor.DataChanged += OnPageEditorDataChanged;
				_tabControl.SelectedTab.Controls.Add((Control)_pageEditor);
			}
		}

		private void OnPageEditorSelectTab(object sender, EventArgs eventArgs)
		{
			_tabControl.SelectedTab = _tabGeneral;
		}

		private void OnPageEditorDisableCancel(object sender, EventArgs eventArgs)
		{
			_btnCancel.Enabled = false;
		}

		private void OnPageEditorDataChanged(object sender, EventArgs eventArgs)
		{
			_hasChanges = true;
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			try
			{
				_hasChanges = false;
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			base.OnFormClosing(eventArgs);
			if (base.DialogResult != DialogResult.Cancel && _pageEditor != null)
			{
				bool hadChanges = _hasChanges;
				if (!_pageEditor.CommitChanges())
				{
					eventArgs.Cancel = true;
				}
				_hasChanges = hadChanges;
			}
			if (_hasChanges && _btnCancel.Enabled)
			{
				switch (MessageBox.Show("Save Changes?", "CommandBars Designer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
				{
				case DialogResult.Cancel:
					eventArgs.Cancel = true;
					break;
				case DialogResult.No:
					RestoreSnapshot();
					break;
				default:
					base.DialogResult = DialogResult.OK;
					break;
				}
			}
			try
			{
				if (_commandManager != null)
				{
					foreach (MxCommandBar commandBar in _commandManager.CommandBars)
					{
						commandBar.SetCustomizeSelectedCommand(null);
						commandBar.SetDropIndex(-1);
						commandBar.SetChildMenu(null);
					}
				}
				using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
				if (base.WindowState == FormWindowState.Normal)
				{
					registryKey.SetValue("Width", (int)(base.Width * ReferenceDpi / DeviceDpi + 0.5));
					registryKey.SetValue("Height", (int)(base.Height * ReferenceDpi / DeviceDpi + 0.5));
					registryKey.SetValue("Top", base.Top);
					registryKey.SetValue("Left", base.Left);
				}
				registryKey.SetValue("Window State", base.WindowState);
			}
			catch
			{
			}
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
				string text = (string)registryKey.GetValue("Window State", FormWindowState.Normal);
				if (text == "Normal")
				{
					base.WindowState = FormWindowState.Normal;
				}
				else if (text == "Maximized")
				{
					base.WindowState = FormWindowState.Maximized;
				}
				if (base.WindowState != FormWindowState.Minimized)
				{
					Rectangle empty = Rectangle.Empty;
					empty.Width = LogicalToDeviceUnits((int)registryKey.GetValue("Width", (int)(base.Width * ReferenceDpi / DeviceDpi + 0.5)));
					empty.Height = LogicalToDeviceUnits((int)registryKey.GetValue("Height", (int)(base.Height * ReferenceDpi / DeviceDpi + 0.5)));
					empty.Y = (int)registryKey.GetValue("Top", base.Top);
					empty.X = (int)registryKey.GetValue("Left", base.Left);
					if (empty.Width > SystemInformation.VirtualScreen.Width)
					{
						empty.Width = SystemInformation.VirtualScreen.Width;
					}
					if (empty.Height > SystemInformation.VirtualScreen.Height)
					{
						empty.Height = SystemInformation.VirtualScreen.Height;
					}
					if (empty.Right > SystemInformation.VirtualScreen.Right)
					{
						empty.X = SystemInformation.VirtualScreen.Right - empty.Width;
					}
					if (empty.X < SystemInformation.VirtualScreen.Left)
					{
						empty.X = SystemInformation.VirtualScreen.Left;
					}
					if (empty.Bottom > SystemInformation.VirtualScreen.Bottom)
					{
						empty.Y = SystemInformation.VirtualScreen.Bottom - empty.Height;
					}
					if (empty.Y < SystemInformation.VirtualScreen.Top)
					{
						empty.Y = SystemInformation.VirtualScreen.Top;
					}
					base.Bounds = empty;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			RestoreSnapshot();
			Close();
		}
	}
}
