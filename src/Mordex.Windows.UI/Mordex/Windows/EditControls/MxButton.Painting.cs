using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.CompilerServices;
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
		// The values set in BackColor, in StateStyles, in the StateStyles of the command and in ActiveFormatStyle for one state,
		// each later one winning. Empty values are left to the theme. The caller disposes the result.
		private FormatStyle GetFormatStyle(bool isHot, bool isSelected)
		{
			StateStyles commandStyles = GetAssociatedCommand()?.StateStyles;
			FormatStyle uiFormatStyle = new FormatStyle((IFormatStyleOwner)null);
			if (base.Enabled && !_backColor.IsEmpty)
			{
				uiFormatStyle.BackColor = BackColorFace(_backColor, isHot, isSelected);
			}
			uiFormatStyle = MergeOwned(uiFormatStyle, StateStyles.FormatStyle);
			uiFormatStyle = MergeOwned(uiFormatStyle, commandStyles?.FormatStyle);
			if (base.ContainsFocus)
			{
				uiFormatStyle = MergeOwned(uiFormatStyle, _activeFormatStyle);
			}
			Func<StateStyles, FormatStyle> state = null;
			if (!base.Enabled)
			{
				state = styles => styles.DisabledFormatStyle;
			}
			else if (isHot && isSelected)
			{
				state = styles => styles.PressedFormatStyle;
			}
			else if (isSelected)
			{
				state = styles => styles.SelectedFormatStyle;
			}
			else if (isHot)
			{
				state = styles => styles.HotFormatStyle;
			}
			if (state != null)
			{
				uiFormatStyle = MergeOwned(uiFormatStyle, state(StateStyles));
				if (commandStyles != null)
				{
					uiFormatStyle = MergeOwned(uiFormatStyle, state(commandStyles));
				}
			}
			return uiFormatStyle;
		}

		// The hot and pressed faces of BackColor are shaded the way a theme shades its accent: darker on a light color, lighter
		// on a dark one. A color that is not opaque is used as it is in every state.
		internal static Color BackColorFace(Color backColor, bool isHot, bool isSelected)
		{
			if (backColor.A < 255 || !(isHot || isSelected))
			{
				return backColor;
			}
			bool pressed = isSelected;
			if (MxTheme.Luminance(backColor) < 0.5)
			{
				return MxTheme.Lighten(backColor, pressed ? 0.3f : 0.15f);
			}
			return MxTheme.Darken(backColor, pressed ? 0.24f : 0.12f);
		}

		// Merges and releases the fonts of the replaced intermediate style.
		private static FormatStyle MergeOwned(FormatStyle current, FormatStyle overrideStyle)
		{
			if (overrideStyle == null)
			{
				return current;
			}
			FormatStyle merged = current.MergeStyle(overrideStyle);
			if (!ReferenceEquals(merged, current))
			{
				current.DisposeResources();
			}
			return merged;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			TextLayoutRenderer textLayoutRenderer = CreateTextLayoutRenderer();
			FormatStyle style = GetFormatStyle(IsHot(), IsPressed() || IsDropDownPressed());
			try
			{
				Font font = style.ResolveFont(Font);
				// A state style can change the font, so the layout follows the font that this paint uses.
				if (_isLayoutDirty || FontKey(font) != _layoutFontKey)
				{
					LayoutContent(e.Graphics, textLayoutRenderer, font);
				}
				PaintThemed(e.Graphics, style, font);
			}
			finally
			{
				style.DisposeResources();
				textLayoutRenderer.ResetCache();
			}
			base.OnPaint(e);
		}

		protected virtual void DrawImage(Graphics g, FormatStyle style)
		{
			if (!_imageBounds.IsEmpty)
			{
				Image image = GetCurrentImage(IsHot(), IsPressed());
				int imageIndex = GetImageIndex(IsHot(), IsPressed());
				Icon icon = _icon;
				if (icon == null && GetAssociatedCommand() != null)
				{
					icon = GetAssociatedCommand().Icon;
				}
				RenderingHelper.DrawStateImage(g, icon, image, imageIndex, _imageList, _imageBounds, base.Enabled, false, false, style.BackColor, true, null);
			}
		}
	}
}
