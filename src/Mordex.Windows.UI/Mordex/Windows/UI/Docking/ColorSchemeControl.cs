using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class ColorSchemeControl : Control
	{
		private const int InitialTickIntervalMs = 150;

		private const int RepeatTickIntervalMs = 50;

		private Timer _scrollTimer;

		private IContainer _components;

		internal bool _isThemed = true;

		internal bool _isVertical = true;

		internal MxColorSchemeAppearanceState _colorSchemeAppearance;


		private int _scrollStep;

		private bool _isHot;

		private int _minimum;

		private int _maximum;

		private int _position;

		private static readonly object _scrollEventKey = new object();

		internal ColorSchemeControl()
		{
			CreateComponents();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			base.TabStop = false;
		}

		public int GetScrollStep()
		{
			return _scrollStep;
		}

		public void SetScrollStep(int step)
		{
			_scrollStep = step;
		}

		public void SetHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				Invalidate();
			}
		}

		public void SetMinimum(int minimum)
		{
			_minimum = minimum;
		}

		public void SetMaximum(int maximum)
		{
			_maximum = maximum;
		}

		public int GetPosition()
		{
			return _position;
		}

		public void SetPosition(int position)
		{
			if (position < _minimum)
			{
				position = _minimum;
			}
			if (position > _maximum)
			{
				position = _maximum;
			}
			if (position != _position)
			{
				_position = position;
				OnScroll(EventArgs.Empty);
			}
			if (position > _maximum || position < _minimum)
			{
				StopTimer();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopTimer();
				if (_components != null)
				{
					_components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private void CreateComponents()
		{
			_components = new Container();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			{
				Color gradientEndColor = Color.Empty;
				Color fillColor;
				Color borderColor;
				if (_isThemed && VisualStyleSupport.AreVisualStylesEnabled())
				{
					VisualStylePainter themeRenderer = new VisualStylePainter("ToolBar");
					if (_isHot)
					{
						fillColor = themeRenderer._itemHotColor;
						borderColor = themeRenderer._hotBorderColor;
					}
					else
					{
						fillColor = themeRenderer._floatingBarColor;
						borderColor = themeRenderer._tabBorderColor;
					}
				}
				else
				{
					Color controlColor = RenderingHelper.GetSchemeColor(ColorName.Control);
					Color highlightColor = RenderingHelper.GetSchemeColor(ColorName.Highlight);
					if (_isHot)
					{
						borderColor = highlightColor;
						Color windowColor = RenderingHelper.GetSchemeColor(ColorName.Window);
						Color blendedColor = RenderingHelper.BlendColors(controlColor, highlightColor, 0.1f);
						fillColor = RenderingHelper.BlendColors(blendedColor, windowColor, 0.33f);
						float brightness = (float)(fillColor.R + fillColor.G + fillColor.B) / 3f / 255f;
						if (brightness >= 0.85f)
						{
							float darkenAmount = brightness - 0.45f;
							fillColor = RenderingHelper.BlendColors(RenderingHelper.BlendColors(blendedColor, Color.Black, 1f - darkenAmount), windowColor, 0.33f);
						}
					}
					else
					{
						borderColor = RenderingHelper.GetSchemeColor(ColorName.ControlDark);
						fillColor = controlColor;
					}
				}
				Brush brush = null;
				brush = (gradientEndColor.IsEmpty ? ((Brush)new SolidBrush(fillColor)) : ((Brush)((!_isVertical) ? new LinearGradientBrush(base.ClientRectangle, fillColor, gradientEndColor, LinearGradientMode.Horizontal) : new LinearGradientBrush(base.ClientRectangle, fillColor, gradientEndColor, LinearGradientMode.Vertical))));
				Pen borderPen = GdiCache.Pen(borderColor);
				eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
				RenderingHelper.DrawRectangleOutline(eventArgs.Graphics, borderPen, base.ClientRectangle);
				borderPen = null;
				brush.Dispose();
				brush = null;
			}
			// Each resource call returns a new bitmap. Rotate and free this copy only.
			using (Bitmap arrow = (_scrollStep >= 0) ? WindowsResources.UpArrow() : WindowsResources.DownArrow())
			{
				if (arrow != null)
				{
					RenderingHelper.MakeMagentaTransparent(arrow);
					if (!_isVertical)
					{
						arrow.RotateFlip(RotateFlipType.Rotate90FlipX);
					}
					RenderingHelper.DrawCenteredImage(eventArgs.Graphics, arrow, base.ClientRectangle);
				}
			}
			base.OnPaint(eventArgs);
		}

		protected override void OnMouseEnter(EventArgs eventArgs)
		{
			base.OnMouseEnter(eventArgs);
			SetHot(true);
			StartTimer();
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetHot(false);
			StopTimer();
		}

		private void StartTimer()
		{
			if (_scrollTimer == null)
			{
				_scrollTimer = new Timer();
				_scrollTimer.Interval = InitialTickIntervalMs;
				_scrollTimer.Tick += OnTimerTick;
				_scrollTimer.Enabled = true;
			}
		}

		private void StopTimer()
		{
			if (_scrollTimer == null)
			{
				return;
			}
			Timer timer = _scrollTimer;
			_scrollTimer = null;
			timer.Stop();
			timer.Tick -= OnTimerTick;
			timer.Dispose();
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			_scrollTimer.Interval = RepeatTickIntervalMs;
			SetPosition(GetPosition() + GetScrollStep());
		}

		public void AddScrollHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_scrollEventKey, eventHandler);
		}

		public void RemoveScrollHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_scrollEventKey, eventHandler);
		}

		protected virtual void OnScroll(EventArgs eventArgs)
		{
			if (base.Events[_scrollEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}
	}
}
