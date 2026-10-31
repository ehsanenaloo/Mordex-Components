using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	[ToolboxItem(false)]
	internal class RibbonContextualTabEditorControl : UserControl
	{
		private RibbonContextualTab _contextualTab;

		private MxRibbon _ribbon;

		private Control _rootControl;

		private IContainer _components = default;

		private LinkLabel _selectAllLink;

		private LinkLabel _clearLink;

		private CheckedListBox _controlsList;

		private Label _lblNoControls;

		private Label _titleLabel;

		internal RibbonContextualTabEditorControl()
		{
			InitializeControls();
		}

		internal RibbonContextualTabEditorControl(RibbonContextualTab contextualTab, Control rootControl)
		{
			InitializeControls();
			_contextualTab = contextualTab;
			_ribbon = contextualTab.Ribbon;
			_rootControl = rootControl;
			if (rootControl == null)
			{
				throw new InvalidOperationException();
			}
			PopulateControlList();
		}

		private void PopulateControlList()
		{
			List<Control> list = new List<Control>();
			CollectControls(list, _rootControl);
			if (list.Count == 0)
			{
				_lblNoControls.Visible = true;
			}
			else
			{
				_lblNoControls.Visible = false;
			}
			_controlsList.Items.Clear();
			foreach (Control item in list)
			{
				_controlsList.Items.Add(item, _contextualTab.Controls.Contains(item));
			}
		}

		private void CollectControls(List<Control> list, Control container)
		{
			if (container != _ribbon && !_ribbon.Controls.Contains(container))
			{
				list.Add(container);
			}
			foreach (Control control in container.Controls)
			{
				CollectControls(list, control);
			}
		}

		private void OnSelectAllLinkClicked(object sender, LinkLabelLinkClickedEventArgs eventArgs)
		{
			for (int i = 0; i < _controlsList.Items.Count; i++)
			{
				_controlsList.SetItemChecked(i, value: true);
			}
		}

		private void OnClearLinkClicked(object sender, LinkLabelLinkClickedEventArgs eventArgs)
		{
			for (int i = 0; i < _controlsList.Items.Count; i++)
			{
				_controlsList.SetItemChecked(i, value: false);
			}
		}

		private void OnControlsListItemCheck(object sender, ItemCheckEventArgs eventArgs)
		{
			if (_controlsList.Items[eventArgs.Index] is Control control)
			{
				if (eventArgs.NewValue == CheckState.Checked)
				{
					_contextualTab.Controls.Add(control);
				}
				else
				{
					_contextualTab.Controls.Remove(control);
				}
			}
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
			_selectAllLink = new LinkLabel();
			_clearLink = new LinkLabel();
			_controlsList = new CheckedListBox();
			_lblNoControls = new Label();
			_titleLabel = new Label();
			SuspendLayout();
			_selectAllLink.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_selectAllLink.AutoSize = true;
			_selectAllLink.Location = new Point(380, 30);
			_selectAllLink.Name = "linkLabel1";
			_selectAllLink.Size = new Size(51, 13);
			_selectAllLink.TabIndex = 1;
			_selectAllLink.TabStop = true;
			_selectAllLink.Text = "Select All";
			_selectAllLink.LinkClicked += OnSelectAllLinkClicked;
			_clearLink.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_clearLink.AutoSize = true;
			_clearLink.Location = new Point(380, 57);
			_clearLink.Name = "linkLabel2";
			_clearLink.Size = new Size(31, 13);
			_clearLink.TabIndex = 2;
			_clearLink.TabStop = true;
			_clearLink.Text = "Clear";
			_clearLink.LinkClicked += OnClearLinkClicked;
			_controlsList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_controlsList.FormattingEnabled = true;
			_controlsList.Location = new Point(0, 30);
			_controlsList.Name = "checkedListBox1";
			_controlsList.Size = new Size(374, 319);
			_controlsList.TabIndex = 3;
			_controlsList.ItemCheck += OnControlsListItemCheck;
			_lblNoControls.BackColor = SystemColors.Window;
			_lblNoControls.Location = new Point(3, 31);
			_lblNoControls.Name = "lblNoControls";
			_lblNoControls.Size = new Size(360, 52);
			_lblNoControls.TabIndex = 4;
			_lblNoControls.Text = "There are no controls in the designer.";
			_lblNoControls.Visible = false;
			_titleLabel.AutoSize = true;
			_titleLabel.Location = new Point(3, 10);
			_titleLabel.Name = "label1";
			_titleLabel.Size = new Size(230, 13);
			_titleLabel.TabIndex = 5;
			_titleLabel.Text = "Select the controls bound to the ContextualTab";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(_titleLabel);
			base.Controls.Add(_lblNoControls);
			base.Controls.Add(_controlsList);
			base.Controls.Add(_clearLink);
			base.Controls.Add(_selectAllLink);
			base.Name = "ControlsManagedEditor";
			base.Size = new Size(434, 354);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}
	}
}
