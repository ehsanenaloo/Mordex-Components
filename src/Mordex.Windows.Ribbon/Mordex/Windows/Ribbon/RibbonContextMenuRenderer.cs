using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonContextMenuRenderer : CommandBaseRenderer
	{
		private Rectangle _textAreaBounds;

		private Rectangle _dropDownArrowBounds;

		private ICommandDesignTarget _designTarget;

		private bool _isArrowHot;

		private MxRibbonContextMenu _contextMenu;

		private bool _isTextAreaPressed;

		private bool _drawImageArea;

		public override CommandType CommandType => CommandType.ContainerControl;

		public RibbonContextMenuRenderer(ICommandRenderHost itemOwner, ICommandDesignTarget owner, bool drawImageArea)
			: base(null, itemOwner, null)
		{
			_designTarget = owner;
			_drawImageArea = drawImageArea;
		}

		public override void Dispose()
		{
			if (_contextMenu != null)
			{
				_contextMenu.CommandClick -= OnAddCommandMenuClick;
				_contextMenu.Dispose();
				_contextMenu = null;
			}
			base.Dispose();
		}

		public void SetArrowHot(bool ribbonContextMenuRendererBoolean)
		{
			if (_isArrowHot != ribbonContextMenuRendererBoolean)
			{
				_isArrowHot = ribbonContextMenuRendererBoolean;
				Invalidate();
			}
		}

		public MxRibbonContextMenu GetOrCreateContextMenu()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new MxRibbonContextMenu();
				_contextMenu.ImageList = _designTarget.DesignerImageList;
				if (_designTarget.CanAddCommand(CommandType.SmallButton))
				{
					ButtonCommand buttonCommand = new ButtonCommand("ButtonCommand", "Menu Item (CommandButton)");
					buttonCommand.ImageIndex = 4;
					_contextMenu.Commands.Add(buttonCommand);
				}
				if (_designTarget.CanAddCommand(CommandType.DropDownButton))
				{
					ButtonCommand dropDownEntry = new ButtonCommand("DropDownCommand", "DropDown Command");
					dropDownEntry.ImageIndex = 6;
					_contextMenu.Commands.Add(dropDownEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Separator))
				{
					ButtonCommand separatorEntry = new ButtonCommand("Separator", "Separator");
					separatorEntry.ImageIndex = 0;
					_contextMenu.Commands.Add(separatorEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Label))
				{
					ButtonCommand labelEntry = new ButtonCommand("Label", "Label");
					labelEntry.ImageIndex = 11;
					_contextMenu.Commands.Add(labelEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand comboBoxEntry = new ButtonCommand("ComboBox", "ComboBox");
					comboBoxEntry.ImageIndex = 3;
					_contextMenu.Commands.Add(comboBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand checkBoxEntry = new ButtonCommand("CheckBox", "CheckBox");
					checkBoxEntry.ImageIndex = 2;
					_contextMenu.Commands.Add(checkBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand textBoxEntry = new ButtonCommand("TextBox", "TextBox");
					textBoxEntry.ImageIndex = 9;
					_contextMenu.Commands.Add(textBoxEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.ContainerControl))
				{
					ButtonCommand containerControlEntry = new ButtonCommand("ContainerControl", "Container Control");
					containerControlEntry.ImageIndex = 7;
					_contextMenu.Commands.Add(containerControlEntry);
				}
				if (_designTarget.CanAddCommand(CommandType.Gallery))
				{
					ButtonCommand galleryEntry = new ButtonCommand("Gallery", "Gallery");
					galleryEntry.ImageIndex = 10;
					_contextMenu.Commands.Add(galleryEntry);
				}
				_contextMenu.CommandClick += OnAddCommandMenuClick;
			}
			return _contextMenu;
		}

		private void OnAddCommandMenuClick(object sender, CommandEventArgs eventArgs)
		{
			CommandBase commandBase = null;
			string key;
			if ((key = eventArgs.Command.Key) != null)
			{
				if (RibbonStringSwitchCache.s_addCommandKeyCases == null)
				{
					RibbonStringSwitchCache.s_addCommandKeyCases = new Dictionary<string, int>(9)
					{
						{
							"DropDownCommand",
							0
						},
						{
							"ButtonCommand",
							1
						},
						{
							"ComboBox",
							2
						},
						{
							"CheckBox",
							3
						},
						{
							"ContainerControl",
							4
						},
						{
							"Separator",
							5
						},
						{
							"TextBox",
							6
						},
						{
							"Gallery",
							7
						},
						{
							"Label",
							8
						}
					};
				}
				if (RibbonStringSwitchCache.s_addCommandKeyCases.TryGetValue(key, out var value))
				{
					switch (value)
					{
					case 0:
						commandBase = new DropDownCommand();
						break;
					case 1:
						commandBase = new ButtonCommand();
						break;
					case 2:
						commandBase = new ComboBoxCommand();
						break;
					case 3:
						commandBase = new CheckBoxCommand();
						break;
					case 4:
						commandBase = new ContainerControlCommand();
						break;
					case 5:
						commandBase = new SeparatorCommand();
						break;
					case 6:
						commandBase = new TextBoxCommand();
						break;
					case 7:
						commandBase = new Gallery();
						break;
					case 8:
						commandBase = new LabelCommand();
						break;
					}
				}
			}
			if (commandBase != null)
			{
				AddCommandToOwner(commandBase, false);
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				int imageAreaWidth = GetRenderHost().SmallImageSize.Width + 8;
				_hasImageContent = false;
				_hasTextContent = false;
				int inputWidth = 100;
				_fullItemWidth = imageAreaWidth + inputWidth + 20;
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
				_textAreaBounds = new Rectangle(x, top + 2, 100, itemHeight - 4);
				_dropDownArrowBounds = new Rectangle(_textAreaBounds.Right - 13, _textAreaBounds.Y + 2, 11, _textAreaBounds.Height - 4);
				_bounds = new Rectangle(left, top, width, itemHeight);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			if (_drawImageArea)
			{
				Brush brush = GdiCache.Brush(colorScheme._menuBackground);
				graphics.FillRectangle(brush, _imageBounds);
			}
			ResolvedTheme theme = colorScheme.Theme;
			Pen pen = null;
			if (Hot)
			{
				pen = new Pen(theme.Color(ThemeRole.Text));
				pen.DashStyle = DashStyle.Dot;
			}
			else
			{
				pen = new Pen(theme.Color(ThemeRole.Border));
			}
			Brush hintBrush = theme.Brush(ThemeRole.TextMuted);
			RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, _textAreaBounds);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString("Type Here", Control.DefaultFont, hintBrush, _textAreaBounds, stringFormat);
			stringFormat.Dispose();
			pen.Dispose();
			if (_drawImageArea)
			{
				Pen separatorPen = GdiCache.Pen(colorScheme._menuSeparator);
				graphics.DrawLine(separatorPen, _imageBounds.Right, _imageBounds.Y, _imageBounds.Right, _imageBounds.Bottom);
			}
			if (Hot)
			{
				DrawArrowButton(graphics, colorScheme);
			}
		}

		private void DrawArrowButton(Graphics graphics, RibbonColorScheme colorScheme)
		{
			if (_isArrowHot)
			{
				RibbonChromePainter.DrawHotButtonBackground(graphics, _dropDownArrowBounds, colorScheme, true);
			}
			Rectangle rectangle = _dropDownArrowBounds;
			rectangle.Inflate(-1, -1);
			ThemedRibbonPainter.Arrow(graphics, colorScheme.Theme, rectangle, rectangle.Width + 3, 1, true, true);
		}

		public void AddCommandToOwner(CommandBase command, bool editItem)
		{
			if (_designTarget != null)
			{
				_designTarget.AddNewCommand(command, editItem);
			}
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (_isTextAreaPressed && _textAreaBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				AddCommandToOwner(new DropDownCommand(), true);
			}
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			SetArrowHot(_dropDownArrowBounds.Contains(eventArgs.X, eventArgs.Y));
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				if (_dropDownArrowBounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_isTextAreaPressed = false;
					Point pos = _designTarget.PointToScreen(new Point(_textAreaBounds.Left, _textAreaBounds.Bottom));
					GetOrCreateContextMenu().Show(pos);
				}
				else
				{
					_isTextAreaPressed = true;
				}
			}
		}
	}
}
