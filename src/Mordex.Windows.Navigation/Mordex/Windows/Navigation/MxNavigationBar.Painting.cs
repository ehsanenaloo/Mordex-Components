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

		private Image GetActiveBackgroundImage()
		{
			if (_selectedGroup != null && _selectedGroup.BackgroundImage != null)
			{
				return _selectedGroup.BackgroundImage;
			}
			if (BackgroundImage != null)
			{
				return BackgroundImage;
			}
			return null;
		}

		private ImageDrawMode GetActiveBackgroundImageDrawMode()
		{
			if (_selectedGroup != null && SelectedGroup.BackgroundImage != null)
			{
				return _selectedGroup.BackGroundImageDrawMode;
			}
			return _backgroundImageDrawMode;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (e.ClipRectangle == Rectangle.Empty || _isPainting)
			{
				base.OnPaint(e);
				return;
			}
			CompleteSlideAnimation();
			_isPainting = true;
			try
			{
				DrawControlBackground(e.Graphics, e.ClipRectangle, true);
				UpdateScrollButtonStates();
				DisposeRenderers();
				base.OnPaint(e);
			}
			finally
			{
				_isPainting = false;
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		internal void DrawArea(Graphics graphics, NavigationBarArea navigationBarArea, object areaTarget)
		{
			switch (navigationBarArea)
			{
			case NavigationBarArea.Background:
				DrawControlBackground(graphics, base.ClientRectangle, false);
				return;
			case NavigationBarArea.GroupHeader:
			{
				NavigationGroup navigationBarGroup = areaTarget as NavigationGroup;
				NavigationBarFormatStyle navigationBarFormatStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
				DrawGroupHeader(graphics, navigationBarGroup, navigationBarGroup.Bounds, navigationBarFormatStyle, false);
				navigationBarFormatStyle.DisposeBrushesAndFont();
				return;
			}
			}
			NavigationBarFormatStyle areaStyle = GetAreaFormatStyle(navigationBarArea, areaTarget);
			if (navigationBarArea == NavigationBarArea.GroupBackground)
			{
				Region clip = graphics.Clip;
				Region groupsClip = (graphics.Clip = new Region(_itemsAreaBounds));
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
					groupsClip.Dispose();
					groupsClip = (graphics.Clip = new Region(extendedBounds));
					DrawItemsAreaBackground(graphics, extendedBounds, areaStyle.GetCachedBackBrush(extendedBounds), _itemsAreaBounds);
				}
				else
				{
					DrawItemsAreaBackground(graphics, _itemsAreaBounds, areaStyle.GetCachedBackBrush(_itemsAreaBounds), _itemsAreaBounds);
				}
				graphics.Clip = clip;
				clip.Dispose();
				if (groupsClip != null)
				{
					groupsClip.Dispose();
					groupsClip = null;
				}
			}
			else if (areaTarget is NavigationItem navigationBarItem)
			{
				NavigationGroup itemGroup = navigationBarItem.Group;
				if (itemGroup.View == NavigationBarView.SmallIcons)
				{
					DrawSmallIconItem(graphics, navigationBarItem, navigationBarItem.GetBounds(), areaStyle, false);
				}
				else
				{
					DrawLargeIconItem(graphics, navigationBarItem, areaStyle, false);
				}
			}
			areaStyle.DisposeBrushesAndFont();
		}

		private void DrawGroupHeader(Graphics graphics, NavigationGroup navigationBarGroup, Rectangle headerBounds, NavigationBarFormatStyle navigationBarFormatStyle, bool includeText)
		{
			Rectangle textBounds = headerBounds;
			textBounds.Inflate(-2, -2);
			textBounds.X += 2;
			textBounds.Width -= 2;
			bool isHorizontal = _orientation == NavigationBarOrientation.Horizontal;
			bool isEnabled = navigationBarGroup.Enabled && base.Enabled;
			BorderFlags borderFlags = isHorizontal ? BorderFlags.Right : BorderFlags.Bottom;
			NavigationThemedPainter.FillHeader(graphics, GetTheme(), headerBounds, navigationBarFormatStyle, borderFlags);
			Image groupImage = GetGroupImage(navigationBarGroup);
			if (groupImage != null || navigationBarGroup.Icon != null)
			{
				Rectangle imageBounds;
				Size imageSize = GetGroupImageSize(navigationBarGroup);
				int height = imageSize.Height;
				int width = imageSize.Width;
				int y = (headerBounds.Height - height) / 2 + headerBounds.Top;
				int x = (headerBounds.Width - width) / 2 + headerBounds.Left;
				if (navigationBarGroup.ImageAlignment == Alignment.Near)
				{
					if (!isHorizontal)
					{
						imageBounds = new Rectangle(headerBounds.Left + 5, y, width, height);
						textBounds.X = imageBounds.Right + 4;
						textBounds.Width -= imageBounds.Width + 9;
					}
					else
					{
						imageBounds = new Rectangle(x, headerBounds.Top + 4, width, height);
						textBounds.Y = imageBounds.Bottom + 4;
						textBounds.Height -= imageBounds.Height + 8;
					}
				}
				else if (navigationBarGroup.ImageAlignment == Alignment.Far)
				{
					if (!isHorizontal)
					{
						imageBounds = new Rectangle(headerBounds.Right - width - 5, y, width, height);
						textBounds.Width -= imageBounds.Width + 9;
					}
					else
					{
						imageBounds = new Rectangle(x, headerBounds.Bottom - height - 4, width, height);
						if (navigationBarGroup.TextAlignment == Alignment.Far)
						{
							textBounds.Height = imageBounds.Top - 4;
						}
						textBounds.Height -= imageBounds.Height + 8;
					}
				}
				else
				{
					imageBounds = new Rectangle(x, y, width, height);
				}
				int imageIndex = GetGroupImageIndexByKey(navigationBarGroup);
				if (imageIndex == -1)
				{
					imageIndex = GetGroupImageIndexByIndex(navigationBarGroup);
				}
				NavigationGraphicsHelper.DrawIconOrImage(graphics, navigationBarGroup.Icon, groupImage, imageIndex, _smallImageList, isEnabled, imageBounds, navigationBarFormatStyle.BackColor);
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateMnemonicEnabled(_useMnemonic);
			if (navigationBarGroup.TextAlignment == Alignment.Near)
			{
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			}
			else if (navigationBarGroup.TextAlignment == Alignment.Far)
			{
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
			}
			else
			{
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			}
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			if (isHorizontal && includeText)
			{
				textLayoutRenderer.UpdateVertical(true);
				DrawGroupHeaderText(graphics, navigationBarGroup, textBounds, textLayoutRenderer, navigationBarFormatStyle);
				textLayoutRenderer.InvalidateFormat();
				return;
			}
			if (includeText)
			{
				Font headerFont = navigationBarFormatStyle.GetEffectiveFont(Font);
				if (isEnabled)
				{
					textLayoutRenderer.DrawTextWithColor(graphics, navigationBarGroup.Text, textBounds, headerFont, navigationBarFormatStyle.GetForeColorOrDefault(GetEffectiveForeColor()));
				}
				else
				{
					textBounds.Offset(1, 1);
					DrawDisabledText(graphics, navigationBarFormatStyle, navigationBarGroup.Text, headerFont, textLayoutRenderer, textBounds);
				}
			}
			textLayoutRenderer.InvalidateFormat();
		}

		internal void DrawSelectedGroupBackground(Graphics graphics, Rectangle fillBounds, Rectangle clipBounds)
		{
			if (_selectedGroup != null)
			{
				NavigationBarFormatStyle navigationBarFormatStyle = CreateBaseFormatStyle();
				NavigationBarFormatStyle groupsStyle = navigationBarFormatStyle.MergeWith(GroupsBackgroundFormatStyle);
				NavigationBarFormatStyle selectedGroupStyle = groupsStyle.MergeWith(SelectedGroup.BackgroundFormatStyle);
				DrawItemsAreaBackground(graphics, fillBounds, selectedGroupStyle.GetCachedBackBrush(fillBounds), clipBounds);
			}
		}

		private void DrawItemsAreaBackground(Graphics graphics, Rectangle areaBounds, Brush backgroundBrush, Rectangle clipBounds)
		{
			Rectangle fillBounds = Rectangle.Intersect(clipBounds, areaBounds);
			if (fillBounds.Width == 0 || fillBounds.Height == 0)
			{
				return;
			}
			if (backgroundBrush != null)
			{
				graphics.FillRectangle(backgroundBrush, fillBounds);
			}
			Image backgroundImage = GetActiveBackgroundImage();
			ImageDrawMode imageDrawMode = GetActiveBackgroundImageDrawMode();
			if (backgroundImage != null)
			{
				switch (imageDrawMode)
				{
				case ImageDrawMode.Centered:
					CenterBackgroundImage(graphics, backgroundImage, areaBounds, fillBounds);
					break;
				case ImageDrawMode.Stretched:
					StretchBackgroundImage(graphics, backgroundImage, areaBounds, fillBounds);
					break;
				case ImageDrawMode.Tiled:
					TileBackgroundImage(graphics, backgroundImage, areaBounds, fillBounds);
					break;
				}
			}
		}

		private void TileBackgroundImage(Graphics graphics, Image image, Rectangle area, Rectangle clipBounds)
		{
			int i = area.X;
			for (int j = area.Y; j < area.Bottom; j += image.Height)
			{
				for (; i < area.Right; i += image.Width)
				{
					Rectangle destRect = new Rectangle(i, j, image.Width, image.Height);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				}
				i = area.X;
			}
		}

		private void CenterBackgroundImage(Graphics graphics, Image image, Rectangle area, Rectangle clipBounds)
		{
			int x = area.X + (area.Width - image.Width) / 2;
			int y = area.Y + (area.Height - image.Height) / 2;
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		private void StretchBackgroundImage(Graphics graphics, Image image, Rectangle destRect, Rectangle clipBounds)
		{
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		private void DrawGroupHeaders(Graphics graphics, Rectangle paintBounds)
		{
			if (!_headerGroupVisible)
			{
				return;
			}
			bool isHandled = false;
			bool isOwnerDrawn = IsOwnerDrawn(OwnerDrawnArea.GroupHeader);
			foreach (NavigationGroup item in _groups)
			{
				if (!item.Visible)
				{
					continue;
				}
				Rectangle visibleBounds = Rectangle.Intersect(paintBounds, item.Bounds);
				if (visibleBounds.Width > 0 && visibleBounds.Height > 0)
				{
					NavigationBarFormatStyle navigationBarFormatStyle = GetGroupFormatStyle(item);
					isHandled = false;
					if (isOwnerDrawn)
					{
						DrawNavigationBarAreaEventArgs e = new DrawNavigationBarAreaEventArgs(graphics, this, NavigationBarArea.GroupHeader, item.Bounds, item);
						OnDrawNavigationBarArea(e);
						isHandled = e.Handled;
					}
					if (!isHandled)
					{
						DrawGroupHeader(graphics, item, item.Bounds, navigationBarFormatStyle, true);
					}
					navigationBarFormatStyle.DisposeBrushesAndFont();
				}
			}
		}

		private void DrawControlBackground(Graphics graphics, Rectangle paintBounds, bool includeContent)
		{
			NavigationBarFormatStyle navigationBarFormatStyle = CreateBaseFormatStyle();
			Rectangle contentBounds = base.ClientRectangle;
			NavigationBarFormatStyle customStyle = new NavigationBarFormatStyle(GroupsBackgroundFormatStyle);
			if (customStyle.BackColor.IsEmpty)
			{
				customStyle.BackColor = GetEffectiveBackColor();
			}
			Brush plainBrush = customStyle.GetCachedBackBrush(base.ClientRectangle);
			if (plainBrush != null)
			{
				graphics.FillRectangle(plainBrush, paintBounds);
				customStyle.DisposeBrushesAndFont();
			}
			_innerBounds = contentBounds;
			if (GetEffectiveBorderStyle() != BorderStyle.None)
			{
				_mordexColorSchemeRenderer.SetBaseColor(GetEffectiveBackColor());
				_mordexColorSchemeRenderer.SetFlatBorderColor(FlatBorderColor);
				contentBounds = _mordexColorSchemeRenderer.DrawBorder(graphics, _mordexColorSchemeRenderer, base.ClientRectangle, newBorderStyle.Flat);
			}
			_innerBounds = contentBounds;
			if (includeContent)
			{
				if (!_isGroupLayoutValid)
				{
					LayoutGroups(graphics);
					_isGroupLayoutValid = true;
				}
				if (!_isItemLayoutValid)
				{
					LayoutVerticalItems(graphics, _selectedGroup);
					_isItemLayoutValid = true;
				}
				if (_selectedGroup != null && !_selectedGroup.Container)
				{
					NavigationBarStateStyles navigationBarStateStyles = new NavigationBarStateStyles(_itemsStateStyles);
					navigationBarStateStyles = navigationBarStateStyles.MergeWith(_selectedGroup.ItemsStateStyles);
					DrawSelectedGroupItems(graphics, paintBounds, navigationBarStateStyles, navigationBarFormatStyle);
				}
				DrawGroupHeaders(graphics, paintBounds);
			}
			contentBounds = (_innerBounds = base.ClientRectangle);
			if (GetEffectiveBorderStyle() != BorderStyle.None)
			{
				_mordexColorSchemeRenderer.SetBaseColor(GetEffectiveBackColor());
				_mordexColorSchemeRenderer.SetFlatBorderColor(FlatBorderColor);
				contentBounds = _mordexColorSchemeRenderer.DrawBorder(graphics, _mordexColorSchemeRenderer, base.ClientRectangle, newBorderStyle.Flat);
			}
			_innerBounds = contentBounds;
			navigationBarFormatStyle.DisposeBrushesAndFont();
		}
	}
}
