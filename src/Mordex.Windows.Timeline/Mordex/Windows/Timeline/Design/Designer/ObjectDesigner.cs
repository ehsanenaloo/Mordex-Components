using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Timeline.Design.Designer
{
	[ToolboxItem(false)]
	public class ObjectDesigner : UserControl, ITimelineElement
	{
		private IContainer _iContainer;

		private MxToolBar _tbCommands;

		private PropertyGrid _properties;

		private ImageList _imageList;

		private object _selectedObject;

		private ToolbarOptions _toolbarOptions = default;

		private static object _selectedObjectChangedEventKey = new object();

		private static object _refreshTreeEventKey = new object();

		object ITimelineElement.SelectedObject
		{
			get
			{
				return _selectedObject;
			}
			set
			{
				_selectedObject = value;
				UpdateObjectDesigner();
			}
		}

		Image ITimelineElement.ItemImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		event EventHandler ITimelineElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_selectedObjectChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedObjectChangedEventKey, value);
			}
		}

		event EventHandler ITimelineElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_refreshTreeEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_refreshTreeEventKey, value);
			}
		}

		internal ObjectDesigner()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
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
			ResourceManager resourceManager = new ResourceManager(typeof(ObjectDesigner));
			_tbCommands = new MxToolBar();
			_imageList = new ImageList(_iContainer);
			_properties = new PropertyGrid();
			SuspendLayout();
			_tbCommands.Appearance = MxToolBarAppearance.Flat;
			_tbCommands.DropDownArrows = true;
			_tbCommands.ImageList = _imageList;
			_tbCommands.Name = "tbCommands";
			_tbCommands.ShowToolTips = true;
			_tbCommands.Size = new Size(456, 25);
			_tbCommands.TabIndex = 0;
			_tbCommands.TextAlign = MxToolBarTextAlign.Right;
			_tbCommands.ButtonClick += OnTbCommandsButtonClick;
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(16, 16);
			ImageStrip.Load(_imageList, resourceManager, "imageList1.ImageStrip");
			_imageList.TransparentColor = Color.Transparent;
			_properties.CommandsVisibleIfAvailable = true;
			_properties.Dock = DockStyle.Fill;
			_properties.LargeButtons = false;
			_properties.LineColor = SystemColors.Control;
			_properties.Location = new Point(0, 25);
			_properties.Name = "properties";
			_properties.PropertySort = PropertySort.Alphabetical;
			_properties.Size = new Size(456, 367);
			_properties.TabIndex = 1;
			_properties.Text = "propertyGrid1";
			_properties.ToolbarVisible = false;
			_properties.ViewBackColor = SystemColors.Window;
			_properties.ViewForeColor = SystemColors.WindowText;
			base.Controls.AddRange(new Control[2] { _properties, _tbCommands });
			base.Name = "ObjectDesigner";
			base.Size = new Size(456, 392);
			ResumeLayout(performLayout: false);
		}

		private void UpdateObjectDesigner()
		{
			_properties.SelectedObject = _selectedObject;
			_tbCommands.Visible = false;
			_tbCommands.Buttons.Clear();
			MxToolBarButton[] buttons = null;
			if (_selectedObject is MxTimeline)
			{
				buttons = new MxToolBarButton[2]
				{
					new MxToolBarButton("Reset Properties"),
					null
				};
				buttons[0].Tag = "ResetTimelineProperties";
				buttons[0].ImageIndex = 2;
				buttons[1] = new MxToolBarButton("Retrieve Fields");
				buttons[1].Tag = "RetrieveFlatStructure";
				buttons[1].ImageIndex = 1;
				_tbCommands.Buttons.AddRange(buttons);
				_tbCommands.Visible = true;
			}
		}

		bool ITimelineElement.CommitChanges()
		{
			return true;
		}

		void ITimelineElement.SetSite(ISite site)
		{
		}

		void ITimelineElement.Unload()
		{
		}

		bool ITimelineElement.Support(ToolbarOptions option)
		{
			return (_toolbarOptions & option) != 0;
		}

		private void OnTbCommandsButtonClick(object sender, MxToolBarButtonClickEventArgs eventArgs)
		{
			string text;
			if ((text = (string)eventArgs.Button.Tag) == null)
			{
				return;
			}
			if (!(text == "ResetTimelineProperties"))
			{
				if (text == "RetrieveFlatStructure")
				{
					RetrieveFieldsWithConfirmation((MxTimeline)_selectedObject);
				}
			}
			else if (MessageBox.Show("Are you sure you want to lose all the changes in the properties of the control?", "Timeline Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
			{
				((MxTimeline)_selectedObject).Reset();
				RaiseRefreshTree();
			}
		}

		private void RetrieveFieldsWithConfirmation(MxTimeline timeline)
		{
			if (timeline.Fields.Count <= 0 || MessageBox.Show("This operation will overwrite the field layout you currently have. \n\r\n\rDo you want to continue?", "Mordex Timeline Control", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.No)
			{
				timeline.RetrieveFields();
				RaiseRefreshTree();
			}
		}

		private void RaiseRefreshTree()
		{
			InvokeRefreshTreeHandlers(EventArgs.Empty);
		}

		private void InvokeRefreshTreeHandlers(EventArgs eventArgs)
		{
			if (base.Events[_refreshTreeEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}
	}
}
