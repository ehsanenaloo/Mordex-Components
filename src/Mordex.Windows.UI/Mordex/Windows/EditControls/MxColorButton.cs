using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Controls
{
	public class MxColorButton : MxButton
	{
		private MxColorPicker _colorPicker;

		private bool _hasImage = true;

		private string _emptyColorText = "(none)";

		private Color _selectedColor = Color.Empty;

		private bool _useColorName = true;

		private Color _imageReplaceableColor = Color.Empty;

		private static readonly object _automaticButtonClickEvent = new object();

		private static readonly object _moreColorsButtonClickEvent = new object();

		private static readonly object _selectedColorChangedEvent = new object();

private static readonly Size DefaultImageSize = new Size(16, 16);

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new ButtonStyle ButtonStyle
		{
			get
			{
				return base.ButtonStyle;
			}
			set
			{
				base.ButtonStyle = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new MxContextMenu DropDownContextMenu
		{
			get
			{
				return base.DropDownContextMenu;
			}
			set
			{
				base.DropDownContextMenu = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Reprsents the UIColorPicker associated with the button")]
		public MxColorPicker ColorPicker => _colorPicker;

		[DefaultValue(true)]
		[Description("Indicates whether the control should display an image.")]
		public bool HasImage
		{
			get
			{
				return _hasImage;
			}
			set
			{
				if (_hasImage != value)
				{
					_hasImage = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[DefaultValue("(none)")]
		[Description("Represents the Text of the button when the selected color is empty")]
		public string EmptyColorText
		{
			get
			{
				return _emptyColorText;
			}
			set
			{
				if (_emptyColorText != value)
				{
					_emptyColorText = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Description("Represents the selected color")]
		public Color SelectedColor
		{
			get
			{
				return _selectedColor;
			}
			set
			{
				if (_selectedColor != value)
				{
					_selectedColor = value;
					OnSelectedColorChanged(EventArgs.Empty);
					_colorPicker.SelectedColor = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Description("Indicates whether the text of the Button will be taken from the name of the Color")]
		[DefaultValue(true)]
		public bool UseColorName
		{
			get
			{
				return _useColorName;
			}
			set
			{
				if (_useColorName != value)
				{
					_useColorName = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the Color in the image that will be replaced for the selected color")]
		public Color ImageReplaceableColor
		{
			get
			{
				return _imageReplaceableColor;
			}
			set
			{
				if (_imageReplaceableColor != value)
				{
					_imageReplaceableColor = value;
					InvalidateLayoutAndPaint();
				}
			}
		}

		protected override string ButtonText
		{
			get
			{
				if (_useColorName)
				{
					if (_selectedColor.IsEmpty)
					{
						return _emptyColorText;
					}
					ColorEntry uiColorEntry = _colorPicker.ColorEntries[_selectedColor];
					if (uiColorEntry != null)
					{
						return uiColorEntry.ColorName;
					}
				}
				return Text;
			}
		}

		[Description("Event fired when the Automatic button is clicked")]
		public event EventHandler AutomaticButtonClick
		{
			add
			{
				base.Events.AddHandler(_automaticButtonClickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_automaticButtonClickEvent, value);
			}
		}

		[Description("Event fired when the MoreColors button is clicked.")]
		public event EventHandler MoreColorsButtonClick
		{
			add
			{
				base.Events.AddHandler(_moreColorsButtonClickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_moreColorsButtonClickEvent, value);
			}
		}

		[Description("Event fired when the SelectedColor property has changed.")]
		public event EventHandler SelectedColorChanged
		{
			add
			{
				base.Events.AddHandler(_selectedColorChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedColorChangedEvent, value);
			}
		}

		public MxColorButton()
		{
			ButtonStyle = ButtonStyle.DropDown;
			_colorPicker = new MxColorPicker();
			_colorPicker.BorderStyle = Mordex.Windows.UI.BorderStyle.None;
			_colorPicker.SelectedColorChanged += OnColorPickerSelectedColorChanged;
			_colorPicker.MoreColorsButtonClick += OnColorPickerMoreColorsButtonClick;
			_colorPicker.AutomaticButtonClick += OnColorPickerAutomaticButtonClick;
			_colorPicker.MouseUp += OnColorPickerMouseUp;
			MxContextMenu uiContextMenu = new MxContextMenu();
			MxCommand command = new MxCommand("ColorPicker")
			{
				CommandType = CommandType.ControlPopup,
				Control = _colorPicker
			};
			uiContextMenu.Commands.Add(command);
			base.DropDownContextMenu = uiContextMenu;
		}

		protected override void Dispose(bool disposing)
		{
			if (_colorPicker != null)
			{
				_colorPicker.SelectedColorChanged -= OnColorPickerSelectedColorChanged;
				_colorPicker.MoreColorsButtonClick -= OnColorPickerMoreColorsButtonClick;
				_colorPicker.AutomaticButtonClick -= OnColorPickerAutomaticButtonClick;
				_colorPicker.MouseUp -= OnColorPickerMouseUp;
				_colorPicker.Dispose();
				_colorPicker = null;
			}
			base.DropDownContextMenu = null;
			base.Dispose(disposing);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedColor()
		{
			return !_selectedColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedColor()
		{
			SelectedColor = Color.Empty;
		}

		private void OnColorPickerMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (DropDownContextMenu != null)
			{
				ColorEntry colorEntryAt = _colorPicker.GetColorEntryAt(eventArgs.X, eventArgs.Y);
				if (colorEntryAt != null)
				{
					DropDownContextMenu.Close();
				}
			}
		}

		private void OnColorPickerSelectedColorChanged(object sender, EventArgs eventArgs)
		{
			SelectedColor = _colorPicker.SelectedColor;
			if (DropDownContextMenu != null)
			{
				DropDownContextMenu.Close();
			}
		}

		private void OnColorPickerMoreColorsButtonClick(object sender, EventArgs eventArgs)
		{
			if (DropDownContextMenu != null)
			{
				DropDownContextMenu.Close();
			}
			OnMoreColorsButtonClick(eventArgs);
		}

		private void OnColorPickerAutomaticButtonClick(object sender, EventArgs eventArgs)
		{
			if (DropDownContextMenu != null)
			{
				DropDownContextMenu.Close();
			}
			OnAutomaticButtonClick(eventArgs);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeImageReplaceableColor()
		{
			return !_imageReplaceableColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetImageReplaceableColor()
		{
			ImageReplaceableColor = Color.Empty;
		}

		protected override Size GetImageSize()
		{
			if (!_hasImage)
			{
				return Size.Empty;
			}
			if (!base.ImageSize.IsEmpty)
			{
				return base.ImageSize;
			}
			Size result = Size.Empty;
			if (base.Icon != null)
			{
				result = base.Icon.Size;
			}
			else if (base.Image != null)
			{
				result = base.Image.Size;
			}
			else if (base.ImageList != null)
			{
				int imageIndex = GetImageIndex(false, false);
				if (imageIndex >= 0)
				{
					result = base.ImageList.ImageSize;
				}
			}
			if (result.IsEmpty)
			{
				return DefaultImageSize;
			}
			return result;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (DropDownContextMenu != null)
			{
			}
			base.OnMouseDown(e);
		}

		protected override void DrawImage(Graphics g, FormatStyle style)
		{
			if (!(_imageBounds != Rectangle.Empty))
			{
				return;
			}
			Image image = GetCurrentImage(IsHot(), IsPressed());
			int imageIndex = GetImageIndex(IsHot(), IsPressed());
			if (image != null || base.Icon != null)
			{
				ImageAttributes imageAttributes = null;
				if (!ImageReplaceableColor.IsEmpty)
				{
					imageAttributes = new ImageAttributes();
					ColorMap colorMap = new ColorMap();
					colorMap.NewColor = SelectedColor;
					colorMap.OldColor = ImageReplaceableColor;
					imageAttributes.SetRemapTable(new ColorMap[1] { colorMap }, ColorAdjustType.Bitmap);
				}
				RenderingHelper.DrawStateImage(g, base.Icon, image, imageIndex, base.ImageList, _imageBounds, base.Enabled, false, false, style.BackColor, true, imageAttributes);
				if (imageAttributes != null)
				{
					imageAttributes.Dispose();
				}
			}
			else
			{
				using (Brush brush = new SolidBrush(_selectedColor))
				{
				g.FillRectangle(brush, _imageBounds);
				}
				Pen outline = MxThemeManager.ResolveDefault(DeviceDpi).Pen(ThemeRole.BorderStrong);
				RenderingHelper.DrawRectangleOutline(g, outline, _imageBounds);
			}
		}

		protected virtual void OnAutomaticButtonClick(EventArgs e)
		{
			if (base.Events[_automaticButtonClickEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnMoreColorsButtonClick(EventArgs e)
		{
			if (base.Events[_moreColorsButtonClickEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnSelectedColorChanged(EventArgs e)
		{
			if (base.Events[_selectedColorChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
