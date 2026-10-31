using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	internal partial class ComboDropDownList
	{
		private ThemeHook _themeHook;

		// The list listens to theme changes only while it is open and themed.
		private void UpdateThemeListening()
		{
			bool wanted = GetIsDropDownOpen();
			if (_themeHook == null)
			{
				if (!wanted)
				{
					return;
				}
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(wanted);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		// Paints the pop-up list from the default theme.
		private void DrawThemedItems(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			Rectangle content = GetContentBounds();
			graphics.FillRectangle(theme.Brush(ThemeRole.InputBack), content);
			if (_items == null || _items.Count == 0)
			{
				return;
			}
			SmoothingMode smoothing = graphics.SmoothingMode;
			int top = content.Top;
			int minItemHeight = Font.Height + ItemPadding;
			ComboBoxItem highlighted = GetHighlightedItem();
			for (int i = _firstVisibleIndex; i < _items.Count; i++)
			{
				ComboBoxItem item = _items[i];
				int indent = item.IndentLevel * GetIndentWidth();
				int left = content.Left + (RightToLeft == RightToLeft.Yes ? 1 : indent);
				int right = content.Right - 1 - (RightToLeft == RightToLeft.Yes ? indent + 1 : 0);
				if (item._isSeparator)
				{
					graphics.DrawLine(theme.Pen(ThemeRole.Border), left, top + 1, right - 1, top + 1);
					top += SeparatorHeight;
					continue;
				}
				_lastVisibleIndex = i;
				int height = ItemPadding;
				Image image = item.GetDisplayImage(_imageList);
				if (image != null)
				{
					height += image.Size.Height + ItemPadding;
				}
				height = Math.Max(height, minItemHeight);
				Rectangle bounds = new Rectangle(left, top, content.Width - indent - 1, height);
				bool selected = item == highlighted;
				if (selected)
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					theme.FillRounded(graphics, Rectangle.Inflate(bounds, -1, 0), ThemeRole.Selection, Math.Min(theme.Shape.CornerRadius, 4));
					graphics.SmoothingMode = smoothing;
				}
				Brush text = theme.Brush(selected ? ThemeRole.SelectionText : ThemeRole.Text);
				RenderingHelper.DrawImageWithText(graphics, text, item.Text, image, item.GetImageListIndex(_imageList), _imageList, bounds, ImageHorizontalAlignment.BeforeText, ImageVerticalAlignment.Center, GetTextLayoutRenderer(), Font, true, Color.Empty);
				top += height;
				if (top >= content.Bottom)
				{
					break;
				}
			}
		}
	}
}
