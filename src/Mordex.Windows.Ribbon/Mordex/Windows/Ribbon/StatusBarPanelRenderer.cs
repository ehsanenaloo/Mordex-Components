using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class StatusBarPanelRenderer : RibbonButtonRenderer
	{
		private bool _hasText;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetPanel() != null && GetPanel().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				if (GetPanel() != null && GetPanel().PanelBehavior == PanelBehavior.Label)
				{
					return AccessibleRole.Pane;
				}
				return base.AccessibleRole;
			}
		}

		public override bool Focusable
		{
			get
			{
				if (GetPanel() != null)
				{
					if (GetPanel().PanelBehavior == PanelBehavior.Label)
					{
						return false;
					}
					return true;
				}
				return base.Focusable;
			}
		}

		public StatusBarPanelRenderer(ButtonCommand command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
			_hasText = command.GetEffectiveCommandStyle() != CommandStyle.Image;
			_isSingleWordText = command.Text.IndexOf(" ") <= 0;
		}

		public StatusBarPanel GetPanel()
		{
			return GetCommand() as StatusBarPanel;
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (!(GetCommand() is IMouseEventInvoker) || !GetCommand().Enabled)
			{
				return;
			}
			if (eventArgs.Button == MouseButtons.Left)
			{
				if (Pushed && GetPanel().PanelBehavior == PanelBehavior.Button)
				{
					GetCommand().InvokeOnClick();
				}
			}
			else if (GetRenderHost().MouseDownItem == this)
			{
				GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
			}
			((IMouseEventInvoker)GetCommand()).InvokeMouseUp(eventArgs);
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			base.CalculateFullItemWidth(graphics, textRenderer, font);
			if (GetPanel().AutoSize == StatusBarPanelAutoSize.None)
			{
				_fullItemWidth = GetPanel().Width;
			}
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
			if (GetRenderHost() == null)
			{
				return;
			}
			if (_hasImageContent)
			{
				textRenderer.GetStringAlignment();
				switch (GetPanel().TextAlignment)
				{
				case HorizontalAlignment.Left:
					textRenderer.SetStringAlignment(StringAlignment.Near);
					break;
				case HorizontalAlignment.Center:
					textRenderer.SetStringAlignment(StringAlignment.Center);
					break;
				case HorizontalAlignment.Right:
					textRenderer.SetStringAlignment(StringAlignment.Far);
					break;
				}
				int height = SmallItemSize.Height;
				Size smallItemSize = SmallItemSize;
				Rectangle rectangle = _bounds;
				int x = RibbonDrawingUtils.LayoutImageAndText(graphics, smallItemSize, ref rectangle, width, GetPanel().ImageAlignment, GetPanel().Text, textRenderer, GetRenderHost().Font, GetPanel().ForegroundStyle);
				_imageBounds = new Rectangle(x, top + (itemHeight - height) / 2, smallItemSize.Width, smallItemSize.Height);
				_textBounds = rectangle;
			}
			else
			{
				_textBounds = new Rectangle(left + 2, top, width - 4, itemHeight);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			StringAlignment stringAlignment = textRenderer.GetStringAlignment();
			switch (GetPanel().TextAlignment)
			{
			case HorizontalAlignment.Left:
				textRenderer.SetStringAlignment(StringAlignment.Near);
				break;
			case HorizontalAlignment.Center:
				textRenderer.SetStringAlignment(StringAlignment.Center);
				break;
			case HorizontalAlignment.Right:
				textRenderer.SetStringAlignment(StringAlignment.Far);
				break;
			}
			bool isHot = Hot || Active;
			bool isPushed = Pushed;
			bool isChecked = GetButtonCommand().Checked;
			if (GetPanel().PanelBehavior == PanelBehavior.Label)
			{
				isHot = false;
				isPushed = false;
			}
			if (DroppedDown)
			{
				isPushed = false;
				isChecked = true;
				isHot = false;
			}
			if ((GetCommand().IsEnabled || Active) && (isHot || isChecked || isPushed))
			{
				DrawButtonBackground(graphics, _bounds, colorScheme, SizeStyle, isHot, isChecked, isPushed);
			}
			if (_hasTextContent)
			{
				Rectangle rectangle = _textBounds;
				textRenderer.SetLineAlignment(StringAlignment.Center);
				textRenderer.SetWrapEnabled(false);
				textRenderer.RenderContent(graphics, Text, rectangle, GetButtonCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, GetButtonCommand(), GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, SizeStyle, UseBitmap, colorScheme);
			}
			textRenderer.SetStringAlignment(stringAlignment);
		}
	}
}
