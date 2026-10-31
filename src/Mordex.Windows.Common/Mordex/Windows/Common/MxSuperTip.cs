using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Common
{
	[ProvideProperty("SuperTip", typeof(Component))]
	public class MxSuperTip : Component, IExtenderProvider
	{
		private Hashtable _superTipsByComponent;

		private SuperTipWindow _tipWindow;

		private Control _hoveredControl;

		private Font _font;

		private Font _headerFont;

		private Font _footerFont;

		private int _bodyWidth;

		private bool _rightToLeft;

		private int _autoPopDelay;

		private int _initialDelay;

		private bool _showAlways;

		private ImageList _imageList;

		private static object _toolTipPopUpEventKey = new object();

		private Color _backColor;

		private Color _foreColor;

		private Color _headerForeColor;

		private Color _footerForeColor;

		private bool _enabled = true;

		private Rectangle _toolTipArea;

		private Control _areaOwner;

		private SuperTipSettings _areaSettings;

		private SuperTipMessageFilter _areaMessageFilter;

		private bool _areaTipShown;

		private IContainer _components;

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
					CommonState.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[DefaultValue(210)]
		public int BodyWidth
		{
			get
			{
				return _bodyWidth;
			}
			set
			{
				_bodyWidth = value;
			}
		}

		[DefaultValue(false)]
		public bool RightToLeft
		{
			get
			{
				return _rightToLeft;
			}
			set
			{
				_rightToLeft = value;
			}
		}

		[DefaultValue(5000)]
		public int AutoPopDelay
		{
			get
			{
				return _autoPopDelay;
			}
			set
			{
				_autoPopDelay = value;
			}
		}

		[DefaultValue(900)]
		public int InitialDelay
		{
			get
			{
				return _initialDelay;
			}
			set
			{
				_initialDelay = value;
			}
		}

		[DefaultValue(false)]
		public bool ShowAlways
		{
			get
			{
				return _showAlways;
			}
			set
			{
				_showAlways = value;
			}
		}

		[DefaultValue(null)]
		public ImageList ImageList
		{
			get
			{
				return _imageList;
			}
			set
			{
				_imageList = value;
			}
		}

		[DefaultValue(null)]
		public Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (_font != value)
				{
					_font = value;
				}
			}
		}

		[DefaultValue(null)]
		public Font HeaderFont
		{
			get
			{
				return _headerFont;
			}
			set
			{
				if (_headerFont != value)
				{
					_headerFont = value;
				}
			}
		}

		[DefaultValue(null)]
		public Font FooterFont
		{
			get
			{
				return _footerFont;
			}
			set
			{
				if (_footerFont != value)
				{
					_footerFont = value;
				}
			}
		}

		public Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				_backColor = value;
			}
		}

		public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;
			}
		}

		/// <summary>
		/// True to draw the tip from the default theme of <see cref="MxThemeManager"/> (tip back and text colors, border) and
		/// ignore the Office color scheme. A fore color that was set by the developer still wins over the theme.
		/// </summary>
		[DefaultValue(false)]
		[Description("True to draw the tips with the colors of the default theme.")]
		public bool UseDefaultTheme { get; set; }

		public Color HeaderForeColor
		{
			get
			{
				if (_headerForeColor.IsEmpty)
				{
					return ForeColor;
				}
				return _headerForeColor;
			}
			set
			{
				_headerForeColor = value;
			}
		}

		public Color FooterForeColor
		{
			get
			{
				if (_footerForeColor.IsEmpty)
				{
					return ForeColor;
				}
				return _footerForeColor;
			}
			set
			{
				_footerForeColor = value;
			}
		}

		public bool Active
		{
			get
			{
				return _tipWindow != null && _tipWindow.IsScheduled;
			}
		}

		[DefaultValue(true)]
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					if (!_enabled)
					{
						HideActiveToolTip();
					}
				}
			}
		}

		public event ToolTipPopUpEventHandler ToolTipPopUp
		{
			add
			{
				base.Events.AddHandler(_toolTipPopUpEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_toolTipPopUpEventKey, value);
			}
		}

		public MxSuperTip()
		{
			CreateComponentContainer();
			InitializeDefaults();
		}

		public MxSuperTip(IContainer container)
		{
			container.Add(this);
			CreateComponentContainer();
			InitializeDefaults();
		}

		private void InitializeDefaults()
		{
			_superTipsByComponent = new Hashtable();
			_autoPopDelay = 2000;
			_initialDelay = 900;
			_backColor = SystemColors.Info;
			_foreColor = Color.FromArgb(76, 76, 76);
			_headerForeColor = Color.Empty;
			_footerForeColor = Color.Empty;
			_bodyWidth = 210;
		}

		bool IExtenderProvider.CanExtend(object target)
		{
			if (target is Control)
			{
				return true;
			}
			return false;
		}

		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design")]
		[DefaultValue(null)]
		public SuperTipSettings GetSuperTip(Component component)
		{
			if (_superTipsByComponent.ContainsKey(component))
			{
				return _superTipsByComponent[component] as SuperTipSettings;
			}
			return null;
		}

		public void SetSuperTip(Component component, SuperTipSettings value)
		{
			if (!(component is Control control))
			{
				return;
			}
			if (value == null)
			{
				if (_superTipsByComponent.ContainsKey(component))
				{
					control.MouseEnter -= OnControlMouseEnter;
					control.MouseLeave -= OnControlMouseLeave;
					_superTipsByComponent.Remove(component);
				}
			}
			else if (_superTipsByComponent.ContainsKey(component))
			{
				_superTipsByComponent[control] = value;
			}
			else
			{
				_superTipsByComponent.Add(control, value);
				control.MouseEnter += OnControlMouseEnter;
				control.MouseLeave += OnControlMouseLeave;
			}
		}

		private void OnControlMouseEnter(object sender, EventArgs eventArgs)
		{
			ShowSuperTipFor(sender as Control);
		}

		private void OnControlMouseLeave(object sender, EventArgs eventArgs)
		{
			HideCurrentTip();
		}

		private SuperTipSettings ResolveSuperTip(Control component)
		{
			if (component == _areaOwner)
			{
				return _areaSettings;
			}
			return GetSuperTip(component);
		}

		internal void ShowSuperTipFor(Control control)
		{
			if (!_enabled)
			{
				return;
			}
			if (control == null)
			{
				HideCurrentTip();
			}
			else if (control != _hoveredControl)
			{
				HideCurrentTip();
				ToolTipPopUpEventArgs e = new ToolTipPopUpEventArgs(control, ResolveSuperTip(control));
				OnToolTipPopUp(e);
				if (!e.Cancel)
				{
					_hoveredControl = control;
					_tipWindow = new SuperTipWindow(this, e.ToolTipSettings);
					_tipWindow.Schedule(control, Point.Empty);
				}
				else
				{
					_hoveredControl = null;
				}
			}
		}

		internal void HideCurrentTip()
		{
			if (_tipWindow != null)
			{
				_tipWindow.Dispose();
				_tipWindow = null;
			}
			_hoveredControl = null;
		}

		private Font GetBaseFont()
		{
			return _font ?? _hoveredControl?.Font ?? Control.DefaultFont;
		}

		// The create methods return new fonts that the caller owns and must dispose.
		internal Font CreateHeaderFont()
		{
			return _headerFont != null ? (Font)_headerFont.Clone() : new Font(GetBaseFont(), FontStyle.Bold);
		}

		internal Font CreateFooterFont()
		{
			return _footerFont != null ? (Font)_footerFont.Clone() : new Font(GetBaseFont(), FontStyle.Bold);
		}

		internal Font CreateBodyFont()
		{
			return (Font)GetBaseFont().Clone();
		}

		protected virtual void OnToolTipPopUp(ToolTipPopUpEventArgs e)
		{
			if (base.Events[_toolTipPopUpEventKey] is ToolTipPopUpEventHandler toolTipPopUpEventHandler)
			{
				toolTipPopUpEventHandler(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !_backColor.Equals(SystemColors.Info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColor()
		{
			BackColor = SystemColors.Info;
		}

		public bool ShouldSerializeForeColor()
		{
			return !_foreColor.Equals(Color.FromArgb(76, 76, 76));
		}

		public void ResetForeColor()
		{
			ForeColor = Color.FromArgb(76, 76, 76);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderForeColor()
		{
			return !_headerForeColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderForeColor()
		{
			_headerForeColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFooterForeColor()
		{
			return !_footerForeColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFooterForeColor()
		{
			_footerForeColor = Color.Empty;
		}

		public void Show(SuperTipSettings settings)
		{
			Show(settings, Point.Empty, AutoPopDelay);
		}

		public void Show(SuperTipSettings settings, Point position)
		{
			Show(settings, position, AutoPopDelay);
		}

		public void Show(SuperTipSettings settings, Point position, int duration)
		{
			if (settings == null)
			{
				throw new ArgumentNullException();
			}
			HideCurrentTip();
			ClearToolTipArea();
			ToolTipPopUpEventArgs e = new ToolTipPopUpEventArgs(null, settings);
			OnToolTipPopUp(e);
			if (!e.Cancel)
			{
				_hoveredControl = null;
				_tipWindow = new SuperTipWindow(this, e.ToolTipSettings);
				_tipWindow.InitialDelay = InitialDelay;
				if (duration <= 0)
				{
					_tipWindow.ShowAlways = true;
				}
				else
				{
					_tipWindow.ShowAlways = false;
					_tipWindow.AutoPopDelay = duration;
				}
				_tipWindow.Schedule(null, position);
			}
		}

		public void Show(SuperTipSettings settings, Control tooltipOwner)
		{
			Show(settings, tooltipOwner, Point.Empty, AutoPopDelay);
		}

		public void Show(SuperTipSettings settings, Control tooltipOwner, Point position)
		{
			Show(settings, tooltipOwner, position, AutoPopDelay);
		}

		public void Show(SuperTipSettings settings, Control tooltipOwner, Point position, int duration)
		{
			if (settings == null)
			{
				throw new ArgumentNullException();
			}
			HideCurrentTip();
			ClearToolTipArea();
			ToolTipPopUpEventArgs e = new ToolTipPopUpEventArgs(null, settings);
			OnToolTipPopUp(e);
			if (!e.Cancel)
			{
				_hoveredControl = null;
				_tipWindow = new SuperTipWindow(this, e.ToolTipSettings);
				_tipWindow.InitialDelay = InitialDelay;
				if (duration <= 0)
				{
					_tipWindow.ShowAlways = true;
				}
				else
				{
					_tipWindow.ShowAlways = false;
					_tipWindow.AutoPopDelay = duration;
				}
				_tipWindow.Schedule(tooltipOwner, position);
			}
		}

		public void HideActiveToolTip()
		{
			HideCurrentTip();
			ClearToolTipArea();
		}

		internal void ShowAreaSuperTip()
		{
			Point position = Cursor.Position;
			if (_toolTipArea.Contains(position))
			{
				if (!_areaTipShown)
				{
					HideCurrentTip();
					_hoveredControl = null;
					_tipWindow = new SuperTipWindow(this, _areaSettings);
					_tipWindow.SchedulePoint(Point.Empty);
					_areaTipShown = true;
				}
			}
			else
			{
				ClearToolTipArea();
			}
		}

		public void SetToolTipArea(SuperTipSettings settings, Control owner, Rectangle toolTipArea)
		{
			if (owner != _areaOwner || toolTipArea != _toolTipArea)
			{
				ReleaseToolTipAreaHooks();
				_toolTipArea = toolTipArea;
				_areaOwner = owner;
				if (owner != null)
				{
					owner.MouseMove += OnControlMouseMove;
				}
				else
				{
					_areaMessageFilter = new SuperTipMessageFilter(this);
				}
			}
			_areaSettings = settings;
		}

		public void ClearToolTipArea()
		{
			if (_tipWindow != null)
			{
				_tipWindow.Dispose();
				_tipWindow = null;
			}
			_areaTipShown = false;
			_hoveredControl = null;
			_toolTipArea = Rectangle.Empty;
			ReleaseToolTipAreaHooks();
			_areaSettings = null;
		}

		private void ReleaseToolTipAreaHooks()
		{
			if (_areaOwner != null)
			{
				_areaOwner.MouseMove -= OnControlMouseMove;
				_areaOwner = null;
			}
			if (_areaMessageFilter != null)
			{
				_areaMessageFilter.Detach();
				_areaMessageFilter = null;
			}
		}

		private void OnControlMouseMove(object sender, MouseEventArgs eventArgs)
		{
			if (_toolTipArea.Contains(eventArgs.X, eventArgs.Y))
			{
				ShowSuperTipFor(_areaOwner);
			}
			else
			{
				ClearToolTipArea();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_components != null)
				{
					_components.Dispose();
				}
				ClearToolTipArea();
				HideCurrentTip();
				foreach (Control key in _superTipsByComponent.Keys)
				{
					key.MouseEnter -= OnControlMouseEnter;
					key.MouseLeave -= OnControlMouseLeave;
				}
				_superTipsByComponent.Clear();
				_superTipsByComponent = null;
			}
			base.Dispose(disposing);
		}

		private void CreateComponentContainer()
		{
			_components = new Container();
		}
	}
}
