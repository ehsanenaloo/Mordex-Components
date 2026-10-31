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
	public class StatusBarDesigner : ControlDesigner
	{
		private bool _handleMouseUp;

		private IDesignerHost _designerHost;

		private ISelectionService _selectionService;

		private BehaviorService _behaviorService;

		private Adorner _commandsAdorner;

		private Adorner _selectAdorner;

		private Adorner _dropTargetsAdorner;

		private bool _isDragging;

		private CommandBase _dragCommand;

		private CommandBase _dropTargetCommand;

		private int _dropTargetIndex;

		private ISupportsDesignerMouse _focusedPanelHandler;

		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[2]
				{
					new DesignerVerb("StatusBar Designer...", OnCustomize),
					new DesignerVerb("About", OnAbout)
				};
				return new DesignerVerbCollection(value);
			}
		}

		private MxRibbonStatusBar StatusBar => Control as MxRibbonStatusBar;

		private ISupportsDesignerMouse FocusedPanelHandler
		{
			get
			{
				return _focusedPanelHandler;
			}
			set
			{
				if (!DesignHelper.GetDesignerItemPressed(StatusBar) && _focusedPanelHandler != value)
				{
					if (_focusedPanelHandler != null)
					{
						_focusedPanelHandler.DesignerMouseIn = false;
					}
					_focusedPanelHandler = value;
					if (_focusedPanelHandler != null)
					{
						_focusedPanelHandler.DesignerMouseIn = true;
					}
				}
			}
		}

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

		internal bool Locked => DesignerChange.IsReadOnly(StatusBar);

		internal new ISelectionService SelectionService => _selectionService;

		public StatusBarDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_behaviorService != null)
				{
					RemoveAdorner(_commandsAdorner);
					RemoveAdorner(_selectAdorner);
					RemoveAdorner(_dropTargetsAdorner);
				}
				if (_selectionService != null)
				{
					_selectionService.SelectionChanged -= SelectionService_SelectionChanged;
				}
				if (_designerHost != null)
				{
					_designerHost.Activated -= OnDesignerHost_Activated;
					_designerHost.Deactivated -= OnDesignerHost_Deactivated;
				}
				if (StatusBar != null)
				{
					DesignHelper.SetDesigner(StatusBar, null);
					DesignHelper.RemoveLayoutCompleteHandler(StatusBar, StatusBar_LayoutComplete);
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

		private void OnCustomize(object sender, EventArgs eventArgs)
		{
			DesignerChange.Run(StatusBar, null, "Status Bar Designer", () => DesignHelper.ShowRibbonDesigner(StatusBar) == DialogResult.OK);
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox(StatusBar);
		}

		internal bool SelectComponent(Component component)
		{
			return DesignerDragHelper.SelectOnly(GetService(typeof(ISelectionService)) as ISelectionService, component);
		}

		protected override void WndProc(ref Message m)
		{
			if (StatusBar == null)
			{
				base.WndProc(ref m);
				return;
			}
			switch (m.Msg)
			{
			case DesignMessages.MouseLeave:
				DesignHelper.SetDesignerMouse(StatusBar, value: false);
				FocusedPanelHandler = null;
				break;
			case DesignMessages.MouseMove:
			{
				DesignHelper.SetDesignerMouse(StatusBar, value: true);
				Point point = DesignMouse.ClientPoint(StatusBar, m);
				ISupportsDesignerMouse panelHandler = DesignHelper.GetPanelHandler(StatusBar, point);
				FocusedPanelHandler = panelHandler;
				break;
			}
			case DesignMessages.LeftButtonDown:
			case DesignMessages.RightButtonDown:
			{
				if (Locked)
				{
					base.WndProc(ref m);
					return;
				}
				Point point = DesignMouse.ClientPoint(StatusBar, m);
				switch (DesignHelper.HitTest(StatusBar, point))
				{
				case DesignHitTest.Command:
				{
					_handleMouseUp = true;
					CommandBase commandAt = DesignHelper.GetCommandAt(StatusBar, point);
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
			case DesignMessages.LeftButtonUp:
			case DesignMessages.RightButtonUp:
				if (_handleMouseUp)
				{
					DefWndProc(ref m);
					return;
				}
				break;
			}
			base.WndProc(ref m);
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			DesignHelper.SetDesigner(StatusBar, this);
			DesignHelper.AddLayoutCompleteHandler(StatusBar, StatusBar_LayoutComplete);
			_selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
			_behaviorService = GetService(typeof(BehaviorService)) as BehaviorService;
			_designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
			if (_selectionService != null)
			{
				_selectionService.SelectionChanged += SelectionService_SelectionChanged;
			}
			if (_behaviorService != null)
			{
				CreateAdorners();
			}
			if (_designerHost != null)
			{
				_designerHost.Activated += OnDesignerHost_Activated;
				_designerHost.Deactivated += OnDesignerHost_Deactivated;
			}
		}

		private void CreateAdorners()
		{
			_commandsAdorner = new Adorner();
			_behaviorService.Adorners.Add(_commandsAdorner);
			_commandsAdorner.Enabled = true;
			_selectAdorner = new Adorner();
			_behaviorService.Adorners.Add(_selectAdorner);
			_selectAdorner.Glyphs.Add(new ClickSelectGlyph(_behaviorService, StatusBar, CanSelectAt, SelectAt));
			_selectAdorner.Enabled = true;
		}

		private bool CanSelectAt(Point clientPoint)
		{
			return !Locked && DesignHelper.HitTest(StatusBar, clientPoint) == DesignHitTest.Command && DesignHelper.GetCommandAt(StatusBar, clientPoint) != null;
		}

		private bool SelectAt(Point clientPoint, MouseButtons button)
		{
			CommandBase command = DesignHelper.GetCommandAt(StatusBar, clientPoint);
			return command != null && SelectComponent(command);
		}

		private void OnDesignerHost_Deactivated(object sender, EventArgs eventArgs)
		{
			DesignHelper.SetChildMenuVisibility(StatusBar, visible: false);
		}

		private void OnDesignerHost_Activated(object sender, EventArgs eventArgs)
		{
			DesignHelper.SetChildMenuVisibility(StatusBar, visible: true);
		}

		private void SelectionService_SelectionChanged(object sender, EventArgs eventArgs)
		{
			if (_commandsAdorner != null)
			{
				AddSelectedCommands(_selectionService);
			}
			if (StatusBar != null)
			{
				DesignHelper.PerformDesignModeSelectionChanged(StatusBar, _selectionService);
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
						commandBase = owner as CommandBase;
						while (commandBase != null && DesignHelper.GetOwner(commandBase) is DropDownCommand)
						{
							commandBase = DesignHelper.GetOwner(commandBase) as CommandBase;
						}
					}
					if (commandBase != null)
					{
						CommandGlyph value = new CommandGlyph(_behaviorService, this, StatusBar, commandBase);
						_commandsAdorner.Glyphs.Add(value);
					}
				}
			}
			foreach (CommandGlyph glyph in _commandsAdorner.Glyphs)
			{
				glyph.Invalidate();
			}
		}

		private void StatusBar_LayoutComplete(object sender, EventArgs eventArgs)
		{
			if (_commandsAdorner == null)
			{
				return;
			}
			foreach (CommandGlyph glyph in _commandsAdorner.Glyphs)
			{
				glyph.Invalidate();
			}
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			if (_isDragging && e.Data.GetDataPresent(typeof(DragCommandData)))
			{
				Point point = StatusBar.PointToClient(new Point(e.X, e.Y));
				CommandBase commandAt = DesignHelper.GetCommandAt(StatusBar, point);
				if (CanDropCommand(commandAt))
				{
					e.Effect = DesignerDragHelper.GetDragEffect();
					SetDropTargets(commandAt, point);
				}
				else
				{
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
				DesignerChange.Run(StatusBar.Site, "Move Command", () =>
				{
					DropCommand();
					return true;
				}, new ChangeTarget(DesignHelper.GetOwner(_dragCommand), null), new ChangeTarget(_dropTargetCommand, null), new ChangeTarget(_dropTargetCommand != null ? DesignHelper.GetOwner(_dropTargetCommand) : null, null));
				Component component = _dragCommand;
				ArrayList arrayList = new ArrayList();
				arrayList.Add(component);
				_selectionService?.SetSelectedComponents(arrayList, SelectionTypes.Replace);
			}
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
				if (data == null)
				{
					_isDragging = false;
					return;
				}
				try
				{
					StatusBar.DoDragDrop(data, DragDropEffects.All);
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
				}
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
				if (owner != null)
				{
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
			object itemsBox = DesignHelper.GetItemsBox(StatusBar, dropTarget);
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
			DropArrowGlyph dropArrowGlyph = new DropArrowGlyph(_behaviorService, StatusBar, arrows.First, arrows.FirstDirection);
			DropArrowGlyph secondArrowGlyph = new DropArrowGlyph(_behaviorService, StatusBar, arrows.Second, arrows.SecondDirection);
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
