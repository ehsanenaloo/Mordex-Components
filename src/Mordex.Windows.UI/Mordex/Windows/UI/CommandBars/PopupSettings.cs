using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class PopupSettings : IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private bool _isEmpty = true;

		internal IAccessibleCommandBarsElement _owner;

		private InheritableBoolean _sidebarVisible;

		private FormatStyle _sidebarFormatStyle;

		private FormatStyle _imageAreaFormatStyle;

		private FormatStyle _infrequentItemsFormatStyle;

		private int _sidebarWidth = 24;

		private StateStyles _stateStyles;


		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Specifies whether a custom bar is displayed on the left side of the popup window.")]
		public InheritableBoolean SidebarVisible
		{
			get
			{
				return _sidebarVisible;
			}
			set
			{
				if (_sidebarVisible != value)
				{
					_sidebarVisible = value;
					NotifyOwnerInvalidated();
				}
			}
		}

		[Description("The format style properties used to draw the custom bar on the left side.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle SidebarFormatStyle
		{
			get
			{
				if (_sidebarFormatStyle == null)
				{
					_sidebarFormatStyle = new FormatStyle(this);
				}
				return _sidebarFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the background of the area where the images and checkboxes are displayed.")]
		public FormatStyle ImageAreaFormatStyle
		{
			get
			{
				if (_imageAreaFormatStyle == null)
				{
					_imageAreaFormatStyle = new FormatStyle(this);
				}
				return _imageAreaFormatStyle;
			}
		}

		[Description("The format style properties used to draw the commands that are not frequently used.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle InfrequentItemsFormatStyle
		{
			get
			{
				if (_infrequentItemsFormatStyle == null)
				{
					_infrequentItemsFormatStyle = new FormatStyle(this);
				}
				return _infrequentItemsFormatStyle;
			}
			set
			{
				if (_infrequentItemsFormatStyle != value)
				{
					_infrequentItemsFormatStyle = value;
					NotifyOwnerInvalidated();
				}
			}
		}

		[DefaultValue(24)]
		[Description("The width of the custom bar that will be displayed on the left of the popup window.")]
		public int SidebarWidth
		{
			get
			{
				return _sidebarWidth;
			}
			set
			{
				if (_sidebarWidth != value)
				{
					_sidebarWidth = value;
					NotifyOwnerInvalidated();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the commands in their different states.")]
		public StateStyles StateStyles
		{
			get
			{
				if (_stateStyles == null)
				{
					_stateStyles = new StateStyles(this);
				}
				return _stateStyles;
			}
		}

		Control IFormatStyleOwner.Owner => null;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_isEmpty = (bool)LayoutReader.GetPropertyValue(reader, "Empty", typeof(bool), true);
			_imageAreaFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "ImageAreaFormatStyle", typeof(FormatStyle), null);
			_infrequentItemsFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "InfrequentItemsFormatStyle", typeof(FormatStyle), null);
			_sidebarFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "SidebarFormatStyle", typeof(FormatStyle), null);
			_sidebarVisible = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "SidebarVisible", typeof(InheritableBoolean), InheritableBoolean.Default);
			_sidebarWidth = (int)LayoutReader.GetPropertyValue(reader, "SidebarWidth", typeof(int), 24);
			_stateStyles = null;
			reader["StateStyles"]?.DeserializeObject(StateStyles);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_imageAreaFormatStyle != null && !_imageAreaFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("ImageAreaFormatStyle", _imageAreaFormatStyle);
			}
			if (_infrequentItemsFormatStyle != null && !_infrequentItemsFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("InfrequentItemsFormatStyle", _infrequentItemsFormatStyle);
			}
			if (_sidebarFormatStyle != null && !_sidebarFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("SidebarFormatStyle", _sidebarFormatStyle);
			}
			if (_sidebarVisible != InheritableBoolean.Default)
			{
				info.AddValue("SidebarVisible", _sidebarVisible);
			}
			if (_sidebarWidth != 24)
			{
				info.AddValue("SidebarWidth", _sidebarWidth);
			}
			if (_stateStyles != null && _stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", StateStyles);
			}
			if (!_isEmpty)
			{
				info.AddValue("Empty", _isEmpty);
			}
		}

		public PopupSettings()
		{
		}

		internal PopupSettings(IAccessibleCommandBarsElement owner)
			: this(owner, empty: true)
		{
		}

		internal PopupSettings(IAccessibleCommandBarsElement owner, bool empty)
		{
			_owner = owner;
			_isEmpty = empty;
		}

		internal PopupSettings(PopupSettings baseSettings)
		{
			_owner = null;
			_imageAreaFormatStyle = new FormatStyle(baseSettings.ImageAreaFormatStyle);
			_infrequentItemsFormatStyle = new FormatStyle(baseSettings.InfrequentItemsFormatStyle);
			_sidebarFormatStyle = new FormatStyle(baseSettings.SidebarFormatStyle);
			_sidebarVisible = baseSettings._sidebarVisible;
			_sidebarWidth = baseSettings._sidebarWidth;
			_stateStyles = new StateStyles(baseSettings.StateStyles);
		}

		internal PopupSettings CreateCopy()
		{
			return new PopupSettings(this);
		}

		internal PopupSettings MergeWith(PopupSettings overrideSettings, StateStyles uiStateStyles)
		{
			PopupSettings uiPopupSettings = new PopupSettings(this);
			if (!overrideSettings.ImageAreaFormatStyle.IsDefaultStyle())
			{
				uiPopupSettings._imageAreaFormatStyle = uiPopupSettings.InfrequentItemsFormatStyle.MergeStyle(overrideSettings.ImageAreaFormatStyle);
			}
			if (!overrideSettings.InfrequentItemsFormatStyle.IsDefaultStyle())
			{
				uiPopupSettings._infrequentItemsFormatStyle = uiPopupSettings.InfrequentItemsFormatStyle.MergeStyle(overrideSettings.InfrequentItemsFormatStyle);
			}
			if (!overrideSettings.SidebarFormatStyle.IsDefaultStyle())
			{
				uiPopupSettings._sidebarFormatStyle = uiPopupSettings._sidebarFormatStyle.MergeStyle(overrideSettings.SidebarFormatStyle);
			}
			if (overrideSettings.SidebarVisible != InheritableBoolean.Default)
			{
				uiPopupSettings._sidebarVisible = overrideSettings.SidebarVisible;
			}
			if (overrideSettings._sidebarWidth != 24)
			{
				uiPopupSettings._sidebarWidth = overrideSettings._sidebarWidth;
			}
			if (uiStateStyles != null)
			{
				uiPopupSettings._stateStyles = uiPopupSettings.StateStyles.CreateMergedStateStyles(uiStateStyles);
			}
			uiPopupSettings._stateStyles = uiPopupSettings._stateStyles.CreateMergedStateStyles(overrideSettings.StateStyles);
			return uiPopupSettings;
		}

		internal void DisposeStyleResources()
		{
			if (_imageAreaFormatStyle != null)
			{
				_imageAreaFormatStyle.DisposeResources();
			}
			if (_infrequentItemsFormatStyle != null)
			{
				_infrequentItemsFormatStyle.DisposeResources();
			}
			if (_sidebarFormatStyle != null)
			{
				_sidebarFormatStyle.DisposeResources();
			}
			if (_stateStyles != null)
			{
				_stateStyles.DisposeFormatStyleResources();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_imageAreaFormatStyle = null;
			_infrequentItemsFormatStyle = null;
			_sidebarFormatStyle = null;
			_sidebarVisible = InheritableBoolean.Default;
			_sidebarWidth = 24;
			_stateStyles = null;
			NotifyOwnerInvalidated();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_imageAreaFormatStyle != null && !_imageAreaFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_infrequentItemsFormatStyle != null && !_infrequentItemsFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_sidebarFormatStyle != null && !_sidebarFormatStyle.AreAllPropertiesDefault())
			{
				return true;
			}
			if (_sidebarVisible != InheritableBoolean.Default)
			{
				return true;
			}
			if (_sidebarWidth != 24)
			{
				return true;
			}
			if (_stateStyles != null && _stateStyles.ShouldSerialize())
			{
				return true;
			}
			return false;
		}

		private void NotifyOwnerInvalidated()
		{
			if (_owner != null)
			{
				_owner.OnInvalidateCommand();
			}
		}

		void IFormatStyleOwner.PropertyChange()
		{
			NotifyOwnerInvalidated();
		}
	}
}
