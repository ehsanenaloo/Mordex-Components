namespace Mordex.Windows.DataGrid.Internal
{
	internal interface IDragHost
	{
		void AddListener(IRefreshListener listener);

		void RemoveListener(IRefreshListener listener);

		void StartDrag(GridGroupLayoutState item, int offsetX, int offsetY);
	}
}
