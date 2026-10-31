using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Mordex.Windows.Ribbon.Design
{
	[ToolboxItem(false)]
	internal class StatusBarDesignerDialog : Form
	{
		private const string PlacementKeyPath = "Software\\Mordex Systems\\Ribbon Control";

		private const double BaseDpi = 96.0;

		private IContainer _components;

		private bool _isModified;

		private MemoryStream _layoutSnapshot;

		private Panel _buttonPanel;

		private TabControl _pageTabs;

		private TabPage _tabGeneral;

		private TabPage _tabLayouts;

		private Button _btnOK;

		private Button _btnCancel;

		private ImageList _tabImages;

		private MxRibbon _ribbon;

		private MxRibbonStatusBar _ribbonStatusBar;

		private TabPage _currentTabPage;

		private IRibbonDesignerPage _currentPage;

		internal StatusBarDesignerDialog()
		{
			InitializeControls();
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_components?.Dispose();
				_layoutSnapshot?.Dispose();
				_layoutSnapshot = null;
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(StatusBarDesignerDialog));
			_buttonPanel = new Panel();
			_btnCancel = new Button();
			_btnOK = new Button();
			_pageTabs = new TabControl();
			_tabGeneral = new TabPage();
			_tabLayouts = new TabPage();
			_tabImages = new ImageList(_components);
			_buttonPanel.SuspendLayout();
			_pageTabs.SuspendLayout();
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
			_pageTabs.Controls.Add(_tabGeneral);
			_pageTabs.Controls.Add(_tabLayouts);
			_pageTabs.Dock = DockStyle.Fill;
			_pageTabs.ImageList = _tabImages;
			_pageTabs.Location = new Point(4, 4);
			_pageTabs.Multiline = true;
			_pageTabs.Name = "tabControl1";
			_pageTabs.SelectedIndex = 0;
			_pageTabs.Size = new Size(640, 346);
			_pageTabs.TabIndex = 3;
			_pageTabs.SelectedIndexChanged += OnPageTabsSelectedIndexChanged;
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
			ImageStrip.Load(_tabImages, componentResourceManager, "icons.ImageStrip");
			_tabImages.Images.SetKeyName(0, "");
			_tabImages.Images.SetKeyName(1, "");
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(648, 398);
			base.Controls.Add(_pageTabs);
			base.Controls.Add(_buttonPanel);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "RibbonDesignerDialog";
			base.Padding = new Padding(4);
			base.ShowInTaskbar = false;
			_buttonPanel.ResumeLayout(performLayout: false);
			_pageTabs.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}

		internal DialogResult ShowStatusBarDesignerDialog(MxRibbonStatusBar ribbonStatusBar)
		{
			try
			{
				if (ribbonStatusBar == null)
				{
					throw new ArgumentNullException(nameof(ribbonStatusBar));
				}
				_ribbonStatusBar = ribbonStatusBar;
				Text = "StatusBar Designer - ";
				if (_ribbonStatusBar.Site != null)
				{
					Text += ribbonStatusBar.Site.Name;
				}
				SetCurrentTabPage(_pageTabs.SelectedTab);
				_pageTabs.TabPages.Remove(_tabLayouts);
				_btnCancel.Enabled = false;
				return ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return DialogResult.None;
			}
		}

		internal DialogResult ShowRibbonDesignerDialog(MxRibbon ribbon)
		{
			try
			{
				if (ribbon == null)
				{
					throw new ArgumentNullException();
				}
				_ribbon = ribbon;
				if (_ribbon.Site != null)
				{
					Text = "Ribbon Designer - " + ribbon.Site.Name;
				}
				else
				{
					Text = "Ribbon Designer - ";
				}
				SetCurrentTabPage(_pageTabs.SelectedTab);
				SaveLayoutSnapshot();
				return ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return DialogResult.None;
			}
		}

		private void SaveLayoutSnapshot()
		{
			_layoutSnapshot?.Dispose();
			_layoutSnapshot = new MemoryStream();
			_ribbon.SaveLayoutFile(_layoutSnapshot);
			_layoutSnapshot.Position = 0L;
			_isModified = false;
		}

		private void RestoreLayoutSnapshot()
		{
			if (_layoutSnapshot != null)
			{
				_layoutSnapshot.Position = 0L;
				if (_ribbon != null)
				{
					_ribbon.LoadLayoutFile(_layoutSnapshot);
					_layoutSnapshot.Close();
					_layoutSnapshot = null;
					SaveLayoutSnapshot();
				}
			}
		}

		private void SetCurrentTabPage(TabPage tabPage)
		{
			if (_currentTabPage == tabPage || tabPage == null)
			{
				return;
			}
			if (_currentPage != null && !_currentPage.CommitChanges())
			{
				_pageTabs.SelectedTab = _currentTabPage;
				return;
			}
			_currentTabPage = tabPage;
			switch (_currentTabPage.Name)
			{
			case "tabLayouts":
				SetCurrentPage(new RibbonLayoutEditorControl());
				break;
			case "tabGeneral":
				SetCurrentPage(new RibbonDesignerControl());
				break;
			default:
				SetCurrentPage(null);
				break;
			}
		}

		private void OnPageTabsSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			SetCurrentTabPage(_pageTabs.SelectedTab);
		}

		private void SetCurrentPage(IRibbonDesignerPage iRibbonElement)
		{
			if (_currentPage == iRibbonElement)
			{
				return;
			}
			if (_currentPage != null)
			{
				_currentPage.DisableCancel -= OnStateDisableCancel;
				_currentPage.SelectTab -= OnStateSelectTab;
				_currentPage.DataChanged -= OnStateDataChanged;
				((Control)_currentPage).Dispose();
			}
			_currentPage = iRibbonElement;
			if (_currentPage != null)
			{
				((Control)_currentPage).Dock = DockStyle.Fill;
				if (_ribbon != null)
				{
					_currentPage.Ribbon = _ribbon;
				}
				else
				{
					_currentPage.StatusBar = _ribbonStatusBar;
				}
				_currentPage.DisableCancel += OnStateDisableCancel;
				_currentPage.SelectTab += OnStateSelectTab;
				_currentPage.DataChanged += OnStateDataChanged;
				_pageTabs.SelectedTab.Controls.Add((Control)_currentPage);
			}
		}

		private void OnStateSelectTab(object sender, EventArgs eventArgs)
		{
			_pageTabs.SelectedTab = _tabGeneral;
		}

		private void OnStateDisableCancel(object sender, EventArgs eventArgs)
		{
			_btnCancel.Enabled = false;
		}

		private void OnStateDataChanged(object sender, EventArgs eventArgs)
		{
			_isModified = true;
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			_isModified = false;
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			base.OnFormClosing(eventArgs);
			if (base.DialogResult != DialogResult.Cancel && _currentPage != null)
			{
				bool wasModified = _isModified;
				if (!_currentPage.CommitChanges())
				{
					eventArgs.Cancel = true;
				}
				_isModified = wasModified;
			}
			if (_isModified && _btnCancel.Enabled)
			{
				switch (MessageBox.Show(this, "Save Changes?", "Ribbon Designer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
				{
				case DialogResult.Cancel:
					eventArgs.Cancel = true;
					break;
				case DialogResult.No:
					RestoreLayoutSnapshot();
					break;
				default:
					base.DialogResult = DialogResult.OK;
					break;
				}
			}
			if (!eventArgs.Cancel)
			{
				SavePlacement();
			}
		}

		private int ToLogicalUnits(int deviceUnits)
		{
			return (int)(deviceUnits * BaseDpi / DeviceDpi + 0.5);
		}

		private static int ReadInt(RegistryKey key, string name, int fallback)
		{
			return key.GetValue(name) is int value ? value : fallback;
		}

		private void SavePlacement()
		{
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(PlacementKeyPath);
				if (registryKey == null)
				{
					return;
				}
				if (base.WindowState == FormWindowState.Normal)
				{
					registryKey.SetValue("Width", ToLogicalUnits(base.Width));
					registryKey.SetValue("Height", ToLogicalUnits(base.Height));
					registryKey.SetValue("Top", base.Top);
					registryKey.SetValue("Left", base.Left);
				}
				registryKey.SetValue("Window State", base.WindowState.ToString());
			}
			catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
			{
				// Remembering the window position is optional, so a blocked registry is ignored.
			}
		}

		private void RestorePlacement()
		{
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(PlacementKeyPath);
				if (registryKey == null)
				{
					return;
				}
				string text = registryKey.GetValue("Window State") as string ?? nameof(FormWindowState.Normal);
				if (text == nameof(FormWindowState.Maximized))
				{
					base.WindowState = FormWindowState.Maximized;
					return;
				}
				if (text != nameof(FormWindowState.Normal))
				{
					return;
				}
				base.WindowState = FormWindowState.Normal;
				Rectangle bounds = new Rectangle(
					ReadInt(registryKey, "Left", base.Left),
					ReadInt(registryKey, "Top", base.Top),
					LogicalToDeviceUnits(ReadInt(registryKey, "Width", ToLogicalUnits(base.Width))),
					LogicalToDeviceUnits(ReadInt(registryKey, "Height", ToLogicalUnits(base.Height))));
				Rectangle virtualScreen = SystemInformation.VirtualScreen;
				if (bounds.Right > virtualScreen.Right)
				{
					bounds.X = virtualScreen.Right - bounds.Width;
				}
				if (bounds.X < virtualScreen.Left)
				{
					bounds.X = virtualScreen.Left;
				}
				if (bounds.Bottom > virtualScreen.Bottom)
				{
					bounds.Y = Math.Max(virtualScreen.Top, virtualScreen.Bottom - bounds.Height);
				}
				if (bounds.Bottom > virtualScreen.Bottom)
				{
					bounds.Height = virtualScreen.Bottom - bounds.Y;
				}
				base.Bounds = bounds;
			}
			catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException)
			{
				// Remembering the window position is optional, so a blocked registry is ignored.
			}
		}

		protected override void OnLoad(EventArgs eventArgs)
		{
			base.OnLoad(eventArgs);
			RestorePlacement();
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			RestoreLayoutSnapshot();
			Close();
		}
	}
}
