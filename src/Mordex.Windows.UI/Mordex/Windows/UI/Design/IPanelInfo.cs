using System;
using System.Collections;
using System.Drawing;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	public interface IPanelInfo
	{
		Guid PanelId { get; }

		int TileSize { get; }

		Size Size { get; }

		IPanelInfo Parent { get; }

		bool IsStaticGroup { get; }

		bool IsGroup { get; }

		IEnumerable Children { get; }

		PanelGroupStyle GroupStyle { get; }

		Point FloatingLocation { get; }

		PanelDockStyle DockStyle { get; }

		bool CurrentState { get; }
	}
}
