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
	public partial class GridDropDown
	{
		private void ApplyLayout(GridLayout layout)
		{
			if (layout != null && !(layout.LayoutString == ""))
			{
				if (!layout.HasFullLayout())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				layout = layout.GetMergedLayout();
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
				PropertyValue propertyValue = mordexLayoutReader["GridLayoutData"];
				if (propertyValue != null)
				{
					DeserializeGridDropDownState(propertyValue.GetLayoutReader(), false);
					SetDataSourceAndMember(DataSource, DataMember, true);
					RefreshRowCountAndInvalidate();
				}
			}
		}

		internal GridLayout BuildDesignTimeLayout()
		{
			if (ShouldSerializeDesignTimeLayout())
			{
				GridLayout gridLayout = new GridLayout();
				gridLayout.UpdatePersistenceMode(PersistenceMode.All);
				LayoutWriter mordexLayoutWriter = new LayoutWriter();
				WriteLayoutByMode(mordexLayoutWriter, gridLayout.GetPersistenceMode());
				SerializeGridDropDownState(mordexLayoutWriter);
				LayoutWriter.StoreInLayout("GridLayoutData", mordexLayoutWriter, gridLayout);
				gridLayout.DataMember = "";
				gridLayout.DataSource = null;
				return gridLayout;
			}
			return null;
		}

		internal void ApplyDesignTimeLayout(GridLayout gridLayout)
		{
			if (gridLayout != null)
			{
				gridLayout.UpdatePersistenceMode(PersistenceMode.All);
				ApplyLayout(gridLayout);
			}
		}

		private void OnAutoScrollTimerTick(object sender, EventArgs eventArgs)
		{
			if (!IsNotInEditorDialog())
			{
				return;
			}
			Point position = Cursor.Position;
			position = PointToClient(position);
			int row = -1;
			if (position.Y < 0)
			{
				if (base.FirstRow > 0)
				{
					row = base.FirstRow - 1;
				}
			}
			else if (position.Y > base.ClientRectangle.Bottom && base.Row != -3 && base.Row < base.RowCount - 1)
			{
				row = base.FirstRow + _visibleRows;
			}
			if (row >= 0 && row < base.RowCount)
			{
				base.Row = row;
				Update();
			}
		}

		internal void ResizeToFitRows(int value)
		{
			if (!base.IsDisposed && !base.Disposing)
			{
				int fullWidth = 0;
				int height = 0;
				fullWidth = ((base.RootTable != null) ? base.RootTable.CalculateFullWidth() : 0);
				Graphics graphics = CreateGraphics();
				int measureRootTableHeaderHeight = MeasureRootTableHeaderHeight(graphics);
				int measureAutoRowHeight = MeasureAutoRowHeight(graphics);
				graphics.Dispose();
				graphics = null;
				height = ((base.RowCount == 0) ? (measureRootTableHeaderHeight + measureAutoRowHeight) : ((base.RowCount > _visibleRows) ? (_visibleRows * measureAutoRowHeight + measureRootTableHeaderHeight) : (base.RowCount * measureAutoRowHeight + measureRootTableHeaderHeight)));
				if (fullWidth > SystemInformation.PrimaryMonitorSize.Width)
				{
					fullWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
					height += SystemInformation.HorizontalScrollBarHeight;
				}
				if (height > SystemInformation.PrimaryMonitorMaximizedWindowSize.Height)
				{
					height = SystemInformation.PrimaryMonitorMaximizedWindowSize.Height;
				}
				Size size = new Size(fullWidth, height);
				while (base.ClientSize != size)
				{
					base.ClientSize = size;
					CalculateLayoutIfReady();
				}
				int width = base.Width;
				if (base.Width < value)
				{
					width = value;
				}
				if (base.Width > SystemInformation.PrimaryMonitorSize.Width)
				{
					width = SystemInformation.PrimaryMonitorSize.Width;
				}
				if (base.ColumnAutoResize)
				{
					width = value;
				}
				if (base.Width != width)
				{
					base.Width = width;
					CalculateLayoutIfReady();
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (IsNotInEditorDialog())
			{
				if (Visible && !IsDropDownOpenState() && !base.DesignMode)
				{
					Visible = false;
				}
			}
			else
			{
				base.OnVisibleChanged(e);
			}
		}

		internal void ScrollSelectedRowToTop()
		{
			if (base.Row + VisibleRows > base.RowCount)
			{
				base.FirstRow = base.RowCount - _visibleRows;
			}
			else
			{
				base.FirstRow = base.Row;
			}
		}
	}
}
