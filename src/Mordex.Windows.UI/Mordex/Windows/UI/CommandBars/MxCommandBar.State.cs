using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFloatingLocation()
		{
			return !_floatingLocation.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFloatingLocation()
		{
			FloatingLocation = Point.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFloatingSize()
		{
			return !_floatingSize.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFloatingSize()
		{
			FloatingSize = Size.Empty;
		}

		internal MxCommand GetSourceCommand()
		{
			return _sourceCommand;
		}

		internal void SetSourceCommand(MxCommand baseCommand)
		{
			MxCommand uiCommand = (_sourceCommand = new MxCommand(baseCommand));
			Commands.DetachAllAndReset();
			_commands = uiCommand.Commands;
		}

		internal CommandBarEditorDialog GetFloatingWindow()
		{
			return _floatingWindow;
		}

		internal void SetFloatingWindow(CommandBarEditorDialog uiCommandBarEditorDialog)
		{
			if (_floatingWindow != uiCommandBarEditorDialog)
			{
				if (_floatingWindow != null)
				{
					CommandBarEditorDialog previousWindow = _floatingWindow;
					_floatingWindow = uiCommandBarEditorDialog;
					previousWindow.Dispose();
				}
				else
				{
					_floatingWindow = uiCommandBarEditorDialog;
				}
			}
		}

		internal CommandBarHostLayoutState GetHostRow()
		{
			return _hostRow;
		}

		internal void SetHostRow(CommandBarHostLayoutState uiRebarLayoutState)
		{
			if (_hostRow != uiRebarLayoutState)
			{
				if (_isDragging && _hostRow != null)
				{
					_hostRow._isLayoutLocked = false;
				}
				_hostRow = uiRebarLayoutState;
				if (_isDragging && _hostRow != null)
				{
					_hostRow._isLayoutLocked = true;
				}
			}
		}

		internal MxCommandBarHost GetHost()
		{
			return _host;
		}

		internal void SetHost(MxCommandBarHost uiRebar)
		{
			if (_host != uiRebar)
			{
				if (_host != null)
				{
					_host.CommandBars.Remove(this);
				}
				_host = uiRebar;
			}
		}

		public void Close()
		{
			SetClosed(true);
		}

		public void Open()
		{
			SetClosed(false);
		}

		internal bool IsClosed()
		{
			if (base.Visible)
			{
				return false;
			}
			return true;
		}

		internal void SetClosed(bool isClosed)
		{
			if (IsClosed() == isClosed)
			{
				return;
			}
			_isClosed = isClosed;
			if (_isClosed)
			{
				if (GetFloatingWindow() != null)
				{
					SetFloatingWindow(null);
				}
				base.Visible = false;
				return;
			}
			base.Visible = true;
			if (DockStyle == BarsDockStyle.Floating && CommandManager != null && _floatingWindow == null)
			{
				CreateFloatingWindow();
			}
		}

		private void CreateFloatingWindow()
		{
			CommandBarEditorDialog floatingWindow = new CommandBarEditorDialog(this);
			Size = FloatingSize;
			floatingWindow.ShowAt(FloatingLocation);
			_floatingWindow = floatingWindow;
			RecalculateLayout();
		}

		internal Guid GetParentID()
		{
			return _parentID;
		}

		internal void SetParentID(Guid parentID)
		{
			_parentID = parentID;
		}

		public bool ShouldSerializaPopupSettings()
		{
			if (_popupSettings != null)
			{
				return _popupSettings.ShouldSerialize();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPopupSettings()
		{
			if (ShouldSerializaPopupSettings())
			{
				_popupSettings.Reset();
			}
		}
	}
}
