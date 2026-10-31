using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageMenuAddCommandRenderer : CommandBaseRenderer
	{
		private MxRibbonContextMenu _addCommandMenu;

		private ICommandDesignTarget _designTarget;

		private int _dropArrowWidth = 12;

		private Rectangle _dropArrowBounds;

		private Rectangle _dropArrowHitBounds;

		public override CommandType CommandType => CommandType.SmallButton;

		internal BackstageMenuAddCommandRenderer(MxRibbon ribbon, ICommandDesignTarget owner)
			: base(ribbon)
		{
			_commandBaseRendererRibbon = ribbon;
			_designTarget = owner;
		}

		public override void Dispose()
		{
			if (_addCommandMenu != null)
			{
				_addCommandMenu.CommandClick -= OnAddCommandMenuClick;
				_addCommandMenu.Dispose();
				_addCommandMenu = null;
			}
			base.Dispose();
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			textRenderer.SetWrapEnabled(false);
			_dropArrowWidth = 12;
			int textWidth = (int)textRenderer.MeasureStyledText(graphics, "Add New Command ", null, font, 0, 0).Width + 2;
			_hasTextContent = true;
			_fullItemWidth = textWidth + _dropArrowWidth;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_textBounds = new Rectangle(left + 2, top, width - 4, itemHeight);
			_bounds = new Rectangle(left, top, width, itemHeight);
			_dropArrowBounds = new Rectangle(_bounds.Right - _dropArrowWidth - 1, _bounds.Top, _dropArrowWidth, _bounds.Height);
			_dropArrowHitBounds = new Rectangle(_bounds.Right - _dropArrowWidth - 1, _textBounds.Y, _dropArrowWidth + 1, _textBounds.Height);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (Pushed)
			{
				RibbonChromePainter.DrawSelectedHotButtonBackground(graphics, _bounds, colorScheme, true);
			}
			else
			{
				RibbonChromePainter.DrawHotButtonBackground(graphics, _bounds, colorScheme, true);
			}
			Rectangle rectangle = _textBounds;
			textRenderer.SetLineAlignment(StringAlignment.Center);
			textRenderer.SetStringAlignment(StringAlignment.Center);
			Brush brush = GdiCache.Brush(foreColor);
			textRenderer.DrawTextWithBrush(graphics, "Add New Command ", rectangle, font, brush);
			textRenderer.SetWrapEnabled(false);
			ThemedRibbonPainter.Arrow(graphics, colorScheme.Theme, _dropArrowBounds, _dropArrowBounds.Width, 1, true, true);
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				Point pos = _designTarget.PointToScreen(new Point(_bounds.Left, _bounds.Bottom));
				Pushed = true;
				_commandBaseRendererRibbon._designerItemPressed = true;
				GetAddCommandMenu()._upwardOffset = _bounds.Height;
				GetAddCommandMenu().Show(pos);
				_commandBaseRendererRibbon._designerItemPressed = false;
				Pushed = false;
			}
		}

		public MxRibbonContextMenu GetAddCommandMenu()
		{
			if (_addCommandMenu == null)
			{
				_addCommandMenu = new MxRibbonContextMenu();
				_addCommandMenu.ImageList = _designTarget.DesignerImageList;
				if (_designTarget.CanAddCommand(CommandType.BackstageTab))
				{
					ButtonCommand buttonCommand = new ButtonCommand("BackstageTab", "Backstage Tab");
					buttonCommand.ImageIndex = 4;
					_addCommandMenu.Commands.Add(buttonCommand);
				}
				if (_designTarget.CanAddCommand(CommandType.SmallButton))
				{
					ButtonCommand smallButtonCommand = new ButtonCommand("ButtonCommand", "Button Command");
					smallButtonCommand.ImageIndex = 4;
					_addCommandMenu.Commands.Add(smallButtonCommand);
				}
				_addCommandMenu.CommandClick += OnAddCommandMenuClick;
			}
			return _addCommandMenu;
		}

		private void OnAddCommandMenuClick(object sender, CommandEventArgs eventArgs)
		{
			CommandBase commandBase = null;
			string key;
			if ((key = eventArgs.Command.Key) != null)
			{
				if (!(key == "BackstageTab"))
				{
					if (key == "ButtonCommand")
					{
						commandBase = new ButtonCommand();
						((ButtonCommand)commandBase).SizeStyle = CommandSizeStyle.Small;
					}
				}
				else
				{
					commandBase = new BackstageTab();
				}
			}
			_designTarget.AddNewCommand(commandBase, editItem: false);
			string clickedCommandKey;
			if ((clickedCommandKey = eventArgs.Command.Key) != null)
			{
				if (!(clickedCommandKey == "ButtonCommand"))
				{
					if (clickedCommandKey == "BackstageTab")
					{
						((BackstageTab)commandBase).Text = commandBase.Key;
					}
				}
				else
				{
					((ButtonCommand)commandBase).Text = commandBase.Key;
				}
			}
			_commandBaseRendererRibbon.DesignerSelectCommand(commandBase);
		}
	}
}
