using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class GridMaskEdit : MaskEdit
	{
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				if (!base.Multiline && base.TextAlign == HorizontalAlignment.Left)
				{
					if ((createParams.Style & 0x40) != 0)
					{
						createParams.Style ^= 64;
					}
					createParams.Style |= 132;
				}
				return createParams;
			}
		}

		internal GridMaskEdit()
			: base(checkLicense: false)
		{
		}

		protected override bool IsInputKey(Keys keyData)
		{
			Control control = GridHelper.GetParentSafe(this);
			if ((control == null || !(control is ControlStyleContainerControl)) && (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Return))
			{
				return false;
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyPress(KeyPressEventArgs eventArgs)
		{
			if (!Multiline && (eventArgs.KeyChar == '\n' || eventArgs.KeyChar == '\r'))
			{
				eventArgs.Handled = true;
			}
			base.OnKeyPress(eventArgs);
		}
	}
}
