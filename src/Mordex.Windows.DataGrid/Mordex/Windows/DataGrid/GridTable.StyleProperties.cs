using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable
	{
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw row headers that belongs to the table.")]
		[Category("Format Styles")]
		public GridFormatStyle RowHeaderFormatStyle
		{
			get
			{
				if (_rowHeaderFormatStyle == null)
				{
					_rowHeaderFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _rowHeaderFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _rowHeaderFormatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || value.DataGrid != DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_rowHeaderFormatStyle != null && _rowHeaderFormatStyle._owner == this)
				{
					_rowHeaderFormatStyle._owner = null;
				}
				_rowHeaderFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Description("Format settings used to draw column and row headers that belongs to the table.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Format Styles")]
		public GridFormatStyle HeaderFormatStyle
		{
			get
			{
				if (_headerFormatStyle == null)
				{
					_headerFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _headerFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _headerFormatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || value.DataGrid != DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_headerFormatStyle != null && _headerFormatStyle._owner == this)
				{
					_headerFormatStyle._owner = null;
				}
				_headerFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw table headers.")]
		[Category("Format Styles")]
		public GridFormatStyle TableHeaderFormatStyle
		{
			get
			{
				if (_tableHeaderFormatStyle == null)
				{
					_tableHeaderFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _tableHeaderFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _tableHeaderFormatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || value.DataGrid != DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_tableHeaderFormatStyle != null && _tableHeaderFormatStyle._owner == this)
				{
					_tableHeaderFormatStyle._owner = null;
				}
				_tableHeaderFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw alternating rows in the table.")]
		[Category("Format Styles")]
		public GridFormatStyle AlternatingRowFormatStyle
		{
			get
			{
				if (_alternatingRowFormatStyle == null)
				{
					_alternatingRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _alternatingRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_alternatingRowFormatStyle == value)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_alternatingRowFormatStyle != null && _alternatingRowFormatStyle._owner == this)
				{
					_alternatingRowFormatStyle._owner = null;
				}
				_alternatingRowFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Format Styles")]
		[Description("Format settings used to draw group header rows that belong to the table.")]
		public GridFormatStyle GroupRowFormatStyle
		{
			get
			{
				if (_groupRowFormatStyle == null)
				{
					_groupRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _groupRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_groupRowFormatStyle == value)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_groupRowFormatStyle != null && _groupRowFormatStyle._owner == this)
				{
					_groupRowFormatStyle._owner = null;
				}
				_groupRowFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Category("Format Styles")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw group total rows that belongs to the table.")]
		public GridFormatStyle GroupTotalRowFormatStyle
		{
			get
			{
				if (_groupTotalRowFormatStyle == null)
				{
					_groupTotalRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _groupTotalRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_groupTotalRowFormatStyle == value)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_groupTotalRowFormatStyle != null && _groupTotalRowFormatStyle._owner == this)
				{
					_groupTotalRowFormatStyle._owner = null;
				}
				_groupTotalRowFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Format Styles")]
		[Description("Format settings used to draw cells in the total row for the table.")]
		public GridFormatStyle TotalRowFormatStyle
		{
			get
			{
				if (_totalRowFormatStyle == null)
				{
					_totalRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _totalRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_totalRowFormatStyle == value)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_totalRowFormatStyle != null && _totalRowFormatStyle._owner == this)
				{
					_totalRowFormatStyle._owner = null;
				}
				_totalRowFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Description("Format settings used to draw cells in the new row(s) that belong to the table.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Format Styles")]
		public GridFormatStyle NewRowFormatStyle
		{
			get
			{
				if (_newRowFormatStyle == null)
				{
					_newRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _newRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_newRowFormatStyle == value)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_newRowFormatStyle != null && _newRowFormatStyle._owner == this)
				{
					_newRowFormatStyle._owner = null;
				}
				_newRowFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Format Styles")]
		[Description("Format settings used to draw previews rows under the rows that belongs to the table.")]
		public GridFormatStyle PreviewRowFormatStyle
		{
			get
			{
				if (_previewRowFormatStyle == null)
				{
					_previewRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _previewRowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_previewRowFormatStyle == value)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_previewRowFormatStyle != null && _previewRowFormatStyle._owner == this)
				{
					_previewRowFormatStyle._owner = null;
				}
				_previewRowFormatStyle = value;
				RefreshDataGrid();
			}
		}

		MxDataGrid IFormatStyleOwner.DataGrid => DataGrid;
	}
}
