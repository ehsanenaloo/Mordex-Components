using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.TaskPane.Internal;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class ControlStyle : IMordexXmlLayoutsSupport
	{
		internal ControlStyleControl _owner;

		private ScrollBarAppearance _buttonAppearance = ScrollBarAppearance.Light3D;

		private Color _controlColor = Color.Empty;

		private Brush _buttonBrush;

		private Color _controlTextColor = Color.Empty;

		private Brush _buttonTextBrush;

		private Pen _buttonTextPen;

		private Color _frameColor = SystemColors.WindowFrame;

		private Pen _framePen;

		private Color _scrollBarColor = Color.Empty;

		private Brush _trackBrush;

		private Color _lightColor = SystemColors.ControlLight;

		private Pen _lightPen;

		private Color _lightLightColor = SystemColors.ControlLightLight;

		private Pen _lightLightPen;

		private Brush _lightLightBrush;

		private Color _darkColor = SystemColors.ControlDark;

		private Pen _darkPen;

		private Brush _darkBrush;

		private Color _darkDarkColor = SystemColors.ControlDarkDark;

		private Pen _darkDarkPen;

		private Color _hoverBaseColor = Color.Empty;

		private Color _hoverBlendColor = Color.Empty;

		private float _hoverBlend = 0.667f;

		private float _hoverSelectedBlend = 0.5f;

		private Brush _hoverBrush;

		private Brush _hoverSelectedBrush;

		private Pen _hoverBorderPen;

		private bool _isThemed = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(ScrollBarAppearance.Light3D)]
		[Description("Determines the appearance of the buttons of the scroll bar.")]
		public ScrollBarAppearance ButtonAppearance
		{
			get
			{
				return _buttonAppearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ScrollBarAppearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _buttonAppearance)
				{
					_buttonAppearance = value;
					RepaintOwner();
				}
			}
		}

		[Description("Specifies the background color of the buttons of the scroll bar.")]
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
					ApplyControlColor();
				}
			}
		}

		[Description("Specifies the foreground color of the buttons of the scroll bar.")]
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
					DisposeTextResources();
				}
			}
		}

		[Description("Represents the background color of the scroll bar.")]
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
					RefreshTrack();
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

		[Description("Represents the color that will be blended with the HoverBaseColor when a flat appearance is used.")]
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

		[Description("Represents the percentage of the HoverBlendColor to blend with the HoverBaseColor when the mouse is captured by the button.")]
		[DefaultValue(0.5f)]
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
			_buttonAppearance = (ScrollBarAppearance)LayoutReader.GetPropertyValue(info, "ButtonAppearance", typeof(ScrollBarAppearance), ScrollBarAppearance.Light3D);
			_controlColor = (Color)LayoutReader.GetPropertyValue(info, "ControlColor", typeof(Color), Color.Empty);
			_controlTextColor = (Color)LayoutReader.GetPropertyValue(info, "ControlTextColor", typeof(Color), Color.Empty);
			_hoverBaseColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBaseColor", typeof(Color), Color.Empty);
			_hoverBlendColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBlendColor", typeof(Color), Color.Empty);
			_scrollBarColor = (Color)LayoutReader.GetPropertyValue(info, "ScrollBarColor", typeof(Color), Color.Empty);
			_hoverBlend = (float)LayoutReader.GetPropertyValue(info, "HoverBlend", typeof(float), 0.667f);
			_hoverSelectedBlend = (float)LayoutReader.GetPropertyValue(info, "HoverSelectedBlend", typeof(float), 0.5f);
			ApplyControlColor();
			DisposeTextResources();
			DisposeHoverResources();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_buttonAppearance != ScrollBarAppearance.Light3D)
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

		internal ControlStyle(ControlStyleControl owner)
		{
			_owner = owner;
		}

		internal void DisposeAllResources()
		{
			if (_buttonBrush != null)
			{
				_buttonBrush.Dispose();
				_buttonBrush = null;
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
			if (_buttonTextBrush != null)
			{
				_buttonTextBrush.Dispose();
				_buttonTextBrush = null;
			}
			if (_buttonTextPen != null)
			{
				_buttonTextPen.Dispose();
				_buttonTextPen = null;
			}
			if (_hoverBrush != null)
			{
				_hoverBrush.Dispose();
				_hoverBrush = null;
			}
			if (_hoverBorderPen != null)
			{
				_hoverBorderPen.Dispose();
				_hoverBorderPen = null;
			}
			if (_hoverSelectedBrush != null)
			{
				_hoverSelectedBrush.Dispose();
				_hoverSelectedBrush = null;
			}
			if (_trackBrush != null)
			{
				_trackBrush.Dispose();
				_trackBrush = null;
			}
			if (_framePen != null)
			{
				_framePen.Dispose();
				_framePen = null;
			}
			_owner = null;
		}

		private void RepaintOwner()
		{
			if (_owner != null)
			{
				_owner.Invalidate();
			}
		}

		internal bool ShouldSerialize()
		{
			if (_buttonAppearance != ScrollBarAppearance.Light3D)
			{
				return true;
			}
			if (_hoverBlend != 0.667f)
			{
				return true;
			}
			if (_hoverSelectedBlend != 0.5f)
			{
				return true;
			}
			if (!ShouldSerializeControlColor() && !ShouldSerializeControlTextColor() && !ShouldSerializeHoverBaseColor() && !ShouldSerializeScrollBarColor())
			{
				return ShouldSerializeHoverBlendColor();
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_buttonAppearance = ScrollBarAppearance.Light3D;
			_hoverBlend = 0.667f;
			_hoverSelectedBlend = 0.5f;
			ResetControlColor();
			ResetControlTextColor();
			ResetHoverBaseColor();
			ResetScrollBarColor();
			ResetHoverBlendColor();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlColor()
		{
			ControlColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlColor()
		{
			return !_controlColor.IsEmpty;
		}

		private void ApplyControlColor()
		{
			if (_buttonBrush != null)
			{
				_buttonBrush.Dispose();
				_buttonBrush = null;
			}
			if (ControlColor == SystemColors.Control || ControlColor == SystemColors.Window)
			{
				UpdateDarkColor(SystemColors.ControlDark);
				UpdateDarkDarkColor(SystemColors.ControlDarkDark);
				UpdateLightColor(SystemColors.ControlLight);
				UpdateLightLightColor(SystemColors.ControlLightLight);
			}
			else
			{
				UpdateDarkColor(ControlPaint.Dark(ControlColor, 0f));
				UpdateDarkDarkColor(ControlPaint.DarkDark(ControlColor));
				UpdateLightColor(ControlPaint.Light(ControlColor, 0f));
				UpdateLightLightColor(ControlPaint.LightLight(ControlColor));
			}
			RefreshTrack();
			RepaintOwner();
		}

		internal Brush GetButtonBrush()
		{
			if (_buttonBrush == null)
			{
				_buttonBrush = new SolidBrush(ControlColor);
			}
			return _buttonBrush;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlTextColor()
		{
			ControlTextColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlTextColor()
		{
			return !_controlTextColor.IsEmpty;
		}

		private void DisposeTextResources()
		{
			if (_buttonTextBrush != null)
			{
				_buttonTextBrush.Dispose();
				_buttonTextBrush = null;
			}
			if (_buttonTextPen != null)
			{
				_buttonTextPen.Dispose();
				_buttonTextPen = null;
			}
			RepaintOwner();
		}

		internal Pen GetButtonTextPen()
		{
			if (_buttonTextPen == null)
			{
				_buttonTextPen = new Pen(ControlTextColor);
			}
			return _buttonTextPen;
		}

		internal void UpdateFrameColor(Color frameColor)
		{
			if (_frameColor != frameColor)
			{
				_frameColor = frameColor;
				if (_framePen != null)
				{
					_framePen.Dispose();
					_framePen = null;
				}
				RepaintOwner();
			}
		}

		internal Pen GetFramePen()
		{
			if (_framePen == null)
			{
				_framePen = new Pen(_frameColor);
			}
			return _framePen;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScrollBarColor()
		{
			ScrollBarColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScrollBarColor()
		{
			return !_scrollBarColor.IsEmpty;
		}

		internal Brush GetTrackBrush()
		{
			if (_trackBrush == null)
			{
				if (ScrollBarColor.ToArgb() == ControlColor.ToArgb() || ScrollBarColor.Equals(SystemColors.ScrollBar))
				{
					Bitmap patternBitmap = new Bitmap(8, 8);
					for (int i = 0; i < patternBitmap.Width; i += 2)
					{
						for (int j = 0; j < patternBitmap.Height; j++)
						{
							try
							{
								if (j % 2 == 0)
								{
									patternBitmap.SetPixel(i, j, ControlColor);
								}
								else
								{
									patternBitmap.SetPixel(i, j, _lightLightColor);
								}
							}
							catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
							{
								ControlFailure.Log(failure);
							}
						}
					}
					for (int i = 1; i < patternBitmap.Width; i += 2)
					{
						for (int j = 0; j < patternBitmap.Height; j++)
						{
							try
							{
								if (j % 2 == 0)
								{
									patternBitmap.SetPixel(i, j, _lightLightColor);
								}
								else
								{
									patternBitmap.SetPixel(i, j, ControlColor);
								}
							}
							catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
							{
								ControlFailure.Log(failure);
							}
						}
					}
					_trackBrush = new TextureBrush(patternBitmap);
					patternBitmap.Dispose();
					patternBitmap = null;
				}
				else
				{
					_trackBrush = new SolidBrush(ScrollBarColor);
				}
			}
			return _trackBrush;
		}

		private void RefreshTrack()
		{
			DisposeTrackBrush();
			RepaintOwner();
		}

		internal void DisposeTrackBrush()
		{
			if (_trackBrush != null)
			{
				_trackBrush.Dispose();
				_trackBrush = null;
			}
		}

		internal void UpdateLightColor(Color lightColor)
		{
			if (_lightColor != lightColor)
			{
				_lightColor = lightColor;
				DisposeLightPen();
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

		private void DisposeLightPen()
		{
			if (_lightPen != null)
			{
				_lightPen.Dispose();
				_lightPen = null;
			}
			RepaintOwner();
		}

		internal void UpdateLightLightColor(Color lightLightColor)
		{
			if (_lightLightColor != lightLightColor)
			{
				_lightLightColor = lightLightColor;
				DisposeLightLightResources();
			}
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
			RefreshTrack();
			RepaintOwner();
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
			RepaintOwner();
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
			RefreshTrack();
			RepaintOwner();
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBlendColor()
		{
			HoverBlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBlendColor()
		{
			return !_hoverBlendColor.IsEmpty;
		}

		internal Brush GetHoverBrush()
		{
			if (_hoverBrush == null)
			{
				_hoverBrush = new SolidBrush(TaskPanePaintHelper.BlendColors(HoverBlendColor, HoverBaseColor, _hoverBlend));
			}
			return _hoverBrush;
		}

		internal Brush GetHoverSelectedBrush()
		{
			if (_hoverSelectedBrush == null)
			{
				_hoverSelectedBrush = new SolidBrush(TaskPanePaintHelper.BlendColors(HoverBlendColor, HoverBaseColor, _hoverSelectedBlend));
			}
			return _hoverSelectedBrush;
		}

		internal Pen GetHoverBorderPen()
		{
			if (_hoverBorderPen == null)
			{
				_hoverBorderPen = new Pen(HoverBaseColor);
			}
			return _hoverBorderPen;
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
			if (_hoverBorderPen != null)
			{
				_hoverBorderPen.Dispose();
				_hoverBorderPen = null;
			}
			RepaintOwner();
		}

		internal void DrawButtonBackground(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed)
		{
			Brush fillBrush = GetButtonBrush();
			GetButtonTextPen();
			if (isPressed)
			{
				switch (_buttonAppearance)
				{
				case ScrollBarAppearance.Flat:
				case ScrollBarAppearance.FlatBorderless:
					fillBrush = GetHoverSelectedBrush();
					bounds = TaskPanePaintHelper.DrawOutline(graphics, GetHoverBorderPen(), bounds);
					break;
				case ScrollBarAppearance.Regular:
					bounds = TaskPanePaintHelper.DrawOutline(graphics, GetDarkPen(), bounds);
					break;
				case ScrollBarAppearance.Light3D:
				case ScrollBarAppearance.PopUp:
					bounds = TaskPanePaintHelper.DrawBorderFromStyle(graphics, this, bounds, BorderStyle.SunkenLight3D);
					break;
				}
			}
			else
			{
				switch (_buttonAppearance)
				{
				case ScrollBarAppearance.Regular:
					bounds = TaskPanePaintHelper.DrawBorderFromStyle(graphics, this, bounds, BorderStyle.Raised);
					break;
				case ScrollBarAppearance.Light3D:
					bounds = TaskPanePaintHelper.DrawBorderFromStyle(graphics, this, bounds, BorderStyle.RaisedLight3D);
					break;
				case ScrollBarAppearance.FlatBorderless:
					if (isHot)
					{
						bounds = TaskPanePaintHelper.DrawOutline(graphics, GetHoverBorderPen(), bounds);
						fillBrush = GetHoverBrush();
					}
					break;
				case ScrollBarAppearance.Flat:
					if (isHot)
					{
						bounds = TaskPanePaintHelper.DrawOutline(graphics, GetHoverBorderPen(), bounds);
						fillBrush = GetHoverBrush();
					}
					else
					{
						bounds = ((_owner == null) ? TaskPanePaintHelper.DrawOutline(graphics, GetDarkPen(), bounds) : TaskPanePaintHelper.DrawOutline(graphics, GetFramePen(), bounds));
					}
					break;
				case ScrollBarAppearance.PopUp:
					bounds = ((!isHot) ? ((_owner == null) ? TaskPanePaintHelper.DrawOutline(graphics, GetDarkPen(), bounds) : TaskPanePaintHelper.DrawOutline(graphics, GetFramePen(), bounds)) : TaskPanePaintHelper.DrawBorderFromStyle(graphics, this, bounds, BorderStyle.RaisedLight3D));
					break;
				}
			}
			if (fillBrush != null)
			{
				graphics.FillRectangle(fillBrush, bounds);
			}
		}

		internal void DrawVerticalArrow(Graphics graphics, Rectangle buttonBounds, int direction, bool isPressed, bool isHot, bool isEnabled)
		{
			if (isPressed && !IsFlatAppearance())
			{
				buttonBounds.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (IsFlatAppearance())
				{
					DrawVerticalArrowGlyph(graphics, buttonBounds.Width, buttonBounds, direction, GetDarkPen());
					return;
				}
				buttonBounds.Offset(1, 1);
				DrawVerticalArrowGlyph(graphics, buttonBounds.Width, buttonBounds, direction, GetLightLightPen());
				buttonBounds.Offset(-1, -1);
				DrawVerticalArrowGlyph(graphics, buttonBounds.Width, buttonBounds, direction, GetDarkPen());
			}
			else
			{
				DrawVerticalArrowGlyph(graphics, buttonBounds.Width, buttonBounds, direction, GetButtonTextPen());
			}
		}

		internal void DrawHorizontalArrow(Graphics graphics, Rectangle buttonBounds, int direction, bool isPressed, bool isHot, bool isEnabled)
		{
			if (isPressed && !IsFlatAppearance())
			{
				buttonBounds.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (IsFlatAppearance())
				{
					DrawHorizontalArrowGlyph(graphics, buttonBounds.Height, buttonBounds, direction, GetDarkPen());
					return;
				}
				buttonBounds.Offset(1, 1);
				DrawHorizontalArrowGlyph(graphics, buttonBounds.Height, buttonBounds, direction, GetLightLightPen());
				buttonBounds.Offset(-1, -1);
				DrawHorizontalArrowGlyph(graphics, buttonBounds.Height, buttonBounds, direction, GetDarkPen());
			}
			else
			{
				DrawHorizontalArrowGlyph(graphics, buttonBounds.Height, buttonBounds, direction, GetButtonTextPen());
			}
		}

		internal bool IsFlatAppearance()
		{
			if (_buttonAppearance == ScrollBarAppearance.Flat || _buttonAppearance == ScrollBarAppearance.FlatBorderless)
			{
				return true;
			}
			return false;
		}

		internal bool GetIsThemed()
		{
			return _isThemed;
		}

		internal void UpdateIsThemed(bool isThemed)
		{
			if (_isThemed != isThemed)
			{
				_isThemed = isThemed;
				RepaintOwner();
			}
		}

		internal bool IsHotTrackingEnabled()
		{
			if (GetIsThemed() && TaskPaneRenderer.IsThemingActive())
			{
				return true;
			}
			if (_buttonAppearance == ScrollBarAppearance.Flat || _buttonAppearance == ScrollBarAppearance.FlatBorderless || _buttonAppearance == ScrollBarAppearance.PopUp)
			{
				return true;
			}
			return false;
		}

		private void DrawVerticalArrowGlyph(Graphics graphics, int buttonSize, Rectangle buttonBounds, int direction, Pen pen)
		{
			int lineWidth = buttonSize / 2;
			if (lineWidth % 2 == 0)
			{
				lineWidth--;
			}
			int arrowHeight = lineWidth / 2 + 1;
			int lineY = ((direction <= 0) ? (buttonBounds.Top + (buttonBounds.Height - arrowHeight) / 2 + arrowHeight - 1) : (buttonBounds.Top + (buttonBounds.Height - arrowHeight) / 2));
			int lineX = buttonBounds.Left + (buttonBounds.Width - lineWidth) / 2;
			while (lineWidth >= 0)
			{
				if (lineWidth == 1)
				{
					graphics.DrawLine(pen, lineX, lineY - direction, lineX, lineY);
				}
				else
				{
					graphics.DrawLine(pen, lineX, lineY, lineX + lineWidth - 1, lineY);
				}
				lineX++;
				lineWidth -= 2;
				lineY += direction;
			}
		}

		private void DrawHorizontalArrowGlyph(Graphics graphics, int buttonSize, Rectangle buttonBounds, int direction, Pen pen)
		{
			int lineHeight = buttonSize / 2;
			if (lineHeight % 2 == 0)
			{
				lineHeight--;
			}
			int arrowWidth = lineHeight / 2 + 1;
			int lineX = ((direction <= 0) ? (buttonBounds.Left + (buttonBounds.Width - arrowWidth) / 2 + arrowWidth - 1) : (buttonBounds.Left + (buttonBounds.Width - arrowWidth) / 2));
			int lineY = buttonBounds.Top + (buttonBounds.Height - lineHeight) / 2;
			while (lineHeight >= 0)
			{
				if (lineHeight == 1)
				{
					graphics.DrawLine(pen, lineX - direction, lineY, lineX, lineY);
				}
				else
				{
					graphics.DrawLine(pen, lineX, lineY, lineX, lineY + lineHeight - 1);
				}
				lineX += direction;
				lineHeight -= 2;
				lineY++;
			}
		}
	}
}
