using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal partial class VisualStylePainter
	{
		private const string ThemeNormalColor = "NormalColor";

		private const string ThemeHomeStead = "HomeStead";

		private const string ThemeMetallic = "Metallic";

		private const int RebarGradientWidth = 700;

		internal Color _openItemBackColor;

		internal Color _borderColor;

		internal Color _hotBorderColor;

		internal Color _pressedBorderColor;

		internal Color _checkedBorderColor;

		internal Color _menuItemBorderColor;

		internal Color _checkedColor;

		internal Color _checkedGradientColor;

		internal Color _hotColor;

		internal Color _hotGradientColor;

		internal Color _pressedColor;

		internal Color _pressedGradientColor;

		internal Color _panelBackColor;

		internal Color _itemBackStartColor;

		internal Color _itemBackEndColor;

		private Color _comboBorderColor;

		public bool _isStandardStyle = true;

		private string _className = "";

		internal Color _rebarColor;

		internal Color _rebarGradientColor;

		internal Color _commandBarColor;

		internal Color _commandBarGradientColor;

		internal Color _commandBarBorderColor;

		internal Color _customizeButtonColor;

		internal Color _customizeButtonGradientColor;

		internal Color _menuColor;

		internal Color _imageAreaColor;

		internal Color _imageAreaGradientColor;

		internal Color _infrequentImageAreaColor;

		internal Color _infrequentImageAreaGradientColor;

		internal Color _itemHotColor;

		internal Color _itemCheckedColor;

		internal Color _itemPressedColor;

		internal Color _itemHotBorderColor;

		internal Color _itemPressedBorderColor;

		internal Color _itemCheckedBorderColor;

		internal Color _floatingBarColor;

		internal Color _gripColor;

		internal Color _gripHighlightColor;

		internal Color _separatorColor;

		internal Color _menuSeparatorColor;

		internal Color _tabColor;

		internal Color _tabGradientColor;

		internal Color _tabPageBackColor;

		internal Color _tabBorderColor;

		internal Color _popupBorderColor;

		private VisualStyleRenderer _visualStyleRenderer;

		private IntPtr _tabThemeHandle = IntPtr.Zero;

		private Pen _borderPen;

		internal VisualStylePainter()
		{
		}

		internal VisualStylePainter(string className)
		{
			_className = className;
			ApplyVisualStyle();
		}

		public void DisposeResources()
		{
			if (_borderPen != null)
			{
				_borderPen.Dispose();
				_borderPen = null;
			}
			if (_tabThemeHandle != IntPtr.Zero)
			{
				NativeMethods.CloseThemeData(_tabThemeHandle);
				_tabThemeHandle = IntPtr.Zero;
			}
		}
	}
}
