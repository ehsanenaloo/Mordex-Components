using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class TaskPaneStateStyles : IMordexXmlLayoutsSupport
	{
		internal ITaskPaneOwner _owner;

		private TaskPaneFormatStyle _formatStyle;

		private TaskPaneFormatStyle _hotFormatStyle;

		private TaskPaneFormatStyle _disabledFormatStyle;

		private TaskPaneFormatStyle _selectedFormatStyle;

		private TaskPaneFormatStyle _pressedFormatStyle;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the items.")]
		public TaskPaneFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new TaskPaneFormatStyle(_owner);
				}
				return _formatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the items when the mouse is over them.")]
		public TaskPaneFormatStyle HotFormatStyle
		{
			get
			{
				if (_hotFormatStyle == null)
				{
					_hotFormatStyle = new TaskPaneFormatStyle(_owner);
				}
				return _hotFormatStyle;
			}
		}

		[Description("Represent the format style used to draw the disabled items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TaskPaneFormatStyle DisabledFormatStyle
		{
			get
			{
				if (_disabledFormatStyle == null)
				{
					_disabledFormatStyle = new TaskPaneFormatStyle(_owner);
				}
				return _disabledFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the selected items.")]
		public TaskPaneFormatStyle SelectedFormatStyle
		{
			get
			{
				if (_selectedFormatStyle == null)
				{
					_selectedFormatStyle = new TaskPaneFormatStyle(_owner);
				}
				return _selectedFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the items when they are pressed or when the item is selected and the mouse is over them.")]
		public TaskPaneFormatStyle PressedFormatStyle
		{
			get
			{
				if (_pressedFormatStyle == null)
				{
					_pressedFormatStyle = new TaskPaneFormatStyle(_owner);
				}
				return _pressedFormatStyle;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			reader["DisabledFormatStyle"]?.DeserializeObject(DisabledFormatStyle);
			reader["FormatStyle"]?.DeserializeObject(FormatStyle);
			reader["HotFormatStyle"]?.DeserializeObject(HotFormatStyle);
			reader["PressedFormatStyle"]?.DeserializeObject(PressedFormatStyle);
			reader["SelectedFormatStyle"]?.DeserializeObject(SelectedFormatStyle);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_disabledFormatStyle != null && !_disabledFormatStyle.IsDefault())
			{
				info.AddValue("DisabledFormatStyle", _disabledFormatStyle);
			}
			if (_formatStyle != null && !_formatStyle.IsDefault())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_hotFormatStyle != null && !_hotFormatStyle.IsDefault())
			{
				info.AddValue("HotFormatStyle", _hotFormatStyle);
			}
			if (_pressedFormatStyle != null && !_pressedFormatStyle.IsDefault())
			{
				info.AddValue("PressedFormatStyle", _pressedFormatStyle);
			}
			if (_selectedFormatStyle != null && !_selectedFormatStyle.IsDefault())
			{
				info.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
		}

		internal TaskPaneStateStyles(ITaskPaneOwner owner)
		{
			_owner = owner;
		}

		internal TaskPaneStateStyles(ITaskPaneOwner owner, TaskPaneFormatStyle formatStyle, TaskPaneFormatStyle hotFormatStyle, TaskPaneFormatStyle selectedFormatStyle)
			: this(owner)
		{
			if (formatStyle != null)
			{
				_formatStyle = formatStyle;
			}
			if (hotFormatStyle != null)
			{
				_hotFormatStyle = hotFormatStyle;
			}
			if (selectedFormatStyle != null)
			{
				_selectedFormatStyle = selectedFormatStyle;
			}
		}

		public TaskPaneStateStyles()
		{
		}

		internal TaskPaneStateStyles(TaskPaneStateStyles baseStyles)
		{
			_owner = null;
			_selectedFormatStyle = new TaskPaneFormatStyle(baseStyles.SelectedFormatStyle);
			_disabledFormatStyle = new TaskPaneFormatStyle(baseStyles.DisabledFormatStyle);
			_formatStyle = new TaskPaneFormatStyle(baseStyles.FormatStyle);
			_hotFormatStyle = new TaskPaneFormatStyle(baseStyles.HotFormatStyle);
			_pressedFormatStyle = new TaskPaneFormatStyle(baseStyles.PressedFormatStyle);
		}

		internal TaskPaneStateStyles MergeWith(TaskPaneStateStyles overrideStyles)
		{
			TaskPaneStateStyles taskPaneStateStyles = new TaskPaneStateStyles(this);
			if (!overrideStyles.SelectedFormatStyle.IsDefault())
			{
				taskPaneStateStyles._selectedFormatStyle = taskPaneStateStyles.SelectedFormatStyle.MergeWith(overrideStyles.SelectedFormatStyle);
			}
			if (!overrideStyles.PressedFormatStyle.IsDefault())
			{
				taskPaneStateStyles._pressedFormatStyle = taskPaneStateStyles.PressedFormatStyle.MergeWith(overrideStyles.PressedFormatStyle);
			}
			if (!overrideStyles.DisabledFormatStyle.IsDefault())
			{
				taskPaneStateStyles._disabledFormatStyle = taskPaneStateStyles.DisabledFormatStyle.MergeWith(overrideStyles.DisabledFormatStyle);
			}
			if (!overrideStyles.FormatStyle.IsDefault())
			{
				taskPaneStateStyles._formatStyle = taskPaneStateStyles.FormatStyle.MergeWith(overrideStyles.FormatStyle);
			}
			if (!overrideStyles.HotFormatStyle.IsDefault())
			{
				taskPaneStateStyles._hotFormatStyle = taskPaneStateStyles.HotFormatStyle.MergeWith(overrideStyles.HotFormatStyle);
			}
			return taskPaneStateStyles;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_disabledFormatStyle != null && !_disabledFormatStyle.IsDefault())
			{
				return true;
			}
			if (_formatStyle != null && !_formatStyle.IsDefault())
			{
				return true;
			}
			if (_hotFormatStyle != null && !_hotFormatStyle.IsDefault())
			{
				return true;
			}
			if (_pressedFormatStyle != null && !_pressedFormatStyle.IsDefault())
			{
				return true;
			}
			if (_selectedFormatStyle != null && !_selectedFormatStyle.IsDefault())
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			if (_disabledFormatStyle != null)
			{
				_disabledFormatStyle.Reset();
			}
			if (_formatStyle != null)
			{
				_formatStyle.Reset();
			}
			if (_hotFormatStyle != null)
			{
				_hotFormatStyle.Reset();
			}
			if (_pressedFormatStyle != null)
			{
				_pressedFormatStyle.Reset();
			}
			if (_selectedFormatStyle != null)
			{
				_selectedFormatStyle.Reset();
			}
			if (_owner != null && _owner.TaskPane != null)
			{
				_owner.TaskPane.InvalidateLayout();
			}
		}

		internal void DisposeDrawingObjects()
		{
			if (_formatStyle != null)
			{
				_formatStyle.DisposeDrawingObjects();
			}
			if (_hotFormatStyle != null)
			{
				_hotFormatStyle.DisposeDrawingObjects();
			}
			if (_selectedFormatStyle != null)
			{
				_selectedFormatStyle.DisposeDrawingObjects();
			}
			if (_disabledFormatStyle != null)
			{
				_disabledFormatStyle.DisposeDrawingObjects();
			}
		}
	}
}
