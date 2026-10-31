using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	internal class AutoHideStripAccessibleObject : Control.ControlAccessibleObject
	{
		private MxAutoHideStrip _autoHideStrip;

		public override string Name
		{
			get
			{
				string accessibleName = _autoHideStrip.AccessibleName;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				accessibleName = _autoHideStrip.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _autoHideStrip.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_autoHideStrip.AccessibleRole != AccessibleRole.Default)
				{
					return _autoHideStrip.AccessibleRole;
				}
				return AccessibleRole.ToolBar;
			}
		}

		public override AccessibleStates State => AccessibleStates.None;

		public AutoHideStripAccessibleObject(MxAutoHideStrip autoHideStrip)
			: base(autoHideStrip)
		{
			_autoHideStrip = autoHideStrip;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_autoHideStrip != null)
			{
				return _autoHideStrip.GetTabAccessibleObject(index);
			}
			return null;
		}

		public override int GetChildCount()
		{
			if (_autoHideStrip != null)
			{
				return _autoHideStrip.GetTabCount();
			}
			return 0;
		}
	}
}
