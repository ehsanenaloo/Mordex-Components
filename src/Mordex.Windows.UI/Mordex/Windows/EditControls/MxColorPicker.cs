using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	[DefaultEvent("SelectedColorChanged")]
	[Designer("Mordex.Windows.UI.Controls.Design.ColorPickerDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxColorPicker : Control, IFormatStyleOwner
	{
		private WindowsAppearanceState _windowsAppearanceState;

		private Container _container = new Container();

		private FormatStyleLayoutState _automaticButtonState;

		private FormatStyleLayoutState _moreColorsButtonState;

		private bool _isLayoutDirty = true;

		private Size _colorItemSize = DefaultColorItemSize;

		private string _automaticButtonText = "Automatic";

		private bool _showMoreColorsButton = true;

		private bool _showAutomaticButton = true;

		private string _moreColorsButtonText = "More Colors...";

		private ColorEntryCollection _colorEntries;

		private int _columns = DefaultColumns;

		private int _borderWidth = FlatBorderWidth;

		private StateStyles _stateStyles;

		private Color _automaticColor = Color.Black;

		private Color _selectedColor = Color.Empty;

		private ColorEntry _selectedEntry;

		private WindowMessageHandler _customToolTip;

		private ToolTip _toolTip;

		private ColorEntry _hotEntry;

		private bool _showToolTips = true;

		private Color _flatBorderColor = Color.Empty;

		private bool? _useCompatibleTextRendering;

		private Mordex.Windows.UI.BorderStyle _borderStyle = Mordex.Windows.UI.BorderStyle.Flat;

		private bool _isAutomaticSelected = true;

		private FormatStyle _automaticButtonFormatStyle;

		private FormatStyle _moreColorsButtonFormatStyle;

		private static readonly object _automaticButtonClickEvent = new object();

		private static readonly object _moreColorsButtonClickEvent = new object();

		private static readonly object _selectedColorChangedEvent = new object();

private static readonly Size DefaultColorItemSize = new Size(12, 12);

private const int DefaultColumns = 8;

private const int DefaultPickerSize = 100;

private const int OuterPadding = 3;

private const int Spacing = 3;

private const int ButtonHeightPadding = 11;

private const int CellPadding = 6;

private const int AutomaticSwatchLeft = 7;

private const int SwatchInset = 3;

private const int FlatBorderWidth = 1;

private const int ThickBorderWidth = 2;

private const float BackColorWindowMix = 0.15f;

		public MxColorPicker()
		{
			InitializeDefaults();
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
		}

		void IFormatStyleOwner.PropertyChange()
		{
			Invalidate();
		}

		private void InitializeDefaults()
		{
			ResetBackColor();
			_selectedColor = _automaticColor;
			Size = new Size(DefaultPickerSize, DefaultPickerSize);
			_colorEntries = new ColorEntryCollection(this);
			AddDefaultPalette();
			_windowsAppearanceState = new WindowsAppearanceState();
			_windowsAppearanceState.ApplyVisualStyle();
		}

		protected override void Dispose(bool disposing)
		{
			DisposeToolTip();
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			_themeHook?.Stop();
			base.Dispose(disposing);
		}

		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			Invalidate();
		}

		private void SetSelectedEntry(ColorEntry uiColorEntry)
		{
			if (_selectedEntry != uiColorEntry)
			{
				if (_selectedEntry != null)
				{
					_selectedEntry._isPressed = false;
				}
				_selectedEntry = uiColorEntry;
				if (_selectedEntry != null)
				{
					_selectedEntry._isPressed = true;
				}
				Invalidate();
			}
		}

		internal void DisposeToolTip()
		{
			if (_customToolTip != null)
			{
				_customToolTip.HideAndDisposeTimers();
				_customToolTip = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		internal void ShowColorEntryToolTip(ColorEntry uiColorEntry)
		{
			if (uiColorEntry == null || uiColorEntry.ColorName.Length == 0)
			{
				DisposeToolTip();
			}
			else
			{
				if (!ShowToolTips)
				{
					return;
				}
				if (VisualStyleSupport.IsToolTipHookSupported())
				{
					if (_customToolTip == null)
					{
						_customToolTip = new WindowMessageHandler(Font, UseCompatibleTextRendering);
						_customToolTip.RequestShow(uiColorEntry.ColorName, this);
					}
					else
					{
						_customToolTip.UpdateTipText(uiColorEntry.ColorName);
					}
				}
				else
				{
					if (_toolTip == null)
					{
						_toolTip = new Mordex.Windows.Common.ThemedToolTip();
					}
					_toolTip.SetToolTip(this, uiColorEntry.ColorName);
				}
			}
		}

		private void SetHotEntry(ColorEntry uiColorPickerUIColorEntry, MouseButtons mouseButtons)
		{
			if (_hotEntry == uiColorPickerUIColorEntry)
			{
				return;
			}
			if (_hotEntry != null)
			{
				_hotEntry._isHot = false;
				_hotEntry._isPressed = false;
			}
			_hotEntry = uiColorPickerUIColorEntry;
			if (_hotEntry != null)
			{
				_hotEntry._isHot = true;
				if (mouseButtons == MouseButtons.Left)
				{
					_hotEntry._isPressed = true;
				}
			}
			ShowColorEntryToolTip(_hotEntry);
			Invalidate();
		}
	}
}
