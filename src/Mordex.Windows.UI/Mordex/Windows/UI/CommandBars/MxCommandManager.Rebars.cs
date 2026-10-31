using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		private bool RemoveRebarsFromContainer()
		{
			bool result = false;
			if (_containerControl.Controls.Contains(TopRebar))
			{
				_containerControl.Controls.Remove(TopRebar);
				result = true;
			}
			if (_containerControl.Controls.Contains(LeftRebar))
			{
				_containerControl.Controls.Remove(LeftRebar);
				result = true;
			}
			if (_containerControl.Controls.Contains(RightRebar))
			{
				_containerControl.Controls.Remove(RightRebar);
				result = true;
			}
			if (_containerControl.Controls.Contains(BottomRebar))
			{
				_containerControl.Controls.Remove(BottomRebar);
				result = true;
			}
			return result;
		}

		internal void AddRebarsToContainer()
		{
			if (!_isInitializing && _containerControl != null)
			{
				ShowRebar(TopRebar);
				ShowRebar(LeftRebar);
				ShowRebar(RightRebar);
				ShowRebar(BottomRebar);
			}
		}

		internal void RefreshRebars()
		{
			if (_leftRebar != null && RenderingHelper.GetParentControl(_leftRebar) != null)
			{
				_leftRebar.InvalidateBarLayouts();
			}
			if (_topRebar != null && RenderingHelper.GetParentControl(_topRebar) != null)
			{
				_topRebar.InvalidateBarLayouts();
			}
			if (_bottomRebar != null && RenderingHelper.GetParentControl(_bottomRebar) != null)
			{
				_bottomRebar.InvalidateBarLayouts();
			}
			if (_rightRebar != null && RenderingHelper.GetParentControl(_rightRebar) != null)
			{
				_rightRebar.InvalidateBarLayouts();
			}
			if (_editorDialogs == null)
			{
				return;
			}
			foreach (CommandBarEditorDialog item in _editorDialogs)
			{
				item.InvalidateLayout();
			}
		}

		private void CollapseRebarRows()
		{
			if (_topRebar != null)
			{
				while (_topRebar.HasRowNeedingAdjust())
				{
				}
			}
			if (_leftRebar != null)
			{
				while (_leftRebar.HasRowNeedingAdjust())
				{
				}
			}
			if (_bottomRebar != null)
			{
				while (_bottomRebar.HasRowNeedingAdjust())
				{
				}
			}
			if (_rightRebar != null)
			{
				while (_rightRebar.HasRowNeedingAdjust())
				{
				}
			}
		}

		private void RefreshRebarRows()
		{
			if (_topRebar != null && _topRebar.HasAddedBar())
			{
				while (_topRebar.AdjustOverflowingRows())
				{
				}
			}
			if (_leftRebar != null && _leftRebar.HasAddedBar())
			{
				while (_leftRebar.AdjustOverflowingRows())
				{
				}
			}
			if (_bottomRebar != null && _bottomRebar.HasAddedBar())
			{
				while (_bottomRebar.AdjustOverflowingRows())
				{
				}
			}
			if (_rightRebar != null && _rightRebar.HasAddedBar())
			{
				while (_rightRebar.AdjustOverflowingRows())
				{
				}
			}
		}

		internal void ResetActiveControl(bool ignoreCapture)
		{
			if (GetForm() == null)
			{
				return;
			}
			Control control = GetForm().ActiveControl;
			if (control == null || (!control.Capture && !ignoreCapture))
			{
				SetActiveCommandControl(null);
				return;
			}
			if (_topRebar != null && _topRebar.ContainsFocus)
			{
				control = _topRebar;
			}
			if (_leftRebar != null && _leftRebar.ContainsFocus)
			{
				control = _leftRebar;
			}
			if (_bottomRebar != null && _bottomRebar.ContainsFocus)
			{
				control = _bottomRebar;
			}
			if (_rightRebar != null && _rightRebar.ContainsFocus)
			{
				control = _rightRebar;
			}
			if (control != null && RenderingHelper.FindForm(control) != null)
			{
				if (ContainerControl is ContainerControl)
				{
					((ContainerControl)ContainerControl).ActiveControl = null;
				}
				else
				{
					GetForm().ActiveControl = null;
				}
			}
			SetActiveCommandControl(null);
		}

		private void HideEmptyRebars()
		{
			if (TopRebar.GetRows().Count == 0)
			{
				HideRebar(TopRebar);
			}
			else
			{
				TopRebar.RefreshRowsAndBars();
			}
			if (BottomRebar.GetRows().Count == 0)
			{
				HideRebar(BottomRebar);
			}
			else
			{
				BottomRebar.RefreshRowsAndBars();
			}
			if (LeftRebar.GetRows().Count == 0)
			{
				HideRebar(LeftRebar);
			}
			else
			{
				LeftRebar.RefreshRowsAndBars();
			}
			if (RightRebar.GetRows().Count == 0)
			{
				HideRebar(RightRebar);
			}
			else
			{
				RightRebar.RefreshRowsAndBars();
			}
		}

		private void AddRebarToDesignContainer(MxCommandBarHost uiRebar)
		{
			switch (uiRebar.GetBarsDockStyle())
			{
			case BarsDockStyle.Top:
				AddToDesignContainer(uiRebar, "TopRebar");
				break;
			case BarsDockStyle.Left:
				AddToDesignContainer(uiRebar, "LeftRebar");
				break;
			case BarsDockStyle.Right:
				AddToDesignContainer(uiRebar, "RightRebar");
				break;
			case BarsDockStyle.Bottom:
				AddToDesignContainer(uiRebar, "BottomRebar");
				break;
			}
		}

		private void AddToDesignContainer(MxCommandBarHost component, string text)
		{
			if (base.Container == null || !base.DesignMode)
			{
				return;
			}
			int suffix = 1;
			string name = text + suffix;
			INameCreationService nameCreationService = (INameCreationService)Site.GetService(typeof(INameCreationService));
			if (nameCreationService != null)
			{
				try
				{
					while (!nameCreationService.IsValidName(name))
					{
						suffix++;
						name = text + suffix;
					}
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// The designer name service belongs to the host; if it fails, the container picks the name.
					base.Container.Add(component);
					return;
				}
				try
				{
					base.Container.Add(component, name);
					return;
				}
				catch (ArgumentException)
				{
					// The name is taken; let the container pick one.
					base.Container.Add(component);
					return;
				}
			}
			base.Container.Add(component);
		}

		internal void ShowRebar(MxCommandBarHost control)
		{
			control.SetHasAddedBar(true);
			if (base.DesignMode && base.Container != null && control.Container == null)
			{
				AddRebarToDesignContainer(control);
			}
			if (!_isInitializing && _containerControl != null)
			{
				if (!_containerControl.Controls.Contains(control))
				{
					_containerControl.Controls.Add(control);
				}
				UpdateRebarZOrder(control);
			}
		}

		internal void OrderContainerControls()
		{
			if (_containerControl == null)
			{
				return;
			}
			_containerControl.SuspendLayout();
			int lastBarIndex = -1;
			int lastHorizontalBarIndex = -1;
			int statusBarIndex = -1;
			Control control = null;
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < _containerControl.Controls.Count; i++)
			{
				control = _containerControl.Controls[i];
				arrayList.Add(control);
				if (control is MxCommandBarHost)
				{
					if ((control.Dock == DockStyle.Top || control.Dock == DockStyle.Bottom) && i > lastHorizontalBarIndex)
					{
						lastHorizontalBarIndex = i;
					}
					if (i > lastBarIndex)
					{
						lastBarIndex = i;
					}
				}
				else if ((control is MxStatusBar) && i > lastBarIndex)
				{
					lastBarIndex = i;
					statusBarIndex = i;
				}
			}
			if (lastBarIndex >= 0)
			{
				for (int index = arrayList.Count - 1; index >= 0; index--)
				{
					control = (Control)arrayList[index];
					if (!(control is MxCommandBarHost) && !(control is MxStatusBar) && index > lastBarIndex)
					{
						_containerControl.Controls.SetChildIndex(control, lastBarIndex);
					}
					else if (lastHorizontalBarIndex > 0 && control is MxCommandBarHost)
					{
						if (control.Dock == DockStyle.Right || control.Dock == DockStyle.Left)
						{
							if (index > lastHorizontalBarIndex)
							{
								_containerControl.Controls.SetChildIndex(control, lastHorizontalBarIndex);
							}
						}
						else if (control.Dock == DockStyle.Bottom && statusBarIndex >= 0 && index > statusBarIndex)
						{
							ContainerControl.Controls.SetChildIndex(control, statusBarIndex);
						}
					}
				}
			}
			_containerControl.ResumeLayout();
		}

		private void UpdateRebarZOrder(MxCommandBarHost child)
		{
			int childIndex = _containerControl.Controls.GetChildIndex(child);
			int targetIndex = childIndex;
			if (child._isHorizontal)
			{
				if (_leftRebar != null && _leftRebar.HasAddedBar())
				{
					int leftIndex = _containerControl.Controls.GetChildIndex(_leftRebar, throwException: false);
					if (targetIndex < leftIndex)
					{
						targetIndex = leftIndex;
					}
				}
				if (_rightRebar != null && _rightRebar.HasAddedBar())
				{
					int rightIndex = _containerControl.Controls.GetChildIndex(_rightRebar, throwException: false);
					if (targetIndex < rightIndex)
					{
						targetIndex = rightIndex;
					}
				}
			}
			else
			{
				if (_topRebar != null && _topRebar.HasAddedBar())
				{
					int topIndex = _containerControl.Controls.GetChildIndex(_topRebar, throwException: false);
					if (targetIndex > topIndex)
					{
						targetIndex = topIndex;
					}
				}
				if (_bottomRebar != null && _bottomRebar.HasAddedBar())
				{
					int bottomIndex = _containerControl.Controls.GetChildIndex(_bottomRebar, throwException: false);
					if (targetIndex > bottomIndex)
					{
						targetIndex = bottomIndex;
					}
				}
			}
			if (targetIndex != childIndex)
			{
				_containerControl.Controls.SetChildIndex(child, targetIndex);
			}
		}

		internal void HideRebar(MxCommandBarHost control)
		{
			if (!_isBatchUpdating)
			{
				if (_containerControl != null && _containerControl.Controls.Contains(control))
				{
					_containerControl.Controls.Remove(control);
				}
				if (!control.IsDisposed)
				{
					control.SetHasAddedBar(false);
				}
			}
		}
	}
}
