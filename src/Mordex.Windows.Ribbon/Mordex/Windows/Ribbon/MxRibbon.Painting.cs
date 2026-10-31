using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		internal Color GetHeaderBackColor()
		{
			if (_ribbonWindowMessageHandler != null && !IsFormActive())
			{
				return _colorScheme._inactiveCaptionGradientSecond;
			}
			return _colorScheme._headerBackColor;
		}

		internal Color GetTabContentColor()
		{
			if (SelectedTab != null && SelectedTab.GetTabOwner() is RibbonContextualTab ribbonContextualTab)
			{
				Color color = RibbonDrawingUtils.BlendColors(ribbonContextualTab.BackColor, _colorScheme._tabContentBackColor, 0.9f);
				return RibbonDrawingUtils.BlendColors(color, Color.White, 0.6f);
			}
			return _colorScheme._tabContentBackColor;
		}

		private void RenderTabContent(Graphics graphics)
		{
			_contentAreaBounds.Width = GetContentAreaWidth();
			if (_contentAreaBounds.Width > 0 && _contentAreaBounds.Height > 0)
			{
				_isTabContentDirty = false;
				if (!_minimized)
				{
					_tabContentRenderer.RenderContent(graphics, _contentAreaBounds);
				}
				else
				{
					_tabContentRenderer.RenderContent(graphics, _tabContentRenderer._bounds);
				}
				if (_contentRenderedHandler != null)
				{
					_contentRenderedHandler(this, EventArgs.Empty);
				}
			}
			else if (!Minimized)
			{
				_tabContentRenderer.RebuildGroupRenderers();
			}
		}

		internal void RenderContent(Graphics graphics, bool raiseRenderedEvent)
		{
			Rectangle rectangle = Rectangle.Empty;
			if (!IsTabStripHidden())
			{
				rectangle = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top + _captionHeight, base.ClientRectangle.Width, _tabStripRowHeight);
				if (IsGlassCaptionActive() && ShowCaptionBar)
				{
					rectangle.X += GetFrameBorderSize().Width;
				}
			}
			_isTabStripLayoutDirty = false;
			_tabStripRenderer.LayoutTabStrip(graphics, rectangle, _captionAreaWidth);
			if (SelectedTab == null)
			{
				_contentAreaBounds = new Rectangle(_contentAreaBounds.X, _contentAreaBounds.Top, 0, 0);
			}
			if (_contentRenderedHandler != null && raiseRenderedEvent)
			{
				_contentRenderedHandler(this, EventArgs.Empty);
			}
		}

		private void RenderTabContentImmediately()
		{
			using (Graphics graphics = CreateGraphics())
			{
				RenderTabContent(graphics);
			}
		}

		private void RenderRibbonImmediately()
		{
			using (Graphics graphics = CreateGraphics())
			{
				RenderContent(graphics, true);
			}
			RenderTabContentImmediately();
		}

		internal void NotifyCommandPropertyChanged(CommandBase command, string text)
		{
			if (_quickAccessCommands.Contains(command))
			{
				if (_headerRenderer != null)
				{
					_headerRenderer.RequestLayoutAndRepaint();
				}
				if ((text == "Visible" || text == "ItemType") && _quickAccessToolbarRenderer != null)
				{
					_quickAccessToolbarRenderer.MarkItemsDirty();
				}
			}
			if (GetPopupMenu() != null)
			{
				GetPopupMenu().RefreshForCommand(command, false);
			}
			if (GetBackstageMenuScrollableBase() != null && BackstageMenu.Commands.Contains(command))
			{
				GetBackstageMenuScrollableBase().ApplyLayout();
			}
			if (_helpButton == command)
			{
				RequestTabStripLayout(text == "Visible");
				RefreshWindowFrame(false);
			}
			else if ((text == "Visible" || text == "Enabled" || text == "Control" || text == "ItemType") && _tabContentRenderer != null)
			{
				_tabContentRenderer.MarkGroupsStale();
			}
			_isTabContentDirty = true;
			Invalidate();
		}

		internal void InvalidateBackstage()
		{
			_isTabStripLayoutDirty = true;
			if (_backstageMenuScrollableBase != null)
			{
				_backstageMenuScrollableBase.ApplyLayout();
			}
			Invalidate();
		}

		internal void InvalidateRibbonAndCaption()
		{
			Invalidate(invalidateChildren: true);
			if (IsGlassCaptionActive() && IsCaptionBarShown() && _ribbonWindowMessageHandler != null)
			{
				RefreshWindowFrame(true);
			}
		}

		internal void RequestTabStripLayout(bool rebuildTabs)
		{
			_isTabStripLayoutDirty = true;
			InvalidateTabContent();
			if (rebuildTabs)
			{
				if (_tabStripRenderer != null)
				{
					_tabStripRenderer.MarkTabRenderersDirty();
				}
				if (_headerRenderer != null)
				{
					_headerRenderer.InvalidateLayout();
				}
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.RequestLayoutAndRepaint();
			}
			Invalidate();
			if (IsGlassCaptionActive() && IsCaptionBarShown() && _ribbonWindowMessageHandler != null)
			{
				if (_tabStripRenderer != null)
				{
					_tabStripRenderer._stripBounds = Rectangle.Empty;
				}
				RefreshWindowFrame(true);
			}
		}

		internal void InvalidateTabContent()
		{
			_isTabContentDirty = true;
			Invalidate();
		}

		internal void InvalidateFullLayout()
		{
			_isTabStripLayoutDirty = true;
			_isTabContentDirty = true;
			if (_tabStripRenderer != null)
			{
				_tabStripRenderer.MarkTabRenderersDirty();
			}
			if (_tabContentRenderer != null)
			{
				_tabContentRenderer.MarkGroupsStale();
			}
			Invalidate(invalidateChildren: true);
		}

		internal void RefreshWindowFrame(bool update)
		{
			if (_ribbonWindowMessageHandler != null && !_isMergeInProgress)
			{
				_ribbonWindowMessageHandler.Refresh(update);
			}
		}

		internal void InvalidateCommandLayout(CommandBase command)
		{
			Invalidate(invalidateChildren: true);
			if (_quickAccessToolbarRenderer != null && QuickAccessCommands.Contains(command))
			{
				_quickAccessToolbarRenderer.Invalidate(update: false);
			}
			if (QuickAccessCommands.Contains(command))
			{
				RefreshCaptionOrb();
			}
			if (GetPopupMenu() != null)
			{
				GetPopupMenu().InvalidateWithChildMenus();
			}
		}

		internal void InvalidateRibbonAndQuickAccess()
		{
			Invalidate(invalidateChildren: true);
			RefreshCaptionOrb();
			if (_quickAccessToolbarRenderer != null)
			{
				_quickAccessToolbarRenderer.Invalidate(update: false);
			}
			if (GetPopupMenu() != null)
			{
				GetPopupMenu().InvalidateWithChildMenus();
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_ribbonWindowMessageHandler != null && !_ribbonWindowMessageHandler._isFrameRefreshed)
			{
				_ribbonWindowMessageHandler.Refresh(update: true);
				_ribbonWindowMessageHandler._isFrameRefreshed = true;
			}
			if (base.ClientRectangle.Width == 0 || base.ClientRectangle.Height == 0 || _isPainting)
			{
				base.OnPaint(e);
				if (_isPainting)
				{
					Invalidate(invalidateChildren: true);
				}
				return;
			}
			_isPainting = true;
			try
			{
				if (_form == null && (!IsInDesignMode() || _isDesignerLoaded))
				{
					Form form = RibbonDrawingUtils.FindParentForm(this);
					AttachToForm(form);
				}
				if (!_isBackgroundBuffered || _backBuffer == null)
				{
					if (_backBuffer == null)
					{
						AllocateBackgroundBuffer();
					}
					EnsureBackgroundBuffered(base.ClientRectangle);
				}
				if (_isBackgroundBuffered && _backBuffer != null)
				{
					_backBuffer.Render(e.Graphics);
				}
				base.OnPaint(e);
			}
			finally
			{
				_isPainting = false;
			}
		}

		private void DrawBackground(Graphics graphics, Rectangle clipRectangle)
		{
			if (_isTabStripLayoutDirty)
			{
				RenderContent(graphics, true);
				RenderTabContent(graphics);
				if (_backBuffer == null)
				{
					return;
				}
				if (QuickAccessPosition == QuickAccessPosition.BelowRibbon && _quickAccessToolbarRenderer != null)
				{
					_quickAccessToolbarRenderer.ArrangeToolbar(graphics, GetContentAreaBounds().X, GetContentAreaBounds().Bottom);
					while (_quickAccessToolbarRenderer._contentWidth > GetContentAreaBounds().Width && _quickAccessToolbarRenderer.HasVisibleItems())
					{
						_quickAccessToolbarRenderer.Shrink(graphics);
						if (_quickAccessToolbarRenderer._contentWidth <= 0)
						{
							break;
						}
					}
				}
			}
			else if (_isTabContentDirty)
			{
				RenderTabContent(graphics);
				if (QuickAccessPosition == QuickAccessPosition.BelowRibbon && _quickAccessToolbarRenderer != null)
				{
					_quickAccessToolbarRenderer.ArrangeToolbar(graphics, GetContentAreaBounds().X, GetContentAreaBounds().Bottom);
					while (_quickAccessToolbarRenderer._contentWidth > GetContentAreaBounds().Width && _quickAccessToolbarRenderer.HasVisibleItems())
					{
						_quickAccessToolbarRenderer.Shrink(graphics);
						if (_quickAccessToolbarRenderer._contentWidth <= 0)
						{
							break;
						}
					}
				}
			}
			if (_contentAreaBounds.Width > 0 && _contentAreaBounds.Height > 0)
			{
				Rectangle rect = new Rectangle(0, _tabStripRowHeight + _captionHeight, base.ClientRectangle.Width, base.ClientRectangle.Height - _tabStripRowHeight - _captionHeight);
				if (rect.Height > 0 && rect.Width > 0)
				{
					GdiCache.FillGradient(graphics, rect, _colorScheme._headerBackColor, _colorScheme._headerBackgroundBottom, LinearGradientMode.Vertical);
				}
			}
			else if (base.ClientRectangle.Width > 0 && base.ClientRectangle.Height > 0)
			{
				Brush fallbackBrush = GdiCache.Brush(_colorScheme._headerBackColor);
				graphics.FillRectangle(fallbackBrush, base.ClientRectangle);
			}
			if (IsCaptionBarShown() && (!IsGlassCaptionActive() || _headerRenderer.GetFormHandle() == IntPtr.Zero))
			{
				_headerRenderer.DrawHeaderBackground(graphics, true, true);
			}
			if (!_minimized)
			{
				_tabContentRenderer.DrawContent(graphics, clipRectangle);
			}
			if (QuickAccessPosition == QuickAccessPosition.BelowRibbon)
			{
				_quickAccessToolbarRenderer.OnPaint(graphics, _colorScheme, clipRectangle);
			}
			if (!_tabStripRenderer.IsInGlassCaption())
			{
				_tabStripRenderer.DrawTabStrip(graphics, clipRectangle);
			}
			DrawControlBoxButton(graphics);
			if (_minimized && _quickAccessPosition == QuickAccessPosition.AboveRibbon && !IsGlassCaptionActive())
			{
				Color color = _colorScheme._tabContentBorderStart;
				Color highlightColor = Color.FromArgb(120, SystemColors.Control);
				color = _colorScheme._dividerLine;
				highlightColor = _colorScheme._dividerLine;
				Pen pen = GdiCache.Pen(color);
				Pen highlightPen = GdiCache.Pen(highlightColor);
				graphics.DrawLine(pen, base.ClientRectangle.X, base.ClientRectangle.Bottom - 2, base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom - 2);
				graphics.DrawLine(highlightPen, base.ClientRectangle.X, base.ClientRectangle.Bottom - 1, base.ClientRectangle.Right - 1, base.ClientRectangle.Bottom - 1);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (_backBuffer != null)
			{
				_backBuffer.Dispose();
				_backBuffer = null;
			}
			if (_layoutHeight != base.Height)
			{
				RelayoutForFormState();
			}
			if (IsCaptionBarShown() && (!IsGlassCaptionActive() || !IsParentedToForm() || !HandlesTitleBar))
			{
				Rectangle rectangle = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, _captionHeight);
				_headerRenderer.SetArea(rectangle);
			}
			_headerRenderer.RequestLayoutAndRepaint();
			RequestTabStripLayout(false);
			if (_tabContentRenderer != null && !_tabContentRenderer._bounds.IsEmpty && base.IsHandleCreated)
			{
				if (_tabContentRenderer.GetScrollOffset() != 0)
				{
					_contentAreaWidth = base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right;
					InvalidateTabContent();
				}
				else
				{
					_contentAreaWidth = base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right;
					InvalidateTabContent();
				}
			}
			else
			{
				_contentAreaWidth = base.ClientRectangle.Width - GetContentBorderPadding().Left - GetContentBorderPadding().Right;
				InvalidateTabContent();
			}
			if (base.Parent == null)
			{
				return;
			}
			if (IsGlassCaptionActive() && IsParentedToForm() && GetHostForm().FormBorderStyle != FormBorderStyle.None)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Y += _captionHeight;
				clientRectangle.Height -= _captionHeight;
				if (ShowCaptionBar)
				{
					clientRectangle.Y += _tabStripRowHeight;
					clientRectangle.Height -= _tabStripRowHeight;
				}
				ReplaceRegion(new Region(clientRectangle));
				if (!Minimized)
				{
					using Region invalidateRegion = new Region(clientRectangle);
					GetHostForm().Invalidate(invalidateRegion);
				}
				else
				{
					GetHostForm().Invalidate();
				}
			}
			else
			{
				ReplaceRegion(null);
			}
		}

		internal void ApplyClientRegion()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Y += _captionHeight;
			clientRectangle.Height -= _captionHeight;
			if (ShowCaptionBar)
			{
				clientRectangle.Y += _tabStripRowHeight;
				clientRectangle.Height -= _tabStripRowHeight;
			}
			ReplaceRegion(new Region(clientRectangle));
		}

		private void ReplaceRegion(Region region)
		{
			Region oldRegion = base.Region;
			base.Region = region;
			oldRegion?.Dispose();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			RelayoutForFormState();
			InvalidateTabContent();
			RequestTabStripLayout(false);
			if (_mordexSuperTip != null)
			{
				_mordexSuperTip.Font = Font;
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (_mergeHost != null)
			{
				_mergeHost.Invalidate();
			}
			OnEnabledChangedForCaption();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			if (_ribbonWindowMessageHandler != null)
			{
				if (_form != null && _form.IsHandleCreated)
				{
					NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
				}
				_ribbonWindowMessageHandler.Dispose();
				_ribbonWindowMessageHandler = null;
			}
			RelayoutForFormState();
			_isGlassAvailable = IsGlassAvailable();
			if (_form != null)
			{
				CreateCaptionHandler(true, false);
			}
			if (_headerRenderer != null)
			{
				_headerRenderer.RequestLayoutAndRepaint();
			}
			RelayoutForFormState();
			InvalidateFullLayout();
			InvalidateWindowFrame();
			base.Width++;
			base.Width--;
			if (_form != null)
			{
				if (_form.IsHandleCreated)
				{
					NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
				}
				_form.Invalidate(invalidateChildren: true);
			}
		}

		internal void DrawControlBoxButton(Graphics graphics)
		{
			if (!ShowCaptionBar)
			{
				if (_controlBoxButtonRenderer == null || (_controlBoxButtonBounds.IsEmpty && GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton))
				{
					_controlBoxButtonRenderer = new ControlBoxButtonRenderer(this);
					if (!IsCaptionBarShown())
					{
						_controlBoxButtonBounds = new Rectangle(4, base.ClientRectangle.Y + 4, 16, 16);
					}
					else {
						_controlBoxButtonBounds = new Rectangle(4, 4 + GetFrameBorderSize().Height, 16, 16);
					}
				}
				else if (GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton)
				{
					_controlBoxButtonRenderer = null;
					_controlBoxButtonBounds = Rectangle.Empty;
				}
			}
			if (_controlBoxButtonRenderer == null || _controlBoxButtonBounds.Bottom <= 0 || GetEffectiveControlBoxMode() != ControlBoxMode.ControlBoxButton)
			{
				return;
			}
			RibbonColorScheme colorScheme = _colorScheme;
			if (_controlBoxButtonRenderer.Pushed || _controlBoxButtonRenderer.IsMenuOpen())
			{
				RibbonChromePainter.DrawOrbPressed(graphics, _controlBoxButtonBounds, colorScheme);
			}
			else if (_controlBoxButtonRenderer.Hot || _controlBoxButtonRenderer.Active || _controlBoxButtonRenderer._fadeFromHot)
			{
				if (_controlBoxButtonRenderer._pressAnimationAlpha != 255)
				{
					RibbonChromePainter.DrawOrbNormal(graphics, _controlBoxButtonBounds, colorScheme);
				}
				RibbonChromePainter.DrawOrbHot(graphics, _controlBoxButtonBounds, _controlBoxButtonRenderer._pressAnimationAlpha, colorScheme);
			}
			else
			{
				RibbonChromePainter.DrawOrbNormal(graphics, _controlBoxButtonBounds, colorScheme);
			}
			if (GetHostForm() != null)
			{
				Rectangle rectangle = _controlBoxButtonBounds;
				rectangle.Inflate(-6, -6);
				rectangle.X++;
				rectangle.Y++;
				RibbonDrawingUtils.DrawIcon(graphics, ResolveControlBoxIcon(), rectangle, true, SystemColors.Window, IsGlassCaptionActive());
			}
		}

		private void RebuildColorScheme()
		{
			_colorScheme = ThemedRibbon.CreateScheme(MxThemeManager.ResolveDefault(DeviceDpi));
			ThemeSub.Set(IsHandleCreated);
			ForeColorDark = _colorScheme._foreColorDark;
			ForeColorLight = _colorScheme._foreColorLight;
			if (_ribbonWindowMessageHandler != null)
			{
				_ribbonWindowMessageHandler.RedrawFrame();
			}
			Invalidate(invalidateChildren: true);
		}

		private void EnsureBackgroundBuffered(Rectangle rectangle)
		{
			if (!_isBackgroundBuffered && _backBuffer != null)
			{
				DrawBackground(_backBuffer.Graphics, rectangle);
				_isBackgroundBuffered = true;
			}
		}

		protected override void OnInvalidated(InvalidateEventArgs e)
		{
			_isBackgroundBuffered = false;
			base.OnInvalidated(e);
		}

		private void AllocateBackgroundBuffer()
		{
			DoubleBuffered = false;
			if (base.IsHandleCreated)
			{
				if (_backBuffer != null)
				{
					_backBuffer.Dispose();
					_backBuffer = null;
				}
				Rectangle bounds = base.Bounds;
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					if (_bufferedGraphicsContext == null)
					{
						_bufferedGraphicsContext = new BufferedGraphicsContext();
					}
					_bufferedGraphicsContext.MaximumBuffer = new Size(bounds.Width + 1, bounds.Height + 1);
					bounds.Location = new Point(0, 0);
					using (Graphics graphics = CreateGraphics())
					{
						_backBuffer = _bufferedGraphicsContext.Allocate(graphics, bounds);
					}
					_isBackgroundBuffered = false;
				}
			}
		}
	}
}
