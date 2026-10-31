using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public abstract partial class DockPanelBase
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBorderPanelColor()
		{
			return !_borderPanelColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBorderPanelColor()
		{
			BorderPanelColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDockState()
		{
			if (ParentGroup != null)
			{
				return _dockState != PanelDockState.Docked;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDockState()
		{
			if (ParentGroup == null)
			{
				DockState = PanelDockState.Docked;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFloatingLocation()
		{
			if (_floatingLayoutState.GetTopLevelState() == _floatingLayoutState)
			{
				return FloatingLocation != new Point(-1, -1);
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFloatingLocation()
		{
			FloatingLocation = new Point(-1, -1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAllowPanelDrop()
		{
			return _allowPanelDrop != InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAllowPanelDrop()
		{
			AllowPanelDrop = InheritableBoolean.Default;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePanelManager()
		{
			if (ParentGroup == null)
			{
				return PanelManager != null;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDockStyle()
		{
			if (ParentGroup == null)
			{
				return DockStyle != PanelDockStyle.Left;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDockStyle()
		{
			if (ParentGroup == null)
			{
				DockStyle = PanelDockStyle.Left;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaximumSize()
		{
			MaximumSize = new Size(-1, -1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaximumSize()
		{
			return _maximumSize != new Size(-1, -1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinimumSize()
		{
			_minimumSize = new Size(-1, -1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinimumSize()
		{
			return _minimumSize != new Size(-1, -1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBorderCaptionColor()
		{
			return !_borderCaptionColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBorderCaptionColor()
		{
			BorderCaptionColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFloatingSize()
		{
			if (_floatingLayoutState.GetTopLevelState() == _floatingLayoutState)
			{
				return FloatingSize != DefaultSize;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFloatingSize()
		{
			FloatingSize = DefaultSize;
		}
	}
}
