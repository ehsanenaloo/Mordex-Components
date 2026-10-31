using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryAccessibleObject : RibbonLayoutItemAccessibleObject
	{
		private GalleryRenderer _galleryRenderer;

		internal GalleryAccessibleObject(GalleryRenderer item)
			: base(item)
		{
			_galleryRenderer = item;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_galleryRenderer != null)
			{
				if (_galleryRenderer._itemRenderers != null)
				{
					childCount = _galleryRenderer._itemRenderers.Count;
				}
				if (_galleryRenderer._scrollUpButton != null)
				{
					childCount++;
				}
				if (_galleryRenderer._scrollDownButton != null)
				{
					childCount++;
				}
				if (_galleryRenderer._expandButton != null)
				{
					childCount++;
				}
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_galleryRenderer != null && _galleryRenderer._itemRenderers != null)
			{
				if (index >= 0 && index < _galleryRenderer._itemRenderers.Count)
				{
					return _galleryRenderer._itemRenderers[index].AccessibilityObject;
				}
				index -= _galleryRenderer._itemRenderers.Count;
			}
			if (index >= 0)
			{
				if (index == 0)
				{
					if (_galleryRenderer._scrollUpButton != null)
					{
						return _galleryRenderer._scrollUpButton.AccessibilityObject;
					}
					if (_galleryRenderer._scrollDownButton != null)
					{
						return _galleryRenderer._scrollDownButton.AccessibilityObject;
					}
					if (_galleryRenderer._expandButton != null)
					{
						return _galleryRenderer._expandButton.AccessibilityObject;
					}
				}
				if (index == 1)
				{
					if (_galleryRenderer._scrollDownButton != null)
					{
						return _galleryRenderer._scrollDownButton.AccessibilityObject;
					}
					if (_galleryRenderer._expandButton != null)
					{
						return _galleryRenderer._expandButton.AccessibilityObject;
					}
				}
				if (index == 2 && _galleryRenderer._expandButton != null)
				{
					return _galleryRenderer._expandButton.AccessibilityObject;
				}
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_galleryRenderer._parentControl != null)
			{
				Point point = _galleryRenderer._parentControl.PointToClient(new Point(screenX, screenY));
				CommandBaseRenderer commandBaseRenderer = _galleryRenderer.GetItemRendererAt(point.X, point.Y);
				if (commandBaseRenderer != null)
				{
					return commandBaseRenderer.AccessibilityObject;
				}
			}
			return this;
		}
	}
}
