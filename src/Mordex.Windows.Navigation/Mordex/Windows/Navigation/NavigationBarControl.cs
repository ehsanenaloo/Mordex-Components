using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Internal;

namespace Mordex.Windows.Navigation
{
	[ToolboxItem(false)]
	internal class NavigationBarControl : Control
	{
		private Container _container = new Container();

		private System.Windows.Forms.Timer _timer;

		private MxNavigationBar _owner;

		private ImageList _imageList;

		private AccessibleObject _accessibleObject;

		private NavigationBarOrientation _orientation;

		public ScrollLocation _scrollLocation;

		internal int _direction;

		private bool _isButtonEnabled = true;

		private bool _isPressed;

		private GroupAppearance _headerAppearance;

		private BorderStyle _border;

		internal ColorSchemeRenderer _mordexColorSchemeRenderer = new ColorSchemeRenderer();

		internal BorderFlags _borderFlags;

		private Rectangle _buttonBounds = Rectangle.Empty;

		private bool _isHot;

		private EventHandler _scrollHandler;

		public NavigationBarControl()
		{
			InitializeTimer();
		}

		internal NavigationBarControl(MxNavigationBar owner, int direction, NavigationBarOrientation orientation, GroupAppearance groupHeaderStyle, BorderStyle border, Color flatBorderColor, ImageList imageList)
		{
			InitializeTimer();
			_owner = owner;
			_direction = direction;
			_orientation = orientation;
			UpdateHeaderAppearance(groupHeaderStyle);
			_border = border;
			_mordexColorSchemeRenderer.SetFlatBorderColor(flatBorderColor);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			_imageList = imageList;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new ScrollButtonAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal MxNavigationBar GetOwner()
		{
			return _owner;
		}

		public void UpdateOrientation(NavigationBarOrientation orientation)
		{
			if (_orientation != orientation)
			{
				_orientation = orientation;
				Invalidate();
			}
		}

		internal bool IsButtonEnabled()
		{
			return _isButtonEnabled;
		}

		internal void SetButtonEnabled(bool isEnabled)
		{
			bool wasEnabled = _isButtonEnabled;
			_isButtonEnabled = isEnabled;
			if (!_isButtonEnabled)
			{
				_isPressed = false;
				_timer.Stop();
			}
			if (_scrollLocation == ScrollLocation.ItemsLevel)
			{
				base.Visible = isEnabled;
			}
			if (wasEnabled != isEnabled)
			{
				Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetHot(false);
			SetPressed(false);
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (_isButtonEnabled)
			{
				SetPressed(true);
				OnScroll(new EventArgs());
				if (_isButtonEnabled)
				{
					_timer.Start();
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			SetPressed(false);
			base.OnMouseUp(eventArgs);
			_timer.Stop();
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (eventArgs.Button == MouseButtons.Left && _isButtonEnabled)
			{
				if (!base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					if (_isPressed)
					{
						SetPressed(false);
						_timer.Stop();
					}
				}
				else if (!_isPressed)
				{
					SetPressed(true);
					_timer.Start();
				}
			}
			if (_buttonBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				SetHot(true);
			}
			else
			{
				SetHot(false);
			}
		}

		public bool IsPressed()
		{
			return _isPressed;
		}

		public void SetPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				Invalidate();
			}
		}

		public void UpdateHeaderAppearance(GroupAppearance navigationBarControlGroupAppearance)
		{
			if (_headerAppearance != navigationBarControlGroupAppearance)
			{
				_headerAppearance = navigationBarControlGroupAppearance;
				Invalidate();
			}
		}

		internal void UpdateBorder(BorderStyle border)
		{
			if (_border != border)
			{
				_border = border;
				Invalidate();
			}
		}

		private NavigationBarFormatStyle ComputeButtonFormatStyle()
		{
			NavigationBarStateStyles navigationBarStateStyles = new NavigationBarStateStyles(_owner.GroupsStateStyles);
			if (_scrollLocation == ScrollLocation.GroupsLevel && _owner.SelectedGroup != null)
			{
				navigationBarStateStyles = navigationBarStateStyles.MergeWith(_owner.SelectedGroup.StateStyles);
			}
			NavigationBarFormatStyle navigationBarFormatStyle = _owner.CreateBaseFormatStyle();
			navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(navigationBarStateStyles.FormatStyle);
			if (!_isButtonEnabled)
			{
				navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(navigationBarStateStyles.DisabledFormatStyle);
			}
			else if (_isPressed && _isHot)
			{
				navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(navigationBarStateStyles.PressedFormatStyle);
			}
			else if (_isPressed)
			{
				navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(navigationBarStateStyles.SelectedFormatStyle);
			}
			else if (_isHot)
			{
				navigationBarFormatStyle = navigationBarFormatStyle.MergeWith(navigationBarStateStyles.HotFormatStyle);
			}
			navigationBarFormatStyle.BackColorAlphaMode = AlphaMode.Opaque;
			navigationBarFormatStyle.ForeColorAlphaMode = AlphaMode.Opaque;
			return navigationBarFormatStyle;
		}

		private void RenderButton(Graphics graphics)
		{
			NavigationBarFormatStyle navigationBarFormatStyle = ComputeButtonFormatStyle();
			Rectangle buttonBounds = _buttonBounds;
			_mordexColorSchemeRenderer.SetBaseColor(navigationBarFormatStyle.BackColor);
			_mordexColorSchemeRenderer.SetTextColor(navigationBarFormatStyle.GetForeColorOrDefault(_owner.GetEffectiveForeColor()));
			NavigationThemedPainter.FillHeader(graphics, _owner.GetTheme(), buttonBounds, navigationBarFormatStyle, _borderFlags);
			if (_orientation == NavigationBarOrientation.Horizontal)
			{
				_mordexColorSchemeRenderer.DrawHorizontalScrollArrow(graphics, buttonBounds, _direction, _isPressed, _isHot, _isButtonEnabled, _headerAppearance);
			}
			else
			{
				_mordexColorSchemeRenderer.DrawVerticalScrollArrow(graphics, buttonBounds, _direction, _isPressed, _isHot, _isButtonEnabled, _headerAppearance);
			}
			_mordexColorSchemeRenderer.DisposeAll();
		}

		public bool IsHot()
		{
			return _isHot;
		}

		public void SetHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				Invalidate();
			}
		}

		protected override void OnPaintBackground(PaintEventArgs eventArgs)
		{
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			_buttonBounds = base.ClientRectangle;
			RenderButton(eventArgs.Graphics);
		}

		protected override void OnSizeChanged(EventArgs eventArgs)
		{
			base.OnSizeChanged(eventArgs);
		}

		public void AddScrollHandler(EventHandler handler)
		{
			EventHandler currentHandler = _scrollHandler;
			EventHandler snapshot;
			do
			{
				snapshot = currentHandler;
				EventHandler combined = (EventHandler)Delegate.Combine(snapshot, handler);
				currentHandler = Interlocked.CompareExchange(ref _scrollHandler, combined, snapshot);
			}
			while ((object)currentHandler != snapshot);
		}

		protected virtual void OnScroll(EventArgs eventArgs)
		{
			if (_scrollHandler != null)
			{
				_scrollHandler(this, eventArgs);
			}
		}

		private void InitializeTimer()
		{
			_container = new Container();
			_timer = new System.Windows.Forms.Timer(_container);
			_timer.Interval = 150;
			_timer.Tick += OnTimerTick;
		}

		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			OnScroll(new EventArgs());
		}
	}
}
