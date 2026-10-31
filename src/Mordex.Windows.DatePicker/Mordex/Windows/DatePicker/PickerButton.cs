using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	internal class PickerButton
	{
		private int _arrowDirection;

		private Control _owner;

		public Rectangle _bounds;

		private bool _isPressed;

		private bool _isHot;

		internal int _arrowHeight;

		public PickerButton(Control owner, int arrowDirection)
		{
			if (owner == null)
			{
				throw new ArgumentNullException();
			}
			_owner = owner;
			_arrowDirection = arrowDirection;
		}

		private void InvalidateOwner()
		{
			if (_owner != null && _bounds.Width > 0)
			{
				_owner.Invalidate();
			}
		}

		public bool GetIsPressed()
		{
			return _isPressed;
		}

		public void SetIsPressed(bool isPressed)
		{
			if (_isPressed != isPressed)
			{
				_isPressed = isPressed;
				InvalidateOwner();
			}
		}

		public bool GetIsHot()
		{
			return _isHot;
		}

		public void SetIsHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				InvalidateOwner();
			}
		}

		public bool ContainsPoint(int x, int y)
		{
			if (_bounds.Width == 0)
			{
				return false;
			}
			return _bounds.Contains(x, y);
		}
	}
}
