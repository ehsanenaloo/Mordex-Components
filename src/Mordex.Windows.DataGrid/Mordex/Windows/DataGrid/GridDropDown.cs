using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[ToolboxItem(false)]
	public partial class GridDropDown : MxDataGrid, IMordexXmlLayoutsSupport, ILookUpTable
	{
		internal class DropDownAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleObject Parent
			{
				get
				{
					AccessibleObject accessibleObject = null;
					if (GetDropDown() != null && GetDropDown()._dropListOwner != null)
					{
						accessibleObject = GetDropDown()._dropListOwner.AccessibleObject;
					}
					if (accessibleObject != null)
					{
						return accessibleObject;
					}
					return base.Parent;
				}
			}

			public override string Name => "MordexComboDropDown";

			internal DropDownAccessibleObject(GridDropDown owner)
				: base(owner)
			{
			}

			private GridDropDown GetDropDown()
			{
				return (GridDropDown)base.Owner;
			}
		}

		internal MxCheckedComboBox _gridDropDownOwner;

		internal GridDropDownCollection _ownerCollection;

		private Container _container = new Container();

		internal Timer _gridDropDownTimer;

		internal IGridDropListOwner _dropListOwner;

		private int _visibleRows = 8;

		private bool _isMouseDownInside;

		private bool _isSelectionSyncEnabled = true;

		private string _valueMember = "";

		private string _displayMember = "";

		private bool _replaceValues = true;

		private string _key = string.Empty;

		private GridColumn _imageColumn;

		private IntPtr _originalParentHandle = IntPtr.Zero;

		private int _dropDownWidth;

		private Timer _deferredShowTimer;

		private Hashtable _hashtable;

		internal bool _isLookupCacheEnabled = true;

		internal bool _isCellDropDown;

		public GridDropDown()
		{
			SetContainer();
			InitializeDropDown();
			SetStyle(ControlStyles.Selectable, value: false);
		}
	}
}
