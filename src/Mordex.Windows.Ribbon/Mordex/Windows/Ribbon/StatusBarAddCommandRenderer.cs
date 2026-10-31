using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class StatusBarAddCommandRenderer : CommandBaseRenderer
	{
		private MxRibbonContextMenu _addCommandMenu;

		private MxRibbonStatusBar _statusBar;

		private ICommandDesignTarget _designTarget;

		private int _dropArrowWidth = 12;

		private Rectangle _dropArrowBounds;

		private Rectangle _dropArrowFrameBounds;

		public override CommandType CommandType => CommandType.SmallButton;

		internal StatusBarAddCommandRenderer(MxRibbonStatusBar statusBar, ICommandDesignTarget owner)
			: base(statusBar)
		{
			_statusBar = statusBar;
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
			_dropArrowBounds = new Rectangle(_bounds.Right - _dropArrowWidth, _bounds.Top, _dropArrowWidth, _bounds.Height);
			_dropArrowFrameBounds = new Rectangle(_bounds.Right - _dropArrowWidth - 1, _textBounds.Y, _dropArrowWidth + 1, _textBounds.Height);
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
			ThemedRibbonPainter.Arrow(graphics, colorScheme.Theme, _dropArrowBounds, _dropArrowBounds.Width, 1, true, true);
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				Point pos = _designTarget.PointToScreen(new Point(_bounds.Left, _bounds.Bottom));
				Pushed = true;
				_statusBar._designerItemPressed = true;
				GetOrCreateAddCommandMenu()._upwardOffset = _bounds.Height;
				GetOrCreateAddCommandMenu().Show(pos);
				_statusBar._designerItemPressed = false;
				Pushed = false;
			}
		}

		public MxRibbonContextMenu GetOrCreateAddCommandMenu()
		{
			if (_addCommandMenu == null)
			{
				_addCommandMenu = new MxRibbonContextMenu();
				_addCommandMenu.ImageList = _designTarget.DesignerImageList;
				if (_designTarget.CanAddCommand(CommandType.StatusBarPanel))
				{
					ButtonCommand buttonCommand = new ButtonCommand("StatusBarPanel", "StatusBar Panel");
					buttonCommand.ImageIndex = 4;
					_addCommandMenu.Commands.Add(buttonCommand);
				}
				if (_designTarget.CanAddCommand(CommandType.SmallButton))
				{
					ButtonCommand buttonItem = new ButtonCommand("ButtonCommand", "Button Command");
					buttonItem.ImageIndex = 4;
					_addCommandMenu.Commands.Add(buttonItem);
				}
				if (_designTarget.CanAddCommand(CommandType.DropDownButton))
				{
					ButtonCommand dropDownItem = new ButtonCommand("DropDownCommand", "DropDown Command");
					dropDownItem.ImageIndex = 6;
					_addCommandMenu.Commands.Add(dropDownItem);
				}
				if (_designTarget.CanAddCommand(CommandType.ButtonGroup))
				{
					ButtonCommand buttonGroupItem = new ButtonCommand("ButtonGroup", "Button Group");
					buttonGroupItem.ImageIndex = 1;
					_addCommandMenu.Commands.Add(buttonGroupItem);
				}
				if (_designTarget.CanAddCommand(CommandType.Separator))
				{
					ButtonCommand separatorItem = new ButtonCommand("Separator", "Separator");
					separatorItem.ImageIndex = 0;
					_addCommandMenu.Commands.Add(separatorItem);
				}
				if (_designTarget.CanAddCommand(CommandType.Label))
				{
					ButtonCommand labelItem = new ButtonCommand("Label", "Label");
					labelItem.ImageIndex = 11;
					_addCommandMenu.Commands.Add(labelItem);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand comboBoxItem = new ButtonCommand("ComboBox", "ComboBox");
					comboBoxItem.ImageIndex = 3;
					_addCommandMenu.Commands.Add(comboBoxItem);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand checkBoxItem = new ButtonCommand("CheckBox", "CheckBox");
					checkBoxItem.ImageIndex = 2;
					_addCommandMenu.Commands.Add(checkBoxItem);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand textBoxItem = new ButtonCommand("TextBox", "TextBox");
					textBoxItem.ImageIndex = 9;
					_addCommandMenu.Commands.Add(textBoxItem);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand containerControlItem = new ButtonCommand("ContainerControl", "Container Control");
					containerControlItem.ImageIndex = 7;
					_addCommandMenu.Commands.Add(containerControlItem);
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
				if (RibbonStringSwitchCache.s_statusBarAddCommandKeyCases == null)
				{
					RibbonStringSwitchCache.s_statusBarAddCommandKeyCases = new Dictionary<string, int>(10)
					{
						{
							"StatusBarPanel",
							0
						},
						{
							"ButtonCommand",
							1
						},
						{
							"DropDownCommand",
							2
						},
						{
							"ComboBox",
							3
						},
						{
							"CheckBox",
							4
						},
						{
							"ContainerControl",
							5
						},
						{
							"Separator",
							6
						},
						{
							"TextBox",
							7
						},
						{
							"Label",
							8
						},
						{
							"ButtonGroup",
							9
						}
					};
				}
				if (RibbonStringSwitchCache.s_statusBarAddCommandKeyCases.TryGetValue(key, out var value))
				{
					switch (value)
					{
					case 0:
						commandBase = new StatusBarPanel();
						((ButtonCommand)commandBase).SizeStyle = CommandSizeStyle.Small;
						break;
					case 1:
						commandBase = new ButtonCommand();
						((ButtonCommand)commandBase).SizeStyle = CommandSizeStyle.Small;
						break;
					case 2:
						commandBase = new DropDownCommand();
						((ButtonCommand)commandBase).SizeStyle = CommandSizeStyle.Small;
						break;
					case 3:
						commandBase = new ComboBoxCommand();
						break;
					case 4:
						commandBase = new CheckBoxCommand();
						break;
					case 5:
						commandBase = new ContainerControlCommand();
						break;
					case 6:
						commandBase = new SeparatorCommand();
						break;
					case 7:
						commandBase = new TextBoxCommand();
						break;
					case 8:
						commandBase = new LabelCommand();
						break;
					case 9:
						commandBase = new ButtonGroup();
						break;
					}
				}
			}
			_designTarget.AddNewCommand(commandBase, editItem: false);
			string clickedKey;
			if ((clickedKey = eventArgs.Command.Key) != null)
			{
				if (!(clickedKey == "StatusBarPanel") && !(clickedKey == "ButtonCommand") && !(clickedKey == "DropDownCommand"))
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
			_statusBar.SelectInDesigner(commandBase);
			if (commandBase is DropDownCommand)
			{
				((DropDownCommand)commandBase).DropDown();
			}
		}
	}
}
