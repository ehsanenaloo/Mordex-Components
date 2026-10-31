using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public partial class ControlStyle : IMordexXmlLayoutsSupport, IGridEnvironment
	{
		internal IGridStyleSource _printDocument;

		private EventHandler _changedHandler;

		private ButtonAppearance _buttonAppearance = ButtonAppearance.Light3D;

		private Color _flatBorderColor = SystemColors.WindowFrame;

		private Color _controlColor = SystemColors.Control;

		private Pen _flatBorderPen;

		private Brush _controlBrush;

		private Color _controlTextColor = SystemColors.ControlText;

		private Brush _controlStyleBrush;

		private Pen _controlStylePen;

		private Color _scrollBarColor = SystemColors.ScrollBar;

		private Brush _scrollBarBrush;

		private Color _controlLightColor = SystemColors.ControlLight;

		private Pen _controlLightPen;

		private Color _controlLightLightColor = SystemColors.ControlLightLight;

		private Pen _controlLightLightPen;

		private Brush _controlLightLightBrush;

		private Color _controlDarkColor = SystemColors.ControlDark;

		private Pen _controlDarkPen;

		private Brush _controlDarkBrush;

		private Color _controlDarkDarkColor = SystemColors.ControlDarkDark;

		private Pen _controlDarkDarkPen;

		private Color _windowColor = SystemColors.Window;

		private Brush _windowBrush;

		private Color _windowTextColor = SystemColors.WindowText;

		private Brush _windowTextBrush;

		private Pen _windowTextPen;

		private Color _hoverBaseColor = SystemColors.Highlight;

		private Color _hoverBlendColor = SystemColors.Window;

		private float _hoverBlend = 0.667f;

		private float _hoverSelectedBlend = 0.5f;

		private Brush _hoverBrush;

		private Brush _hoverSelectedBrush;

		private Pen _hoverPen;

		private ControlStyleLayoutState _controlStyleLayoutState;

		private bool _classicLook;

		internal ControlStyle(IGridStyleSource owner)
		{
			ApplyThemedPalette();
			_printDocument = owner;
		}
	}
}
