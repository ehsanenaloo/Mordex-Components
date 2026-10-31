using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane
{
	internal class ScrollBarAccessibleObject : Control.ControlAccessibleObject
	{
		private ScrollBarPartAccessibleObject[] _parts;

		public override AccessibleRole Role => AccessibleRole.ScrollBar;

		public override AccessibleObject Parent
		{
			get
			{
				if (base.Owner.Parent is MxTaskPane taskPane)
				{
					return taskPane.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override string Name
		{
			get
			{
				ControlStyleControl controlStyleControl = (ControlStyleControl)base.Owner;
				if (controlStyleControl.GetScrollBarType() == ScrollBarType.Horizontal)
				{
					return "Horizontal ScrollBar";
				}
				return "Vertical ScrollBar";
			}
		}

		public override string Value => string.Concat(((ControlStyleControl)base.Owner).GetValue());

		internal ScrollBarAccessibleObject(ControlStyleControl owner)
			: base(owner)
		{
			_parts = new ScrollBarPartAccessibleObject[5];
			_parts[0] = new ScrollBarPartAccessibleObject(owner, 0);
			_parts[1] = new ScrollBarPartAccessibleObject(owner, 1);
			_parts[2] = new ScrollBarPartAccessibleObject(owner, 2);
			_parts[3] = new ScrollBarPartAccessibleObject(owner, 3);
			_parts[4] = new ScrollBarPartAccessibleObject(owner, 4);
		}

		public override int GetChildCount()
		{
			return _parts.Length;
		}

		public override AccessibleObject GetChild(int index)
		{
			return _parts[index];
		}

		public override AccessibleObject HitTest(int integer, int integerArgument)
		{
			if (base.Owner != null)
			{
				Point clientPoint = base.Owner.PointToClient(new Point(integer, integerArgument));
				ScrollBarPartAccessibleObject[] parts = _parts;
				foreach (ScrollBarPartAccessibleObject part in parts)
				{
					if (part.Bounds.Contains(clientPoint))
					{
						return part;
					}
				}
			}
			return base.HitTest(integer, integerArgument);
		}
	}
}
