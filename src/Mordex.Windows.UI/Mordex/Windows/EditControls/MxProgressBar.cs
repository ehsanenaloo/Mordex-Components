using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	[DefaultEvent("ValueChanged")]
	[ToolboxItem(true)]
	[DefaultProperty("Value")]
	[DefaultBindingProperty("Value")]
	public partial class MxProgressBar : Control, IFormatStyleOwner
	{
		private const int ContentInset = 3;

		private const long RepaintIntervalTicks = 50000000L;

		private const double RepaintMinValueFraction = 0.001;

		private Container _container = new Container();

		private Mordex.Windows.UI.BorderStyle _borderStyle = Mordex.Windows.UI.BorderStyle.SunkenLight3D;

		private int _value;

		private int _maximum = 100;

		private int _minimum;

		private Color _flatBorderColor = Color.Empty;

		private FormatStyle _progressFormatStyle;

		private FormatStyle _backgroundFormatStyle;

		private bool _showPercentage;

		private StringAlignment _percentageAlignment = StringAlignment.Center;

		private VisualStylePainter _themeRenderer;

		private long _lastPaintTicks;

		private int _lastPaintValue;

		private static readonly object _valueChangedKey = new object();

		Control IFormatStyleOwner.Owner => this;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[Description("The border style of the control.")]
		[DefaultValue(Mordex.Windows.UI.BorderStyle.SunkenLight3D)]
		public Mordex.Windows.UI.BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (_borderStyle != value)
				{
					_borderStyle = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the current position for the progress bar, in the range specified by the Minimum and Maximum properties.")]
		[Bindable(BindableSupport.Yes)]
		[DefaultValue(0)]
		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					if (value > _maximum || value < _minimum)
					{
						throw new Exception("'" + value + "' is is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'.");
					}
					_value = value;
					OnValueChanged(EventArgs.Empty);
					Invalidate();
					RepaintIfNeeded();
				}
			}
		}

		[Description("Represents the maximum value of the range of the control.")]
		[DefaultValue(100)]
		public int Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				if (_maximum != value)
				{
					if (value < _minimum)
					{
						_minimum = value;
					}
					_maximum = value;
					if (Value > _maximum)
					{
						Value = _maximum;
					}
					Invalidate();
				}
			}
		}

		[Description("Represents the minimum value of the range of the control.")]
		[DefaultValue(0)]
		public int Minimum
		{
			get
			{
				return _minimum;
			}
			set
			{
				if (_minimum != value)
				{
					if (value > _maximum)
					{
						_maximum = value;
					}
					_minimum = value;
					if (Value < _minimum)
					{
						Value = _minimum;
					}
					Invalidate();
				}
			}
		}

		[Description("The color of the border of the control when it\u00b4s border is Flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
				}
				return _flatBorderColor;
			}
			set
			{
				if (_flatBorderColor != value)
				{
					_flatBorderColor = value;
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The format style properties used to draw the progress bar of the control.")]
		public FormatStyle ProgressFormatStyle => _progressFormatStyle;

		[Description("The format style properties used to draw the background of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle BackgroundFormatStyle => _backgroundFormatStyle;

		[DefaultValue(false)]
		[Description("Specifies whether a text, representing the percentage of the value, will be draw in the control.")]
		public bool ShowPercentage
		{
			get
			{
				return _showPercentage;
			}
			set
			{
				if (_showPercentage != value)
				{
					_showPercentage = value;
					Invalidate();
				}
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[DefaultValue(StringAlignment.Center)]
		[Description("Determines how the the text is aligned")]
		public StringAlignment StringAlignment
		{
			get
			{
				return _percentageAlignment;
			}
			set
			{
				if (_percentageAlignment != value)
				{
					_percentageAlignment = value;
					Invalidate();
				}
			}
		}

		[Description("Event fired when the Value property has changed.")]
		public event EventHandler ValueChanged
		{
			add
			{
				base.Events.AddHandler(_valueChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_valueChangedKey, value);
			}
		}

		public MxProgressBar()
		{
			InitializeComponents();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			_backgroundFormatStyle = new FormatStyle(this);
			_progressFormatStyle = new FormatStyle(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponents()
		{
			_container = new Container();
		}

		void IFormatStyleOwner.PropertyChange()
		{
			Invalidate();
		}

		internal Color GetEffectiveForeColor()
		{
			if (ForeColor == SystemColors.ControlText)
			{
				if (!base.Enabled)
				{
					return SystemColors.GrayText;
				}
			}
			return base.ForeColor;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}

		private FormatStyle CreateBackgroundFormatStyle()
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			Color controlColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			uiFormatStyle.BackColor = controlColor;
			return uiFormatStyle.MergeStyle(BackgroundFormatStyle);
		}

		private FormatStyle CreateProgressFormatStyle()
		{
			FormatStyle uiFormatStyle = new FormatStyle();
			uiFormatStyle.BackColor = RenderingHelper.GetSchemeColor(ColorName.Highlight);
			uiFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.HighlightText);
			return uiFormatStyle.MergeStyle(ProgressFormatStyle);
		}

		private void RepaintIfNeeded()
		{
			if (DateTime.Now.Ticks - _lastPaintTicks > RepaintIntervalTicks)
			{
				Update();
				return;
			}
			double valueDelta = Math.Abs((double)_lastPaintValue - Value);
			double valueRange = (double)_maximum - _minimum;
			if (valueRange > 0 && valueDelta / valueRange > RepaintMinValueFraction)
			{
				Update();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			PaintControl(e.Graphics, e.ClipRectangle);
			base.OnPaint(e);
		}

		private void PaintControl(Graphics targetGraphics, Rectangle clipRectangle)
		{
			if (base.Width <= 0 || base.Height <= 0)
			{
				return;
			}
			_lastPaintTicks = DateTime.Now.Ticks;
			_lastPaintValue = Value;
			if (IsThemed)
			{
				PaintThemed(targetGraphics);
				return;
			}
			if ((VisualStyleSupport.IsThemingUsable(false)))
			{
				_themeRenderer = new VisualStylePainter("ProgessBar");
			}
			int value = Value;
			FormatStyle progressStyle = CreateProgressFormatStyle();
			Rectangle textBounds = base.ClientRectangle;
			Bitmap bitmap = new Bitmap(base.Width, base.Height);
			Graphics graphics = Graphics.FromImage(bitmap);
			if (_themeRenderer != null)
			{
				if (_maximum > _minimum)
				{
					float fillPercent = ((float)value - (float)_minimum) / ((float)_maximum - (float)_minimum) * 100f;
					value = (int)fillPercent;
				}
				else
				{
					value = 0;
				}
				_themeRenderer.DrawProgressBar(graphics, base.ClientRectangle, value);
				progressStyle.ForeColor = GetEffectiveForeColor();
				value = Value;
				textBounds.Inflate(-ContentInset, -ContentInset);
			}
			else
			{
				FormatStyle backgroundStyle = CreateBackgroundFormatStyle();
				RenderingHelper.DrawFormatStyleBackground(graphics, backgroundStyle, clipRectangle, base.ClientRectangle);
				Rectangle trackBounds = base.ClientRectangle;
				if (BorderStyle != Mordex.Windows.UI.BorderStyle.None)
				{
					{
						Pen borderPen = GdiCache.Pen(FlatBorderColor);
						trackBounds = RenderingHelper.DrawRectangleOutline(graphics, borderPen, trackBounds);
					}
				}
				backgroundStyle.DisposeResources();
				trackBounds.Inflate(-ContentInset, -ContentInset);
				textBounds = trackBounds;
				if (_maximum > _minimum)
				{
					float fillWidth = (float)trackBounds.Width * ((float)value - (float)_minimum) / ((float)_maximum - (float)_minimum);
					trackBounds.Width = (int)fillWidth;
				}
				else
				{
					trackBounds.Width = 0;
				}
				progressStyle.GetBackgroundBrush(trackBounds);
				if (trackBounds.Width > 0)
				{
					RenderingHelper.DrawFormatStyleBackground(graphics, progressStyle, clipRectangle, trackBounds);
					{
						Pen highlightPen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.Highlight));
						RenderingHelper.DrawRectangleOutline(graphics, highlightPen, trackBounds);
					}
				}
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}
			targetGraphics.DrawImage(bitmap, base.ClientRectangle);
			graphics.Dispose();
			graphics = null;
			bitmap.Dispose();
			bitmap = null;
			if (_showPercentage)
			{
				float percentValue = 0f;
				if (_maximum > _minimum)
				{
					percentValue = ((float)value - (float)_minimum) / ((float)_maximum - (float)_minimum) * 100f;
				}
				string text = (int)percentValue + "%";
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: true);
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				textLayoutRenderer.SetWordWrap(false);
				textLayoutRenderer.SetStringAlignment(_percentageAlignment);
				textLayoutRenderer.SetRightToLeft(RightToLeft);
				textLayoutRenderer.DrawTextWithBrush(targetGraphics, text, textBounds, progressStyle.ResolveFont(Font), progressStyle.GetForeBrush());
				textLayoutRenderer.ResetCache();
			}
			if (_themeRenderer != null)
			{
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			progressStyle.DisposeResources();
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (base.Events[_valueChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
