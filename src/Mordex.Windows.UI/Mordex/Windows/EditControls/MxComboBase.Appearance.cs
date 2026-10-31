using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		private void InitializeControlAppearance()
		{
			_controlAppearance = new ControlAppearance(this);
			base.Size = new Size(DefaultWidth, DefaultHeight);
			BackColor = SystemColors.Window;
			ForeColor = SystemColors.WindowText;
		}

		internal ComboBoxThemeRenderer GetOrCreateComboThemeRenderer()
		{
			if (_comboThemeRenderer == null)
			{
				_comboThemeRenderer = new ComboBoxThemeRenderer();
			}
			return _comboThemeRenderer;
		}

		internal TextLayoutRenderer GetOrCreateTextLayoutRenderer()
		{
			if (_textLayoutRenderer == null)
			{
				_textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				switch (TextAlignment)
				{
				case TextAlignment.Center:
					_textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
					break;
				case TextAlignment.Empty:
				case TextAlignment.Near:
					_textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					break;
				case TextAlignment.Far:
					_textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
					break;
				}
				_textLayoutRenderer.SetShowHotkeyPrefix(false);
				_textLayoutRenderer.SetWordWrap(false);
				_textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				_textLayoutRenderer.SetRightToLeft(RightToLeft);
			}
			return _textLayoutRenderer;
		}

		private Mordex.Windows.UI.BorderStyle GetEffectiveBorderStyle()
		{
			if (BorderStyle == Mordex.Windows.UI.BorderStyle.None)
			{
				if (HoverMode == HoverMode.Highlight)
				{
					return Mordex.Windows.UI.BorderStyle.SunkenLight3D;
				}
				return BorderStyle;
			}
			return BorderStyle;
		}

		internal bool UsesNativeTheme()
		{
			if (_isWindows7OrLater)
			{
				if (VisualStyleSupport.IsThemingUsable(false))
				{
					return false;
				}
				return false;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return !ForeColor.Equals(SystemColors.WindowText);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetForeColor()
		{
			ForeColor = SystemColors.WindowText;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		internal Color GetEffectiveForeColor()
		{
			if (ForeColor == SystemColors.WindowText && IsThemed)
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		internal Color GetEffectiveBackColor()
		{
			if (BackColor == SystemColors.Window)
			{
				if (IsThemed)
				{
					return MxThemeManager.ResolveDefault(DeviceDpi).Color(base.Enabled ? ThemeRole.InputBack : ThemeRole.InputBackDisabled);
				}
				return RenderingHelper.GetSchemeColor(ColorName.Window);
			}
			return BackColor;
		}
	}
}
