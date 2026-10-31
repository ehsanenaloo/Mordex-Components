using System.Collections;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public class ControlCollection : CollectionBase
	{
		private RibbonContextualTab _contextualTab;

		public Control this[int index] => (Control)base.List[index];

		internal ControlCollection(RibbonContextualTab ribbonTab)
		{
			_contextualTab = ribbonTab;
		}

		public void Add(Control control)
		{
			base.List.Add(control);
		}

		public void Remove(Control control)
		{
			if (Contains(control))
			{
				base.List.Remove(control);
			}
		}

		public bool Contains(Control control)
		{
			return base.List.Contains(control);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			Control control = (Control)value;
			_contextualTab.HookControlEvents(control);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			Control control = (Control)value;
			_contextualTab.UnhookControlEvents(control);
		}
	}
}
