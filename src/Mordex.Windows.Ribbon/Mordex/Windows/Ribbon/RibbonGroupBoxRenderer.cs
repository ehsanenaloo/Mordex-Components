using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonGroupBoxRenderer : CommandContainerRenderer, IKeyboardNavigable, IDropDownItem, ISupportsDesignerMouse
	{
		private IList<RibbonCommandColumn> _commandColumns;

		internal IList<RibbonCommandColumn> _galleryColumns;

		public bool _isCollapsed;

		internal int _captionWidth;

		internal int _fullCaptionWidth;

		internal int _minCaptionWidth;

		internal int _contentWidth;

		private Rectangle _lastWordBounds;

		private bool _isSingleWordCaption;

		private int _captionGapWidth;

		private Rectangle _captionBarBounds;

		internal RibbonGroupDialogLauncherRenderer _dialogLauncher;

		private const int OpaqueAlpha = 255;

		private const int HotStartAlpha = 100;

		private const int ColdStartAlpha = 200;

		private const int FadeStepAlpha = 55;

		private const int FadeEndAlpha = 100;

		private const int FadeStartIntervalMilliseconds = 100;

		private const int FadeIntervalMilliseconds = 50;

		private Timer _fadeTimer;

		private int _fadeAlpha;

		private Rectangle _collapsedIconBounds;

		private bool _isPopupOpen;

		private bool _isDesignerMouseIn;

		private bool _drawsBackground = true;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				return AccessibleRole.ToolBar;
			}
		}

		public override string AccessibleName
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleName != null && GetCommand().AccessibleName.Length > 0)
				{
					return GetCommand().AccessibleName;
				}
				if (GetRibbonGroup() != null && GetRibbonGroup().Text != null)
				{
					string text = GetRibbonGroup().Text.Replace("&", "");
					if (text.Length > 0)
					{
						return text;
					}
				}
				return base.AccessibleName;
			}
		}

		public override string AccessibleDescription
		{
			get
			{
				if (GetRibbonGroup() != null)
				{
					if (GetCommand().AccessibleDescription != null && GetCommand().AccessibleDescription.Length > 0)
					{
						return GetCommand().AccessibleDescription;
					}
					if (GetRibbonGroup().SuperTipSettings != null)
					{
						string text = GetRibbonGroup().SuperTipSettings.Text;
						if (text != null && text.Length > 0)
						{
							return text;
						}
					}
				}
				return base.AccessibleDescription;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_isCollapsed)
				{
					return "Open";
				}
				return base.DefaultAction;
			}
		}

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (base.Hot != value)
				{
					base.Hot = value;
					if (!value && _dialogLauncher != null)
					{
						_dialogLauncher.Hot = false;
					}
					if (!_isCollapsed)
					{
						StartFadeAnimation();
					}
				}
			}
		}

		internal RibbonGroupBoxRenderer(RibbonGroup group, MxRibbon ribbon, ICommandRenderHost owner)
			: base(group, owner, null)
		{
			if (group != null)
			{
				_isSingleWordCaption = group.Text.IndexOf(" ") <= 0;
			}
			_commandBaseRendererRibbon = ribbon;
			_fadeAlpha = 255;
		}

		public RibbonGroup GetRibbonGroup()
		{
			return _commandBaseRendererCommand as RibbonGroup;
		}

		public string GetCaptionText()
		{
			if (!_isCollapsed || _isSingleWordCaption)
			{
				return GetRibbonGroup().Text;
			}
			return GetRibbonGroup().Text + "  ";
		}

		public bool IsDesignerMouseIn()
		{
			return _isDesignerMouseIn;
		}

		public void SetDesignerMouseIn(bool isMouseIn)
		{
			if (_commandBaseRendererRibbon != null && _commandBaseRendererRibbon._designer != null && _commandBaseRendererRibbon.GetPopupMenu() != null)
			{
				isMouseIn = false;
			}
			if (_isDesignerMouseIn != isMouseIn)
			{
				_isDesignerMouseIn = isMouseIn;
				_commandBaseRendererRibbon.InvalidateTabContent();
			}
		}

		public bool IsPopupOpen()
		{
			return _isPopupOpen;
		}

		public void SetPopupOpen(bool isPopupOpen)
		{
			if (_isPopupOpen != isPopupOpen)
			{
				_isPopupOpen = isPopupOpen;
				Hot = false;
				Invalidate();
			}
		}

		public void SetDrawsBackground(bool drawsBackground)
		{
			_drawsBackground = drawsBackground;
		}

		private void ApplyColumnHeightLimit()
		{
			int columnHeightLimit = GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows;
			foreach (RibbonCommandColumn item in _commandColumns)
			{
				item.SpreadLastGroupVertically(columnHeightLimit, GetRibbonGroup().ItemsSpace);
			}
		}

		private void OnControlDisposed(object sender, EventArgs eventArgs)
		{
			SetPopupOpen(false);
		}

		private void StartFadeAnimation()
		{
			if (_fadeTimer == null)
			{
				if (Hot)
				{
					_fadeAlpha = HotStartAlpha;
				}
				else
				{
					_fadeAlpha = ColdStartAlpha;
				}
				_fadeTimer = new Timer();
				_fadeTimer.Interval = FadeStartIntervalMilliseconds;
				_fadeTimer.Tick += OnTimerTick;
				_fadeTimer.Start();
			}
		}

		public override void StopAnimation()
		{
			StopFadeTimer();
			if (_dialogLauncher != null)
			{
				_dialogLauncher.StopAnimation();
			}
			base.StopAnimation();
		}

		private void StopFadeTimer()
		{
			if (_fadeTimer != null)
			{
				_fadeTimer.Stop();
				_fadeTimer.Tick -= OnTimerTick;
				_fadeTimer.Dispose();
				_fadeTimer = null;
			}
			_fadeAlpha = OpaqueAlpha;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_fadeTimer == null)
			{
				return;
			}
			if (Hot)
			{
				_fadeAlpha += FadeStepAlpha;
				if (_fadeAlpha >= OpaqueAlpha)
				{
					_fadeAlpha = OpaqueAlpha;
					StopFadeTimer();
				}
				else
				{
					_fadeTimer.Interval = FadeIntervalMilliseconds;
				}
			}
			else
			{
				_fadeAlpha -= FadeStepAlpha;
				if (_fadeAlpha < FadeEndAlpha)
				{
					_fadeAlpha = OpaqueAlpha;
					StopFadeTimer();
				}
				else
				{
					_fadeTimer.Interval = FadeIntervalMilliseconds;
				}
			}
			if (GetRenderHost() != null && GetRenderHost().ParentControl != null)
			{
				Rectangle rc = _bounds;
				rc.Inflate(2, 2);
				GetRenderHost().ParentControl.Invalidate(rc);
				GetRenderHost().ParentControl.Update();
			}
		}
	}
}
