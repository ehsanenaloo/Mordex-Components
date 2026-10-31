using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation
{
	[ToolboxItem(false)]
	[Designer("Mordex.Windows.Navigation.Design.ContainerDesigner, Mordex.Windows.Navigation.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class NavigationBarContainerControl : Panel
	{
		private Container _container = new Container();

		internal MxNavigationBar _owner;

		internal NavigationGroup _group;

		public MxNavigationBar NavigationBar => _owner;

		public NavigationGroup Group => _group;

		public NavigationBarContainerControl()
		{
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetContainer();
		}

		internal NavigationBarContainerControl(MxNavigationBar owner, NavigationGroup group)
		{
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetContainer();
			_owner = owner;
			_group = group;
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			bool isHandled = false;
			if (_owner != null)
			{
				if (!_owner._isGroupLayoutValid)
				{
					_owner.LayoutGroups(e.Graphics);
					_owner._isGroupLayoutValid = true;
				}
				if (_owner.IsOwnerDrawn(OwnerDrawnArea.GroupBackground))
				{
					DrawNavigationBarAreaEventArgs drawArgs = new DrawNavigationBarAreaEventArgs(e.Graphics, _owner, NavigationBarArea.GroupBackground, _owner._itemsAreaBounds, _group);
					_owner.OnDrawNavigationBarAreaCore(drawArgs);
					isHandled = drawArgs.Handled;
				}
				if (!isHandled)
				{
					_owner.DrawSelectedGroupBackground(e.Graphics, base.ClientRectangle, e.ClipRectangle);
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			Invalidate(invalidateChildren: true);
		}

		protected override void Dispose(bool disposing)
		{
			if (_group != null && _group.Container)
			{
				_group.Container = false;
				_owner.InvalidateLayout(true);
			}
			if (base.ContainsFocus)
			{
				NavigationGraphicsHelper.FocusControl(_owner);
			}
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
