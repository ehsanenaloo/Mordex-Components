using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class CommandContainerAccessibleObject : RibbonLayoutItemAccessibleObject
	{
		private CommandContainerRenderer _containerRenderer;

		internal CommandContainerAccessibleObject(CommandContainerRenderer item)
			: base(item)
		{
			_containerRenderer = item;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_containerRenderer != null && _containerRenderer._childRenderers != null)
			{
				childCount = _containerRenderer._childRenderers.Count;
			}
			if (_containerRenderer is RibbonGroupBoxRenderer ribbonRenderer && ribbonRenderer._dialogLauncher != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_containerRenderer != null && _containerRenderer._childRenderers != null)
			{
				if (index >= 0 && index < _containerRenderer._childRenderers.Count)
				{
					return _containerRenderer._childRenderers[index].AccessibilityObject;
				}
				index -= _containerRenderer._childRenderers.Count;
			}
			if (index == 0 && _containerRenderer is RibbonGroupBoxRenderer ribbonRenderer && ribbonRenderer._dialogLauncher != null)
			{
				return ribbonRenderer._dialogLauncher.AccessibilityObject;
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_containerRenderer._parentControl != null)
			{
				Point point = _containerRenderer._parentControl.PointToClient(new Point(screenX, screenY));
				CommandBaseRenderer commandBaseRenderer = _containerRenderer.FindRendererAt(point.X, point.Y);
				if (commandBaseRenderer != null)
				{
					return commandBaseRenderer.AccessibilityObject;
				}
			}
			return this;
		}
	}
}
