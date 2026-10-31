using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Controls
{
	[DefaultEvent("CheckedChanged")]
	[ToolboxItem(true)]
	[DefaultBindingProperty("BindableValue")]
	public class MxRadioButton : CheckBase
	{
		private const int DotInset = 3;

		private bool _canCheckOnFocus = true;

		private bool _isChecked;

		private object _checkedValue;

		private object _bindableValue = false;

		private static readonly object _checkedChangedKey = new object();

		private static readonly object _bindableValueChangedKey = new object();

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(false)]
		[Browsable(false)]
		public new bool TabStop
		{
			get
			{
				if (base.DesignMode)
				{
					return base.TabStop;
				}
				if (Checked)
				{
					return base.TabStop;
				}
				return false;
			}
			set
			{
				base.TabStop = value;
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the radio button is checked or not")]
		[Bindable(BindableSupport.Yes)]
		public bool Checked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				if (_isChecked == value)
				{
					return;
				}
				if (value)
				{
					MxRadioButton uiRadioButton = FindCheckedSibling();
					_isChecked = value;
					if (uiRadioButton != null)
					{
						uiRadioButton.Checked = false;
					}
				}
				else
				{
					_isChecked = value;
				}
				if (_isChecked)
				{
					TabStop = true;
				}
				else
				{
					TabStop = false;
				}
				OnCheckedChanged(EventArgs.Empty);
				BindableValue = BindableValue;
				AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
				Invalidate();
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the value that will be consider as the checked value.")]
		public object CheckedValue
		{
			get
			{
				return _checkedValue;
			}
			set
			{
				if (_checkedValue != value)
				{
					_checkedValue = value;
					SyncCheckedFromBindableValue();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable(BindableSupport.Yes)]
		[Description("An object that represents a value, or null (Nothing in Visual Basic), associated with the control.")]
		public object BindableValue
		{
			get
			{
				if (Checked)
				{
					if (_checkedValue != null)
					{
						return _checkedValue;
					}
				}
				else
				{
					MxRadioButton uiRadioButton = FindCheckedSibling();
					if (uiRadioButton != null && uiRadioButton.CheckedValue != null)
					{
						return uiRadioButton.CheckedValue;
					}
				}
				return _bindableValue;
			}
			set
			{
				if (RenderingHelper.CompareValuesInvariantCulture(value, _bindableValue, null, true) != 0)
				{
					_bindableValue = value;
					_ = Checked;
					SyncCheckedFromBindableValue();
					OnBindableValueChanged(EventArgs.Empty);
					SetSiblingBindableValues(_bindableValue);
				}
			}
		}

		[Description("Event fired when the Checked property has changed.")]
		public event EventHandler CheckedChanged
		{
			add
			{
				base.Events.AddHandler(_checkedChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_checkedChangedKey, value);
			}
		}

		[Description("Event fired when the BindableValue property has changed.")]
		public event EventHandler BindableValueChanged
		{
			add
			{
				base.Events.AddHandler(_bindableValueChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_bindableValueChangedKey, value);
			}
		}

		public MxRadioButton()
		{
			SetStyle(ControlStyles.Selectable, value: true);
			TabStop = false;
		}

		private void SyncCheckedFromBindableValue()
		{
			if (_checkedValue != null)
			{
				if (RenderingHelper.CompareValuesInvariantCulture(_bindableValue, _checkedValue, null, true) == 0)
				{
					Checked = true;
					return;
				}
			}
			else if (RenderingHelper.CompareValuesInvariantCulture(_bindableValue, true, null, true) == 0)
			{
				Checked = true;
				return;
			}
			Checked = false;
		}

		private MxRadioButton FindCheckedSibling()
		{
			Control control = RenderingHelper.GetParentControl(this);
			if (control != null)
			{
				foreach (Control sibling in control.Controls)
				{
					if (sibling is MxRadioButton)
					{
						MxRadioButton uiRadioButton = (MxRadioButton)sibling;
						if (uiRadioButton.Checked)
						{
							return uiRadioButton;
						}
					}
				}
			}
			return null;
		}

		private void SetSiblingBindableValues(object value)
		{
			Control control = RenderingHelper.GetParentControl(this);
			if (control == null)
			{
				return;
			}
			foreach (Control sibling in control.Controls)
			{
				if (sibling is MxRadioButton)
				{
					MxRadioButton uiRadioButton = (MxRadioButton)sibling;
					uiRadioButton.BindableValue = value;
				}
			}
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible && Control.IsMnemonic(charCode, Text))
			{
				Focus();
				Checked = true;
				return true;
			}
			return base.ProcessMnemonic(charCode);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			SetIsPressed(false);
		}

		protected override void OnClick(EventArgs e)
		{
			if (base.AutoCheck && GetIsPressed())
			{
				Checked = true;
			}
			base.OnClick(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			_canCheckOnFocus = false;
			base.OnMouseDown(e);
			_canCheckOnFocus = true;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (_canCheckOnFocus && base.AutoCheck && base.Enabled)
			{
				Checked = true;
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new RadioButtonAccessibleObject(this);
		}

		public new void Focus()
		{
			base.Focus();
			if (_canCheckOnFocus && base.AutoCheck && base.Enabled)
			{
				Checked = true;
			}
		}

		protected override bool ProcessDialogKey(Keys key)
		{
			switch (key)
			{
			case Keys.Left:
			case Keys.Up:
			{
				Control parentControl = RenderingHelper.GetParentControl(this);
				Control previousControl = parentControl.GetNextControl(this, forward: false);
				if (previousControl != null && previousControl is MxRadioButton && previousControl.Enabled && ((MxRadioButton)previousControl).AutoCheck)
				{
					((MxRadioButton)previousControl).Checked = true;
				}
				break;
			}
			case Keys.Right:
			case Keys.Down:
			{
				Control control = RenderingHelper.GetParentControl(this);
				Control nextControl = control.GetNextControl(this, forward: true);
				if (nextControl != null && nextControl is MxRadioButton && ((MxRadioButton)nextControl).AutoCheck && nextControl.Enabled)
				{
					((MxRadioButton)nextControl).Checked = true;
				}
				break;
			}
			}
			return base.ProcessDialogKey(key);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			TextLayoutRenderer textLayoutRenderer = CreateTextLayoutRenderer();
			FormatStyle uiFormatStyle = CreateFormatStyle();
			uiFormatStyle = MergePaintStyle(uiFormatStyle);
			if (_isLayoutDirty)
			{
				LayoutContent(e.Graphics, uiFormatStyle, textLayoutRenderer);
			}
			if (base.Appearance == System.Windows.Forms.Appearance.Normal)
			{
				PaintThemed(e.Graphics, uiFormatStyle, true, Checked, false);
				uiFormatStyle.DisposeResources();
				textLayoutRenderer.ResetCache();
				base.OnPaint(e);
				return;
			}
			if ((VisualStyleSupport.IsThemingUsable(false)))
			{
				_themeRenderer = new VisualStylePainter("Button");
			}
			if (base.Appearance == System.Windows.Forms.Appearance.Normal)
			{
				DrawRadioIndicator(e.Graphics, uiFormatStyle, e.ClipRectangle);
			}
			else
			{
				PaintCheckBackground(e.Graphics, uiFormatStyle, e.ClipRectangle, Checked, false);
			}
			PaintContent(e.Graphics, uiFormatStyle, textLayoutRenderer);
			if (_themeRenderer != null)
			{
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			uiFormatStyle.DisposeResources();
			textLayoutRenderer.ResetCache();
			base.OnPaint(e);
		}

		private void DrawRadioIndicator(Graphics graphics, FormatStyle uiFormatStyle, Rectangle indicatorBounds)
		{
			if (_themeRenderer != null)
			{
				_themeRenderer.DrawRadioButtonTheme(graphics, _glyphBounds, GetIsHot(), GetIsPressed() && GetIsHot(), Checked, base.Enabled);
				return;
			}
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, indicatorBounds, base.ClientRectangle);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			Brush brush = null;
			Pen pen = null;
			Color color = Color.Black;
			if (!base.Enabled)
			{
				brush = GdiCache.Brush(RenderingHelper.GetSchemeColor(ColorName.Control));
				color = RenderingHelper.GetSchemeColor(ColorName.GrayText);
				pen = GdiCache.Pen(color);
			}
			else if (GetIsHot())
			{
				brush = ((!GetIsPressed()) ? GdiCache.Brush(_windowsAppearanceState._hotBackColor) : GdiCache.Brush(_windowsAppearanceState._pressedBackColor));
				pen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.Highlight));
			}
			else
			{
				brush = GdiCache.Brush(RenderingHelper.GetSchemeColor(ColorName.Window));
				pen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.ControlDark));
			}
			Rectangle rectangle = _glyphBounds;
			rectangle.Height -= 2;
			rectangle.Width -= 2;
			graphics.FillEllipse(brush, rectangle);
			graphics.DrawEllipse(pen, rectangle);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (Checked)
			{
				Rectangle rect = rectangle;
				rect.Inflate(-DotInset, -DotInset);
				Brush dotBrush = GdiCache.Brush(color);
				graphics.FillEllipse(dotBrush, rect);
			}
			graphics.SmoothingMode = smoothingMode;
		}

		private FormatStyle CreateFormatStyle()
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = GetEffectiveBackColor();
			if (base.Enabled)
			{
				uiFormatStyle.ForeColor = GetEffectiveForeColor();
			}
			else {
				uiFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			}
			uiFormatStyle.BackgroundImage = BackgroundImage;
			return uiFormatStyle;
		}

		protected virtual void OnCheckedChanged(EventArgs e)
		{
			if (base.Events[_checkedChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnBindableValueChanged(EventArgs e)
		{
			if (base.Events[_bindableValueChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
