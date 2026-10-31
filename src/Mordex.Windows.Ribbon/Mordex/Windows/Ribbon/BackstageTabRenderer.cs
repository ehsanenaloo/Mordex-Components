using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageTabRenderer : CommandBaseRenderer
	{
		private BackstageTab _backStageTab;

		private BackstageMenu _backstageMenu;

		public override CommandType CommandType => CommandType.BackstageTab;

		public BackstageTabRenderer(BackstageTab backStageTab, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(backStageTab, owner, layoutItemOwner)
		{
			_backStageTab = backStageTab;
			if (owner is BackstageMenuScrollableBase backstageMenuScrollableBase)
			{
				_backstageMenu = backstageMenuScrollableBase.GetBackstageMenu();
			}
		}

		public BackstageTab GetBackStageTab()
		{
			return _backStageTab;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (_backStageTab != null && _backStageTab.Font != null)
			{
				font = _backStageTab.Font;
			}
			if (GetRenderHost() != null)
			{
				_hasImageContent = RibbonDrawingUtils.HasImageSource(_backStageTab.Icon, _backStageTab.Image, GetRenderHost().GetImageList(GetCommand()), _backStageTab.ImageIndex, _backStageTab.ImageKey);
				textRenderer.SetWrapEnabled(false);
				int textWidth = 0;
				if (GetTabText().Length > 0)
				{
					textWidth = (int)textRenderer.MeasureStyledText(graphics, GetTabText(), _backStageTab.ForegroundStyle, font, 0, 0).Width;
					_hasTextContent = true;
					if (!_hasImageContent)
					{
						textWidth += 4;
					}
				}
				else
				{
					_hasTextContent = false;
				}
				int imageWidth = 0;
				if (_hasImageContent)
				{
					imageWidth = GetSmallImageSize().Width + 6;
				}
				_fullItemWidth = imageWidth + textWidth;
				if (_fullItemWidth < GetSmallImageSize().Width + 6)
				{
					_fullItemWidth = GetSmallImageSize().Width + 6;
				}
				_fullItemWidth += 28;
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() == null)
			{
				return;
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
			left += 6;
			width -= 12;
			if (_hasImageContent)
			{
				int imageWidth = GetSmallImageSize().Width;
				int height = GetSmallImageSize().Height;
				if (!_hasTextContent)
				{
					_imageBounds = new Rectangle(left + 15, top + (itemHeight - height) / 2, imageWidth, height);
				}
				else
				{
					_imageBounds = new Rectangle(left + 14, top + (itemHeight - height) / 2, imageWidth, height);
				}
				_textBounds = new Rectangle(left + imageWidth + 16, top, width - imageWidth - 4, itemHeight);
			}
			else
			{
				_textBounds = new Rectangle(left + 14, top, width - 4, itemHeight);
			}
		}

		private void DrawSelectedBackground(Graphics graphics, Rectangle bounds, Color baseColor)
		{
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			Rectangle rect = bounds;
			if (rect.Height > 0 && rect.Width > 0)
			{
				using (LinearGradientBrush brush = new LinearGradientBrush(rect, baseColor, baseColor, LinearGradientMode.Vertical))
				{
					graphics.FillRectangle(brush, bounds);
				}
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			DrawSelectedGlow(graphics, bounds, baseColor);
			int brightness = RibbonDrawingUtils.GetLightness(baseColor);
			Color color = ((brightness <= 120) ? RibbonDrawingUtils.DarkenColor(baseColor, 0.1f) : RibbonDrawingUtils.DarkenColor(baseColor, 0.35f));
			int[] alphaSteps = new int[3] { 255, 80, 25 };
			for (int i = 0; i < 3; i++)
			{
				Pen pen = GdiCache.Pen(Color.FromArgb(alphaSteps[i], color));
				graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
				graphics.DrawLine(pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
				bounds.Inflate(0, -1);
			}
			graphics.SmoothingMode = smoothingMode;
			using GraphicsPath graphicsPath = new GraphicsPath();
			float arrowTop = (float)bounds.Y + (float)(bounds.Height - 15) / 2f;
			float arrowMiddle = arrowTop + 8f;
			float arrowBottom = arrowTop + 16f;
			float arrowLeft = bounds.Right - 8;
			float arrowRight = bounds.Right;
			alphaSteps = new int[5] { 135, 55, 32, 18, 8 };
			for (int j = 0; j < 5; j++)
			{
				RectangleF arrowRect = new RectangleF(arrowLeft, arrowTop, arrowRight - arrowLeft, arrowBottom - arrowTop);
				using (Brush arrowBrush = new LinearGradientBrush(RectangleF.Inflate(arrowRect, 1f, 1f), Color.FromArgb(alphaSteps[j] - 8, color), Color.FromArgb(alphaSteps[j], color), LinearGradientMode.Horizontal))
				{
					using (Pen arrowPen = new Pen(arrowBrush))
					{
						graphics.DrawLine(arrowPen, arrowRight, arrowTop, arrowLeft + 0.5f, arrowMiddle);
						graphics.DrawLine(arrowPen, arrowLeft + 1f, arrowMiddle + 1f, arrowRight, arrowBottom);
					}
				}
				arrowLeft -= 1f;
				arrowTop -= 1f;
				arrowBottom += 1f;
			}
			arrowTop = (float)bounds.Y + (float)(bounds.Height - 15) / 2f;
			arrowMiddle = arrowTop + 8f;
			arrowBottom = arrowTop + 16f;
			arrowLeft = bounds.Right - 8;
			arrowRight = bounds.Right;
			graphicsPath.AddLine(arrowRight, arrowTop + 1f, arrowLeft + 1f, arrowMiddle);
			graphicsPath.AddLine(arrowLeft + 1f, arrowMiddle, arrowRight, arrowBottom - 1f);
			graphics.FillPath(Brushes.White, graphicsPath);
		}

		internal void DrawSelectedGlow(Graphics graphics, Rectangle rect, Color baseColor)
		{
			Rectangle empty = rect;
			if (empty.Width <= 0 || empty.Height <= 0)
			{
				return;
			}
			int brightness = RibbonDrawingUtils.GetLightness(baseColor);
			using Region clip = graphics.Clip;
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			try
			{
				using (Region region = new Region(rect))
				{
					graphics.Clip = region;
				}
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddRectangle(empty);
					using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
					if (brightness > 120)
					{
						pathGradientBrush.CenterColor = Color.FromArgb(5, Color.Black);
					}
					else
					{
						pathGradientBrush.CenterColor = Color.FromArgb(35, Color.White);
					}
					pathGradientBrush.SurroundColors = new Color[1] { Color.Empty };
					pathGradientBrush.CenterPoint = new Point(empty.X + empty.Width / 2, empty.Y + empty.Height / 2);
					graphics.FillRectangle(pathGradientBrush, rect);
				}
				double redScale = 2.0;
				double greenScale = 2.0;
				double blueScale = 2.0;
				if (brightness > 120)
				{
					redScale = 1f + (1f - (float)(int)baseColor.R / 255f);
					greenScale = 1f + (1f - (float)(int)baseColor.G / 255f);
					blueScale = 1f + (1f - (float)(int)baseColor.B / 255f);
				}
				double brightRed = Math.Min((double)(int)baseColor.R * redScale, 255.0);
				double brightGreen = Math.Min((double)(int)baseColor.G * greenScale, 255.0);
				double brightBlue = Math.Min((double)(int)baseColor.B * blueScale, 255.0);
				Color color = Color.FromArgb(255, RibbonDrawingUtils.BlendColors(Color.FromArgb((int)brightRed, (int)brightGreen, (int)brightBlue), Color.White, 0.85f));
				empty = new Rectangle(rect.X - rect.Width / 2, rect.Top - rect.Height / 2, rect.Width + rect.Width / 2 * 2, rect.Height * 3);
				using (GraphicsPath glowPath = new GraphicsPath())
				{
					glowPath.AddEllipse(empty);
					using PathGradientBrush glowBrush = new PathGradientBrush(glowPath);
					glowBrush.CenterColor = color;
					glowBrush.SurroundColors = new Color[1] { Color.FromArgb(0, Color.White) };
					glowBrush.CenterPoint = new Point(empty.X + empty.Width / 2, empty.Y + empty.Height / 2);
					graphics.FillPath(glowBrush, glowPath);
				}
				empty = new Rectangle(rect.X + rect.Width / 6, rect.Top - rect.Height / 2, rect.Width / 3 * 2, rect.Height * 3);
				using GraphicsPath highlightPath = new GraphicsPath();
				highlightPath.AddEllipse(empty);
				using PathGradientBrush highlightBrush = new PathGradientBrush(highlightPath);
				color = Color.FromArgb(160, RibbonDrawingUtils.BlendColors(Color.FromArgb((int)brightRed, (int)brightGreen, (int)brightBlue), Color.White, 0.65f));
				highlightBrush.CenterColor = color;
				highlightBrush.SurroundColors = new Color[1] { Color.Transparent };
				highlightBrush.CenterPoint = new Point(empty.X + empty.Width / 2, empty.Y + empty.Height / 2);
				graphics.FillPath(highlightBrush, highlightPath);
			}
			finally
			{
				graphics.Clip = clip;
				graphics.SmoothingMode = smoothingMode;
			}
		}

		internal override void DrawButtonBackground(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme, CommandSizeStyle sizeStyle, bool hot, bool selected, bool pushed)
		{
			ThemedRibbonPainter.BackstageRow(graphics, colorScheme.Theme, rect, hot, selected, pushed);
			return;
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			if (GetCommand() != null && GetRenderHost() != null)
			{
				if (_backStageTab != null && _backStageTab.Font != null)
				{
					font = _backStageTab.Font;
				}
				bool isHighlighted = Hot || Active;
				bool pushed = Pushed;
				if (Selected || ((GetCommand().IsEnabled || Active) && (isHighlighted || pushed)))
				{
					DrawButtonBackground(graphics, _bounds, colorScheme, CommandSizeStyle.Small, isHighlighted, Selected, pushed);
				}
				if (_hasTextContent)
				{
					Rectangle rectangle = _textBounds;
					textRenderer.SetLineAlignment(StringAlignment.Center);
					textRenderer.SetStringAlignment(StringAlignment.Near);
					textRenderer.RenderContent(graphics, GetTabText(), rectangle, _backStageTab.ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
					textRenderer.SetWrapEnabled(false);
				}
				if (_hasImageContent)
				{
					RibbonDrawingUtils.DrawImageFromSources(graphics, _backStageTab.Icon, _backStageTab.Image, _backStageTab.ImageIndex, _backStageTab.ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, _backStageTab.IsEnabled, GetRenderHost().Ribbon.IsGlassCaptionActive(), colorScheme);
				}
			}
		}

		public string GetTabText()
		{
			return _backStageTab.Text;
		}

		public Size GetSmallImageSize()
		{
			return GetRenderHost().SmallImageSize;
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (_backStageTab != null && _backStageTab.Font != null)
			{
				font = _backStageTab.Font;
			}
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			SizeF sizeF = textRenderer.MeasureText(graphics, keyTip, font, 0, 0);
			int x = _textBounds.X - (int)sizeF.Width + 4;
			int y = _textBounds.Y + _textBounds.Height / 2 - (int)sizeF.Height - 4;
			return new Point(x, y);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (GetCommand() is IMouseEventInvoker && GetCommand().IsEnabled)
			{
				if (eventArgs.Button == MouseButtons.Left)
				{
					if (_commandBaseRendererOwner.Ribbon != null && _commandBaseRendererOwner.Ribbon.GetPopupMenu() != null)
					{
						_commandBaseRendererOwner.Ribbon.GetPopupMenu().TryCloseOwnerChildMenu(CommandCloseReason.CommandClicked);
					}
					GetCommand().InvokeOnClick();
				}
				else if (GetRenderHost() != null && GetRenderHost().MouseDownItem == this && eventArgs.Button == MouseButtons.Right)
				{
					GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
				}
				((IMouseEventInvoker)GetCommand()).InvokeMouseUp(eventArgs);
			}
			else if (eventArgs.Button == MouseButtons.Right && GetRenderHost() != null && GetRenderHost().MouseDownItem == this && GetCommand() != null && GetCommand().CommandType != CommandType.Separator)
			{
				GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
			}
		}
	}
}
