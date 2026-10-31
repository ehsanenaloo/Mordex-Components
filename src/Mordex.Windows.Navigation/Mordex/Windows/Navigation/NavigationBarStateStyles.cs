using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class NavigationBarStateStyles : IMordexXmlLayoutsSupport
	{
		internal INavigationBarOwner _owner;

		private NavigationBarFormatStyle _formatStyle;

		private NavigationBarFormatStyle _hotFormatStyle;

		private NavigationBarFormatStyle _disabledFormatStyle;

		private NavigationBarFormatStyle _selectedFormatStyle;

		private NavigationBarFormatStyle _pressedFormatStyle;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Represent the format style used to draw the items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new NavigationBarFormatStyle(_owner);
				}
				return _formatStyle;
			}
		}

		[Description("Represent the format style used to draw the items when the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarFormatStyle HotFormatStyle
		{
			get
			{
				if (_hotFormatStyle == null)
				{
					_hotFormatStyle = new NavigationBarFormatStyle(_owner);
				}
				return _hotFormatStyle;
			}
		}

		[Description("Represent the format style used to draw the disabled items.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public NavigationBarFormatStyle DisabledFormatStyle
		{
			get
			{
				if (_disabledFormatStyle == null)
				{
					_disabledFormatStyle = new NavigationBarFormatStyle(_owner);
				}
				return _disabledFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the selected items.")]
		public NavigationBarFormatStyle SelectedFormatStyle
		{
			get
			{
				if (_selectedFormatStyle == null)
				{
					_selectedFormatStyle = new NavigationBarFormatStyle(_owner);
				}
				return _selectedFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represent the format style used to draw the items when they are pressed or when the item is selected and the mouse is over them.")]
		public NavigationBarFormatStyle PressedFormatStyle
		{
			get
			{
				if (_pressedFormatStyle == null)
				{
					_pressedFormatStyle = new NavigationBarFormatStyle(_owner);
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
			if (_disabledFormatStyle != null && !_disabledFormatStyle.IsEmptyStyle())
			{
				info.AddValue("DisabledFormatStyle", _disabledFormatStyle);
			}
			if (_formatStyle != null && !_formatStyle.IsEmptyStyle())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_hotFormatStyle != null && !_hotFormatStyle.IsEmptyStyle())
			{
				info.AddValue("HotFormatStyle", _hotFormatStyle);
			}
			if (_pressedFormatStyle != null && !_pressedFormatStyle.IsEmptyStyle())
			{
				info.AddValue("PressedFormatStyle", _pressedFormatStyle);
			}
			if (_selectedFormatStyle != null && !_selectedFormatStyle.IsEmptyStyle())
			{
				info.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
		}

		internal NavigationBarStateStyles(INavigationBarOwner owner)
		{
			_owner = owner;
		}

		internal NavigationBarStateStyles(INavigationBarOwner owner, NavigationBarFormatStyle formatStyle, NavigationBarFormatStyle hotFormatStyle, NavigationBarFormatStyle selectedFormatStyle)
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

		public NavigationBarStateStyles()
		{
		}

		internal NavigationBarStateStyles(NavigationBarStateStyles baseStyles)
		{
			_owner = null;
			_selectedFormatStyle = new NavigationBarFormatStyle(baseStyles.SelectedFormatStyle);
			_disabledFormatStyle = new NavigationBarFormatStyle(baseStyles.DisabledFormatStyle);
			_formatStyle = new NavigationBarFormatStyle(baseStyles.FormatStyle);
			_hotFormatStyle = new NavigationBarFormatStyle(baseStyles.HotFormatStyle);
			_pressedFormatStyle = new NavigationBarFormatStyle(baseStyles.PressedFormatStyle);
		}

		internal NavigationBarStateStyles MergeWith(NavigationBarStateStyles overrideStyles)
		{
			NavigationBarStateStyles navigationBarStateStyles = new NavigationBarStateStyles(this);
			if (!overrideStyles.SelectedFormatStyle.IsEmptyStyle())
			{
				navigationBarStateStyles._selectedFormatStyle = navigationBarStateStyles.SelectedFormatStyle.MergeWith(overrideStyles.SelectedFormatStyle);
			}
			if (!overrideStyles.PressedFormatStyle.IsEmptyStyle())
			{
				navigationBarStateStyles._pressedFormatStyle = navigationBarStateStyles.PressedFormatStyle.MergeWith(overrideStyles.PressedFormatStyle);
			}
			if (!overrideStyles.DisabledFormatStyle.IsEmptyStyle())
			{
				navigationBarStateStyles._disabledFormatStyle = navigationBarStateStyles.DisabledFormatStyle.MergeWith(overrideStyles.DisabledFormatStyle);
			}
			if (!overrideStyles.FormatStyle.IsEmptyStyle())
			{
				navigationBarStateStyles._formatStyle = navigationBarStateStyles.FormatStyle.MergeWith(overrideStyles.FormatStyle);
			}
			if (!overrideStyles.HotFormatStyle.IsEmptyStyle())
			{
				navigationBarStateStyles._hotFormatStyle = navigationBarStateStyles.HotFormatStyle.MergeWith(overrideStyles.HotFormatStyle);
			}
			return navigationBarStateStyles;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_disabledFormatStyle != null && !_disabledFormatStyle.IsEmptyStyle())
			{
				return true;
			}
			if (_formatStyle != null && !_formatStyle.IsEmptyStyle())
			{
				return true;
			}
			if (_hotFormatStyle != null && !_hotFormatStyle.IsEmptyStyle())
			{
				return true;
			}
			if (_pressedFormatStyle != null && !_pressedFormatStyle.IsEmptyStyle())
			{
				return true;
			}
			if (_selectedFormatStyle != null && !_selectedFormatStyle.IsEmptyStyle())
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
			if (_owner != null && _owner.NavigationBar != null)
			{
				_owner.NavigationBar.InvalidateLayout(false);
			}
		}

		internal void DisposeBrushesAndFont()
		{
			if (_formatStyle != null)
			{
				_formatStyle.DisposeBrushesAndFont();
			}
			if (_hotFormatStyle != null)
			{
				_hotFormatStyle.DisposeBrushesAndFont();
			}
			if (_selectedFormatStyle != null)
			{
				_selectedFormatStyle.DisposeBrushesAndFont();
			}
			if (_disabledFormatStyle != null)
			{
				_disabledFormatStyle.DisposeBrushesAndFont();
			}
		}
	}
}
