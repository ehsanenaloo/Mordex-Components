using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal interface IKeyboardNavigable
	{
		bool MoveRight(CommandBaseRenderer selectedItem);

		bool MoveLeft(CommandBaseRenderer selectedItem);

		bool MoveDown(CommandBaseRenderer selectedItem);

		bool MoveUp(CommandBaseRenderer selectedItem);

		bool MoveNext(CommandBaseRenderer selectedItem);

		bool MovePrevious(CommandBaseRenderer selectedItem);

		void RemoveControls();

		CommandBase GetCommandAt(int x, int y);

		CommandBaseRenderer GetCommandLayoutItem(CommandBase command);

		bool SelectFirstItem();

		bool SelectLastItem();

		void AddKeyTips(Graphics graphics, TextLayoutRenderer textRenderer, Font font, IList<RibbonKeyTipEntry> keyTips, bool showToolTip);
	}
}
