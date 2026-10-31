using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		private int GetGroupImageIndexByIndex(NavigationGroup navigationBarGroup)
		{
			if (navigationBarGroup.Image == null && navigationBarGroup.Icon == null)
			{
				try
				{
					if (_smallImageList != null)
					{
						if (!navigationBarGroup.Enabled || !base.Enabled)
						{
							if (navigationBarGroup.DisabledImageIndex >= 0 && navigationBarGroup.DisabledImageIndex < _smallImageList.Images.Count)
							{
								return navigationBarGroup.DisabledImageIndex;
							}
						}
						else if ((navigationBarGroup._itemState == ItemState.Focus || navigationBarGroup._itemState == ItemState.SelectedFocus) && navigationBarGroup.FocusImageIndex >= 0 && navigationBarGroup.FocusImageIndex < _smallImageList.Images.Count)
						{
							return navigationBarGroup.FocusImageIndex;
						}
						if (navigationBarGroup.ImageIndex >= 0 && navigationBarGroup.ImageIndex < _smallImageList.Images.Count)
						{
							return navigationBarGroup.ImageIndex;
						}
						if (_defaultGroupImageKey.Length > 0 && _smallImageList.Images.ContainsKey(_defaultGroupImageKey))
						{
							return _smallImageList.Images.IndexOfKey(_defaultGroupImageKey);
						}
						if (_defaultGroupImageIndex >= 0 && _defaultGroupImageIndex < _smallImageList.Images.Count)
						{
							return _defaultGroupImageIndex;
						}
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
					return -1;
				}
			}
			return -1;
		}

		private int GetGroupImageIndexByKey(NavigationGroup navigationBarGroup)
		{
			if (navigationBarGroup.Image == null && navigationBarGroup.Icon == null)
			{
				try
				{
					if (_smallImageList != null)
					{
						if (!navigationBarGroup.Enabled || !base.Enabled)
						{
							if (navigationBarGroup.DisabledImageKey.Length > 0 && _smallImageList.Images.ContainsKey(navigationBarGroup.DisabledImageKey))
							{
								return _smallImageList.Images.IndexOfKey(navigationBarGroup.DisabledImageKey);
							}
						}
						else if ((navigationBarGroup._itemState == ItemState.Focus || navigationBarGroup._itemState == ItemState.SelectedFocus) && navigationBarGroup.FocusImageKey.Length > 0 && _smallImageList.Images.ContainsKey(navigationBarGroup.FocusImageKey))
						{
							return _smallImageList.Images.IndexOfKey(navigationBarGroup.FocusImageKey);
						}
						if (navigationBarGroup.ImageKey.Length > 0 && _smallImageList.Images.ContainsKey(navigationBarGroup.ImageKey))
						{
							return _smallImageList.Images.IndexOfKey(navigationBarGroup.ImageKey);
						}
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
					return -1;
				}
			}
			return -1;
		}

		private Image GetGroupImage(NavigationGroup navigationBarGroup)
		{
			if (navigationBarGroup.Image != null)
			{
				return navigationBarGroup.Image;
			}
			try
			{
				if (_smallImageList != null)
				{
					int imageIndex = GetGroupImageIndexByKey(navigationBarGroup);
					if (imageIndex == -1)
					{
						imageIndex = GetGroupImageIndexByIndex(navigationBarGroup);
					}
					if (imageIndex >= 0 && imageIndex < _smallImageList.Images.Count)
					{
						return _smallImageList.Images[imageIndex];
					}
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return null;
			}
			return null;
		}

		private int GetItemImageIndexByKey(NavigationItem navigationBarItem)
		{
			if (navigationBarItem.Image == null && navigationBarItem.Icon == null)
			{
				try
				{
					if (!navigationBarItem.Enabled || !base.Enabled || !_selectedGroup.Enabled)
					{
						if (_selectedGroup.View == NavigationBarView.LargeIcons && _largeImageList != null && navigationBarItem.LargeImageDisabledKey.Length > 0 && _largeImageList.Images.ContainsKey(navigationBarItem.LargeImageDisabledKey))
						{
							return _largeImageList.Images.IndexOfKey(navigationBarItem.LargeImageDisabledKey);
						}
						if (_selectedGroup.View == NavigationBarView.SmallIcons && _smallImageList != null && navigationBarItem.SmallImageDisabledKey.Length > 0 && _smallImageList.Images.ContainsKey(navigationBarItem.SmallImageDisabledKey))
						{
							return _smallImageList.Images.IndexOfKey(navigationBarItem.SmallImageDisabledKey);
						}
					}
					else if (navigationBarItem._itemState == ItemState.Focus || navigationBarItem._itemState == ItemState.SelectedFocus)
					{
						if (_selectedGroup.View == NavigationBarView.LargeIcons)
						{
							if (_largeImageList != null && navigationBarItem.LargeImageFocusKey.Length > 0 && _largeImageList.Images.ContainsKey(navigationBarItem.LargeImageFocusKey))
							{
								return _largeImageList.Images.IndexOfKey(navigationBarItem.LargeImageFocusKey);
							}
						}
						else if (_smallImageList != null && navigationBarItem.SmallImageFocusKey.Length > 0 && _smallImageList.Images.ContainsKey(navigationBarItem.SmallImageFocusKey))
						{
							return _smallImageList.Images.IndexOfKey(navigationBarItem.SmallImageFocusKey);
						}
					}
					if (_selectedGroup.View == NavigationBarView.LargeIcons)
					{
						if (_largeImageList != null && navigationBarItem.LargeImageKey.Length > 0 && _largeImageList.Images.ContainsKey(navigationBarItem.LargeImageKey))
						{
							return _largeImageList.Images.IndexOfKey(navigationBarItem.LargeImageKey);
						}
					}
					else if (_smallImageList != null && navigationBarItem.SmallImageKey.Length > 0 && _smallImageList.Images.ContainsKey(navigationBarItem.SmallImageKey))
					{
						return _smallImageList.Images.IndexOfKey(navigationBarItem.SmallImageKey);
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
					return -1;
				}
			}
			return -1;
		}

		private int GetItemImageIndexByIndex(NavigationItem navigationBarItem)
		{
			if (navigationBarItem.Image == null && navigationBarItem.Icon == null)
			{
				try
				{
					if (!navigationBarItem.Enabled || !base.Enabled || !_selectedGroup.Enabled)
					{
						if (_selectedGroup.View == NavigationBarView.LargeIcons && _largeImageList != null && navigationBarItem.LargeImageDisabledIndex >= 0 && navigationBarItem.LargeImageDisabledIndex < _largeImageList.Images.Count)
						{
							return navigationBarItem.LargeImageDisabledIndex;
						}
						if (_selectedGroup.View == NavigationBarView.SmallIcons && _smallImageList != null && navigationBarItem.SmallImageDisabledIndex >= 0 && navigationBarItem.SmallImageDisabledIndex < _smallImageList.Images.Count)
						{
							return navigationBarItem.SmallImageDisabledIndex;
						}
					}
					else if (navigationBarItem._itemState == ItemState.Focus || navigationBarItem._itemState == ItemState.SelectedFocus)
					{
						if (_selectedGroup.View == NavigationBarView.LargeIcons)
						{
							if (_largeImageList != null && navigationBarItem.LargeImageFocusIndex >= 0 && navigationBarItem.LargeImageFocusIndex < _largeImageList.Images.Count)
							{
								return navigationBarItem.LargeImageFocusIndex;
							}
						}
						else if (_smallImageList != null && navigationBarItem.SmallImageFocusIndex >= 0 && navigationBarItem.SmallImageFocusIndex < _smallImageList.Images.Count)
						{
							return navigationBarItem.SmallImageFocusIndex;
						}
					}
					if (_selectedGroup.View == NavigationBarView.LargeIcons)
					{
						if (_largeImageList != null)
						{
							if (navigationBarItem.LargeImageIndex >= 0 && navigationBarItem.LargeImageIndex < _largeImageList.Images.Count)
							{
								return navigationBarItem.LargeImageIndex;
							}
							if (_defaultItemLargeImageKey.Length > 0 && _largeImageList.Images.ContainsKey(_defaultItemLargeImageKey))
							{
								return _largeImageList.Images.IndexOfKey(_defaultItemLargeImageKey);
							}
							if (_defaultItemLargeImageIndex >= 0 && _defaultItemLargeImageIndex < _largeImageList.Images.Count)
							{
								return _defaultItemLargeImageIndex;
							}
						}
					}
					else if (_smallImageList != null)
					{
						if (navigationBarItem.SmallImageIndex >= 0 && navigationBarItem.SmallImageIndex < _smallImageList.Images.Count)
						{
							return navigationBarItem.SmallImageIndex;
						}
						if (_defaultItemSmallImageKey.Length > 0 && _smallImageList.Images.ContainsKey(_defaultItemSmallImageKey))
						{
							return _smallImageList.Images.IndexOfKey(_defaultItemSmallImageKey);
						}
						if (_defaultItemSmallImageIndex >= 0 && _defaultItemSmallImageIndex < _smallImageList.Images.Count)
						{
							return _defaultItemSmallImageIndex;
						}
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
					return -1;
				}
			}
			return -1;
		}

		private Image GetItemImage(NavigationItem navigationBarItem)
		{
			if (navigationBarItem.Image != null)
			{
				return navigationBarItem.Image;
			}
			try
			{
				ImageList imageList = null;
				imageList = ((_selectedGroup.View != NavigationBarView.LargeIcons) ? _smallImageList : _largeImageList);
				if (imageList != null)
				{
					int imageIndex = GetItemImageIndexByKey(navigationBarItem);
					if (imageIndex == -1)
					{
						imageIndex = GetItemImageIndexByIndex(navigationBarItem);
					}
					if (imageIndex >= 0 && imageIndex < imageList.Images.Count)
					{
						return imageList.Images[imageIndex];
					}
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return null;
			}
			return null;
		}

		internal void DrawDisabledText(Graphics graphics, NavigationBarFormatStyle navigationBarFormatStyle, string text, Font font, TextLayoutRenderer textLayoutRenderer, Rectangle textBounds)
		{
			textLayoutRenderer.DrawTextWithColor(graphics, text, textBounds, font, navigationBarFormatStyle.GetForeColorOrDefault(GetTheme().Color(ThemeRole.TextDisabled)));
		}

		private void DrawSelectedGroupItems(Graphics graphics, Rectangle paintBounds, NavigationBarStateStyles navigationBarStateStyles, NavigationBarFormatStyle baseFormatStyle)
		{
			if (Groups.Count <= 0)
			{
				return;
			}
			Region clip = graphics.Clip;
			Region clipRegion = (graphics.Clip = new Region(_itemsAreaBounds));
			bool isBackgroundOwnerDrawn = IsOwnerDrawn(OwnerDrawnArea.GroupBackground);
			bool areItemsOwnerDrawn = IsOwnerDrawn(OwnerDrawnArea.Items);
			bool isHandled = false;
			NavigationBarFormatStyle navigationBarFormatStyle = baseFormatStyle.MergeWith(_groupsBackgroundFormatStyle);
			navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(SelectedGroup.BackgroundFormatStyle);
			if (_isSelectedGroupLast)
			{
				Rectangle extendedBounds = _itemsAreaBounds;
				if (_orientation == NavigationBarOrientation.Horizontal)
				{
					extendedBounds.Width = _innerBounds.Right - _itemsAreaBounds.Left;
				}
				else
				{
					extendedBounds.Height = _innerBounds.Bottom - _itemsAreaBounds.Top;
				}
				clipRegion.Dispose();
				clipRegion = (graphics.Clip = new Region(extendedBounds));
				if (isBackgroundOwnerDrawn)
				{
					DrawNavigationBarAreaEventArgs e = new DrawNavigationBarAreaEventArgs(graphics, this, NavigationBarArea.GroupBackground, extendedBounds, null);
					OnDrawNavigationBarArea(e);
					isHandled = e.Handled;
				}
				if (!isHandled)
				{
					DrawItemsAreaBackground(graphics, extendedBounds, navigationBarFormatStyle.GetCachedBackBrush(extendedBounds), paintBounds);
				}
				clipRegion.Dispose();
				clipRegion = (graphics.Clip = new Region(_itemsAreaBounds));
			}
			else
			{
				if (isBackgroundOwnerDrawn)
				{
					DrawNavigationBarAreaEventArgs backgroundDrawArgs = new DrawNavigationBarAreaEventArgs(graphics, this, NavigationBarArea.GroupBackground, _itemsAreaBounds, null);
					OnDrawNavigationBarArea(backgroundDrawArgs);
					isHandled = backgroundDrawArgs.Handled;
				}
				if (!isHandled)
				{
					DrawItemsAreaBackground(graphics, _itemsAreaBounds, navigationBarFormatStyle.GetCachedBackBrush(_itemsAreaBounds), paintBounds);
				}
			}
			isHandled = false;
			Rectangle visibleArea = Rectangle.Intersect(paintBounds, _itemsAreaBounds);
			if (_selectedGroup.View == NavigationBarView.SmallIcons)
			{
				foreach (NavigationItem item in _selectedGroup.Items)
				{
					if (item.Visible)
					{
						if (!Rectangle.Intersect(visibleArea, item.GetBounds()).IsEmpty)
						{
							NavigationBarFormatStyle smallItemStyle = GetItemFormatStyle(item, navigationBarStateStyles);
							isHandled = false;
							if (areItemsOwnerDrawn)
							{
								DrawNavigationBarAreaEventArgs smallItemDrawArgs = new DrawNavigationBarAreaEventArgs(graphics, this, NavigationBarArea.Item, item.GetBounds(), item);
								OnDrawNavigationBarArea(smallItemDrawArgs);
								isHandled = smallItemDrawArgs.Handled;
							}
							if (!isHandled)
							{
								DrawSmallIconItem(graphics, item, item.GetBounds(), smallItemStyle, true);
							}
							smallItemStyle.DisposeBrushesAndFont();
						}
					}
					else
					{
						item.UpdateBounds(Rectangle.Empty);
						item.SetTextBounds(Rectangle.Empty);
						item.UpdateImageBounds(Rectangle.Empty);
					}
				}
			}
			else
			{
				foreach (NavigationItem largeItem in _selectedGroup.Items)
				{
					if (largeItem.Visible)
					{
						if (!Rectangle.Intersect(visibleArea, Rectangle.Union(largeItem.GetBounds(), largeItem.GetTextBounds())).IsEmpty)
						{
							NavigationBarFormatStyle largeItemStyle = GetItemFormatStyle(largeItem, navigationBarStateStyles);
							isHandled = false;
							if (areItemsOwnerDrawn)
							{
								DrawNavigationBarAreaEventArgs largeItemDrawArgs = new DrawNavigationBarAreaEventArgs(graphics, this, NavigationBarArea.Item, largeItem.GetBounds(), largeItem);
								OnDrawNavigationBarArea(largeItemDrawArgs);
								isHandled = largeItemDrawArgs.Handled;
							}
							if (!isHandled)
							{
								DrawLargeIconItem(graphics, largeItem, largeItemStyle, true);
							}
							largeItemStyle.DisposeBrushesAndFont();
						}
					}
					else
					{
						largeItem.UpdateBounds(Rectangle.Empty);
						largeItem.SetTextBounds(Rectangle.Empty);
						largeItem.UpdateImageBounds(Rectangle.Empty);
					}
				}
			}
			graphics.Clip = clip;
			clip.Dispose();
			clipRegion.Dispose();
			clipRegion = null;
		}

		private void DrawLargeIconItem(Graphics graphics, NavigationItem navigationBarItem, NavigationBarFormatStyle navigationBarFormatStyle, bool includeText)
		{
			_ = Rectangle.Empty;
			bool isPressed = false;
			bool isHot = false;
			bool isEnabled = navigationBarItem.Enabled && base.Enabled && _selectedGroup.Enabled;
			Rectangle itemBounds = navigationBarItem.GetBounds();
			Rectangle imageBounds = itemBounds;
			bool canDrawText = true;
			if (_selectionArea == SelectionArea.FullItem)
			{
				imageBounds = navigationBarItem.GetImageBounds();
			}
			else
			{
				imageBounds.Inflate(-2, -2);
			}
			if (navigationBarItem._itemState == ItemState.Selected && isEnabled)
			{
				isPressed = true;
			}
			else if (navigationBarItem._itemState == ItemState.Focus && isEnabled)
			{
				isHot = true;
			}
			else if (navigationBarItem._itemState == ItemState.SelectedFocus && isEnabled)
			{
				isHot = true;
				isPressed = true;
			}
			NavigationThemedPainter.FillItem(graphics, GetTheme(), itemBounds, isHot, isPressed, navigationBarFormatStyle, RightToLeft == RightToLeft.Yes);
			if (isPressed && _itemAppearance == ItemAppearance.PopUp)
			{
				imageBounds.Offset(1, 1);
			}
			Image itemImage = GetItemImage(navigationBarItem);
			if (isHot && isEnabled && _shadowOnHover)
			{
				NavigationGraphicsHelper.DrawImageWithShadow(graphics, navigationBarItem.Icon, itemImage, imageBounds);
			}
			else
			{
				int imageIndex = GetItemImageIndexByKey(navigationBarItem);
				if (imageIndex == -1)
				{
					imageIndex = GetItemImageIndexByIndex(navigationBarItem);
				}
				NavigationGraphicsHelper.DrawIconOrImage(graphics, navigationBarItem.Icon, itemImage, imageIndex, _largeImageList, isEnabled, imageBounds, navigationBarFormatStyle.BackColor);
			}
			if (canDrawText && includeText)
			{
				DrawItemText(graphics, navigationBarItem, navigationBarFormatStyle);
			}
		}

		private void DrawGroupHeaderText(Graphics graphics, NavigationGroup navigationBarGroup, Rectangle textBounds, TextLayoutRenderer textLayoutRenderer, NavigationBarFormatStyle navigationBarFormatStyle)
		{
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			Font headerFont = navigationBarFormatStyle.GetEffectiveFont(Font);
			if (!navigationBarGroup.Enabled)
			{
				textBounds.Offset(1, 1);
				DrawDisabledText(graphics, navigationBarFormatStyle, navigationBarGroup.Text, headerFont, textLayoutRenderer, textBounds);
			}
			else
			{
				textLayoutRenderer.DrawTextWithColor(graphics, navigationBarGroup.Text, textBounds, headerFont, navigationBarFormatStyle.GetForeColorOrDefault(GetEffectiveForeColor()));
			}
		}

		private void DrawItemText(Graphics graphics, NavigationItem navigationBarItem, NavigationBarFormatStyle navigationBarFormatStyle)
		{
			Rectangle textBounds = navigationBarItem.GetTextBounds();
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateMnemonicEnabled(_useMnemonic);
			Font itemFont = navigationBarFormatStyle.GetEffectiveFont(Font);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.UpdateWrapEnabled(true);
			if (!navigationBarItem.Enabled || !base.Enabled || !_selectedGroup.Enabled)
			{
				textBounds.Offset(1, 1);
				DrawDisabledText(graphics, navigationBarFormatStyle, navigationBarItem.Text, itemFont, textLayoutRenderer, textBounds);
			}
			else
			{
				textLayoutRenderer.DrawTextWithColor(graphics, navigationBarItem.Text, textBounds, itemFont, navigationBarFormatStyle.GetForeColorOrDefault(GetEffectiveForeColor()));
			}
			textLayoutRenderer.InvalidateFormat();
		}

		private void DrawSmallIconItem(Graphics graphics, NavigationItem navigationBarItem, Rectangle itemBounds, NavigationBarFormatStyle navigationBarFormatStyle, bool includeText)
		{
			_ = Rectangle.Empty;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateMnemonicEnabled(_useMnemonic);
			if (_orientation == NavigationBarOrientation.Horizontal && itemBounds.Right > _itemsAreaBounds.Right)
			{
				itemBounds.Width = _itemsAreaBounds.Right - itemBounds.X;
			}
			Rectangle textBounds = itemBounds;
			Rectangle imageBounds = itemBounds;
			textBounds.X += 2;
			textBounds.Width -= 2;
			Image itemImage = GetItemImage(navigationBarItem);
			bool isHot = false;
			bool isPressed = false;
			bool isEnabled = navigationBarItem.Enabled && base.Enabled && _selectedGroup.Enabled;
			Size imageSize = GetItemImageSize(navigationBarItem);
			int height = imageSize.Height;
			int width = imageSize.Width;
			if (navigationBarItem.ImageAlignment == Alignment.Near)
			{
				imageBounds = new Rectangle(itemBounds.Left + 5, itemBounds.Top + 2, width, height);
				if (navigationBarItem.TextAlignment == Alignment.Near)
				{
					textBounds.X = imageBounds.Right + 4;
					textBounds.Width -= imageBounds.Width + 9;
				}
			}
			else if (navigationBarItem.ImageAlignment == Alignment.Far)
			{
				imageBounds = new Rectangle(itemBounds.Right - width - 5, itemBounds.Top + 2, width, height);
				if (navigationBarItem.TextAlignment == Alignment.Far)
				{
					textBounds.Width -= imageBounds.Width + 9;
				}
			}
			else
			{
				int x = itemBounds.Left + (itemBounds.Width - width) / 2;
				imageBounds = new Rectangle(x, itemBounds.Top + 2, width, height);
			}
			if (_selectionArea == SelectionArea.Image)
			{
				itemBounds = imageBounds;
				itemBounds.Inflate(2, 2);
				itemBounds.Height += 2;
			}
			if (navigationBarItem._itemState == ItemState.Selected)
			{
				isPressed = true;
			}
			else if (navigationBarItem._itemState == ItemState.Focus)
			{
				isHot = true;
			}
			else if (navigationBarItem._itemState == ItemState.SelectedFocus)
			{
				isHot = true;
				isPressed = true;
			}
			NavigationThemedPainter.FillItem(graphics, GetTheme(), itemBounds, isHot, isPressed, navigationBarFormatStyle, RightToLeft == RightToLeft.Yes);
			if (isPressed && ItemAppearance == ItemAppearance.PopUp)
			{
				imageBounds.Offset(1, 1);
				if (_selectionArea == SelectionArea.FullItem)
				{
					textBounds.Offset(1, 1);
				}
			}
			if (isEnabled && isHot && _shadowOnHover)
			{
				imageBounds.Offset(1, 1);
				NavigationGraphicsHelper.DrawImageWithShadow(graphics, navigationBarItem.Icon, itemImage, imageBounds);
			}
			else if (_useImageListIcons)
			{
				int imageIndex = GetItemImageIndexByKey(navigationBarItem);
				if (imageIndex == -1)
				{
					imageIndex = GetItemImageIndexByIndex(navigationBarItem);
				}
				NavigationGraphicsHelper.DrawIconOrImage(graphics, navigationBarItem.Icon, itemImage, imageIndex, _smallImageList, isEnabled, imageBounds, navigationBarFormatStyle.BackColor);
			}
			else
			{
				NavigationGraphicsHelper.DrawIconOrImage(graphics, navigationBarItem.Icon, itemImage, -1, null, isEnabled, imageBounds, navigationBarFormatStyle.BackColor);
			}
			if (navigationBarItem.TextAlignment == Alignment.Near)
			{
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			}
			else if (navigationBarItem.TextAlignment == Alignment.Far)
			{
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
			}
			else
			{
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			}
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			Font itemFont = navigationBarFormatStyle.GetEffectiveFont(Font);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			if (includeText)
			{
				if (isEnabled)
				{
					textLayoutRenderer.DrawTextWithColor(graphics, navigationBarItem.Text, textBounds, itemFont, navigationBarFormatStyle.GetForeColorOrDefault(GetEffectiveForeColor()));
				}
				else
				{
					textBounds.Offset(1, 1);
					DrawDisabledText(graphics, navigationBarFormatStyle, navigationBarItem.Text, itemFont, textLayoutRenderer, textBounds);
				}
			}
			textLayoutRenderer.InvalidateFormat();
		}
	}
}
