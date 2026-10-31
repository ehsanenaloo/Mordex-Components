using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Docking
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class PanelPadding : IMordexXmlLayoutsSupport
	{
		private int _left;

		private int _top;

		private int _right;

		private int _bottom;

		private int _defaultLeft;

		private int _defaultTop;

		private int _defaultRight;

		private int _defaultBottom;

		private MxDockManager _panelManager;

		[Description("The padding width for all panels in its container control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Localizable(true)]
		public int All
		{
			get
			{
				if (_left == _top && _top == _right && _right == _bottom)
				{
					return _left;
				}
				return 0;
			}
			set
			{
				_left = value;
				_top = value;
				_right = value;
				_bottom = value;
				NotifyManager();
			}
		}

		[Localizable(true)]
		[Description("The padding width for the left edge of the container control.")]
		public int Left
		{
			get
			{
				return _left;
			}
			set
			{
				if (_left != value)
				{
					_left = value;
					NotifyManager();
				}
			}
		}

		[Description("The padding width for the top edge of the container control.")]
		[Localizable(true)]
		public int Top
		{
			get
			{
				return _top;
			}
			set
			{
				if (_top != value)
				{
					_top = value;
					NotifyManager();
				}
			}
		}

		[Description("The padding width for the top edge of the container control.")]
		[Localizable(true)]
		public int Right
		{
			get
			{
				return _right;
			}
			set
			{
				if (_right != value)
				{
					_right = value;
					NotifyManager();
				}
			}
		}

		[Description("The padding width for the bottom edge of the container control.")]
		[Localizable(true)]
		public int Bottom
		{
			get
			{
				return _bottom;
			}
			set
			{
				if (_bottom != value)
				{
					_bottom = value;
					NotifyManager();
				}
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal PanelPadding(MxDockManager panelManager, int left, int top, int right, int bottom)
		{
			_panelManager = panelManager;
			SetDefaults(left, top, right, bottom);
			_left = left;
			_top = top;
			_right = right;
			_bottom = bottom;
		}

		private void SetDefaults(int left, int top, int right, int bottom)
		{
			_defaultLeft = left;
			_defaultTop = top;
			_defaultRight = right;
			_defaultBottom = bottom;
		}

		private void NotifyManager()
		{
			_panelManager.LayoutAutoHideStrips();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (!ShouldSerializeBottom() && !ShouldSerializeLeft() && !ShouldSerializeRight())
			{
				return ShouldSerializeTop();
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLeft()
		{
			return _left != _defaultLeft;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLeft()
		{
			Left = _defaultLeft;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTop()
		{
			return _top != _defaultTop;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTop()
		{
			Top = _defaultTop;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeRight()
		{
			return _right != _defaultRight;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetRight()
		{
			Right = _defaultRight;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBottom()
		{
			return _bottom != _defaultBottom;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBottom()
		{
			Bottom = _defaultBottom;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			ResetBottom();
			ResetLeft();
			ResetRight();
			ResetTop();
		}

		internal Rectangle Deflate(Rectangle rectangle)
		{
			rectangle.X += _left;
			rectangle.Y += _top;
			rectangle.Width -= _right + _left;
			rectangle.Height -= _top + _bottom;
			return rectangle;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_bottom = (int)LayoutReader.GetPropertyValue(reader, "Bottom", typeof(int), _defaultBottom);
			_top = (int)LayoutReader.GetPropertyValue(reader, "Top", typeof(int), _defaultTop);
			_left = (int)LayoutReader.GetPropertyValue(reader, "Left", typeof(int), _defaultLeft);
			_right = (int)LayoutReader.GetPropertyValue(reader, "Right", typeof(int), _defaultRight);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_left != _defaultLeft)
			{
				info.AddValue("Left", _left);
			}
			if (_top != _defaultTop)
			{
				info.AddValue("Top", _top);
			}
			if (_right != _defaultRight)
			{
				info.AddValue("Right", _right);
			}
			if (_bottom != _defaultBottom)
			{
				info.AddValue("Bottom", _bottom);
			}
		}
	}
}
