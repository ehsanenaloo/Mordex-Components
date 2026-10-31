using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonTabContentAccessibleObject : Control.ControlAccessibleObject
	{
		private RibbonTabContentRenderer _contentRenderer;

		public override Rectangle Bounds
		{
			get
			{
				if (_contentRenderer != null && _contentRenderer.GetParentControl() != null)
				{
					return new Rectangle(_contentRenderer.GetParentControl().PointToScreen(_contentRenderer._bounds.Location), _contentRenderer._bounds.Size);
				}
				return base.Bounds;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_contentRenderer != null && _contentRenderer.GetParentControl() != null)
				{
					return _contentRenderer.GetParentControl().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override string Name => "Lower Ribbon";

		public override AccessibleRole Role => AccessibleRole.Pane;

		public override AccessibleStates State => AccessibleStates.None;

		public RibbonTabContentAccessibleObject(RibbonTabContentRenderer ribbonLayoutHandler)
			: base(ribbonLayoutHandler.GetParentControl())
		{
			_contentRenderer = ribbonLayoutHandler;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_contentRenderer._groupRenderers != null)
			{
				childCount += _contentRenderer._groupRenderers.Count;
			}
			if (_contentRenderer._leftScrollButton != null)
			{
				childCount++;
			}
			if (_contentRenderer._rightScrollButton != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_contentRenderer._groupRenderers != null)
			{
				if (index >= 0 && index < _contentRenderer._groupRenderers.Count)
				{
					return _contentRenderer._groupRenderers[index].AccessibilityObject;
				}
				index -= _contentRenderer._groupRenderers.Count;
			}
			if (index >= 0)
			{
				if (index == 0)
				{
					if (_contentRenderer._leftScrollButton != null)
					{
						return _contentRenderer._leftScrollButton.AccessibilityObject;
					}
					if (_contentRenderer._rightScrollButton != null)
					{
						return _contentRenderer._rightScrollButton.AccessibilityObject;
					}
				}
				if (index == 1 && _contentRenderer._rightScrollButton != null)
				{
					return _contentRenderer._rightScrollButton.AccessibilityObject;
				}
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_contentRenderer.GetParentControl() != null)
			{
				Point point = _contentRenderer.GetParentControl().PointToClient(new Point(screenX, screenY));
				RibbonGroupBoxRenderer ribbonRenderer = _contentRenderer.FindGroupRendererAt(point.X, point.Y);
				if (ribbonRenderer != null)
				{
					return ribbonRenderer.AccessibilityObject;
				}
			}
			return this;
		}
	}
}
