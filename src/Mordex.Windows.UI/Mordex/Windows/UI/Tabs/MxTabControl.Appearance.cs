using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			return !_imageSize.Equals(DefaultImageSize);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = DefaultImageSize;
		}

		internal Color GetEffectiveBackColor()
		{
			if (BackColor == SystemColors.Control && (!VisualStyleSupport.IsThemingUsable(false)))
			{
				return RenderingHelper.GetSchemeColor(ColorName.Control);
			}
			return BackColor;
		}

		internal Color GetEffectiveForeColor()
		{
			if (ForeColor == SystemColors.ControlText)
			{
			}
			return base.ForeColor;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemSize()
		{
			return !_itemSize.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemSize()
		{
			ItemSize = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				ResetTabSizes();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		internal void SetThemeRendered(bool isThemeRendered)
		{
			if (_isThemeRendered != isThemeRendered)
			{
				_isThemeRendered = isThemeRendered;
				InvalidateLayout();
			}
		}
	}
}
