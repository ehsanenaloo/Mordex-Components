using Mordex.Windows.Common;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class DataSourceDialog : Form
	{
		private Label _label;

		private ComboBox _comboBox;

		private Label _dataSourceDialogLabel;

		private TreeView _treeDataMember;

		private ImageList _imageList;

		private Button _button;

		private Button _dataSourceDialogButton;

		private IContainer _iContainer;

		private object _selectedDataSource;

		private string _selectedDataMember = "";

		internal DataSourceDialog()
		{
			InitializeControls();
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
			ResourceManager resourceManager = new ResourceManager(typeof(DataSourceDialog));
			_label = new Label();
			_comboBox = new ComboBox();
			_dataSourceDialogLabel = new Label();
			_treeDataMember = new TreeView();
			_imageList = new ImageList(_iContainer);
			_button = new Button();
			_dataSourceDialogButton = new Button();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_label.AutoSize = true;
			_label.Location = new Point(8, 8);
			_label.Name = "label1";
			_label.Size = new Size(66, 14);
			_label.TabIndex = 0;
			_label.Text = "DataSource:";
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.Location = new Point(92, 4);
			_comboBox.Name = "comboBox1";
			_comboBox.Size = new Size(224, 21);
			_comboBox.TabIndex = 1;
			_comboBox.SelectedIndexChanged += OnComboBoxSelectedIndexChanged;
			_dataSourceDialogLabel.AutoSize = true;
			_dataSourceDialogLabel.Location = new Point(8, 36);
			_dataSourceDialogLabel.Name = "label2";
			_dataSourceDialogLabel.Size = new Size(72, 14);
			_dataSourceDialogLabel.TabIndex = 2;
			_dataSourceDialogLabel.Text = "DataMember:";
			_treeDataMember.HideSelection = false;
			_treeDataMember.ImageList = _imageList;
			_treeDataMember.Location = new Point(92, 32);
			_treeDataMember.Name = "treeDataMember";
			_treeDataMember.PathSeparator = ".";
			_treeDataMember.Size = new Size(224, 152);
			_treeDataMember.TabIndex = 3;
			_treeDataMember.AfterSelect += OnTreeDataMemberAfterSelect;
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "icons.ImageStrip");
			_imageList.TransparentColor = Color.Empty;
			_button.DialogResult = DialogResult.OK;
			_button.Location = new Point(144, 196);
			_button.Name = "button1";
			_button.Size = new Size(80, 24);
			_button.TabIndex = 4;
			_button.Text = "OK";
			_button.Click += OnButtonClick;
			_dataSourceDialogButton.DialogResult = DialogResult.Cancel;
			_dataSourceDialogButton.Location = new Point(232, 196);
			_dataSourceDialogButton.Name = "button2";
			_dataSourceDialogButton.Size = new Size(80, 24);
			_dataSourceDialogButton.TabIndex = 5;
			_dataSourceDialogButton.Text = "Cancel";
			_dataSourceDialogButton.Click += OnCloseButtonClick;
			base.AcceptButton = _button;
			base.CancelButton = _dataSourceDialogButton;
			base.ClientSize = new Size(320, 233);
			base.Controls.AddRange(new Control[6] { _dataSourceDialogButton, _button, _treeDataMember, _dataSourceDialogLabel, _label, _comboBox });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Icon = (Icon)resourceManager.GetObject("$this.Icon");
			base.Name = "DataSourceDialog";
			base.StartPosition = FormStartPosition.CenterParent;
			Text = "Select DataSource";
			ResumeLayout(performLayout: false);
		}

		private string GetQualifiedName(IComponent iComponent, string text)
		{
			return iComponent.Site.Name + "." + text;
		}

		internal void ProcessDataGrid(MxDataGrid dataGrid, object value, string text)
		{
			if (dataGrid.Container != null)
			{
				_comboBox.DisplayMember = "Text";
				foreach (IComponent component in dataGrid.Container.Components)
				{
					if (component is IListSource)
					{
						if (typeof(DataSet).IsAssignableFrom(component.GetType()))
						{
							PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
							properties.Sort();
							foreach (PropertyDescriptor item in properties)
							{
								if (typeof(DataTable).IsAssignableFrom(item.PropertyType))
								{
									_comboBox.Items.Add(new GridValueListItem(item.GetValue(component), GetQualifiedName(component, item.Name)));
								}
							}
						}
						_comboBox.Items.Add(new GridValueListItem(component, component.Site.Name));
					}
					else if (component is IList)
					{
						_comboBox.Items.Add(new GridValueListItem(component, component.Site.Name));
					}
				}
				_comboBox.Items.Add(new GridValueListItem(null, "(none)"));
				foreach (GridValueListItem otherItem in _comboBox.Items)
				{
					if (otherItem.Value == value)
					{
						_comboBox.SelectedItem = otherItem;
						break;
					}
				}
				ShowDialog();
				return;
			}
			throw new InvalidOperationException();
		}

		private TreeNode ComputeTreeNode(PropertyDescriptor propertyDescriptor, string text, int imageIndex)
		{
			TreeNode treeNode = new TreeNode(text, imageIndex, imageIndex);
			treeNode.Tag = propertyDescriptor;
			return treeNode;
		}

		private void OnComboBoxSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_treeDataMember.Nodes.Clear();
			if (_comboBox.SelectedItem != null)
			{
				object value = (_selectedDataSource = ((GridValueListItem)_comboBox.SelectedItem).Value);
				if (value != null)
				{
					ProcessTreeNodeCollection(_treeDataMember.Nodes, value);
				}
				TreeNode treeNode = ComputeTreeNode(null, "None", 1);
				_treeDataMember.Nodes.Add(treeNode);
				_treeDataMember.SelectedNode = treeNode;
			}
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
					TreeNode treeNode = ComputeTreeNode(item, item.Name, 0);
					treeNodeCollection.Add(treeNode);
					ProcessTreeNode(treeNode, typedList, new PropertyDescriptor[1] { item });
				}
			}
		}

		private void ProcessTreeNode(TreeNode otherTreeNode, ITypedList iTypedList, PropertyDescriptor[] listAccessors)
		{
			PropertyDescriptorCollection itemProperties = iTypedList.GetItemProperties(listAccessors);
			foreach (PropertyDescriptor item in itemProperties)
			{
				if ((typeof(IList).IsAssignableFrom(item.PropertyType) || typeof(IListSource).IsAssignableFrom(item.PropertyType)) && otherTreeNode.Text != item.Name)
				{
					TreeNode treeNode = ComputeTreeNode(item, item.Name, 0);
					if (!DataSourceEditorControl.IsPropertyInAncestors(otherTreeNode, treeNode))
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

		internal object GetSelectedDataSource()
		{
			return _selectedDataSource;
		}

		internal string GetSelectedDataMember()
		{
			return _selectedDataMember;
		}

		private void OnTreeDataMemberAfterSelect(object sender, TreeViewEventArgs eventArgs)
		{
			if (_treeDataMember.SelectedNode == null)
			{
				_selectedDataMember = "";
				return;
			}
			string text = "";
			for (TreeNode treeNode = _treeDataMember.SelectedNode; treeNode != null; treeNode = treeNode.Parent)
			{
				string otherText = "";
				otherText = ((!(treeNode.Tag is PropertyDescriptor)) ? "" : ((PropertyDescriptor)treeNode.Tag).Name);
				text = ((text.Length != 0) ? (text + "." + otherText) : otherText);
			}
			_selectedDataMember = text;
		}

		private void OnButtonClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnCloseButtonClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
