using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Design
{
	[ToolboxItem(false)]
	internal class DataSourceEditorControl : UserControl, IDesignDialog
	{
		private bool _isTreeOnly;

		private EventHandler _changedHandler;

		private IContainer _iContainer;

		private object _dataSource;

		private string _dataMember;

		private IContainer _gridEditorControl4IContainer;

		private RadioButton _optDataSource;

		private RadioButton _optNoDataSource;

		private ComboBox _cboDataSource;

		private TreeView _treeDataMember;

		private Label _lblDataMember;

		private Label _lblDataSource;

		private ImageList _imageList;

		object IDesignDialog.Value => GetDataMember();

		public DataSourceEditorControl()
		{
			InitializeControls();
		}

		internal void SetDataSourceLabel(string text)
		{
			if (GetDataMemberLabel() != text)
			{
				_lblDataSource.Text = text;
				LayoutControls();
			}
		}

		internal void SetTreeOnlyMode(bool condition)
		{
			if (_isTreeOnly != condition)
			{
				_isTreeOnly = condition;
				if (_isTreeOnly)
				{
					_treeDataMember.BorderStyle = System.Windows.Forms.BorderStyle.None;
				}
				LayoutControls();
			}
		}

		internal string GetDataMemberLabel()
		{
			return _lblDataMember.Text;
		}

		internal void SetDataMemberLabel(string text)
		{
			if (GetDataMemberLabel() != text)
			{
				_lblDataMember.Text = text;
				LayoutControls();
			}
		}

		private void LayoutControls()
		{
			if (_isTreeOnly)
			{
				foreach (Control control in base.Controls)
				{
					if (control != _treeDataMember)
					{
						control.Visible = false;
					}
				}
				_treeDataMember.Bounds = base.ClientRectangle;
				return;
			}
			int width = base.ClientRectangle.Width;
			int right = _lblDataSource.Bounds.Right;
			if (right < _lblDataMember.Bounds.Right)
			{
				right = _lblDataMember.Bounds.Right;
			}
			right += 6;
			_cboDataSource.Left = right;
			_treeDataMember.Left = right;
			if (width > right)
			{
				_cboDataSource.Width = width - right - 4;
				_treeDataMember.Width = _cboDataSource.Width;
			}
			int height = base.ClientRectangle.Height;
			if (height > _treeDataMember.Top)
			{
				_treeDataMember.Height = height - _treeDataMember.Top - 4;
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			LayoutControls();
		}

		internal bool IsSelectionComplete()
		{
			if (_optDataSource.Checked)
			{
				return _dataSource != null;
			}
			return true;
		}

		internal void AddChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _changedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _changedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		private void RaiseChanged()
		{
			if (_changedHandler != null)
			{
				_changedHandler(this, EventArgs.Empty);
			}
		}

		private void OnOptNoDataSourceCheckedChanged(object sender, EventArgs eventArgs)
		{
			_cboDataSource.Enabled = _optDataSource.Checked;
			_treeDataMember.Enabled = _optDataSource.Checked;
			RaiseChanged();
		}

		internal void UpdateIContainer(IContainer iContainer)
		{
			if (_iContainer == iContainer)
			{
				return;
			}
			_iContainer = iContainer;
			_cboDataSource.Items.Clear();
			_cboDataSource.DisplayMember = "Text";
			_cboDataSource.Items.Add(new GridValueListItem(null, "(none)"));
			if (_iContainer != null)
			{
				foreach (IComponent component in _iContainer.Components)
				{
					if (component is IListSource)
					{
						_cboDataSource.Items.Add(new GridValueListItem(component, component.Site.Name));
					}
					else if (component is IList)
					{
						_cboDataSource.Items.Add(new GridValueListItem(component, component.Site.Name));
					}
				}
			}
			SelectCurrentDataSourceItem();
		}

		private TreeNode ComputeTreeNode(TreeNodeCollection treeNodeCollection, string strB)
		{
			foreach (TreeNode item in treeNodeCollection)
			{
				if (string.Compare(item.FullPath, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return item;
				}
				TreeNode treeNode = ComputeTreeNode(item.Nodes, strB);
				if (treeNode != null)
				{
					return treeNode;
				}
			}
			return null;
		}

		private void SelectCurrentDataSourceItem()
		{
			foreach (GridValueListItem item in _cboDataSource.Items)
			{
				if (item.Value == _dataSource)
				{
					_cboDataSource.SelectedItem = item;
					break;
				}
			}
		}

		internal object GetDataSource()
		{
			return _dataSource;
		}

		internal void SetDataSource(object value)
		{
			if (_dataSource != value)
			{
				_dataSource = value;
				SelectCurrentDataSourceItem();
				RaiseChanged();
			}
		}

		internal string GetDataMember()
		{
			return _dataMember;
		}

		internal void SetDataMember(string text)
		{
			if (!(_dataMember != text))
			{
				return;
			}
			_dataMember = text;
			if (_dataMember.Length > 0)
			{
				TreeNode treeNode = ComputeTreeNode(_treeDataMember.Nodes, _dataMember);
				if (treeNode != null)
				{
					_treeDataMember.SelectedNode = treeNode;
				}
			}
			RaiseChanged();
		}

		private void OnCboDataSourceSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_treeDataMember.Nodes.Clear();
			if (_cboDataSource.SelectedItem != null)
			{
				object value = ((GridValueListItem)_cboDataSource.SelectedItem).Value;
				SetDataSource(value);
				if (value != null)
				{
					ProcessTreeNodeCollection(_treeDataMember.Nodes, value);
				}
				TreeNode treeNode = CreatePropertyNode(null, "None", 1);
				_treeDataMember.Nodes.Add(treeNode);
				_treeDataMember.SelectedNode = treeNode;
			}
			else
			{
				SetDataSource(null);
			}
		}

		private TreeNode CreatePropertyNode(PropertyDescriptor propertyDescriptor, string text, int imageIndex)
		{
			TreeNode treeNode = new TreeNode(text, imageIndex, imageIndex);
			treeNode.Tag = propertyDescriptor;
			return treeNode;
		}

		private void ProcessTreeNodeCollection(TreeNodeCollection treeNodeCollection, object value)
		{
			IList list = null;
			if (value is IListSource)
			{
				IListSource listSource = (IListSource)value;
				list = listSource.GetList();
			}
			else if (value is IList)
			{
				list = (IList)value;
			}
			if (list == null || !(list is ITypedList))
			{
				return;
			}
			ITypedList typedList = (ITypedList)list;
			PropertyDescriptorCollection itemProperties = typedList.GetItemProperties(null);
			foreach (PropertyDescriptor item in itemProperties)
			{
				if (typeof(IList).IsAssignableFrom(item.PropertyType) || typeof(IListSource).IsAssignableFrom(item.PropertyType))
				{
					TreeNode treeNode = CreatePropertyNode(item, item.Name, 0);
					treeNodeCollection.Add(treeNode);
					ProcessTreeNode(treeNode, typedList, new PropertyDescriptor[1] { item });
				}
			}
		}

		private void OnTreeDataMemberVisibleChanged(object sender, EventArgs eventArgs)
		{
			int width = _treeDataMember.Width;
			_treeDataMember.Width = 0;
			_treeDataMember.Width = width;
		}

		private void OnTreeDataMemberAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (eventArgs.Node != null)
			{
				SetDataMember(BuildDataMemberPath(eventArgs.Node));
			}
			else
			{
				SetDataMember("");
			}
		}

		private string BuildDataMemberPath(TreeNode treeNode)
		{
			string text = "";
			while (treeNode != null)
			{
				text = ((treeNode.Tag == null) ? "" : ((text.Length != 0) ? (((PropertyDescriptor)treeNode.Tag).Name + "." + text) : ((PropertyDescriptor)treeNode.Tag).Name));
				treeNode = treeNode.Parent;
			}
			return text;
		}

		private void ProcessTreeNode(TreeNode otherTreeNode, ITypedList iTypedList, PropertyDescriptor[] listAccessors)
		{
			PropertyDescriptorCollection itemProperties = iTypedList.GetItemProperties(listAccessors);
			foreach (PropertyDescriptor item in itemProperties)
			{
				if ((typeof(IList).IsAssignableFrom(item.PropertyType) || typeof(IListSource).IsAssignableFrom(item.PropertyType)) && otherTreeNode.Text != item.Name)
				{
					TreeNode treeNode = CreatePropertyNode(item, item.Name, 0);
					if (!IsPropertyInAncestors(otherTreeNode, treeNode))
					{
						otherTreeNode.Nodes.Add(treeNode);
						PropertyDescriptor[] array = new PropertyDescriptor[listAccessors.Length + 1];
						listAccessors.CopyTo(array, 0);
						array[array.Length - 1] = item;
						ProcessTreeNode(treeNode, iTypedList, array);
					}
				}
			}
		}

		internal static bool IsPropertyInAncestors(TreeNode treeNode, TreeNode otherTreeNode)
		{
			if (otherTreeNode.Tag is PropertyDescriptor propertyDescriptor)
			{
				while (treeNode != null)
				{
					if (treeNode.Tag is PropertyDescriptor otherPropertyDescriptor && otherPropertyDescriptor.Name == propertyDescriptor.Name && otherPropertyDescriptor.PropertyType == propertyDescriptor.PropertyType)
					{
						return true;
					}
					treeNode = treeNode.Parent;
				}
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _gridEditorControl4IContainer != null)
			{
				_gridEditorControl4IContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_gridEditorControl4IContainer = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(DataSourceEditorControl));
			_optDataSource = new RadioButton();
			_optNoDataSource = new RadioButton();
			_cboDataSource = new ComboBox();
			_treeDataMember = new TreeView();
			_imageList = new ImageList(_gridEditorControl4IContainer);
			_lblDataMember = new Label();
			_lblDataSource = new Label();
			SuspendLayout();
			_optDataSource.Checked = true;
			_optDataSource.Location = new Point(11, 29);
			_optDataSource.Name = "optDataSource";
			_optDataSource.Size = new Size(376, 27);
			_optDataSource.TabIndex = 15;
			_optDataSource.TabStop = true;
			_optDataSource.Text = "Select a datasource from the components available at design time.";
			_optDataSource.CheckedChanged += OnOptNoDataSourceCheckedChanged;
			_optNoDataSource.Location = new Point(11, 13);
			_optNoDataSource.Name = "optNoDataSource";
			_optNoDataSource.Size = new Size(240, 16);
			_optNoDataSource.TabIndex = 14;
			_optNoDataSource.Text = "DataSource is not available at design time.";
			_optNoDataSource.CheckedChanged += OnOptNoDataSourceCheckedChanged;
			_cboDataSource.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboDataSource.Location = new Point(105, 62);
			_cboDataSource.Name = "cboDataSource";
			_cboDataSource.Size = new Size(330, 21);
			_cboDataSource.TabIndex = 11;
			_cboDataSource.SelectedIndexChanged += OnCboDataSourceSelectedIndexChanged;
			_treeDataMember.HideSelection = false;
			_treeDataMember.ImageIndex = 0;
			_treeDataMember.ImageList = _imageList;
			_treeDataMember.Location = new Point(105, 90);
			_treeDataMember.Name = "treeDataMember";
			_treeDataMember.PathSeparator = ".";
			_treeDataMember.SelectedImageIndex = 0;
			_treeDataMember.Size = new Size(330, 109);
			_treeDataMember.TabIndex = 13;
			_treeDataMember.VisibleChanged += OnTreeDataMemberVisibleChanged;
			_treeDataMember.AfterSelect += OnTreeDataMemberAfterSelect;
			ImageStrip.Load(_imageList, componentResourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Empty;
			_imageList.Images.SetKeyName(0, "");
			_imageList.Images.SetKeyName(1, "");
			_lblDataMember.AutoSize = true;
			_lblDataMember.Location = new Point(27, 90);
			_lblDataMember.Name = "lblDataMember";
			_lblDataMember.Size = new Size(71, 13);
			_lblDataMember.TabIndex = 12;
			_lblDataMember.Text = "DataMember:";
			_lblDataSource.AutoSize = true;
			_lblDataSource.Location = new Point(27, 65);
			_lblDataSource.Name = "lblDataSource";
			_lblDataSource.Size = new Size(67, 13);
			_lblDataSource.TabIndex = 10;
			_lblDataSource.Text = "DataSource:";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(_optDataSource);
			base.Controls.Add(_optNoDataSource);
			base.Controls.Add(_cboDataSource);
			base.Controls.Add(_treeDataMember);
			base.Controls.Add(_lblDataMember);
			base.Controls.Add(_lblDataSource);
			base.Name = "DataSourceControl";
			base.Size = new Size(441, 211);
			ResumeLayout(performLayout: false);
			PerformLayout();
		}
	}
}
