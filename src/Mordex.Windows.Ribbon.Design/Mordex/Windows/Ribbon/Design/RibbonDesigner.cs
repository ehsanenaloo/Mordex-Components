using System;
using Mordex.Windows.Common.Design;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class RibbonDesigner : ControlDesigner
	{
		private bool _handleMouseUp;

		private IDesignerHost _designerHost;

		private ISelectionService _selectionService;

		private BehaviorService _behaviorService;

		private Adorner _tabsAdorner;

		private Adorner _commandsAdorner;

		private Adorner _dropTargetsAdorner;

		private Adorner _backstageAdorner;

		private Adorner _selectAdorner;

		private RibbonTabGlyph _focusedTabGlyph;

		private RibbonTab _focusedTab;

		private bool _isDragging;

		private RibbonTab _dragTab;

		private CommandBase _dragCommand;

		private CommandBase _dropTargetCommand;

		private int _dropTargetIndex;

		private ISupportsDesignerMouse _focusedRibbonGroup;

		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[3]
				{
					new DesignerVerb("Ribbon Designer...", OnCustomize),
					new DesignerVerb("New Tab", OnNewTab),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		public override SelectionRules SelectionRules
		{
			get
			{
				SelectionRules selectionRules = SelectionRules.Visible;
				return selectionRules | (SelectionRules.LeftSizeable | SelectionRules.RightSizeable);
			}
		}

		private MxRibbon Ribbon => Control as MxRibbon;

		internal bool IsDragging
		{
			get
			{
				return _isDragging;
			}
			set
			{
				_isDragging = value;
			}
		}

		internal bool Locked => DesignerChange.IsReadOnly(Ribbon);

		internal new ISelectionService SelectionService => _selectionService;

		private RibbonTab FocusedTab
		{
			get
			{
				return _focusedTab;
			}
			set
			{
				if (_focusedTab != value && _tabsAdorner != null)
				{
					if (_focusedTabGlyph != null && _tabsAdorner.Glyphs.Contains(_focusedTabGlyph))
					{
						_tabsAdorner.Glyphs.Remove(_focusedTabGlyph);
						_tabsAdorner.Invalidate(_focusedTabGlyph.Bounds);
						_focusedTabGlyph = null;
					}
					_focusedTab = value;
					if (_focusedTab != null)
					{
						_focusedTabGlyph = new RibbonTabGlyph(_behaviorService, this, Ribbon, _focusedTab, handleMouseEvents: false);
						_tabsAdorner.Glyphs.Add(_focusedTabGlyph);
						_tabsAdorner.Invalidate(_focusedTabGlyph.Bounds);
					}
				}
			}
		}

		private ISupportsDesignerMouse FocusedRibbonGroup
		{
			get
			{
				return _focusedRibbonGroup;
			}
			set
			{
				if (!DesignHelper.GetDesignerItemPressed(Ribbon) && _focusedRibbonGroup != value)
				{
					if (_focusedRibbonGroup != null)
					{
						_focusedRibbonGroup.DesignerMouseIn = false;
					}
					_focusedRibbonGroup = value;
					if (_focusedRibbonGroup != null)
					{
						_focusedRibbonGroup.DesignerMouseIn = true;
					}
				}
			}
		}

		static RibbonDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public RibbonDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_behaviorService != null)
				{
					RemoveAdorner(_tabsAdorner);
					RemoveAdorner(_commandsAdorner);
					RemoveAdorner(_backstageAdorner);
					RemoveAdorner(_selectAdorner);
					RemoveAdorner(_dropTargetsAdorner);
				}
				if (_selectionService != null)
				{
					_selectionService.SelectionChanged -= OnSelectionServiceSelectionChanged;
				}
				if (_designerHost != null)
				{
					_designerHost.Activated -= OnDesignerHostActivated;
					_designerHost.Deactivated -= OnDesignerHostDeactivated;
					_designerHost.LoadComplete -= OnDesignerHostLoadComplete;
				}
				if (Ribbon != null)
				{
					DesignHelper.SetDesigner(Ribbon, null);
					DesignHelper.RemoveLayoutCompleteHandler(Ribbon, OnRibbonLayoutComplete);
					Ribbon.BackstageOpened -= OnRibbonBackstageOpened;
					Ribbon.BackstageClosed -= OnRibbonBackstageClosed;
				}
			}
			base.Dispose(disposing);
		}

		private void RemoveAdorner(Adorner adorner)
		{
			if (adorner != null)
			{
				_behaviorService.Adorners.Remove(adorner);
			}
		}

		private void OnDesignerHostLoadComplete(object sender, EventArgs eventArgs)
		{
			if (Ribbon != null)
			{
				DesignHelper.PerformDesignHostComplete(Ribbon);
			}
		}

		private void OnNewTab(object sender, EventArgs eventArgs)
		{
			DesignerChange.Run(Ribbon, "Tabs", "New Tab", () =>
			{
				RibbonTab ribbonTab = CreateDefaultTab();
				ribbonTab.Text = "Tab " + (Ribbon.Tabs.Count + 1);
				Ribbon.Tabs.Add(ribbonTab);
			});
		}

		private void OnCustomize(object sender, EventArgs eventArgs)
		{
			DesignerChange.Run(Ribbon, null, "Ribbon Designer", () => DesignHelper.ShowRibbonDesigner(Ribbon) == DialogResult.OK);
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox(Ribbon);
		}

		// The Design Tools Server delivers mouse input to glyph behaviors, not to WndProc, so selection by click lives here.
		internal bool CanSelectAt(Point clientPoint)
		{
			switch (DesignHelper.HitTest(Ribbon, clientPoint))
			{
			case DesignHitTest.RibbonTab:
			case DesignHitTest.RibbonBackground:
			case DesignHitTest.Command:
			case DesignHitTest.BackstageMenuButton:
			case DesignHitTest.ControlBoxButton:
				return true;
			default:
				return false;
			}
		}

		internal bool SelectAt(Point clientPoint, MouseButtons button)
		{
			switch (DesignHelper.HitTest(Ribbon, clientPoint))
			{
			case DesignHitTest.RibbonTab:
			case DesignHitTest.BackstageMenuButton:
			{
				RibbonTab tab = Ribbon.GetTabAt(clientPoint);
				if (tab != null && SelectComponent(tab))
				{
					if (button == MouseButtons.Right)
					{
						Ribbon.SelectedTab = tab;
					}
					return true;
				}
				return false;
			}
			case DesignHitTest.RibbonBackground:
				if (Ribbon.SelectedTab != null)
				{
					return SelectComponent(Ribbon.SelectedTab);
				}
				return false;
			case DesignHitTest.Command:
			{
				CommandBase command = Ribbon.GetCommandAt(clientPoint);
				return command != null && SelectComponent(command);
			}
			case DesignHitTest.ControlBoxButton:
				if (Ribbon.ControlBoxMode == ControlBoxMode.ControlBoxButton)
				{
					DesignHelper.ShowFileMenu(Ribbon);
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		internal bool SelectComponent(Component component)
		{
			return DesignerDragHelper.SelectOnly(GetService(typeof(ISelectionService)) as ISelectionService, component);
		}

		protected override void WndProc(ref Message m)
		{
			if (Ribbon == null)
			{
				base.WndProc(ref m);
				return;
			}
			switch (m.Msg)
			{
			case DesignMessages.MouseLeave:
			{
				Point pt = DesignMouse.ClientPoint(Ribbon, m);
				if (DesignHelper.HitTest(Ribbon, pt) == DesignHitTest.None)
				{
					DesignHelper.SetDesignerMouse(Ribbon, value: false);
					FocusedRibbonGroup = null;
				}
				break;
			}
			case DesignMessages.MouseMove:
			{
				DesignHelper.SetDesignerMouse(Ribbon, value: true);
				Point pt = DesignMouse.ClientPoint(Ribbon, m);
				RibbonTab hoveredTab = Ribbon.GetTabAt(pt.X, pt.Y);
				FocusedTab = hoveredTab;
				ISupportsDesignerMouse ribbonGroup = DesignHelper.GetRibbonGroup(Ribbon, pt);
				FocusedRibbonGroup = ribbonGroup;
				break;
			}
			case DesignMessages.LeftButtonDown:
			case DesignMessages.RightButtonDown:
			{
				Point pt = DesignMouse.ClientPoint(Ribbon, m);
				switch (DesignHelper.HitTest(Ribbon, pt))
				{
				case DesignHitTest.RibbonTab:
				case DesignHitTest.BackstageMenuButton:
				{
					_handleMouseUp = true;
					RibbonTab tabAt = Ribbon.GetTabAt(pt);
					if (tabAt != null)
					{
						if (SelectComponent(tabAt))
						{
							DefWndProc(ref m);
							if (m.Msg == DesignMessages.RightButtonDown)
							{
								Ribbon.SelectedTab = tabAt;
							}
							return;
						}
						break;
					}
					DefWndProc(ref m);
					return;
				}
				case DesignHitTest.ControlBoxButton:
					if (Ribbon.ControlBoxMode == ControlBoxMode.ControlBoxButton)
					{
						DesignHelper.ShowFileMenu(Ribbon);
						_handleMouseUp = true;
					}
					break;
				case DesignHitTest.CustomizeButton:
					_handleMouseUp = true;
					DefWndProc(ref m);
					return;
				case DesignHitTest.RibbonBackground:
					_handleMouseUp = true;
					if (Ribbon.SelectedTab != null)
					{
						SelectComponent(Ribbon.SelectedTab);
						DefWndProc(ref m);
						return;
					}
					break;
				case DesignHitTest.Command:
				{
					_handleMouseUp = true;
					CommandBase commandAt = Ribbon.GetCommandAt(pt);
					if (commandAt != null)
					{
						if (SelectComponent(commandAt))
						{
							DefWndProc(ref m);
							return;
						}
						break;
					}
					DefWndProc(ref m);
					return;
				}
				case DesignHitTest.None:
					_handleMouseUp = false;
					break;
				case DesignHitTest.RibbonScrollButton:
					DefWndProc(ref m);
					_handleMouseUp = true;
					break;
				}
				break;
			}
			case DesignMessages.LeftButtonDoubleClick:
			{
				Point pt = DesignMouse.ClientPoint(Ribbon, m);
				int hitRegion = DesignHelper.HitTest(Ribbon, pt);
				if (hitRegion == 7)
				{
					DefWndProc(ref m);
					return;
				}
				break;
			}
			case DesignMessages.LeftButtonUp:
			case DesignMessages.RightButtonUp:
			{
				Point pt = DesignMouse.ClientPoint(Ribbon, m);
				int releaseHitRegion = DesignHelper.HitTest(Ribbon, pt);
				if (releaseHitRegion == 7)
				{
					return;
				}
				if (_handleMouseUp)
				{
					if (!Locked)
					{
						DefWndProc(ref m);
					}
					else
					{
						base.WndProc(ref m);
					}
					return;
				}
				break;
			}
			}
			base.WndProc(ref m);
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			DesignHelper.SetDesigner(Ribbon, this);
			DesignHelper.AddLayoutCompleteHandler(Ribbon, OnRibbonLayoutComplete);
			Ribbon.BackstageOpened += OnRibbonBackstageOpened;
			Ribbon.BackstageClosed += OnRibbonBackstageClosed;
			_selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
			_behaviorService = GetService(typeof(BehaviorService)) as BehaviorService;
			_designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
			if (_selectionService != null)
			{
				_selectionService.SelectionChanged += OnSelectionServiceSelectionChanged;
			}
			if (_behaviorService != null)
			{
				CreateAdorners();
			}
			if (_designerHost != null)
			{
				_designerHost.Activated += OnDesignerHostActivated;
				_designerHost.Deactivated += OnDesignerHostDeactivated;
				_designerHost.LoadComplete += OnDesignerHostLoadComplete;
			}
		}

		private void OnRibbonBackstageClosed(object sender, EventArgs eventArgs)
		{
			if (_backstageAdorner == null)
			{
				return;
			}
			_backstageAdorner.Glyphs.Clear();
			_backstageAdorner.Invalidate();
		}

		private void OnRibbonBackstageOpened(object sender, EventArgs eventArgs)
		{
			if (_backstageAdorner == null)
			{
				return;
			}
			BackstageGlyph value = new BackstageGlyph(_behaviorService, this, Ribbon, Ribbon.BackstageMenu);
			_backstageAdorner.Glyphs.Add(value);
			_backstageAdorner.Invalidate();
		}

		private void CreateAdorners()
		{
			_tabsAdorner = new Adorner();
			_commandsAdorner = new Adorner();
			_backstageAdorner = new Adorner();
			_behaviorService.Adorners.Add(_tabsAdorner);
			_behaviorService.Adorners.Add(_commandsAdorner);
			_behaviorService.Adorners.Add(_backstageAdorner);
			_selectAdorner = new Adorner();
			_behaviorService.Adorners.Add(_selectAdorner);
			_selectAdorner.Glyphs.Add(new ClickSelectGlyph(_behaviorService, Ribbon, CanSelectAt, SelectAt));
			_selectAdorner.Enabled = true;
			_tabsAdorner.Enabled = true;
			_commandsAdorner.Enabled = true;
			_backstageAdorner.Enabled = true;
		}

		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);
			if (Control != null)
			{
				MxRibbon ribbon = (MxRibbon)Control;
				if (ribbon.Tabs.Count == 0)
				{
					ribbon.Tabs.Add(CreateDefaultTab());
				}
			}
		}

		private RibbonTab CreateDefaultTab()
		{
			IDesignerHost designerHost = _designerHost ?? GetService(typeof(IDesignerHost)) as IDesignerHost;
			RibbonTab ribbonTab = designerHost != null ? (RibbonTab)designerHost.CreateComponent(typeof(RibbonTab)) : new RibbonTab();
			ribbonTab.Text = "Tab 1";
			return ribbonTab;
		}

		private void OnDesignerHostDeactivated(object sender, EventArgs eventArgs)
		{
			DesignHelper.SetChildMenuVisibility(Ribbon, visible: false);
		}

		private void OnDesignerHostActivated(object sender, EventArgs eventArgs)
		{
			DesignHelper.SetChildMenuVisibility(Ribbon, visible: true);
		}

		private void OnSelectionServiceSelectionChanged(object sender, EventArgs eventArgs)
		{
			if (_tabsAdorner != null && _commandsAdorner != null)
			{
				AddSelectedTabs(_selectionService);
				AddSelectedCommands(_selectionService);
			}
			if (Ribbon != null)
			{
				DesignHelper.PerformDesignModeSelectionChanged(Ribbon, _selectionService);
			}
		}

		private void AddSelectedCommands(ISelectionService selection)
		{
			_commandsAdorner.Glyphs.Clear();
			if (selection != null)
			{
				foreach (Component selectedComponent in selection.GetSelectedComponents())
				{
					if (!(selectedComponent is CommandBase))
					{
						continue;
					}
					CommandBase commandBase = selectedComponent as CommandBase;
					Component owner = DesignHelper.GetOwner(commandBase);
					if (owner is DropDownCommand && DesignHelper.GetCommandType(owner) != CommandType.Gallery)
					{
						commandBase = DesignHelper.GetOwner(commandBase) as CommandBase;
						while (commandBase != null && DesignHelper.GetOwner(commandBase) is DropDownCommand)
						{
							commandBase = DesignHelper.GetOwner(commandBase) as CommandBase;
						}
					}
					if (commandBase != null && !Ribbon.Minimized)
					{
						CommandGlyph value = new CommandGlyph(_behaviorService, this, Ribbon, commandBase);
						_commandsAdorner.Glyphs.Add(value);
					}
				}
			}
			foreach (CommandGlyph glyph in _commandsAdorner.Glyphs)
			{
				glyph.Invalidate();
			}
		}

		private void AddSelectedTabs(ISelectionService selection)
		{
			FocusedTab = null;
			_tabsAdorner.Glyphs.Clear();
			if (selection != null)
			{
				foreach (Component selectedComponent in selection.GetSelectedComponents())
				{
					if (selectedComponent is RibbonTab)
					{
						RibbonTab tab = selectedComponent as RibbonTab;
						RibbonTabGlyph value = new RibbonTabGlyph(_behaviorService, this, Ribbon, tab, handleMouseEvents: true);
						_tabsAdorner.Glyphs.Add(value);
					}
				}
			}
			_tabsAdorner.Invalidate();
		}

		private void OnRibbonLayoutComplete(object sender, EventArgs eventArgs)
		{
			if (_commandsAdorner != null)
			{
				foreach (CommandGlyph glyph in _commandsAdorner.Glyphs)
				{
					glyph.Invalidate();
				}
			}
			if (_tabsAdorner == null)
			{
				return;
			}
			foreach (RibbonTabGlyph tabGlyph in _tabsAdorner.Glyphs)
			{
				tabGlyph.Invalidate();
			}
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			if (_isDragging && e.Data.GetDataPresent(typeof(DragCommandData)))
			{
				Point point = Ribbon.PointToClient(new Point(e.X, e.Y));
				if (_dragTab != null)
				{
					RibbonTab tabAt = Ribbon.GetTabAt(point);
					if (tabAt != null)
					{
						e.Effect = DesignerDragHelper.GetDragEffect();
						SetTabDropTargets(tabAt, point);
					}
					else
					{
						e.Effect = DragDropEffects.None;
						_dropTargetIndex = -1;
						RemoveDropTargetGlyphs();
					}
					return;
				}
				CommandBase commandAt = Ribbon.GetCommandAt(point);
				if (CanDropCommand(commandAt))
				{
					e.Effect = DesignerDragHelper.GetDragEffect();
					SetDropTargets(commandAt, point);
				}
				else
				{
					RibbonTab hoveredTab = Ribbon.GetTabAt(point);
					if (hoveredTab != null)
					{
						if (hoveredTab != Ribbon.SelectedTab)
						{
							Ribbon.SelectedTab = hoveredTab;
						}
					}
					else if (DesignHelper.GetCommandType(_dragCommand) == CommandType.RibbonGroup && DesignHelper.GetRibbonBounds(Ribbon).Contains(point) && DesignHelper.GetVisibleGroupsCount(Ribbon) == 0)
					{
						e.Effect = DesignerDragHelper.GetDragEffect();
						SetDropTargets(Ribbon.SelectedTab);
						return;
					}
					e.Effect = DragDropEffects.None;
					_dropTargetCommand = null;
					_dropTargetIndex = -1;
					RemoveDropTargetGlyphs();
				}
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
			base.OnDragOver(e);
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			if (_isDragging && e.Data.GetDataPresent(typeof(DragCommandData)))
			{
				e.Effect = DesignerDragHelper.GetDragEffect();
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
			base.OnDragEnter(e);
		}

		protected override void OnDragDrop(DragEventArgs e)
		{
			base.OnDragDrop(e);
			if (_isDragging && e.Data.GetDataPresent(typeof(DragCommandData)) && _dropTargetIndex != -1)
			{
				Component component = null;
				DesignerChange.Run(Ribbon.Site, _dragTab != null ? "Move Tab" : "Move Command", () =>
				{
					if (_dragTab != null)
					{
						DropRibbonTab();
						component = _dragTab;
					}
					else
					{
						DropCommand();
						component = _dragCommand;
					}
					return true;
				}, DropChangeTargets());
				ArrayList arrayList = new ArrayList();
				arrayList.Add(component);
				_selectionService?.SetSelectedComponents(arrayList, SelectionTypes.Replace);
			}
		}

		// Every component whose collection a drop can change: the old owner and the new one.
		private ChangeTarget[] DropChangeTargets()
		{
			if (_dragTab != null)
			{
				return new[] { new ChangeTarget(Ribbon, "Tabs") };
			}
			return new[]
			{
				new ChangeTarget(DesignHelper.GetOwner(_dragCommand), null),
				new ChangeTarget(_dropTargetCommand, null),
				new ChangeTarget(_dropTargetCommand != null ? DesignHelper.GetOwner(_dropTargetCommand) : null, null),
				new ChangeTarget(_dropTargetCommand == null ? Ribbon.SelectedTab : null, null)
			};
		}

		private void DropCommand()
		{
			if (Control.ModifierKeys != Keys.Control && _dropTargetCommand != null && DesignHelper.GetOwner(_dragCommand) == DesignHelper.GetOwner(_dropTargetCommand) && DesignHelper.GetCommandIndex(_dragCommand) < _dropTargetIndex)
			{
				_dropTargetIndex--;
			}
			if (_dropTargetIndex < 0)
			{
				return;
			}
			if (Control.ModifierKeys != Keys.Control)
			{
				DesignHelper.RemoveFromOwner(_dragCommand);
			}
			else
			{
				_dragCommand = (CommandBase)_dragCommand.Clone();
			}
			if (_dropTargetCommand != null)
			{
				if (DesignHelper.IsCommandOwner(_dropTargetCommand) && DesignHelper.GetCommandType(_dragCommand) != CommandType.RibbonGroup && DesignHelper.GetCommandType(_dropTargetCommand) != CommandType.DropDownButton)
				{
					DesignHelper.InsertCommand(_dropTargetCommand, _dropTargetIndex, _dragCommand);
				}
				else
				{
					DesignHelper.InsertCommandInOwner(_dropTargetCommand, _dropTargetIndex, _dragCommand);
				}
			}
			else if (DesignHelper.GetCommandType(_dragCommand) == CommandType.RibbonGroup)
			{
				Ribbon.SelectedTab.Groups.Insert(_dropTargetIndex, _dragCommand as RibbonGroup);
			}
		}

		private void DropRibbonTab()
		{
			if (Control.ModifierKeys != Keys.Control && _dropTargetIndex >= 0 && _dragTab.Index < _dropTargetIndex)
			{
				_dropTargetIndex--;
			}
			if (_dropTargetIndex >= 0)
			{
				if (Control.ModifierKeys != Keys.Control)
				{
					DesignHelper.RemoveFromOwner(_dragTab);
				}
				else
				{
					_dragTab = _dragTab.Clone();
				}
				Ribbon.Tabs.Insert(_dropTargetIndex, _dragTab);
			}
		}

		internal void ShowBackstageMenu()
		{
			if (Ribbon != null)
			{
				DesignHelper.ShowBackStageMenu(Ribbon);
			}
		}

		internal void StartDrag(Component component)
		{
			if (_selectionService != null)
			{
				if (_selectionService.SelectionCount > 1)
				{
					ArrayList arrayList = new ArrayList();
					arrayList.Add(component);
					_selectionService.SetSelectedComponents(arrayList, SelectionTypes.Replace);
				}
				_dropTargetIndex = -1;
				_isDragging = true;
				DragCommandData data = null;
				if (component is CommandBase)
				{
					_dragCommand = component as CommandBase;
					data = new DragCommandData(_dragCommand);
				}
				else if (component is RibbonTab)
				{
					_dragTab = component as RibbonTab;
					data = new DragCommandData(_dragTab);
				}
				if (data == null)
				{
					_isDragging = false;
					return;
				}
				try
				{
					Ribbon.DoDragDrop(data, DragDropEffects.All);
				}
				finally
				{
					_isDragging = false;
					if (_dropTargetsAdorner != null)
					{
						_dropTargetsAdorner.Glyphs.Clear();
						_dropTargetsAdorner.Invalidate();
						_behaviorService.Adorners.Remove(_dropTargetsAdorner);
						_dropTargetsAdorner = null;
					}
					_dropTargetCommand = null;
					_dragCommand = null;
					_dragTab = null;
				}
			}
		}

		private void SetTabDropTargets(RibbonTab dropTab, Point clientPoint)
		{
			if (dropTab == _dragTab)
			{
				dropTab = null;
			}
			if (dropTab == null)
			{
				return;
			}
			int index = _dragTab.Index;
			int insertIndex = dropTab.Index;
			Rectangle bounds = dropTab.Bounds;
			if (clientPoint.X >= bounds.Right - bounds.Width / 2)
			{
				if (index - 1 != insertIndex)
				{
					insertIndex++;
				}
			}
			else if (index + 1 == insertIndex)
			{
				insertIndex++;
			}
			if (insertIndex != _dropTargetIndex)
			{
				_dropTargetIndex = insertIndex;
				AddArrowGlyph(DesignerDragHelper.HorizontalInsertArrows(bounds, insertIndex > dropTab.Index));
			}
		}

		private void SetDropTargets(RibbonTab tab)
		{
			if (_dropTargetIndex != 0)
			{
				_dropTargetIndex = 0;
				AddArrowGlyph(DesignerDragHelper.ContainerStartArrows(DesignHelper.GetRibbonBounds(Ribbon)));
			}
		}

		private void SetDropTargets(CommandBase dropTarget, Point clientPoint)
		{
			if (dropTarget == _dragCommand)
			{
				dropTarget = null;
			}
			if (DesignHelper.GetCommandType(_dragCommand) == CommandType.RibbonGroup)
			{
				SetRibbonGroupDropTargets(dropTarget, clientPoint);
			}
			else if (dropTarget != null)
			{
				if (DesignHelper.IsCommandOwner(dropTarget) && DesignHelper.GetCommandType(dropTarget) != CommandType.DropDownButton)
				{
					DropOnCommandContainer(dropTarget, clientPoint);
					return;
				}
				_dropTargetCommand = dropTarget;
				int commandIndex = DesignHelper.GetCommandIndex(_dragCommand);
				int insertIndex = DesignHelper.GetCommandIndex(_dropTargetCommand);
				Rectangle bounds = _dropTargetCommand.Bounds;
				Component owner = DesignHelper.GetOwner(_dropTargetCommand);
				if (owner == null)
				{
					return;
				}
				if (DesignHelper.IsHorizontalLayout(owner))
				{
					if (clientPoint.X >= bounds.Right - bounds.Width / 2)
					{
						if (DesignHelper.GetOwner(_dragCommand) != DesignHelper.GetOwner(_dropTargetCommand) || commandIndex - 1 != insertIndex)
						{
							insertIndex++;
						}
					}
					else if (DesignHelper.GetOwner(_dragCommand) == DesignHelper.GetOwner(_dropTargetCommand) && commandIndex + 1 == insertIndex)
					{
						insertIndex++;
					}
					if (insertIndex != _dropTargetIndex)
					{
						_dropTargetIndex = insertIndex;
						AddArrowGlyph(DesignerDragHelper.HorizontalInsertArrows(bounds, insertIndex > DesignHelper.GetCommandIndex(_dropTargetCommand)));
					}
					return;
				}
				if (clientPoint.Y >= bounds.Bottom - bounds.Height / 2)
				{
					if (DesignHelper.GetOwner(_dragCommand) != DesignHelper.GetOwner(_dropTargetCommand) || commandIndex - 1 != insertIndex)
					{
						insertIndex++;
					}
				}
				else if (DesignHelper.GetOwner(_dragCommand) == DesignHelper.GetOwner(_dropTargetCommand) && commandIndex + 1 == insertIndex)
				{
					insertIndex++;
				}
				if (insertIndex != _dropTargetIndex)
				{
					_dropTargetIndex = insertIndex;
					AddArrowGlyph(DesignerDragHelper.VerticalInsertArrows(bounds, insertIndex > DesignHelper.GetCommandIndex(_dropTargetCommand)));
				}
			}
			else
			{
				_dropTargetIndex = -1;
				RemoveDropTargetGlyphs();
			}
		}

		private void SetRibbonGroupDropTargets(CommandBase dropTarget, Point clientPoint)
		{
			while (dropTarget != null && DesignHelper.GetCommandType(dropTarget) != CommandType.RibbonGroup)
			{
				dropTarget = DesignHelper.GetOwner(dropTarget) as CommandBase;
			}
			_dropTargetCommand = dropTarget;
			if (_dropTargetCommand != null)
			{
				int commandIndex = DesignHelper.GetCommandIndex(_dragCommand);
				int insertIndex = DesignHelper.GetCommandIndex(_dropTargetCommand);
				Rectangle bounds = _dropTargetCommand.Bounds;
				if (clientPoint.X >= bounds.Right - bounds.Width / 2)
				{
					if (Control.ModifierKeys == Keys.Control || DesignHelper.GetOwner(_dragCommand) != DesignHelper.GetOwner(_dropTargetCommand) || commandIndex - 1 != insertIndex)
					{
						insertIndex++;
					}
				}
				else if (Control.ModifierKeys != Keys.Control && DesignHelper.GetOwner(_dragCommand) == DesignHelper.GetOwner(_dropTargetCommand) && commandIndex + 1 == insertIndex)
				{
					insertIndex++;
				}
				if (insertIndex != _dropTargetIndex)
				{
					_dropTargetIndex = insertIndex;
					AddArrowGlyph(DesignerDragHelper.HorizontalInsertArrows(bounds, insertIndex > DesignHelper.GetCommandIndex(_dropTargetCommand)));
				}
			}
			else
			{
				_dropTargetIndex = -1;
				RemoveDropTargetGlyphs();
			}
		}

		private void DropOnCommandContainer(CommandBase dropTarget, Point clientPoint)
		{
			object itemsBox = DesignHelper.GetItemsBox(Ribbon, dropTarget);
			if (itemsBox != null)
			{
				object dropInfo = DesignHelper.GetDropInfo(itemsBox, clientPoint, _dragCommand);
				if (dropInfo != null)
				{
					Rectangle dropRect = DesignHelper.GetDropRect(dropInfo);
					if (_dropTargetCommand != dropTarget)
					{
						_dropTargetIndex = -1;
					}
					_dropTargetCommand = dropTarget;
					int dropIndex = DesignHelper.GetDropIndex(dropInfo);
					if (DesignHelper.IsHorizontalLayout(_dropTargetCommand))
					{
						if (_dropTargetIndex != dropIndex)
						{
							_dropTargetIndex = dropIndex;
							AddArrowGlyph(DesignerDragHelper.ContainerStartArrows(dropRect));
						}
					}
					else if (_dropTargetIndex != dropIndex)
					{
						_dropTargetIndex = dropIndex;
						AddArrowGlyph(DesignerDragHelper.ContainerSideArrows(dropRect));
					}
					return;
				}
			}
			_dropTargetCommand = null;
			_dropTargetIndex = -1;
			RemoveDropTargetGlyphs();
		}

		private void AddArrowGlyph(ArrowPair arrows)
		{
			if (_dropTargetsAdorner == null)
			{
				_dropTargetsAdorner = new Adorner();
				_behaviorService.Adorners.Add(_dropTargetsAdorner);
				_dropTargetsAdorner.Enabled = true;
			}
			else
			{
				RemoveDropTargetGlyphs();
			}
			DropArrowGlyph dropArrowGlyph = new DropArrowGlyph(_behaviorService, Ribbon, arrows.First, arrows.FirstDirection);
			DropArrowGlyph secondArrowGlyph = new DropArrowGlyph(_behaviorService, Ribbon, arrows.Second, arrows.SecondDirection);
			_dropTargetsAdorner.Glyphs.Add(dropArrowGlyph);
			_dropTargetsAdorner.Glyphs.Add(secondArrowGlyph);
			_dropTargetsAdorner.Invalidate(dropArrowGlyph.Bounds);
			_dropTargetsAdorner.Invalidate(secondArrowGlyph.Bounds);
		}

		private void RemoveDropTargetGlyphs()
		{
			if (_dropTargetsAdorner != null)
			{
				int glyphIndex;
				for (glyphIndex = 0; glyphIndex < _dropTargetsAdorner.Glyphs.Count; glyphIndex++)
				{
					Glyph glyph = _dropTargetsAdorner.Glyphs[glyphIndex];
					_dropTargetsAdorner.Glyphs.Remove(glyph);
					_dropTargetsAdorner.Invalidate(glyph.Bounds);
					glyphIndex--;
				}
			}
		}

		private bool CanDropCommand(CommandBase dropTarget)
		{
			return DesignerDragHelper.CanDropCommand(_dragCommand, dropTarget);
		}

		public bool RibbonDesigner_SelectComponent(Component component)
		{
			return SelectComponent(component);
		}

		public bool RibbonDesigner_IsLocked()
		{
			return Locked;
		}
	}
}
