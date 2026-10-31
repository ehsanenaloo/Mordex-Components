using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
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

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_buttonAppearance = (Appearance)LayoutReader.GetPropertyValue(info, "ButtonAppearance", typeof(Appearance), Appearance.Light3D);
			_controlColor = (Color)LayoutReader.GetPropertyValue(info, "ControlColor", typeof(Color), Color.Empty);
			_controlTextColor = (Color)LayoutReader.GetPropertyValue(info, "ControlTextColor", typeof(Color), Color.Empty);
			_scrollBarColor = (Color)LayoutReader.GetPropertyValue(info, "ScrollBarColor", typeof(Color), Color.Empty);
			UpdateSolidBrush();
			DisposeResources();
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
			if (!_scrollBarColor.IsEmpty)
			{
				info.AddValue("ScrollBarColor", _scrollBarColor);
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
				_owner.RefreshHorizontalPanelColor();
			}
		}

		internal bool IsFlatOrThemed()
		{
			if (UsesVisualStyles())
			{
				return true;
			}
			if (_buttonAppearance == Appearance.Flat || _buttonAppearance == Appearance.FlatBorderless || _buttonAppearance == Appearance.PopUp)
			{
				return true;
			}
			return false;
		}

		internal bool IsFlatAppearance()
		{
			if (_buttonAppearance == Appearance.Flat || _buttonAppearance == Appearance.FlatBorderless)
			{
				return true;
			}
			return false;
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
			return !_controlTextColor.IsEmpty;
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

		internal Brush GetControlTextBrush()
		{
			if (_controlTextBrush == null)
			{
				_controlTextBrush = new SolidBrush(ControlTextColor);
			}
			return _controlTextBrush;
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
									bitmap.SetPixel(i, j, GetScrollBarStyleColor());
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

		internal Rectangle DrawButtonBackground(Graphics graphics, Rectangle rectangle, bool isHot, bool booleanArgument)
		{
			return DrawAppearanceBackground(graphics, rectangle, isHot, booleanArgument, ButtonAppearance);
		}

		internal Rectangle DrawAppearanceBackground(Graphics graphics, Rectangle rect, bool isHot, bool booleanArgument, Appearance appearance)
		{
			Brush brush = GetControlBrush();
			GetControlTextPen();
			if (booleanArgument)
			{
				switch (appearance)
				{
				case Appearance.Flat:
				case Appearance.FlatBorderless:
					rect = TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect);
					break;
				case Appearance.Regular:
					rect = TimelineUtilities.DrawFlatBorder(graphics, GetDarkPen(), rect);
					break;
				case Appearance.PopUp:
				case Appearance.Light3D:
					rect = TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (appearance)
				{
				case Appearance.Regular:
					rect = TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.Raised);
					break;
				case Appearance.Light3D:
					rect = TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D);
					break;
				case Appearance.FlatBorderless:
					if (isHot)
					{
						rect = TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect);
					}
					break;
				case Appearance.Flat:
					rect = ((!isHot) ? TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect) : TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect));
					break;
				case Appearance.PopUp:
					rect = ((!isHot) ? TimelineUtilities.DrawFlatBorder(graphics, GetDarkDarkPen(), rect) : TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D));
					break;
				}
			}
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
			return rect;
		}

		internal void DrawArrowWithBar(Graphics graphics, Rectangle rectangleArgument, int direction, bool isPressed, bool booleanArgument, bool isEnabled)
		{
			int glyphHeight = rectangleArgument.Height / 2;
			if (glyphHeight % 2 == 0)
			{
				glyphHeight--;
			}
			int arrowWidth = glyphHeight / 2 + 1;
			int glyphTop = rectangleArgument.Top + (rectangleArgument.Height - glyphHeight) / 2;
			int barWidth = arrowWidth / 2;
			int glyphWidth = barWidth * 2 + arrowWidth;
			int barLeft = ((direction != -1) ? (rectangleArgument.Right - (rectangleArgument.Width - glyphWidth) / 2 - barWidth) : (rectangleArgument.Left + (rectangleArgument.Width - glyphWidth) / 2));
			if (isPressed && !IsFlatAppearance())
			{
				rectangleArgument.Offset(1, 1);
			}
			Rectangle rectangle = ((direction != -1) ? new Rectangle(barLeft - barWidth - arrowWidth, rectangleArgument.Top, arrowWidth, rectangleArgument.Height) : new Rectangle(barLeft + barWidth * 2, rectangleArgument.Top, arrowWidth, rectangleArgument.Height));
			if (!isEnabled)
			{
				if (IsFlatAppearance())
				{
					DrawHorizontalArrowGlyph(graphics, rectangleArgument.Height, rectangle, direction, GetDarkPen());
					RenderContent(graphics, barLeft, glyphTop, glyphHeight, barWidth, GetDarkPen());
					return;
				}
				rectangle.Offset(1, 1);
				DrawHorizontalArrowGlyph(graphics, rectangleArgument.Height, rectangle, direction, GetLightLightPen());
				RenderContent(graphics, barLeft + 1, glyphTop + 1, glyphHeight, barWidth, GetLightLightPen());
				rectangle.Offset(-1, -1);
				DrawHorizontalArrowGlyph(graphics, rectangleArgument.Height, rectangle, direction, GetDarkPen());
				RenderContent(graphics, barLeft, glyphTop, glyphHeight, barWidth, GetDarkPen());
			}
			else
			{
				DrawHorizontalArrowGlyph(graphics, rectangleArgument.Height, rectangle, direction, GetControlTextPen());
				RenderContent(graphics, barLeft, glyphTop, glyphHeight, barWidth, GetControlTextPen());
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
					rect = TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect);
					break;
				case Appearance.Regular:
					rect = TimelineUtilities.DrawFlatBorder(graphics, GetDarkPen(), rect);
					break;
				case Appearance.PopUp:
				case Appearance.Light3D:
					rect = TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (appearance)
				{
				case Appearance.Regular:
					rect = TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.Raised);
					break;
				case Appearance.Light3D:
					rect = TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D);
					break;
				case Appearance.Flat:
					rect = ((!isHot) ? TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect) : TimelineUtilities.DrawFlatBorder(graphics, GetFlatBorderPen(), rect));
					break;
				case Appearance.PopUp:
					rect = ((!isHot) ? TimelineUtilities.DrawFlatBorder(graphics, GetDarkPen(), rect) : TimelineUtilities.RenderContent(graphics, this, rect, BorderStyle.RaisedLight3D));
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

		internal void DrawDownArrow(Graphics graphics, Rectangle rectangle, Color color)
		{
			Pen pen = new Pen(color);
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
			pen.Dispose();
			pen = null;
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

		internal void DrawHorizontalArrow(Graphics graphics, Rectangle rectangle, int direction, bool isPressed, bool booleanArgument, bool isEnabled)
		{
			DrawHorizontalArrowWithAppearance(graphics, rectangle, direction, isPressed, booleanArgument, isEnabled, _buttonAppearance);
		}

		internal void DrawHorizontalArrowWithAppearance(Graphics graphics, Rectangle rectangle, int direction, bool isPressed, bool booleanArgument, bool isEnabled, Appearance appearance)
		{
			if (isPressed && appearance != Appearance.Flat)
			{
				rectangle.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (appearance == Appearance.Flat)
				{
					DrawHorizontalArrowGlyph(graphics, rectangle.Height, rectangle, direction, GetDarkPen());
					return;
				}
				rectangle.Offset(1, 1);
				DrawHorizontalArrowGlyph(graphics, rectangle.Height, rectangle, direction, GetLightLightPen());
				rectangle.Offset(-1, -1);
				DrawHorizontalArrowGlyph(graphics, rectangle.Height, rectangle, direction, GetDarkPen());
			}
			else if (appearance == Appearance.Flat && booleanArgument)
			{
				rectangle.Offset(1, 1);
				DrawHorizontalArrowGlyph(graphics, rectangle.Height, rectangle, direction, GetDarkPen());
				rectangle.Offset(-2, -2);
				DrawHorizontalArrowGlyph(graphics, rectangle.Height, rectangle, direction, GetControlTextPen());
			}
			else
			{
				DrawHorizontalArrowGlyph(graphics, rectangle.Height, rectangle, direction, GetControlTextPen());
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

		private void RenderContent(Graphics graphics, int x, int y1, int integerArgument, int barWidth, Pen pen)
		{
			for (int i = 0; i < barWidth; i++)
			{
				graphics.DrawLine(pen, x + i, y1, x + i, y1 + integerArgument - 1);
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
			ScrollBarColor = SystemColors.ScrollBar;
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
			if (_buttonAppearance != Appearance.Light3D)
			{
				return true;
			}
			return false;
		}

		private void DrawHorizontalArrowGlyph(Graphics graphics, int buttonSize, Rectangle rectangle, int integerArgument, Pen pen)
		{
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

		internal bool UsesVisualStyles()
		{
			if (_owner != null)
			{
				return TimelineVisualStyleRenderer.AreVisualStylesActive(false);
			}
			return true;
		}
	}
}
