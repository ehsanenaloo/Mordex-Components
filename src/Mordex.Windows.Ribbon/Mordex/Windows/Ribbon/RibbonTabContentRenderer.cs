using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabContentRenderer : IDisposable
	{
		private MxRibbon _ribbon;

		public IList<RibbonGroupBoxRenderer> _groupRenderers;

		private bool _areGroupsStale;

		private RibbonGroupBoxRenderer _hotGroup;

		private RibbonLayoutItem _hotScrollButton;

		public Rectangle _bounds;

		private bool _isMousePressed;

		internal RibbonLayoutItem _leftScrollButton;

		internal RibbonLayoutItem _rightScrollButton;

		private int _scrollOffset;

		private int _totalGroupsWidth;

		public Rectangle _groupsBounds;

		private Timer _scrollTimer;

		private bool _isScrollingLeft;

		private bool _isScrollTimerRunning;

		private const int ScrollStartIntervalMilliseconds = 50;

		private const int ScrollIntervalStepMilliseconds = 20;

		private const int ScrollMinimumIntervalMilliseconds = 10;

		private int _scrollIntervalMs = 30;

		private ICommandRenderHost _renderHost;

		private AccessibleObject _accessibleObject;

		public RibbonTabContentRenderer(MxRibbon ribbon, ICommandRenderHost itemOwner)
		{
			_ribbon = ribbon;
			_renderHost = itemOwner;
			_areGroupsStale = true;
		}

		public Control GetParentControl()
		{
			return _renderHost.ParentControl;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new RibbonTabContentAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal int GetScrollOffset()
		{
			return _scrollOffset;
		}

		internal void SetScrollOffset(int offset)
		{
			if (offset > 0)
			{
				offset = 0;
			}
			if (_scrollOffset != offset)
			{
				_scrollOffset = offset;
				if (_ribbon.SelectedTab != null)
				{
					_ribbon.SelectedTab._scrollOffset = _scrollOffset;
				}
				RelayoutGroups();
				_ribbon.Invalidate();
			}
		}

		internal void SetHotScrollButton(RibbonLayoutItem hotButton)
		{
			if (_hotScrollButton != hotButton)
			{
				if (_hotScrollButton != null)
				{
					_hotScrollButton.Hot = false;
					InvalidateRibbonArea(_hotScrollButton._bounds);
				}
				_hotScrollButton = hotButton;
				if (_hotScrollButton != null)
				{
					SetHotGroup(null);
					_hotScrollButton.Hot = true;
					InvalidateRibbonArea(_hotScrollButton._bounds);
				}
			}
		}

		internal RibbonGroupBoxRenderer GetHotGroup()
		{
			return _hotGroup;
		}

		internal void SetHotGroup(RibbonGroupBoxRenderer ribbonRenderer)
		{
			if (_hotGroup != ribbonRenderer)
			{
				if (_hotGroup != null)
				{
					_hotGroup.Hot = false;
					_hotGroup.SetHotRenderer(null);
				}
				_hotGroup = ribbonRenderer;
				if (_hotGroup != null && _hotGroup.GetRibbonGroup() != null && _hotGroup.GetRibbonGroup().Enabled)
				{
					_hotGroup.Hot = true;
					SetHotScrollButton(null);
				}
				if (_ribbon != null)
				{
					_ribbon.ShowSuperTip(_hotGroup);
				}
			}
		}

		internal void InvalidateRibbonArea(Rectangle rc)
		{
			if (_ribbon != null)
			{
				_ribbon.Invalidate(rc);
			}
		}

		internal void MarkGroupsStale()
		{
			_areGroupsStale = true;
		}

		internal void ScrollGalleryItemIntoView(Gallery gallery, GalleryItem galleryItem)
		{
			if (_groupRenderers == null)
			{
				return;
			}
			GalleryRenderer galleryRenderer = null;
			foreach (RibbonGroupBoxRenderer item in _groupRenderers)
			{
				foreach (CommandBaseRenderer commandRenderer in item._childRenderers)
				{
					if (commandRenderer.CommandType == CommandType.Gallery && commandRenderer.GetCommand() == gallery)
					{
						galleryRenderer = commandRenderer as GalleryRenderer;
						break;
					}
				}
			}
			galleryRenderer?.ScrollItemIntoView(galleryItem);
		}

		internal CommandBase FindCommandAt(int pointX, int pointY)
		{
			if (_bounds.Contains(pointX, pointY))
			{
				if (_leftScrollButton != null && _leftScrollButton._bounds.Contains(pointX, pointY))
				{
					return null;
				}
				if (_rightScrollButton != null && _rightScrollButton._bounds.Contains(pointX, pointY))
				{
					return null;
				}
				RibbonGroupBoxRenderer ribbonRenderer = FindGroupRendererAt(pointX, pointY);
				if (ribbonRenderer != null)
				{
					CommandBase commandAt = ((IKeyboardNavigable)ribbonRenderer).GetCommandAt(pointX, pointY);
					if (commandAt != null)
					{
						return commandAt;
					}
					if (ribbonRenderer.FindRendererAt(pointX, pointY) != null)
					{
						return null;
					}
					return ribbonRenderer.GetRibbonGroup();
				}
			}
			return null;
		}

		internal CommandBaseRenderer FindRendererForCommand(CommandBase command)
		{
			if (_groupRenderers != null)
			{
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					if (item.GetCommand() == command)
					{
						return item;
					}
					CommandBaseRenderer commandLayoutItem = ((IKeyboardNavigable)item).GetCommandLayoutItem(command);
					if (commandLayoutItem != null)
					{
						return commandLayoutItem;
					}
				}
			}
			return null;
		}

		private RibbonGroupBoxRenderer FindOwnerGroupRenderer(CommandBaseRenderer commandBaseRenderer)
		{
			for (CommandBaseRenderer layoutItemOwner = commandBaseRenderer._layoutItemOwner; layoutItemOwner != null; layoutItemOwner = layoutItemOwner._layoutItemOwner)
			{
				if (layoutItemOwner is RibbonGroupBoxRenderer)
				{
					return layoutItemOwner as RibbonGroupBoxRenderer;
				}
			}
			return null;
		}

		public void MoveFocusLeft(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return;
			}
			if (selectedItem is RibbonGroupDialogLauncherRenderer)
			{
				RibbonGroupBoxRenderer item = (RibbonGroupBoxRenderer)selectedItem._layoutItemOwner;
				int launcherGroupIndex = _groupRenderers.IndexOf(item);
				for (int leftGroupIndex = launcherGroupIndex - 1; leftGroupIndex >= 0; leftGroupIndex--)
				{
					RibbonGroupBoxRenderer thisValue = _groupRenderers[leftGroupIndex];
					if (thisValue._dialogLauncher != null)
					{
						_ribbon.SetKeyboardFocusRenderer(thisValue._dialogLauncher);
						return;
					}
				}
				for (int lastGroupIndex = _groupRenderers.Count - 1; lastGroupIndex >= 0; lastGroupIndex--)
				{
					RibbonGroupBoxRenderer lastFocusableGroup = _groupRenderers[lastGroupIndex];
					if (lastFocusableGroup._dialogLauncher != null)
					{
						_ribbon.SetKeyboardFocusRenderer(lastFocusableGroup._dialogLauncher);
						break;
					}
				}
			}
			else
			{
				if (!(selectedItem._layoutItemOwner is IKeyboardNavigable) || ((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveLeft(selectedItem))
				{
					return;
				}
				RibbonGroupBoxRenderer ownerGroup = FindOwnerGroupRenderer(selectedItem);
				int ownerGroupIndex = _groupRenderers.IndexOf(ownerGroup);
				for (int previousGroupIndex = ownerGroupIndex - 1; previousGroupIndex >= 0; previousGroupIndex--)
				{
					RibbonGroupBoxRenderer previousGroup = _groupRenderers[previousGroupIndex];
					if (((IKeyboardNavigable)previousGroup).MoveLeft((CommandBaseRenderer)null))
					{
						return;
					}
				}
				for (int wrapGroupIndex = _groupRenderers.Count - 1; wrapGroupIndex >= 0; wrapGroupIndex--)
				{
					RibbonGroupBoxRenderer wrapGroup = _groupRenderers[wrapGroupIndex];
					if (((IKeyboardNavigable)wrapGroup).MoveLeft((CommandBaseRenderer)null))
					{
						break;
					}
				}
			}
		}

		private void RelayoutGroups()
		{
			using Graphics graphics = _ribbon.CreateGraphics();
			using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_ribbon.UseCompatibleTextRendering);
			LayoutGroupsAtOffset(graphics, textLayoutRenderer, GetScrollOffset());
		}

		private void StartScrollLeft()
		{
			_scrollIntervalMs = ScrollStartIntervalMilliseconds;
			_isScrollingLeft = true;
			_isScrollTimerRunning = true;
			_leftScrollButton = null;
			if (_scrollTimer == null)
			{
				_scrollTimer = new Timer();
				_scrollTimer.Interval = _scrollIntervalMs;
				_scrollTimer.Tick += OnTimerTick;
			}
			_scrollTimer.Start();
		}

		private void StartScrollRight()
		{
			_scrollIntervalMs = ScrollStartIntervalMilliseconds;
			_isScrollingLeft = false;
			_isScrollTimerRunning = true;
			_rightScrollButton = null;
			if (_scrollTimer == null)
			{
				_scrollTimer = new Timer();
				_scrollTimer.Interval = _scrollIntervalMs;
				_scrollTimer.Tick += OnTimerTick;
			}
			_scrollTimer.Start();
		}

		public void ClearHotItems()
		{
			if (GetHotGroup() != null)
			{
				GetHotGroup().SetHotRenderer(null);
				if (!GetHotGroup()._bounds.Contains(_ribbon.PointToClient(Cursor.Position)))
				{
					SetHotGroup(null);
				}
			}
			_ribbon.SetHotRenderer(null);
		}

		internal RibbonGroupBoxRenderer FindGroupRendererAt(int pointX, int pointY)
		{
			if (_groupRenderers != null)
			{
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					if (item._bounds.Contains(pointX, pointY))
					{
						return item;
					}
				}
			}
			return null;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_scrollTimer == null)
			{
				return;
			}
			_scrollIntervalMs -= ScrollIntervalStepMilliseconds;
			if (_scrollIntervalMs < ScrollMinimumIntervalMilliseconds)
			{
				_scrollIntervalMs = ScrollMinimumIntervalMilliseconds;
			}
			_scrollTimer.Interval = _scrollIntervalMs;
			if (_isScrollingLeft)
			{
				int leftOffset = _scrollOffset + 10;
				if (leftOffset >= 0)
				{
					StopScrollTimer();
					leftOffset = 0;
				}
				SetScrollOffset(leftOffset);
				return;
			}
			int minOffset = _groupsBounds.Width - _totalGroupsWidth;
			int rightOffset = _scrollOffset - 10;
			if (rightOffset <= minOffset)
			{
				StopScrollTimer();
				rightOffset = minOffset;
			}
			SetScrollOffset(rightOffset);
		}

		public void Dispose()
		{
			StopScrollTimer();
			StopItemAnimations();
		}

		private void StopItemAnimations()
		{
			if (_groupRenderers == null)
			{
				return;
			}
			foreach (RibbonGroupBoxRenderer item in _groupRenderers)
			{
				item.StopAnimation();
			}
		}

		private void StopScrollTimer()
		{
			_isScrollTimerRunning = false;
			if (_scrollTimer != null)
			{
				_scrollTimer.Stop();
				_scrollTimer.Tick -= OnTimerTick;
				_scrollTimer.Dispose();
				_scrollTimer = null;
			}
		}

		internal void ScrollGroupIntoView(RibbonGroupBoxRenderer ribbonRenderer)
		{
			Rectangle rectangle = _groupsBounds;
			if (_leftScrollButton != null && _leftScrollButton._isVisible)
			{
				rectangle.X += _leftScrollButton._bounds.Width;
				rectangle.Width -= _leftScrollButton._bounds.Width;
			}
			if (_rightScrollButton != null && _rightScrollButton._isVisible)
			{
				rectangle.Width -= _rightScrollButton._bounds.Width;
			}
			if (ribbonRenderer._bounds.X < rectangle.X)
			{
				SetScrollOffset(GetScrollOffset() - (ribbonRenderer._bounds.X - rectangle.X));
			}
			if (ribbonRenderer._bounds.Right > rectangle.Right)
			{
				SetScrollOffset(GetScrollOffset() - (ribbonRenderer._bounds.Right - rectangle.Right));
			}
		}
	}
}
