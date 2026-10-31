using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal partial class CommandControl
	{
		Control ICommandBarsElement.Control => this;

		MxCollection ICommandBarsElement.LayoutItems => _layoutItems;

		MxCommand ICommandBarsElement.SelectedCommand
		{
			get
			{
				return GetCustomizeCommand();
			}
			set
			{
				SetCustomizeCommand(value);
			}
		}

		Point ICommandMenuOwner.MousePoint
		{
			get
			{
				return _owner.MousePoint;
			}
			set
			{
				_owner.MousePoint = value;
			}
		}

		Size ICommandMenuOwner.LargeImageSize
		{
			get
			{
				if (_owner != null)
				{
					return _owner.LargeImageSize;
				}
				return Size.Empty;
			}
		}

		Size ICommandMenuOwner.SmallImageSize
		{
			get
			{
				if (_owner != null)
				{
					return _owner.SmallImageSize;
				}
				return Size.Empty;
			}
		}

		bool ICommandMenuOwner.UseSmallImages
		{
			get
			{
				if (_owner != null)
				{
					return _owner.UseSmallImages;
				}
				return true;
			}
		}

		Form ICommandMenuOwner.FindForm => _owner.FindForm;

		MxCommandManager ICommandMenuOwner.CommandManagerOwner => GetCommandManagerOwner();

		bool ICommandMenuOwner.ShowInfrequentItems
		{
			get
			{
				if (_owner != null)
				{
					return _owner.ShowInfrequentItems;
				}
				return true;
			}
			set
			{
				if (_owner != null)
				{
					_owner.ShowInfrequentItems = value;
				}
			}
		}

		bool ICommandMenuOwner.ShowToolTips => ShouldShowToolTips();

		bool ICommandMenuOwner.CanUseThemes => _canUseThemes;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Parent = IntPtr.Zero;
				createParams.Style = WsPopup;
				createParams.Style |= WsClipSiblings;
				createParams.Style |= WsClipChildren;
				createParams.ExStyle += WsExTopmost;
				createParams.ExStyle += WsExToolWindow;
				if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				{
					createParams.ExStyle += WsExNoActivate;
				}
				else if (Environment.OSVersion.Version.Major > 4)
				{
					createParams.ExStyle += WsExNoActivate;
				}
				return createParams;
			}
		}

		public override Cursor Cursor
		{
			get
			{
				Point pt = PointToClient(Cursor.Position);
				if (_tearOffBar != null && _tearOffBar._bounds.Contains(pt))
				{
					return Cursors.SizeAll;
				}
				CommandLayoutState uiCommandLayoutState = HitTestItem(pt.X, pt.Y);
				if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand() != null && uiCommandLayoutState.GetCommand().Cursor != null)
				{
					return uiCommandLayoutState.GetCommand().Cursor;
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		public MxCommandCollection Commands => _commands;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public CommandControl ChildMenu
		{
			get
			{
				return _childMenu;
			}
			set
			{
				if (_childMenu != value)
				{
					StopChildCloseTimer();
					_insertMarkIndex = -1;
					if (_childMenu != null)
					{
						_childMenu.Dispose();
					}
					_childMenu = value;
					_expandOnHover = false;
					Invalidate();
				}
			}
		}

		public DropDownAnimation EffectiveAnimation => _dropDownAnimation;


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public MxCommand SelectedCommand
		{
			get
			{
				if (GetOpenItem() != null)
				{
					return GetOpenItem().GetCommand();
				}
				return null;
			}
			set
			{
				SetOpenItem(_layoutItems.FindCommandItem(value));
			}
		}

		Control ILayoutItemOwner.ParentControl => this;
	}
}
