using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	[ToolboxItem(true)]
	[Designer("Mordex.Windows.Ribbon.Design.OfficeFormAdornerDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class MxRibbonFormAdorner : Component, ISupportInitialize
	{
		private Form _form;

		internal RibbonWindowMessageHandler _ribbonWindowMessageHandler;

		internal RibbonColorScheme _colorScheme;

		private string _documentName;

		private string _applicationName;

		private Font _titleBarFont;

		private Form _activeMdiChild;

		private bool _enableGlassEffect = true;

		internal bool _isInitializing;

		private IContainer _components;

		[Description("Represents the Form that gets the ribbon title bar and frame")]
		[DefaultValue(null)]
		public Form Form
		{
			get
			{
				return _form;
			}
			set
			{
				if (_form == value)
				{
					return;
				}
				if (_form != null)
				{
					_form.HandleCreated -= OnFormHandleCreated;
					_form.MdiChildActivate -= OnFormMdiChildActivate;
					_form.Disposed -= OnFormDisposed;
					if (_ribbonWindowMessageHandler != null)
					{
						if (_form.WindowState == FormWindowState.Maximized)
						{
							_form.Region = null;
							_ribbonWindowMessageHandler.InvalidateNonClientArea();
						}
						_ribbonWindowMessageHandler.Dispose();
						_ribbonWindowMessageHandler = null;
						try
						{
							if (!_form.IsDisposed && !_form.Disposing)
							{
								_form.Height--;
								_form.Height++;
							}
						}
						catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
						{
							// The form can be closing; the size nudge only forces a non-client repaint.
						}
					}
				}
				_form = value;
				if (_form != null)
				{
					_form.PerformLayout();
					_form.HandleCreated += OnFormHandleCreated;
					_form.MdiChildActivate += OnFormMdiChildActivate;
					_form.Disposed += OnFormDisposed;
					CreateMessageHandler(true, false);
				}
				UpdateThemedFrame();
			}
		}

		private MxThemedFormFrame _themedFrame;

		private FormFrameMode _frameMode = FormFrameMode.FollowTheme;

		[DefaultValue(FormFrameMode.FollowTheme)]
		[Description("Follow theme gives the form the caption and frame of the default theme. System leaves the frame of Windows alone. A ribbon that paints the caption itself keeps its own caption.")]
		public FormFrameMode FrameMode
		{
			get
			{
				return _frameMode;
			}
			set
			{
				if (_frameMode != value)
				{
					_frameMode = value;
					UpdateThemedFrame();
				}
			}
		}

		// The older handler paints a frame of its own when Windows has no desktop composition, so the themed frame stays out.
		private void UpdateThemedFrame()
		{
			if (_form == null || base.DesignMode)
			{
				if (_themedFrame != null)
				{
					_themedFrame.Dispose();
					_themedFrame = null;
				}
				return;
			}
			if (_themedFrame == null)
			{
				_themedFrame = new MxThemedFormFrame();
			}
			_themedFrame.Mode = _ribbonWindowMessageHandler != null ? FormFrameMode.System : _frameMode;
			_themedFrame.Form = _form;
		}

		[DefaultValue(true)]
		[Description("Specifies whether the glass effect for the Form's caption is enabled when the application is running under Windows Vista.")]
		public bool EnableGlassEffect
		{
			get
			{
				return _enableGlassEffect;
			}
			set
			{
				if (_enableGlassEffect != value)
				{
					_enableGlassEffect = value;
					if (_form != null && !base.DesignMode)
					{
						throw new InvalidOperationException("This property can only be set before the Form property is assigned.");
					}
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the Text of the Form.")]
		public string DocumentName
		{
			get
			{
				if (_documentName.Length == 0 && _form != null)
				{
					return _form.Text;
				}
				return _documentName;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_documentName != value)
				{
					_documentName = value;
					RelayoutCaption();
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("Represents the name of the application that will be displayed in the title bar of Form.")]
		public string ApplicationName
		{
			get
			{
				return _applicationName;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_applicationName != value)
				{
					_applicationName = value;
					RelayoutCaption();
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

		[Description("Represents the Font used to draw the text in the title bar of the Form.")]
		[DefaultValue(null)]
		public Font TitleBarFont
		{
			get
			{
				return _titleBarFont;
			}
			set
			{
				if (_titleBarFont != value)
				{
					_titleBarFont = value;
					if (_ribbonWindowMessageHandler != null)
					{
						_ribbonWindowMessageHandler._font?.Dispose();
						_ribbonWindowMessageHandler._font = null;
					}
					RedrawAndRelayoutCaption();
				}
			}
		}

		public MxRibbonFormAdorner()
		{
			CreateComponents();
			InitializeOfficeDefaults();
		}

		public MxRibbonFormAdorner(IContainer container)
		{
			container.Add(this);
			CreateComponents();
			InitializeOfficeDefaults();
		}

		private void InitializeOfficeDefaults()
		{
			_documentName = "";
			_applicationName = "";
			RebuildColorScheme();
		}

		public static string GetRibbonVersionString()
		{
			string localPath = typeof(MxRibbon).Assembly.Location;
			if (string.IsNullOrEmpty(localPath))
			{
				// Single-file apps have no assembly path.
				return typeof(MxRibbon).Assembly.GetName().Version?.ToString() ?? "";
			}
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private void OnFormDisposed(object sender, EventArgs eventArgs)
		{
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.Dispose();
				_ribbonWindowMessageHandler = null;
			}
		}

		private void OnFormMdiChildActivate(object sender, EventArgs eventArgs)
		{
			TrackActiveMdiChild();
			RelayoutCaption();
		}

		private void OnFormHandleCreated(object sender, EventArgs eventArgs)
		{
			bool addOffset = true;
			if (_ribbonWindowMessageHandler != null)
			{
				if (IsInDesignMode())
				{
					_ribbonWindowMessageHandler.ReleaseHandle();
					_ribbonWindowMessageHandler.AssignHandle(Form.Handle);
					return;
				}
				_ribbonWindowMessageHandler.Dispose();
				addOffset = false;
			}
			CreateMessageHandler(addOffset, true);
			UpdateThemedFrame();
		}

		private void OnFormSizeChanged(object sender, EventArgs eventArgs)
		{
			RelayoutCaption();
		}

		private void TrackActiveMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.SizeChanged -= OnFormSizeChanged;
			}
			if (_form != null)
			{
				_activeMdiChild = _form.ActiveMdiChild;
				if (_activeMdiChild != null)
				{
					_activeMdiChild.SizeChanged += OnFormSizeChanged;
				}
			}
			else
			{
				_activeMdiChild = null;
			}
		}

		private void CreateMessageHandler(bool addOffset, bool runTimeForm)
		{
			bool isCompositionEnabled = false;
			if (_enableGlassEffect)
			{
				NativeMethods.DwmIsCompositionEnabled(ref isCompositionEnabled);
				if (isCompositionEnabled && base.DesignMode)
				{
					isCompositionEnabled = false;
				}
			}
			if (!isCompositionEnabled && _form != null && _form.IsHandleCreated)
			{
				_ribbonWindowMessageHandler = new FormAdornerMessageHandler(Form, this, addOffset, runTimeForm);
				_ribbonWindowMessageHandler.InvalidateNonClientArea();
			}
		}

		internal bool IsInDesignMode()
		{
			return base.DesignMode;
		}

		private ThemeSubscription _themeSubscription;

		private ThemeSubscription ThemeSub => _themeSubscription ?? (_themeSubscription = new ThemeSubscription(OnDefaultThemeChanged));

		private void OnDefaultThemeChanged()
		{
			RebuildColorScheme();
		}

		private void RebuildColorScheme()
		{
			_colorScheme = ThemedRibbon.CreateScheme(MxThemeManager.ResolveDefault(96));
			ThemeSub.Set(true);
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.InvalidateNonClientArea();
			}
		}

		private void RedrawAndRelayoutCaption()
		{
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.RedrawFrame();
				_ribbonWindowMessageHandler.RefreshCaptionLayout();
			}
		}

		private void RelayoutCaption()
		{
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.RefreshCaptionLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDocumentName()
		{
			return _documentName.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDocumentName()
		{
			DocumentName = "";
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			_isInitializing = false;
		}

		protected override void Dispose(bool disposing)
		{
			_themeSubscription?.Set(false);
			if (disposing)
			{
				if (_components != null)
				{
					_components.Dispose();
				}
				if (_activeMdiChild != null)
				{
					_activeMdiChild.SizeChanged -= OnFormSizeChanged;
					_activeMdiChild = null;
				}
				Form = null;
			}
			base.Dispose(disposing);
		}

		private void CreateComponents()
		{
			_components = new Container();
		}
	}
}
