using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonDropDownButtonRenderer : RibbonButtonRenderer, IDropDownItem
	{
		private Rectangle _arrowBounds;

		private Rectangle _dropDownAreaBounds;

		private int _arrowWidth;

		private bool _isArrowHot;

		private bool _isArrowPressed;

		protected bool _isDropDownAllowed;

		public override string DefaultAction => "Open";

		public override string Text
		{
			get
			{
				if (_isSingleWordText || SizeStyle != CommandSizeStyle.Large)
				{
					return base.Text;
				}
				return GetButtonCommand().Text + "  ";
			}
		}

		public override bool HasChildren => true;

		public override int ArrowWidth => _arrowWidth;

		public RibbonDropDownButtonRenderer(DropDownCommand command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
			_isDropDownAllowed = true;
		}

		public DropDownCommand GetDropDownCommand()
		{
			return _commandBaseRendererCommand as DropDownCommand;
		}

		public override void DoDefaultAction()
		{
			DropDown(selectFirstItem: true);
		}

		public bool IsArrowHot()
		{
			return _isArrowHot;
		}

		public void SetArrowHot(bool ribbonRenderer8Boolean)
		{
			if (_isArrowHot != ribbonRenderer8Boolean)
			{
				_isArrowHot = ribbonRenderer8Boolean;
				Invalidate();
			}
		}

		public bool IsArrowPressed()
		{
			return _isArrowPressed;
		}

		public void SetArrowPressed(bool isArrowPressed)
		{
			if (_isArrowPressed != isArrowPressed)
			{
				_isArrowPressed = isArrowPressed;
				Invalidate();
			}
		}

		public override void MoveY(int newTop)
		{
			int offset = _bounds.Y - newTop;
			_bounds.Y = newTop;
			if (!_textBounds.IsEmpty)
			{
				_textBounds.Y -= offset;
			}
			if (!_imageBounds.IsEmpty)
			{
				_imageBounds.Y -= offset;
			}
			if (!_arrowBounds.IsEmpty)
			{
				_arrowBounds.Y -= offset;
			}
			if (!_dropDownAreaBounds.IsEmpty)
			{
				_dropDownAreaBounds.Y -= offset;
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_arrowWidth = (int)textRenderer.MeasureStyledText(graphics, "  ", GetButtonCommand().ForegroundStyle, font, 0, 0).Width - 1;
			if (_arrowWidth < 12)
			{
				_arrowWidth = 12;
			}
			base.CalculateFullItemWidth(graphics, textRenderer, font);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			base.ArrangeItem(graphics, textRenderer, left, top, width, itemHeight);
			if (CommandType == CommandType.SmallButton)
			{
				_arrowBounds = new Rectangle(_bounds.Right - _arrowWidth, _bounds.Top, _arrowWidth, _bounds.Height);
				_dropDownAreaBounds = new Rectangle(_bounds.Right - ArrowWidth - 1, _textBounds.Y, ArrowWidth + 1, _textBounds.Height);
				return;
			}
			using (RibbonFontLease font = GetButtonCommand().ForegroundStyle.LeaseFont(GetRenderHost().Font))
			{
				_arrowBounds = RibbonDrawingUtils.GetLastWordBounds(graphics, textRenderer, GetButtonCommand().Text, _textBounds, font.Font, _arrowWidth, _isSingleWordText);
			}
			_dropDownAreaBounds = new Rectangle(_textBounds.X, _textBounds.Y, _textBounds.Width, _bounds.Bottom - _textBounds.Y);
			_dropDownAreaBounds.Inflate(0, -1);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			base.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRect);
			Rectangle rectangle = _arrowBounds;
			RibbonChromePainter.DrawVerticalArrow(graphics, rectangle, 1, _isEnabled, colorScheme._glyphDisabledColor, colorScheme);
			return;
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_dropDownAreaBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				SetArrowHot(true);
			}
			else
			{
				SetArrowHot(false);
			}
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			((IMouseEventInvoker)GetCommand()).InvokeMouseDown(eventArgs);
			if (eventArgs.Button != MouseButtons.Left || !GetDropDownCommand().Enabled)
			{
				return;
			}
			if (GetDropDownCommand().DropDownStyle == DropDownButtonStyle.DropDown)
			{
				if (!DroppedDown)
				{
					DropDown(selectFirstItem: false);
				}
			}
			else if (_dropDownAreaBounds.Contains(eventArgs.X, eventArgs.Y) && !DroppedDown)
			{
				SetArrowPressed(true);
				DropDown(selectFirstItem: false);
				SetArrowPressed(true);
			}
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			SetArrowPressed(false);
			if (!GetCommand().Enabled)
			{
				return;
			}
			if (eventArgs.Button == MouseButtons.Left && GetDropDownCommand().DropDownStyle == DropDownButtonStyle.SplitButton)
			{
				if (!_isArrowPressed && !_dropDownAreaBounds.Contains(eventArgs.X, eventArgs.Y))
				{
					((ButtonCommand)GetCommand()).InvokeOnClick();
				}
			}
			else if (eventArgs.Button == MouseButtons.Right)
			{
				GetRenderHost().ShowQuickCustomizeMenu(GetCommand());
			}
			((IMouseEventInvoker)GetCommand()).InvokeMouseUp(eventArgs);
		}

		public virtual void DropDown(bool selectFirstItem)
		{
			if (!_isDropDownAllowed)
			{
				return;
			}
			DropDownCommand dropDownCommand = GetCommand() as DropDownCommand;
			if (dropDownCommand.CanOpenDropDown())
			{
				ICommandRenderHost dialogHost = GetRenderHost().ParentControl as ICommandRenderHost;
				if (dialogHost == null && GetRenderHost() is MxRibbon)
				{
					dialogHost = (MxRibbon)GetRenderHost();
				}
				CommandBaseEditorDialog commandBaseEditorDialog = new CommandBaseEditorDialog(dialogHost, dropDownCommand.Commands, this, dropDownCommand);
				commandBaseEditorDialog._selectFirstItem = selectFirstItem;
				DroppedDown = true;
				_isDropDownAllowed = false;
				if (dialogHost != null)
				{
					commandBaseEditorDialog.ShowAt(dialogHost.PointToScreen(new Point(_bounds.X, _bounds.Y)));
				}
				else
				{
					commandBaseEditorDialog.ShowAt(GetRenderHost().PointToScreen(new Point(_bounds.X, _bounds.Y)));
				}
				_isDropDownAllowed = true;
				DroppedDown = false;
			}
		}

		internal override void DrawButtonBackground(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme, CommandSizeStyle sizeStyle, bool hot, bool selected, bool pushed)
		{
			if (GetDropDownCommand().DropDownStyle == DropDownButtonStyle.SplitButton)
			{
				DrawOffice2010SplitButtonBackground(graphics, colorScheme, hot, selected, pushed);
			}
			else
			{
				base.DrawButtonBackground(graphics, rect, colorScheme, SizeStyle, hot, selected, pushed);
			}
		}

		private void DrawOffice2010SplitButtonBackground(Graphics graphics, RibbonColorScheme colorScheme, bool hot, bool selected, bool pushed)
		{
			Rectangle rectangle = _bounds;
			if (SizeStyle == CommandSizeStyle.Large)
			{
				rectangle.Height -= _dropDownAreaBounds.Height;
			}
			else
			{
				rectangle.Width -= _dropDownAreaBounds.Width - 1;
			}
			if (pushed || selected)
			{
				RibbonChromePainter.DrawSplitButtonActiveBackground(graphics, _bounds, rectangle, _dropDownAreaBounds, colorScheme, IsArrowPressed() || selected, SizeStyle == CommandSizeStyle.Large);
			}
			else if (hot)
			{
				RibbonChromePainter.DrawSplitButtonHotBackground(graphics, _bounds, rectangle, _dropDownAreaBounds, colorScheme, IsArrowHot(), SizeStyle == CommandSizeStyle.Large);
			}
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			DropDown(selectFirstItem);
		}
	}
}
