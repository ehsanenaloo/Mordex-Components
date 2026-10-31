using System.Collections.Generic;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelCaptionContainerAccessibleObject : Control.ControlAccessibleObject
	{
		private DockPanelCaptionContainer _captionContainer;

		public override string Name
		{
			get
			{
				string accessibleName = _captionContainer.AccessibleName;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				accessibleName = _captionContainer.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _captionContainer.Name;
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
				if (_captionContainer.AccessibleRole != AccessibleRole.Default)
				{
					return _captionContainer.AccessibleRole;
				}
				return AccessibleRole.Pane;
			}
		}

		public override AccessibleStates State => AccessibleStates.None;

		public DockPanelCaptionContainerAccessibleObject(DockPanelCaptionContainer captionContainer)
			: base(captionContainer)
		{
			_captionContainer = captionContainer;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_captionContainer.Panel != null && _captionContainer.Panel._captionRenderer != null)
			{
				childCount++;
			}
			return childCount + _captionContainer.Controls.Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			IList<AccessibleObject> list = new List<AccessibleObject>();
			if (_captionContainer.Panel != null && _captionContainer.Panel._captionRenderer != null)
			{
				list.Add(_captionContainer.Panel._captionRenderer.GetAccessibleObject());
			}
			foreach (Control control in _captionContainer.Controls)
			{
				list.Add(control.AccessibilityObject);
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
