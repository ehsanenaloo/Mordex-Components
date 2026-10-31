using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class EditBase
	{
		public Color DisabledBackColor
		{
			get
			{
				if (_disabledBackground.IsEmpty)
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.InputBackDisabled);
				}
				return _disabledBackground;
			}
			set
			{
				if (value.A < byte.MaxValue)
				{
					throw new ArgumentException("DisabledBackColor can not be transparent.");
				}
				if (_disabledBackground != value)
				{
					_disabledBackground = value;
					InvalidateOnColorChange();
				}
			}
		}

		public Color DisabledForeColor
		{
			get
			{
				if (!ShouldSerializeDisabledForeColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.TextDisabled);
				}
				return _disabledForeground;
			}
			set
			{
				if (value.A < byte.MaxValue)
				{
					throw new ArgumentException("DisabledForeColor can not be transparent.");
				}
				if (_disabledForeground != value)
				{
					_disabledForeground = value;
					InvalidateOnColorChange();
				}
			}
		}

		[DefaultValue(ImageLayout.None)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout => ImageLayout.None;

		[DefaultValue(true)]
		public virtual bool ShortcutsEnabled
		{
			get
			{
				return _shortcutsEnabled;
			}
			set
			{
				_shortcutsEnabled = value;
				if (TextBox != null)
				{
					TextBox.ShortcutsEnabled = value;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Padding Padding
		{
			get
			{
				return new Padding(0);
			}
			set
			{
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new AutoScaleMode AutoScaleMode
		{
			get
			{
				return AutoScaleMode.None;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new SizeF AutoScaleDimensions
		{
			get
			{
				return default(SizeF);
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (!_useCompatibleTextRendering.HasValue || _useCompatibleTextRendering.Value != value)
				{
					_useCompatibleTextRendering = value;
					RecalculateLayout();
				}
			}
		}

		protected virtual AccessibleObject AccessibleObjectCore => null;

		bool IRibbonEditControl.ActAsRibbonEditControl
		{
			get
			{
				return _actAsRibbonEditControl;
			}
			set
			{
				if (_actAsRibbonEditControl == value)
				{
					return;
				}
				_actAsRibbonEditControl = value;
				if (_actAsRibbonEditControl)
				{
					if (_isMouseOver)
					{
						BackColor = Color.White;
					}
					else if (!base.ContainsFocus)
					{
						BackColor = Color.White;
					}
				}
				if (!ShouldSerializeBackColor())
				{
					ResetBackColor();
				}
				RecalculateLayout();
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[Description("Event fired when the user clicks in the button of the control")]
		public event EventHandler ButtonClick
		{
			add
			{
				base.Events.AddHandler(_editBaseValue, value);
			}
			remove
			{
				base.Events.RemoveHandler(_editBaseValue, value);
			}
		}

		[Description("Event fired when the ReadOnly property changes")]
		public event EventHandler ReadOnlyChanged
		{
			add
			{
				base.Events.AddHandler(_readOnlyChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_readOnlyChangedEventKey, value);
			}
		}

		[Description("Event fired when the ImageList property changes.")]
		public event EventHandler ImageListChanged
		{
			add
			{
				base.Events.AddHandler(_imageListChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_imageListChangedKey, value);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new event EventHandler AutoSizeChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new event ScrollEventHandler Scroll
		{
			add
			{
			}
			remove
			{
			}
		}
	}
}
