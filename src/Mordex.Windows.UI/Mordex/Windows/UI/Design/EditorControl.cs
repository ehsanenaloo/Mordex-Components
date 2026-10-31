using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[ToolboxItem(false)]
	internal class EditorControl : UserControl, IUIElement
	{
		private IContainer _components;

		private PropertyGrid _properties;

		private ImageList _imageList;

		private object _selectedObject;

		private EventHandler _disableCancelHandler;


		private EventHandler _dataChangedHandler;

		private static object _selectedObjectChangedKey = new object();

		private static object _refreshTreeKey = new object();

		object IUIElement.SelectedObject
		{
			get
			{
				return _selectedObject;
			}
			set
			{
				_selectedObject = value;
				ShowSelectedObject();
			}
		}

		Image IUIElement.ItemImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Combine(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Remove(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Combine(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler currentHandler;
				do
				{
					currentHandler = eventHandler;
					EventHandler newHandler = (EventHandler)Delegate.Remove(currentHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, newHandler, currentHandler);
				}
				while ((object)eventHandler != currentHandler);
			}
		}

		event EventHandler IUIElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_selectedObjectChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedObjectChangedKey, value);
			}
		}

		event EventHandler IUIElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_refreshTreeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_refreshTreeKey, value);
			}
		}

		internal EditorControl()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
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
			ResourceManager resourceManager = new ResourceManager(typeof(EditorControl));
			_imageList = new ImageList(_components);
			_properties = new PropertyGrid();
			SuspendLayout();
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
			_properties.PropertyValueChanged += OnPropertiesPropertyValueChanged;
			base.Controls.AddRange(new Control[1] { _properties });
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "ObjectDesigner";
			base.Size = new Size(456, 392);
			ResumeLayout(performLayout: false);
		}

		private void ShowSelectedObject()
		{
			_properties.SelectedObject = _selectedObject;
			_properties.Enabled = IsSelectionEditable();
			_properties.Invalidate();
			_properties.Update();
		}

		private bool IsSelectionEditable()
		{
			if (_selectedObject is MxCommand)
			{
				return ((MxCommand)_selectedObject).CanModifySelf();
			}
			if (_selectedObject is MxCommandBar)
			{
				return ((MxCommandBar)_selectedObject).IsEditableInDesigner();
			}
			if (_selectedObject is MxContextMenu)
			{
				return ((MxContextMenu)_selectedObject).IsNotInheritedReadOnly();
			}
			return true;
		}

		bool IUIElement.CommitChanges()
		{
			return true;
		}

		void IUIElement.SetSite(ISite site)
		{
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		private void NotifyIdentityChanged()
		{
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void RaiseRefreshTree(EventArgs eventArgs)
		{
			if (base.Events[_refreshTreeKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
			string displayName = eventArgs.ChangedItem.PropertyDescriptor.DisplayName;
			if (displayName == "Key" || displayName == "Name" || displayName == "IsEntityPanel" || displayName == "Site")
			{
				NotifyIdentityChanged();
			}
		}
	}
}
