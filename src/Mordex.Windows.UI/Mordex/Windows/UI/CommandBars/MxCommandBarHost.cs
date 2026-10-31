using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	[Designer("Mordex.Windows.UI.Design.RebarDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(false)]
	public partial class MxCommandBarHost : Control, ICommandBarOwner, IFormatStyleOwner, IMordexXmlLayoutsSupport, ISupportInitialize
	{

		private const int InsertEdgeTolerance = 10;

		internal bool _isInitializing;

		private bool _shouldCheckBars;

		private Container _container = new Container();

		internal bool _isHorizontal = true;

		private FormatStyle _commandBarsFormatStyle;

		private CommandBarCollection _bars;

		private FormatStyle _formatStyle;

		private Size _lastSize;

		private int _resizeDepth;

		private bool _isLayoutUpdateEnabled = true;

		internal MxCommandManager _commandManager;

		private ArrayList _rows;

		private bool _hasAddedBar;

		private ArrayList _barStates;

		string IMordexXmlLayoutsSupport.Identifier => GetBarsDockStyle().ToString();

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		Control IFormatStyleOwner.Owner => this;

		bool ICommandBarOwner.IsInitializing => _isInitializing;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[Description("Represents the format style properties used to draw the UICommandBar objects associated with this control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle CommandBarsFormatStyle
		{
			get
			{
				if (_commandBarsFormatStyle == null)
				{
					_commandBarsFormatStyle = new FormatStyle(this);
				}
				return _commandBarsFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandBarCollection CommandBars => _bars;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DefaultValue(DockStyle.None)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}

		[DefaultValue(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int TabIndex
		{
			get
			{
				return base.TabIndex;
			}
			set
			{
				base.TabIndex = value;
			}
		}

		[Description("The format style properties used to draw the background of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new FormatStyle(this);
				}
				return _formatStyle;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public MxCommandManager CommandManager
		{
			get
			{
				return _commandManager;
			}
			set
			{
				if (_commandManager != value)
				{
					_commandManager = value;
					if (_bars.Count > 0)
					{
						_commandManager.ShowRebar(this);
					}
					LayoutRows();
				}
			}
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			_isInitializing = false;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			PropertyValue propertyValue = reader["CommandBars"];
			if (propertyValue != null)
			{
				CommandManagerState targetInstance = new CommandManagerState(null, CommandManager);
				propertyValue.DeserializeObject(targetInstance);
			}
			foreach (CommandBarHostLayoutState item in GetRows())
			{
				item.PackBars();
			}
			_commandBarsFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "CommandBarsFormatStyle", typeof(FormatStyle), null);
			if (_commandBarsFormatStyle != null)
			{
				_commandBarsFormatStyle._owner = this;
			}
			_formatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "FormatStyle", typeof(FormatStyle), null);
			if (_formatStyle != null)
			{
				_formatStyle._owner = this;
			}
			foreach (CommandBarHostLayoutState row in _rows)
			{
				row.NotifyLayoutChanged(true);
				row.NotifyLayoutChanged(false);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_bars != null && _bars.Count > 0)
			{
				CommandManagerState value = new CommandManagerState(_bars.GetSortedBars(_isHorizontal, true), CommandManager);
				info.AddValue("CommandBars", value);
			}
			if (_commandBarsFormatStyle != null && !_commandBarsFormatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("CommandBarsFormatStyle", _commandBarsFormatStyle);
			}
			if (_formatStyle != null && !_formatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
		}

		void IFormatStyleOwner.PropertyChange()
		{
			Invalidate();
		}

		void ICommandBarOwner.OnBarAdded(MxCommandBar commandBar)
		{
			commandBar.SetHost(this);
			if (RenderingHelper.GetParentControl(this) == null && _commandManager != null)
			{
				_commandManager.ShowRebar(this);
			}
			SetHasAddedBar(true);
			base.Controls.Add(commandBar);
			AddRowForBar(commandBar);
		}

		void ICommandBarOwner.OnBarRemoved(MxCommandBar commandBar)
		{
			commandBar._host = null;
			try
			{
				DetachBar(commandBar);
			}
			catch (InvalidOperationException)
			{
				// The row list can change while the bar leaves it; a second pass finishes the detach.
				DetachBar(commandBar);
			}
		}

		private void DetachBar(MxCommandBar commandBar)
		{
			if (commandBar.GetHostRow() != null)
			{
				commandBar.GetHostRow().GetBars().Remove(commandBar);
			}
			if (base.Controls.Contains(commandBar))
			{
				base.Controls.Remove(commandBar);
			}
			if (GetRows() != null && GetRows().Count == 0 && _commandManager != null)
			{
				_commandManager.HideRebar(this);
			}
		}

		MxCommandManager ICommandBarOwner.GetCommandManagerOwner()
		{
			return CommandManager;
		}

		public MxCommandBarHost()
			: this(null)
		{
		}

		internal MxCommandBarHost(MxCommandManager manager)
		{
			_commandManager = manager;
			_bars = new CommandBarCollection(this);
			_rows = new ArrayList();
			Dock = DockStyle.Top;
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			TabStop = false;
		}

		protected override void Dispose(bool disposing)
		{
			_themeHook?.Stop();
			if (base.Container != null)
			{
				base.Container.Remove(this);
			}
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
			base.Events.Dispose();
		}

		internal BarsDockStyle GetBarsDockStyle()
		{
			switch (Dock)
			{
			case DockStyle.Top:
				return BarsDockStyle.Top;
			case DockStyle.Left:
				return BarsDockStyle.Left;
			case DockStyle.Bottom:
				return BarsDockStyle.Bottom;
			case DockStyle.Right:
				return BarsDockStyle.Right;
			default:
				return BarsDockStyle.Top;
			}
		}

		protected override void OnDockChanged(EventArgs e)
		{
			base.OnDockChanged(e);
			if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
			{
				_isHorizontal = true;
			}
			else
			{
				_isHorizontal = false;
			}
			foreach (CommandBarHostLayoutState item in _rows)
			{
				item.SetIsHorizontal(_isHorizontal);
				Rectangle rectangle = item._bounds;
				if (_isHorizontal)
				{
					rectangle.Width = base.ClientRectangle.Width;
				}
				else
				{
					rectangle.Height = base.ClientRectangle.Height;
				}
				item.SetBounds(rectangle);
			}
			Invalidate();
		}

		internal bool IsResizing()
		{
			if (_resizeDepth > 0)
			{
				return true;
			}
			return false;
		}

		internal bool IsLayoutUpdateEnabled()
		{
			return _isLayoutUpdateEnabled;
		}

		internal void SetLayoutUpdateEnabled(bool isEnabled)
		{
			_isLayoutUpdateEnabled = isEnabled;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (_isHorizontal)
			{
				if (Size.Width == _lastSize.Width)
				{
					_lastSize = Size;
					return;
				}
			}
			else if (Size.Height == _lastSize.Height)
			{
				_lastSize = Size;
				return;
			}
			_resizeDepth++;
			LayoutRows();
			_shouldCheckBars = true;
			Invalidate();
			_resizeDepth--;
			if (!base.Visible || IsResizing())
			{
				return;
			}
			foreach (CommandBarHostLayoutState item in GetRows())
			{
				item.ResetBarPositions();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (CommandManager != null)
			{
				CommandManager.ResetActiveControl(true);
			}
			if (e.Button == MouseButtons.Right && CommandManager != null && CommandManager.ShowQuickCustomizeMenu)
			{
				CommandManager.ShowToolbarsMenu(this);
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			// A host must belong to a command manager, but leaving a parent or being disposed is always allowed.
			if (CommandManager == null && !_isInitializing && base.Parent != null && !base.Disposing && !base.IsDisposed)
			{
				throw new InvalidOperationException();
			}
			if (!base.IsHandleCreated)
			{
				return;
			}
			UpdateBarForms();
		}

		private void UpdateBarForms()
		{
			Form form = RenderingHelper.FindForm(this);
			foreach (MxCommandBar item in _bars)
			{
				item.SetParentForm(form);
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!base.IsHandleCreated)
			{
				return;
			}
			UpdateBarForms();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			UpdateThemeListening();
			PaintThemedBackground(e.Graphics);
			if (_shouldCheckBars)
			{
				foreach (MxCommandBar commandBar in CommandBars)
				{
					if (!commandBar.Visible || !commandBar._isLayoutDirty)
					{
						continue;
					}
					if (_isHorizontal)
					{
						if (commandBar.Bounds.Left >= base.Right)
						{
							commandBar.RecalculateLayout();
						}
						else if (commandBar.Height == 0 || commandBar.Width == 0)
						{
							commandBar.RecalculateLayout(true);
						}
					}
					else if (commandBar.Bounds.Top >= base.Bottom)
					{
						commandBar.RecalculateLayout();
					}
					else if (commandBar.Height == 0 || commandBar.Width == 0)
					{
						commandBar.RecalculateLayout(true);
					}
				}
				_shouldCheckBars = false;
			}
			base.OnPaint(e);
		}

		internal void InvalidateBarLayouts()
		{
			foreach (MxCommandBar item in _bars)
			{
				item.InvalidateLayout();
			}
			Invalidate();
		}

		internal void ApplyRowRectangles()
		{
			foreach (CommandBarHostLayoutState item in _rows)
			{
				item.UpdateBounds(item._bounds, true);
			}
		}

		internal ArrayList GetRows()
		{
			return _rows;
		}

		internal bool AdjustOverflowingRows()
		{
			foreach (CommandBarHostLayoutState item in GetRows())
			{
				item.UpdateWrapFlags();
			}
			foreach (CommandBarHostLayoutState overflowRow in GetRows())
			{
				if (overflowRow.TryWrapOverflow())
				{
					LayoutRows();
					return true;
				}
			}
			return false;
		}

		internal bool HasRowNeedingAdjust()
		{
			foreach (CommandBarHostLayoutState item in GetRows())
			{
				if (item.IsAnyBarMarked())
				{
					return true;
				}
			}
			return false;
		}

		internal void ReapplyBarRowIndexes()
		{
			ArrayList bars = _bars.GetBarsSortedByPosition(_isHorizontal);
			bool wasInitializing = _isInitializing;
			_isInitializing = false;
			SetLayoutUpdateEnabled(false);
			foreach (MxCommandBar item in bars)
			{
				Point location = item.Location;
				item.UpdateRowIndex(item._rowIndex);
				if (_isHorizontal)
				{
					item.Location = new Point(location.X, item.Location.Y);
				}
				else
				{
					item.Location = new Point(item.Location.X, location.Y);
				}
			}
			SetLayoutUpdateEnabled(true);
			_isInitializing = wasInitializing;
		}

		internal int HitTestRow(int x, int y, ref bool isAtLeadingEdge)
		{
			if (!_hasAddedBar)
			{
				return 0;
			}
			if (_isHorizontal)
			{
				if (Dock == DockStyle.Bottom)
				{
					if (y <= base.ClientRectangle.Y && y >= base.ClientRectangle.Top - InsertEdgeTolerance)
					{
						isAtLeadingEdge = true;
						return 0;
					}
					y--;
				}
				else if (y >= base.ClientRectangle.Bottom && y <= base.ClientRectangle.Bottom + InsertEdgeTolerance)
				{
					return _rows.Count;
				}
			}
			else if (Dock == DockStyle.Right)
			{
				if (x <= base.ClientRectangle.Left && x >= base.ClientRectangle.Left - InsertEdgeTolerance)
				{
					isAtLeadingEdge = true;
					return 0;
				}
				x--;
			}
			else if (x >= base.ClientRectangle.Right && x <= base.ClientRectangle.Right + InsertEdgeTolerance)
			{
				return _rows.Count;
			}
			foreach (CommandBarHostLayoutState item in _rows)
			{
				if (item._bounds.Contains(x, y))
				{
					return item.GetRowIndex();
				}
			}
			return -1;
		}

		internal void SwapRows(CommandBarHostLayoutState row, int targetIndex)
		{
			int currentIndex = GetRows().IndexOf(row);
			if (currentIndex >= 0)
			{
				CommandBarHostLayoutState value = (CommandBarHostLayoutState)_rows[targetIndex];
				_rows[targetIndex] = row;
				_rows[currentIndex] = value;
				LayoutRows();
			}
			InvalidateBarLayouts();
		}

		internal void StackRowsAfter(CommandBarHostLayoutState row)
		{
			if (!_rows.Contains(row) || CommandManager == null || CommandManager._isBatchUpdating)
			{
				return;
			}
			int rowIndex = row.GetRowIndex();
			if (_isHorizontal)
			{
				int nextTop = row._bounds.Bottom;
				if (rowIndex < _rows.Count - 1)
				{
					for (int i = rowIndex + 1; i < _rows.Count; i++)
					{
						CommandBarHostLayoutState nextRow = (CommandBarHostLayoutState)_rows[i];
						Rectangle nextRowRect = nextRow._bounds;
						nextRowRect.Y = nextTop;
						nextRow.SetBounds(nextRowRect);
						nextTop += nextRow.GetThicknessWithFollowingRows();
					}
				}
				base.Height = nextTop - base.ClientRectangle.Top;
				return;
			}
			int nextLeft = row._bounds.Right;
			if (rowIndex < _rows.Count - 1)
			{
				for (int j = rowIndex + 1; j < _rows.Count; j++)
				{
					CommandBarHostLayoutState nextColumn = (CommandBarHostLayoutState)_rows[j];
					Rectangle nextColumnRect = nextColumn._bounds;
					nextColumnRect.X = nextLeft;
					nextColumn.SetBounds(nextColumnRect);
					nextLeft += nextColumn.GetThicknessWithFollowingRows();
				}
			}
			base.Width = nextLeft - base.ClientRectangle.Left;
		}

		internal void RefreshRowsAndBars()
		{
			LayoutRows();
			foreach (CommandBarHostLayoutState item in _rows)
			{
				item.NotifyLayoutChanged(true);
				ArrayList barSnapshot = new ArrayList();
				foreach (MxCommandBar rowBar in item.GetBars())
				{
					barSnapshot.Add(rowBar);
				}
				foreach (MxCommandBar snapshotBar in barSnapshot)
				{
					if (snapshotBar.Visible)
					{
						snapshotBar.RecalculateLayout();
					}
				}
			}
		}

		internal void ReapplyBarOffsets()
		{
			foreach (CommandBarHostLayoutState item in _rows)
			{
				foreach (MxCommandBar bar in item.GetBars())
				{
					if (_isHorizontal)
					{
						bar.MoveToLeft(bar.Location.X);
					}
					else
					{
						bar.MoveToTop(bar.Location.Y);
					}
				}
			}
		}

		internal void RefreshRows()
		{
			if (_rows == null)
			{
				return;
			}
			foreach (CommandBarHostLayoutState item in _rows)
			{
				item.NotifyLayoutChanged(true);
			}
		}

		internal void LayoutRows()
		{
			bool wasLayoutUpdateEnabled = _isLayoutUpdateEnabled;
			_isLayoutUpdateEnabled = false;
			int rowIndex = 0;
			int nextTop = 0;
			int nextLeft = 0;
			foreach (CommandBarHostLayoutState item in _rows)
			{
				item.SetRowIndex(rowIndex);
				rowIndex++;
				if (_isHorizontal)
				{
					int height = item.ComputeMaxBarThickness();
					item.SetBounds(new Rectangle(0, nextTop, base.ClientRectangle.Width, height));
					nextTop += item.GetThicknessWithFollowingRows();
				}
				else
				{
					int width = item.ComputeMaxBarThickness();
					item.SetBounds(new Rectangle(nextLeft, 0, width, base.ClientRectangle.Height));
					nextLeft += item.GetThicknessWithFollowingRows();
				}
			}
			if (_isHorizontal)
			{
				if ((CommandManager == null || !CommandManager._isBatchUpdating) && base.Height != nextTop - base.ClientRectangle.Top)
				{
					base.Height = nextTop - base.ClientRectangle.Top;
				}
			}
			else if ((CommandManager == null || !CommandManager._isBatchUpdating) && base.Width != nextLeft - base.ClientRectangle.Left)
			{
				base.Width = nextLeft - base.ClientRectangle.Left;
			}
			_isLayoutUpdateEnabled = wasLayoutUpdateEnabled;
		}

		internal void AddRowForBar(MxCommandBar commandBar)
		{
			int rowIndex = commandBar._rowIndex;
			if (rowIndex > _rows.Count || rowIndex < 0 || commandBar._isRestoringDockedPosition)
			{
				rowIndex = _rows.Count;
			}
			CommandBarHostLayoutState row = new CommandBarHostLayoutState(this, rowIndex, null);
			_rows.Insert(rowIndex, row);
			if (_isHorizontal)
			{
				row._bounds = new Rectangle(0, base.Bottom, base.ClientRectangle.Width, 0);
			}
			else
			{
				row._bounds = new Rectangle(base.Right, 0, 0, base.ClientRectangle.Height);
			}
			row.GetBars().Add(commandBar);
			LayoutRows();
		}

		internal bool HasAddedBar()
		{
			return _hasAddedBar;
		}

		internal void SetHasAddedBar(bool hasAddedBar)
		{
			if (_hasAddedBar != hasAddedBar)
			{
				_hasAddedBar = hasAddedBar;
			}
		}

		internal void InsertRowByDockedIndex(CommandBarHostLayoutState row, int dockedIndex, MxCommandBar commandBar)
		{
			int position = 0;
			foreach (CommandBarHostLayoutState item in _rows)
			{
				if (row == item)
				{
					continue;
				}
				if (item.GetSavedRowIndex() == -1)
				{
					if (item.GetRowIndex() < dockedIndex)
					{
						position++;
					}
				}
				else if (item.GetSavedRowIndex() < dockedIndex)
				{
					position++;
				}
				else if (item.GetSavedRowIndex() == dockedIndex)
				{
					commandBar._forceRowUpdate = true;
					commandBar.RowIndex = dockedIndex;
					return;
				}
			}
			row.SetSavedRowIndex(dockedIndex);
			if (position != row.GetRowIndex())
			{
				GetRows().Remove(row);
				GetRows().Insert(position, row);
			}
			LayoutRows();
		}

		internal void RemoveBarState(MxCommandBar commandBar)
		{
			if (_barStates == null)
			{
				return;
			}
			foreach (CommandBarState item in _barStates)
			{
				if (item._commandBar == commandBar)
				{
					_barStates.Remove(item);
					break;
				}
			}
		}

		internal void AddBarState(MxCommandBar commandBar, int rowIndex)
		{
			if (_barStates == null)
			{
				_barStates = new ArrayList();
			}
			CommandBarState value = new CommandBarState(commandBar, rowIndex);
			CommandBarRowComparer comparer = new CommandBarRowComparer();
			_barStates.Sort(comparer);
			_barStates.Add(value);
		}

		internal int GetOrAssignBarRowIndex(MxCommandBar commandBar, int defaultRowIndex)
		{
			CommandBarState uiCommandBarState = null;
			if (_barStates == null)
			{
				_barStates = new ArrayList();
			}
			else
			{
				foreach (CommandBarState item in _barStates)
				{
					if (item._commandBar == commandBar)
					{
						return item._rowIndex;
					}
				}
			}
			int probeIndex = defaultRowIndex;
			int assignedIndex = defaultRowIndex;
			foreach (CommandBarState state in _barStates)
			{
				if (state._rowIndex <= probeIndex)
				{
					probeIndex = state._rowIndex + 1;
					assignedIndex++;
				}
			}
			uiCommandBarState = new CommandBarState(commandBar, assignedIndex);
			_barStates.Add(uiCommandBarState);
			CommandBarRowComparer comparer = new CommandBarRowComparer();
			_barStates.Sort(comparer);
			return assignedIndex;
		}
	}
}
