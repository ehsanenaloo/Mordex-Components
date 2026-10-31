using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[ToolboxItem(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public partial class ScrollableBase : ContainerControl, IGridStyleSource
	{
		private int _scrollBarWidth = -1;

		private ScrollBarControl _horizontalScrollBar;

		internal Panel _scrollableBasePanel;

		private ScrollBarControl _vScroll;

		internal NumericEditControl _numericEditControl;

		internal Font _controlStyleFont;

		private bool _isRightToLeft;

		private BorderStyle _borderStyle = BorderStyle.SunkenLight3D;

		private Rectangle _contentBounds;

		private bool _isHorizontalBarVisible;

		private bool _isVerticalBarVisible;

		internal int _borderWidth;

		private bool _needsVerticalScroll;

		private bool _needsHorizontalScroll;

		private ScrollBars _scrollBars = ScrollBars.Automatic;

		private bool _isDropDownOpen;

		private bool _recordNavigator;

		internal ControlStyle _gridControlStyle;


		private static readonly object _scrollableBaseValue = new object();

		private bool? _useCompatibleTextRendering;

		protected ScrollableBase()
		{
			UpdateScrollableBase();
			InitializeScrollControls();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			RelayoutScrollControls();
		}
	}
}
