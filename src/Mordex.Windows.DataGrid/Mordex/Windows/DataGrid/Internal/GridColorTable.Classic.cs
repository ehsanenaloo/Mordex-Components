using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridColorTable
	{
				private bool LoadClassicColors()
		{
			ref Color reference = ref _colors[37];
			reference = SystemColors.Highlight;
			ref Color highlightTextColor = ref _colors[38];
			highlightTextColor = SystemColors.HighlightText;
			ref Color windowColor = ref _colors[35];
			windowColor = SystemColors.Window;
			ref Color windowTextColor = ref _colors[36];
			windowTextColor = SystemColors.WindowText;
			ref Color controlDarkColor = ref _colors[0];
			controlDarkColor = SystemColors.ControlDark;
			ref Color otherReference = ref _colors[1];
			otherReference = Color.Empty;
			ref Color nextReference = ref _colors[3];
			nextReference = Color.Empty;
			ref Color controlColor = ref _colors[2];
			controlColor = SystemColors.Control;
			ref Color otherControlDarkColor = ref _colors[4];
			otherControlDarkColor = SystemColors.ControlDark;
			ref Color otherControlColor = ref _colors[5];
			otherControlColor = SystemColors.Control;
			ref Color previousReference = ref _colors[6];
			previousReference = Color.Empty;
			ref Color controlTextColor = ref _colors[7];
			controlTextColor = SystemColors.ControlText;
			ref Color nextControlDarkColor = ref _colors[9];
			nextControlDarkColor = SystemColors.ControlDark;
			ref Color previousControlDarkColor = ref _colors[8];
			previousControlDarkColor = SystemColors.ControlDark;
			ref Color nextControlColor = ref _colors[10];
			nextControlColor = SystemColors.Control;
			ref Color firstReference = ref _colors[11];
			firstReference = Color.Empty;
			ref Color otherControlTextColor = ref _colors[12];
			otherControlTextColor = SystemColors.ControlText;
			ref Color highlightColor = ref _colors[15];
			highlightColor = SystemColors.Highlight;
			ref Color firstControlDarkColor = ref _colors[39];
			firstControlDarkColor = SystemColors.ControlDark;
			ref Color disabledColor = ref _colors[40];
			disabledColor = SystemColors.Control;
			ref Color grayTextColor = ref _colors[34];
			grayTextColor = SystemColors.GrayText;
			ref Color gridColor = ref _colors[41];
			gridColor = _colors[9];
			return true;
		}
	}
}
