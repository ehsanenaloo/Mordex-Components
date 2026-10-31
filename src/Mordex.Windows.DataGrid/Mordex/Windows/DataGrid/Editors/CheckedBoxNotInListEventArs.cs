using System;

namespace Mordex.Windows.DataGrid.Editors
{
	public class CheckedBoxNotInListEventArs : EventArgs
	{
		private readonly string _notInListText;

		private NotInListAction _action;

		public string Text => _notInListText;

		public NotInListAction Action
		{
			get
			{
				return _action;
			}
			set
			{
				_action = value;
			}
		}

		internal CheckedBoxNotInListEventArs(string text)
		{
			_notInListText = text;
			_action = NotInListAction.Cancel;
		}
	}
}
