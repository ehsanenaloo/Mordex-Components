namespace Mordex.Windows.Ribbon
{
	public class LabelCommand : ButtonCommand
	{
		internal override CommandType CommandType => CommandType.Label;

		public LabelCommand()
		{
		}

		public LabelCommand(string key)
			: base(key)
		{
		}

		public LabelCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public LabelCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
		}

		internal override CommandSizeStyle GetSizeStyle()
		{
			if (SizeStyle == CommandSizeStyle.Default)
			{
				return CommandSizeStyle.Small;
			}
			return SizeStyle;
		}
	}
}
