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
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class CheckBase
	{

		internal void PaintCheckBackground(Graphics graphics, FormatStyle uiFormatStyle, Rectangle rectangle, bool isChecked, bool isIndeterminate)
		{
			bool isEnabled = base.Enabled && !isIndeterminate;
			bool isHot = GetIsHot();
			bool isPressedOrChecked = GetIsPressed() || isChecked;
			Rectangle clientRectangle = base.ClientRectangle;
			if (_themeRenderer != null)
			{
				_themeRenderer.DrawPushButtonTheme(graphics, base.ClientRectangle, isHot || isChecked, isPressedOrChecked || isChecked, isEnabled, base.ContainsFocus);
				return;
			}
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, rectangle, clientRectangle);
			PaintStandardBorder(graphics, clientRectangle, uiFormatStyle, isChecked, isHot, isPressedOrChecked, isIndeterminate);
		}

		private void PaintStandardBorder(Graphics graphics, Rectangle rectangle, FormatStyle uiFormatStyle, bool isChecked, bool isHot, bool isPressed, bool isIndeterminate)
		{
			if ((isHot && isPressed) || isChecked)
			{
				RenderingHelper.DrawColoredBorder(graphics, uiFormatStyle.BackColor, rectangle, Mordex.Windows.UI.BorderStyle.SunkenLight3D);
				return;
			}
			if (isIndeterminate)
			{
				Pen pen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.ControlDark));
				RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
				return;
			}
			RenderingHelper.DrawColoredBorder(graphics, uiFormatStyle.BackColor, rectangle, Mordex.Windows.UI.BorderStyle.RaisedLight3D);
		}

		// Draws the icon, image or image-list image in the bounds the layout placed. A disabled image is grayed against backColor.
		private void DrawContentImage(Graphics graphics, Color backColor)
		{
			if (_imageBounds.IsEmpty)
			{
				return;
			}
			if (_image == null && ImageList != null)
			{
				int imageIndex = GetImageListIndex();
				if (imageIndex >= 0 && imageIndex < ImageList.Images.Count)
				{
					// The image list returns a new copy of the image on each read.
					using (Image copy = ImageList.Images[imageIndex])
					{
						RenderingHelper.DrawStateImage(graphics, Icon, copy, imageIndex, ImageList, _imageBounds, base.Enabled, false, false, backColor, true, null);
					}
					return;
				}
			}
			RenderingHelper.DrawStateImage(graphics, Icon, _image, -1, null, _imageBounds, base.Enabled, false, false, backColor, true, null);
		}

		internal void PaintContent(Graphics graphics, FormatStyle uiFormatStyle, TextLayoutRenderer textLayoutRenderer)
		{
			DrawContentImage(graphics, uiFormatStyle.BackColor);
			Color color = uiFormatStyle.GetEffectiveForeColor();
			if (!color.IsEmpty)
			{
				textLayoutRenderer.DrawText(graphics, Text, _textBounds, uiFormatStyle.ResolveFont(Font), color);
			}
			if (!Focused || !ShowFocusRectangle)
			{
				return;
			}
			Pen pen = new Pen(SystemColors.WindowFrame);
			pen.DashStyle = DashStyle.Dot;
			Rectangle clientRectangle = _textBounds;
			if (_appearance == System.Windows.Forms.Appearance.Button)
			{
				clientRectangle = base.ClientRectangle;
				clientRectangle.Inflate(-FocusRectInflateInButton, -FocusRectInflateInButton);
			}
			else
			{
				if (!UseCompatibleTextRendering)
				{
					clientRectangle.Inflate(2, 0);
				}
				if (clientRectangle.Height > base.ClientRectangle.Height)
				{
					clientRectangle.Height = base.ClientRectangle.Height;
				}
			}
			RenderingHelper.DrawRectangleOutline(graphics, pen, clientRectangle);
			pen.Dispose();
		}
	}
}
