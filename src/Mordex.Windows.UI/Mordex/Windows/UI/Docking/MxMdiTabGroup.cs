using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	[ToolboxItem(false)]
	public partial class MxMdiTabGroup : Control, IDockElement, IMordexXmlLayoutsSupport
	{
		private const int DefaultStripHeight = 20;

		private const int HoverTimerIntervalMs = 250;

		private const int ScrollRepeatDelayMs = 150;

		private const int ScrollRepeatIntervalMs = 200;

		private const int TabTopInsetDefault = 2;

		private const int CloseImageIndex = 0;

		private const int ActiveFilesImageIndex = 5;

		private const int ActiveFilesAlternateImageIndex = 6;

		private const int DropEdgeSize = 12;

		private const int ToolTipOffsetX = 4;

		private const int ToolTipOffsetFromBottom = 12;

		private string _key;

		internal float _relativeSize;

		private Rectangle _groupBounds;

		internal ArrayList _childTabs;

		private Rectangle _formAreaBounds;

		private MdiTabGroupRenderer _tabGroupHost;

		private MdiTabStripLayout _groupLayout;

		private int _stripHeight;

		private DropTargetWindowDialog _dropTargetWindowDialog;

		private bool _allowCancelDrag;

		private Form _form;

		internal bool _isDeserializing;

		private WindowMessageHandler _toolTipPopup;

		private ToolTip _toolTip;

		private Timer _hoverTimer;

		private MdiTabItem _pressedTabLayout;

		private Timer _scrollRepeatTimer;

		private SplitterMdi _splitter;

		private bool _hasSplitter;

		private Rectangle _dropIndicatorBounds;

		private DockMessageFilter _dockMessageFilter;

		private bool _isDraggingTab;

		private MxMdiTabGroup _dropTargetGroup;

		private MdiGroupOrientation _mdiGroupOrientation;

		private int _dropGroupIndex;

		private Cursor _dragCursor;

		private Cursor _noDragCursor;

		internal bool _isSelected;

		public int GroupIndex
		{
			get
			{
				if (_tabGroupHost != null)
				{
					return _tabGroupHost.GetGroups().IndexOf(this);
				}
				return -1;
			}
		}

		[Browsable(false)]
		public MdiChildTab[] MdiTabs
		{
			get
			{
				MdiChildTab[] tabs = new MdiChildTab[_groupLayout.GetCount()];
				for (int i = 0; i < _groupLayout.GetCount(); i++)
				{
					tabs[i] = _groupLayout.GetTab(i).GetChildTab();
				}
				return tabs;
			}
		}

		public MxDockManager PanelManager => _tabGroupHost.GetManager();

		public MdiChildTab SelectedTab
		{
			get
			{
				if (_form != null)
				{
					foreach (MdiChildTab item in _childTabs)
					{
						if (item.Form == _form)
						{
							return item;
						}
					}
				}
				return null;
			}
		}

		public Rectangle FormRectangle => _formAreaBounds;

		public int TabCount => _childTabs.Count;

		bool IDockElement.AllowCancelDrag => _allowCancelDrag;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Tab";

		internal MxMdiTabGroup(MdiTabGroupRenderer mdiHandler)
		{
			_key = "";
			_relativeSize = -1f;
			_tabGroupHost = mdiHandler;
			_allowCancelDrag = true;
			_childTabs = new ArrayList();
			_groupLayout = new MdiTabStripLayout(this);
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopHoverTimer();
				StopScrollRepeatTimer();
				if (_dragCursor != null)
				{
					_dragCursor.Dispose();
					_dragCursor = null;
				}
				if (_noDragCursor != null)
				{
					_noDragCursor.Dispose();
					_noDragCursor = null;
				}
				_form = null;
				SetHoveredTabState(null);
				if (_groupLayout != null)
				{
					_groupLayout.DisposeResources();
				}
			}
			base.Dispose(disposing);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			if (_tabGroupHost != null)
			{
				_tabGroupHost._isLoadingLayout = true;
			}
			_isDeserializing = true;
			MdiChildTab uiMdiChildTab = null;
			_relativeSize = (float)LayoutReader.GetPropertyValue(reader, "Size", typeof(float), -1f);
			int tabIndex = 0;
			for (PropertyValue propertyValue = reader["Tab" + tabIndex]; propertyValue != null; propertyValue = reader["Tab" + tabIndex])
			{
				MdiChildTab loadedTab = new MdiChildTab(this, null);
				propertyValue.DeserializeObject(loadedTab);
				if (PanelManager.TryRestoreMdiChildForm(loadedTab))
				{
					_childTabs.Add(loadedTab);
					if (loadedTab._isSelected)
					{
						uiMdiChildTab = loadedTab;
					}
				}
				else
				{
					loadedTab.RestoreForm();
				}
				tabIndex++;
			}
			RebuildLayout(true);
			if (uiMdiChildTab == null)
			{
				if (_childTabs.Count > 0)
				{
					SelectForm(((MdiChildTab)_childTabs[0]).Form);
				}
			}
			else
			{
				SelectForm(uiMdiChildTab.Form);
			}
			_isDeserializing = false;
			_isSelected = (bool)LayoutReader.GetPropertyValue(reader, "IsSelected", typeof(bool), false);
			if (_tabGroupHost != null)
			{
				_tabGroupHost._isLoadingLayout = false;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			info.AddValue("Size", _relativeSize);
			if (_tabGroupHost.GetActiveGroup() == this)
			{
				info.AddValue("IsSelected", true);
			}
			int tabIndex = 0;
			foreach (MdiChildTab item in _childTabs)
			{
				info.AddValue("Tab" + tabIndex, item);
				tabIndex++;
			}
		}
	}
}
