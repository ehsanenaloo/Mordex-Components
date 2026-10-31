using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class PopupOwnerItemRenderer : CommandBaseRenderer
	{
		private TextBox _textBox;

		private Rectangle _textBoxBounds;

		private CommandSizeStyle _sizeStyle;

		internal int _preferredHeight;

		public override CommandType CommandType => CommandType.ContainerControl;

		public PopupOwnerItemRenderer(CommandBase command, ICommandRenderHost owner, CommandSizeStyle sizeStyle)
			: base(command, owner, null)
		{
			_sizeStyle = sizeStyle;
		}

		public string GetCommandText()
		{
			if (_commandBaseRendererCommand is ButtonCommand)
			{
				return ((ButtonCommand)_commandBaseRendererCommand).Text;
			}
			return "";
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_textBox != null)
			{
				_textBox.Dispose();
			}
		}

		public TextBox GetOrCreateTextBox()
		{
			if (_textBox == null)
			{
				_textBox = new TextBox();
				_textBox.BorderStyle = BorderStyle.FixedSingle;
				_textBox.Width = 100;
				_textBox.Text = GetCommandText();
			}
			return _textBox;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				_hasImageContent = false;
				_hasTextContent = false;
				int imageAreaWidth = 100;
				int textBoxWidth = GetRenderHost().SmallImageSize.Width + 8;
				if (_sizeStyle == CommandSizeStyle.Large)
				{
					_preferredHeight = GetRenderHost().LargeImageSize.Width + 12;
					_fullItemWidth = textBoxWidth + imageAreaWidth + 20;
				}
				else
				{
					_preferredHeight = GetRenderHost().SmallItemHeight;
					_fullItemWidth = textBoxWidth + imageAreaWidth + 20;
				}
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() != null)
			{
				int imageAreaWidth = GetRenderHost().SmallImageSize.Width + 8;
				_imageBounds = new Rectangle(left, top, imageAreaWidth, itemHeight);
				int x = left + imageAreaWidth + 10;
				_textBounds = Rectangle.Empty;
				_textBoxBounds = new Rectangle(x, top + (itemHeight - GetOrCreateTextBox().Height) / 2, 100, GetOrCreateTextBox().Height);
				_bounds = new Rectangle(left, top, width, itemHeight);
				ShowTextBox();
			}
		}

		internal void ShowTextBox()
		{
			if (GetRenderHost() != null && GetRenderHost().ParentControl != null && !GetRenderHost().ParentControl.Controls.Contains(GetOrCreateTextBox()))
			{
				GetOrCreateTextBox().Bounds = _textBoxBounds;
				GetRenderHost().ParentControl.Controls.Add(GetOrCreateTextBox());
				GetOrCreateTextBox().Focus();
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			Brush brush = GdiCache.Brush(colorScheme._menuBackground);
			graphics.FillRectangle(brush, _imageBounds);
			Pen pen = GdiCache.Pen(colorScheme._menuSeparator);
			graphics.DrawLine(pen, _imageBounds.Right, _imageBounds.Y, _imageBounds.Right, _imageBounds.Bottom);
		}
	}
}
