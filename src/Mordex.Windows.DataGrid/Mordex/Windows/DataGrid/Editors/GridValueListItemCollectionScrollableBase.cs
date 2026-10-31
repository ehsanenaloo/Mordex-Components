using System;
using System.Collections;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class GridValueListItemCollectionScrollableBase : ScrollableBase
	{
		internal class ValueListAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleObject Parent
			{
				get
				{
					AccessibleObject accessibleObject = null;
					if (GetOwnerControl() != null && GetOwnerControl()._dropListOwner != null)
					{
						accessibleObject = GetOwnerControl()._dropListOwner.AccessibleObject;
					}
					if (accessibleObject != null)
					{
						return accessibleObject;
					}
					return base.Parent;
				}
			}

			public override string Name => "MordexComboDropDown";

			internal ValueListAccessibleObject(GridValueListItemCollectionScrollableBase owner)
				: base(owner)
			{
			}

			private GridValueListItemCollectionScrollableBase GetOwnerControl()
			{
				return (GridValueListItemCollectionScrollableBase)base.Owner;
			}
		}

		private ImageList _controlImageList;

		private bool _showImages;

		private GridValueListItemCollection _gridValueListItemCollection;

		private TextLayoutRenderer _textLayoutRenderer;

		private int _lastVisibleIndex = -1;

		private int _rowHeight;

		internal int _maxVisibleRows = 8;

		private Timer _timer;

		private bool _wasPressedInside;

		private Brush _foreBrush;

		private Brush _gridValueListItemCollectionScrollableBaseBrush;

		protected IGridDropListOwner _dropListOwner;

		private int _firstVisibleIndex;

		private GridValueListItem _gridValueListItem;

		private GridFormatStyle _gridFormatStyle;

		private IntPtr _dropDownHandle = IntPtr.Zero;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 136;
				return createParams;
			}
		}

		internal GridValueListItemCollectionScrollableBase()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			base.BorderStyle = BorderStyle.Flat;
			GetVScroll().MouseUp += OnControlMouseUp;
			RightToLeft = RightToLeft.Inherit;
			base.BackColor = SystemColors.Window;
			base.ForeColor = SystemColors.WindowText;
			Text = "CellEditor_DropDown";
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void OnParentChanged(EventArgs eventArgs)
		{
			base.OnParentChanged(eventArgs);
		}

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (IsDropDownOpenState())
			{
				GridHelper.SetCapture(this, true);
			}
		}

		public ImageList GetItemImageList()
		{
			return _controlImageList;
		}

		public void SetItemImageList(ImageList controlImageList)
		{
			if (controlImageList != _controlImageList)
			{
				_controlImageList = controlImageList;
			}
		}

		protected override ImageList GetControlImageList()
		{
			return _controlImageList;
		}

		public void SetShowImages(bool gridValueListItemCollectionScrollableBaseBoolean)
		{
			_showImages = gridValueListItemCollectionScrollableBaseBoolean;
		}

		public void AssignItems(GridValueListItemCollection gridValueListItemCollection)
		{
			if (gridValueListItemCollection != _gridValueListItemCollection)
			{
				_gridValueListItemCollection = gridValueListItemCollection;
				_gridValueListItem = null;
			}
		}

		public TextLayoutRenderer ComputeTextLayoutRenderer()
		{
			if (_textLayoutRenderer == null)
			{
				_textLayoutRenderer = new TextLayoutRenderer(!base.UseCompatibleTextRendering);
				_textLayoutRenderer.UpdateRightToLeft(RightToLeft);
			}
			return _textLayoutRenderer;
		}

		protected void SizeToItems(int value)
		{
			if (_gridValueListItemCollection == null || _gridValueListItemCollection.Count == 0)
			{
				int height = (int)Font.GetHeight() + 2;
				if (GetItemImageList() != null && GetItemImageList().ImageSize.Height > height)
				{
					height = GetItemImageList().ImageSize.Height;
				}
				base.Height = height + _borderWidth + 2;
				_rowHeight = height;
				base.SetVerticalScrollRequired(false);
				return;
			}
			Graphics graphics = CreateGraphics();
			graphics.PageUnit = GraphicsUnit.Pixel;
			int otherHeight = 0;
			int width = 0;
			int otherValue = 0;
			int rowHeight = (int)Font.GetHeight(graphics) + 2;
			int otherRowHeight = 0;
			if (_controlImageList != null)
			{
				otherRowHeight = _controlImageList.ImageSize.Height + 2;
			}
			foreach (GridValueListItem item in (IEnumerable)_gridValueListItemCollection)
			{
				otherValue++;
				int nextValue = 2;
				if (item.Image != null)
				{
					nextValue += item.Image.Width + 2;
					if (otherRowHeight < item.Image.Height + 2)
					{
						otherRowHeight = item.Image.Height + 2;
					}
				}
				else if (_controlImageList != null && (item.ImageIndex >= 0 || item.ImageKey.Length > 0))
				{
					nextValue += _controlImageList.ImageSize.Width + 2;
				}
				if (item.Text.Length != 0)
				{
					nextValue += (int)ComputeTextLayoutRenderer().MeasureText(graphics, item.Text, Font, 0).Width + 2;
				}
				if (nextValue > width)
				{
					width = nextValue;
				}
			}
			_rowHeight = rowHeight;
			if (otherRowHeight > _rowHeight)
			{
				_rowHeight = otherRowHeight;
			}
			otherHeight = ((_gridValueListItemCollection.Count > _maxVisibleRows) ? (_maxVisibleRows * _rowHeight) : (_gridValueListItemCollection.Count * _rowHeight));
			width += _borderWidth * 2;
			otherHeight += _borderWidth * 2;
			if (_gridValueListItemCollection.Count > _maxVisibleRows)
			{
				width += SystemInformation.VerticalScrollBarWidth;
			}
			graphics.Dispose();
			if (width < value)
			{
				width = value;
			}
			base.Size = new Size(width, otherHeight);
			base.Width = width;
			if (_gridValueListItemCollection.Count > _maxVisibleRows)
			{
				ConfigureVerticalScrollBar(0, _gridValueListItemCollection.Count - 1, _maxVisibleRows, _firstVisibleIndex);
				base.SetVerticalScrollRequired(true);
			}
			else
			{
				SetFirstVisibleIndex(0);
				base.SetVerticalScrollRequired(false);
			}
		}

		private int GetItemIndexAt(MouseButtons mouseButtons, int value, int otherValue)
		{
			if (base.IsDisposed || base.Disposing)
			{
				return -1;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			int containsCore = 0;
			if (_gridValueListItemCollection == null || (!clientRectangle.Contains(value, otherValue) && mouseButtons == MouseButtons.None))
			{
				return -1;
			}
			if (otherValue < clientRectangle.Top)
			{
				containsCore = _firstVisibleIndex - 1;
				if (containsCore < 0)
				{
					containsCore = 0;
				}
				return containsCore;
			}
			if (otherValue > clientRectangle.Bottom)
			{
				containsCore = _lastVisibleIndex + 1;
				if (containsCore > _gridValueListItemCollection.Count - 1)
				{
					containsCore = _gridValueListItemCollection.Count - 1;
				}
				return containsCore;
			}
			int nextValue = otherValue / _rowHeight;
			return _firstVisibleIndex + nextValue;
		}

		private void UpdateTimer()
		{
			_timer = new Timer();
			_timer.Interval = Timing.DeferredActionInterval;
			_timer.Tick += OnTimerTick;
			_timer.Enabled = true;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (base.Visible)
			{
				GridHelper.SetCapture(this, true);
			}
			if (_timer != null)
			{
				_timer.Stop();
				_timer.Tick -= OnTimerTick;
				_timer.Dispose();
				_timer = null;
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.Capture = false;
			base.OnMouseUp(eventArgs);
			if (!_wasPressedInside)
			{
				if (base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					HideDropDown(true);
				}
				else
				{
					UpdateTimer();
				}
			}
			else
			{
				HideDropDown(!base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y));
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
			{
				base.OnMouseDown(eventArgs);
				if (eventArgs.Button == MouseButtons.Left)
				{
					int containsCore = GetItemIndexAt(eventArgs.Button, eventArgs.X, eventArgs.Y);
					if (containsCore >= 0 && containsCore < _gridValueListItemCollection.Count)
					{
						SetSelectedItem(_gridValueListItemCollection[containsCore]);
					}
					_wasPressedInside = true;
				}
				return;
			}
			Point point = new Point(eventArgs.X, eventArgs.Y);
			Control control = null;
			try
			{
				control = GetChildAtPoint(point);
			}
			finally
			{
			}
			if (control == GetVScroll())
			{
				point = PointToScreen(point);
				point = control.PointToClient(point);
				((ScrollBarControl)control).ProcessMouseInput(eventArgs, point.X, point.Y);
			}
			else
			{
				base.OnMouseDown(eventArgs);
				HideDropDown(true);
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			DrawItems(eventArgs.Graphics);
			base.DrawControlBorder(eventArgs.Graphics);
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
		}

		private void DrawItems(Graphics graphics)
		{
			if (_gridValueListItemCollection != null && _gridValueListItemCollection.Count > 0)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				int top = clientRectangle.Top;
				for (int i = _firstVisibleIndex; i < _gridValueListItemCollection.Count; i++)
				{
					_lastVisibleIndex = i;
					_ = _gridValueListItemCollection[i];
					Rectangle rectangle = new Rectangle(clientRectangle.Left, top, clientRectangle.Width, _rowHeight);
					DrawItem(graphics, i, rectangle);
					top += _rowHeight;
					if (top >= clientRectangle.Bottom)
					{
						return;
					}
				}
			}
			else
			{
				graphics.FillRectangle(GetBackBrush(), base.ClientRectangle);
			}
			DisposeResources();
		}

		private Brush GetForeBrush()
		{
			if (_foreBrush == null)
			{
				_foreBrush = new SolidBrush(base.ForeColor);
			}
			return _foreBrush;
		}

		private Brush GetBackBrush()
		{
			if (_gridValueListItemCollectionScrollableBaseBrush == null)
			{
				_gridValueListItemCollectionScrollableBaseBrush = new SolidBrush(base.BackColor);
			}
			return _gridValueListItemCollectionScrollableBaseBrush;
		}

		private void DisposeResources()
		{
			if (_gridValueListItemCollectionScrollableBaseBrush != null)
			{
				_gridValueListItemCollectionScrollableBaseBrush.Dispose();
				_gridValueListItemCollectionScrollableBaseBrush = null;
			}
			if (_foreBrush != null)
			{
				_foreBrush.Dispose();
				_foreBrush = null;
			}
			GetSelectedFormatStyle().DisposeResources();
		}

		private void DrawItem(Graphics graphics, int value, Rectangle rectangle)
		{
			GridValueListItem gridValueListItem = _gridValueListItemCollection[value];
			Brush foreBrush;
			if (gridValueListItem == GetSelectedItem())
			{
				graphics.FillRectangle(GetBackBrush(), rectangle);
				Brush brush = GetSelectedFormatStyle().GetOrCreateBackBrush(rectangle);
				if (brush == null)
				{
					graphics.FillRectangle(SystemBrushes.Highlight, rectangle);
				}
				else
				{
					graphics.FillRectangle(brush, rectangle);
				}
				foreBrush = GetSelectedFormatStyle().GetForeBrush();
				if (foreBrush == null)
				{
					foreBrush = SystemBrushes.HighlightText;
				}
			}
			else
			{
				graphics.FillRectangle(GetBackBrush(), rectangle);
				foreBrush = GetForeBrush();
			}
			_ = rectangle.Left;
			int imageIndex = gridValueListItem.ImageIndex;
			if (_controlImageList != null && gridValueListItem.ImageKey.Length > 0)
			{
				imageIndex = _controlImageList.Images.IndexOfKey(gridValueListItem.ImageKey);
			}
			GridHelper.DrawImageAndText(graphics, foreBrush, gridValueListItem.Text, gridValueListItem.Image, rectangle, ImageHorizontalAlignment.BeforeText, ImageVerticalAlignment.Center, ComputeTextLayoutRenderer(), Font, RightToLeft == RightToLeft.Yes, imageIndex, _controlImageList, false);
		}

		protected void SetFirstVisibleIndex(int firstVisibleIndex)
		{
			if (!base.IsDisposed)
			{
				if (_gridValueListItemCollection == null)
				{
					firstVisibleIndex = 0;
				}
				else if (firstVisibleIndex > _gridValueListItemCollection.Count - _maxVisibleRows)
				{
					firstVisibleIndex = _gridValueListItemCollection.Count - _maxVisibleRows;
				}
				if (firstVisibleIndex < 0)
				{
					firstVisibleIndex = 0;
				}
				if (firstVisibleIndex != _firstVisibleIndex)
				{
					_firstVisibleIndex = firstVisibleIndex;
					GetVScroll().SetScrollValue(_firstVisibleIndex);
					Invalidate();
				}
			}
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			switch (scrollCode)
			{
			case 0:
				SetFirstVisibleIndex(GetVScroll().GetScrollValue());
				break;
			case 1:
				SetFirstVisibleIndex(GetVScroll().GetScrollValue());
				break;
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				int containsCore = GetItemIndexAt(eventArgs.Button, eventArgs.X, eventArgs.Y);
				if (containsCore >= 0 && containsCore < _gridValueListItemCollection.Count)
				{
					SetSelectedItem(_gridValueListItemCollection[containsCore]);
				}
			}
		}

		public GridValueListItem GetSelectedItem()
		{
			return _gridValueListItem;
		}

		public void SetSelectedItem(GridValueListItem gridValueListItem)
		{
			if (gridValueListItem == _gridValueListItem)
			{
				return;
			}
			_gridValueListItem = gridValueListItem;
			if (_gridValueListItem != null)
			{
				int firstVisibleIndex = _gridValueListItemCollection.IndexOf(_gridValueListItem);
				if (firstVisibleIndex < _firstVisibleIndex)
				{
					SetFirstVisibleIndex(firstVisibleIndex);
				}
				else if (firstVisibleIndex > _lastVisibleIndex)
				{
					SetFirstVisibleIndex(firstVisibleIndex - 7);
				}
			}
			Invalidate();
		}

		private void SelectItemAt(int value)
		{
			if (_gridValueListItemCollection != null)
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > _gridValueListItemCollection.Count - 1)
				{
					value = _gridValueListItemCollection.Count - 1;
				}
				if (value != -1)
				{
					SetSelectedItem(_gridValueListItemCollection[value]);
				}
			}
		}

		internal virtual bool DropedProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
			{
				int value = ((_gridValueListItem != null) ? (_gridValueListItemCollection.IndexOf(_gridValueListItem) - 7) : 0);
				SelectItemAt(value);
				break;
			}
			case Keys.Next:
			{
				int otherValue = ((_gridValueListItem != null) ? (_gridValueListItemCollection.IndexOf(_gridValueListItem) + 7) : 0);
				SelectItemAt(otherValue);
				break;
			}
			case Keys.Down:
			{
				int nextValue = ((_gridValueListItem != null) ? (_gridValueListItemCollection.IndexOf(_gridValueListItem) + 1) : 0);
				SelectItemAt(nextValue);
				break;
			}
			case Keys.Up:
			{
				int previousValue = ((_gridValueListItem != null) ? (_gridValueListItemCollection.IndexOf(_gridValueListItem) - 1) : 0);
				SelectItemAt(previousValue);
				break;
			}
			case Keys.Home:
				SelectItemAt(0);
				break;
			case Keys.End:
				if (_gridValueListItemCollection != null)
				{
					SelectItemAt(_gridValueListItemCollection.Count);
				}
				break;
			case Keys.Return:
			case Keys.F4:
			case Keys.Return | Keys.Shift:
			case Keys.Return | Keys.Control:
			case Keys.Return | Keys.Alt:
			case Keys.Up | Keys.Alt:
				if (IsDropDownOpenState())
				{
					HideDropDown(false);
				}
				break;
			case Keys.Escape:
				if (IsDropDownOpenState())
				{
					HideDropDown(true);
				}
				break;
			default:
				return false;
			}
			return true;
		}

		internal GridFormatStyle GetSelectedFormatStyle()
		{
			if (_gridFormatStyle == null)
			{
				_gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.SelectedFormatStyle);
			}
			return _gridFormatStyle;
		}

		internal void SetSelectedFormatStyle(GridFormatStyle gridFormatStyle)
		{
			_gridFormatStyle = gridFormatStyle;
		}

		internal void ShowDropDownAt(Rectangle otherBounds, IGridDropListOwner iGridElement)
		{
			Cursor = Cursors.Arrow;
			base.Visible = false;
			base.Parent = iGridElement as Control;
			_dropListOwner = iGridElement;
			base.Width = otherBounds.Width;
			SizeToItems(otherBounds.Width);
			_dropDownHandle = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			Point location = new Point(otherBounds.X, otherBounds.Bottom + 1);
			Screen screen = Screen.FromRectangle(otherBounds);
			if (screen == null)
			{
				screen = Screen.PrimaryScreen;
			}
			Rectangle bounds = screen.Bounds;
			if (location.X + base.Width > bounds.Right)
			{
				location.X = bounds.Right - base.Width;
			}
			if (location.X < bounds.Left)
			{
				location.X = bounds.Left;
			}
			if (location.Y + base.Height > bounds.Bottom)
			{
				if (otherBounds.Y > bounds.Bottom - otherBounds.Bottom)
				{
					location.Y = otherBounds.Y - base.Height;
					if (location.Y < bounds.Top)
					{
						location.Y = bounds.Top;
						base.Height = otherBounds.Y;
					}
				}
				else
				{
					base.Height = bounds.Bottom - otherBounds.Bottom;
				}
			}
			base.Location = location;
			base.SetDropDownOpenState(true);
			NativeMethods.SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 3);
			base.Visible = true;
			GetVScroll().RelayoutAndInvalidate();
			Update();
			GridHelper.SelectAndFocus(this);
		}

		internal void HideDropDown(bool cancel)
		{
			if (IsDropDownOpenState())
			{
				base.SetDropDownOpenState(false);
				GridHelper.SetCapture(this, false);
				NativeMethods.ShowWindow(base.Handle, 0);
				if (_dropDownHandle != IntPtr.Zero)
				{
					NativeMethods.SetParent(base.Handle, _dropDownHandle);
					_dropDownHandle = IntPtr.Zero;
				}
				_wasPressedInside = false;
				base.Parent = null;
				if (_dropListOwner != null)
				{
					_dropListOwner.OnDropListHide(cancel);
					_dropListOwner = null;
				}
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WindowMessages.CancelMode)
			{
				HideDropDown(true);
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);
			NativeMethods.SetProp(base.Handle, "PROP_ISCELLDROPDOWN", new IntPtr(1));
		}

		protected override void OnHandleDestroyed(EventArgs eventArgs)
		{
			NativeMethods.RemoveProp(base.Handle, "PROP_ISCELLDROPDOWN");
			base.OnHandleDestroyed(eventArgs);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new ValueListAccessibleObject(this);
		}
	}
}
