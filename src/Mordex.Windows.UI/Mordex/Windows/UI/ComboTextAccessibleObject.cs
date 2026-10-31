using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class ComboTextAccessibleObject : AccessibleObject
	{
		private MxComboBase _comboBox;

		public override AccessibleObject Parent => _comboBox.AccessibilityObject;

		public override AccessibleRole Role => AccessibleRole.StaticText;

		public override string Name
		{
			get
			{
				if (_comboBox.Text != null)
				{
					return _comboBox.Text;
				}
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				if (_comboBox != null)
				{
					Rectangle field_ = _comboBox._textBounds;
					return new Rectangle(_comboBox.PointToScreen(field_.Location), field_.Size);
				}
				return base.Bounds;
			}
		}

		public ComboTextAccessibleObject(MxComboBase comboBox)
		{
			_comboBox = comboBox;
		}
	}
}
