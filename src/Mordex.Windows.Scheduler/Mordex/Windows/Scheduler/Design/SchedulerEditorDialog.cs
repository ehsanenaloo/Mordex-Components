using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Mordex.Windows.Scheduler.Design
{
	[ToolboxItem(false)]
	internal class SchedulerEditorDialog : Form
	{
		private IContainer _iContainer;

		private Panel _panel;

		private TabControl _tabControl;

		private TabPage _tabGeneral;

		private TabPage _tabLayouts;

		private Button _btnOK;

		private Button _btnCancel;

		private ImageList _imageList;

		private MxScheduler _scheduler;

		private TabPage _tabPage;

		private ISchedulerEditorPage _activeEditor;

		internal SchedulerEditorDialog()
		{
			InitializeControls();
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			ResourceManager resourceManager = new ResourceManager(typeof(SchedulerEditorDialog));
			_panel = new Panel();
			_btnCancel = new Button();
			_btnOK = new Button();
			_tabControl = new TabControl();
			_tabGeneral = new TabPage();
			_tabLayouts = new TabPage();
			_imageList = new ImageList(_iContainer);
			_panel.SuspendLayout();
			_tabControl.SuspendLayout();
			SuspendLayout();
			_panel.Controls.AddRange(new Control[2] { _btnCancel, _btnOK });
			_panel.Dock = DockStyle.Bottom;
			_panel.Location = new Point(4, 350);
			_panel.Name = "panel1";
			_panel.Size = new Size(640, 44);
			_panel.TabIndex = 2;
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
			_tabControl.Controls.AddRange(new Control[2] { _tabGeneral, _tabLayouts });
			_tabControl.Dock = DockStyle.Fill;
			_tabControl.ImageList = _imageList;
			_tabControl.Location = new Point(4, 4);
			_tabControl.Multiline = true;
			_tabControl.Name = "tabControl1";
			_tabControl.SelectedIndex = 0;
			_tabControl.Size = new Size(640, 346);
			_tabControl.TabIndex = 3;
			_tabControl.SelectedIndexChanged += OnTabControl1SelectedIndexChanged;
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
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(648, 398);
			base.Controls.AddRange(new Control[2] { _tabControl, _panel });
			base.DockPadding.All = 4;
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "SchedulerDesignerDialog";
			_panel.ResumeLayout(performLayout: false);
			_tabControl.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		internal void UpdateSchedule(MxScheduler scheduler)
		{
			if (scheduler == null)
			{
				throw new ArgumentNullException();
			}
			_scheduler = scheduler;
			Text = "Schedule Control Designer - " + scheduler.Name;
			UpdateTabPage(_tabControl.SelectedTab);
			ShowDialog();
		}

		private void UpdateTabPage(TabPage tabPage)
		{
			if (_tabPage == tabPage)
			{
				return;
			}
			if (_activeEditor != null && !_activeEditor.CommitChanges())
			{
				_tabControl.SelectedTab = _tabPage;
				return;
			}
			_tabPage = tabPage;
			string name;
			if ((name = _tabPage.Name) != null)
			{
				if (name == "tabLayouts")
				{
					SetActiveEditor(new SchedulerLayoutsEditorControl());
					return;
				}
				if (name == "tabGeneral")
				{
					SetActiveEditor(new SchedulerTreeEditorPage());
					return;
				}
			}
			SetActiveEditor(null);
		}

		private void OnTabControl1SelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			UpdateTabPage(_tabControl.SelectedTab);
		}

		private void SetActiveEditor(ISchedulerEditorPage activeEditor)
		{
			if (_activeEditor != activeEditor)
			{
				if (_activeEditor != null)
				{
					_activeEditor.SelectTab -= OnStateSelectTab;
					((Control)_activeEditor).Dispose();
				}
				_activeEditor = activeEditor;
				if (_activeEditor != null)
				{
					((Control)_activeEditor).Dock = DockStyle.Fill;
					_activeEditor.Scheduler = _scheduler;
					_activeEditor.SelectTab += OnStateSelectTab;
					_tabControl.SelectedTab.Controls.Add((Control)_activeEditor);
				}
			}
		}

		private void OnStateSelectTab(object sender, EventArgs eventArgs)
		{
			_tabControl.SelectedTab = _tabGeneral;
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			base.OnFormClosing(eventArgs);
			if (base.DialogResult != DialogResult.Cancel && _activeEditor != null && !_activeEditor.CommitChanges())
			{
				eventArgs.Cancel = true;
			}
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\Schedule Control");
				_ = Rectangle.Empty;
				registryKey.SetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5));
				registryKey.SetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5));
				registryKey.SetValue("Top", base.Top);
				registryKey.SetValue("Left", base.Left);
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
				RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Mordex Systems\\Schedule Control");
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
					empty.Width = LogicalToDeviceUnits((int)registryKey.GetValue("Width", (int)(base.Width * 96.0 / DeviceDpi + 0.5)));
					empty.Height = LogicalToDeviceUnits((int)registryKey.GetValue("Height", (int)(base.Height * 96.0 / DeviceDpi + 0.5)));
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
			catch
			{
			}
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
