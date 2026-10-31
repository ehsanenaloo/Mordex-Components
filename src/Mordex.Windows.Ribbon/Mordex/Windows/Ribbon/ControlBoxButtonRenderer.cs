using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class ControlBoxButtonRenderer : CommandBaseRenderer, IDropDownItem
	{
		private bool _isMenuOpen;

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (base.Hot != value)
				{
					if (!value)
					{
						base.StartPressAnimation();
					}
					base.Hot = value;
					if (value)
					{
						ShowToolTip();
					}
					else
					{
						HideActiveSuperTip();
					}
				}
			}
		}

		public ControlBoxButtonRenderer(MxRibbon ribbon)
			: base(ribbon)
		{
			_commandBaseRendererRibbon = ribbon;
			_pressAnimationAlpha = 255;
		}

		public bool IsMenuOpen()
		{
			return _isMenuOpen;
		}

		public void SetMenuOpen(bool ribbonState2Boolean)
		{
			if (_isMenuOpen != ribbonState2Boolean)
			{
				_isMenuOpen = ribbonState2Boolean;
				Invalidate();
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (_commandBaseRendererRibbon.GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton)
			{
				if (_commandBaseRendererRibbon.GetHostForm() != null)
				{
					RibbonDrawingUtils.DrawIcon(graphics, _commandBaseRendererRibbon.ResolveControlBoxIcon(), _bounds, true, SystemColors.Window, _commandBaseRendererRibbon.IsGlassCaptionActive());
				}
				return;
			}
			bool isHighlighted = Hot || _fadeFromHot || Active;
			if (Pushed || IsMenuOpen())
			{
				RibbonChromePainter.DrawOrbPressed(graphics, _bounds, colorScheme);
			}
			else if (isHighlighted)
			{
				if (_pressAnimationAlpha != 255)
				{
					RibbonChromePainter.DrawOrbNormal(graphics, _bounds, colorScheme);
				}
				RibbonChromePainter.DrawOrbHot(graphics, _bounds, _commandBaseRendererRibbon._controlBoxButtonRenderer._pressAnimationAlpha, colorScheme);
			}
			else
			{
				RibbonChromePainter.DrawOrbNormal(graphics, _bounds, colorScheme);
			}
			if (_commandBaseRendererRibbon.GetHostForm() != null)
			{
				Rectangle rectangle = _bounds;
				rectangle.Inflate(-6, -6);
				rectangle.X++;
				rectangle.Y++;
				RibbonDrawingUtils.DrawIcon(graphics, _commandBaseRendererRibbon.ResolveControlBoxIcon(), rectangle, true, SystemColors.Window, _commandBaseRendererRibbon.IsGlassCaptionActive());
			}
		}

		public override void Invalidate()
		{
			if (_commandBaseRendererRibbon != null)
			{
				_commandBaseRendererRibbon.Invalidate(_commandBaseRendererRibbon._controlBoxButtonBounds);
				_commandBaseRendererRibbon.Update();
				if (_commandBaseRendererRibbon._headerRenderer != null)
				{
					_commandBaseRendererRibbon._headerRenderer.RefreshHeader(true);
				}
			}
		}

		internal void ShowToolTip()
		{
			if (_commandBaseRendererRibbon.ShowToolTips && !_commandBaseRendererRibbon.ControlBoxMenu.SuperTipSettings.IsEmpty)
			{
				_commandBaseRendererRibbon._mordexSuperTip.InitialDelay = 900;
				if (_commandBaseRendererRibbon.SuperTipDelay <= 0)
				{
					_commandBaseRendererRibbon._mordexSuperTip.ShowAlways = true;
				}
				else
				{
					_commandBaseRendererRibbon._mordexSuperTip.ShowAlways = false;
					_commandBaseRendererRibbon._mordexSuperTip.AutoPopDelay = _commandBaseRendererRibbon.SuperTipDelay;
				}
				_commandBaseRendererRibbon._mordexSuperTip.BodyWidth = _commandBaseRendererRibbon.SuperTipWidth;
				_commandBaseRendererRibbon._mordexSuperTip.Show(_commandBaseRendererRibbon.ControlBoxMenu.SuperTipSettings, _commandBaseRendererRibbon, Cursor.Position, _commandBaseRendererRibbon.SuperTipDelay);
			}
		}

		internal void HideActiveSuperTip()
		{
			if (_commandBaseRendererRibbon != null && !_commandBaseRendererRibbon.Disposing && !_commandBaseRendererRibbon.IsDisposed && !_commandBaseRendererRibbon.ControlBoxMenu.SuperTipSettings.IsEmpty)
			{
				_commandBaseRendererRibbon._mordexSuperTip.HideActiveToolTip();
			}
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int keyTipWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			Rectangle ribbonRectangle = _commandBaseRendererRibbon._controlBoxButtonBounds;
			int x = ribbonRectangle.X + (ribbonRectangle.Width - keyTipWidth) / 2;
			int y = ribbonRectangle.Y + ribbonRectangle.Height / 2 - 8;
			return new Point(x, y);
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			_commandBaseRendererRibbon.ShowControlBoxMenu();
		}
	}
}
