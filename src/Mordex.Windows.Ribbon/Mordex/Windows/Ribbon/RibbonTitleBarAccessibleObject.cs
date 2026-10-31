using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonTitleBarAccessibleObject : AccessibleObject
	{
		private RibbonWindowMessageHandler _titleBar;

		public override AccessibleObject Parent
		{
			get
			{
				if (_titleBar != null)
				{
					if (_titleBar.GetRibbon() != null)
					{
						return _titleBar.GetRibbon().AccessibilityObject;
					}
					if (_titleBar.GetForm() != null)
					{
						return _titleBar.GetForm().AccessibilityObject;
					}
				}
				return base.Parent;
			}
		}

		public override string Name => "Title Bar";

		public override AccessibleRole Role => AccessibleRole.TitleBar;

		public override AccessibleStates State => AccessibleStates.None;

		public override string Value
		{
			get
			{
				string value = base.Value;
				if (value == null || value.Length == 0)
				{
					return _titleBar.DocumentText + _titleBar.ApplicationText;
				}
				return value;
			}
			set
			{
				base.Value = value;
			}
		}

		public RibbonTitleBarAccessibleObject(RibbonWindowMessageHandler titleBar)
		{
			_titleBar = titleBar;
		}
	}
}
