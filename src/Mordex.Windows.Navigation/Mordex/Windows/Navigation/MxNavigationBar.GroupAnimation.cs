using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		private const int SlideAnimationDurationMs = 200;

		private SlideAnimation _slideAnimation;

		/// <summary>Finishes a running group switch animation at once so the control state is final.</summary>
		internal void CompleteSlideAnimation()
		{
			_slideAnimation?.Complete();
		}

		private void CancelSlideAnimation()
		{
			_slideAnimation?.Cancel();
		}

		internal bool IsSlideAnimationRunning => _slideAnimation != null;

		private void AnimateGroupSwitch(NavigationGroup selectedGroup)
		{
			CompleteSlideAnimation();
			_dragItemIndex = -1;
			if (!selectedGroup.Enabled)
			{
				return;
			}
			_isSlideAnimating = true;
			if (_timer != null)
			{
				_timer.Stop();
			}
			NavigationGroup previousGroup = _selectedGroup;
			SelectedGroup = selectedGroup;
			_isGroupLayoutValid = true;
			_isItemLayoutValid = true;
			Rectangle animationArea = Rectangle.Empty;
			if (!_groupSelectionAccepted)
			{
				_isSlideAnimating = false;
				if (_dragGroupIndex != -1)
				{
					UpdateGroupDragHighlight(null, null);
					_dragGroupIndex = -1;
				}
				return;
			}
			_scrollNextButton.Visible = false;
			_scrollPreviousButton.Visible = false;
			bool isHorizontal = _orientation == NavigationBarOrientation.Horizontal;
			_selectedGroup = previousGroup;
			Graphics controlGraphics = CreateGraphics();
			Bitmap contentBitmap = null;
			Bitmap targetBitmap = null;
			bool started = false;
			try
			{
				if (_scrollLocation == ScrollLocation.GroupsLevel)
				{
					Rectangle bounds = _selectedGroup.Bounds;
					Rectangle nextGroupBounds = Rectangle.Empty;
					if (_selectedGroup._index < _groups.Count - 1)
					{
						nextGroupBounds = _groups[_selectedGroup._index + 1].Bounds;
					}
					if (!isHorizontal)
					{
						bounds.Width = _itemsAreaBounds.Width;
						nextGroupBounds.Width = _itemsAreaBounds.Width;
						_slideBounds = new Rectangle(0, 0, _innerBounds.Width + _innerBounds.Left, selectedGroup.Bounds.Bottom);
						animationArea = new Rectangle(0, _itemsAreaBounds.Top, _innerBounds.Width + _innerBounds.Left, selectedGroup.Bounds.Bottom - _selectedGroup.Bounds.Bottom);
					}
					else
					{
						bounds.Height = _itemsAreaBounds.Height;
						nextGroupBounds.Height = _itemsAreaBounds.Height;
						bounds.Y = _itemsAreaBounds.Top;
						nextGroupBounds.Y = _itemsAreaBounds.Top;
						_slideBounds = new Rectangle(0, 0, selectedGroup.Bounds.Right, _innerBounds.Height + _innerBounds.Top);
						animationArea = new Rectangle(_itemsAreaBounds.Left + 1, 0, selectedGroup.Bounds.Right - _selectedGroup.Bounds.Right, _innerBounds.Height + _innerBounds.Top);
					}
					_selectedGroup.SetBounds(bounds);
					if (_selectedGroup._index < _groups.Count - 1)
					{
						_groups[_selectedGroup._index + 1].SetBounds(nextGroupBounds);
					}
				}
				else if (!isHorizontal)
				{
					_slideBounds = new Rectangle(0, 0, _innerBounds.Width + _innerBounds.Left, selectedGroup.Bounds.Bottom);
					animationArea = new Rectangle(0, _itemsAreaBounds.Top, _innerBounds.Width + _innerBounds.Left, selectedGroup.Bounds.Bottom - _selectedGroup.Bounds.Bottom);
				}
				else
				{
					_slideBounds = new Rectangle(0, 0, selectedGroup.Bounds.Right, _innerBounds.Height + _innerBounds.Top);
					animationArea = new Rectangle(_itemsAreaBounds.Left + 1, 0, selectedGroup.Bounds.Right - _selectedGroup.Bounds.Right, _innerBounds.Height + _innerBounds.Top);
				}
				bool movesTowardsStart = selectedGroup._index < previousGroup._index;
				if (movesTowardsStart)
				{
					if (!isHorizontal)
					{
						_slideBounds = new Rectangle(0, 0, base.ClientRectangle.Width, _itemsAreaBounds.Bottom - selectedGroup.Bounds.Bottom);
						animationArea = new Rectangle(_innerBounds.Left, selectedGroup.Bounds.Bottom, _innerBounds.Width + _innerBounds.Left, _itemsAreaBounds.Bottom - selectedGroup.Bounds.Bottom);
					}
					else
					{
						_slideBounds = new Rectangle(selectedGroup.Bounds.Right, _innerBounds.Top, _itemsAreaBounds.Right - selectedGroup.Bounds.Right, _innerBounds.Height + _innerBounds.Top);
						animationArea = new Rectangle(selectedGroup.Bounds.Right, _innerBounds.Top, _itemsAreaBounds.Right - selectedGroup.Bounds.Right, _innerBounds.Height + _innerBounds.Top);
					}
				}
				if (_slideBounds.Width <= 0 || _slideBounds.Height <= 0)
				{
					return;
				}
				contentBitmap = new Bitmap(_slideBounds.Width, _slideBounds.Height, controlGraphics);
				using (Graphics contentGraphics = Graphics.FromImage(contentBitmap))
				{
					DrawControlBackground(contentGraphics, animationArea, true);
					DisposeRenderers();
					_selectedGroup = selectedGroup;
					LayoutGroups(contentGraphics);
					try
					{
						LayoutVerticalItems(contentGraphics, selectedGroup);
					}
					catch (Exception ex) when (ex is ArgumentException || ex is ExternalException || ex is InvalidOperationException || ex is OutOfMemoryException)
					{
						// The new group could not be drawn off screen; fall back to a plain relayout without motion.
						Debug.WriteLine("MxNavigationBar: group switch animation skipped: " + ex.Message);
						InvalidateLayout(true);
						return;
					}
				}
				targetBitmap = new Bitmap(_slideBounds.Width, _slideBounds.Height, controlGraphics);
				using (Graphics targetGraphics = Graphics.FromImage(targetBitmap))
				{
					DrawControlBackground(targetGraphics, movesTowardsStart ? animationArea : _innerBounds, true);
				}
				using (Region clip = new Region(animationArea))
				{
					controlGraphics.Clip = clip;
				}
				Action<int, bool> drawFrame;
				int travel;
				if (movesTowardsStart)
				{
					travel = CreateRevealFrame(selectedGroup, isHorizontal, contentBitmap, targetBitmap, controlGraphics, out drawFrame);
				}
				else
				{
					travel = CreateSlideFrame(isHorizontal, contentBitmap, targetBitmap, controlGraphics, out drawFrame);
				}
				SlideAnimation animation = new SlideAnimation(this, controlGraphics, contentBitmap, targetBitmap, Math.Max(1, travel), drawFrame);
				_slideAnimation = animation;
				started = true;
				animation.Start();
			}
			finally
			{
				if (!started)
				{
					// The new group was accepted, so it stays selected even when the animation could not run.
					_selectedGroup = selectedGroup;
					InvalidateLayout(true);
					targetBitmap?.Dispose();
					contentBitmap?.Dispose();
					controlGraphics.Dispose();
					_scrollNextButton.Visible = true;
					_scrollPreviousButton.Visible = true;
					DisposeRenderers();
					_isSlideAnimating = false;
				}
			}
		}

		/// <summary>The new group opens below the old one: its content is revealed from the lower group edge.</summary>
		private int CreateRevealFrame(NavigationGroup selectedGroup, bool isHorizontal, Bitmap contentBitmap, Bitmap targetBitmap, Graphics graphics, out Action<int, bool> drawFrame)
		{
			Rectangle slideBounds = _slideBounds;
			Rectangle itemsArea = _itemsAreaBounds;
			Rectangle groupBounds = selectedGroup.Bounds;
			if (!isHorizontal)
			{
				int startY = groupBounds.Bottom;
				int endY = itemsArea.Bottom - groupBounds.Height;
				drawFrame = (offset, isLast) =>
				{
					Rectangle slide = slideBounds;
					slide.Y = isLast ? endY : startY + offset;
					Rectangle reveal = new Rectangle(slide.Left, groupBounds.Bottom, slide.Width, offset);
					graphics.DrawImage(contentBitmap, slide, slide.Left, reveal.Top, slide.Width, slide.Height, GraphicsUnit.Pixel);
					graphics.DrawImage(targetBitmap, reveal, reveal.Left, reveal.Top, reveal.Width, reveal.Height, GraphicsUnit.Pixel);
				};
				return itemsArea.Height - startY;
			}
			int startX = groupBounds.Right;
			int endX = itemsArea.Right - groupBounds.Width;
			drawFrame = (offset, isLast) =>
			{
				Rectangle slide = slideBounds;
				slide.X = isLast ? endX : startX + offset;
				Rectangle reveal = new Rectangle(groupBounds.Right, slide.Top, offset, slide.Height);
				graphics.DrawImage(contentBitmap, slide, reveal.Left, slide.Top, slide.Width, slide.Height, GraphicsUnit.Pixel);
				graphics.DrawImage(targetBitmap, reveal, reveal.Left, reveal.Top, reveal.Width, reveal.Height, GraphicsUnit.Pixel);
			};
			return itemsArea.Width - startX;
		}

		/// <summary>The new group opens above the old one: the old content moves out while the new one slides in.</summary>
		private int CreateSlideFrame(bool isHorizontal, Bitmap contentBitmap, Bitmap targetBitmap, Graphics graphics, out Action<int, bool> drawFrame)
		{
			Rectangle itemsArea = _itemsAreaBounds;
			int groupBottom = _selectedGroup.Bounds.Bottom;
			int groupRight = _selectedGroup.Bounds.Right;
			int startY = itemsArea.Bottom;
			int startX = itemsArea.Right;
			if (_isSelectedGroupLast)
			{
				startY = _innerBounds.Bottom;
				startX = _innerBounds.Right;
			}
			Rectangle baseBounds = new Rectangle(0, 0, _slideBounds.Width, _slideBounds.Height);
			if (!isHorizontal)
			{
				drawFrame = (offset, isLast) =>
				{
					Rectangle old = baseBounds;
					old.Y = -offset;
					Rectangle incoming = new Rectangle(baseBounds.Left, isLast ? itemsArea.Top : startY - offset, baseBounds.Width, baseBounds.Height);
					graphics.DrawImage(contentBitmap, old);
					graphics.DrawImage(targetBitmap, incoming, incoming.Left, groupBottom, incoming.Width, incoming.Height, GraphicsUnit.Pixel);
				};
				return startY - itemsArea.Top;
			}
			drawFrame = (offset, isLast) =>
			{
				Rectangle old = baseBounds;
				old.X = -offset;
				Rectangle incoming = new Rectangle(isLast ? itemsArea.Left : startX - offset, baseBounds.Top, baseBounds.Width, baseBounds.Height);
				graphics.DrawImage(contentBitmap, old);
				graphics.DrawImage(targetBitmap, incoming, groupRight, incoming.Top, incoming.Width, incoming.Height, GraphicsUnit.Pixel);
			};
			return startX - itemsArea.Left;
		}

		/// <summary>Moves the two group bitmaps over the control while a timer drives the frames.</summary>
		private sealed class SlideAnimation : IDisposable
		{
			private readonly MxNavigationBar _owner;

			private readonly Graphics _graphics;

			private readonly Bitmap _contentBitmap;

			private readonly Bitmap _targetBitmap;

			private readonly int _travel;

			private readonly Action<int, bool> _drawFrame;

			private StepAnimator _animator;

			private bool _disposed;

			internal SlideAnimation(MxNavigationBar owner, Graphics graphics, Bitmap contentBitmap, Bitmap targetBitmap, int travel, Action<int, bool> drawFrame)
			{
				_owner = owner;
				_graphics = graphics;
				_contentBitmap = contentBitmap;
				_targetBitmap = targetBitmap;
				_travel = travel;
				_drawFrame = drawFrame;
			}

			internal void Start()
			{
				_animator = new StepAnimator(SlideAnimationDurationMs, DrawStep, Finish);
				_animator.Start();
			}

			internal void Complete()
			{
				_animator?.Complete();
			}

			internal void Cancel()
			{
				_animator?.Cancel();
				if (_owner._slideAnimation == this)
				{
					_owner._slideAnimation = null;
				}
				_owner._isSlideAnimating = false;
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
				_targetBitmap.Dispose();
				_contentBitmap.Dispose();
				_graphics.Dispose();
			}

			private void DrawStep(double progress)
			{
				if (_owner.IsDisposed || !_owner.IsHandleCreated)
				{
					Cancel();
					return;
				}
				bool isLast = progress >= 1.0;
				_drawFrame((int)Math.Round(_travel * progress), isLast);
			}

			private void Finish()
			{
				if (_owner._slideAnimation == this)
				{
					_owner._slideAnimation = null;
				}
				try
				{
					if (_owner.IsDisposed)
					{
						return;
					}
					_owner._scrollNextButton.Visible = true;
					_owner._scrollPreviousButton.Visible = true;
					_owner.DisposeRenderers();
					_owner._isSlideAnimating = false;
					_owner.Invalidate();
					_owner.Update();
				}
				finally
				{
					Dispose();
				}
			}
		}
	}
}
