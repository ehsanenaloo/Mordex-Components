using System;
using System.Collections;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class MdiTabGroupRenderer : IMordexXmlLayoutsSupport
	{
		// A group size that was never set. Groups with this value share the space equally.
		private const float UnsetGroupSize = -1f;

		private const int SplitterExtraSize = 2;

		// New MDI children start far to the right of the client area, so they do not flash before they are placed in a group.
		private const int OffscreenLeftOffset = 2000;

		private MxMdiTabGroup _activeGroup;

		private ArrayList _groups;

		private MxDockManager _manager;

		private Form _form;

		private MdiClient _mdiClient;

		internal bool _isLoadingLayout;

		private Form _trackedChildForm;

		private bool _isInitializing;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => "TabGroup";

		internal MdiTabGroupRenderer(MxDockManager manager, Form form)
		{
			_groups = new ArrayList();
			_manager = manager;
			_form = form;
			foreach (Control control in _form.Controls)
			{
				if (control is MdiClient)
				{
					SetMdiClient((MdiClient)control);
					break;
				}
			}
			_isInitializing = true;
			_form.ControlAdded += OnFormControlAdded;
			_form.ControlRemoved += OnFormControlRemoved;
			_form.MdiChildActivate += OnFormMdiChildActivate;
			_form.Load += OnMdiClientResize;
			Form activeChild = _form.ActiveMdiChild;
			Form[] mdiChildren = _form.MdiChildren;
			foreach (Form child in mdiChildren)
			{
				if (child.Visible)
				{
					ActivateForm(child, false);
				}
				child.VisibleChanged += OnControlVisibleChanged;
			}
			if (activeChild != null)
			{
				activeChild.Activate();
				ActivateForm(activeChild, true);
			}
			_isInitializing = false;
		}

		private void OnControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			Form form = (Form)sender;
			if (!_isLoadingLayout)
			{
				if (form.Visible)
				{
					AddFormToGroup(form);
				}
				else
				{
					RemoveFormFromGroup(form);
				}
			}
		}

		internal ArrayList GetGroups()
		{
			return _groups;
		}

		internal void AppendGroup(Form form)
		{
			InsertGroup(_groups.Count, form);
		}

		internal void InsertGroup(int index, Form form)
		{
			if (form == null)
			{
				return;
			}
			MdiChildTab tab = null;
			foreach (MxMdiTabGroup item in _groups)
			{
				if (item.ContainsForm(form))
				{
					tab = item.RemoveFormTab(form, false);
					break;
				}
			}
			MxMdiTabGroup newGroup = new MxMdiTabGroup(this);
			_groups.Insert(index, newGroup);
			_activeGroup = newGroup;
			if (tab == null)
			{
				tab = _activeGroup.AddFormTab(form);
			}
			else
			{
				_activeGroup.AttachTab(tab);
			}
			_activeGroup.Parent = _form;
			NormalizeGroupSizes();
			_manager.RaiseMdiTabGroupCreated(_activeGroup, tab);
			_activeGroup.SelectForm(form);
			LayoutGroups();
		}

		internal void MoveFormToGroup(MxMdiTabGroup targetGroup, Form form)
		{
			if (form == null)
			{
				return;
			}
			MdiChildTab tab = null;
			foreach (MxMdiTabGroup item in _groups)
			{
				if (item.ContainsForm(form))
				{
					tab = item.RemoveFormTab(form, false);
					break;
				}
			}
			_activeGroup = targetGroup;
			if (tab != null)
			{
				targetGroup.AttachTab(tab);
			}
			else
			{
				targetGroup.AddFormTab(form);
			}
			_activeGroup.SelectForm(form);
			LayoutGroups();
		}

		internal Form GetForm()
		{
			return _form;
		}

		internal MdiClient GetMdiClient()
		{
			return _mdiClient;
		}

		internal MxDockManager GetManager()
		{
			return _manager;
		}

		internal void Detach()
		{
			SetMdiClient(null);
			_form.ControlAdded -= OnFormControlAdded;
			_form.ControlRemoved -= OnFormControlRemoved;
			_form.MdiChildActivate -= OnFormMdiChildActivate;
			_form.Load -= OnMdiClientResize;
			_form.LayoutMdi(MdiLayout.Cascade);
			_form = null;
			_manager = null;
		}

		internal MxMdiTabGroup GetActiveGroup()
		{
			return _activeGroup;
		}

		private void SetMdiClient(MdiClient mdiClient)
		{
			if (_mdiClient == mdiClient)
			{
				return;
			}
			if (_mdiClient != null)
			{
				foreach (Control control in _mdiClient.Controls)
				{
					if (control is Form)
					{
						control.VisibleChanged -= OnControlVisibleChanged;
					}
				}
				foreach (MxMdiTabGroup item in _groups)
				{
					MdiChildTab[] mdiTabs = item.MdiTabs;
					MdiChildTab[] tabs = mdiTabs;
					foreach (MdiChildTab tab in tabs)
					{
						tab.RestoreForm();
					}
					item.SetHasSplitter(false);
					item.Dispose();
				}
				_activeGroup = null;
				_groups.Clear();
				MdiClientHook.RemoveTabUser(_mdiClient);
				_mdiClient.ControlAdded -= OnMdiClientControlAdded;
				_mdiClient.ControlRemoved -= OnMdiClientControlRemoved;
				_mdiClient.Move -= OnMdiClientResize;
				_mdiClient.Resize -= OnMdiClientResize;
				_mdiClient.Disposed -= OnMdiClientDisposed;
			}
			_mdiClient = mdiClient;
			if (_mdiClient != null)
			{
				_mdiClient.ControlAdded += OnMdiClientControlAdded;
				_mdiClient.ControlRemoved += OnMdiClientControlRemoved;
				_mdiClient.Move += OnMdiClientResize;
				_mdiClient.Resize += OnMdiClientResize;
				_mdiClient.Disposed += OnMdiClientDisposed;
				MdiClientHook.AddTabUser(this, _mdiClient);
				_mdiClient.Invalidate();
			}
		}

		private void AddFormToGroup(Control control)
		{
			if (!(control is Form) || !control.Visible)
			{
				return;
			}
			foreach (MxMdiTabGroup item in _groups)
			{
				MdiChildTab[] mdiTabs = item.MdiTabs;
				foreach (MdiChildTab tab in mdiTabs)
				{
					if (tab.Form == control)
					{
						return;
					}
				}
			}
			if (_activeGroup == null)
			{
				AppendGroup((Form)control);
			}
			else
			{
				_activeGroup.AddFormTab((Form)control);
			}
			LayoutGroups();
		}

		private void RemoveFormFromGroup(Control control)
		{
			if (!(control is Form))
			{
				return;
			}
			foreach (MxMdiTabGroup item in _groups)
			{
				if (!item.ContainsForm((Form)control))
				{
					continue;
				}
				item.RemoveFormTab((Form)control, true);
				if ((Form)control == _trackedChildForm)
				{
					_trackedChildForm = null;
				}
				if (item.TabCount == 0)
				{
					item.SetHasSplitter(false);
					item.Dispose();
					_groups.Remove(item);
					if (_groups.Count == 0)
					{
						_activeGroup = null;
					}
					_mdiClient.Invalidate();
					NormalizeGroupSizes();
					if (GetManager() != null)
					{
						GetManager().RaiseMdiTabGroupDestroyed(item);
					}
				}
				break;
			}
			LayoutGroups();
		}

		private void NormalizeGroupSizes()
		{
			float totalSize = 0f;
			int unsetCount = 0;
			for (int i = 0; i < _groups.Count; i++)
			{
				MxMdiTabGroup group = (MxMdiTabGroup)_groups[i];
				group.SetHasSplitter(i < _groups.Count - 1);
				if (group._relativeSize == UnsetGroupSize)
				{
					unsetCount++;
				}
				else
				{
					totalSize += group._relativeSize;
				}
			}
			if (unsetCount > 0)
			{
				totalSize = _groups.Count;
				foreach (MxMdiTabGroup item in _groups)
				{
					if (item._relativeSize == UnsetGroupSize)
					{
						item._relativeSize = 1f;
					}
					else
					{
						item._relativeSize *= _groups.Count - 1;
					}
				}
			}
			foreach (MxMdiTabGroup sizedGroup in _groups)
			{
				sizedGroup._relativeSize /= totalSize;
			}
		}

		internal void LayoutGroups()
		{
			if (_mdiClient == null)
			{
				return;
			}
			for (int groupIndex = _groups.Count - 1; groupIndex >= 0; groupIndex--)
			{
				MxMdiTabGroup emptyGroup = (MxMdiTabGroup)_groups[groupIndex];
				if (emptyGroup.TabCount == 0)
				{
					emptyGroup.SetHasSplitter(false);
					emptyGroup.Dispose();
					_groups.RemoveAt(groupIndex);
					NormalizeGroupSizes();
					if (GetManager() != null)
					{
						GetManager().RaiseMdiTabGroupDestroyed(emptyGroup);
					}
				}
			}
			Rectangle clientRectangle = _mdiClient.ClientRectangle;
			int groupCount = _groups.Count;
			if (groupCount > 0)
			{
				if (_manager.TabbedMdiSettings.GroupOrientation == MdiGroupOrientation.Horizontal)
				{
					int availableHeight = clientRectangle.Height;
					if (_groups.Count > 1)
					{
						availableHeight -= (groupCount - 1) * (_manager.SplitterSize + SplitterExtraSize);
					}
					_ = availableHeight / groupCount;
					int nextY = clientRectangle.Top;
					int usedHeight = 0;
					for (int i = 0; i < groupCount; i++)
					{
						MxMdiTabGroup rowGroup = (MxMdiTabGroup)_groups[i];
						int rowHeight = (int)((float)availableHeight * rowGroup._relativeSize);
						Rectangle rectangle = new Rectangle(clientRectangle.X, nextY, clientRectangle.Width, rowHeight);
						usedHeight += rowHeight;
						nextY += rectangle.Height;
						rowGroup.SetGroupBounds(rectangle);
						if (rowGroup.HasSplitter())
						{
							rectangle = new Rectangle(clientRectangle.X - 1, nextY, clientRectangle.Width + 2, _manager.SplitterSize + SplitterExtraSize);
							Point p = _mdiClient.PointToScreen(rectangle.Location);
							rectangle.Location = GetForm().PointToClient(p);
							rowGroup.GetSplitter().Bounds = rectangle;
							nextY += rectangle.Height;
						}
					}
					if (groupCount > 0)
					{
						MxMdiTabGroup lastRowGroup = (MxMdiTabGroup)_groups[_groups.Count - 1];
						Rectangle lastRowBounds = lastRowGroup.GetGroupBounds();
						lastRowBounds.Height += availableHeight - usedHeight;
						lastRowGroup.SetGroupBounds(lastRowBounds);
					}
				}
				else
				{
					int availableWidth = clientRectangle.Width;
					if (_groups.Count > 1)
					{
						availableWidth -= (groupCount - 1) * (_manager.SplitterSize + SplitterExtraSize);
					}
					_ = availableWidth / groupCount;
					int nextX = clientRectangle.X;
					int usedWidth = 0;
					for (int j = 0; j < groupCount; j++)
					{
						MxMdiTabGroup columnGroup = (MxMdiTabGroup)_groups[j];
						int columnWidth = (int)((float)availableWidth * columnGroup._relativeSize);
						Rectangle columnBounds = new Rectangle(nextX, clientRectangle.Y, columnWidth, clientRectangle.Height);
						usedWidth += columnWidth;
						nextX += columnBounds.Width;
						columnGroup.SetGroupBounds(columnBounds);
						if (columnGroup.HasSplitter())
						{
							columnBounds = new Rectangle(nextX, clientRectangle.Y - 1, _manager.SplitterSize + SplitterExtraSize, clientRectangle.Height + 2);
							Point columnSplitterOrigin = _mdiClient.PointToScreen(columnBounds.Location);
							columnBounds.Location = GetForm().PointToClient(columnSplitterOrigin);
							columnGroup.GetSplitter().Bounds = columnBounds;
							nextX += columnBounds.Width;
						}
					}
					if (groupCount > 0)
					{
						MxMdiTabGroup lastColumnGroup = (MxMdiTabGroup)_groups[_groups.Count - 1];
						Rectangle lastColumnBounds = lastColumnGroup.GetGroupBounds();
						lastColumnBounds.Width += availableWidth - usedWidth;
						lastColumnGroup.SetGroupBounds(lastColumnBounds);
					}
				}
			}
			_mdiClient.Invalidate();
			foreach (MxMdiTabGroup item in _groups)
			{
				item.RebuildLayout(true);
				if (item.HasSplitter())
				{
					item.GetSplitter().Invalidate();
				}
			}
		}

		internal void RenderContent(Graphics graphics, Rectangle rect)
		{
			DockManagerRendererBase managerRenderer = _manager.GetMdiRenderer();
			if (_groups.Count == 0)
			{
				managerRenderer.DrawInnerAreaBackground(graphics, _mdiClient, rect, PanelInnerAreaStyle.AppWorkspace);
				return;
			}
			PanelPadding windowPadding = _manager.TabbedMdiSettings.WindowPadding;
			if (windowPadding.Left == 0 && windowPadding.Top == 0 && windowPadding.Right == 0 && windowPadding.Bottom == 0)
			{
				return;
			}
			foreach (MxMdiTabGroup item in _groups)
			{
				Rectangle bounds = item.GetGroupBounds();
				bounds.Y += item.Height - 1;
				bounds.Height -= item.Height - 1;
				ButtonState buttonState = ButtonState.Normal;
				if (item == _activeGroup)
				{
					buttonState = ButtonState.Selected;
				}
				FormatStyle style = item.ResolveTabStyle(item.SelectedTab, buttonState, false);
				managerRenderer.DrawMdiBackground(graphics, item, bounds, style);
			}
		}

		private void OnFormMdiChildActivate(object sender, EventArgs eventArgs)
		{
			if (!_isInitializing)
			{
				if (_mdiClient != null)
				{
					_mdiClient.Invalidate();
				}
				if (GetForm().ActiveMdiChild == null)
				{
					_activeGroup = null;
				}
				else
				{
					ActivateForm(GetForm().ActiveMdiChild, true);
				}
			}
		}

		private void ActivateForm(Form childForm, bool makeGroupActive)
		{
			if (childForm == _trackedChildForm)
			{
				return;
			}
			_trackedChildForm = childForm;
			MxMdiTabGroup group = null;
			foreach (MxMdiTabGroup item in _groups)
			{
				if (item.ContainsForm(childForm))
				{
					group = item;
					break;
				}
			}
			if (group != null)
			{
				if (makeGroupActive)
				{
					_activeGroup = group;
				}
			}
			else
			{
				if (_activeGroup == null)
				{
					AppendGroup(childForm);
				}
				else
				{
					_activeGroup.AddFormTab(childForm);
				}
				group = _activeGroup;
			}
			group.SelectForm(childForm);
			foreach (MxMdiTabGroup otherGroup in _groups)
			{
				otherGroup.Invalidate();
			}
		}

		private void OnFormControlAdded(object sender, ControlEventArgs eventArgs)
		{
			if (eventArgs.Control is MdiClient)
			{
				SetMdiClient((MdiClient)eventArgs.Control);
			}
		}

		private void OnFormControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			if (eventArgs.Control == _mdiClient)
			{
				SetMdiClient(null);
			}
		}

		private void OnMdiClientControlAdded(object sender, ControlEventArgs eventArgs)
		{
			Form form = (Form)eventArgs.Control;
			form.VisibleChanged += OnControlVisibleChanged;
			form.StartPosition = FormStartPosition.Manual;
			eventArgs.Control.Left = _mdiClient.ClientRectangle.Right + OffscreenLeftOffset;
			AddFormToGroup(eventArgs.Control);
		}

		private void OnMdiClientControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			RemoveFormFromGroup(eventArgs.Control);
			eventArgs.Control.VisibleChanged -= OnControlVisibleChanged;
		}

		private void OnMdiClientResize(object sender, EventArgs eventArgs)
		{
			LayoutGroups();
		}

		private void OnMdiClientDisposed(object sender, EventArgs eventArgs)
		{
			SetMdiClient(null);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			MdiClient mdiClient = _mdiClient;
			SetMdiClient(null);
			if (mdiClient != null)
			{
				foreach (Form control in mdiClient.Controls)
				{
					control.Close();
				}
			}
			SetMdiClient(mdiClient);
			_isInitializing = true;
			MxMdiTabGroup selectedGroup = null;
			int groupIndex = 0;
			for (PropertyValue propertyValue = reader["TabGroup" + groupIndex]; propertyValue != null; propertyValue = reader["TabGroup" + groupIndex])
			{
				MxMdiTabGroup loadedGroup = (_activeGroup = new MxMdiTabGroup(this));
				propertyValue.DeserializeObject(loadedGroup);
				if (loadedGroup.MdiTabs.Length > 0)
				{
					loadedGroup.Parent = GetForm();
					_groups.Add(loadedGroup);
					if (loadedGroup._isSelected)
					{
						selectedGroup = loadedGroup;
					}
				}
				else
				{
					if (_activeGroup == loadedGroup)
					{
						_activeGroup = selectedGroup;
					}
					loadedGroup.Dispose();
				}
				groupIndex++;
			}
			NormalizeGroupSizes();
			LayoutGroups();
			foreach (MxMdiTabGroup item in _groups)
			{
				item.RebuildLayout(true);
			}
			_isInitializing = false;
			if (selectedGroup == null && _groups.Count > 0)
			{
				selectedGroup = (MxMdiTabGroup)_groups[0];
			}
			selectedGroup?.GetSelectedForm().Activate();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int groupIndex = 0;
			foreach (MxMdiTabGroup item in _groups)
			{
				info.AddValue("TabGroup" + groupIndex, item);
				groupIndex++;
			}
		}
	}
}
