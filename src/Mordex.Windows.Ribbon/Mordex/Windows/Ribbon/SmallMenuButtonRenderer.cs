using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class SmallMenuButtonRenderer : CommandBaseRenderer, IDropDownItem
	{
		internal Rectangle _arrowBounds;

		private bool _isArrowHot;

		internal bool _isMainAreaPressed;

		protected bool _isMenuItemEnabled;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				if (HasChildren)
				{
					return AccessibleRole.MenuPopup;
				}
				return AccessibleRole.MenuItem;
			}
		}

		public override bool HasChildren
		{
			get
			{
				if (GetDropDownCommand() != null)
				{
					if (GetDropDownCommand().Commands.Count > 0)
					{
						return true;
					}
					if (GetCommand() is Gallery)
					{
						return true;
					}
					if (GetCommand() is MdiListCommand && GetRenderHost().ParentForm != null && GetRenderHost().ParentForm.MdiChildren.Length > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public override CommandType CommandType => CommandType.SmallButton;

		public override bool Focusable
		{
			get
			{
				if (GetCommand() != null && GetCommand().CommandType == CommandType.Label)
				{
					return false;
				}
				return true;
			}
		}

		public SmallMenuButtonRenderer(ButtonCommand command, ICommandRenderHost owner)
			: base(command, owner, null)
		{
			_isMenuItemEnabled = true;
		}

		public DropDownCommand GetDropDownCommand()
		{
			return _commandBaseRendererCommand as DropDownCommand;
		}

		public bool ShouldShowShortcut()
		{
			if (GetButtonCommand() != null && GetButtonCommand().Shortcut != Shortcut.None)
			{
				if (GetRenderHost() != null)
				{
					if (GetRenderHost().Ribbon != null)
					{
						return GetRenderHost().Ribbon.ShowShortcutInMenus;
					}
					if (GetRenderHost() is CommandBaseEditorDialog)
					{
						MxRibbonContextMenu ribbonContextMenu = ((CommandBaseEditorDialog)GetRenderHost()).GetRibbonContextMenu();
						if (ribbonContextMenu != null)
						{
							return ribbonContextMenu.ShowShortcutInMenus;
						}
					}
				}
				return true;
			}
			return false;
		}

		public bool IsArrowHot()
		{
			return _isArrowHot;
		}

		public void SetArrowHot(bool ribbonRenderer2Boolean)
		{
			if (_isArrowHot != ribbonRenderer2Boolean)
			{
				_isArrowHot = ribbonRenderer2Boolean;
				Invalidate();
			}
		}

		public bool IsArrowPressed()
		{
			return false;
		}

		public bool IsMenuOpen()
		{
			return false;
		}

		public ButtonCommand GetButtonCommand()
		{
			return _commandBaseRendererCommand as ButtonCommand;
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			return new Point(_bounds.X + 12, _bounds.Bottom - 12);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			int arrowWidth = 18;
			if (GetRenderHost() != null)
			{
				Size smallImageSize = GetRenderHost().SmallImageSize;
				int imageAreaWidth = smallImageSize.Width + 8;
				_imageBounds = new Rectangle(left + 4, top + 4, smallImageSize.Width, smallImageSize.Height);
				_textBounds = new Rectangle(left + imageAreaWidth + 7, top, width - imageAreaWidth - arrowWidth - 6, itemHeight);
				_bounds = new Rectangle(left, top, width, itemHeight);
				if (GetDropDownCommand() != null && GetDropDownCommand().DropDownStyle == DropDownButtonStyle.SplitButton)
				{
					_arrowBounds = new Rectangle(_bounds.Right - arrowWidth - 1, top, arrowWidth + 1, itemHeight);
				}
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				int arrowWidth = 18;
				_hasImageContent = RibbonDrawingUtils.HasButtonImage(GetButtonCommand(), CommandSizeStyle.Small, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
				int imageAreaWidth = GetRenderHost().SmallImageSize.Width + 8;
				string text = GetButtonCommand().Text;
				if (ShouldShowShortcut())
				{
					text = text + "   " + RibbonDrawingUtils.GetShortcutText(GetButtonCommand().Shortcut);
				}
				int textWidth = (int)textRenderer.MeasureStyledText(graphics, text, GetButtonCommand().ForegroundStyle, GetRenderHost().Font, 0, 0).Width + 1 + 8;
				_fullItemWidth = imageAreaWidth + textWidth + arrowWidth;
			}
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_arrowBounds.Contains(eventArgs.X, eventArgs.Y))
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
			_isMainAreaPressed = false;
			if (eventArgs.Button == MouseButtons.Left && GetDropDownCommand() != null && GetDropDownCommand().DropDownStyle == DropDownButtonStyle.SplitButton && !_arrowBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				_isMainAreaPressed = true;
			}
			base.OnMouseDown(eventArgs);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (Hot || Selected)
			{
				if (GetCommand().IsEnabled)
				{
					DrawButtonBackground(graphics, _bounds, colorScheme, CommandSizeStyle.Small, hot: true, selected: false, pushed: false);
				}
				else
				{
					RibbonChromePainter.DrawHotMenuItemBackground(graphics, _bounds, colorScheme, true);
				}
			}
			if (HasChildren)
			{
				Pen pen = GdiCache.Pen(foreColor);
				Rectangle rectangle = new Rectangle(_bounds.Right - 19, _bounds.Y, 18, _bounds.Height);
				RibbonDrawingUtils.DrawHorizontalArrow(graphics, 17, rectangle, 1, pen);
			}
			textRenderer.SetLineAlignment(StringAlignment.Center);
			textRenderer.SetStringAlignment(StringAlignment.Near);
			textRenderer.RenderContent(graphics, GetButtonCommand().Text, _textBounds, GetButtonCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
			if (ShouldShowShortcut())
			{
				textRenderer.SetStringAlignment(StringAlignment.Far);
				textRenderer.RenderContent(graphics, RibbonDrawingUtils.GetShortcutText(GetButtonCommand().Shortcut), _textBounds, GetButtonCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
				textRenderer.SetStringAlignment(StringAlignment.Near);
			}
			if (GetButtonCommand() != null && GetButtonCommand().Checked)
			{
				Rectangle checkedImageBounds = _imageBounds;
				checkedImageBounds.Inflate(2, 2);
				DrawCheckedImageBackground(graphics, checkedImageBounds, colorScheme, _hasImageContent, foreColor);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, GetButtonCommand(), GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, CommandSizeStyle.Small, false, colorScheme);
			}
		}

		internal void DrawCheckedImageBackground(Graphics graphics, Rectangle imageBounds, RibbonColorScheme colorScheme, bool hasImage, Color color)
		{
			ThemedRibbonPainter.CheckedBackground(graphics, colorScheme.Theme, imageBounds);
			if (!hasImage)
			{
				ThemedRibbonPainter.Check(graphics, colorScheme.Theme, _imageBounds, ThemeRole.Text);
			}
			return;
		}

		internal void DrawSplitButtonBackground(Graphics graphics, RibbonColorScheme colorScheme, bool hot, bool selected, bool pushed)
		{
			bool open = IsMenuOpen() || IsArrowPressed();
			if (hot || open)
			{
				Rectangle body = new Rectangle(_bounds.X, _bounds.Y, _bounds.Width - _arrowBounds.Width, _bounds.Height);
				ThemedRibbonPainter.SplitFace(graphics, colorScheme.Theme, _bounds, body, IsArrowHot() || open, false, open);
			}
			else if (selected)
			{
				ThemedRibbonPainter.Face(graphics, colorScheme.Theme, _bounds, ThemeRole.AccentSubtle, ThemeRole.Accent);
			}
			return;
		}

		internal override void DrawButtonBackground(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme, CommandSizeStyle sizeStyle, bool hot, bool selected, bool pushed)
		{
			if (GetDropDownCommand() != null && GetDropDownCommand().DropDownStyle == DropDownButtonStyle.SplitButton)
			{
				DrawSplitButtonBackground(graphics, colorScheme, hot, selected, pushed);
			}
			else
			{
				base.DrawButtonBackground(graphics, rect, colorScheme, sizeStyle, hot, selected, pushed);
			}
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			if (GetDropDownCommand() != null)
			{
				DropDown(selectFirstItem);
			}
		}

		public virtual void DropDown(bool selectFirstItem)
		{
			GetDropDownCommand().DropDown();
		}
	}
}
