using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation
{
	internal class ColorSchemeRenderer
	{
		private Color _baseColor = Color.Empty;

		private Brush _baseBrush;

		private Color _lightColor = SystemColors.ControlLight;

		private Pen _lightPen;

		private Color _lightLightColor = SystemColors.ControlLightLight;

		private Pen _lightLightPen;

		private Brush _lightLightBrush;

		private Color _textColor = Color.Empty;

		private Pen _textPen;

		private Color _darkColor = SystemColors.ControlDark;

		private Pen _darkPen;

		private Color _darkDarkColor = SystemColors.ControlDarkDark;

		private Pen _darkDarkPen;

		private Color _flatBorderColor = Color.Empty;

		private Pen _flatBorderPen;

		internal ColorSchemeRenderer()
		{
		}

		public Color GetBaseColor()
		{
			if (_baseColor.IsEmpty)
			{
				return SystemColors.Control;
			}
			return _baseColor;
		}

		public void SetBaseColor(Color baseColor)
		{
			if (baseColor != _baseColor)
			{
				_baseColor = baseColor;
				ResetBaseBrushAndShades();
			}
		}

		private void ResetBaseBrushAndShades()
		{
			if (_baseBrush != null)
			{
				_baseBrush.Dispose();
				_baseBrush = null;
			}
			SetDarkColor(ControlPaint.Dark(GetBaseColor(), 0f));
			SetDarkDarkColor(ControlPaint.DarkDark(GetBaseColor()));
			SetLightColor(ControlPaint.Light(GetBaseColor(), 0f));
			SetLightLightColor(ControlPaint.LightLight(GetBaseColor()));
		}

		internal Brush GetBaseBrush()
		{
			if (_baseBrush == null)
			{
				_baseBrush = new SolidBrush(GetBaseColor());
			}
			return _baseBrush;
		}

		private void SetLightColor(Color lightColor)
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
		}

		internal void SetLightLightColor(Color lightLightColor)
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
		}

		public Color GetTextColor()
		{
			if (_textColor.IsEmpty)
			{
				return SystemColors.ControlText;
			}
			return _textColor;
		}

		public void SetTextColor(Color textColor)
		{
			if (_textColor != textColor)
			{
				_textColor = textColor;
				DisposeTextPen();
			}
		}

		private void DisposeTextPen()
		{
			if (_textPen != null)
			{
				_textPen.Dispose();
				_textPen = null;
			}
		}

		internal Pen GetTextPen()
		{
			if (_textPen == null)
			{
				_textPen = new Pen(GetTextColor());
			}
			return _textPen;
		}

		private void SetDarkColor(Color darkColor)
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
		}

		private void SetDarkDarkColor(Color darkDarkColor)
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
		}

		internal Color GetFlatBorderColor()
		{
			return _flatBorderColor;
		}

		internal void SetFlatBorderColor(Color flatBorderColor)
		{
			if (_flatBorderColor != flatBorderColor)
			{
				_flatBorderColor = flatBorderColor;
				if (_flatBorderPen != null)
				{
					_flatBorderPen.Dispose();
					_flatBorderPen = null;
				}
			}
		}

		internal Pen GetFlatBorderPen()
		{
			if (_flatBorderPen == null)
			{
				_flatBorderPen = new Pen(GetFlatBorderColor());
			}
			return _flatBorderPen;
		}

		public Rectangle DrawBorder(Graphics graphics, ColorSchemeRenderer mordexColorSchemeRenderer, Rectangle bounds, newBorderStyle newBorderStyle)
		{
			if (mordexColorSchemeRenderer == null)
			{
				return bounds;
			}
			Pen pen = null;
			switch (newBorderStyle)
			{
			case newBorderStyle.Flat:
				if (_flatBorderColor != Color.Empty)
				{
					return NavigationGraphicsHelper.DrawBorderEdges(graphics, GetFlatBorderPen(), bounds, BorderFlags.All);
				}
				return NavigationGraphicsHelper.DrawBorderEdges(graphics, mordexColorSchemeRenderer.GetDarkDarkPen(), bounds, BorderFlags.All);
			case newBorderStyle.RaisedLight3D:
			case newBorderStyle.Raised:
				pen = mordexColorSchemeRenderer.GetLightLightPen();
				break;
			case newBorderStyle.SunkenLight3D:
				pen = mordexColorSchemeRenderer.GetDarkPen();
				break;
			case newBorderStyle.Sunken:
				pen = mordexColorSchemeRenderer.GetDarkPen();
				break;
			}
			if (pen == null)
			{
				return bounds;
			}
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
			graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
			switch (newBorderStyle)
			{
			case newBorderStyle.Raised:
				pen = mordexColorSchemeRenderer.GetDarkDarkPen();
				break;
			case newBorderStyle.RaisedLight3D:
				pen = mordexColorSchemeRenderer.GetDarkPen();
				break;
			case newBorderStyle.SunkenLight3D:
				pen = mordexColorSchemeRenderer.GetLightLightPen();
				break;
			case newBorderStyle.Sunken:
				pen = mordexColorSchemeRenderer.GetLightLightPen();
				break;
			}
			graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			if (newBorderStyle == newBorderStyle.Raised || newBorderStyle == newBorderStyle.Sunken)
			{
				bounds.Inflate(-1, -1);
				pen = ((newBorderStyle != newBorderStyle.Raised) ? mordexColorSchemeRenderer.GetDarkDarkPen() : mordexColorSchemeRenderer.GetLightPen());
				if (pen == null)
				{
					return bounds;
				}
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Left, bounds.Top);
				graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right - 1, bounds.Top);
				pen = ((newBorderStyle != newBorderStyle.Raised) ? mordexColorSchemeRenderer.GetLightPen() : mordexColorSchemeRenderer.GetDarkPen());
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
				graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			}
			if (newBorderStyle != newBorderStyle.None)
			{
				bounds.Inflate(-1, -1);
			}
			return bounds;
		}

		public void DisposeAll()
		{
			if (_baseBrush != null)
			{
				_baseBrush.Dispose();
				_baseBrush = null;
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
			if (_textPen != null)
			{
				_textPen.Dispose();
				_textPen = null;
			}
			if (_flatBorderPen != null)
			{
				_flatBorderPen.Dispose();
				_flatBorderPen = null;
			}
		}

		internal void DrawVerticalScrollArrow(Graphics graphics, Rectangle buttonBounds, int direction, bool isPressed, bool isHot, bool isEnabled, GroupAppearance groupAppearance)
		{
			if (isPressed && groupAppearance != GroupAppearance.Flat)
			{
				buttonBounds.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (groupAppearance == GroupAppearance.Flat)
				{
					DrawUpDownTriangle(graphics, buttonBounds.Width, buttonBounds, direction, GetDarkPen());
					return;
				}
				buttonBounds.Offset(1, 1);
				DrawUpDownTriangle(graphics, buttonBounds.Width, buttonBounds, direction, GetLightLightPen());
				buttonBounds.Offset(-1, -1);
				DrawUpDownTriangle(graphics, buttonBounds.Width, buttonBounds, direction, GetDarkPen());
			}
			else
			{
				DrawUpDownTriangle(graphics, buttonBounds.Width, buttonBounds, direction, GetTextPen());
			}
		}

		internal void DrawHorizontalScrollArrow(Graphics graphics, Rectangle buttonBounds, int direction, bool isPressed, bool isHot, bool isEnabled, GroupAppearance groupAppearance)
		{
			if (isPressed && groupAppearance != GroupAppearance.Flat)
			{
				buttonBounds.Offset(1, 1);
			}
			if (!isEnabled)
			{
				if (groupAppearance == GroupAppearance.Flat)
				{
					DrawLeftRightTriangle(graphics, buttonBounds.Height, buttonBounds, direction, GetDarkPen());
					return;
				}
				buttonBounds.Offset(1, 1);
				DrawLeftRightTriangle(graphics, buttonBounds.Height, buttonBounds, direction, GetLightLightPen());
				buttonBounds.Offset(-1, -1);
				DrawLeftRightTriangle(graphics, buttonBounds.Height, buttonBounds, direction, GetDarkPen());
			}
			else
			{
				DrawLeftRightTriangle(graphics, buttonBounds.Height, buttonBounds, direction, GetTextPen());
			}
		}

		private void DrawUpDownTriangle(Graphics graphics, int arrowSize, Rectangle bounds, int direction, Pen pen)
		{
			int rowLength = arrowSize / 2;
			if (rowLength % 2 == 0)
			{
				rowLength--;
			}
			int arrowDepth = rowLength / 2 + 1;
			int rowY = ((direction <= 0) ? (bounds.Top + (bounds.Height - arrowDepth) / 2 + arrowDepth - 1) : (bounds.Top + (bounds.Height - arrowDepth) / 2));
			int rowX = bounds.Left + (bounds.Width - rowLength) / 2;
			while (rowLength >= 0)
			{
				if (rowLength == 1)
				{
					graphics.DrawLine(pen, rowX, rowY - direction, rowX, rowY);
				}
				else
				{
					graphics.DrawLine(pen, rowX, rowY, rowX + rowLength - 1, rowY);
				}
				rowX++;
				rowLength -= 2;
				rowY += direction;
			}
		}

		private void DrawLeftRightTriangle(Graphics graphics, int arrowSize, Rectangle bounds, int direction, Pen pen)
		{
			int columnLength = arrowSize / 2;
			if (columnLength % 2 == 0)
			{
				columnLength--;
			}
			int arrowDepth = columnLength / 2 + 1;
			int columnX = ((direction <= 0) ? (bounds.Left + (bounds.Width - arrowDepth) / 2 + arrowDepth - 1) : (bounds.Left + (bounds.Width - arrowDepth) / 2));
			int columnY = bounds.Top + (bounds.Height - columnLength) / 2;
			while (columnLength >= 0)
			{
				if (columnLength == 1)
				{
					graphics.DrawLine(pen, columnX - direction, columnY, columnX, columnY);
				}
				else
				{
					graphics.DrawLine(pen, columnX, columnY, columnX, columnY + columnLength - 1);
				}
				columnX += direction;
				columnLength -= 2;
				columnY++;
			}
		}
	}
}
