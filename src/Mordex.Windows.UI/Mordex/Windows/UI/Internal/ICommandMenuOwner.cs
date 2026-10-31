using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal interface ICommandMenuOwner
	{
		CommandControl ChildMenu { get; set; }

		MxCommandCollection Commands { get; }

		MxCommandManager CommandManagerOwner { get; }

		Size SmallImageSize { get; }

		Size LargeImageSize { get; }

		bool UseSmallImages { get; }


		DropDownAnimation EffectiveAnimation { get; }

		bool CanUseThemes { get; }

		Font Font { get; }

		Point MousePoint { get; set; }

		Form FindForm { get; }

		MxCommand SelectedCommand { get; set; }

		bool ShowToolTips { get; }

		bool ShowInfrequentItems { get; set; }

		RightToLeft RightToLeft { get; }

		ImageList GetImageList(MxCommand cmd);
	}
}
