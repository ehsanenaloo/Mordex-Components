using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	internal partial class NumericEditControl : Control
	{
		private bool _isCompactLayout = true;

		private System.Windows.Forms.Timer _timer;

		private NumericEdit _numericEdit;

		private EventHandler _changedHandler;

		private bool _showNewButton;

		private int _totalCount;

		private int _currentIndex = -1;

		private int _pressedButton = -1;

		private int _hotButton = -1;

		private bool _isNewRecord;

		private bool _useThemedAppearance = true;

		private string _captionText = "Record:";

		private string _ofLabel = "of";

		private int _customHeight = -1;

		private Rectangle _captionBounds;

		private Rectangle _ofTextBounds;

		private Rectangle _totalCountBounds;

		private GridLayoutState[] _buttonStates = new GridLayoutState[5];

		private ButtonIndexCallback _buttonClickedHandler;

		private ControlStyle _cStyle;

		private bool _useCompatibleTextRendering;

		private Font _editorFont;

		internal NumericEditControl(ControlStyle cStyle)
		{
			_cStyle = cStyle;
			_timer = new System.Windows.Forms.Timer();
			_timer.Interval = Timing.InitialAutoRepeatDelay;
			_timer.Tick += OnTimerTick;
			BackColor = SystemColors.Control;
			_numericEdit = new NumericEdit(checkLicense: false);
			_numericEdit.SetEmptyTextAsString(true);
			_numericEdit.TabStop = false;
			_numericEdit.AutoSize = false;
			if (_cStyle != null)
			{
				_numericEdit.BackColor = _cStyle.WindowColor;
				_numericEdit.ForeColor = _cStyle.WindowTextColor;
			}
			_numericEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_numericEdit.ValueType = NumericEditValueType.Int32;
			_numericEdit.FormatMask = NumericEditFormatMask.Number;
			_numericEdit.TextAlign = HorizontalAlignment.Right;
			_numericEdit.Multiline = false;
			UpdateEditorFont();
			base.Controls.Add(_numericEdit);
			SetStyle(ControlStyles.FixedWidth, value: true);
			SetStyle(ControlStyles.FixedHeight, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			for (int i = 0; i < 5; i++)
			{
				_buttonStates[i]._isVisible = true;
			}
			RecalculateLayout();
		}

		internal void SetCompactLayout(bool condition)
		{
			if (_isCompactLayout != condition)
			{
				_isCompactLayout = condition;
				RecalculateLayout();
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Return)
			{
				UpdateNumericEditControlInteger();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		private void UpdateNumericEditControlInteger()
		{
			if (_numericEdit.Text == "")
			{
				int currentIndex = _currentIndex;
				_currentIndex = -1;
				SetCurrentIndex(currentIndex);
			}
			else
			{
				try
				{
					SetCurrentIndex((int)_numericEdit.Value - 1);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					SetCurrentIndex(0);
				}
			}
			OnMoveToRow(EventArgs.Empty);
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

		internal virtual void OnMoveToRow(EventArgs eventArgs)
		{
			if (_changedHandler != null)
			{
				_changedHandler(this, eventArgs);
			}
		}

		protected override void OnFontChanged(EventArgs eventArgs)
		{
			base.OnFontChanged(eventArgs);
			if (_numericEdit.Font != null)
			{
				UpdateEditorFont();
			}
			RecalculateLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_timer != null)
				{
					_timer.Tick -= OnTimerTick;
					_timer.Dispose();
					_timer = null;
				}
				if (_numericEdit != null)
				{
					_numericEdit.Dispose();
					_numericEdit = null;
				}
				if (_editorFont != null)
				{
					_editorFont.Dispose();
					_editorFont = null;
				}
				_cStyle = null;
			}
			base.Dispose(disposing);
		}

		public void UpdateNumericEditControlBoolean(bool numericEditControlBoolean)
		{
			if (numericEditControlBoolean != _showNewButton)
			{
				_showNewButton = numericEditControlBoolean;
				RecalculateLayout();
			}
		}

		public int GetTotalCount()
		{
			return _totalCount;
		}

		public void SetTotalCount(int value)
		{
			if (value != _totalCount)
			{
				_totalCount = value;
				UpdateNumericEditControl();
				RecalculateLayout();
				Invalidate();
			}
		}

		public int GetNumericEditControlInteger()
		{
			return _currentIndex;
		}

		public void SetCurrentIndex(int numericEditControlInteger)
		{
			if (_currentIndex != numericEditControlInteger)
			{
				_currentIndex = numericEditControlInteger;
				if (_currentIndex >= 0)
				{
					_numericEdit.Text = (numericEditControlInteger + 1).ToString(Application.CurrentCulture);
				}
				else
				{
					_numericEdit.Value = null;
				}
				UpdateNumericEditControl();
			}
		}

		private int HitTestButton(int value, int otherValue)
		{
			for (int i = 0; i < 5; i++)
			{
				if (_buttonStates[i]._isVisible && _buttonStates[i]._isEnabled && _buttonStates[i]._bounds.Contains(value, otherValue))
				{
					return i;
				}
			}
			return -1;
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				SetButtonHot(_hotButton, false);
				_hotButton = -1;
				_pressedButton = HitTestButton(eventArgs.X, eventArgs.Y);
				if (_pressedButton != -1)
				{
					SetButtonPressed(_pressedButton, true);
					RaiseButtonClicked(new NavigatorButtonEventArgs(_pressedButton));
					_timer.Enabled = true;
				}
			}
		}

		private void UpdateHotButton(bool condition, int value, int otherValue)
		{
			if (condition)
			{
				SetButtonHot(_hotButton, false);
				_hotButton = -1;
				return;
			}
			int containsCore = HitTestButton(value, otherValue);
			if (containsCore != _hotButton)
			{
				SetButtonHot(_hotButton, false);
				_hotButton = containsCore;
				SetButtonHot(_hotButton, true);
			}
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.Button == MouseButtons.None)
			{
				UpdateHotButton(false, eventArgs.X, eventArgs.Y);
			}
			else if (eventArgs.Button == MouseButtons.Left && _pressedButton != -1)
			{
				SetButtonPressed(_pressedButton, _buttonStates[_pressedButton]._bounds.Contains(eventArgs.X, eventArgs.Y));
				SetButtonHot(_pressedButton, true);
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			if (_pressedButton != -1)
			{
				SetButtonPressed(_pressedButton, false);
				_pressedButton = -1;
				ResetRepeatTimer();
			}
			UpdateHotButton(true, 0, 0);
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (_pressedButton != -1)
			{
				ResetRepeatTimer();
				SetButtonHot(_pressedButton, false);
				SetButtonPressed(_pressedButton, false);
				_pressedButton = -1;
			}
		}

		public void SetIsNewRecord(bool isNewRecord)
		{
			if (isNewRecord != _isNewRecord)
			{
				_isNewRecord = isNewRecord;
				UpdateNumericEditControl();
			}
		}

		internal void SetUseThemedAppearance(bool useThemedAppearance)
		{
			if (useThemedAppearance != _useThemedAppearance)
			{
				_useThemedAppearance = useThemedAppearance;
				Invalidate();
			}
		}

		private void UpdateNumericEditControl()
		{
			if (base.Enabled)
			{
				SetButtonEnabled(0, _totalCount > 0);
				if (_isNewRecord)
				{
					SetButtonEnabled(1, _currentIndex > 0 && _totalCount > 0);
				}
				else
				{
					SetButtonEnabled(1, _currentIndex != 0 && _totalCount > 0);
				}
				SetButtonEnabled(2, _currentIndex != _totalCount - 1 && _totalCount > 0);
				SetButtonEnabled(3, _totalCount > 0);
				SetButtonEnabled(4, !_isNewRecord);
				_numericEdit.Enabled = _totalCount != 0;
				if (!_numericEdit.Enabled)
				{
					SetCurrentIndex(-1);
				}
			}
			else
			{
				for (int i = 0; i < 5; i++)
				{
					SetButtonEnabled(i, false);
				}
			}
		}

		protected override void OnEnabledChanged(EventArgs eventArgs)
		{
			base.OnEnabledChanged(eventArgs);
			UpdateNumericEditControl();
		}

		public string GetCaptionText()
		{
			return _captionText;
		}

		public void SetCaptionText(string text)
		{
			if (_captionText != text)
			{
				_captionText = text;
				RecalculateLayout();
			}
		}

		public string GetNumericEditControlText()
		{
			return _ofLabel;
		}

		public void UpdateNumericEditControlText(string numericEditControlText)
		{
			if (numericEditControlText != _ofLabel)
			{
				_ofLabel = numericEditControlText;
				RecalculateLayout();
			}
		}

		private TextLayoutRenderer ComputeTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!GetUseCompatibleTextRendering());
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			return textLayoutRenderer;
		}

		internal int GetButtonHeight()
		{
			if (_customHeight == -1)
			{
				return SystemInformation.HorizontalScrollBarHeight;
			}
			return _customHeight;
		}

		internal void SetButtonHeight(int customHeight)
		{
			if (_customHeight != customHeight)
			{
				_customHeight = customHeight;
				UpdateEditorFont();
				RecalculateLayout();
			}
		}

		private void UpdateEditorFont()
		{
			Font created = CreateEditorFont();
			// Control.Font ignores a value that equals the current one and keeps the old object, so the old one must stay alive then.
			if (_editorFont != null && _editorFont.Equals(created))
			{
				created.Dispose();
				return;
			}
			Font previousFont = _editorFont;
			_editorFont = created;
			_numericEdit.Font = _editorFont;
			previousFont?.Dispose();
		}

		private Font CreateEditorFont()
		{
			try
			{
				float emSize = (float)GetButtonHeight() - 6f;
				if (emSize < 4f)
				{
					emSize = 4f;
				}
				return new Font(Font.FontFamily, emSize, GraphicsUnit.Pixel);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return (Font)Font.Clone();
			}
		}

		public void RecalculateLayout()
		{
			if (base.IsDisposed || base.Disposing || !base.IsHandleCreated)
			{
				return;
			}
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = GetCStyle().ResolveAppearanceState(GridStyleArea.EditControls);
			Graphics graphics = CreateGraphics();
			Rectangle clientRectangle = base.ClientRectangle;
			TextLayoutRenderer textLayoutRenderer = ComputeTextLayoutRenderer();
			UpdateEditorFont();
			Font font = _editorFont;
			int value = 0;
			if (_captionText.Length > 0)
			{
				value = (int)textLayoutRenderer.MeasureText(graphics, _captionText, font, 0).Width + 6;
			}
			int otherValue = 0;
			if (_ofLabel.Length > 0)
			{
				otherValue = (int)textLayoutRenderer.MeasureText(graphics, _ofLabel, font, 0).Width + 6;
			}
			int width = (int)textLayoutRenderer.MeasureText(graphics, GetTotalCount().ToString("N0", Application.CurrentCulture), font, 0).Width + 2;
			int nextValue = (int)textLayoutRenderer.MeasureText(graphics, GetTotalCount().ToString("N0", Application.CurrentCulture), font, 0).Width + 4;
			int previousValue = (int)textLayoutRenderer.MeasureText(graphics, "00000", font, 0).Width + 6;
			if (nextValue < previousValue)
			{
				nextValue = previousValue;
			}
			int otherWidth = value + otherValue + width + nextValue;
			int nextWidth = 24;
			int firstValue = clientRectangle.Top + 1;
			int otherHeight = clientRectangle.Height - 3;
			textLayoutRenderer.ReleaseStringFormat();
			graphics.Dispose();
			graphics = null;
			if (_showNewButton)
			{
				otherWidth += nextWidth;
			}
			otherWidth += nextWidth * 4;
			int top = clientRectangle.Top;
			int height = clientRectangle.Height;
			int otherTop = clientRectangle.Top;
			if (otherWidth != base.Width || GetButtonHeight() != base.Height)
			{
				base.Size = new Size(otherWidth, GetButtonHeight());
			}
			else if (RightToLeft == RightToLeft.No)
			{
				int x = clientRectangle.Left;
				if (_captionText.Length > 0)
				{
					_captionBounds = new Rectangle(clientRectangle.Left + 2, otherTop, value - 4, height);
					x = _captionBounds.Right + 2;
				}
				_buttonStates[0]._bounds = new Rectangle(x, top, nextWidth, height);
				x = _buttonStates[0]._bounds.Right;
				_buttonStates[1]._bounds = new Rectangle(x, top, nextWidth, height);
				x = _buttonStates[1]._bounds.Right;
				if (_numericEdit != null)
				{
					x += 2;
					_numericEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
					_numericEdit.Bounds = new Rectangle(x, firstValue, nextValue - 2, otherHeight);
					x = _numericEdit.Bounds.Right;
				}
				_ofTextBounds = new Rectangle(x + 2, otherTop, otherValue - 2, height);
				x = _ofTextBounds.Right;
				_totalCountBounds = new Rectangle(x, otherTop, width, height);
				x = _totalCountBounds.Right;
				_buttonStates[2]._bounds = new Rectangle(x, top, nextWidth, height);
				x = _buttonStates[2]._bounds.Right;
				_buttonStates[3]._bounds = new Rectangle(x, top, nextWidth, height);
				x = _buttonStates[3]._bounds.Right;
				if (_showNewButton)
				{
					_buttonStates[4]._isVisible = true;
					_buttonStates[4]._bounds = new Rectangle(x, top, nextWidth, height);
				}
				else
				{
					_buttonStates[4]._isVisible = false;
				}
			}
			else
			{
				int right = clientRectangle.Right;
				if (_captionText.Length > 0)
				{
					_captionBounds = new Rectangle(right - 2 - (value - 4), clientRectangle.Top, value - 4, clientRectangle.Height);
					right = _captionBounds.Left - 2;
				}
				_buttonStates[0]._bounds = new Rectangle(right - nextWidth, top, nextWidth, height);
				right = _buttonStates[0]._bounds.Left;
				_buttonStates[1]._bounds = new Rectangle(right - nextWidth, top, nextWidth, height);
				right = _buttonStates[1]._bounds.Left;
				if (_numericEdit != null)
				{
					right -= 2;
					_numericEdit.Bounds = new Rectangle(right - (nextValue - 2), firstValue, nextValue - 2, clientRectangle.Height - 3);
					right = _numericEdit.Bounds.Left;
				}
				_ofTextBounds = new Rectangle(right - otherValue, clientRectangle.Top, otherValue - 2, clientRectangle.Height);
				right = _ofTextBounds.Left;
				_totalCountBounds = new Rectangle(right - width, clientRectangle.Top, width, clientRectangle.Height);
				right = _totalCountBounds.Left;
				_buttonStates[2]._bounds = new Rectangle(right - nextWidth, top, nextWidth, height);
				right = _buttonStates[2]._bounds.Left;
				_buttonStates[3]._bounds = new Rectangle(right - nextWidth, top, nextWidth, height);
				right = _buttonStates[3]._bounds.Left;
				if (_showNewButton)
				{
					_buttonStates[4]._isVisible = true;
					_buttonStates[4]._bounds = new Rectangle(right - nextWidth, top, nextWidth, height);
				}
				else
				{
					_buttonStates[4]._isVisible = false;
				}
			}
			Invalidate();
		}

		protected override void OnRightToLeftChanged(EventArgs eventArgs)
		{
			base.OnRightToLeftChanged(eventArgs);
			RecalculateLayout();
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			RecalculateLayout();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			PaintContent(eventArgs.Graphics);
		}

		internal static void DrawBackground(Graphics graphics, Rectangle bounds, ControlStyle controlStyle)
		{
			try
			{
				MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = null;
				mordexColorSchemeAppearanceState = controlStyle.GetLayoutState().GetEditControlsStyle();
				if (mordexColorSchemeAppearanceState.IsThemedLook)
				{
					Color color = mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.Control);
					Brush brush = GdiCache.Brush(color);
					graphics.FillRectangle(brush, bounds);
				}
				else if (mordexColorSchemeAppearanceState._isThemed)
				{
					RenderContent(graphics, bounds);
				}
				else
				{
					graphics.FillRectangle(controlStyle.GetControlBrush(), bounds);
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal static void RenderContent(Graphics dc, Rectangle bounds)
		{
			if (VisualStyleDrawer.TrySetElement(VisualStyleElement.ScrollBar.LeftTrackHorizontal.Disabled))
			{
				VisualStyleDrawer._visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		private void PaintContent(Graphics graphics)
		{
			if (IsThemedLook)
			{
				PaintThemedContent(graphics);
				return;
			}
			DrawBackground(graphics, base.ClientRectangle, _cStyle);
			Color controlTextColor = _cStyle.ControlTextColor;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(GetUseCompatibleTextRendering());
			Font font = _editorFont;
			if (_captionText.Length > 0)
			{
				textLayoutRenderer.DrawTextWithColor(graphics, _captionText, _captionBounds, font, controlTextColor);
			}
			if (_ofLabel.Length > 0)
			{
				textLayoutRenderer.DrawTextWithColor(graphics, _ofLabel, _ofTextBounds, font, controlTextColor);
			}
			string text = GetTotalCount().ToString("N0", Application.CurrentCulture);
			textLayoutRenderer.DrawTextWithColor(graphics, text, _totalCountBounds, font, controlTextColor);
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _cStyle.GetLayoutState().GetEditControlsStyle();
			for (int i = 0; i < 5; i++)
			{
				PaintButton(graphics, i, mordexColorSchemeAppearanceState);
			}
			if (_numericEdit == null)
			{
				return;
			}
			Rectangle bounds = _numericEdit.Bounds;
			bounds.Inflate(1, 1);
			if (!mordexColorSchemeAppearanceState.IsThemedLook && !mordexColorSchemeAppearanceState._isThemed)
			{
				BorderStyle borderStyle;
				switch (_cStyle.ButtonAppearance)
				{
				case ButtonAppearance.Flat:
					borderStyle = BorderStyle.Flat;
					break;
				case ButtonAppearance.FlatBorderless:
					borderStyle = BorderStyle.None;
					break;
				default:
					borderStyle = BorderStyle.SunkenLight3D;
					break;
				}
				GridHelper.DrawBorderForStyle(graphics, _cStyle, bounds, borderStyle);
			}
			else
			{
				Pen pen = GdiCache.Pen(mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder));
				GridHelper.DrawFlatBorder(graphics, pen, bounds);
			}
		}

		private void SetButtonEnabled(int value, bool layoutStateBoolean)
		{
			if (_buttonStates[value]._isEnabled != layoutStateBoolean)
			{
				if (value == _pressedButton)
				{
					_pressedButton = -1;
				}
				if (value == _hotButton)
				{
					_hotButton = -1;
				}
				SetButtonHot(value, false);
				SetButtonPressed(value, false);
				_buttonStates[value]._isEnabled = layoutStateBoolean;
				InvalidateButton(value);
			}
		}

		private void SetButtonPressed(int value, bool condition)
		{
			if (value >= 0 && _buttonStates[value]._isPressed != condition && _buttonStates[value]._isEnabled)
			{
				_buttonStates[value]._isPressed = condition;
				InvalidateButton(value);
			}
		}

		private void SetButtonHot(int value, bool gridLayoutStateBoolean)
		{
			if (value >= 0 && _buttonStates[value]._isHot != gridLayoutStateBoolean && _buttonStates[value]._isEnabled)
			{
				_buttonStates[value]._isHot = gridLayoutStateBoolean;
				InvalidateButton(value);
			}
		}

		private void InvalidateButton(int value)
		{
			if (_buttonStates[value]._isVisible)
			{
				Invalidate(_buttonStates[value]._bounds);
			}
		}

		private void PaintButton(Graphics graphics, int value, MxColorSchemeAppearanceState mordexColorSchemeAppearanceState)
		{
			if (!_buttonStates[value]._isVisible)
			{
				return;
			}
			if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				if (_buttonStates[value]._isEnabled && (_buttonStates[value]._isHot || _buttonStates[value]._isPressed))
				{
					GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.FreeButton, null, null);
					gridGroupLayoutState._isHot = _buttonStates[value]._isHot;
					gridGroupLayoutState._isPressed = _buttonStates[value]._isPressed;
					CellRendererBase renderer = _cStyle.GetLayoutState().GetRenderer(gridGroupLayoutState);
					Rectangle rectangle = _buttonStates[value]._bounds;
					renderer.PaintBackground(graphics, gridGroupLayoutState, null, rectangle, selected: false, focused: false);
					renderer.PaintAddornments(graphics, gridGroupLayoutState, null, rectangle);
				}
				DrawButtonGlyph(graphics, _buttonStates[value]._bounds, value, mordexColorSchemeAppearanceState, _buttonStates[value]._isEnabled);
			}
			else
			{
				Rectangle otherRectangle = _buttonStates[value]._bounds;
				if (mordexColorSchemeAppearanceState._isThemed)
				{
					otherRectangle.Y++;
					otherRectangle.Height -= 2;
					otherRectangle.Width--;
					PaintThemedButton(graphics, otherRectangle, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, _buttonStates[value]._isEnabled);
					DrawButtonGlyph(graphics, otherRectangle, value, mordexColorSchemeAppearanceState, _buttonStates[value]._isEnabled);
				}
				else
				{
					_cStyle.DrawBackground(graphics, otherRectangle, _buttonStates[value]._isHot, _buttonStates[value]._isPressed);
					PaintStandardButton(graphics, value);
				}
			}
		}

		private void PaintThemedButton(Graphics graphics, Rectangle rectangle, bool condition, bool otherCondition, bool nextCondition)
		{
			rectangle.Inflate(1, 1);
			VisualStyleDrawer.DrawPushButton(graphics, rectangle, condition, otherCondition, !nextCondition);
		}

		private void DrawButtonGlyph(Graphics graphics, Rectangle rectangle, int value, MxColorSchemeAppearanceState mordexColorSchemeAppearanceState, bool condition)
		{
			string text = "";
			string otherText = "";
			switch (value)
			{
			case 0:
				text = ((RightToLeft != RightToLeft.No) ? "Last" : "First");
				break;
			case 1:
				text = ((RightToLeft != RightToLeft.No) ? "Next" : "Prev");
				break;
			case 2:
				text = ((RightToLeft != RightToLeft.No) ? "Prev" : "Next");
				break;
			case 3:
				text = ((RightToLeft != RightToLeft.No) ? "First" : "Last");
				break;
			case 4:
				text = "New";
				break;
			}
			if (!mordexColorSchemeAppearanceState.IsThemedLook)
			{
				otherText = "Blue";
			}
			else
			{
				switch (mordexColorSchemeAppearanceState.GetResolvedAppearance().GetThemeColorName())
				{
				case "DEFAULT":
					otherText = "Blue";
					break;
				case "OLIVE":
					otherText = "Green";
					break;
				default:
					otherText = "Black";
					break;
				}
			}
			Image image = null;
			string nextText;
			if ((nextText = otherText) != null)
			{
				string previousText;
				if (!(nextText == "Blue"))
				{
					string firstText;
					if (!(nextText == "Green"))
					{
						string secondText;
						if (nextText == "Black" && (secondText = text) != null)
						{
							if (!(secondText == "First"))
							{
								if (!(secondText == "Last"))
								{
									if (!(secondText == "Next"))
									{
										if (!(secondText == "Prev"))
										{
											if (secondText == "New")
											{
												image = ResourceImages.GetNewRecordBlackImage();
											}
										}
										else
										{
											image = ResourceImages.GetPreviousBlackImage();
										}
									}
									else
									{
										image = ResourceImages.GetNextBlackImage();
									}
								}
								else
								{
									image = ResourceImages.GetLastBlackImage();
								}
							}
							else
							{
								image = ResourceImages.GetFirstBlackImage();
							}
						}
					}
					else if ((firstText = text) != null)
					{
						if (!(firstText == "First"))
						{
							if (!(firstText == "Last"))
							{
								if (!(firstText == "Next"))
								{
									if (!(firstText == "Prev"))
									{
										if (firstText == "New")
										{
											image = ResourceImages.GetNewRecordGreenImage();
										}
									}
									else
									{
										image = ResourceImages.GetPreviousGreenImage();
									}
								}
								else
								{
									image = ResourceImages.GetNextGreenImage();
								}
							}
							else
							{
								image = ResourceImages.GetLastGreenImage();
							}
						}
						else
						{
							image = ResourceImages.GetFirstGreenImage();
						}
					}
				}
				else if ((previousText = text) != null)
				{
					if (!(previousText == "First"))
					{
						if (!(previousText == "Last"))
						{
							if (!(previousText == "Next"))
							{
								if (!(previousText == "Prev"))
								{
									if (previousText == "New")
									{
										image = ResourceImages.GetNewRecordBlueImage();
									}
								}
								else
								{
									image = ResourceImages.GetPreviousBlueImage();
								}
							}
							else
							{
								image = ResourceImages.GetNextBlueImage();
							}
						}
						else
						{
							image = ResourceImages.GetLastBlueImage();
						}
					}
					else
					{
						image = ResourceImages.GetFirstBlueImage();
					}
				}
			}
			image = GridHelper.MakeBitmapTransparent((Bitmap)image, Color.Magenta);
			rectangle.Inflate(-2, -2);
			Rectangle destRect;
			if (rectangle.Height < image.Height)
			{
				int height = rectangle.Height;
				if ((height & 2) != 0)
				{
					height--;
				}
				destRect = new Rectangle(rectangle.X + (rectangle.Width - image.Width) / 2, rectangle.Y, image.Width, height);
				Bitmap bitmap = new Bitmap(image, image.Width, height);
				image.Dispose();
				image = bitmap;
			}
			else
			{
				destRect = new Rectangle(rectangle.X + (rectangle.Width - image.Width) / 2, rectangle.Y + (rectangle.Height - image.Height) / 2, image.Width, image.Height);
			}
			if (condition)
			{
				graphics.DrawImage(image, destRect, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
			}
			else
			{
				ControlPaint.DrawImageDisabled(graphics, image, destRect.X, destRect.Y, Color.White);
			}
			image.Dispose();
		}

		private void PaintStandardButton(Graphics graphics, int value)
		{
			Rectangle rectangle = _buttonStates[value]._bounds;
			switch (value)
			{
			case 0:
				_cStyle.DrawArrowWithBar(graphics, rectangle, -1, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, _buttonStates[value]._isEnabled);
				break;
			case 1:
				_cStyle.DrawHorizontalArrow(graphics, rectangle, -1, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, _buttonStates[value]._isEnabled);
				break;
			case 2:
				_cStyle.DrawHorizontalArrow(graphics, rectangle, 1, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, _buttonStates[value]._isEnabled);
				break;
			case 3:
				_cStyle.DrawArrowWithBar(graphics, rectangle, 1, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, _buttonStates[value]._isEnabled);
				break;
			case 4:
				_cStyle.DrawArrowWithAsterisk(graphics, rectangle, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, _buttonStates[value]._isEnabled);
				break;
			}
		}

		internal void AddButtonClickedHandler(ButtonIndexCallback callback)
		{
			ButtonIndexCallback gridCallback2Argument = _buttonClickedHandler;
			ButtonIndexCallback gridCallback2Argument2;
			do
			{
				gridCallback2Argument2 = gridCallback2Argument;
				ButtonIndexCallback value = (ButtonIndexCallback)Delegate.Combine(gridCallback2Argument2, callback);
				gridCallback2Argument = Interlocked.CompareExchange(ref _buttonClickedHandler, value, gridCallback2Argument2);
			}
			while ((object)gridCallback2Argument != gridCallback2Argument2);
		}

		internal void RemoveButtonClickedHandler(ButtonIndexCallback otherValue)
		{
			ButtonIndexCallback gridCallback2Argument = _buttonClickedHandler;
			ButtonIndexCallback gridCallback2Argument2;
			do
			{
				gridCallback2Argument2 = gridCallback2Argument;
				ButtonIndexCallback value = (ButtonIndexCallback)Delegate.Remove(gridCallback2Argument2, otherValue);
				gridCallback2Argument = Interlocked.CompareExchange(ref _buttonClickedHandler, value, gridCallback2Argument2);
			}
			while ((object)gridCallback2Argument != gridCallback2Argument2);
		}

		private void RaiseButtonClicked(NavigatorButtonEventArgs eventArgs)
		{
			if (_buttonClickedHandler != null)
			{
				_buttonClickedHandler(this, eventArgs);
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_pressedButton == -1)
			{
				ResetRepeatTimer();
				return;
			}
			Point mousePosition = Control.MousePosition;
			mousePosition = PointToClient(mousePosition);
			if (_buttonStates[_pressedButton]._bounds.Contains(mousePosition.X, mousePosition.Y))
			{
				RaiseButtonClicked(new NavigatorButtonEventArgs(_pressedButton));
			}
			_timer.Interval = Timing.AutoRepeatInterval;
		}

		private void ResetRepeatTimer()
		{
			_timer.Enabled = false;
			_timer.Interval = Timing.InitialAutoRepeatDelay;
		}

		internal ControlStyle GetCStyle()
		{
			return _cStyle;
		}

		internal void UpdateCStyle(ControlStyle cStyle)
		{
			_cStyle = cStyle;
			if (_cStyle != null && _numericEdit != null)
			{
				_numericEdit.BackColor = _cStyle.WindowColor;
				_numericEdit.ForeColor = _cStyle.WindowTextColor;
			}
		}

		internal bool GetUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering;
		}

		internal void SetUseCompatibleTextRendering(bool useCompatibleTextRendering)
		{
			if (_useCompatibleTextRendering != useCompatibleTextRendering)
			{
				_useCompatibleTextRendering = useCompatibleTextRendering;
				RecalculateLayout();
			}
		}
	}
}
