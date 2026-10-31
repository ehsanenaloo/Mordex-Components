using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	[Serializable]
	[Designer("Mordex.Windows.TaskPane.Design.ContainerDesigner, Mordex.Windows.TaskPane.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(false)]
	public class TaskPaneContainerControl : Panel
	{
		private Container _container = new Container();

		internal MxTaskPane _owner;

		internal TaskPaneGroup _group;

		internal TaskPaneItem _item;

		internal bool _updatesLayoutOnResize = true;

		private TaskPaneFormatStyle _formatStyle;

		[Browsable(false)]
		public MxTaskPane TaskPane => _owner;

		[Browsable(false)]
		public TaskPaneGroup Group
		{
			get
			{
				if (_item != null)
				{
					return _item.Group;
				}
				return _group;
			}
		}

		[Browsable(false)]
		public TaskPaneItem Item => _item;

		public TaskPaneContainerControl()
		{
			SetContainer();
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		}

		internal TaskPaneContainerControl(MxTaskPane owner, TaskPaneGroup group)
			: this()
		{
			_owner = owner;
			_group = group;
		}

		internal TaskPaneContainerControl(MxTaskPane owner, TaskPaneItem item)
			: this()
		{
			_owner = owner;
			_item = item;
		}

		internal Color GetEffectiveBackColor()
		{
			if (TaskPane != null && BackColor == TaskPane.BackColor)
			{
				return TaskPane.GetEffectiveBackColor();
			}
			return BackColor;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (_updatesLayoutOnResize && _owner != null)
			{
				if (_group != null && _group._expanded && _group.Visible)
				{
					if (!_owner.ShouldShowGroupBorder(_group))
					{
						_group.ContainerHeight = base.Height;
					}
					else
					{
						_group.ContainerHeight = base.Height + 1;
					}
					_owner.InvalidateLayout();
				}
				else if (_item != null && _item.Visible && _item.Group != null && _item.Group._expanded)
				{
					_item.ContainerHeight = base.Height;
					_owner.InvalidateLayout();
				}
			}
			Invalidate(invalidateChildren: true);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		internal void SetContainerStyle(TaskPaneFormatStyle taskPaneFormatStyle)
		{
			_formatStyle = taskPaneFormatStyle;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			bool isHandled = false;
			if (_owner != null && (Group != null || Item != null))
			{
				if (!_owner._isLayoutValid)
				{
					_owner._isLayoutValid = true;
					_owner.LayoutContent(e.Graphics);
				}
				_owner.PrepareContainerStyle(this, Group, _item);
				if (_group != null)
				{
					if (_owner.IsAreaOwnerDrawn(OwnerDrawnArea.Item))
					{
						DrawTaskPaneAreaEventArgs groupAreaArgs = new DrawTaskPaneAreaEventArgs(e.Graphics, _owner, TaskPaneArea.Items, _group.GetItemsBounds(), _group);
						_owner.OnDrawTaskPaneAreaCore(groupAreaArgs);
						isHandled = groupAreaArgs.Handled;
					}
					if (!isHandled)
					{
						_formatStyle.BackColorAlphaMode = AlphaMode.Opaque;
						if (_formatStyle.BackColor.IsEmpty)
						{
							TaskPaneFormatStyle taskPaneFormatStyle = _owner.CreateDefaultFormatStyle();
							taskPaneFormatStyle = taskPaneFormatStyle.MergeWith(_owner.BackgroundFormatStyle);
							_formatStyle = _formatStyle.MergeWith(taskPaneFormatStyle);
							if (_formatStyle.BackColor.IsEmpty)
							{
								_formatStyle.BackColor = GetEffectiveBackColor();
							}
						}
						_owner.DrawGroupBackground(e.Graphics, _group, base.ClientRectangle, _formatStyle.GetBackBrush(base.ClientRectangle), e.ClipRectangle);
						_group.WatermarkImage.DrawImage(e.Graphics, base.ClientRectangle);
					}
				}
				else if (_item != null)
				{
					if (_owner.IsAreaOwnerDrawn(OwnerDrawnArea.Item))
					{
						DrawTaskPaneAreaEventArgs itemAreaArgs = new DrawTaskPaneAreaEventArgs(e.Graphics, _owner, TaskPaneArea.Items, Item.GetBounds(), Group);
						_owner.OnDrawTaskPaneAreaCore(itemAreaArgs);
						isHandled = itemAreaArgs.Handled;
					}
					if (!isHandled)
					{
						Rectangle backgroundBounds = Group.GetItemsBounds();
						backgroundBounds.Y -= _item.Bounds.Y;
						backgroundBounds.X = -1;
						_formatStyle.BackColorAlphaMode = AlphaMode.Opaque;
						if (_formatStyle.BackColor.IsEmpty)
						{
							_formatStyle.BackColor = GetEffectiveBackColor();
						}
						_owner.DrawGroupBackground(e.Graphics, Group, backgroundBounds, _formatStyle.GetBackBrush(backgroundBounds), e.ClipRectangle);
						Group.WatermarkImage.DrawImage(e.Graphics, backgroundBounds);
					}
				}
				if (_formatStyle != null)
				{
					_formatStyle.DisposeDrawingObjects();
				}
			}
			base.OnPaint(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (_group != null && _group.Container)
			{
				_group._containerControl = null;
				_group.Container = false;
				_owner.SyncGroupContainer(_group);
				_owner.Update();
				_group = null;
			}
			if (base.ContainsFocus)
			{
				TaskPanePaintHelper.FocusControl(_owner);
			}
			_owner = null;
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void SetContainer()
		{
			_container = new Container();
		}
	}
}
