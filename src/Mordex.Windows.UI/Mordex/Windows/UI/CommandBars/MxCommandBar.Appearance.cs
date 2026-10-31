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
		internal bool ShouldUseCompatibleTextRendering()
		{
			if (CommandManager != null)
			{
				return CommandManager.UseCompatibleTextRendering;
			}
			return false;
		}

		internal Color GetTextColor()
		{
			return base.ForeColor;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !(BackColor == Color.Transparent);
		}

		public new void ResetBackColor()
		{
			BackColor = Color.Transparent;
		}

		internal bool IsThemed()
		{
			return _isThemed;
		}

		internal void SetThemed(bool isThemed)
		{
			if (_isThemed != isThemed)
			{
				_isThemed = isThemed;
				if (_floatingWindow != null)
				{
					_floatingWindow.InvalidateLayout();
				}
				InvalidateLayout();
			}
		}

		internal bool IsFullRowLayout()
		{
			if (IsWrapping() || FullRow)
			{
				return true;
			}
			return false;
		}

		internal bool IsWrapping()
		{
			if (Wrappable == InheritableBoolean.Default)
			{
				if (_commandBarType == CommandBarType.Menu)
				{
					return true;
				}
			}
			else if (Wrappable == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal void SetWrappableSetting(InheritableBoolean uiCommandBarWrappable)
		{
			if (_commandBarWrappable != uiCommandBarWrappable)
			{
				_commandBarWrappable = uiCommandBarWrappable;
				ReassignSharedRow();
				InvalidateLayout();
			}
		}

		// A full row bar cannot share its row, so it is placed again when the row holds other bars.
		private void ReassignSharedRow()
		{
			if (IsFullRowLayout() && GetHostRow() != null && GetHostRow().GetBars().Count > 1)
			{
				_forceRowUpdate = true;
				RowIndex = RowIndex;
			}
		}

		internal TextAlign GetEffectiveTextAlign()
		{
			if (_textAlign == TextAlign.Default)
			{
				if (_commandManager != null)
				{
					return _commandManager.TextAlign;
				}
				return TextAlign.Right;
			}
			return _textAlign;
		}

		internal View GetEffectivePopupView()
		{
			if (_popupView == View.Default)
			{
				if (_commandManager != null)
				{
					return _commandManager.PopupView;
				}
				return GetEffectiveView();
			}
			return _popupView;
		}

		internal View GetEffectiveView()
		{
			if (_view == View.Default)
			{
				if (_commandManager != null)
				{
					return _commandManager.View;
				}
				return View.SmallIcons;
			}
			return View;
		}

		internal bool ShouldShowToolTips()
		{
			if (_showToolTips == InheritableBoolean.Default)
			{
				if (_commandManager != null)
				{
					return _commandManager.ShowToolTips;
				}
			}
			else if (_showToolTips == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal DropDownAnimation GetEffectiveAnimation()
		{
			if (_animation == DropDownAnimation.Default)
			{
				if (_commandManager != null)
				{
					if (_commandManager.MenuAnimation == DropDownAnimation.Default)
					{
						return DropDownAnimation.System;
					}
					return _commandManager.MenuAnimation;
				}
				return DropDownAnimation.System;
			}
			return _animation;
		}

		internal bool IsHorizontalLayout()
		{
			if (_floatingWindow != null)
			{
				return true;
			}
			return _isHorizontal;
		}

		internal void SetHorizontal(bool isHorizontal)
		{
			if (_isHorizontal == isHorizontal)
			{
				return;
			}
			_isHorizontal = isHorizontal;
			if (!_isHorizontal)
			{
				foreach (MxCommand command in Commands)
				{
					if (command.HasEmbeddedControl())
					{
						RemoveHostedControl(command);
					}
				}
			}
			InvalidateLayout();
		}
	}
}
