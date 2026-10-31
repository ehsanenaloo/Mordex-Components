using System;
using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal partial class CommandBarHostLayoutState
	{
		internal int ComputeMinOffsetX(MxCommandBar uiCommandBar)
		{
			int offset = 0;
			if (!IsContentFull())
			{
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible && item != uiCommandBar && item.GetPosition().X <= uiCommandBar.GetPosition().X && (item.GetPosition().X < uiCommandBar.GetPosition().X || item.Location.X < uiCommandBar.Location.X || (item.Location.X == uiCommandBar.Location.X && item.GetBarIndex() < uiCommandBar.GetBarIndex())))
					{
						offset += item.GetFullLength();
					}
				}
			}
			else
			{
				if (uiCommandBar.IsFullRowLayout())
				{
					return 0;
				}
				offset = 0;
				int trailingSize = 0;
				foreach (MxCommandBar otherBar in _bars)
				{
					if (!otherBar.Visible || otherBar == uiCommandBar)
					{
						continue;
					}
					if (otherBar.GetPosition().X <= uiCommandBar.GetPosition().X)
					{
						if (otherBar.GetPosition().X < uiCommandBar.GetPosition().X || otherBar.Location.X < uiCommandBar.Location.X || (otherBar.Location.X == uiCommandBar.Location.X && otherBar.GetBarIndex() < uiCommandBar.GetBarIndex()))
						{
							offset += otherBar.GetMinimumLength();
						}
						else
						{
							trailingSize += otherBar.GetFullLength();
						}
					}
					else
					{
						trailingSize += otherBar.GetFullLength();
					}
				}
				int rightLimit = _bounds.Right - trailingSize - uiCommandBar.GetFullLength();
				if (rightLimit > offset)
				{
					return rightLimit;
				}
			}
			return offset;
		}

		internal int ComputeMinOffsetY(MxCommandBar uiCommandBar)
		{
			int offset = 0;
			if (!IsContentFull())
			{
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible && item != uiCommandBar && item.GetPosition().Y <= uiCommandBar.GetPosition().Y && (item.GetPosition().Y < uiCommandBar.GetPosition().Y || item.Location.Y < uiCommandBar.Location.Y || (item.Location.Y == uiCommandBar.Location.Y && item.GetBarIndex() < uiCommandBar.GetBarIndex())))
					{
						offset += item.GetFullLength();
					}
				}
			}
			else
			{
				if (uiCommandBar.IsFullRowLayout())
				{
					return 0;
				}
				offset = 0;
				int trailingSize = 0;
				foreach (MxCommandBar otherBar in _bars)
				{
					if (!otherBar.Visible || otherBar == uiCommandBar)
					{
						continue;
					}
					if (otherBar.GetPosition().Y <= uiCommandBar.GetPosition().Y)
					{
						if (otherBar.GetPosition().Y < uiCommandBar.GetPosition().Y || otherBar.Location.Y < uiCommandBar.Location.Y || (otherBar.Location.Y == uiCommandBar.Location.Y && otherBar.GetBarIndex() < uiCommandBar.GetBarIndex()))
						{
							offset += otherBar.GetMinimumLength();
						}
						else
						{
							trailingSize += otherBar.GetFullLength();
						}
					}
					else
					{
						trailingSize += otherBar.GetFullLength();
					}
				}
				int bottomLimit = _bounds.Bottom - trailingSize - uiCommandBar.GetFullLength();
				if (bottomLimit > offset)
				{
					return bottomLimit;
				}
			}
			return offset;
		}

		internal int ComputeMaxOffsetY(MxCommandBar uiCommandBar)
		{
			int offset = 0;
			if (!IsContentFull())
			{
				offset = _bounds.Bottom - uiCommandBar.GetFullLength();
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible && item != uiCommandBar && item.GetPosition().Y >= uiCommandBar.GetPosition().Y && item.GetPosition().Y < uiCommandBar.GetPosition().Y)
					{
						offset -= item.GetFullLength();
					}
				}
			}
			else
			{
				int trailingSize = uiCommandBar.GetMinimumLength();
				foreach (MxCommandBar otherBar in _bars)
				{
					if (!otherBar.Visible || otherBar == uiCommandBar)
					{
						continue;
					}
					if (otherBar.GetPosition().Y <= uiCommandBar.GetPosition().Y)
					{
						if (otherBar.GetPosition().Y < uiCommandBar.GetPosition().Y || otherBar.Location.Y > uiCommandBar.Location.Y || (otherBar.Location.Y == uiCommandBar.Location.Y && otherBar.GetBarIndex() < uiCommandBar.GetBarIndex()))
						{
							offset += otherBar.GetFullLength();
						}
						else
						{
							trailingSize += otherBar.GetMinimumLength();
						}
					}
					else
					{
						trailingSize += otherBar.GetMinimumLength();
					}
				}
				if (offset + trailingSize > _bounds.Bottom)
				{
					offset = _bounds.Bottom - trailingSize;
				}
			}
			if (offset < 0)
			{
				return 0;
			}
			return offset;
		}

		internal int ComputeMaxOffsetX(MxCommandBar uiCommandBar)
		{
			int offset = 0;
			if (!IsContentFull())
			{
				offset = _bounds.Right - uiCommandBar.GetFullLength();
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible && item != uiCommandBar && item.GetPosition().X >= uiCommandBar.GetPosition().X && item.Location.X > uiCommandBar.Location.X)
					{
						offset -= item.GetFullLength();
					}
				}
			}
			else
			{
				int trailingSize = uiCommandBar.GetMinimumLength();
				foreach (MxCommandBar otherBar in _bars)
				{
					if (!otherBar.Visible || otherBar == uiCommandBar)
					{
						continue;
					}
					if (otherBar.GetPosition().X <= uiCommandBar.GetPosition().X)
					{
						if (otherBar.GetPosition().X < uiCommandBar.GetPosition().X || otherBar.Location.X > uiCommandBar.Location.X || (otherBar.Location.X == uiCommandBar.Location.X && otherBar.GetBarIndex() < uiCommandBar.GetBarIndex()))
						{
							offset += otherBar.GetFullLength();
						}
						else
						{
							trailingSize += otherBar.GetMinimumLength();
						}
					}
					else
					{
						trailingSize += otherBar.GetMinimumLength();
					}
				}
				if (offset + trailingSize > _bounds.Right)
				{
					offset = _bounds.Right - trailingSize;
				}
			}
			if (offset < 0)
			{
				return 0;
			}
			return offset;
		}
	}
}
