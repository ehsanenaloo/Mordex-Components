using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public TaskPaneLayoutCollection Layouts => _layouts;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		public TaskPaneLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.ResolveTaskPane() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.CanBeLoaded())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				if (_currentLayout == value)
				{
					return;
				}
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnCurrentLayoutChanging(e);
				if (!e.Cancel)
				{
					if (_currentLayout != null)
					{
						_currentLayout.AttachTaskPane(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.AttachTaskPane(this);
					}
					if (!_isInitializing)
					{
						LoadCurrentLayout();
					}
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(false)]
		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if ((_settingsKey == null || _settingsKey.Length == 0) && base.Name != null && base.Name.Length > 0)
				{
					return base.Name;
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[DefaultValue(false)]
		[Category("Appearance")]
		[Description("Indicates whether the control should display focus rectangles.")]
		public bool ShowFocusRectangle
		{
			get
			{
				return _showFocusRectangle;
			}
			set
			{
				if (_showFocusRectangle != value)
				{
					_showFocusRectangle = value;
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the control can recieve focus")]
		public bool Selectable
		{
			get
			{
				return _selectable;
			}
			set
			{
				if (_selectable == value)
				{
					return;
				}
				_selectable = value;
				if (!_selectable)
				{
					SetStyle(ControlStyles.Selectable, value: false);
					if (ActiveGroup != null)
					{
						ActiveGroup = null;
					}
					if (ActiveItem != null)
					{
						ActiveItem = null;
					}
				}
				else
				{
					SetStyle(ControlStyles.Selectable, value: true);
				}
			}
		}

		[Description("Represents the collection of the TaskPaneGroup objects contained in this TaskPane.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Data")]
		[Editor(typeof(GroupEditor), typeof(UITypeEditor))]
		public TaskPaneGroupCollection Groups => _groups;

		MxTaskPane ITaskPaneOwner.TaskPane => this;

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TaskPaneItem SelectedItem
		{
			get
			{
				return _selectedItem;
			}
			set
			{
				if (_selectedItem != value)
				{
					if (_selectedItem != null)
					{
						_selectedItem._selected = false;
					}
					_selectedItem = value;
					if (_selectedItem != null)
					{
						_selectedItem._selected = true;
					}
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[Description("Indicates if the TaskPaneItem appears selected when the user clicks on it.")]
		[DefaultValue(false)]
		public bool KeepSelection
		{
			get
			{
				return _keepSelection;
			}
			set
			{
				if (_keepSelection != value)
				{
					_keepSelection = value;
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether an ampersand (&) is included in the text of the control.")]
		[DefaultValue(true)]
		public bool UseMnemonic
		{
			get
			{
				return _useMnemonic;
			}
			set
			{
				if (_useMnemonic != value)
				{
					_useMnemonic = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(0)]
		[Description("Represents the current position of the scroll box on the scroll bar control.")]
		public int VerticalScrollPosition
		{
			get
			{
				return _verticalScrollPosition;
			}
			set
			{
				if (_verticalScrollbar != null)
				{
					_verticalScrollbar.UpdateValue(value);
				}
				_verticalScrollPosition = value;
			}
		}

		[DefaultValue(0)]
		[Browsable(false)]
		public int InnerHeight
		{
			get
			{
				if (base.DesignMode)
				{
					return 0;
				}
				if (!_isLayoutValid)
				{
					using (Graphics layoutGraphics = CreateGraphics())
					{
						LayoutContent(layoutGraphics);
					}
				}
				return _innerHeight;
			}
		}

		[DefaultValue(1)]
		[Category("Behavior")]
		[Description("Indicates the number of groups displayed horizontally across the TaskPane control.")]
		public int Columns
		{
			get
			{
				return _columns;
			}
			set
			{
				if (_columns != value)
				{
					if (value < 1)
					{
						throw new InvalidOperationException();
					}
					_columns = value;
					InvalidateLayout();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(VerticalScrollBar.Automatic)]
		[Description("Specifies when the vertical scroll bar will be visible.")]
		public VerticalScrollBar VerticalScrollBar
		{
			get
			{
				return _verticalScrollBar;
			}
			set
			{
				if (!Enum.IsDefined(typeof(VerticalScrollBar), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_verticalScrollBar != value)
				{
					_verticalScrollBar = value;
					if (value == VerticalScrollBar.Never)
					{
						_verticalScrollPosition = 0;
					}
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[DefaultValue(OwnerDrawnArea.None)]
		[Description("Gets or sets a value specifying which area of a TaskPane control needs to be drawn manually.")]
		[TypeConverter(typeof(FlagsEnumConverter))]
		[Category("Behavior")]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		public OwnerDrawnArea OwnerDrawnArea
		{
			get
			{
				return _ownerDrawnArea;
			}
			set
			{
				_ownerDrawnArea = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TaskPaneItem ActiveItem
		{
			get
			{
				return _activeItem;
			}
			set
			{
				if (_activeItem == value || (!_selectable && value != null))
				{
					return;
				}
				_activeItem = value;
				if (_activeItem != null)
				{
					if (!ActiveItem.Container && !Focused)
					{
						TaskPanePaintHelper.FocusControl(this);
					}
					_activeGroup = null;
				}
				SelectedItem = value;
				Invalidate();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TaskPaneGroup ActiveGroup
		{
			get
			{
				if (_activeGroup != null)
				{
					return _activeGroup;
				}
				if (_activeItem != null)
				{
					return _activeItem.Group;
				}
				return null;
			}
			set
			{
				if (_activeGroup != value && (_selectable || value == null))
				{
					_activeGroup = value;
					if (_activeGroup != null)
					{
						_activeItem = null;
					}
					Invalidate();
				}
			}
		}

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a group or item.")]
		[Category("Behavior")]
		[DefaultValue(true)]
		public bool ShowToolTips
		{
			get
			{
				return _showToolTips;
			}
			set
			{
				_showToolTips = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}
	}
}
