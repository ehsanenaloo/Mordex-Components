using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonGroupAddCommandRenderer : CommandBaseRenderer
	{
		private MxRibbonContextMenu _addCommandMenu;

		private ICommandDesignTarget _designTarget;

		private int _arrowWidth = 12;

		private Rectangle _arrowBounds;

		public override CommandType CommandType => CommandType.LargeButton;

		internal RibbonGroupAddCommandRenderer(MxRibbon ribbon, ICommandDesignTarget owner)
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
			_arrowWidth = (int)textRenderer.MeasureStyledText(graphics, "  ", null, font, 0, 0).Width - 1;
			if (_arrowWidth < 12)
			{
				_arrowWidth = 12;
			}
			int textWidth = 0;
			textWidth = RibbonDrawingUtils.MeasureTwoLineTextWidth(graphics, textRenderer, "Add New Command ", null, font, 0, _arrowWidth);
			if (_arrowWidth > 0 && RibbonDrawingUtils.MeasureLastWord(graphics, "Add New Command", new Rectangle(0, 0, textWidth, font.Height * 2), font).Right > (float)(textWidth - 4))
			{
				textWidth += _arrowWidth;
			}
			_hasTextContent = true;
			int itemWidth = _commandBaseRendererRibbon.LargeImageSize.Width + 10;
			if (textWidth > itemWidth)
			{
				itemWidth = textWidth;
			}
			_fullItemWidth = itemWidth;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			int y = top + _commandBaseRendererRibbon.LargeImageSize.Height + 4;
			_textBounds = new Rectangle(left, y, width, _commandBaseRendererRibbon.Font.Height * 2 + 2);
			_arrowBounds = RibbonDrawingUtils.GetLastWordBounds(graphics, textRenderer, "Add New Command", _textBounds, _commandBaseRendererRibbon.Font, _arrowWidth, false);
			_bounds = new Rectangle(left, top, width, itemHeight);
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
			textRenderer.SetWrapEnabled(true);
			textRenderer.SetLineAlignment(StringAlignment.Near);
			textRenderer.SetStringAlignment(StringAlignment.Center);
			rectangle.Y += 2;
			Brush brush = GdiCache.Brush(foreColor);
			textRenderer.DrawTextWithBrush(graphics, "Add New Command ", rectangle, font, brush);
			textRenderer.SetWrapEnabled(false);
			ThemedRibbonPainter.Arrow(graphics, colorScheme.Theme, _arrowBounds, _arrowBounds.Width, 1, true, true);
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				Point pos = _designTarget.PointToScreen(new Point(_bounds.Left, _bounds.Bottom));
				Pushed = true;
				_commandBaseRendererRibbon._designerItemPressed = true;
				GetOrCreateAddCommandMenu().Show(pos);
				_commandBaseRendererRibbon._designerItemPressed = false;
				Pushed = false;
			}
		}

		public MxRibbonContextMenu GetOrCreateAddCommandMenu()
		{
			if (_addCommandMenu == null)
			{
				_addCommandMenu = new MxRibbonContextMenu();
				_addCommandMenu.ImageList = _designTarget.DesignerImageList;
				if (_designTarget.CanAddCommand(CommandType.SmallButton))
				{
					ButtonCommand buttonCommand = new ButtonCommand("ButtonCommandLarge", "Button Command (Large)");
					buttonCommand.ImageIndex = 4;
					_addCommandMenu.Commands.Add(buttonCommand);
				}
				if (_designTarget.CanAddCommand(CommandType.SmallButton))
				{
					ButtonCommand smallButtonEntry = new ButtonCommand("ButtonCommandSmall", "Button Command (Small)");
					smallButtonEntry.ImageIndex = 4;
					_addCommandMenu.Commands.Add(smallButtonEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.DropDownButton))
				{
					ButtonCommand largeDropDownEntry = new ButtonCommand("DropDownCommandLarge", "DropDown Command (Large)");
					largeDropDownEntry.ImageIndex = 6;
					_addCommandMenu.Commands.Add(largeDropDownEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.DropDownButton))
				{
					ButtonCommand smallDropDownEntry = new ButtonCommand("DropDownCommandSmall", "DropDown Command (Small)");
					smallDropDownEntry.ImageIndex = 6;
					_addCommandMenu.Commands.Add(smallDropDownEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ButtonGroup))
				{
					ButtonCommand buttonGroupEntry = new ButtonCommand("ButtonGroup", "ButtonGroup");
					buttonGroupEntry.ImageIndex = 6;
					_addCommandMenu.Commands.Add(buttonGroupEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Box))
				{
					ButtonCommand commandBoxEntry = new ButtonCommand("CommandBox", "Command Box");
					commandBoxEntry.ImageIndex = 5;
					_addCommandMenu.Commands.Add(commandBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Separator))
				{
					ButtonCommand separatorEntry = new ButtonCommand("Separator", "Separator");
					separatorEntry.ImageIndex = 0;
					_addCommandMenu.Commands.Add(separatorEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Label))
				{
					ButtonCommand labelEntry = new ButtonCommand("Label", "Label");
					labelEntry.ImageIndex = 11;
					_addCommandMenu.Commands.Add(labelEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand comboBoxEntry = new ButtonCommand("ComboBox", "ComboBox");
					comboBoxEntry.ImageIndex = 3;
					_addCommandMenu.Commands.Add(comboBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand checkBoxEntry = new ButtonCommand("CheckBox", "CheckBox");
					checkBoxEntry.ImageIndex = 2;
					_addCommandMenu.Commands.Add(checkBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand textBoxEntry = new ButtonCommand("TextBox", "TextBox");
					textBoxEntry.ImageIndex = 9;
					_addCommandMenu.Commands.Add(textBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand containerControlEntry = new ButtonCommand("ContainerControl", "ContainerControl");
					containerControlEntry.ImageIndex = 7;
					_addCommandMenu.Commands.Add(containerControlEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Gallery))
				{
					ButtonCommand galleryEntry = new ButtonCommand("Gallery", "Gallery");
					galleryEntry.ImageIndex = 10;
					_addCommandMenu.Commands.Add(galleryEntry);
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
				if (RibbonStringSwitchCache.s_groupAddCommandKeyCases == null)
				{
					RibbonStringSwitchCache.s_groupAddCommandKeyCases = new Dictionary<string, int>(13)
					{
						{
							"ButtonCommandLarge",
							0
						},
						{
							"ButtonCommandSmall",
							1
						},
						{
							"DropDownCommandLarge",
							2
						},
						{
							"DropDownCommandSmall",
							3
						},
						{
							"ComboBox",
							4
						},
						{
							"CheckBox",
							5
						},
						{
							"ContainerControl",
							6
						},
						{
							"Separator",
							7
						},
						{
							"TextBox",
							8
						},
						{
							"Gallery",
							9
						},
						{
							"Label",
							10
						},
						{
							"ButtonGroup",
							11
						},
						{
							"CommandBox",
							12
						}
					};
				}
				if (RibbonStringSwitchCache.s_groupAddCommandKeyCases.TryGetValue(key, out var value))
				{
					switch (value)
					{
					case 0:
						commandBase = new ButtonCommand();
						break;
					case 1:
						commandBase = new ButtonCommand();
						((ButtonCommand)commandBase).SizeStyle = CommandSizeStyle.Small;
						break;
					case 2:
						commandBase = new DropDownCommand();
						break;
					case 3:
						commandBase = new DropDownCommand();
						((ButtonCommand)commandBase).SizeStyle = CommandSizeStyle.Small;
						break;
					case 4:
						commandBase = new ComboBoxCommand();
						break;
					case 5:
						commandBase = new CheckBoxCommand();
						break;
					case 6:
						commandBase = new ContainerControlCommand();
						break;
					case 7:
						commandBase = new SeparatorCommand();
						break;
					case 8:
						commandBase = new TextBoxCommand();
						break;
					case 9:
						commandBase = new Gallery();
						break;
					case 10:
						commandBase = new LabelCommand();
						break;
					case 11:
						commandBase = new ButtonGroup();
						break;
					case 12:
						commandBase = new CommandBox();
						break;
					}
				}
			}
			_designTarget.AddNewCommand(commandBase, editItem: false);
			string clickedKey;
			if ((clickedKey = eventArgs.Command.Key) != null)
			{
				if (!(clickedKey == "ButtonCommandLarge") && !(clickedKey == "ButtonCommandSmall") && !(clickedKey == "DropDownCommandLarge") && !(clickedKey == "DropDownCommandSmall"))
				{
					if (clickedKey == "Label")
					{
						((LabelCommand)commandBase).Text = commandBase.Key;
					}
				}
				else
				{
					((ButtonCommand)commandBase).Text = commandBase.Key;
				}
			}
			_commandBaseRendererRibbon.DesignerSelectCommand(commandBase);
			if (commandBase is DropDownCommand && !(commandBase is Gallery))
			{
				((DropDownCommand)commandBase).DropDown();
			}
		}
	}
}
