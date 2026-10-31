using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxButton
	{
		internal bool IsHot()
		{
			if (!_isHot && IsDropDownHot())
			{
				return true;
			}
			return _isHot;
		}

		internal void SetHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				if (_isHot)
				{
					UpdateToolTip(_toolTipText);
				}
				else
				{
					UpdateToolTip("");
				}
				Invalidate();
			}
		}

		internal bool IsPressed()
		{
			if (!_isPressed && IsDropDownPressed() && ResolveButtonStyle() == ButtonStyle.DropDown)
			{
				return true;
			}
			return _isPressed;
		}

		internal void SetPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
				Invalidate();
			}
		}

		private bool IsDropDownPressed()
		{
			if (_isDropDownOpen)
			{
				return true;
			}
			if (!_isDropDownPressed && _isPressed && ResolveButtonStyle() == ButtonStyle.DropDown)
			{
				return true;
			}
			return _isDropDownPressed;
		}

		private void SetDropDownPressed(bool isDropDownPressed)
		{
			if (_isDropDownPressed != isDropDownPressed)
			{
				_isDropDownPressed = isDropDownPressed;
				Invalidate();
			}
		}

		private bool IsDropDownHot()
		{
			if (_isDropDownOpen)
			{
				return true;
			}
			return _isDropDownHot;
		}

		private void SetDropDownHot(bool isDropDownHot)
		{
			if (_isDropDownHot != isDropDownHot)
			{
				_isDropDownHot = isDropDownHot;
				Invalidate();
			}
		}

		internal void InvalidateLayoutAndPaint()
		{
			_isLayoutDirty = true;
			_preferredSize = Size.Empty;
			Invalidate();
		}

		internal Image GetCurrentImage(bool isHot, bool isPressed)
		{
			Image image = Image;
			if (image == null && GetAssociatedCommand() != null)
			{
				image = GetAssociatedCommand().Image;
			}
			if (image == null && ImageList != null)
			{
				int imageIndex = GetImageIndex(isHot, isPressed);
				if (imageIndex >= 0 && imageIndex < ImageList.Images.Count)
				{
					return ImageList.Images[imageIndex];
				}
			}
			return image;
		}

		internal int GetImageIndex(bool isHot, bool isPressed)
		{
			if (Image == null && ImageList != null)
			{
				if (!base.Enabled || (GetAssociatedCommand() != null && !GetAssociatedCommand().IsEnabled))
				{
					if (_disabledImageKey.Length > 0 && ImageList.Images.ContainsKey(_disabledImageKey))
					{
						return ImageList.Images.IndexOfKey(_disabledImageKey);
					}
					if (_disabledImageIndex >= 0 && _disabledImageIndex < ImageList.Images.Count)
					{
						return _disabledImageIndex;
					}
					if (GetAssociatedCommand() != null)
					{
						if (GetAssociatedCommand().DisabledImageKey.Length > 0 && ImageList.Images.ContainsKey(GetAssociatedCommand().DisabledImageKey))
						{
							return ImageList.Images.IndexOfKey(GetAssociatedCommand().DisabledImageKey);
						}
						if (GetAssociatedCommand().DisabledImageIndex >= 0 && GetAssociatedCommand().DisabledImageIndex < ImageList.Images.Count)
						{
							return GetAssociatedCommand().DisabledImageIndex;
						}
					}
				}
				else if (isPressed && isHot)
				{
					if (_pressedImageKey.Length > 0 && ImageList.Images.ContainsKey(_pressedImageKey))
					{
						return ImageList.Images.IndexOfKey(_pressedImageKey);
					}
					if (_pressedImageIndex >= 0 && _pressedImageIndex < ImageList.Images.Count)
					{
						return _pressedImageIndex;
					}
					if (GetAssociatedCommand() != null)
					{
						if (GetAssociatedCommand().SelectedImageKey.Length > 0 && ImageList.Images.ContainsKey(GetAssociatedCommand().SelectedImageKey))
						{
							return ImageList.Images.IndexOfKey(GetAssociatedCommand().SelectedImageKey);
						}
						if (GetAssociatedCommand().SelectedImageIndex >= 0 && GetAssociatedCommand().SelectedImageIndex < ImageList.Images.Count)
						{
							return GetAssociatedCommand().SelectedImageIndex;
						}
					}
				}
				else if (isHot)
				{
					if (_hotImageKey.Length > 0 && ImageList.Images.ContainsKey(_hotImageKey))
					{
						return ImageList.Images.IndexOfKey(_hotImageKey);
					}
					if (_hotImageIndex >= 0 && _hotImageIndex < ImageList.Images.Count)
					{
						return _hotImageIndex;
					}
					if (GetAssociatedCommand() != null)
					{
						if (GetAssociatedCommand().HotImageKey.Length > 0 && ImageList.Images.ContainsKey(GetAssociatedCommand().HotImageKey))
						{
							return ImageList.Images.IndexOfKey(GetAssociatedCommand().HotImageKey);
						}
						if (GetAssociatedCommand().HotImageIndex >= 0 && GetAssociatedCommand().HotImageIndex < ImageList.Images.Count)
						{
							return GetAssociatedCommand().HotImageIndex;
						}
					}
				}
				if (_imageKey.Length > 0 && ImageList.Images.ContainsKey(_imageKey))
				{
					return ImageList.Images.IndexOfKey(_imageKey);
				}
				if (_imageIndex >= 0 && _imageIndex < ImageList.Images.Count)
				{
					return _imageIndex;
				}
				if (GetAssociatedCommand() != null)
				{
					if (GetAssociatedCommand().ImageKey.Length > 0 && ImageList.Images.ContainsKey(GetAssociatedCommand().ImageKey))
					{
						return ImageList.Images.IndexOfKey(GetAssociatedCommand().ImageKey);
					}
					if (GetAssociatedCommand().ImageIndex >= 0 && GetAssociatedCommand().ImageIndex < ImageList.Images.Count)
					{
						return GetAssociatedCommand().ImageIndex;
					}
				}
			}
			return -1;
		}
	}
}
