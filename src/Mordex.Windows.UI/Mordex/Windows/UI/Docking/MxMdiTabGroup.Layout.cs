using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxMdiTabGroup
	{
		public MdiChildTab GetChildTabAt(Point pt)
		{
			return GetChildTabAt(pt.X, pt.Y);
		}

		public MdiChildTab GetChildTabAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetChildTabAt(point.X, point.Y);
		}

		public MdiChildTab GetChildTabAt(int x, int y)
		{
			return HitTestTab(x, y)?.GetChildTab();
		}

		public Rectangle GetMdiTabItemBounds(MdiChildTab tab)
		{
			return GetMdiTabItemBounds(tab.Form);
		}

		public Rectangle GetMdiTabItemBounds(Form form)
		{
			if (_groupLayout != null)
			{
				MdiTabItem uiMdiChildTabLayoutState = _groupLayout.FindTabByForm(form);
				if (uiMdiChildTabLayoutState != null)
				{
					return _groupLayout.GetTabBounds(uiMdiChildTabLayoutState, true);
				}
			}
			return Rectangle.Empty;
		}

		public MdiTabHitTest HitTest()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return HitTest(position.X, position.Y);
		}

		public MdiTabHitTest HitTest(Point pt)
		{
			return HitTest(pt.X, pt.Y);
		}

		public MdiTabHitTest HitTest(int x, int y)
		{
			MdiTabItem uiMdiChildTabLayoutState = HitTestTab(x, y);
			if (uiMdiChildTabLayoutState != null)
			{
				switch (uiMdiChildTabLayoutState.GetButtonType())
				{
				case TabButtonType.ActiveFiles:
					return MdiTabHitTest.ActiveFiles;
				case TabButtonType.ChildForm:
					return MdiTabHitTest.ChildTab;
				case TabButtonType.Close:
					return MdiTabHitTest.CloseButton;
				case TabButtonType.ScrollLeft:
					return MdiTabHitTest.ScrollLeft;
				case TabButtonType.ScrollRight:
					return MdiTabHitTest.ScrollRight;
				}
			}
			return MdiTabHitTest.None;
		}

		internal MdiTabStripLayout GetGroupLayout()
		{
			return _groupLayout;
		}

		private int MeasureStripHeight()
		{
			if (_groupLayout != null)
			{
				using Graphics graphics = CreateGraphics();
				_stripHeight = _groupLayout.LayoutTabs(graphics, Font, 1, base.ClientRectangle, -1).Height + 1;
				return _stripHeight;
			}
			return DefaultStripHeight;
		}

		internal MdiTabGroupRenderer GetTabGroupHost()
		{
			return _tabGroupHost;
		}

		public bool ContainsForm(Form form)
		{
			foreach (MdiTabItem item in (IEnumerable)_groupLayout)
			{
				if (item.GetChildTab().Form == form)
				{
					return true;
				}
			}
			return false;
		}

		internal Rectangle GetGroupBounds()
		{
			return _groupBounds;
		}

		internal void SetGroupBounds(Rectangle rectangle)
		{
			_groupBounds = rectangle;
			UpdateBounds(true);
		}

		internal FormatStyle ResolveTabStyle(MdiChildTab uiMdiChildTab, ButtonState buttonState, bool isCloseButton)
		{
			FormatStyle uiFormatStyle = PanelManager.GetMdiTabStyle(buttonState, isCloseButton);
			if (isCloseButton)
			{
				return uiFormatStyle;
			}
			uiFormatStyle.MergeStyle(PanelManager.GetMdiTabStateStyle(ButtonState.Normal));
			if (uiMdiChildTab != null)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiMdiChildTab.StateStyles.FormatStyle);
			}
			uiFormatStyle = uiFormatStyle.MergeStyle(PanelManager.GetMdiTabStateStyle(buttonState));
			if (uiMdiChildTab != null)
			{
				FormatStyle tabStateStyle = null;
				switch (buttonState)
				{
				case ButtonState.Hot:
					tabStateStyle = new FormatStyle(uiMdiChildTab.StateStyles.HotFormatStyle);
					break;
				case ButtonState.Pressed:
					tabStateStyle = new FormatStyle(uiMdiChildTab.StateStyles.PressedFormatStyle);
					break;
				case ButtonState.Selected:
					tabStateStyle = new FormatStyle(uiMdiChildTab.StateStyles.SelectedFormatStyle);
					break;
				}
				if (tabStateStyle != null)
				{
					uiFormatStyle = uiFormatStyle.MergeStyle(tabStateStyle);
				}
			}
			return uiFormatStyle;
		}

		internal void RebuildLayout(bool remeasureStrip)
		{
			if (base.IsDisposed)
			{
				return;
			}
			int measuredHeight = 0;
			int scrollOffset = 0;
			if (_groupLayout != null)
			{
				if (PanelManager.TabbedMdiSettings.ScrollButtons != TabScrollButtons.UseContextMenu)
				{
					scrollOffset = _groupLayout.GetFirstVisibleIndex();
				}
				_groupLayout.DisposeResources();
			}
			_groupLayout = new MdiTabStripLayout(this);
			for (int i = 0; i < _childTabs.Count; i++)
			{
				MdiTabItem uiMdiChildTabLayoutState = new MdiTabItem(TabButtonType.ChildForm, (MdiChildTab)_childTabs[i], this, PanelManager.UseCompatibleTextRendering, PanelManager.TabbedMdiSettings.ShowCloseButtonOnSelectedTab);
				if (uiMdiChildTabLayoutState.GetChildTab().Form == GetSelectedForm())
				{
					uiMdiChildTabLayoutState.SetIsSelected(true);
				}
				_groupLayout.AddTab(uiMdiChildTabLayoutState);
				if (_pressedTabLayout != null && _pressedTabLayout.GetButtonType() == TabButtonType.ChildForm && uiMdiChildTabLayoutState.GetChildTab().Form == _pressedTabLayout.GetChildTab().Form)
				{
					_pressedTabLayout = uiMdiChildTabLayoutState;
				}
			}
			Graphics graphics = CreateGraphics();
			Size size = ((!remeasureStrip) ? _groupLayout.LayoutTabs(graphics, Font, 1, base.ClientRectangle, _stripHeight) : _groupLayout.LayoutTabs(graphics, Font, 1, base.ClientRectangle, -1));
			graphics.Dispose();
			_groupLayout.SetFirstVisibleIndex(scrollOffset);
			measuredHeight = size.Height;
			if (remeasureStrip && measuredHeight != _stripHeight)
			{
				_stripHeight = measuredHeight;
				UpdateBounds(true);
			}
			Invalidate();
			if (_tabGroupHost.GetMdiClient() != null)
			{
				_tabGroupHost.GetMdiClient().Invalidate();
			}
			foreach (MdiChildTab item in _childTabs)
			{
				item.FitFormToGroup();
			}
		}

		internal Form GetSelectedForm()
		{
			return _form;
		}

		internal void SelectForm(Form form)
		{
			if (_form != form)
			{
				_form = form;
				RebuildLayout(false);
				_tabGroupHost.LayoutGroups();
				if (_form != null)
				{
					_form.BringToFront();
				}
			}
			if (_groupLayout.EnsureTabVisible(form))
			{
				Invalidate();
			}
		}

		internal MdiChildTab AddFormTab(Form frm)
		{
			MdiChildTab uiMdiChildTab = new MdiChildTab(this, frm);
			PanelManager.RaiseMdiTabInitialize(uiMdiChildTab);
			if (_groupLayout._useActiveFilesMenu)
			{
				_childTabs.Insert(0, uiMdiChildTab);
			}
			else
			{
				_childTabs.Add(uiMdiChildTab);
			}
			RebuildLayout(true);
			PanelManager.RaiseMdiTabCreated(uiMdiChildTab);
			if (_form != null)
			{
				_form.BringToFront();
			}
			return uiMdiChildTab;
		}

		internal void AttachTab(MdiChildTab uiMdiChildTab)
		{
			uiMdiChildTab.SetTabGroup(this);
			if (_groupLayout._useActiveFilesMenu)
			{
				_childTabs.Insert(0, uiMdiChildTab);
			}
			else
			{
				_childTabs.Add(uiMdiChildTab);
			}
			RebuildLayout(true);
		}

		internal MdiChildTab RemoveFormTab(Form form, bool raiseClosedEvent)
		{
			MdiChildTab result = null;
			int removedIndex = -1;
			for (int i = 0; i < _childTabs.Count; i++)
			{
				MdiChildTab uiMdiChildTab = (MdiChildTab)_childTabs[i];
				if (uiMdiChildTab.Form == form)
				{
					result = uiMdiChildTab;
					if (raiseClosedEvent)
					{
						PanelManager.RaiseMdiTabClosed(uiMdiChildTab);
						uiMdiChildTab.RestoreForm();
					}
					else
					{
						uiMdiChildTab.SetTabGroup(null);
					}
					_childTabs.Remove(uiMdiChildTab);
					removedIndex = i;
					break;
				}
			}
			RebuildLayout(true);
			if (SelectedTab == null)
			{
				if (removedIndex > _childTabs.Count - 1)
				{
					removedIndex = _childTabs.Count - 1;
				}
				if (removedIndex >= 0)
				{
					SelectForm(((MdiChildTab)_childTabs[removedIndex]).Form);
					GetSelectedForm().BringToFront();
					_form.Refresh();
				}
			}
			if (_groupLayout.EnsureTabVisible(_form))
			{
				Invalidate();
			}
			return result;
		}

		private void UpdateBounds(bool remeasureStrip)
		{
			Rectangle bounds = _groupBounds;
			if (remeasureStrip)
			{
				MeasureStripHeight();
			}
			bounds.Height = _stripHeight;
			if (bounds.Height > _groupBounds.Height)
			{
				bounds.Height = _groupBounds.Height;
			}
			else
			{
				bounds.Height = _stripHeight;
			}
			Point p = bounds.Location;
			if (_tabGroupHost.GetMdiClient() != null)
			{
				p = _tabGroupHost.GetMdiClient().PointToScreen(p);
			}
			p = _tabGroupHost.GetForm().PointToClient(p);
			bounds.Location = p;
			base.Bounds = bounds;
			_formAreaBounds = _groupBounds;
			_formAreaBounds.Y += bounds.Height;
			_formAreaBounds.Height -= bounds.Height;
			_formAreaBounds = PanelManager.TabbedMdiSettings.WindowPadding.Deflate(_formAreaBounds);
			if (_formAreaBounds.Height < 0)
			{
				_formAreaBounds.Height = 0;
			}
			if (_formAreaBounds.Width < 0)
			{
				_formAreaBounds.Width = 0;
			}
			foreach (MdiChildTab item in _childTabs)
			{
				item.FitFormToGroup();
			}
		}

		protected override void OnResize(EventArgs e)
		{
			RebuildLayout(false);
			base.OnResize(e);
		}

		private MdiTabItem HitTestTab(int x, int y)
		{
			return _groupLayout.HitTest(_form, x, y);
		}

		internal void HandleTabButtonClick(MdiTabItem uiMdiChildTabLayoutState)
		{
			if (uiMdiChildTabLayoutState == null)
			{
				return;
			}
			switch (uiMdiChildTabLayoutState.GetButtonType())
			{
			case TabButtonType.ActiveFiles:
				if (_groupLayout != null)
				{
					_groupLayout.ShowActiveFilesMenu();
				}
				break;
			case TabButtonType.ChildForm:
				if (uiMdiChildTabLayoutState.GetChildTab() == null)
				{
					break;
				}
				if (uiMdiChildTabLayoutState.GetChildTab().Form != null)
				{
					uiMdiChildTabLayoutState.GetChildTab().Form.Activate();
					if (uiMdiChildTabLayoutState.GetChildTab().Form.ActiveControl != null)
					{
						uiMdiChildTabLayoutState.GetChildTab().Form.ActiveControl.Select();
					}
					else
					{
						uiMdiChildTabLayoutState.GetChildTab().Form.Select();
					}
				}
				else
				{
					_tabGroupHost.GetMdiClient().Select();
				}
				break;
			case TabButtonType.Close:
				if (GetSelectedForm() != null)
				{
					GetSelectedForm().Close();
				}
				if (_groupLayout != null && _groupLayout.GetCount() > 0 && _tabGroupHost != null && _tabGroupHost.GetForm() != null && !ContainsForm(_tabGroupHost.GetForm().ActiveMdiChild))
				{
					_groupLayout.GetTab(_groupLayout.GetCount() - 1).GetChildTab().Form.Activate();
					break;
				}
				break;
			case TabButtonType.ScrollLeft:
			{
				MdiTabStripLayout uiMdiTabGroupLayoutState = _groupLayout;
				uiMdiTabGroupLayoutState.SetFirstVisibleIndex(uiMdiTabGroupLayoutState.GetFirstVisibleIndex() - 1);
				break;
			}
			case TabButtonType.ScrollRight:
			{
				MdiTabStripLayout tabLayout = _groupLayout;
				tabLayout.SetFirstVisibleIndex(tabLayout.GetFirstVisibleIndex() + 1);
				break;
			}
			}
		}

		internal void DisposeToolTipResources()
		{
			if (_toolTipPopup != null)
			{
				_toolTipPopup.HideAndDisposeTimers();
				_toolTipPopup = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		internal void UpdateToolTip(MdiTabItem uiMdiChildTabLayoutState)
		{
			if (uiMdiChildTabLayoutState == null || uiMdiChildTabLayoutState._isDisabled)
			{
				SetToolTipText("", Rectangle.Empty);
				return;
			}
			if (uiMdiChildTabLayoutState.GetButtonType() == TabButtonType.Close)
			{
				SetToolTipText(PanelManager.BuiltInTexts[PanelsBuiltInText.CloseMdiToolTip], Rectangle.Empty);
				return;
			}
			if (uiMdiChildTabLayoutState.GetButtonType() == TabButtonType.ActiveFiles)
			{
				SetToolTipText(PanelManager.BuiltInTexts[PanelsBuiltInText.ActiveFilesToolTip], Rectangle.Empty);
				return;
			}
			if (uiMdiChildTabLayoutState.GetButtonType() == TabButtonType.ScrollLeft)
			{
				SetToolTipText(PanelManager.BuiltInTexts[PanelsBuiltInText.ScrollMdiLeftToolTip], Rectangle.Empty);
				return;
			}
			if (uiMdiChildTabLayoutState.GetButtonType() == TabButtonType.ScrollRight)
			{
				SetToolTipText(PanelManager.BuiltInTexts[PanelsBuiltInText.ScrollMdiRightToolTip], Rectangle.Empty);
				return;
			}
			Rectangle rectangle = _groupLayout.GetTabBounds(uiMdiChildTabLayoutState, false);
			SetToolTipText(uiMdiChildTabLayoutState.GetChildTab().ToolTipText, rectangle);
		}

		internal MdiTabItem GetPressedTabLayout()
		{
			return _pressedTabLayout;
		}

		internal void SetPressedTabLayout(MdiTabItem uiMdiChildTabLayoutState)
		{
			if (_pressedTabLayout != uiMdiChildTabLayoutState)
			{
				if (_pressedTabLayout != null)
				{
					EndTabDrag(false);
					_pressedTabLayout._isPressed = false;
				}
				_pressedTabLayout = uiMdiChildTabLayoutState;
				if (_pressedTabLayout != null)
				{
					if (_pressedTabLayout.GetButtonType() == TabButtonType.ChildForm)
					{
						_pressedTabLayout.GetChildTab().Form.Activate();
						if (_pressedTabLayout != null && _pressedTabLayout.GetChildTab().Form != null)
						{
							if (_pressedTabLayout.GetChildTab().Form.ActiveControl != null)
							{
								_pressedTabLayout.GetChildTab().Form.ActiveControl.Select();
							}
							else
							{
								_pressedTabLayout.GetChildTab().Form.Select();
							}
						}
						else
						{
							_tabGroupHost.GetMdiClient().Select();
						}
					}
					else
					{
						_pressedTabLayout._isPressed = true;
					}
					if (_pressedTabLayout != null)
					{
						if (_pressedTabLayout.GetButtonType() == TabButtonType.ScrollRight)
						{
							MdiTabStripLayout uiMdiTabGroupLayoutState = _groupLayout;
							uiMdiTabGroupLayoutState.SetFirstVisibleIndex(uiMdiTabGroupLayoutState.GetFirstVisibleIndex() + 1);
							StartScrollRepeatTimer();
						}
						if (_pressedTabLayout.GetButtonType() == TabButtonType.ScrollLeft)
						{
							MdiTabStripLayout tabLayout = _groupLayout;
							tabLayout.SetFirstVisibleIndex(tabLayout.GetFirstVisibleIndex() - 1);
							StartScrollRepeatTimer();
						}
					}
				}
				Invalidate();
			}
			else if (_pressedTabLayout != null && _pressedTabLayout.GetButtonType() == TabButtonType.ChildForm)
			{
				_pressedTabLayout.GetChildTab().Form.Activate();
			}
		}
	}
}
