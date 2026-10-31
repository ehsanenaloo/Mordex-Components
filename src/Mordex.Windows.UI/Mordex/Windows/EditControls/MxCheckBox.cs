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
	[DefaultBindingProperty("BindableValue")]
	[DefaultEvent("CheckedChanged")]
	[ToolboxItem(true)]
	public class MxCheckBox : CheckBase
	{
		private Container _components;

		private CheckState _checkState;

		private bool _isChecked;

		private object _checkedBindingValue;

		private object _uncheckedBindingValue;

		private object _indeterminateBindingValue;

		private object _bindableValue;

		private bool _threeState;

		private static readonly object _bindableValueChangedEvent = new object();

		private static readonly object _checkedChangedEvent = new object();

		private static readonly object _checkStateChangedEvent = new object();

private const int CheckMarkInset = 2;

private const int IndeterminateInset = 3;

private const int CheckMarkTopOffset = 2;

private const int CheckMarkLineHeight = 2;

private const int CheckMarkDescendColumns = 2;

		[Bindable(BindableSupport.Yes)]
		[Description("Indicates the check state of the check box")]
		[DefaultValue(CheckState.Unchecked)]
		public CheckState CheckState
		{
			get
			{
				return _checkState;
			}
			set
			{
				if (_checkState != value)
				{
					_checkState = value;
					OnBindableValueChanged(EventArgs.Empty);
					OnCheckStateChanged(EventArgs.Empty);
					if (_checkState == CheckState.Checked)
					{
						Checked = true;
					}
					else if (_checkState == CheckState.Unchecked)
					{
						Checked = false;
					}
					AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
					Invalidate();
				}
			}
		}

		[Description("Indicates whether the check box is checked or unchecked")]
		[DefaultValue(false)]
		[Bindable(BindableSupport.Yes)]
		public bool Checked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					if (_isChecked)
					{
						CheckState = CheckState.Checked;
					}
					else
					{
						CheckState = CheckState.Unchecked;
					}
					OnCheckedChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the value that will be consider as the checked value.")]
		public object CheckedValue
		{
			get
			{
				return _checkedBindingValue;
			}
			set
			{
				if (_checkedBindingValue != value)
				{
					_checkedBindingValue = value;
					if (_bindableValue != null)
					{
						UpdateCheckStateFromBindableValue();
					}
				}
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		[Description("Represents the value that will be consider as the unchecked value.")]
		[DefaultValue(null)]
		public object UncheckedValue
		{
			get
			{
				return _uncheckedBindingValue;
			}
			set
			{
				if (_uncheckedBindingValue != value)
				{
					_uncheckedBindingValue = value;
					if (_bindableValue != null)
					{
						UpdateCheckStateFromBindableValue();
					}
				}
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the value that will be consider as the indeterminated value.")]
		public object IndeterminatedValue
		{
			get
			{
				return _indeterminateBindingValue;
			}
			set
			{
				if (_indeterminateBindingValue != value)
				{
					_indeterminateBindingValue = value;
					if (_bindableValue != null)
					{
						UpdateCheckStateFromBindableValue();
					}
				}
			}
		}

		[Bindable(BindableSupport.Yes)]
		[Browsable(false)]
		[Description("An object that represents a value, or null (Nothing in Visual Basic), associated with the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object BindableValue
		{
			get
			{
				switch (CheckState)
				{
				case CheckState.Checked:
					if (_checkedBindingValue != null)
					{
						return _checkedBindingValue;
					}
					return true;
				case CheckState.Unchecked:
					if (_uncheckedBindingValue != null)
					{
						return _uncheckedBindingValue;
					}
					return false;
				default:
					if (_indeterminateBindingValue != null)
					{
						return _indeterminateBindingValue;
					}
					return DBNull.Value;
				}
			}
			set
			{
				if (value != _bindableValue)
				{
					_bindableValue = value;
					UpdateCheckStateFromBindableValue();
				}
			}
		}

		[Description("Indicates whether the check box will allow three check states rather than two.")]
		[DefaultValue(false)]
		public bool ThreeState
		{
			get
			{
				return _threeState;
			}
			set
			{
				_threeState = value;
			}
		}

		[Description("Event fired when the BindableValue property has changed.")]
		public event EventHandler BindableValueChanged
		{
			add
			{
				base.Events.AddHandler(_bindableValueChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_bindableValueChangedEvent, value);
			}
		}

		[Description("Event fired when the Checked property has changed.")]
		public event EventHandler CheckedChanged
		{
			add
			{
				base.Events.AddHandler(_checkedChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_checkedChangedEvent, value);
			}
		}

		[Description("Event fired when the CheckState property has changed.")]
		public event EventHandler CheckStateChanged
		{
			add
			{
				base.Events.AddHandler(_checkStateChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_checkStateChangedEvent, value);
			}
		}

		public MxCheckBox()
		{
			CreateComponents();
		}

		private void CreateComponents()
		{
			_components = new Container();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void UpdateCheckStateFromBindableValue()
		{
			if (_bindableValue == null)
			{
				CheckState = CheckState.Indeterminate;
				return;
			}
			if (_checkedBindingValue != null)
			{
				if (RenderingHelper.CompareValuesInvariantCulture(_bindableValue, _checkedBindingValue, null, true) == 0)
				{
					CheckState = CheckState.Checked;
					return;
				}
			}
			else if (RenderingHelper.CompareValuesInvariantCulture(_bindableValue, true, null, true) == 0)
			{
				CheckState = CheckState.Checked;
				return;
			}
			if (_uncheckedBindingValue != null)
			{
				if (RenderingHelper.CompareValuesInvariantCulture(_bindableValue, _uncheckedBindingValue, null, true) == 0)
				{
					CheckState = CheckState.Unchecked;
					return;
				}
			}
			else if (RenderingHelper.CompareValuesInvariantCulture(_bindableValue, false, null, true) == 0)
			{
				CheckState = CheckState.Unchecked;
				return;
			}
			CheckState = CheckState.Indeterminate;
		}

		private void ToggleCheckState()
		{
			if (ThreeState)
			{
				switch (_checkState)
				{
				case CheckState.Checked:
					CheckState = CheckState.Indeterminate;
					break;
				case CheckState.Indeterminate:
					CheckState = CheckState.Unchecked;
					break;
				case CheckState.Unchecked:
					CheckState = CheckState.Checked;
					break;
				}
			}
			else if (Checked)
			{
				CheckState = CheckState.Unchecked;
			}
			else
			{
				CheckState = CheckState.Checked;
			}
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible && Control.IsMnemonic(charCode, Text))
			{
				Focus();
				ToggleCheckState();
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
			if (base.AutoCheck)
			{
				ToggleCheckState();
			}
			base.OnClick(e);
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			if (base.AutoCheck)
			{
				ToggleCheckState();
			}
			base.OnDoubleClick(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled && base.AutoCheck && e.KeyData == Keys.Space)
			{
				SetIsPressed(true);
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new CheckBoxAccessibleObject(this);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (!e.Handled && base.AutoCheck && e.KeyData == Keys.Space)
			{
				ToggleCheckState();
				SetIsPressed(false);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			TextLayoutRenderer textLayoutRenderer = CreateTextLayoutRenderer();
			FormatStyle uiFormatStyle = BuildTextFormatStyle();
			uiFormatStyle = MergePaintStyle(uiFormatStyle);
			if (_isLayoutDirty)
			{
				LayoutContent(e.Graphics, uiFormatStyle, textLayoutRenderer);
			}
			if (base.Appearance == System.Windows.Forms.Appearance.Normal)
			{
				PaintThemed(e.Graphics, uiFormatStyle, false, Checked, CheckState == CheckState.Indeterminate);
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
				DrawCheckBoxGlyph(e.Graphics, uiFormatStyle, e.ClipRectangle);
			}
			else
			{
				PaintCheckBackground(e.Graphics, uiFormatStyle, e.ClipRectangle, Checked, CheckState == CheckState.Indeterminate);
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

		private void DrawCheckBoxGlyph(Graphics graphics, FormatStyle uiFormatStyle, Rectangle clipRectangle)
		{
			if (_themeRenderer != null)
			{
				_themeRenderer.DrawCheckBoxTheme(graphics, _glyphBounds, GetIsHot(), GetIsPressed() && GetIsHot(), _checkState, base.Enabled);
				return;
			}
			Color color = RenderingHelper.GetSchemeColor(ColorName.Window);
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, clipRectangle, base.ClientRectangle);
			Brush brush = null;
			Color empty = Color.Empty;
			if (!base.Enabled)
			{
				brush = new SolidBrush(RenderingHelper.GetSchemeColor(ColorName.Control));
				empty = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			}
			else if (GetIsHot())
			{
				brush = ((!GetIsPressed()) ? new SolidBrush(_windowsAppearanceState._hotBackColor) : new SolidBrush(_windowsAppearanceState._pressedBackColor));
				empty = RenderingHelper.GetSchemeColor(ColorName.Highlight);
			}
			else
			{
				brush = new SolidBrush(color);
				empty = RenderingHelper.GetSchemeColor(ColorName.ControlDark);
			}
			graphics.FillRectangle(brush, _glyphBounds);
			RenderingHelper.DrawColoredBorder(graphics, color, _glyphBounds, Mordex.Windows.UI.BorderStyle.SunkenLight3D);
			brush.Dispose();
			if (_checkState == CheckState.Checked)
			{
				if (_checkState == CheckState.Checked)
				{
					empty = SystemColors.WindowText;
				}
				Rectangle rectangle = _glyphBounds;
				rectangle.Inflate(-CheckMarkInset, -CheckMarkInset);
				Pen checkMarkPen = GdiCache.Pen(empty);
				DrawCheckMark(graphics, rectangle, checkMarkPen);
			}
			else if (_checkState == CheckState.Indeterminate)
			{
				Rectangle rect = _glyphBounds;
				rect.Inflate(-IndeterminateInset, -IndeterminateInset);
				Brush indeterminateBrush = GdiCache.Brush(empty);
				graphics.FillRectangle(indeterminateBrush, rect);
			}
		}

		private void DrawCheckMark(Graphics graphics, Rectangle rectangle, Pen pen)
		{
			rectangle.Inflate(-1, -1);
			int y = rectangle.Top + CheckMarkTopOffset;
			int x = rectangle.Left;
			for (int i = 0; i < rectangle.Width; i++)
			{
				graphics.DrawLine(pen, x, y, x, y + CheckMarkLineHeight);
				y = ((i >= CheckMarkDescendColumns) ? (y - 1) : (y + 1));
				x++;
			}
		}

		private FormatStyle BuildTextFormatStyle()
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = GetEffectiveBackColor();
			bool isTextEnabled = base.Enabled;
			if (CheckState == CheckState.Indeterminate && base.Appearance == System.Windows.Forms.Appearance.Button)
			{
				isTextEnabled = false;
			}
			if (isTextEnabled)
			{
				uiFormatStyle.ForeColor = GetEffectiveForeColor();
			}
			else {
				uiFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			}
			uiFormatStyle.BackgroundImage = BackgroundImage;
			return uiFormatStyle;
		}

		protected virtual void OnBindableValueChanged(EventArgs e)
		{
			if (base.Events[_bindableValueChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCheckedChanged(EventArgs e)
		{
			if (base.Events[_checkedChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCheckStateChanged(EventArgs e)
		{
			if (base.Events[_checkStateChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
