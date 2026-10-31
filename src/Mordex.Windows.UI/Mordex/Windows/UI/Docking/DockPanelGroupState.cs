using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal abstract class DockPanelGroupState
	{
		internal MxDockPanelGroup _panelGroup;

		private DockPanelBase _selectedPanel;

		private ArrayList _visiblePanels;

		internal virtual bool IsTabLayout => true;

		internal virtual Rectangle ContentBounds
		{
			get
			{
				if (_panelGroup != null)
				{
					return _panelGroup.GetInnerAreaBounds();
				}
				return Rectangle.Empty;
			}
		}

		internal abstract Size MinimumSize { get; }

		internal abstract Size MaximumSize { get; }

		internal virtual bool HasBorderPanel => false;

		internal virtual bool HasCaption => false;

		internal DockPanelGroupState(MxDockPanelGroup panelGroup)
		{
			_panelGroup = panelGroup;
			_visiblePanels = new ArrayList();
		}

		internal virtual Rectangle GetNewTabBounds()
		{
			return Rectangle.Empty;
		}

		internal ArrayList GetVisiblePanels()
		{
			return _visiblePanels;
		}

		internal void RefreshVisiblePanels()
		{
			UnhookPanelEvents();
			_visiblePanels = new ArrayList();
			foreach (DockPanelBase item in (IEnumerable)_panelGroup.Panels)
			{
				if (!item.Closed && !item.AutoHide)
				{
					_visiblePanels.Add(item);
				}
			}
			HookPanelEvents();
		}

		internal abstract void RefreshGroupLayout();

		internal virtual BorderEdges GetChildPanelEdges(DockPanelBase childPanel)
		{
			if ((childPanel != null && childPanel.GetAutoHidePanel() != null) || (_panelGroup != null && !_panelGroup.HasPanelBorder()))
			{
				return BorderEdges.All;
			}
			return BorderEdges.None;
		}

		internal virtual void Dispose()
		{
			_panelGroup = null;
			UnhookPanelEvents();
		}

		internal virtual bool ProcessDoubleClick()
		{
			return true;
		}

		internal virtual bool ProcessMouseDown(MouseEventArgs eventArgs)
		{
			return true;
		}

		internal virtual bool ProcessMouseMove(MouseEventArgs eventArgs)
		{
			return true;
		}

		internal virtual bool ProcessMouseUp(MouseEventArgs eventArgs)
		{
			return true;
		}

		internal virtual Cursor GetCursor(Point pt)
		{
			return null;
		}

		internal virtual void ProcessPaint(PaintEventArgs eventArgs)
		{
		}

		internal DockPanelBase GetSelectedPanel()
		{
			return _selectedPanel;
		}

		internal void SetSelectedPanel(DockPanelBase panel)
		{
			if (_selectedPanel != panel && IsSelectable(panel) && (CanChangeSelectedPanel(panel, giveFocus: true) || _selectedPanel == null || !_visiblePanels.Contains(_selectedPanel)))
			{
				AssignSelectedPanel(panel);
				OnSelectedPanelChanged(activate: true);
			}
		}

		internal void SelectPanel(DockPanelBase panel, bool giveFocus)
		{
			if (IsSelectable(panel) && (CanChangeSelectedPanel(panel, giveFocus) || _selectedPanel == null || !_visiblePanels.Contains(_selectedPanel)))
			{
				AssignSelectedPanel(panel);
				OnSelectedPanelChanged(giveFocus);
			}
		}

		private bool IsSelectable(DockPanelBase panel)
		{
			return panel == null || (!panel.Closed && panel.ParentGroup == _panelGroup);
		}

		private void AssignSelectedPanel(DockPanelBase panel)
		{
			if (_selectedPanel is MxDockPanel && IsTabLayout)
			{
				((MxDockPanel)_selectedPanel).InnerContainer.SetSelectable(false);
			}
			_selectedPanel = panel;
			if (_selectedPanel is MxDockPanel && IsTabLayout)
			{
				((MxDockPanel)_selectedPanel).InnerContainer.SetSelectable(true);
			}
		}

		internal virtual bool CanChangeSelectedPanel(DockPanelBase panel, bool giveFocus)
		{
			return _panelGroup.CanSelectPanel(panel, giveFocus);
		}

		internal virtual bool ProcessCloseButtonClick(DockPanelBase panel)
		{
			return false;
		}

		internal virtual bool ProcessCaptionButtonDown(DockPanelBase panel, Rectangle buttonBounds)
		{
			return false;
		}

		internal virtual bool ProcessAutoHideButtonClick(DockPanelBase panel)
		{
			return false;
		}

		internal virtual bool ProcessCaptionDoubleClick(DockPanelBase panel)
		{
			if (_panelGroup.IsFloating())
			{
				_panelGroup.DockPanel();
			}
			else
			{
				_panelGroup.UndockPanel();
			}
			return true;
		}

		internal virtual void OnSelectedPanelChanged(bool activate)
		{
			if (_selectedPanel != null && !_selectedPanel.IsAutoHideEffective() && activate)
			{
				_selectedPanel.Activate();
			}
			_panelGroup.RaiseSelectedPanelChanged();
		}

		internal virtual bool ShouldDragGroup()
		{
			return false;
		}

		internal virtual PanelDropInfo GetDropInfo(DockPanelBase panel, Point screenPoint)
		{
			return null;
		}

		internal virtual void ProcessChildDropInfo(PanelDropInfo dropInfo)
		{
		}

		internal virtual bool TreatChildPanelTextAsButton()
		{
			return false;
		}

		internal virtual bool PaintChildCaptionBar(Graphics graphics, DockCaptionRenderer captionBar)
		{
			return false;
		}

		internal void HookPanelEvents()
		{
			if (_visiblePanels == null)
			{
				return;
			}
			foreach (DockPanelBase item in _visiblePanels)
			{
				item.Resize += OnVisiblePanelResized;
				item.Move += OnVisiblePanelMoved;
			}
		}

		internal void UnhookPanelEvents()
		{
			if (_visiblePanels == null)
			{
				return;
			}
			foreach (DockPanelBase item in _visiblePanels)
			{
				item.Resize -= OnVisiblePanelResized;
				item.Move -= OnVisiblePanelMoved;
			}
		}

		private void OnVisiblePanelResized(object sender, EventArgs eventArgs)
		{
			if (_visiblePanels.Contains(sender) && ((DockPanelBase)sender).GetAutoHidePanel() == null)
			{
				OnPanelResize((DockPanelBase)sender);
			}
		}

		private void OnVisiblePanelMoved(object sender, EventArgs eventArgs)
		{
			if (_visiblePanels.Contains(sender) && ((DockPanelBase)sender).GetAutoHidePanel() == null)
			{
				RefreshGroupLayout();
			}
		}

		internal virtual void OnPanelResize(DockPanelBase panel)
		{
			RefreshGroupLayout();
		}

		internal virtual int GetAccessibleChildCount()
		{
			if (_visiblePanels != null)
			{
				return GetVisiblePanels().Count;
			}
			return 0;
		}

		internal virtual AccessibleObject GetAccessibleChild(int index)
		{
			List<AccessibleObject> list = new List<AccessibleObject>();
			if (GetVisiblePanels() != null)
			{
				foreach (DockPanelBase item in _panelGroup._groupState.GetVisiblePanels())
				{
					list.Add(item.AccessibilityObject);
				}
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
