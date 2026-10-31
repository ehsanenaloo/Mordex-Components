using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal partial class CommandBarHostLayoutState : ICommandBarOwner
	{
		private CommandBarHostLayoutState _rowOwner;

		private MxCommandBarHost _host;

		private CommandBarCollection _bars;

		private CommandBarHostLayoutState _nextRow;

		private int _contentSize;

		private int _rowIndex;

		private int _savedRowIndex = -1;

		private bool _isHorizontal = true;

		private bool _isMovingBar;

		private bool _isPacking;

		public Rectangle _bounds = Rectangle.Empty;

		internal bool _isLayoutLocked;

		private bool _isLayoutEnabled = true;

		private bool _canResolveOverlap = true;

		bool ICommandBarOwner.IsInitializing
		{
			get
			{
				if (_host != null)
				{
					return _host._isInitializing;
				}
				return true;
			}
		}

		internal CommandBarHostLayoutState(MxCommandBarHost rebar, int rowIndex, CommandBarHostLayoutState rowOwner)
		{
			_host = rebar;
			_isHorizontal = rebar._isHorizontal;
			_bars = new CommandBarCollection(this);
			_rowIndex = rowIndex;
			_rowOwner = rowOwner;
		}

		internal bool HasRowOwner()
		{
			return _rowOwner != null;
		}

		public CommandBarCollection GetBars()
		{
			return _bars;
		}

		internal bool IsAnyBarMarked()
		{
			foreach (MxCommandBar item in GetBars())
			{
				if (item.RestoreLayoutState())
				{
					return true;
				}
			}
			return false;
		}

		internal void UpdateWrapFlags()
		{
			if (!IsContentFull())
			{
				return;
			}
			int rowSize = _bounds.Width;
			if (!_isHorizontal)
			{
				rowSize = _bounds.Height;
			}
			foreach (MxCommandBar item in GetBars())
			{
				if (!item.IsFullRowLayout() && item.GetFullLength() > rowSize)
				{
					item.SaveLayoutState();
				}
				else
				{
					item._savedWrappable = item.Wrappable;
				}
			}
			if (_bars.Count <= 1)
			{
				return;
			}
			foreach (MxCommandBar outerBar in _bars)
			{
				if (!outerBar.IsFullShown)
				{
					foreach (MxCommandBar innerBar in _bars)
					{
						innerBar.SaveLayoutState();
					}
				}
				else
				{
					outerBar._savedWrappable = outerBar.Wrappable;
				}
			}
		}

		internal bool TryWrapOverflow()
		{
			if (IsContentFull())
			{
				int rowSize = _bounds.Width;
				if (!_isHorizontal)
				{
					rowSize = _bounds.Height;
				}
				foreach (MxCommandBar item in GetBars())
				{
					if (!item.IsFullRowLayout() && item.GetFullLength() > rowSize)
					{
						item.SetWrappableSetting(InheritableBoolean.True);
						item.RecalculateLayout();
						return true;
					}
				}
				if (_bars.Count > 1)
				{
					foreach (MxCommandBar partialBar in _bars)
					{
						if (!partialBar.IsFullShown)
						{
							partialBar._forceRowUpdate = true;
							partialBar.RowIndex = partialBar.RowIndex;
							partialBar.RecalculateLayout(true);
							return true;
						}
					}
				}
			}
			return false;
		}

		internal bool IsContentFull()
		{
			if (_bars.Count > 0 && _bars[0].IsFullRowLayout())
			{
				return true;
			}
			if (_isHorizontal)
			{
				if (_contentSize >= _bounds.Width)
				{
					return true;
				}
			}
			else if (_contentSize >= _bounds.Height)
			{
				return true;
			}
			return false;
		}

		internal void RecalculateContentSize()
		{
			_contentSize = 0;
			foreach (MxCommandBar item in _bars)
			{
				if (item.Visible)
				{
					_contentSize += item.GetFullLength();
				}
			}
		}

		public int GetRowIndex()
		{
			return _rowIndex;
		}

		public void SetRowIndex(int rowIndex)
		{
			_rowIndex = rowIndex;
		}

		public int GetSavedRowIndex()
		{
			return _savedRowIndex;
		}

		public void SetSavedRowIndex(int uiRebarLayoutStateInteger)
		{
			_savedRowIndex = uiRebarLayoutStateInteger;
		}

		public bool GetIsHorizontal()
		{
			return _isHorizontal;
		}

		public void SetIsHorizontal(bool isHorizontal)
		{
			_isHorizontal = isHorizontal;
		}

		public void SetBounds(Rectangle newBounds)
		{
			UpdateBounds(newBounds, false);
		}

		public void UpdateBounds(Rectangle newBounds, bool forceUpdate)
		{
			if (_host._commandManager == null || (_host._commandManager.GetForm() != null && _host._commandManager.GetForm().WindowState == FormWindowState.Minimized) || (!(newBounds != _bounds) && !forceUpdate))
			{
				return;
			}
			Rectangle oldBounds = _bounds;
			_bounds = newBounds;
			if (_nextRow != null)
			{
				Rectangle childBounds = newBounds;
				if (_isHorizontal)
				{
					childBounds.Y += _bounds.Height;
				}
				else
				{
					childBounds.X += _bounds.Width;
				}
				_nextRow.SetBounds(childBounds);
			}
			int sizeChange = 0;
			sizeChange = (_isHorizontal ? ((newBounds.Width >= oldBounds.Width) ? (newBounds.Width - oldBounds.Width) : ((oldBounds.Width <= _contentSize) ? (newBounds.Width - oldBounds.Width) : (newBounds.Width - _contentSize))) : ((newBounds.Height >= oldBounds.Height) ? (newBounds.Height - oldBounds.Height) : ((oldBounds.Height <= _contentSize) ? (newBounds.Height - oldBounds.Height) : (newBounds.Height - _contentSize))));
			foreach (MxCommandBar item in _bars)
			{
				if (item.IsFullRowLayout())
				{
					item.Bounds = newBounds;
				}
				else if (_isHorizontal)
				{
					item.Top = _bounds.Top;
				}
				else
				{
					item.Left = _bounds.Left;
				}
			}
			if (sizeChange != 0)
			{
				RecalculateContentSize();
				if (IsContentFull())
				{
					if (_isHorizontal)
					{
						ResizeBarsWidth(sizeChange);
					}
					else
					{
						ResizeBarsHeight(sizeChange);
					}
				}
				else
				{
					NotifyLayoutChanged(sizeChange > 0);
				}
			}
			RebalanceRows();
		}

		private void OnControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			MxCommandBar uiCommandBar = (MxCommandBar)sender;
			if (uiCommandBar.Visible)
			{
				if (uiCommandBar.GetFullLength() == 0)
				{
					uiCommandBar.RecalculateLayout(true);
				}
				else
				{
					uiCommandBar.RecalculateLayout();
				}
			}
			RecalculateContentSize();
			NotifyLayoutChanged(true);
			_host.LayoutRows();
			if (uiCommandBar.Visible)
			{
				if (GetIsHorizontal())
				{
					ResolveOverlapHorizontal(uiCommandBar, 0);
				}
				else
				{
					ResolveOverlapVertical(uiCommandBar, 0);
				}
			}
			if (IsContentFull())
			{
				PackBars();
			}
			RebalanceRows();
		}

		internal bool IsLayoutSuspended()
		{
			if (_host != null && _host.CommandManager != null && _host.CommandManager._isBatchUpdating)
			{
				return true;
			}
			return _isLayoutLocked;
		}

		internal bool IsLayoutActive()
		{
			if (!_isLayoutEnabled)
			{
				return false;
			}
			if (_host != null)
			{
				return _host.IsLayoutUpdateEnabled();
			}
			return true;
		}

		private void OnControlResize(object sender, EventArgs eventArgs)
		{
			if (!_isMovingBar)
			{
				HandleBarResized(sender as MxCommandBar);
			}
		}

		internal void HandleBarResized(MxCommandBar uiCommandBar)
		{
			if (!IsLayoutActive() || _bars.Count == 0)
			{
				return;
			}
			RecalculateContentSize();
			if (_isHorizontal)
			{
				if (_bars.Count == 1 || _bounds.Height < uiCommandBar.Height)
				{
					_bounds.Height = uiCommandBar.Height;
					_host.StackRowsAfter(this);
				}
				int desiredWidth = GetDesiredBarExtent(uiCommandBar);
				if (desiredWidth > uiCommandBar.Width)
				{
					uiCommandBar.Width = desiredWidth;
					return;
				}
				if (!IsLayoutSuspended() && _bounds.Width > 0)
				{
					if (IsContentFull())
					{
						if (uiCommandBar.Left + uiCommandBar.Width > _bounds.Right)
						{
							uiCommandBar.MoveToLeft(_bounds.Right - uiCommandBar.Width);
						}
						else
						{
							PackBars();
						}
					}
					else
					{
						uiCommandBar.MoveToLeft(uiCommandBar.Left);
					}
				}
			}
			else
			{
				if (_bars.Count == 1 || _bounds.Width < uiCommandBar.Width)
				{
					_bounds.Width = uiCommandBar.Width;
					_host.StackRowsAfter(this);
				}
				int desiredHeight = GetDesiredBarExtent(uiCommandBar);
				if (desiredHeight > uiCommandBar.Height)
				{
					uiCommandBar.Height = desiredHeight;
					return;
				}
				if (!IsLayoutSuspended() && _bounds.Height > 0)
				{
					if (IsContentFull())
					{
						if (uiCommandBar.Top + uiCommandBar.Height <= _bounds.Bottom)
						{
							PackBars();
							return;
						}
						uiCommandBar.MoveToTop(_bounds.Bottom - uiCommandBar.Height);
					}
					else
					{
						uiCommandBar.MoveToTop(uiCommandBar.Top);
					}
				}
			}
			RebalanceRows();
		}

		internal bool IsRowFilled()
		{
			if (!IsLayoutActive())
			{
				return true;
			}
			if (_isHorizontal)
			{
				int usedWidth = 0;
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible)
					{
						usedWidth += item.Width;
					}
				}
				return usedWidth >= _bounds.Width;
			}
			int usedHeight = 0;
			foreach (MxCommandBar visibleBar in _bars)
			{
				if (visibleBar.Visible)
				{
					usedHeight += visibleBar.Height;
				}
			}
			return usedHeight >= _bounds.Height;
		}
	}
}
