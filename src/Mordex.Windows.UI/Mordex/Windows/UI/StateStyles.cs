using System.ComponentModel;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class StateStyles : IMordexXmlLayoutsSupport
	{
		internal IFormatStyleOwner _owner;

		private FormatStyle _formatStyle;

		private FormatStyle _hotFormatStyle;

		private FormatStyle _disabledFormatStyle;

		private FormatStyle _selectedFormatStyle;

		private FormatStyle _pressedFormatStyle;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Represent the format style used to draw the items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new FormatStyle(_owner);
				}
				return _formatStyle;
			}
		}

		[Description("Represent the format style used to draw the items when the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle HotFormatStyle
		{
			get
			{
				if (_hotFormatStyle == null)
				{
					_hotFormatStyle = new FormatStyle(_owner);
				}
				return _hotFormatStyle;
			}
		}

		[Description("Represent the format style used to draw the disabled items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle DisabledFormatStyle
		{
			get
			{
				if (_disabledFormatStyle == null)
				{
					_disabledFormatStyle = new FormatStyle(_owner);
				}
				return _disabledFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the selected items.")]
		public FormatStyle SelectedFormatStyle
		{
			get
			{
				if (_selectedFormatStyle == null)
				{
					_selectedFormatStyle = new FormatStyle(_owner);
				}
				return _selectedFormatStyle;
			}
		}

		[Description("Represent the format style used to draw the items when they are pressed or when the item is selected and the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle PressedFormatStyle
		{
			get
			{
				if (_pressedFormatStyle == null)
				{
					_pressedFormatStyle = new FormatStyle(_owner);
				}
				return _pressedFormatStyle;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_disabledFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "DisabledFormatStyle", typeof(FormatStyle), null);
			if (_disabledFormatStyle != null)
			{
				_disabledFormatStyle._owner = _owner;
			}
			_formatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "FormatStyle", typeof(FormatStyle), null);
			if (_formatStyle != null)
			{
				_formatStyle._owner = _owner;
			}
			_hotFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "HotFormatStyle", typeof(FormatStyle), null);
			if (_hotFormatStyle != null)
			{
				_hotFormatStyle._owner = _owner;
			}
			_pressedFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "PressedFormatStyle", typeof(FormatStyle), null);
			if (_pressedFormatStyle != null)
			{
				_pressedFormatStyle._owner = _owner;
			}
			_selectedFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "SelectedFormatStyle", typeof(FormatStyle), null);
			if (_selectedFormatStyle != null)
			{
				_selectedFormatStyle._owner = _owner;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_disabledFormatStyle != null && !_disabledFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("DisabledFormatStyle", _disabledFormatStyle);
			}
			if (_formatStyle != null && !_formatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_hotFormatStyle != null && !_hotFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("HotFormatStyle", _hotFormatStyle);
			}
			if (_pressedFormatStyle != null && !_pressedFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("PressedFormatStyle", _pressedFormatStyle);
			}
			if (_selectedFormatStyle != null && !_selectedFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
		}

		internal StateStyles(IFormatStyleOwner owner)
		{
			_owner = owner;
		}

		public StateStyles()
		{
		}

		internal StateStyles(StateStyles baseStyles)
		{
			_owner = null;
			_selectedFormatStyle = new FormatStyle(baseStyles.SelectedFormatStyle);
			_disabledFormatStyle = new FormatStyle(baseStyles.DisabledFormatStyle);
			_formatStyle = new FormatStyle(baseStyles.FormatStyle);
			_hotFormatStyle = new FormatStyle(baseStyles.HotFormatStyle);
			_pressedFormatStyle = new FormatStyle(baseStyles.PressedFormatStyle);
		}

		internal StateStyles CreateMergedStateStyles(StateStyles overrideStyles)
		{
			StateStyles uiStateStyles = new StateStyles(this);
			if (!overrideStyles.SelectedFormatStyle.IsDefaultStyle())
			{
				uiStateStyles._selectedFormatStyle = uiStateStyles.SelectedFormatStyle.MergeStyle(overrideStyles.SelectedFormatStyle);
			}
			if (!overrideStyles.PressedFormatStyle.IsDefaultStyle())
			{
				uiStateStyles._pressedFormatStyle = uiStateStyles.PressedFormatStyle.MergeStyle(overrideStyles.PressedFormatStyle);
			}
			if (!overrideStyles.DisabledFormatStyle.IsDefaultStyle())
			{
				uiStateStyles._disabledFormatStyle = uiStateStyles.DisabledFormatStyle.MergeStyle(overrideStyles.DisabledFormatStyle);
			}
			if (!overrideStyles.FormatStyle.IsDefaultStyle())
			{
				uiStateStyles._formatStyle = uiStateStyles.FormatStyle.MergeStyle(overrideStyles.FormatStyle);
			}
			if (!overrideStyles.HotFormatStyle.IsDefaultStyle())
			{
				uiStateStyles._hotFormatStyle = uiStateStyles.HotFormatStyle.MergeStyle(overrideStyles.HotFormatStyle);
			}
			return uiStateStyles;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_disabledFormatStyle != null && !_disabledFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_formatStyle != null && !_formatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_hotFormatStyle != null && !_hotFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_pressedFormatStyle != null && !_pressedFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_selectedFormatStyle != null && !_selectedFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_disabledFormatStyle = null;
			_formatStyle = null;
			_hotFormatStyle = null;
			_pressedFormatStyle = null;
			_selectedFormatStyle = null;
			if (_owner != null)
			{
				_owner.PropertyChange();
			}
		}

		internal void DisposeFormatStyleResources()
		{
			if (_formatStyle != null)
			{
				_formatStyle.DisposeResources();
			}
			if (_hotFormatStyle != null)
			{
				_hotFormatStyle.DisposeResources();
			}
			if (_selectedFormatStyle != null)
			{
				_selectedFormatStyle.DisposeResources();
			}
			if (_disabledFormatStyle != null)
			{
				_disabledFormatStyle.DisposeResources();
			}
		}
	}
}
