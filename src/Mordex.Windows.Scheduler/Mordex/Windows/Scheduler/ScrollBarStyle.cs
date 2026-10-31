using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class ScrollBarStyle : IMordexXmlLayoutsSupport
	{

		private ScrollableBase _owner;


		private Appearance _buttonAppearance = Appearance.Light3D;

		private Color _controlColor = Color.Empty;

		private SolidBrush _solidBrush;

		internal Color _controlTextColor = Color.Empty;

		private Brush _controlTextBrush;

		private Pen _controlTextPen;

		private Color _scrollBarColor = Color.Empty;

		private Brush _scrollBarBrush;

		private Color _lightColor = SystemColors.ControlLight;

		private Pen _lightPen;

		private Color _lightLightColor = SystemColors.ControlLightLight;

		private Pen _flatBorderPen;

		private Pen _lightLightPen;

		private Brush _lightLightBrush;

		private Color _darkColor = SystemColors.ControlDark;

		private Pen _darkPen;

		private Brush _darkBrush;

		private Color _darkDarkColor = SystemColors.ControlDarkDark;

		private Pen _darkDarkPen;

		private Color _hoverBlendColor = Color.Empty;

		private float _hoverBlend = 0.667f;

		private Color _hoverBaseColor = Color.Empty;

		private float _hoverSelectedBlend = 0.5f;

		private SolidBrush _hoverBrush;

		private SolidBrush _hoverSelectedBrush;

		private Pen _hoverBasePen;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Determines the appearance of the buttons of the scroll bar.")]
		[DefaultValue(Appearance.Light3D)]
		public Appearance ButtonAppearance
		{
			get
			{
				return _buttonAppearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Appearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _buttonAppearance)
				{
					_buttonAppearance = value;
					UpdateScrollBarStyle();
				}
			}
		}

		[Description("The background color of the buttons of the ScrollBar.")]
		public Color ControlColor
		{
			get
			{
				if (_controlColor.IsEmpty)
				{
					return SystemColors.Control;
				}
				return _controlColor;
			}
			set
			{
				if (value != _controlColor)
				{
					_controlColor = value;
					UpdateSolidBrush();
				}
			}
		}

		[Description("The foreground color of the buttons of the ScrollBar.")]
		public Color ControlTextColor
		{
			get
			{
				if (_controlTextColor.IsEmpty)
				{
					return SystemColors.ControlText;
				}
				return _controlTextColor;
			}
			set
			{
				if (_controlTextColor != value)
				{
					_controlTextColor = value;
					DisposeResources();
				}
			}
		}

		[Description("The background color of the ScrollBar.")]
		public Color ScrollBarColor
		{
			get
			{
				if (_scrollBarColor.IsEmpty)
				{
					return SystemColors.ScrollBar;
				}
				return _scrollBarColor;
			}
			set
			{
				if (_scrollBarColor != value)
				{
					_scrollBarColor = value;
					UpdateScrollBarStyleBrush();
				}
			}
		}

		public Color HoverBlendColor
		{
			get
			{
				if (_hoverBlendColor.IsEmpty)
				{
					return SystemColors.Window;
				}
				return _hoverBlendColor;
			}
			set
			{
				if (_hoverBlendColor != value)
				{
					_hoverBlendColor = value;
					DisposeHoverResources();
				}
			}
		}

		[DefaultValue(0.667f)]
		[Description("Represents the percentage of the HoverBlendColor to blend whit the HoverBaseColor.")]
		public float HoverBlend
		{
			get
			{
				return _hoverBlend;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException("value", "HoverBlend property value must be between 0 and 1");
				}
				if (_hoverBlend != value)
				{
					_hoverBlend = value;
					DisposeHoverResources();
				}
			}
		}

		[Description("Represents the color that will be blended with the HoverBlendColor when a flat apperance is used.")]
		public Color HoverBaseColor
		{
			get
			{
				if (_hoverBaseColor.IsEmpty)
				{
					return SystemColors.Highlight;
				}
				return _hoverBaseColor;
			}
			set
			{
				if (_hoverBaseColor != value)
				{
					_hoverBaseColor = value;
					DisposeHoverResources();
				}
			}
		}

		[DefaultValue(0.5f)]
		[Description("Represents the percentage of the HoverBlendColor to blend with the HoverBaseColor when the mouse is captured by the button.")]
		public float HoverSelectedBlend
		{
			get
			{
				return _hoverSelectedBlend;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException("value", "HoverSelectedBlend property value must be between 0 and 1");
				}
				if (_hoverSelectedBlend != value)
				{
					_hoverSelectedBlend = value;
					DisposeHoverResources();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_buttonAppearance = (Appearance)LayoutReader.GetPropertyValue(info, "ButtonAppearance", typeof(Appearance), Appearance.Light3D);
			_controlColor = (Color)LayoutReader.GetPropertyValue(info, "ControlColor", typeof(Color), Color.Empty);
			_controlTextColor = (Color)LayoutReader.GetPropertyValue(info, "ControlTextColor", typeof(Color), Color.Empty);
			_hoverBaseColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBaseColor", typeof(Color), Color.Empty);
			_hoverBlendColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBlendColor", typeof(Color), Color.Empty);
			_scrollBarColor = (Color)LayoutReader.GetPropertyValue(info, "ScrollBarColor", typeof(Color), Color.Empty);
			_hoverBlend = (float)LayoutReader.GetPropertyValue(info, "HoverBlend", typeof(float), 0.667f);
			_hoverSelectedBlend = (float)LayoutReader.GetPropertyValue(info, "HoverSelectedBlend", typeof(float), 0.5f);
			UpdateSolidBrush();
			DisposeResources();
			DisposeHoverResources();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_buttonAppearance != Appearance.Light3D)
			{
				info.AddValue("ButtonAppearance", _buttonAppearance);
			}
			if (!_controlColor.IsEmpty)
			{
				info.AddValue("ControlColor", _controlColor);
			}
			if (!_controlTextColor.IsEmpty)
			{
				info.AddValue("ControlTextColor", _controlTextColor);
			}
			if (!_hoverBaseColor.IsEmpty)
			{
				info.AddValue("HoverBaseColor", _hoverBaseColor);
			}
			if (!_hoverBlendColor.IsEmpty)
			{
				info.AddValue("HoverBlendColor", _hoverBlendColor);
			}
			if (!_scrollBarColor.IsEmpty)
			{
				info.AddValue("ScrollBarColor", _scrollBarColor);
			}
			if (_hoverBlend != 0.667f)
			{
				info.AddValue("HoverBlend", _hoverBlend);
			}
			if (_hoverSelectedBlend != 0.5f)
			{
				info.AddValue("HoverSelectedBlend", _hoverSelectedBlend);
			}
		}

		internal ScrollBarStyle(ScrollableBase owner)
		{
			_owner = owner;
		}

		internal ScrollBarStyle()
		{
		}

		private void UpdateScrollBarStyle()
		{
			if (_owner != null)
			{
				_owner.RefreshScrollBarColors();
			}
		}


		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlColor()
		{
			return !_controlColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlColor()
		{
			ControlColor = Color.Empty;
		}

		private void UpdateSolidBrush()
		{
			if (_solidBrush != null)
			{
				_solidBrush.Dispose();
				_solidBrush = null;
			}
			UpdateDarkColor(ControlPaint.Dark(ControlColor, 0f));
			UpdateDarkDarkColor(ControlPaint.DarkDark(ControlColor));
			SetLightColor(ControlPaint.Light(ControlColor, 0f));
			UpdateScrollBarStyleColor(ControlPaint.LightLight(ControlColor));
			UpdateScrollBarStyleBrush();
			UpdateScrollBarStyle();
		}

		internal SolidBrush GetControlBrush()
		{
			if (_solidBrush == null)
			{
				_solidBrush = new SolidBrush(ControlColor);
			}
			return _solidBrush;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlTextColor()
		{
			return !_controlTextColor.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlTextColor()
		{
			ControlTextColor = Color.Empty;
		}

		private void DisposeResources()
		{
			if (_controlTextBrush != null)
			{
				_controlTextBrush.Dispose();
				_controlTextBrush = null;
			}
			if (_controlTextPen != null)
			{
				_controlTextPen.Dispose();
				_controlTextPen = null;
			}
			UpdateScrollBarStyle();
		}

		internal Pen GetControlTextPen()
		{
			if (_controlTextPen == null)
			{
				_controlTextPen = new Pen(ControlTextColor);
			}
			return _controlTextPen;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScrollBarColor()
		{
			return !_scrollBarColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScrollBarColor()
		{
			ScrollBarColor = Color.Empty;
		}

		internal Brush GetScrollBarBrush()
		{
			if (_scrollBarBrush == null)
			{
				if (ScrollBarColor.ToArgb() == ControlColor.ToArgb() || ScrollBarColor.Equals(SystemColors.ScrollBar))
				{
					Bitmap bitmap = new Bitmap(8, 8);
					for (int i = 0; i < bitmap.Width; i += 2)
					{
						for (int j = 0; j < bitmap.Height; j++)
						{
							try
							{
								if (j % 2 == 0)
								{
									bitmap.SetPixel(i, j, ControlColor);
								}
								else
								{
									bitmap.SetPixel(i, j, _lightLightColor);
								}
							}
							catch
							{
							}
						}
					}
					for (int i = 1; i < bitmap.Width; i += 2)
					{
						for (int j = 0; j < bitmap.Height; j++)
						{
							try
							{
								if (j % 2 == 0)
								{
									bitmap.SetPixel(i, j, _lightLightColor);
								}
								else
								{
									bitmap.SetPixel(i, j, ControlColor);
								}
							}
							catch
							{
							}
						}
					}
					_scrollBarBrush = new TextureBrush(bitmap);
					bitmap.Dispose();
					bitmap = null;
				}
				else
				{
					_scrollBarBrush = new SolidBrush(ScrollBarColor);
				}
			}
			return _scrollBarBrush;
		}

		private void UpdateScrollBarStyleBrush()
		{
			if (_scrollBarBrush != null)
			{
				_scrollBarBrush.Dispose();
				_scrollBarBrush = null;
			}
			UpdateScrollBarStyle();
		}

		internal void SetLightColor(Color color)
		{
			if (_lightColor != color)
			{
				_lightColor = color;
				UpdateScrollBarStylePen();
			}
		}

		internal Pen GetLightPen()
		{
			if (_lightPen == null)
			{
				_lightPen = new Pen(_lightColor);
			}
			return _lightPen;
		}

		private void UpdateScrollBarStylePen()
		{
			if (_lightPen != null)
			{
				_lightPen.Dispose();
				_lightPen = null;
			}
			UpdateScrollBarStyle();
		}

		internal Color GetScrollBarStyleColor()
		{
			return _lightLightColor;
		}

		internal void UpdateScrollBarStyleColor(Color scrollBarStyleColor)
		{
			if (_lightLightColor != scrollBarStyleColor)
			{
				_lightLightColor = scrollBarStyleColor;
				DisposeLightLightResources();
			}
		}

		internal Pen GetFlatBorderPen()
		{
			if (_flatBorderPen == null)
			{
				if (_owner != null)
				{
					_flatBorderPen = new Pen(_owner.FlatBorderColor);
				}
				else
				{
					_flatBorderPen = new Pen(ControlPaint.Dark(ControlColor, 0f));
				}
			}
			return _flatBorderPen;
		}

		internal Pen GetLightLightPen()
		{
			if (_lightLightPen == null)
			{
				_lightLightPen = new Pen(_lightLightColor);
			}
			return _lightLightPen;
		}

		private void DisposeLightLightResources()
		{
			if (_lightLightPen != null)
			{
				_lightLightPen.Dispose();
				_lightLightPen = null;
			}
			if (_lightLightBrush != null)
			{
				_lightLightBrush.Dispose();
				_lightLightBrush = null;
			}
			UpdateScrollBarStyleBrush();
			UpdateScrollBarStyle();
		}

		internal Color GetDarkColor()
		{
			return _darkColor;
		}

		internal void UpdateDarkColor(Color darkColor)
		{
			if (_darkColor != darkColor)
			{
				_darkColor = darkColor;
				DisposeDarkPen();
			}
		}

		internal Pen GetDarkPen()
		{
			if (_darkPen == null)
			{
				_darkPen = new Pen(_darkColor);
			}
			return _darkPen;
		}

		private void DisposeDarkPen()
		{
			if (_darkPen != null)
			{
				_darkPen.Dispose();
				_darkPen = null;
			}
			UpdateScrollBarStyle();
		}

		internal void UpdateDarkDarkColor(Color darkDarkColor)
		{
			if (_darkDarkColor != darkDarkColor)
			{
				_darkDarkColor = darkDarkColor;
				DisposeDarkDarkPen();
			}
		}

		internal Pen GetDarkDarkPen()
		{
			if (_darkDarkPen == null)
			{
				_darkDarkPen = new Pen(_darkDarkColor);
			}
			return _darkDarkPen;
		}

		private void DisposeDarkDarkPen()
		{
			if (_darkDarkPen != null)
			{
				_darkDarkPen.Dispose();
				_darkDarkPen = null;
			}
			UpdateScrollBarStyleBrush();
			UpdateScrollBarStyle();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBlendColor()
		{
			return !_hoverBlendColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBlendColor()
		{
			HoverBlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBaseColor()
		{
			HoverBaseColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBaseColor()
		{
			return !_hoverBaseColor.IsEmpty;
		}

		internal SolidBrush GetHoverBrush()
		{
			if (_hoverBrush == null)
			{
				_hoverBrush = new SolidBrush(MixColors(HoverBlendColor, HoverBaseColor, _hoverBlend));
			}
			return _hoverBrush;
		}

		internal SolidBrush GetHoverSelectedBrush()
		{
			if (_hoverSelectedBrush == null)
			{
				_hoverSelectedBrush = new SolidBrush(SchedulerRenderer.BlendColors(HoverBlendColor, HoverBaseColor, _hoverSelectedBlend));
			}
			return _hoverSelectedBrush;
		}

		internal Pen GetHoverBasePen()
		{
			if (_hoverBasePen == null)
			{
				_hoverBasePen = new Pen(HoverBaseColor);
			}
			return _hoverBasePen;
		}

		private Color MixColors(Color color, Color colorArgument, float floatValue)
		{
			int red = (int)((float)(int)color.R * floatValue + (float)(int)colorArgument.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)colorArgument.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)colorArgument.B * (1f - floatValue));
			return Color.FromArgb(red, green, blue);
		}

		private void DisposeHoverResources()
		{
			if (_hoverBrush != null)
			{
				_hoverBrush.Dispose();
				_hoverBrush = null;
			}
			if (_hoverSelectedBrush != null)
			{
				_hoverSelectedBrush.Dispose();
				_hoverSelectedBrush = null;
			}
			if (_hoverBasePen != null)
			{
				_hoverBasePen.Dispose();
				_hoverBasePen = null;
			}
			UpdateScrollBarStyle();
		}

		internal void DrawButtonBackground(Graphics graphics, Rectangle rectangle, bool isHot, bool booleanArgument)
		{
			DrawAppearanceBackground(graphics, rectangle, isHot, booleanArgument, ButtonAppearance);
		}

		internal void DrawScrollPartBackground(Graphics graphics, Rectangle rect, bool isHot, bool booleanArgument)
		{
			Brush brush = GetControlBrush();
			GetControlTextPen();
			if (booleanArgument)
			{
				switch (_buttonAppearance)
				{
				case Appearance.Flat:
				case Appearance.FlatBorderless:
					brush = GetHoverSelectedBrush();
					rect = SchedulerRenderer.DrawBorder(graphics, GetHoverBasePen(), rect);
					break;
				case Appearance.Regular:
					rect = SchedulerRenderer.DrawBorder(graphics, GetDarkPen(), rect);
					break;
				case Appearance.PopUp:
				case Appearance.Light3D:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (_buttonAppearance)
				{
				case Appearance.Regular:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.Raised);
					break;
				case Appearance.Light3D:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D);
					break;
				case Appearance.FlatBorderless:
					if (isHot)
					{
						rect = SchedulerRenderer.DrawBorder(graphics, GetHoverBasePen(), rect);
						brush = GetHoverBrush();
					}
					break;
				case Appearance.Flat:
					if (isHot)
					{
						rect = SchedulerRenderer.DrawBorder(graphics, GetHoverBasePen(), rect);
						brush = GetHoverBrush();
					}
					else
					{
						rect = ((_owner == null) ? SchedulerRenderer.DrawBorder(graphics, GetDarkPen(), rect) : SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect));
					}
					break;
				case Appearance.PopUp:
					rect = ((!isHot) ? ((_owner == null) ? SchedulerRenderer.DrawBorder(graphics, GetDarkPen(), rect) : SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect)) : SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D));
					break;
				}
			}
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
		}

		internal void DrawAppearanceBackground(Graphics graphics, Rectangle rect, bool isHot, bool booleanArgument, Appearance appearance)
		{
			Brush brush = GetControlBrush();
			GetControlTextPen();
			if (booleanArgument)
			{
				switch (appearance)
				{
				case Appearance.Flat:
				case Appearance.FlatBorderless:
					rect = SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect);
					break;
				case Appearance.Regular:
					rect = SchedulerRenderer.DrawBorder(graphics, GetDarkPen(), rect);
					break;
				case Appearance.PopUp:
				case Appearance.Light3D:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (appearance)
				{
				case Appearance.Regular:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.Raised);
					break;
				case Appearance.Light3D:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D);
					break;
				case Appearance.FlatBorderless:
					if (isHot)
					{
						rect = SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect);
					}
					break;
				case Appearance.Flat:
					rect = ((!isHot) ? SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect) : SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect));
					break;
				case Appearance.PopUp:
					rect = ((!isHot) ? SchedulerRenderer.DrawBorder(graphics, GetDarkDarkPen(), rect) : SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D));
					break;
				}
			}
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
		}

		internal void DrawBackground(Graphics graphics, Rectangle rect, bool isHot, bool booleanArgument, Appearance appearance, Color baseColor, Brush brush)
		{
			Pen pen = _darkPen;
			Pen savedLightPen = _lightPen;
			Pen savedDarkDarkPen = _darkDarkPen;
			Pen savedLightLightPen = _lightLightPen;
			if (baseColor != ControlColor)
			{
				_darkPen = new Pen(ControlPaint.Dark(baseColor, 0f));
				_lightPen = new Pen(ControlPaint.Light(baseColor, 0f));
				_darkDarkPen = new Pen(ControlPaint.DarkDark(baseColor));
				_lightLightPen = new Pen(ControlPaint.LightLight(baseColor));
			}
			if (booleanArgument)
			{
				switch (appearance)
				{
				case Appearance.Flat:
					rect = SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect);
					break;
				case Appearance.Regular:
					rect = SchedulerRenderer.DrawBorder(graphics, GetDarkPen(), rect);
					break;
				case Appearance.PopUp:
				case Appearance.Light3D:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (appearance)
				{
				case Appearance.Regular:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.Raised);
					break;
				case Appearance.Light3D:
					rect = SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D);
					break;
				case Appearance.Flat:
					rect = ((!isHot) ? SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect) : SchedulerRenderer.DrawBorder(graphics, GetFlatBorderPen(), rect));
					break;
				case Appearance.PopUp:
					rect = ((!isHot) ? SchedulerRenderer.DrawBorder(graphics, GetDarkPen(), rect) : SchedulerRenderer.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D));
					break;
				}
			}
			if (ControlColor != baseColor)
			{
				_darkPen.Dispose();
				_lightPen.Dispose();
				_darkDarkPen.Dispose();
				_lightLightPen.Dispose();
				_darkPen = pen;
				_lightPen = savedLightPen;
				_darkDarkPen = savedDarkDarkPen;
				_lightLightPen = savedLightLightPen;
			}
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
		}

		internal void DrawVerticalArrow(Graphics graphics, Rectangle rectangle, int direction, bool isPressed, bool booleanArgument, bool isEnabled)
		{
			if (isPressed && _buttonAppearance != Appearance.Flat)
			{
				rectangle.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (_buttonAppearance == Appearance.Flat)
				{
					DrawVerticalArrowGlyph(graphics, rectangle.Width, rectangle, direction, GetDarkPen());
					return;
				}
				rectangle.Offset(1, 1);
				DrawVerticalArrowGlyph(graphics, rectangle.Width, rectangle, direction, GetLightLightPen());
				rectangle.Offset(-1, -1);
				DrawVerticalArrowGlyph(graphics, rectangle.Width, rectangle, direction, GetDarkPen());
			}
			else if (ButtonAppearance == Appearance.Flat && booleanArgument)
			{
				rectangle.Offset(1, 1);
				DrawVerticalArrowGlyph(graphics, rectangle.Width, rectangle, direction, GetDarkPen());
				rectangle.Offset(-2, -2);
				DrawVerticalArrowGlyph(graphics, rectangle.Width, rectangle, direction, GetControlTextPen());
			}
			else
			{
				DrawVerticalArrowGlyph(graphics, rectangle.Width, rectangle, direction, GetControlTextPen());
			}
		}

		internal void DrawDownArrow(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = GetControlTextPen();
			int width = rectangle.Width;
			int arrowWidth = width / 2;
			if (arrowWidth % 2 == 0)
			{
				arrowWidth--;
			}
			_ = arrowWidth / 2;
			int rowTop = rectangle.Top;
			int rowLeft = rectangle.Left;
			while (arrowWidth >= 0)
			{
				if (arrowWidth == 1)
				{
					graphics.DrawLine(pen, rowLeft, rowTop - 1, rowLeft, rowTop);
				}
				else
				{
					graphics.DrawLine(pen, rowLeft, rowTop, rowLeft + arrowWidth - 1, rowTop);
				}
				rowLeft++;
				arrowWidth -= 2;
				rowTop++;
			}
		}

		internal void DrawHorizontalArrow(Graphics graphics, Rectangle rectangle, int direction, bool isPressed, bool booleanArgument, bool isEnabled)
		{
			RenderContent(graphics, rectangle, ControlTextColor, direction, isPressed, booleanArgument, isEnabled, _buttonAppearance);
		}

		internal void RenderContent(Graphics graphics, Rectangle rectangle, Color color, int direction, bool isPressed, bool booleanArgument, bool isEnabled, Appearance appearance)
		{
			if (isPressed && appearance != Appearance.Flat)
			{
				rectangle.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (appearance == Appearance.Flat)
				{
					DrawHorizontalArrowGlyph(graphics, GetDarkColor(), rectangle.Height, rectangle, direction);
					return;
				}
				rectangle.Offset(1, 1);
				DrawHorizontalArrowGlyph(graphics, GetScrollBarStyleColor(), rectangle.Height, rectangle, direction);
				rectangle.Offset(-1, -1);
				DrawHorizontalArrowGlyph(graphics, color, rectangle.Height, rectangle, direction);
			}
			else if (appearance == Appearance.Flat && booleanArgument)
			{
				rectangle.Offset(1, 1);
				DrawHorizontalArrowGlyph(graphics, GetDarkColor(), rectangle.Height, rectangle, direction);
				rectangle.Offset(-2, -2);
				DrawHorizontalArrowGlyph(graphics, color, rectangle.Height, rectangle, direction);
			}
			else
			{
				DrawHorizontalArrowGlyph(graphics, color, rectangle.Height, rectangle, direction);
			}
		}

		internal void DisposeAllResources()
		{
			if (_solidBrush != null)
			{
				_solidBrush.Dispose();
				_solidBrush = null;
			}
			if (_darkBrush != null)
			{
				_darkBrush.Dispose();
				_darkBrush = null;
			}
			if (_darkDarkPen != null)
			{
				_darkDarkPen.Dispose();
				_darkDarkPen = null;
			}
			if (_darkPen != null)
			{
				_darkPen.Dispose();
				_darkPen = null;
			}
			if (_lightLightBrush != null)
			{
				_lightLightBrush.Dispose();
				_lightLightBrush = null;
			}
			if (_lightLightPen != null)
			{
				_lightLightPen.Dispose();
				_lightLightPen = null;
			}
			if (_lightPen != null)
			{
				_lightPen.Dispose();
				_lightPen = null;
			}
			if (_controlTextBrush != null)
			{
				_controlTextBrush.Dispose();
				_controlTextBrush = null;
			}
			if (_controlTextPen != null)
			{
				_controlTextPen.Dispose();
				_controlTextPen = null;
			}
			if (_hoverBrush != null)
			{
				_hoverBrush.Dispose();
				_hoverBrush = null;
			}
			if (_hoverBasePen != null)
			{
				_hoverBasePen.Dispose();
				_hoverBasePen = null;
			}
			if (_flatBorderPen != null)
			{
				_flatBorderPen.Dispose();
				_flatBorderPen = null;
			}
			if (_hoverSelectedBrush != null)
			{
				_hoverSelectedBrush.Dispose();
				_hoverSelectedBrush = null;
			}
			if (_scrollBarBrush != null)
			{
				_scrollBarBrush.Dispose();
				_scrollBarBrush = null;
			}
		}

		private void DrawVerticalArrowGlyph(Graphics graphics, int buttonSize, Rectangle rectangle, int integerArgument, Pen pen)
		{
			int rowWidth = buttonSize / 2;
			if (rowWidth % 2 == 0)
			{
				rowWidth--;
			}
			int arrowHeight = rowWidth / 2 + 1;
			int rowY = ((integerArgument <= 0) ? (rectangle.Top + (rectangle.Height - arrowHeight) / 2 + arrowHeight - 1) : (rectangle.Top + (rectangle.Height - arrowHeight) / 2));
			int rowX = rectangle.Left + (rectangle.Width - rowWidth) / 2;
			while (rowWidth >= 0)
			{
				if (rowWidth == 1)
				{
					graphics.DrawLine(pen, rowX, rowY - integerArgument, rowX, rowY);
				}
				else
				{
					graphics.DrawLine(pen, rowX, rowY, rowX + rowWidth - 1, rowY);
				}
				rowX++;
				rowWidth -= 2;
				rowY += integerArgument;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			ControlColor = Color.Empty;
			ControlTextColor = Color.Empty;
			ScrollBarColor = Color.Empty;
			HoverBaseColor = Color.Empty;
			HoverBlendColor = Color.Empty;
			ButtonAppearance = Appearance.Light3D;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (!_controlColor.IsEmpty)
			{
				return true;
			}
			if (!_controlTextColor.IsEmpty)
			{
				return true;
			}
			if (!_scrollBarColor.IsEmpty)
			{
				return true;
			}
			if (!_hoverBlendColor.IsEmpty)
			{
				return true;
			}
			if (!_hoverBaseColor.IsEmpty)
			{
				return true;
			}
			if (_buttonAppearance != Appearance.Light3D)
			{
				return true;
			}
			return false;
		}


		private void DrawHorizontalArrowGlyph(Graphics graphics, Color color, int buttonSize, Rectangle rectangle, int integerArgument)
		{
			Pen pen = GdiCache.Pen(color);
			int columnHeight = buttonSize / 2;
			if (columnHeight % 2 == 0)
			{
				columnHeight--;
			}
			int arrowWidth = columnHeight / 2 + 1;
			int columnX = ((integerArgument <= 0) ? (rectangle.Left + (rectangle.Width - arrowWidth) / 2 + arrowWidth - 1) : (rectangle.Left + (rectangle.Width - arrowWidth) / 2));
			int columnY = rectangle.Top + (rectangle.Height - columnHeight) / 2;
			while (columnHeight >= 0)
			{
				if (columnHeight == 1)
				{
					graphics.DrawLine(pen, columnX - integerArgument, columnY, columnX, columnY);
				}
				else
				{
					graphics.DrawLine(pen, columnX, columnY, columnX, columnY + columnHeight - 1);
				}
				columnX += integerArgument;
				columnHeight -= 2;
				columnY++;
			}
		}

	}
}
