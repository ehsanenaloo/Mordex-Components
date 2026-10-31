using System;

namespace Mordex.Windows.UI.Controls
{
	internal interface IDropListOwner
	{
		IntPtr DropWindowHandle { get; }

		bool UseCompatibleTextRendering { get; }

		void OnDropListHide(bool cancel);
	}
}
