using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class ControlStyle
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_buttonAppearance = (ButtonAppearance)LayoutReader.GetPropertyValue(info, "ButtonAppearance", typeof(ButtonAppearance), ButtonAppearance.Light3D);
			_controlColor = (Color)LayoutReader.GetPropertyValue(info, "ControlColor", typeof(Color), SystemColors.Control);
			_controlTextColor = (Color)LayoutReader.GetPropertyValue(info, "ControlTextColor", typeof(Color), SystemColors.ControlText);
			_hoverBaseColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBaseColor", typeof(Color), SystemColors.Highlight);
			_hoverBlendColor = (Color)LayoutReader.GetPropertyValue(info, "HoverBlendColor", typeof(Color), SystemColors.Window);
			_scrollBarColor = (Color)LayoutReader.GetPropertyValue(info, "ScrollBarColor", typeof(Color), SystemColors.ScrollBar);
			_windowColor = (Color)LayoutReader.GetPropertyValue(info, "WindowColor", typeof(Color), SystemColors.Window);
			_windowTextColor = (Color)LayoutReader.GetPropertyValue(info, "WindowTextColor", typeof(Color), SystemColors.WindowText);
			_hoverBlend = (float)LayoutReader.GetPropertyValue(info, "HoverBlend", typeof(float), 0.667f);
			_hoverSelectedBlend = (float)LayoutReader.GetPropertyValue(info, "HoverSelectedBlend", typeof(float), 0.5f);
			RefreshPaletteColors();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_buttonAppearance != ButtonAppearance.Light3D)
			{
				info.AddValue("ButtonAppearance", _buttonAppearance);
			}
			if (!_controlColor.Equals(SystemColors.Control))
			{
				info.AddValue("ControlColor", _controlColor);
			}
			if (!_controlTextColor.Equals(SystemColors.ControlText))
			{
				info.AddValue("ControlTextColor", _controlTextColor);
			}
			if (!_hoverBaseColor.Equals(SystemColors.Highlight))
			{
				info.AddValue("HoverBaseColor", _hoverBaseColor);
			}
			if (ShouldSerializeHoverBlendColor())
			{
				info.AddValue("HoverBlendColor", _hoverBlendColor);
			}
			if (!_scrollBarColor.Equals(SystemColors.ScrollBar))
			{
				info.AddValue("ScrollBarColor", _scrollBarColor);
			}
			if (!_windowColor.Equals(SystemColors.Window))
			{
				info.AddValue("WindowColor", _windowColor);
			}
			if (!_windowTextColor.Equals(SystemColors.WindowText))
			{
				info.AddValue("WindowTextColor", _windowTextColor);
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

		internal void ReleaseLayoutState()
		{
			if (_controlStyleLayoutState != null)
			{
				_controlStyleLayoutState.DisposeRenderers();
			}
			DisposeResources();
			if (_controlStyleLayoutState != null)
			{
				_controlStyleLayoutState.ResetCachedStyles();
				_controlStyleLayoutState.ReleaseAll();
				_controlStyleLayoutState = null;
			}
		}

		private void NotifyChanged()
		{
			if (_printDocument != null)
			{
				_printDocument.OnControlStyleChanged();
			}
			if (_changedHandler != null)
			{
				_changedHandler(this, EventArgs.Empty);
			}
		}

		internal void AddChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _changedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _changedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		internal void RemoveChangedHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _changedHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _changedHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		internal bool HasNonDefaultSettings()
		{
			if (_buttonAppearance != ButtonAppearance.Light3D)
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
			if (!ShouldSerializeControlColor() && !ShouldSerializeControlTextColor() && !ShouldSerializeHoverBaseColor() && !ShouldSerializeScrollBarColor() && !ShouldSerializeWindowColor() && !ShouldSerializeWindowTextColor())
			{
				return ShouldSerializeHoverBlendColor();
			}
			return true;
		}

		public void Reset()
		{
			_buttonAppearance = ButtonAppearance.Light3D;
			_hoverBlend = 0.667f;
			_hoverSelectedBlend = 0.5f;
			ResetControlColor();
			ResetControlTextColor();
			ResetHoverBaseColor();
			ResetScrollBarColor();
			ResetWindowColor();
			ResetWindowTextColor();
			ResetHoverBlendColor();
		}

		private void ValidateOpaqueColor(Color color)
		{
			if (color.IsEmpty || color.A != byte.MaxValue)
			{
				throw new ArgumentException();
			}
		}

		internal Color GetFlatBorderColor()
		{
			return _flatBorderColor;
		}

		internal void SetFlatBorderColor(Color color)
		{
			if (_flatBorderColor != color)
			{
				_flatBorderColor = color;
				if (_flatBorderPen != null)
				{
					_flatBorderPen.Dispose();
					_flatBorderPen = null;
					NotifyChanged();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlColor()
		{
			ControlColor = SystemColors.Control;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlColor()
		{
			return !_controlColor.Equals(SystemColors.Control);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetControlTextColor()
		{
			ControlTextColor = SystemColors.ControlText;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeControlTextColor()
		{
			return !_controlTextColor.Equals(SystemColors.ControlText);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetScrollBarColor()
		{
			ScrollBarColor = SystemColors.ScrollBar;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeScrollBarColor()
		{
			return !_scrollBarColor.Equals(SystemColors.ScrollBar);
		}

		internal void SetControlLightColor(Color controlStyleColor)
		{
			ValidateOpaqueColor(controlStyleColor);
			if (_controlLightColor != controlStyleColor)
			{
				_controlLightColor = controlStyleColor;
				DisposeControlLightPen();
			}
		}

		internal void SetControlLightLightColor(Color color)
		{
			ValidateOpaqueColor(color);
			if (_controlLightLightColor != color)
			{
				_controlLightLightColor = color;
				DisposeControlLightLightResources();
			}
		}

		internal Color ResolveControlDarkColor()
		{
			if (!_classicLook && !ShouldSerializeControlColor())
			{
				return MxThemeManager.DefaultTheme.GetColor(ThemeRole.BorderStrong);
			}
			return _controlDarkColor;
		}

		internal void SetControlDarkColor(Color color)
		{
			ValidateOpaqueColor(color);
			if (_controlDarkColor != color)
			{
				_controlDarkColor = color;
				DisposeControlDarkPen();
			}
		}

		internal void SetControlDarkDarkColor(Color color)
		{
			ValidateOpaqueColor(color);
			if (_controlDarkDarkColor != color)
			{
				_controlDarkDarkColor = color;
				DisposeControlDarkDarkPen();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWindowColor()
		{
			WindowColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWindowColor()
		{
			return !_windowColor.Equals(SystemColors.Window);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWindowTextColor()
		{
			WindowTextColor = SystemColors.WindowText;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWindowTextColor()
		{
			return !_windowTextColor.Equals(SystemColors.WindowText);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBaseColor()
		{
			HoverBaseColor = SystemColors.Highlight;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBaseColor()
		{
			return !_hoverBaseColor.Equals(SystemColors.Highlight);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHoverBlendColor()
		{
			_hoverBlendColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHoverBlendColor()
		{
			return !HoverBlendColor.Equals(SystemColors.Window);
		}

		private static Color BlendColors(Color color, Color otherColor, float floatValue)
		{
			int red = (int)((float)(int)color.R * floatValue + (float)(int)otherColor.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)otherColor.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)otherColor.B * (1f - floatValue));
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
			if (_hoverPen != null)
			{
				_hoverPen.Dispose();
				_hoverPen = null;
			}
			NotifyChanged();
		}

		internal bool IsFlatAppearance()
		{
			if (_buttonAppearance == ButtonAppearance.Flat || _buttonAppearance == ButtonAppearance.FlatBorderless)
			{
				return true;
			}
			return false;
		}

		internal bool UsesHoverEffect(bool condition)
		{
			if (condition)
			{
				return true;
			}
			if (_buttonAppearance == ButtonAppearance.Flat || _buttonAppearance == ButtonAppearance.FlatBorderless || _buttonAppearance == ButtonAppearance.PopUp)
			{
				return true;
			}
			return false;
		}

		internal ControlStyleLayoutState GetLayoutState()
		{
			if (_controlStyleLayoutState == null)
			{
				_controlStyleLayoutState = new ControlStyleLayoutState(this);
			}
			return _controlStyleLayoutState;
		}

		// Printing and the standalone option button keep the system colors.
		internal void UseClassicLook()
		{
			if (!_classicLook)
			{
				_classicLook = true;
				ApplyThemeChange();
			}
		}

		bool IGridEnvironment.IsPrinting()
		{
			return _printDocument is PrintDocument;
		}
	}
}
