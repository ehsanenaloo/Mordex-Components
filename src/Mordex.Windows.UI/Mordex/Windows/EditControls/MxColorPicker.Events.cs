using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxColorPicker
	{
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

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (_automaticButtonState != null)
			{
				if (_automaticButtonState._bounds.Contains(e.X, e.Y))
				{
					_isAutomaticSelected = true;
					OnAutomaticButtonClick(EventArgs.Empty);
					AutomaticColor = _automaticColor;
				}
				_automaticButtonState.SetPressed(false);
			}
			if (_moreColorsButtonState != null)
			{
				if (_moreColorsButtonState._bounds.Contains(e.X, e.Y))
				{
					OnMoreColorsButtonClick(EventArgs.Empty);
				}
				_moreColorsButtonState.SetPressed(false);
			}
			ColorEntry colorEntryAt = GetColorEntryAt(e.X, e.Y);
			if (colorEntryAt != null)
			{
				SelectedColor = colorEntryAt.Color;
			}
			SetHotEntry(colorEntryAt, e.Button);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			RefreshColorScheme();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			DisposeToolTip();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			if (_automaticButtonState != null)
			{
				if (_automaticButtonState._bounds.Contains(e.X, e.Y))
				{
					_automaticButtonState.SetPressed(true);
					_automaticButtonState.SetHot(true);
					return;
				}
				_automaticButtonState.SetHot(false);
				_automaticButtonState.SetPressed(false);
			}
			if (_moreColorsButtonState != null)
			{
				if (_moreColorsButtonState._bounds.Contains(e.X, e.Y))
				{
					_moreColorsButtonState.SetPressed(true);
					_moreColorsButtonState.SetHot(true);
				}
				else
				{
					_moreColorsButtonState.SetPressed(false);
					_moreColorsButtonState.SetHot(false);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_automaticButtonState != null)
			{
				if (_automaticButtonState._bounds.Contains(e.X, e.Y))
				{
					if (e.Button == MouseButtons.Left)
					{
						_automaticButtonState.SetPressed(true);
					}
					_automaticButtonState.SetHot(true);
					return;
				}
				_automaticButtonState.SetHot(false);
				_automaticButtonState.SetPressed(false);
			}
			if (_moreColorsButtonState != null)
			{
				if (_moreColorsButtonState._bounds.Contains(e.X, e.Y))
				{
					if (e.Button == MouseButtons.Left)
					{
						_moreColorsButtonState.SetPressed(true);
					}
					_moreColorsButtonState.SetHot(true);
					return;
				}
				_moreColorsButtonState.SetPressed(false);
				_moreColorsButtonState.SetHot(false);
			}
			SetHotEntry(GetColorEntryAt(e.X, e.Y), e.Button);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHotEntry(null, MouseButtons.None);
			if (_automaticButtonState != null)
			{
				_automaticButtonState.SetHot(false);
				_automaticButtonState.SetPressed(false);
			}
			if (_moreColorsButtonState != null)
			{
				_moreColorsButtonState.SetHot(false);
				_moreColorsButtonState.SetPressed(false);
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
