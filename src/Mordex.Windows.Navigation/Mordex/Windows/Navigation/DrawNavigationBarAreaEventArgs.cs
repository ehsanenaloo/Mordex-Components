using System;
using System.Drawing;

namespace Mordex.Windows.Navigation
{
	public class DrawNavigationBarAreaEventArgs : EventArgs
	{
		private MxNavigationBar _navigationBar;

		private object _itemDrawn;

		private Graphics _graphics;

		private Rectangle _bounds = Rectangle.Empty;

		private Brush _backBrush;

		private Brush _foreBrush;

		private Color _backColor = Color.Empty;

		private Color _foreColor = Color.Empty;

		private NavigationBarArea _area;

		private bool _handled;

		public Graphics Graphics => _graphics;

		public Rectangle Bounds => _bounds;

		public NavigationGroup Group
		{
			get
			{
				if (_area != NavigationBarArea.Background)
				{
					if (_area != NavigationBarArea.Item)
					{
						return _itemDrawn as NavigationGroup;
					}
					if (Item != null)
					{
						return Item.Group;
					}
				}
				return null;
			}
		}

		public NavigationItem Item
		{
			get
			{
				if (_area == NavigationBarArea.Item)
				{
					return _itemDrawn as NavigationItem;
				}
				return null;
			}
		}

		public Brush BackBrush
		{
			get
			{
				if (_backBrush == null)
				{
					_backBrush = _navigationBar.CreateAreaBackBrush(_area, _itemDrawn);
				}
				return _backBrush;
			}
		}

		public Brush ForeBrush
		{
			get
			{
				if (_area == NavigationBarArea.Background || _area == NavigationBarArea.GroupBackground)
				{
					return null;
				}
				if (_foreBrush == null)
				{
					_foreBrush = _navigationBar.CreateAreaTextBrush(_area, _itemDrawn);
				}
				return _foreBrush;
			}
		}

		public Color BackColor
		{
			get
			{
				if (_backColor == Color.Empty)
				{
					_backColor = _navigationBar.GetAreaBackColor(_area, _itemDrawn);
				}
				return _backColor;
			}
		}

		public Color ForeColor
		{
			get
			{
				if (_foreColor == Color.Empty)
				{
					_foreColor = _navigationBar.GetAreaTextColor(_area, _itemDrawn);
				}
				return _foreColor;
			}
		}

		public NavigationBarArea NavigationBarArea => _area;

		public bool Handled
		{
			get
			{
				return _handled;
			}
			set
			{
				_handled = value;
			}
		}

		internal DrawNavigationBarAreaEventArgs(Graphics graphics, MxNavigationBar navigationBar, NavigationBarArea area, Rectangle bounds, object itemDrawn)
		{
			_graphics = graphics;
			_area = area;
			_bounds = bounds;
			_itemDrawn = itemDrawn;
			_navigationBar = navigationBar;
		}

		public void PaintBackground()
		{
			_navigationBar.DrawArea(_graphics, _area, _itemDrawn);
		}
	}
}
