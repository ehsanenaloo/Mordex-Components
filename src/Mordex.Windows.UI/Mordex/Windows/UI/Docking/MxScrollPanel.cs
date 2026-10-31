using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	[Designer("Mordex.Windows.UI.Design.ContainerDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultProperty("ChildControl")]
	[ToolboxItem(true)]
	public partial class MxScrollPanel : Control, IFormatStyleOwner
	{
		private const int DefaultScrollButtonSize = 10;

		private const int DefaultScrollStep = 5;

		private static readonly Size DefaultMinSize = new Size(100, 100);

		private static readonly Size DefaultControlSize = new Size(200, 100);

		private Container _container = new Container();

		private bool _isLayingOut;

		private bool _needsInitialLayout = true;

		private MxColorSchemeAppearanceState _mordexColorSchemeAppearanceState;


		private Size _minSize = DefaultMinSize;

		private int _scrollButtonSize = DefaultScrollButtonSize;

		private PanelInnerAreaStyle _innerAreaStyle = PanelInnerAreaStyle.Window;

		private FormatStyle _formatStyle;

		private int _verticalOffset;

		private int _horizontalOffset;

		private ColorSchemeControl _scrollLeftButton;

		private ColorSchemeControl _scrollUpButton;

		private ColorSchemeControl _scrollDownButton;

		private ColorSchemeControl _scrollRightButton;

		private Control _childControl;

		private ScrollMode _scrollMode;

		private int _scrollStep = DefaultScrollStep;

		private bool _isScrolling;

		private static readonly object _scrollEventKey = new object();

		Control IFormatStyleOwner.Owner => null;

		[Description("Specifies the minimum size that the control should have before displaying the scollbar.")]
		public Size MinSize
		{
			get
			{
				return _minSize;
			}
			set
			{
				if (_minSize != value)
				{
					_minSize = value;
					RefreshLayout();
				}
			}
		}

		[Description("Indicates the size of the scroll buttons.")]
		[DefaultValue(DefaultScrollButtonSize)]
		public int ScrollButtonSize
		{
			get
			{
				return _scrollButtonSize;
			}
			set
			{
				if (_scrollButtonSize != value)
				{
					_scrollButtonSize = value;
					RefreshLayout();
				}
			}
		}

		[DefaultValue(PanelInnerAreaStyle.Window)]
		[Description("Determines how the inner area is drawn.")]
		[Localizable(false)]
		public PanelInnerAreaStyle InnerAreaStyle
		{
			get
			{
				return _innerAreaStyle;
			}
			set
			{
				if (value == PanelInnerAreaStyle.UseDefault)
				{
					value = PanelInnerAreaStyle.Window;
				}
				if (!Enum.IsDefined(typeof(PanelInnerAreaStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_innerAreaStyle != value)
				{
					_innerAreaStyle = value;
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

		[Description("Represent the format style used to draw the inner area of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new FormatStyle(this);
				}
				return _formatStyle;
			}
		}

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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new int TabIndex
		{
			get
			{
				return base.TabIndex;
			}
			set
			{
				base.TabIndex = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DefaultValue(false)]
		[Browsable(false)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
			}
		}

		[DefaultValue(null)]
		[Description("Represents the control contained by the pager.")]
		[Editor(typeof(ControlEditor), typeof(UITypeEditor))]
		public Control ChildControl
		{
			get
			{
				return _childControl;
			}
			set
			{
				if (_childControl == value)
				{
					return;
				}
				if (_childControl != null && base.Controls.Contains(_childControl))
				{
					_childControl.DockChanged -= OnChildControlDockChanged;
					_childControl.Resize -= OnChildControlLocationChanged;
					_childControl.LocationChanged -= OnChildControlLocationChanged;
					base.Controls.Remove(_childControl);
				}
				_verticalOffset = 0;
				_horizontalOffset = 0;
				_childControl = value;
				if (_childControl != null)
				{
					_childControl.Dock = DockStyle.None;
					_childControl.DockChanged += OnChildControlDockChanged;
					_childControl.Resize += OnChildControlLocationChanged;
					_childControl.LocationChanged += OnChildControlLocationChanged;
					if (!base.Controls.Contains(_childControl))
					{
						base.Controls.Add(_childControl);
					}
				}
				RefreshLayout();
			}
		}

		[Description("Specifies the how the UIPager should scroll its contents.")]
		[DefaultValue(ScrollMode.Vertical)]
		public ScrollMode ScrollMode
		{
			get
			{
				return _scrollMode;
			}
			set
			{
				if (_scrollMode != value)
				{
					_scrollMode = value;
					if (_scrollMode == ScrollMode.Horizontal)
					{
						SetVerticalOffset(0);
					}
					else if (ScrollMode == ScrollMode.Vertical)
					{
						SetHorizontalOffset(0);
					}
					RefreshLayout();
				}
			}
		}

		[Description("Specifies the value that will be added or subtracted to the Location of the child control after scrolling the UIPager")]
		[DefaultValue(DefaultScrollStep)]
		public int ScrollStep
		{
			get
			{
				return _scrollStep;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentException();
				}
				if (_scrollStep != value)
				{
					_scrollStep = value;
					RefreshLayout();
				}
			}
		}

		[Description("Event fired when the user scrolls the UIPager control.")]
		public event ScrollEventHandler Scroll
		{
			add
			{
				base.Events.AddHandler(_scrollEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_scrollEventKey, value);
			}
		}

		public MxScrollPanel()
		{
			ResetContainer();
			_mordexColorSchemeAppearanceState = new MxColorSchemeAppearanceState();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			TabStop = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			_themeHook?.Stop();
			base.Dispose(disposing);
		}

		private void ResetContainer()
		{
			_container = new Container();
			base.Size = DefaultControlSize;
		}

		void IFormatStyleOwner.PropertyChange()
		{
			Invalidate(invalidateChildren: true);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinSize()
		{
			return _minSize != DefaultMinSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinSize()
		{
			MinSize = DefaultMinSize;
		}

		internal int GetVerticalOffset()
		{
			return _verticalOffset;
		}

		internal void SetVerticalOffset(int verticalOffset)
		{
			if (verticalOffset > 0 || base.Height > MinSize.Height)
			{
				verticalOffset = 0;
			}
			else if (base.Height - verticalOffset > MinSize.Height)
			{
				verticalOffset = base.Height - MinSize.Height;
			}
			if (_verticalOffset != verticalOffset)
			{
				_verticalOffset = verticalOffset;
				RefreshLayout();
			}
		}

		internal int GetHorizontalOffset()
		{
			return _horizontalOffset;
		}

		internal void SetHorizontalOffset(int uiPagerInteger)
		{
			if (uiPagerInteger > 0 || base.Width > MinSize.Width)
			{
				uiPagerInteger = 0;
			}
			else if (base.Width - uiPagerInteger > MinSize.Width)
			{
				uiPagerInteger = base.Width - MinSize.Width;
			}
			if (_horizontalOffset != uiPagerInteger)
			{
				_horizontalOffset = uiPagerInteger;
				RefreshLayout();
			}
		}

		private void SetScrollLeftButton(ColorSchemeControl scrollButton)
		{
			ReplaceScrollButton(ref _scrollLeftButton, scrollButton, OnScrollLeft);
		}

		private ColorSchemeControl GetScrollUpButton()
		{
			return _scrollUpButton;
		}

		private void SetScrollUpButton(ColorSchemeControl scrollButton)
		{
			ReplaceScrollButton(ref _scrollUpButton, scrollButton, OnScrollUp);
		}

		private ColorSchemeControl GetScrollDownButton()
		{
			return _scrollDownButton;
		}

		private void SetScrollDownButton(ColorSchemeControl scrollButton)
		{
			ReplaceScrollButton(ref _scrollDownButton, scrollButton, OnScrollDown);
		}

		private void SetScrollRightButton(ColorSchemeControl scrollButton)
		{
			ReplaceScrollButton(ref _scrollRightButton, scrollButton, OnScrollRight);
		}

		private void ReplaceScrollButton(ref ColorSchemeControl current, ColorSchemeControl replacement, EventHandler clickHandler)
		{
			if (current == replacement)
			{
				return;
			}
			if (replacement == null)
			{
				current.Visible = false;
				return;
			}
			if (current != null)
			{
				current.RemoveScrollHandler(clickHandler);
				current.Dispose();
				current = null;
			}
			current = replacement;
			current.AddScrollHandler(clickHandler);
			base.Controls.Add(current);
		}

		private void ApplyAppearance(ColorSchemeControl scrollButton)
		{
			scrollButton._isThemed = true;
			scrollButton._colorSchemeAppearance = _mordexColorSchemeAppearanceState;
		}
		private void OnScrollDown(object sender, EventArgs eventArgs)
		{
			int newPosition = 0;
			int oldPosition = 0;
			if (!_isScrolling)
			{
				_isScrolling = true;
				oldPosition = -GetVerticalOffset() / _scrollStep;
				SetVerticalOffset(GetVerticalOffset() - _scrollStep);
				newPosition = -GetVerticalOffset() / _scrollStep;
				_isScrolling = false;
				if (_scrollDownButton != null)
				{
					ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.SmallIncrement, oldPosition, newPosition);
					OnScroll(e);
				}
			}
		}

		private void OnScrollRight(object sender, EventArgs eventArgs)
		{
			int newPosition = 0;
			int oldPosition = 0;
			if (!_isScrolling)
			{
				_isScrolling = true;
				oldPosition = -GetHorizontalOffset() / _scrollStep;
				SetHorizontalOffset(GetHorizontalOffset() - _scrollStep);
				newPosition = -GetHorizontalOffset() / _scrollStep;
				_isScrolling = false;
				if (_scrollRightButton != null)
				{
					ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.SmallIncrement, oldPosition, newPosition);
					OnScroll(e);
				}
			}
		}

		private void OnScrollLeft(object sender, EventArgs eventArgs)
		{
			int newPosition = 0;
			int oldPosition = 0;
			if (!_isScrolling)
			{
				_isScrolling = true;
				oldPosition = -GetHorizontalOffset() / _scrollStep;
				SetHorizontalOffset(GetHorizontalOffset() + _scrollStep);
				newPosition = -GetHorizontalOffset() / _scrollStep;
				_isScrolling = false;
				if (_scrollLeftButton != null)
				{
					ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.SmallIncrement, oldPosition, newPosition);
					OnScroll(e);
				}
			}
		}

		private void OnScrollUp(object sender, EventArgs eventArgs)
		{
			int newPosition = 0;
			int oldPosition = 0;
			if (!_isScrolling)
			{
				_isScrolling = true;
				oldPosition = -GetVerticalOffset() / _scrollStep;
				SetVerticalOffset(GetVerticalOffset() + _scrollStep);
				newPosition = -GetVerticalOffset() / _scrollStep;
				_isScrolling = false;
				if (_scrollUpButton != null)
				{
					ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.SmallIncrement, oldPosition, newPosition);
					OnScroll(e);
				}
			}
		}

		protected virtual void OnScroll(ScrollEventArgs e)
		{
			if (base.Events[_scrollEventKey] is ScrollEventHandler scrollEventHandler)
			{
				scrollEventHandler(this, e);
			}
		}

		private void OnChildControlDockChanged(object sender, EventArgs eventArgs)
		{
			if (ChildControl != null)
			{
				ChildControl.Dock = DockStyle.None;
			}
		}

		private void OnChildControlLocationChanged(object sender, EventArgs eventArgs)
		{
			if (!_isLayingOut && _childControl != null)
			{
				if (ScrollMode == ScrollMode.Vertical)
				{
					SetVerticalOffset(ChildControl.Top);
				}
				else if (ScrollMode == ScrollMode.Both)
				{
					SetVerticalOffset(ChildControl.Top);
					SetHorizontalOffset(ChildControl.Left);
				}
				else
				{
					SetHorizontalOffset(ChildControl.Left);
				}
				RefreshLayout();
			}
		}

		private void RefreshLayout()
		{
			Invalidate(invalidateChildren: true);
			LayoutChildAndButtons();
		}

		private void LayoutChildAndButtons()
		{
			if (_isLayingOut)
			{
				return;
			}
			_isLayingOut = true;
			try
			{
				if (ChildControl != null)
				{
					Control childControl = ChildControl;
					childControl.SuspendLayout();
					try
					{
						Point location = Point.Empty;
						Size size = base.Size;
						if (_scrollMode == ScrollMode.Vertical || _scrollMode == ScrollMode.Both)
						{
							location = new Point(_horizontalOffset, _verticalOffset);
							if (MinSize.Height + _verticalOffset > base.Size.Height)
							{
								if (_scrollDownButton == null)
								{
									SetScrollDownButton(new ColorSchemeControl());
									GetScrollDownButton().SetScrollStep(-_scrollStep);
								}
								_scrollDownButton.Visible = true;
								_scrollDownButton._isVertical = true;
								GetScrollDownButton().SetMinimum(base.Height - MinSize.Height);
								_scrollDownButton.Bounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Bottom - _scrollButtonSize, base.ClientRectangle.Width, _scrollButtonSize);
								ApplyAppearance(_scrollDownButton);
								_scrollDownButton.BringToFront();
								_scrollDownButton.SetPosition(-GetVerticalOffset());
							}
							else
							{
								if (MinSize.Height > base.Size.Height)
								{
									SetVerticalOffset(base.Size.Height - MinSize.Height);
								}
								else
								{
									SetVerticalOffset(0);
								}
								SetScrollDownButton(null);
							}
							if (_verticalOffset < 0)
							{
								if (GetScrollUpButton() == null)
								{
									SetScrollUpButton(new ColorSchemeControl());
									GetScrollUpButton().SetScrollStep(_scrollStep);
								}
								_scrollUpButton.Visible = true;
								_scrollUpButton._isVertical = true;
								GetScrollUpButton().SetMaximum(MinSize.Height - base.Height);
								_scrollUpButton.Bounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, base.ClientRectangle.Width, _scrollButtonSize);
								ApplyAppearance(_scrollUpButton);
								_scrollUpButton.BringToFront();
								_scrollUpButton.SetPosition(GetVerticalOffset());
							}
							else
							{
								SetScrollUpButton(null);
							}
							location = new Point(_horizontalOffset, _verticalOffset);
							size = new Size(size.Width, base.Size.Height - _verticalOffset);
						}
						else
						{
							SetScrollDownButton(null);
							SetScrollUpButton(null);
						}
						if (_scrollMode == ScrollMode.Horizontal || _scrollMode == ScrollMode.Both)
						{
							location = new Point(_horizontalOffset, _verticalOffset);
							if (MinSize.Width + _horizontalOffset > base.Size.Width)
							{
								if (_scrollRightButton == null)
								{
									SetScrollRightButton(new ColorSchemeControl());
									_scrollRightButton.SetScrollStep(-_scrollStep);
								}
								_scrollRightButton.Visible = true;
								_scrollRightButton._isVertical = false;
								_scrollRightButton.SetMinimum(base.Width - MinSize.Width);
								_scrollRightButton.Bounds = new Rectangle(base.ClientRectangle.Right - _scrollButtonSize, base.ClientRectangle.Top, _scrollButtonSize, base.ClientRectangle.Height);
								ApplyAppearance(_scrollRightButton);
								_scrollRightButton.BringToFront();
								_scrollRightButton.SetPosition(-GetHorizontalOffset());
							}
							else
							{
								if (MinSize.Width > base.Size.Width)
								{
									SetHorizontalOffset(base.Size.Width - MinSize.Width);
								}
								else
								{
									SetHorizontalOffset(0);
								}
								SetScrollRightButton(null);
							}
							if (GetHorizontalOffset() < 0)
							{
								if (_scrollLeftButton == null)
								{
									SetScrollLeftButton(new ColorSchemeControl());
									_scrollLeftButton.SetScrollStep(_scrollStep);
								}
								_scrollLeftButton.Visible = true;
								_scrollLeftButton._isVertical = false;
								_scrollLeftButton.SetMaximum(MinSize.Width - base.Width);
								_scrollLeftButton.Bounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, _scrollButtonSize, base.ClientRectangle.Height);
								ApplyAppearance(_scrollLeftButton);
								_scrollLeftButton.BringToFront();
								_scrollLeftButton.SetPosition(GetHorizontalOffset());
							}
							else
							{
								SetScrollLeftButton(null);
							}
							location = new Point(_horizontalOffset, _verticalOffset);
							size = new Size(base.Size.Width - _horizontalOffset, size.Height);
						}
						else
						{
							SetScrollLeftButton(null);
							SetScrollRightButton(null);
						}
						ChildControl.Bounds = new Rectangle(location, size);
					}
					finally
					{
						childControl.ResumeLayout(performLayout: true);
					}
				}
			}
			finally
			{
				_isLayingOut = false;
			}
			_needsInitialLayout = false;
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			if (e.Control.GetType() != typeof(ColorSchemeControl))
			{
				if (ChildControl == null)
				{
					ChildControl = e.Control;
				}
				else if (e.Control != ChildControl)
				{
					throw new InvalidOperationException("Only one control can be assigned to the Pager");
				}
			}
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);
			if (e.Control.GetType() != typeof(ColorSchemeControl) && ChildControl == e.Control)
			{
				ChildControl = null;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RefreshLayout();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			_mordexColorSchemeAppearanceState.RebuildColors();
			Invalidate();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (e.Delta > 0)
			{
				if (GetScrollUpButton() != null)
				{
					OnScrollUp(this, EventArgs.Empty);
				}
			}
			else if (GetScrollDownButton() != null)
			{
				OnScrollDown(this, EventArgs.Empty);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			if (_needsInitialLayout)
			{
				LayoutChildAndButtons();
			}
			if (InnerAreaStyle == PanelInnerAreaStyle.UseFormatStyle)
			{
				FormatStyle uiFormatStyle = new FormatStyle(FormatStyle);
				RenderingHelper.DrawFormatStyleBackground(e.Graphics, uiFormatStyle, base.ClientRectangle, base.ClientRectangle);
				uiFormatStyle.DisposeResources();
			}
			else
			{
				DrawBackground(e.Graphics);
			}
		}

		private void DrawBackground(Graphics graphics)
		{
			{
				PanelColors slot = _innerAreaStyle == PanelInnerAreaStyle.AppWorkspace ? PanelColors.InnerWorkspace : _innerAreaStyle == PanelInnerAreaStyle.ContainerPanel ? PanelColors.InnerContainer : PanelColors.InnerWindow;
				graphics.FillRectangle(GdiCache.Brush(_mordexColorSchemeAppearanceState.GetColor(slot)), base.ClientRectangle);
				return;
			}
		}
	}
}
