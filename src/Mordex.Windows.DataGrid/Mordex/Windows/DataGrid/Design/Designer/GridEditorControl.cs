using Mordex.Windows.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;
using Microsoft.Win32;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	[ToolboxItem(false)]
	internal partial class GridEditorControl : UserControl, IGridElement
	{
		private enum CollectionType
		{
			None,
			Columns,
			ColumnSets,
			FormatConditions,
			FormatStyles,
			Groups,
			SortKeys,
			ChildTables,
			DropDowns,
			GroupHeaderTotals,
			StoredFilters,
			CustomGroups,
			CustomGroupRows
		}

		private IContainer _iContainer;

		private EventHandler _dataChangedHandler;

		private ICollection _selectedCollection;

		private CollectionType _collectionType;

		private MxToolBar _tbOperations;

		private MxToolBarButton _tbtnAdd;

		private MxToolBarButton _tbtnRemove;

		private MxToolBarButton _tbtnClear;

		private MxToolBarButton _separatorFirst;

		private MxToolBarButton _tbtnMoveUp;

		private MxToolBarButton _tbtnMoveDown;

		private MxToolBarButton _separatorSecond;

		private ImageList _imageList;

		private ListBox _itemListBox;

		private Splitter _listSplitter;

		private PropertyGrid _properties;

		private MxToolBarButton _tbtnValueList;

		private MxToolBarButton _tbtnColumnSets;

		private MxToolBarButton _tbtnGroupProperties;

		private MxToolBarButton _separatorThird;

		private ArrayList _arrayList;

		private Image _itemImage;

		private ToolbarOptions _toolbarOptions = ToolbarOptions.All;

		private static object _dataChangedEventKey = new object();

		private static object _gridEditorControlValue = new object();

		public GridEditorControl()
		{
			InitializeComponent();
			_properties.CommandsVisibleIfAvailable = false;
			_itemListBox.BindingContext = new BindingContext();
		}
	}
}
