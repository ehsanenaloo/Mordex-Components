
namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabContentRenderer
	{
		public bool TryMoveDownIntoGroups()
		{
			if (_groupRenderers != null)
			{
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					if (((IKeyboardNavigable)item).MoveDown((CommandBaseRenderer)null))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool TrySelectLastItem()
		{
			if (_groupRenderers != null)
			{
				for (int groupIndex = _groupRenderers.Count - 1; groupIndex >= 0; groupIndex--)
				{
					RibbonGroupBoxRenderer thisValue = _groupRenderers[groupIndex];
					if (thisValue._dialogLauncher != null)
					{
						_ribbon.SetKeyboardFocusRenderer(thisValue._dialogLauncher);
						return true;
					}
					if (((IKeyboardNavigable)thisValue).SelectLastItem())
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool TryMovePrevious(CommandBaseRenderer selectedItem)
		{
			if (selectedItem != null)
			{
				if (selectedItem is RibbonGroupDialogLauncherRenderer)
				{
					RibbonGroupBoxRenderer ribbonRenderer = (RibbonGroupBoxRenderer)selectedItem._layoutItemOwner;
					if (((IKeyboardNavigable)ribbonRenderer).SelectLastItem())
					{
						return true;
					}
					int launcherIndex = _groupRenderers.IndexOf(ribbonRenderer);
					for (int previousIndex = launcherIndex - 1; previousIndex >= 0; previousIndex--)
					{
						RibbonGroupBoxRenderer thisValue = _groupRenderers[previousIndex];
						if (thisValue._dialogLauncher != null)
						{
							_ribbon.SetKeyboardFocusRenderer(thisValue._dialogLauncher);
							return true;
						}
						if (((IKeyboardNavigable)thisValue).MovePrevious((CommandBaseRenderer)null))
						{
							return true;
						}
					}
				}
				else if (selectedItem._layoutItemOwner is IKeyboardNavigable)
				{
					if (((IKeyboardNavigable)selectedItem._layoutItemOwner).MovePrevious(selectedItem))
					{
						return true;
					}
					RibbonGroupBoxRenderer item = FindOwnerGroupRenderer(selectedItem);
					int ownerIndex = _groupRenderers.IndexOf(item);
					for (int previousOwnerIndex = ownerIndex - 1; previousOwnerIndex >= 0; previousOwnerIndex--)
					{
						RibbonGroupBoxRenderer previousOwnerGroup = _groupRenderers[previousOwnerIndex];
						if (((IKeyboardNavigable)previousOwnerGroup).MovePrevious((CommandBaseRenderer)null))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool TryMoveNext(CommandBaseRenderer selectedItem)
		{
			if (selectedItem != null)
			{
				if (selectedItem is RibbonGroupDialogLauncherRenderer)
				{
					RibbonGroupBoxRenderer item = (RibbonGroupBoxRenderer)selectedItem._layoutItemOwner;
					int launcherIndex = _groupRenderers.IndexOf(item);
					for (int i = launcherIndex + 1; i < _groupRenderers.Count; i++)
					{
						RibbonGroupBoxRenderer thisValue = _groupRenderers[i];
						if (((IKeyboardNavigable)thisValue).MoveNext((CommandBaseRenderer)null))
						{
							return true;
						}
					}
				}
				else if (selectedItem._layoutItemOwner is IKeyboardNavigable)
				{
					if (((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveNext(selectedItem))
					{
						return true;
					}
					RibbonGroupBoxRenderer ribbonRenderer = FindOwnerGroupRenderer(selectedItem);
					if (ribbonRenderer == null)
					{
						return false;
					}
					if (ribbonRenderer._dialogLauncher != null)
					{
						_ribbon.SetKeyboardFocusRenderer(ribbonRenderer._dialogLauncher);
						return true;
					}
					int ownerIndex = _groupRenderers.IndexOf(ribbonRenderer);
					for (int j = ownerIndex + 1; j < _groupRenderers.Count; j++)
					{
						RibbonGroupBoxRenderer nextOwnerGroup = _groupRenderers[j];
						if (((IKeyboardNavigable)nextOwnerGroup).MoveNext((CommandBaseRenderer)null))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public void MoveRightFromItem(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return;
			}
			if (selectedItem is RibbonGroupDialogLauncherRenderer)
			{
				RibbonGroupBoxRenderer item = (RibbonGroupBoxRenderer)selectedItem._layoutItemOwner;
				int launcherIndex = _groupRenderers.IndexOf(item);
				for (int i = launcherIndex + 1; i < _groupRenderers.Count; i++)
				{
					RibbonGroupBoxRenderer thisValue = _groupRenderers[i];
					if (thisValue._dialogLauncher != null)
					{
						_ribbon.SetKeyboardFocusRenderer(thisValue._dialogLauncher);
						return;
					}
				}
				for (int j = 0; j < _groupRenderers.Count; j++)
				{
					RibbonGroupBoxRenderer firstFocusableGroup = _groupRenderers[j];
					if (firstFocusableGroup._dialogLauncher != null)
					{
						_ribbon.SetKeyboardFocusRenderer(firstFocusableGroup._dialogLauncher);
						break;
					}
				}
			}
			else
			{
				if (!(selectedItem._layoutItemOwner is IKeyboardNavigable) || ((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveRight(selectedItem))
				{
					return;
				}
				RibbonGroupBoxRenderer ownerGroup = FindOwnerGroupRenderer(selectedItem);
				int ownerIndex = _groupRenderers.IndexOf(ownerGroup);
				for (int k = ownerIndex + 1; k < _groupRenderers.Count; k++)
				{
					RibbonGroupBoxRenderer nextOwnerGroup = _groupRenderers[k];
					if (((IKeyboardNavigable)nextOwnerGroup).MoveRight((CommandBaseRenderer)null))
					{
						return;
					}
				}
				for (int l = 0; l < _groupRenderers.Count; l++)
				{
					RibbonGroupBoxRenderer wrapOwnerGroup = _groupRenderers[l];
					if (((IKeyboardNavigable)wrapOwnerGroup).MoveRight((CommandBaseRenderer)null))
					{
						break;
					}
				}
			}
		}

		public void MoveDownFromItem(CommandBaseRenderer selectedItem)
		{
			if (selectedItem is RibbonTabRenderer)
			{
				selectedItem = null;
			}
			if (_groupRenderers == null)
			{
				return;
			}
			foreach (RibbonGroupBoxRenderer item in _groupRenderers)
			{
				if (((IKeyboardNavigable)item).MoveDown(selectedItem))
				{
					break;
				}
			}
		}

		public bool TryMoveUpIntoGroups(CommandBaseRenderer selectedItem)
		{
			if (selectedItem is RibbonTabRenderer)
			{
				selectedItem = null;
			}
			if (_groupRenderers != null)
			{
				foreach (RibbonGroupBoxRenderer item in _groupRenderers)
				{
					if (((IKeyboardNavigable)item).MoveUp(selectedItem))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
