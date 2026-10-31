using System.ComponentModel;

namespace Mordex.Windows.Ribbon
{
	public class MdiListCommand : DropDownCommand
	{
		private bool _showFormIcon;

		internal override CommandType CommandType => base.CommandType;

		[DefaultValue(false)]
		public bool ShowFormIcon
		{
			get
			{
				return _showFormIcon;
			}
			set
			{
				if (_showFormIcon != value)
				{
					_showFormIcon = value;
				}
			}
		}

		public MdiListCommand()
		{
		}

		public MdiListCommand(string key)
			: base(key)
		{
		}

		public MdiListCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public MdiListCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}
	}
}
