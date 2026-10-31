using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class GridColumnControl : Control
	{
		internal class CheckBoxCellAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.CheckButton;

			public override string Name
			{
				get
				{
					return "CheckBox Cell Editor";
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			public override string DefaultAction
			{
				get
				{
					GridColumnControl gridColumnControl = GetCheckBoxControl();
					if (gridColumnControl.ComputeCheckState() == CheckState.Checked)
					{
						return "Check";
					}
					return "Uncheck";
				}
			}

			public override AccessibleStates State
			{
				get
				{
					switch (GetCheckBoxControl().ComputeCheckState())
					{
					case CheckState.Checked:
						return AccessibleStates.Checked | base.State;
					case CheckState.Indeterminate:
						return AccessibleStates.Mixed | base.State;
					default:
						return base.State;
					}
				}
			}

			internal CheckBoxCellAccessibleObject(GridColumnControl owner)
				: base(owner)
			{
			}

			private GridColumnControl GetCheckBoxControl()
			{
				return (GridColumnControl)base.Owner;
			}

			public override void DoDefaultAction()
			{
				GetCheckBoxControl().CycleValue();
			}
		}

		private GridColumn _column;

		private object _checkState;

		private EventHandler _valueChangedHandler;

		private bool _isMouseCaptured;

		private bool _isHot;

		private bool _isPressed;

		private Rectangle _checkBoxBounds;

		private bool _isCentered = true;

		private bool _isThreeState;

		private object _gridColumnControlValue;

		private object _checkedValue = true;

		private object _uncheckedValue = false;

		private ControlStyle _style;

		internal bool _isRowSelector;


		internal GridColumnControl(GridColumn column)
		{
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			_column = column;
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			PaintCheckBox(pe.Graphics, _checkState);
		}

		protected override void OnClick(EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
		}

		protected override void OnMouseClick(MouseEventArgs eventArgs)
		{
			base.OnMouseClick(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				CycleValue();
			}
		}

		protected override void OnDoubleClick(EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
			CycleValue();
		}

		protected override void OnKeyPress(KeyPressEventArgs eventArgs)
		{
			base.OnKeyPress(eventArgs);
			if (!eventArgs.Handled && char.IsSeparator(eventArgs.KeyChar))
			{
				CycleValue();
			}
		}

		protected virtual void OnReturnKey(EventArgs eventArgs)
		{
		}

		public object GetStoredValue()
		{
			return _checkState;
		}

		public void SetCheckState(object value)
		{
			if (GridHelper.CompareValues(value, _checkState, null, true) == 0)
			{
				return;
			}
			if (_isThreeState)
			{
				if (IsNullValue(value) || IsCheckedValue(value) || IsUncheckedValue(value))
				{
					_checkState = value;
				}
				else
				{
					_checkState = _gridColumnControlValue;
				}
			}
			else if (IsUncheckedValue(value) || IsCheckedValue(value))
			{
				_checkState = value;
			}
			else
			{
				_checkState = _uncheckedValue;
			}
			Invalidate();
			OnChange(EventArgs.Empty);
		}

		internal void SetValueFromText(string strB)
		{
			if (string.IsNullOrEmpty(strB) || string.CompareOrdinal("{Null}", strB) == 0)
			{
				SetCheckState(_gridColumnControlValue);
			}
			else if (string.CompareOrdinal("True", strB) == 0)
			{
				SetCheckState(_checkedValue);
			}
			else
			{
				SetCheckState(_uncheckedValue);
			}
		}

		public void AddValueChangedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _valueChangedHandler;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _valueChangedHandler, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		public void RemoveValueChangedHandler(EventHandler otherValue)
		{
			EventHandler eventHandler = _valueChangedHandler;
			EventHandler otherEventHandler;
			do
			{
				otherEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(otherEventHandler, otherValue);
				eventHandler = Interlocked.CompareExchange(ref _valueChangedHandler, value, otherEventHandler);
			}
			while ((object)eventHandler != otherEventHandler);
		}

		protected virtual void OnChange(EventArgs eventArgs)
		{
			AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
			if (_valueChangedHandler != null)
			{
				_valueChangedHandler(this, eventArgs);
			}
		}

		public void InitializeValue(object value)
		{
			if (_isThreeState)
			{
				if (IsNullValue(value) || IsCheckedValue(value) || IsUncheckedValue(value))
				{
					_checkState = value;
				}
				else
				{
					_checkState = _gridColumnControlValue;
				}
			}
			else if (IsUncheckedValue(value) || IsCheckedValue(value))
			{
				_checkState = value;
			}
			else
			{
				_checkState = _uncheckedValue;
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			Rectangle clientRectangle = base.ClientRectangle;
			int width = 11;
			int height = 11;
			if (VisualStyleDrawer.IsThemingAvailable())
			{
				width += 2;
				height += 2;
			}
			if (_isCentered)
			{
				_checkBoxBounds = new Rectangle(clientRectangle.Left + (clientRectangle.Width - width) / 2, clientRectangle.Top + (clientRectangle.Height - height) / 2, width, height);
			}
			else
			{
				_checkBoxBounds = new Rectangle(clientRectangle.Left + 2, clientRectangle.Top + (clientRectangle.Height - height) / 2, width, height);
			}
			Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (eventArgs.Button == MouseButtons.Left && base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
			{
				_isMouseCaptured = true;
				SetPressed(true);
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_isMouseCaptured)
			{
				if (!base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					SetPressed(false);
				}
				else
				{
					SetPressed(true);
				}
			}
			else if (eventArgs.Button == MouseButtons.None)
			{
				if (base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					SetHot(true);
				}
				else
				{
					SetHot(false);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (_isMouseCaptured)
			{
				_isMouseCaptured = false;
				SetPressed(false);
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetHot(false);
		}

		private void SetHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				Invalidate(_checkBoxBounds);
			}
		}

		private void SetPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				Invalidate(_checkBoxBounds);
			}
		}

		private void PaintCheckBox(Graphics graphics, object value)
		{
			TriState triState = ((!IsNullValue(value)) ? ((!IsCheckedValue(value)) ? TriState.False : TriState.True) : TriState.Empty);
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _style.GetLayoutState().GetCheckBoxesStyle();
			if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				DrawGradientCheckBox(graphics, base.ClientRectangle, _column, mordexColorSchemeAppearanceState.GetResolvedAppearance(), triState, _isCentered, _isHot, _isPressed, _isRowSelector, base.Enabled);
			}
			else if (mordexColorSchemeAppearanceState._isThemed)
			{
				DrawClassicCheckBox(graphics, base.ClientRectangle, _column, triState, _isHot, _isPressed, _isCentered, _isRowSelector, base.Enabled);
			}
			else
			{
				DrawBackground(graphics, base.ClientRectangle, _column, triState, _isCentered, _isRowSelector, base.Enabled);
			}
		}

		private static void RenderContent(Graphics graphics, Rectangle rectangle, Pen pen)
		{
			rectangle.Inflate(-1, -1);
			int value = rectangle.Top + 2;
			int left = rectangle.Left;
			for (int i = 0; i < rectangle.Width; i++)
			{
				graphics.DrawLine(pen, left, value, left, value + 2);
				value = ((i >= 2) ? (value - 1) : (value + 1));
				left++;
			}
		}

		public void SetCentered(bool isCentered)
		{
			if (isCentered != _isCentered)
			{
				_isCentered = isCentered;
				Invalidate();
			}
		}

		public void SetThreeState(bool isThreeState)
		{
			if (isThreeState != _isThreeState)
			{
				_isThreeState = isThreeState;
			}
		}

		public void SetIndeterminateValue(object value)
		{
			if (value != _gridColumnControlValue)
			{
				if (IsNullValue(_checkState))
				{
					_checkState = value;
				}
				_gridColumnControlValue = value;
				Invalidate();
			}
		}

		public void SetCheckedValue(object value)
		{
			if (value != _checkedValue)
			{
				if (IsCheckedValue(_checkState))
				{
					_checkState = value;
				}
				_checkedValue = value;
				Invalidate();
			}
		}

		public void SetUncheckedValue(object value)
		{
			if (value != _uncheckedValue)
			{
				if (IsUncheckedValue(_checkState))
				{
					_checkState = value;
				}
				_uncheckedValue = value;
				Invalidate();
			}
		}

		internal static bool AreValuesEqual(object value, object item)
		{
			return GridHelper.CompareValues(value, item, null, true) == 0;
		}

		internal static Rectangle GetCheckBoxBounds(Rectangle rectangle, CheckBoxAlignment checkBoxAlignment, int height)
		{
			Rectangle result = rectangle;
			switch (checkBoxAlignment)
			{
			case CheckBoxAlignment.BottomLeft:
			case CheckBoxAlignment.BottomCenter:
			case CheckBoxAlignment.BottomRight:
				result.Y = rectangle.Bottom - height - 1;
				break;
			case CheckBoxAlignment.MiddleLeft:
			case CheckBoxAlignment.MiddleCenter:
			case CheckBoxAlignment.MiddleRight:
				result.Y = rectangle.Top + (rectangle.Height - height) / 2;
				break;
			case CheckBoxAlignment.TopLeft:
			case CheckBoxAlignment.TopCenter:
			case CheckBoxAlignment.TopRight:
				result.Y = rectangle.Top + 1;
				break;
			}
			switch (checkBoxAlignment)
			{
			case CheckBoxAlignment.TopLeft:
			case CheckBoxAlignment.MiddleLeft:
			case CheckBoxAlignment.BottomLeft:
				result.X = rectangle.X + 1;
				break;
			case CheckBoxAlignment.TopCenter:
			case CheckBoxAlignment.MiddleCenter:
			case CheckBoxAlignment.BottomCenter:
				result.X = rectangle.X + (rectangle.Width - height) / 2;
				break;
			case CheckBoxAlignment.TopRight:
			case CheckBoxAlignment.MiddleRight:
			case CheckBoxAlignment.BottomRight:
				result.X = rectangle.Right - height - 1;
				break;
			}
			result.Height = height;
			result.Width = height;
			return result;
		}

		internal static bool DrawClassicCheckBox(Graphics graphics, Rectangle rectangle, GridColumn gridColumn, TriState triState, bool condition, bool otherCondition, bool nextCondition, bool previousCondition, bool firstCondition)
		{
			bool secondCondition = false;
			bool thirdCondition = false;
			switch (triState)
			{
			case TriState.True:
				secondCondition = true;
				break;
			case TriState.Empty:
				thirdCondition = true;
				break;
			}
			CheckBoxAlignment checkBoxAlignment = CheckBoxAlignment.MiddleCenter;
			if (gridColumn != null)
			{
				checkBoxAlignment = gridColumn.CheckBoxAlignment;
			}
			if (checkBoxAlignment == CheckBoxAlignment.Default)
			{
				checkBoxAlignment = ((!nextCondition) ? CheckBoxAlignment.MiddleLeft : CheckBoxAlignment.MiddleCenter);
			}
			if (gridColumn != null && gridColumn.IsRightToLeft())
			{
				checkBoxAlignment = ComputeCheckBoxAlignment(checkBoxAlignment);
			}
			int value = 13;
			rectangle = GetCheckBoxBounds(rectangle, checkBoxAlignment, value);
			Mordex.Windows.Common.ResolvedTheme older = Mordex.Windows.Common.MxThemeManager.ResolveDefault((int)graphics.DpiX);
			if (older.Style.Detail.ChoiceLook != Mordex.Windows.Common.ThemeChoiceLook.Default && older.DrawChoiceGlyph(graphics, rectangle, false, secondCondition, thirdCondition, firstCondition, condition, otherCondition))
			{
				return true;
			}
			return VisualStyleDrawer.DrawThemedCheckBox(graphics, rectangle, condition, otherCondition, thirdCondition, secondCondition, firstCondition);
		}

		internal static void DrawBackground(Graphics graphics, Rectangle otherRectangle, GridColumn gridColumn, TriState triState, bool condition, bool otherCondition, bool nextCondition)
		{
			CheckBoxAlignment checkBoxAlignment = CheckBoxAlignment.MiddleCenter;
			if (gridColumn != null)
			{
				checkBoxAlignment = gridColumn.CheckBoxAlignment;
			}
			if (checkBoxAlignment == CheckBoxAlignment.Default)
			{
				checkBoxAlignment = ((!condition) ? CheckBoxAlignment.MiddleLeft : CheckBoxAlignment.MiddleCenter);
			}
			if (gridColumn != null && gridColumn.IsRightToLeft())
			{
				checkBoxAlignment = ComputeCheckBoxAlignment(checkBoxAlignment);
			}
			Rectangle rectangle = GetCheckBoxBounds(otherRectangle, checkBoxAlignment, 11);
			Pen pen = GdiCache.Pen(SystemColors.WindowFrame);
			if (!nextCondition)
			{
				pen = GdiCache.Pen(Color.Silver);
			}
			rectangle = GridHelper.DrawFlatBorder(graphics, pen, rectangle);
			graphics.FillRectangle(SystemBrushes.Window, rectangle);
			if (triState == TriState.Empty)
			{
				rectangle.Inflate(-1, -1);
				graphics.FillRectangle(SystemBrushes.Control, rectangle);
				if (otherCondition)
				{
					rectangle.Inflate(1, 1);
					triState = TriState.True;
				}
			}
			if (triState == TriState.True)
			{
				RenderContent(graphics, rectangle, pen);
			}
			pen = null;
		}

		private bool IsNullValue(object value)
		{
			return AreValuesEqual(_gridColumnControlValue, value);
		}

		private bool IsCheckedValue(object value)
		{
			return AreValuesEqual(_checkedValue, value);
		}

		private bool IsUncheckedValue(object value)
		{
			return AreValuesEqual(_uncheckedValue, value);
		}

		internal void ProcessCharacter(char character)
		{
			if (char.IsSeparator(character))
			{
				CycleValue();
			}
		}

		public void SetControlStyle(ControlStyle controlStyle)
		{
			if (_style != controlStyle)
			{
				_style = controlStyle;
				Invalidate();
			}
		}

		internal void CycleValue()
		{
			if (_isThreeState)
			{
				if (IsNullValue(_checkState))
				{
					SetCheckState(_uncheckedValue);
				}
				else if (IsUncheckedValue(_checkState))
				{
					SetCheckState(_checkedValue);
				}
				else
				{
					SetCheckState(_gridColumnControlValue);
				}
			}
			else if (IsUncheckedValue(_checkState))
			{
				SetCheckState(_checkedValue);
			}
			else
			{
				SetCheckState(_uncheckedValue);
			}
		}

		internal CheckState ComputeCheckState()
		{
			object value = GetStoredValue();
			if (IsNullValue(value))
			{
				return CheckState.Indeterminate;
			}
			if (IsCheckedValue(value))
			{
				return CheckState.Checked;
			}
			return CheckState.Unchecked;
		}

		internal void CycleValueIfEnabled()
		{
			if (base.Enabled)
			{
				CycleValue();
			}
		}

		private static CheckBoxAlignment ComputeCheckBoxAlignment(CheckBoxAlignment checkBoxAlignment)
		{
			switch (checkBoxAlignment)
			{
			case CheckBoxAlignment.BottomLeft:
				return CheckBoxAlignment.BottomRight;
			case CheckBoxAlignment.BottomRight:
				return CheckBoxAlignment.BottomLeft;
			case CheckBoxAlignment.MiddleLeft:
				return CheckBoxAlignment.MiddleRight;
			case CheckBoxAlignment.MiddleRight:
				return CheckBoxAlignment.MiddleLeft;
			case CheckBoxAlignment.TopLeft:
				return CheckBoxAlignment.TopRight;
			case CheckBoxAlignment.TopRight:
				return CheckBoxAlignment.TopLeft;
			default:
				return checkBoxAlignment;
			}
		}

		internal static void DrawGradientCheckBox(Graphics graphics, Rectangle otherRectangle, GridColumn gridColumn, GridColorTable colorSchemeAppearanceState, TriState triState, bool condition, bool otherCondition, bool nextCondition, bool previousCondition, bool firstCondition)
		{
			CheckBoxAlignment checkBoxAlignment = CheckBoxAlignment.MiddleCenter;
			if (gridColumn != null)
			{
				checkBoxAlignment = gridColumn.CheckBoxAlignment;
			}
			if (checkBoxAlignment == CheckBoxAlignment.Default)
			{
				checkBoxAlignment = ((!condition) ? CheckBoxAlignment.MiddleLeft : CheckBoxAlignment.MiddleCenter);
			}
			if (gridColumn != null && gridColumn.IsRightToLeft())
			{
				checkBoxAlignment = ComputeCheckBoxAlignment(checkBoxAlignment);
			}
			Rectangle rectangle = GetCheckBoxBounds(otherRectangle, checkBoxAlignment, 13);
			LinearGradientBrush linearGradientBrush;
			if (!firstCondition)
			{
				linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rectangle, 1, 1), colorSchemeAppearanceState.GetGridColor(GridColorKey.DisabledColor), Color.White, LinearGradientMode.Vertical);
			}
			else if (nextCondition)
			{
				linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rectangle, 1, 1), colorSchemeAppearanceState.GetGridColor(GridColorKey.PressedElement), colorSchemeAppearanceState.GetGridColor(GridColorKey.PressedElementGradient), LinearGradientMode.Vertical);
			}
			else if (otherCondition)
			{
				linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rectangle, 1, 1), colorSchemeAppearanceState.GetGridColor(GridColorKey.HotElement), colorSchemeAppearanceState.GetGridColor(GridColorKey.HotElementGradient), LinearGradientMode.Vertical);
			}
			else
			{
				linearGradientBrush = new LinearGradientBrush(Rectangle.Inflate(rectangle, 1, 1), colorSchemeAppearanceState.GetGridColor(GridColorKey.HeaderBorder), Color.White, LinearGradientMode.ForwardDiagonal);
				Blend blend = new Blend(3);
				blend.Positions = new float[3] { 0f, 0.3f, 1f };
				blend.Factors = new float[3] { 0f, 0.8f, 1f };
				linearGradientBrush.Blend = blend;
			}
			graphics.FillRectangle(linearGradientBrush, rectangle);
			linearGradientBrush.Dispose();
			Pen pen = GdiCache.Pen(GridHelper.BlendColors(colorSchemeAppearanceState.GetGridColor(GridColorKey.ControlBorder), Color.Black, 0.5f));
			GridHelper.DrawFlatBorder(graphics, pen, rectangle);
			Color color = SystemColors.Highlight;
			if (!firstCondition)
			{
				color = SystemColors.GrayText;
			}
			if ((double)color.GetBrightness() > 0.4)
			{
				color = GridHelper.BlendColors(color, Color.Black, 0.75f);
			}
			switch (triState)
			{
			case TriState.True:
				rectangle.Inflate(-2, -2);
				pen = GdiCache.Pen(color);
				RenderContent(graphics, rectangle, pen);
				break;
			case TriState.Empty:
				color = GridHelper.BlendColors(color, Color.White, 0.5f);
				if (previousCondition)
				{
					rectangle.Inflate(-2, -2);
					Brush brush = GdiCache.Brush(color);
					graphics.FillRectangle(brush, rectangle);
					color = SystemColors.Highlight;
					if ((double)color.GetBrightness() > 0.4)
					{
						color = GridHelper.BlendColors(color, Color.Black, 0.75f);
					}
					pen = GdiCache.Pen(color);
					RenderContent(graphics, rectangle, pen);
				}
				else
				{
					rectangle.Inflate(-3, -3);
					Brush solidBrush = GdiCache.Brush(color);
					graphics.FillRectangle(solidBrush, rectangle);
				}
				break;
			}
		}
	}
}
