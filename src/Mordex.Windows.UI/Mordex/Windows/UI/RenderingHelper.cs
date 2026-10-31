#pragma warning disable CS0618 // Bridges the obsolete Office2007 API to the current one.
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI
{
	internal partial class RenderingHelper
	{
		private const int MaxChannelValue = 255;

		private const int KeyCodeMask = 0xFFFF;

		private const long ModifierMask = 0xFFFF0000L;

		private const int FadeLength = 4;

		private const int DragFrameThickness = 4;

		private const int DragHatchSize = 8;

		private const int RegionXor = 3;

		// Raster operation that inverts the destination where the pattern brush is set (PATINVERT).
		private const uint PatternInvertRop = 5898313u;

		// Brush style value for a brush made from a bitmap pattern (BS_PATTERN).
		private const uint PatternBrushStyle = 3u;

		// Windows 7 is version 6.1, build 7600.
		private static readonly Version Windows7Version = new Version(6, 1, 7600);

		private static readonly object _dragHatchLock = new object();

		private static IntPtr _dragHatchBrush;

		private static Color _dragHatchColor;
	}
}
