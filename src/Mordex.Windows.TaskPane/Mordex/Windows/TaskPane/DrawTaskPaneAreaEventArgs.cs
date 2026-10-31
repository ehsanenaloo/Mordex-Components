using System;
using System.Drawing;

namespace Mordex.Windows.TaskPane
{
	public class DrawTaskPaneAreaEventArgs : EventArgs
	{
		private MxTaskPane _taskPane;

		private object _itemDrawn;

		private Graphics _graphics;

		private Rectangle _bounds = Rectangle.Empty;

		private Brush _backBrush;

		private Brush _foreBrush;

		private Color _backColor = Color.Empty;

		private Color _foreColor = Color.Empty;

		private TaskPaneArea _area;

		private bool _handled;

		public Graphics Graphics => _graphics;

		public Rectangle Bounds => _bounds;

		public TaskPaneGroup Group
		{
			get
			{
				if (_area != TaskPaneArea.Background)
				{
					if (_area == TaskPaneArea.Items)
					{
						return Item.Group;
					}
					return _itemDrawn as TaskPaneGroup;
				}
				return null;
			}
		}

		public TaskPaneItem Item
		{
			get
			{
				if (_area == TaskPaneArea.Items)
				{
					return _itemDrawn as TaskPaneItem;
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
					_backBrush = _taskPane.GetAreaBackBrush((ExplorerArea)_area, _itemDrawn);
				}
				return _backBrush;
			}
		}

		public Brush ForeBrush
		{
			get
			{
				if (_area == TaskPaneArea.Background || _area == TaskPaneArea.Items)
				{
					return null;
				}
				if (_foreBrush == null)
				{
					_foreBrush = _taskPane.GetAreaForeBrush((ExplorerArea)_area, _itemDrawn);
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
					_backColor = _taskPane.GetAreaBackColor((ExplorerArea)_area, _itemDrawn);
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
					_foreColor = _taskPane.GetAreaForeColor((ExplorerArea)_area, _itemDrawn);
				}
				return _foreColor;
			}
		}

		public TaskPaneArea TaskPaneArea => _area;

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

		internal DrawTaskPaneAreaEventArgs(Graphics graphics, MxTaskPane explorer, TaskPaneArea area, Rectangle bounds, object itemDrawn)
		{
			_graphics = graphics;
			_area = area;
			_bounds = bounds;
			_itemDrawn = itemDrawn;
			_taskPane = explorer;
		}

		public void PaintBackground()
		{
			_taskPane.DrawDefaultArea(_graphics, (ExplorerArea)_area, _itemDrawn);
		}
	}
}
