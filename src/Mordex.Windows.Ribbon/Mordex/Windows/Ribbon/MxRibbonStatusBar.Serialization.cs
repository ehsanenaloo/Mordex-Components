using System;
using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageSize()
		{
			if (_imageSize.Width != 16 || _imageSize.Height != 16)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageSize()
		{
			ImageSize = new Size(16, 16);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}
	}
}
