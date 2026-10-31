using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal interface ICommandRenderHost
	{
		Control ParentControl { get; }

		int SmallItemHeight { get; }

		Size SmallImageSize { get; }

		Size LargeImageSize { get; }

		Font Font { get; }

		RibbonColorScheme ColorScheme { get; }


		CommandBaseRenderer FocusItem { get; set; }

		CommandBaseRenderer MouseDownItem { get; set; }

		AccessibleObject AccessibilityObject { get; }

		int MaxItemRows { get; }

		CommandBaseRenderer ActiveInputItem { get; set; }

		ISite Site { get; }

		bool UseCompatibleTextRendering { get; }

		RightToLeft RightToLeft { get; }

		bool IsInDesignMode { get; }

		CommandBaseEditorDialog ChildMenu { get; set; }

		CommandCollection Commands { get; }

		Form ParentForm { get; }

		CommandBaseRenderer SelectedItem { get; set; }

		bool ShowToolTips { get; }

		MxRibbon Ribbon { get; }

		ImageList GetImageList(CommandBase command);

		ImageList GetLargeImageList(CommandBase command);

		Point PointToScreen(Point clientPoint);

		void ShowQuickCustomizeMenu(CommandBase command);
	}
}
