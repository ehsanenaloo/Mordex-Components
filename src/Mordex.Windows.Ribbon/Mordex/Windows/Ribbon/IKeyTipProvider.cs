namespace Mordex.Windows.Ribbon
{
	internal interface IKeyTipProvider
	{
		bool HasKeyTip { get; }

		string KeyTip { get; }
	}
}
