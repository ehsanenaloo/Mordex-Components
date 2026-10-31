using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal partial class ScrollBarControl : Control
	{
		private enum ScrollAreaType
		{
			NotSet,
			Arrow,
			Thumb,
			Page
		}

		private struct ControlStyleControl2Data
		{
			internal bool _isPressed;

			internal bool _controlStyleControl2DataBoolean;

			internal Rectangle _bounds;

			internal bool _isVisible;

			internal ScrollAreaType _type;

			internal ControlStyleControl2Data(ScrollAreaType type)
			{
				_isPressed = false;
				_controlStyleControl2DataBoolean = false;
				_bounds = Rectangle.Empty;
				_type = type;
				_isVisible = true;
			}
		}

		internal class ScrollBarAccessibleObject : ControlAccessibleObject
		{
			private ScrollBarAreaAccessibleObject[] _gridAccessibleObject11Array;

			public override AccessibleRole Role => AccessibleRole.ScrollBar;

			public override AccessibleObject Parent
			{
				get
				{
					if (base.Owner.Parent is MxDataGrid dataGrid)
					{
						return dataGrid.AccessibilityObject;
					}
					return base.Parent;
				}
			}

			public override string Name
			{
				get
				{
					ScrollBarControl owner = (ScrollBarControl)base.Owner;
					if (owner.GetScrollBarType() == ScrollBarType.Horizontal)
					{
						return "Horizontal ScrollBar";
					}
					return "Vertical ScrollBar";
				}
			}

			public override string Value => string.Concat(((ScrollBarControl)base.Owner).GetScrollValue());

			internal ScrollBarAccessibleObject(ScrollBarControl owner)
				: base(owner)
			{
				_gridAccessibleObject11Array = new ScrollBarAreaAccessibleObject[5];
				_gridAccessibleObject11Array[0] = new ScrollBarAreaAccessibleObject(owner, 0);
				_gridAccessibleObject11Array[1] = new ScrollBarAreaAccessibleObject(owner, 1);
				_gridAccessibleObject11Array[2] = new ScrollBarAreaAccessibleObject(owner, 2);
				_gridAccessibleObject11Array[3] = new ScrollBarAreaAccessibleObject(owner, 3);
				_gridAccessibleObject11Array[4] = new ScrollBarAreaAccessibleObject(owner, 4);
			}

			public override int GetChildCount()
			{
				return _gridAccessibleObject11Array.Length;
			}

			public override AccessibleObject GetChild(int index)
			{
				return _gridAccessibleObject11Array[index];
			}
		}

		internal class ScrollBarAreaAccessibleObject : AccessibleObject
		{
			private int _areaID;

			private ScrollBarControl _owner;

			public override AccessibleRole Role
			{
				get
				{
					if (_areaID == 2)
					{
						return AccessibleRole.Indicator;
					}
					return AccessibleRole.PushButton;
				}
			}

			public override string DefaultAction
			{
				get
				{
					if (_areaID != 2)
					{
						return "Press";
					}
					return base.DefaultAction;
				}
			}

			public override string Name
			{
				get
				{
					bool condition = _owner.GetScrollBarType() == ScrollBarType.Vertical;
					switch (_areaID)
					{
					case 0:
						if (condition)
						{
							return "Line Up";
						}
						return "Line Left";
					case 1:
						if (condition)
						{
							return "Page Up";
						}
						return "Page Left";
					case 2:
						return "Position";
					case 3:
						if (condition)
						{
							return "Page Down";
						}
						return "Page Right";
					case 4:
						if (condition)
						{
							return "Line Down";
						}
						return "Line Up";
					default:
						return "";
					}
				}
			}

			public override AccessibleStates State
			{
				get
				{
					if (Parent.State == AccessibleStates.Invisible)
					{
						return AccessibleStates.Invisible;
					}
					return _owner.ComputeAccessibleStates(_areaID);
				}
			}

			public override Rectangle Bounds => _owner.GetAreaScreenBounds(_areaID);

			public override AccessibleObject Parent => _owner.AccessibilityObject;

			internal ScrollBarAreaAccessibleObject(ScrollBarControl owner, int areaID)
			{
				_areaID = areaID;
				_owner = owner;
			}

			public override void DoDefaultAction()
			{
				if (_areaID != 2)
				{
					_owner.PerformScroll(_areaID);
				}
				base.DoDefaultAction();
			}
		}

		private int _thickness = -1;

		private Timer _timer;

		private Timer _controlStyleControl2Timer;


		private ControlStyleControl2Data[] _controlStyleControl2DataArray = new ControlStyleControl2Data[10];

		private ScrollBarType _type;

		private int _pageSize = 1;

		private int _minimum;

		private int _maximum = int.MaxValue;

		private int _scrollValue;

		private int _scrollStep = 1;

		private bool _controlStyleControl2Boolean = true;

		private bool _isDraggingThumb;

		private int _dragOffset;

		private int _valueAtDragStart;

		private int _pressedArea = -1;

		private int _hotArea = -1;

		private bool _isMouseOver;

		private static object _scrollEventKey = new object();

		private static object _controlStyleControl2Value = new object();

		internal ControlStyle _style;

		internal ScrollBarControl(ScrollBarType type)
		{
			_type = type;
			InitializeTimers();
		}
	}
}
