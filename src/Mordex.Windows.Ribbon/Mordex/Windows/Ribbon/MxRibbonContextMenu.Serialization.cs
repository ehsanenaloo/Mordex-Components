using System;
using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonContextMenu
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				RefreshChildMenuLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSmallImageSize()
		{
			return !_smallImageSize.Equals(Size.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSmallImageSize()
		{
			SmallImageSize = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageSize()
		{
			return !_largeImageSize.Equals(Size.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageSize()
		{
			LargeImageSize = Size.Empty;
		}
	}
}
