using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		private const int GroupAnimationDurationMs = 200;

		private GroupToggleAnimation _groupAnimation;

		/// <summary>Finishes a running expand or collapse animation at once so the control state is final.</summary>
		internal void CompleteGroupAnimation()
		{
			_groupAnimation?.Complete();
		}

		private void CancelGroupAnimation()
		{
			_groupAnimation?.Cancel();
		}

		internal bool IsGroupAnimationRunning => _groupAnimation != null;

		private void AnimateGroupToggle(TaskPaneGroup group)
		{
			CompleteGroupAnimation();
			GroupToggleAnimation animation = new GroupToggleAnimation(this, group);
			_groupAnimation = animation;
			animation.Start();
		}

		/// <summary>
		/// Slides the items of one group in or out. The group already has its final Expanded value.
		/// The bitmaps are drawn once, then a timer moves them frame by frame.
		/// </summary>
		private sealed class GroupToggleAnimation : IDisposable
		{
			private readonly MxTaskPane _owner;

			private readonly TaskPaneGroup _group;

			private readonly bool _collapsing;

			private readonly Rectangle _previousBounds;

			private Graphics _controlGraphics;

			private Bitmap _targetBitmap;

			private Bitmap _frameBitmap;

			private Graphics _targetGraphics;

			private Graphics _frameGraphics;

			private TaskPaneFormatStyle _groupStyle;

			private TaskPaneFormatStyle _buttonStyle;

			private StepAnimator _animator;

			private Rectangle _destRect;

			private int _headerBottom;

			private int _itemsHeight;

			private bool _scrollBarVisibleAfter;

			private bool _scrollBarAppeared;

			private bool _disposed;

			private Rectangle ContentBounds => _owner._contentBounds;

			private void ShrinkContentBounds()
			{
				Rectangle bounds = _owner._contentBounds;
				bounds.Height -= _itemsHeight;
				_owner._contentBounds = bounds;
			}

			internal GroupToggleAnimation(MxTaskPane owner, TaskPaneGroup group)
			{
				_owner = owner;
				_group = group;
				_collapsing = !group.Expanded;
				_previousBounds = owner._contentBounds;
			}

			internal void Start()
			{
				bool canAnimate;
				try
				{
					canAnimate = Prepare();
				}
				catch (Exception ex) when (ex is ArgumentException || ex is OutOfMemoryException || ex is ExternalException)
				{
					// GDI+ could not allocate the frame bitmaps; the group still changes state, just without motion.
					canAnimate = false;
				}
				catch
				{
					Abort();
					throw;
				}
				if (!canAnimate)
				{
					FinishState();
					return;
				}
				_animator = new StepAnimator(GroupAnimationDurationMs, DrawStep, FinishState);
				_animator.Start();
			}

			internal void Complete()
			{
				if (_animator != null)
				{
					_animator.Complete();
				}
			}

			// Leaves the owner usable after an unexpected failure: no running animation and a fresh layout.
			private void Abort()
			{
				ReleaseOwnerReference();
				_owner._isAnimatingGroup = false;
				_owner._contentBounds = _previousBounds;
				Dispose();
				if (!_owner.IsDisposed)
				{
					_owner.InvalidateLayout();
				}
			}

			internal void Cancel()
			{
				_animator?.Cancel();
				_owner._isAnimatingGroup = false;
				ReleaseOwnerReference();
				Dispose();
			}

			public void Dispose()
			{
				if (_disposed)
				{
					return;
				}
				_disposed = true;
				_animator?.Dispose();
				_frameGraphics?.Dispose();
				_targetGraphics?.Dispose();
				_frameBitmap?.Dispose();
				_targetBitmap?.Dispose();
				_controlGraphics?.Dispose();
				_buttonStyle?.DisposeDrawingObjects();
				_groupStyle?.DisposeDrawingObjects();
			}

			private bool Prepare()
			{
				MxTaskPane owner = _owner;
				owner._isAnimatingGroup = true;
				owner._contentBounds = owner.ClientRectangle;
				_controlGraphics = owner.CreateGraphics();
				if (_group.SpecialGroup)
				{
					_groupStyle = new TaskPaneFormatStyle(owner._specialGroupsFormatStyle);
					_buttonStyle = _groupStyle.MergeWith(owner._hoverSpecialGroupFormatStyle);
				}
				else
				{
					_groupStyle = new TaskPaneFormatStyle(owner._groupsFormatStyle);
					_buttonStyle = _groupStyle.MergeWith(owner._hoverGroupFormatStyle);
				}
				return _collapsing ? PrepareCollapse() : PrepareExpand();
			}

			private bool PrepareCollapse()
			{
				MxTaskPane owner = _owner;
				// The group is laid out expanded while its content is captured.
				_group._expanded = true;
				try
				{
					owner.DrawExpandButton(_controlGraphics, _group, true, _buttonStyle, true);
					if (_group.ShowCloseButton)
					{
						owner.DrawGroupCloseButton(_controlGraphics, _group, _buttonStyle);
					}
					if (ContentBounds.Width <= 0 || ContentBounds.Height <= 0)
					{
						return false;
					}
					Rectangle itemsBounds = _group.GetItemsBounds();
					Rectangle headerBounds = _group.GetHeaderBounds();
					_headerBottom = headerBounds.Bottom;
					_itemsHeight = itemsBounds.Height;
					int bitmapHeight = ContentBounds.Height + _itemsHeight;
					CreateFrameSurfaces(ContentBounds.Width, bitmapHeight);
					Rectangle collapseBounds = new Rectangle(0, 0, ContentBounds.Width, ContentBounds.Bottom + _itemsHeight);
					RenderContent(collapseBounds);
					SetClip(itemsBounds, headerBounds);
					_destRect = new Rectangle(owner.ClientRectangle.Left, _headerBottom, owner.ClientRectangle.Width, _targetBitmap.Height);
					return true;
				}
				finally
				{
					_group._expanded = false;
				}
			}

			private bool PrepareExpand()
			{
				MxTaskPane owner = _owner;
				bool scrollBarVisibleBefore = owner._verticalScrollbar.Visible;
				owner.LayoutContent(_controlGraphics);
				_scrollBarVisibleAfter = owner._verticalScrollbar.Visible;
				if (_scrollBarVisibleAfter != scrollBarVisibleBefore && _scrollBarVisibleAfter)
				{
					owner._verticalScrollbar.Visible = false;
					owner._isLayoutValid = true;
					_scrollBarAppeared = true;
				}
				owner.DrawExpandButton(_controlGraphics, _group, true, _buttonStyle, false);
				if (_group.ShowCloseButton)
				{
					owner.DrawGroupCloseButton(_controlGraphics, _group, _buttonStyle);
				}
				if (ContentBounds.Width <= 0 || ContentBounds.Height <= 0)
				{
					return false;
				}
				Rectangle itemsBounds = _group.GetItemsBounds();
				Rectangle headerBounds = _group.GetHeaderBounds();
				_headerBottom = headerBounds.Bottom;
				_itemsHeight = itemsBounds.Height;
				int bitmapHeight = ContentBounds.Height + itemsBounds.Bottom;
				CreateFrameSurfaces(ContentBounds.Width, bitmapHeight);
				Rectangle expandBounds = new Rectangle(0, 0, owner.ClientRectangle.Width, bitmapHeight);
				RenderContent(expandBounds);
				SetClip(headerBounds, headerBounds);
				_destRect = new Rectangle(owner.ClientRectangle.Left, _headerBottom - _itemsHeight, owner.ClientRectangle.Width, _targetBitmap.Height);
				return true;
			}

			private void CreateFrameSurfaces(int width, int height)
			{
				_targetBitmap = new Bitmap(width, height, _controlGraphics);
				_frameBitmap = new Bitmap(width, height, _controlGraphics);
				_targetGraphics = Graphics.FromImage(_targetBitmap);
				_frameGraphics = Graphics.FromImage(_frameBitmap);
			}

			private void RenderContent(Rectangle bounds)
			{
				MxTaskPane owner = _owner;
				_targetGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				_targetGraphics.FillRectangle(Brushes.Magenta, bounds);
				owner.DrawControlBackground(_targetGraphics, bounds);
				owner.PaintContent(_targetGraphics, bounds);
				owner.PaintContent(_frameGraphics, bounds);
				_targetBitmap.MakeTransparent(Color.Magenta);
			}

			private void SetClip(Rectangle columnBounds, Rectangle headerBounds)
			{
				MxTaskPane owner = _owner;
				Rectangle clip;
				if (owner.Columns <= 1)
				{
					clip = new Rectangle(ContentBounds.X, _headerBottom, ContentBounds.Width, ContentBounds.Height - _headerBottom);
				}
				else
				{
					clip = new Rectangle(columnBounds.X, _headerBottom, columnBounds.Width, ContentBounds.Height - _headerBottom);
				}
				using (Region region = new Region(clip))
				{
					_controlGraphics.Clip = region;
				}
			}

			private void DrawStep(double progress)
			{
				if (_owner.IsDisposed || !_owner.IsHandleCreated)
				{
					Cancel();
					return;
				}
				int offset = (int)Math.Round(_itemsHeight * progress);
				if (_collapsing)
				{
					_destRect.Y = offset >= _itemsHeight ? _headerBottom - _itemsHeight + 1 : _headerBottom - offset;
				}
				else
				{
					_destRect.Y = offset >= _itemsHeight ? _headerBottom : _headerBottom - _itemsHeight + offset;
				}
				_owner.DrawControlBackground(_frameGraphics, _previousBounds);
				_frameGraphics.DrawImage(_targetBitmap, _destRect, _destRect.Left, _headerBottom, _destRect.Width, _destRect.Height, GraphicsUnit.Pixel);
				_controlGraphics.DrawImageUnscaled(_frameBitmap, 0, 0);
			}

			private void ReleaseOwnerReference()
			{
				if (_owner._groupAnimation == this)
				{
					_owner._groupAnimation = null;
				}
			}

			/// <summary>Applies the end state of the toggle, then frees the frame resources.</summary>
			private void FinishState()
			{
				MxTaskPane owner = _owner;
				ReleaseOwnerReference();
				try
				{
					if (owner.IsDisposed)
					{
						return;
					}
					owner._isAnimatingGroup = false;
					if (_collapsing)
					{
						_group._expanded = false;
						if (_itemsHeight > 0)
						{
							ShrinkContentBounds();
						}
						if (_controlGraphics != null)
						{
							owner.LayoutContent(_controlGraphics);
						}
						else
						{
							owner._isLayoutValid = false;
						}
						owner.Invalidate(invalidateChildren: true);
					}
					else
					{
						if (_controlGraphics != null)
						{
							owner._verticalScrollbar.Visible = _scrollBarVisibleAfter;
						}
						if (_itemsHeight > 0)
						{
							ShrinkContentBounds();
						}
						if (_scrollBarAppeared || _controlGraphics == null)
						{
							owner.InvalidateLayout();
						}
						else
						{
							owner.Invalidate(invalidateChildren: true);
						}
					}
					owner.Update();
				}
				finally
				{
					Dispose();
				}
			}
		}
	}
}
