using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	internal class PanelTreeDialog : Form, IDesignDialog
	{
		private ImageList _imageList;

		private TreeView _tvwPanels;

		private Button _btnOK;

		private Button _btnCancel;

		private IContainer _components;

		private bool _isAutoHideMode = true;

		private MxDockManager _panelManager;

		private bool _isUpdatingChecks;

		object IDesignDialog.Value => null;

		internal PanelTreeDialog()
		{
			InitializeControls();
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(PanelTreeDialog));
			_tvwPanels = new TreeView();
			_imageList = new ImageList(_components);
			_btnOK = new Button();
			_btnCancel = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_tvwPanels.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_tvwPanels.CheckBoxes = true;
			_tvwPanels.ImageIndex = 0;
			_tvwPanels.ImageList = _imageList;
			_tvwPanels.Location = new Point(8, 8);
			_tvwPanels.Name = "tvwPanels";
			_tvwPanels.SelectedImageIndex = 0;
			_tvwPanels.ShowRootLines = false;
			_tvwPanels.Size = new Size(328, 208);
			_tvwPanels.TabIndex = 0;
			_tvwPanels.AfterCheck += OnTvwPanelsAfterCheck;
			ImageStrip.Load(_imageList, componentResourceManager, "imageList1.ImageStrip");
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.Location = new Point(168, 224);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(80, 24);
			_btnOK.TabIndex = 1;
			_btnOK.Text = "OK";
			_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.Location = new Point(256, 224);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(80, 24);
			_btnCancel.TabIndex = 2;
			_btnCancel.Text = "Cancel";
			base.AcceptButton = _btnOK;
			base.CancelButton = _btnCancel;
			base.ClientSize = new Size(344, 254);
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_tvwPanels);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "PanelTree";
			Text = "PanelTree";
			base.VisibleChanged += OnControlVisibleChanged;
			ResumeLayout(performLayout: false);
		}

		internal DialogResult ShowAutoHidePanels(MxDockManager uiPanelManager)
		{
			Text = "Auto Hide Panels";
			_tvwPanels.Nodes.Clear();
			_panelManager = uiPanelManager;
			_isAutoHideMode = true;
			if (ShowDialog() == DialogResult.OK)
			{
				ApplyCheckedState(_tvwPanels.Nodes);
				return DialogResult.OK;
			}
			return DialogResult.Cancel;
		}

		internal DialogResult ShowClosedPanels(MxDockManager uiPanelManager)
		{
			Text = "Closed Panels";
			_tvwPanels.Nodes.Clear();
			_panelManager = uiPanelManager;
			_isAutoHideMode = false;
			if (ShowDialog() == DialogResult.OK)
			{
				ApplyCheckedState(_tvwPanels.Nodes);
				return DialogResult.OK;
			}
			return DialogResult.Cancel;
		}

		private void ApplyCheckedState(TreeNodeCollection treeNodeCollection)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				ApplyCheckedState(item.Nodes);
				DockPanelBase uiPanelBase = (DockPanelBase)item.Tag;
				if (_isAutoHideMode)
				{
					uiPanelBase.AutoHide = item.Checked;
				}
				else
				{
					uiPanelBase.Closed = item.Checked;
				}
			}
		}

		private void AddPanelNodes(TreeNodeCollection treeNodeCollection, DockPanelCollection uiPanelCollection)
		{
			foreach (DockPanelBase item in (IEnumerable)uiPanelCollection)
			{
				TreeNode treeNode = new TreeNode(FormatPanelText(item));
				treeNode.Tag = item;
				if (item is MxDockPanelGroup)
				{
					MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)item;
					treeNode.ImageIndex = 1;
					if (!uiPanelGroup.StaticGroup || !_isAutoHideMode)
					{
						AddPanelNodes(treeNode.Nodes, uiPanelGroup.Panels);
					}
				}
				else
				{
					treeNode.ImageIndex = 0;
				}
				if (_isAutoHideMode)
				{
					treeNode.Checked = item.AutoHide;
				}
				else
				{
					treeNode.Checked = item.Closed;
				}
				treeNode.Expand();
				treeNodeCollection.Add(treeNode);
			}
		}

		private string FormatPanelText(DockPanelBase uiPanelBase)
		{
			string text = "";
			if (uiPanelBase is MxDockPanelGroup)
			{
				MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)uiPanelBase;
				text = uiPanelBase.Name + ": {" + uiPanelGroup.GroupStyle;
				if (uiPanelGroup.ParentGroup == null)
				{
					text = text + ", " + uiPanelGroup.DockState;
					if (uiPanelGroup.DockState == PanelDockState.Docked)
					{
						text = text + ", " + uiPanelGroup.DockStyle;
					}
				}
				return text + "}";
			}
			text = uiPanelBase.Name + ": {'" + uiPanelBase.Text + "'";
			if (uiPanelBase.ParentGroup == null)
			{
				text = text + ", " + uiPanelBase.DockState;
				if (uiPanelBase.DockState == PanelDockState.Docked)
				{
					text = text + ", " + uiPanelBase.DockStyle;
				}
			}
			return text + "}";
		}

		private void OnControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (base.Visible && _tvwPanels.Nodes.Count == 0)
			{
				_isUpdatingChecks = true;
				AddPanelNodes(_tvwPanels.Nodes, _panelManager.Panels);
				_isUpdatingChecks = false;
			}
		}

		private void SetCheckedRecursive(TreeNodeCollection treeNodeCollection, bool isChecked)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				item.Checked = isChecked;
				SetCheckedRecursive(item.Nodes, isChecked);
			}
		}

		private void UpdateParentChecked(TreeNode treeNode)
		{
			if (treeNode == null)
			{
				return;
			}
			bool allChildrenChecked = true;
			foreach (TreeNode node in treeNode.Nodes)
			{
				if (!node.Checked)
				{
					allChildrenChecked = false;
					break;
				}
			}
			treeNode.Checked = allChildrenChecked;
			UpdateParentChecked(treeNode.Parent);
		}

		private void OnTvwPanelsAfterCheck(object sender, TreeViewEventArgs eventArgs)
		{
			if (!_isUpdatingChecks)
			{
				_isUpdatingChecks = true;
				SetCheckedRecursive(eventArgs.Node.Nodes, eventArgs.Node.Checked);
				UpdateParentChecked(eventArgs.Node.Parent);
				_isUpdatingChecks = false;
			}
		}
	}
}
