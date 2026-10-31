using System.Collections;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers.Behaviors;

namespace Mordex.Windows.Ribbon.Design
{
	internal class CommandBehavior : Behavior
	{
		private CommandBase _command;

		private GlyphOwnerWrapper _owner;

		private RibbonDesigner _ribbonDesigner;

		private StatusBarDesigner _statusBarDesigner;

		private BehaviorService _behaviorService;

		private Point _dragStartPoint;

		private bool _isCommandMouseDown;

		internal CommandBehavior(CommandBase command, MxRibbon owner, RibbonDesigner designer, BehaviorService behaviorService)
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			this._command = command;
			this._owner = new GlyphOwnerWrapper(owner);
			_ribbonDesigner = designer;
			this._behaviorService = behaviorService;
			_dragStartPoint = Cursor.Position;
		}

		internal CommandBehavior(CommandBase command, MxRibbonStatusBar owner, StatusBarDesigner designer, BehaviorService behaviorService)
		{
			this._command = command;
			this._owner = new GlyphOwnerWrapper(owner);
			_statusBarDesigner = designer;
			this._behaviorService = behaviorService;
			_dragStartPoint = Cursor.Position;
		}

		private ISelectionService SelectionService => _ribbonDesigner != null ? _ribbonDesigner.SelectionService : _statusBarDesigner?.SelectionService;

		private bool SelectComponent(CommandBase component)
		{
			if (_ribbonDesigner != null)
			{
				return _ribbonDesigner.SelectComponent(component);
			}
			return _statusBarDesigner != null && _statusBarDesigner.SelectComponent(component);
		}

		public override bool OnMouseDown(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			_dragStartPoint = Cursor.Position;
			_isCommandMouseDown = true;
			Point screenPoint = _behaviorService.AdornerWindowToScreen(mouseLoc);
			CommandBase commandAt = _owner.GetCommandAt(_owner.PointToClient(screenPoint));
			if (commandAt != null && Control.ModifierKeys != Keys.Control && commandAt != _command)
			{
				SelectComponent(commandAt);
			}
			return true;
		}

		public override bool OnMouseUp(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			_dragStartPoint = Point.Empty;
			if (_isCommandMouseDown)
			{
				_isCommandMouseDown = false;
				ISelectionService selectionService = SelectionService;
				if (Control.ModifierKeys == Keys.Control && selectionService != null)
				{
					CommandBase commandAt = _owner.GetCommandAt(_owner.PointToClient(Cursor.Position));
					if (commandAt == _command)
					{
						selectionService.SetSelectedComponents(new ArrayList { _command }, SelectionTypes.Remove);
					}
					else if (commandAt != null)
					{
						SelectComponent(commandAt);
					}
					return true;
				}
			}
			return base.OnMouseUp(glyph, button, mouseLoc);
		}

		public override bool OnMouseMove(Glyph glyph, MouseButtons button, Point mouseLoc)
		{
			if (button == MouseButtons.Left && !_dragStartPoint.IsEmpty && DesignHelper.GetCommandType(_command) != CommandType.GalleryItem && DesignerDragHelper.DragDetect(_dragStartPoint))
			{
				_ribbonDesigner?.StartDrag(_command);
				_statusBarDesigner?.StartDrag(_command);
			}
			return base.OnMouseMove(glyph, button, mouseLoc);
		}
	}
}
