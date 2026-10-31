using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	[ToolboxItem(false)]
	internal class RibbonPropertyEditorControl : UserControl, IRibbonItemEditor
	{
		private PropertyGrid _properties;

		private object _editedObject;

		private EventHandler _disableCancelHandler;

		private EventHandler _dataChangedHandler;

		private static object s_selectedObjectChangedKey = new object();

		private static object s_refreshTreeKey = new object();

		object IRibbonItemEditor.SelectedObject
		{
			get
			{
				return _editedObject;
			}
			set
			{
				_editedObject = value;
				BindEditedObject();
			}
		}

		Image IRibbonItemEditor.ItemImage
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
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		event EventHandler IRibbonItemEditor.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(s_selectedObjectChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectedObjectChangedKey, value);
			}
		}

		event EventHandler IRibbonItemEditor.RefreshTree
		{
			add
			{
				base.Events.AddHandler(s_refreshTreeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_refreshTreeKey, value);
			}
		}

		internal RibbonPropertyEditorControl()
		{
			InitializeControls();
			_properties.CommandsVisibleIfAvailable = false;
		}

		private void InitializeControls()
		{
			_properties = new PropertyGrid();
			SuspendLayout();
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

		private void BindEditedObject()
		{
			_properties.SelectedObject = _editedObject;
			_properties.Enabled = CanEditSelectedObject();
			_properties.Invalidate();
			_properties.Update();
		}

		private bool CanEditSelectedObject()
		{
			if (_editedObject is CommandBase commandBase)
			{
				return commandBase.IsEditable();
			}
			if (_editedObject is RibbonTab ribbonTab)
			{
				return ribbonTab.IsEditable();
			}
			if (_editedObject is RibbonContextualTab ribbonContextualTab)
			{
				return ribbonContextualTab.IsEditable();
			}
			return true;
		}

		bool IRibbonItemEditor.CommitChanges()
		{
			return true;
		}

		void IRibbonItemEditor.SetSite(ISite site)
		{
		}

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
			if (_disableCancelHandler != null)
			{
				_disableCancelHandler(this, eventArgs);
			}
		}

		bool IRibbonItemEditor.IsOptionSupported(ToolbarOptions option)
		{
			return false;
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		private void RequestTreeRefresh()
		{
			RaiseRefreshTree(EventArgs.Empty);
		}

		private void RaiseRefreshTree(EventArgs eventArgs)
		{
			if (base.Events[s_refreshTreeKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		private void OnPropertiesPropertyValueChanged(object sender, PropertyValueChangedEventArgs eventArgs)
		{
			OnDataChanged(EventArgs.Empty);
			string displayName = eventArgs.ChangedItem.PropertyDescriptor?.DisplayName;
			if (displayName == "Key" || displayName == "Name" || displayName == "IsEntityPanel" || displayName == "Site")
			{
				RequestTreeRefresh();
			}
		}
	}
}
