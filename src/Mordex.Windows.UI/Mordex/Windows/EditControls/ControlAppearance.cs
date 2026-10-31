using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI;

namespace Mordex.Windows.UI.Controls
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class ControlAppearance : IMordexXmlLayoutsSupport
	{
		private const float DefaultHoverBlend = 0.667f;

		private const float DefaultHoverSelectedBlend = 0.5f;

		internal IWindowsElement _owner;

		private ButtonAppearance _buttonAppearance = ButtonAppearance.Light3D;

		private Color _controlColor = Color.Empty;

		private Brush _controlBrush;

		private Color _controlTextColor = Color.Empty;

		private Pen _controlTextPen;

		private Color _scrollBarColor = Color.Empty;

		private Brush _scrollBarBrush;

		private Color _lightColor = SystemColors.ControlLight;

		private Pen _lightPen;

		private Color _lightLightColor = SystemColors.ControlLightLight;

		private Pen _lightLightPen;

		private Color _darkColor = SystemColors.ControlDark;

		private Pen _darkPen;

		private Color _darkDarkColor = SystemColors.ControlDarkDark;

		private Pen _darkDarkPen;

		private Color _windowColor = Color.Empty;

		private Color _windowTextColor = Color.Empty;

		private Color _hoverBaseColor = Color.Empty;

		private Color _hoverBlendColor = Color.Empty;

		private float _hoverBlend = DefaultHoverBlend;

		private float _hoverSelectedBlend = DefaultHoverSelectedBlend;

		private Brush _hoverBrush;

		private Brush _hoverSelectedBrush;

		private Pen _hoverBasePen;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(ButtonAppearance.Light3D)]
		public ButtonAppearance ButtonAppearance
		{
			get
			{
				return _buttonAppearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ButtonAppearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _buttonAppearance)
				{
					_buttonAppearance = value;
					ApplyStyleChange();
				}
			}
		}

		public Color ControlColor
		{
			get
			{
				if (_controlColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Control);
				}
				return _controlColor;
			}
			set
			{
				if (value != _controlColor)
				{
					_controlColor = value;
					UpdateShadeColors();
				}
			}
		}

		public Color ControlTextColor
		{
			get
			{
				if (_controlTextColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ControlText);
				}
				return _controlTextColor;
			}
			set
			{
				if (_controlTextColor != value)
				{
					_controlTextColor = value;
					ApplyStyleChange();
				}
			}
		}

		public Color ScrollBarColor
		{
			get
			{
				if (_scrollBarColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Control);
				}
				return _scrollBarColor;
			}
			set
			{
				if (_scrollBarColor != value)
				{
					_scrollBarColor = value;
					ApplyStyleChange();
				}
			}
		}

		public Color WindowColor
		{
			get
			{
				if (_windowColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Window);
				}
				return _windowColor;
			}
			set
			{
				if (value != _windowColor)
				{
					_windowColor = value;
					ApplyStyleChange();
				}
			}
		}

		public Color WindowTextColor
		{
			get
			{
				if (_windowTextColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.WindowText);
				}
				return _windowTextColor;
			}
			set
			{
				if (_windowTextColor != value)
				{
					_windowTextColor = value;
					ApplyStyleChange();
				}
			}
		}

		public Color HoverBaseColor
		{
			get
			{
				if (_hoverBaseColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Highlight);
				}
				return _hoverBaseColor;
			}
			set
			{
				if (_hoverBaseColor != value)
				{
					_hoverBaseColor = value;
					ApplyStyleChange();
				}
			}
		}

		public Color HoverBlendColor
		{
			get
			{
				if (_hoverBlendColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.Window);
				}
				return _hoverBlendColor;
			}
			set
			{
				if (!_hoverBlendColor.Equals(value))
				{
					_hoverBlendColor = value;
					ApplyStyleChange();
				}
			}
		}

		[DefaultValue(DefaultHoverBlend)]
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
				}
			}
		}

		[DefaultValue(DefaultHoverSelectedBlend)]
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
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_buttonAppearance = (ButtonAppearance)LayoutReader.GetPropertyValue(info, "ButtonAppearance", typeof(ButtonAppearance), ButtonAppearance.Light3D);
			_controlColor = (Color)LayoutReader.GetPropertyValue(info, "ControlColor", typeof(Color), Color.Empty);
			_controlTextColor = (Color)LayoutReader.GetPropertyValue(info, "ControlTextColor", typeof(Color), Color.Empty);
			_hoverBaseColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBaseColor", typeof(Color), Color.Empty);
			_hoverBlendColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBlendColor", typeof(Color), Color.Empty);
			_scrollBarColor = (Color)LayoutReader.GetPropertyValue(info, "ScrollBarColor", typeof(Color), Color.Empty);
			_windowColor = (Color)LayoutReader.GetPropertyValue(info, "WindowColor", typeof(Color), Color.Empty);
			_windowTextColor = (Color)LayoutReader.GetPropertyValue(info, "WindowTextColor", typeof(Color), Color.Empty);
			_hoverBlend = (float)LayoutReader.GetPropertyValue(info, "HoverBlend", typeof(float), DefaultHoverBlend);
			_hoverSelectedBlend = (float)LayoutReader.GetPropertyValue(info, "HoverSelectedBlend", typeof(float), DefaultHoverSelectedBlend);
			UpdateShadeColors();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_buttonAppearance != ButtonAppearance.Light3D)
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
			if (!_windowColor.IsEmpty)
			{
				info.AddValue("WindowColor", _windowColor);
			}
			if (!_windowTextColor.IsEmpty)
			{
				info.AddValue("WindowTextColor", _windowTextColor);
			}
			if (_hoverBlend != DefaultHoverBlend)
			{
				info.AddValue("HoverBlend", _hoverBlend);
			}
			if (_hoverSelectedBlend != DefaultHoverSelectedBlend)
			{
				info.AddValue("HoverSelectedBlend", _hoverSelectedBlend);
			}
		}

		internal ControlAppearance(IWindowsElement owner)
		{
			_owner = owner;
		}

		internal void DisposeResources()
		{
			if (_controlBrush != null)
			{
				_controlBrush.Dispose();
				_controlBrush = null;
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

		private void ApplyStyleChange()
		{
			if (_owner != null)
			{
				DisposeResources();
				_owner.OnControlStyleChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_buttonAppearance = ButtonAppearance.Regular;
			_hoverBlend = DefaultHoverBlend;
			_hoverSelectedBlend = DefaultHoverSelectedBlend;
			ResetControlColor();
			ResetControlTextColor();
			ResetHoverBaseColor();
			ResetScrollBarColor();
			ResetWindowColor();
			ResetWindowTextColor();
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

		private void UpdateShadeColors()
		{
			SetDarkColor(ControlPaint.Dark(ControlColor, 0f));
			SetDarkDarkColor(ControlPaint.DarkDark(ControlColor));
			SetLightColor(ControlPaint.Light(ControlColor, 0f));
			SetLightLightColor(ControlPaint.LightLight(ControlColor));
			ApplyStyleChange();
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

		internal void SetLightColor(Color color)
		{
			if (_lightColor != color)
			{
				_lightColor = color;
			}
		}

		internal void SetLightLightColor(Color lightLightColor)
		{
			if (_lightLightColor != lightLightColor)
			{
				_lightLightColor = lightLightColor;
			}
		}

		internal void SetDarkColor(Color darkColor)
		{
			if (_darkColor != darkColor)
			{
				_darkColor = darkColor;
			}
		}

		internal void SetDarkDarkColor(Color darkDarkColor)
		{
			_darkDarkColor = darkDarkColor;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWindowColor()
		{
			WindowColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWindowColor()
		{
			return !_windowColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWindowTextColor()
		{
			WindowTextColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWindowTextColor()
		{
			return !_windowTextColor.IsEmpty;
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
			_hoverBlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBlendColor()
		{
			return !_hoverBlendColor.IsEmpty;
		}

		internal bool IsFlat()
		{
			if (_buttonAppearance == ButtonAppearance.Flat || _buttonAppearance == ButtonAppearance.FlatBorderless)
			{
				return true;
			}
			return false;
		}

		internal bool ShouldHighlightOnHover(bool isThemed)
		{
			if (isThemed)
			{
				return true;
			}
			if (_buttonAppearance == ButtonAppearance.Flat || _buttonAppearance == ButtonAppearance.FlatBorderless || _buttonAppearance == ButtonAppearance.PopUp)
			{
				return true;
			}
			return false;
		}
	}
}
