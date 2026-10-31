using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class CustomCellEditor : CellEditor
	{
		private Control _editTextBox;

		public override EditType EditType => EditType.Custom;

		public override TextBox EditTextBox
		{
			get
			{
				if (_editTextBox is TextBox)
				{
					return (TextBox)_editTextBox;
				}
				return null;
			}
		}

		internal CustomCellEditor(GridColumnContainerControl editContainer, Control customControl)
			: base(editContainer)
		{
			_editTextBox = customControl;
		}

		protected override Control CreateEditControl()
		{
			if (_editTextBox != null)
			{
				_editTextBox.Visible = true;
			}
			return _editTextBox;
		}

		public override void Destroy()
		{
			if (_editTextBox != null && _editTextBox.Parent == GetEditContainer())
			{
				_editTextBox.Parent = null;
				_editTextBox.Visible = false;
			}
		}
	}
}
