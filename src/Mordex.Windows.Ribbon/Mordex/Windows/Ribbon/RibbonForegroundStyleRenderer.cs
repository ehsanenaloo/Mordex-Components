using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonForegroundStyleRenderer : CommandBaseRenderer
	{
		private string _text;

		private RibbonForegroundStyle _foregroundStyle;

		internal bool _isInControlBoxPanel;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				if (_text.Length > 0)
				{
					return AccessibleRole.StaticText;
				}
				return AccessibleRole.Separator;
			}
		}

		public override CommandType CommandType => CommandType.Separator;

		public RibbonForegroundStyleRenderer(CommandBase command, ICommandRenderHost owner)
			: base(command, owner, null)
		{
			if (command is GalleryItem)
			{
				_text = ((GalleryItem)command).Text ?? "";
				_foregroundStyle = ((GalleryItem)command).ForegroundStyle;
			}
			else if (command is SeparatorCommand)
			{
				_text = ((SeparatorCommand)command).Title ?? "";
				_foregroundStyle = ((SeparatorCommand)command).ForegroundStyle;
			}
			else
			{
				_text = "";
			}
		}

		public int GetPreferredHeight(Font font)
		{
			if (_text.Length > 0)
			{
				return font.Height + 6;
			}
			return 2;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (_text.Length > 0)
			{
				_fullItemWidth = (int)textRenderer.MeasureStyledText(graphics, _text, _foregroundStyle, font, 0, 0).Width + 1;
				_fullItemWidth += 17;
			}
			else
			{
				_fullItemWidth = 4;
			}
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
			_textBounds = new Rectangle(left + 10, top, width - 7, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			using Pen pen = new Pen(colorScheme._menuSeparator);
			if (_text.Length > 0)
			{
				Rectangle rect = _bounds;
				if (!_isInControlBoxPanel)
				{
					Brush brush = GdiCache.Brush(colorScheme._menuSectionHeaderBackground);
					graphics.FillRectangle(brush, rect);
				}
				StringAlignment stringAlignment = textRenderer.GetStringAlignment();
				textRenderer.SetLineAlignment(StringAlignment.Center);
				textRenderer.SetStringAlignment(StringAlignment.Near);
				textRenderer.RenderContent(graphics, _text, _textBounds, _foregroundStyle, font, foreColor, colorScheme._disabledTextColor, true);
				textRenderer.SetStringAlignment(stringAlignment);
			}
			else
			{
				pen.DashStyle = DashStyle.Dash;
				int lineStart = _bounds.Left;
				if (!_isInControlBoxPanel)
				{
					lineStart += GetRenderHost().SmallImageSize.Width + 16;
				}
				graphics.DrawLine(pen, lineStart, _bounds.Top, _bounds.Right - 1, _bounds.Top);
				if (_isInControlBoxPanel)
				{
					graphics.DrawLine(Pens.White, lineStart, _bounds.Top + 1, _bounds.Right - 1, _bounds.Top + 1);
				}
			}
		}
	}
}
