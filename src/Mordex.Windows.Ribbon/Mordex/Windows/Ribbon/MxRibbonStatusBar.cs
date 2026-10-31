using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	[ToolboxItem(true)]
	[Designer("Mordex.Windows.Ribbon.Design.StatusBarDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultEvent("CommandClick")]
	public partial class MxRibbonStatusBar : Control, ICommandContainer, ICommandRenderHost, IRibbonHitTestSource
	{
		private CommandCollection _leftPanelCommands;

		private CommandCollection _rightPanelCommands;

		private bool _isLayoutDirty;

		private CommandBaseEditorDialog _childMenu;

		private Size _imageSize;

		private CommandBaseRenderer _hotRenderer;

		private CommandBaseRenderer _pushedRenderer;

		internal RibbonColorScheme _colorScheme;

		private bool _showToolTips;

		private bool? _useCompatibleTextRendering;

		private ImageList _commandImageList;

		private StatusBarCommandsRenderer _leftPanelRenderer;

		private StatusBarCommandsRenderer _rightPanelRenderer;

		internal MxSuperTip _mordexSuperTip;

		private ButtonCommand _superTipCommand;

		private Point _resizeStartPoint;

		private Size _resizeStartSize;

		internal Rectangle _sizeGripBounds;

		private bool _isDesignerMouseActive;

		internal object _designer;

		internal bool _designerItemPressed;

		private int _superTipDelay;

		private int _superTipWidth;

		private bool _showSizeGrip = true;

		private static readonly object s_commandDropDownClosingKey = new object();

		private static readonly object s_commandDropDownClosedKey = new object();

		private static readonly object s_commandDropDownOpeningKey = new object();

		private static readonly object s_commandDropDownOpenedKey = new object();

		private static readonly object s_commandClickKey = new object();

		private EventHandler _layoutCalculatedHandler;

		private static readonly object s_layoutChangedKey = new object();

		private RibbonWindowMessageHandler _windowMessageHandler;

		private IContainer _components;

		internal ImageList _designerImageList;

		public MxRibbonStatusBar()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			InitializeControls();
			InitializeStatusBar();
		}

		public static string GetRibbonVersionString()
		{
			string localPath = typeof(MxRibbon).Assembly.Location;
			if (string.IsNullOrEmpty(localPath))
			{
				// Single-file apps have no assembly path.
				return typeof(MxRibbon).Assembly.GetName().Version?.ToString() ?? "";
			}
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		internal void InitializeStatusBar()
		{
			Dock = DockStyle.Bottom;
			_leftPanelCommands = new CommandCollection(this, isQuickAccessCollection: false);
			_rightPanelCommands = new CommandCollection(this, isQuickAccessCollection: false);
			_mordexSuperTip = new MxSuperTip { UseDefaultTheme = true };
			_showToolTips = true;
			_leftPanelRenderer = new StatusBarCommandsRenderer(_leftPanelCommands, this, StatusBarPanelType.LeftPanel);
			_rightPanelRenderer = new StatusBarCommandsRenderer(_rightPanelCommands, this, StatusBarPanelType.RightPanel);
			_imageSize = new Size(16, 16);
			RefreshColorScheme();
			_superTipWidth = 210;
			_superTipDelay = 9000;
		}

		private void RefreshColorScheme()
		{
			_colorScheme = ThemedRibbon.CreateScheme(MxThemeManager.ResolveDefault(DeviceDpi));
			Color statusText = _colorScheme.Theme.Color(ThemeRole.StatusBarText);
			_colorScheme._foreColorDark = statusText;
			_colorScheme._foreColorLight = statusText;
			ForeColor = _colorScheme._foreColorLight;
			Invalidate();
			if (GetWindowMessageHandler() != null)
			{
				GetWindowMessageHandler().Refresh(update: false);
			}
		}

		internal void RebuildPanelLayouts()
		{
			if (_leftPanelCommands != null)
			{
				_leftPanelRenderer.InvalidateItems();
			}
			if (_rightPanelCommands != null)
			{
				_rightPanelRenderer.InvalidateItems();
			}
			InvalidateLayout();
		}

		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			if (_leftPanelRenderer != null)
			{
				_leftPanelRenderer.InvalidateItems();
			}
			if (_rightPanelRenderer != null)
			{
				_rightPanelRenderer.InvalidateItems();
			}
			Invalidate(invalidateChildren: true);
			OnLayoutChanged(EventArgs.Empty);
		}

		internal void OnCommandPropertyChanged(CommandBase commandBase, string text)
		{
			_isLayoutDirty = true;
			if (text == "Visible")
			{
				InvalidateLayout();
			}
			Invalidate(invalidateChildren: true);
		}

		internal CommandBase FindCommandAt(Point point)
		{
			if (_leftPanelRenderer != null)
			{
				CommandBase commandBase = _leftPanelRenderer.FindCommandAt(point.X, point.Y);
				if (commandBase != null)
				{
					return commandBase;
				}
			}
			if (_rightPanelRenderer != null)
			{
				return _rightPanelRenderer.FindCommandAt(point.X, point.Y);
			}
			return null;
		}

		public CommandCollection GetAllCommands()
		{
			Hashtable hashtable = new Hashtable();
			CollectCommands(LeftPanelCommands, hashtable);
			CollectCommands(RightPanelCommands, hashtable);
			CommandCollection commandCollection = new CommandCollection(null, isQuickAccessCollection: false);
			foreach (CommandBase value in hashtable.Values)
			{
				commandCollection.Add(value);
			}
			return commandCollection;
		}

		private void CollectCommands(CommandCollection commandCollection, Hashtable hashtable)
		{
			foreach (CommandBase item in commandCollection)
			{
				if (!hashtable.ContainsKey(item))
				{
					hashtable.Add(item, item);
					if (item is DropDownCommand)
					{
						CollectCommands(((DropDownCommand)item).Commands, hashtable);
					}
				}
			}
		}

		internal StatusBarCommandsRenderer GetPanelRendererAt(int x, int y)
		{
			if (_leftPanelRenderer != null && _leftPanelRenderer._bounds.Contains(x, y))
			{
				return _leftPanelRenderer;
			}
			if (_rightPanelRenderer != null && _rightPanelRenderer._bounds.Contains(x, y))
			{
				return _rightPanelRenderer;
			}
			return null;
		}

		internal Rectangle GetCommandBounds(CommandBase commandBase)
		{
			CommandBaseRenderer commandBaseRenderer = FindCommandRenderer(commandBase);
			if (commandBaseRenderer != null)
			{
				Rectangle bounds = commandBaseRenderer._bounds;
				if (commandBaseRenderer is GalleryRenderer)
				{
					bounds.Inflate(0, -3);
				}
				return bounds;
			}
			return Rectangle.Empty;
		}

		internal CommandBaseRenderer FindCommandRenderer(CommandBase commandBase)
		{
			if (!base.IsHandleCreated)
			{
				return null;
			}
			if (_isLayoutDirty)
			{
				using Graphics graphics = CreateGraphics();
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				LayoutPanels(graphics, textLayoutRenderer, false);
			}
			CommandBaseRenderer commandBaseRenderer = _leftPanelRenderer.GetItemRendererForCommand(commandBase);
			if (commandBaseRenderer != null)
			{
				return commandBaseRenderer;
			}
			commandBaseRenderer = _rightPanelRenderer.GetItemRendererForCommand(commandBase);
			if (commandBaseRenderer != null)
			{
				return commandBaseRenderer;
			}
			if (GetChildMenu() != null)
			{
				commandBaseRenderer = GetChildMenu().FindRendererForCommand(commandBase);
			}
			return commandBaseRenderer;
		}

		private void OnControlComponentRename(object sender, ComponentRenameEventArgs eventArgs)
		{
			if (eventArgs.Component is CommandBase)
			{
				CommandBase commandBase = (CommandBase)eventArgs.Component;
				commandBase.SyncNameFromSite();
			}
		}

		internal void SelectInDesigner(CommandBase component)
		{
			if (_designer != null)
			{
				DesignHelper.Designer_SelectComponent(_designer, component);
			}
		}

		private void DistributeSpringWidth(int extraWidth)
		{
			System.Collections.Generic.List<CommandBaseRenderer> springs = new System.Collections.Generic.List<CommandBaseRenderer>();
			foreach (CommandBaseRenderer item in _leftPanelRenderer._itemRenderers)
			{
				if (item is StatusBarPanelRenderer && ((StatusBarPanel)item.GetCommand()).AutoSize == StatusBarPanelAutoSize.Spring)
				{
					springs.Add(item);
				}
			}
			foreach (CommandBaseRenderer rightPanelItem in _rightPanelRenderer._itemRenderers)
			{
				if (rightPanelItem is StatusBarPanelRenderer && ((StatusBarPanel)rightPanelItem.GetCommand()).AutoSize == StatusBarPanelAutoSize.Spring)
				{
					springs.Add(rightPanelItem);
				}
			}
			if (springs.Count == 0 || extraWidth <= 0)
			{
				return;
			}
			int share = extraWidth / springs.Count;
			int remainder = extraWidth % springs.Count;
			for (int i = 0; i < springs.Count; i++)
			{
				springs[i]._fullItemWidth += share + (i < remainder ? 1 : 0);
			}
		}

		internal void LayoutPanels(Graphics graphics, TextLayoutRenderer textLayoutRenderer, bool raiseLayoutEvent)
		{
			_isLayoutDirty = false;
			int leftWidth = _leftPanelRenderer.MeasureItems(graphics, textLayoutRenderer, Font);
			int rightPanelWidth = _rightPanelRenderer.MeasureItems(graphics, textLayoutRenderer, Font);
			if (leftWidth + rightPanelWidth < base.Width)
			{
				DistributeSpringWidth(base.Width - (leftWidth + rightPanelWidth));
			}
			rightPanelWidth = _rightPanelRenderer.GetTotalWidth();
			int splitX = base.Width - rightPanelWidth;
			int topMargin = 2;
			Rectangle rectangle = new Rectangle(0, topMargin, splitX, base.Height - topMargin);
			Rectangle rightPanelRect = new Rectangle(splitX, topMargin, base.Width - splitX, base.Height - topMargin);
			_leftPanelRenderer.RenderContent(graphics, textLayoutRenderer, rectangle);
			_rightPanelRenderer.RenderContent(graphics, textLayoutRenderer, rightPanelRect);
			if (raiseLayoutEvent && _layoutCalculatedHandler != null)
			{
				_layoutCalculatedHandler(this, EventArgs.Empty);
			}
		}

		internal CommandBaseEditorDialog GetChildMenu()
		{
			return _childMenu;
		}

		internal void SetChildMenu(CommandBaseEditorDialog childMenu)
		{
			if (_childMenu != childMenu)
			{
				if (_childMenu != null)
				{
					NativeMethods.AnimateWindow(_childMenu.Handle, NativeMethods.PopupFadeMilliseconds, NativeMethods.AwHideWithFade);
					_childMenu.Dispose();
				}
				_childMenu = childMenu;
			}
		}

		internal void SetDesignerMouseActive(bool active)
		{
			if (_designer != null && GetChildMenu() != null)
			{
				active = false;
			}
			if (_isDesignerMouseActive != active && !_designerItemPressed)
			{
				_isDesignerMouseActive = active;
				RebuildPanelLayouts();
			}
		}

		internal CommandBaseRenderer GetHotRenderer()
		{
			return _hotRenderer;
		}

		internal void SetHotRenderer(CommandBaseRenderer commandBaseRenderer)
		{
			if (_hotRenderer == commandBaseRenderer)
			{
				return;
			}
			if (_hotRenderer != null)
			{
				_hotRenderer.StartPressAnimation();
				_hotRenderer.Hot = false;
				if (_hotRenderer.GetCommand() is IMouseEventInvoker)
				{
					((IMouseEventInvoker)_hotRenderer.GetCommand()).InvokeMouseLeave();
				}
			}
			if (commandBaseRenderer != null && commandBaseRenderer.GetCommand() is IMouseEventInvoker)
			{
				((IMouseEventInvoker)commandBaseRenderer.GetCommand()).InvokeMouseEnter();
			}
			_hotRenderer = commandBaseRenderer;
			if (_hotRenderer != null)
			{
				_hotRenderer.Hot = true;
			}
			ShowSuperTipFor(_hotRenderer);
		}

		internal CommandBaseRenderer GetPushedRenderer()
		{
			return _pushedRenderer;
		}

		internal void SetPushedRenderer(CommandBaseRenderer ribbonStatusBarCommandBaseRenderer)
		{
			if (_pushedRenderer != ribbonStatusBarCommandBaseRenderer)
			{
				if (_pushedRenderer != null)
				{
					_pushedRenderer.Pushed = false;
				}
				_pushedRenderer = ribbonStatusBarCommandBaseRenderer;
				if (_pushedRenderer != null)
				{
					_pushedRenderer.Pushed = true;
				}
				Invalidate();
			}
		}

		internal bool CanShowSizeGrip()
		{
			if (_showSizeGrip)
			{
				Control control = RibbonDrawingUtils.GetParentControl(this);
				if (control != null && control is Form)
				{
					Form form = control as Form;
					if (form.WindowState == FormWindowState.Normal && (form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual void OnCommandDropDownClosing(DropDownClosingEventArgs e)
		{
			if (base.Events[s_commandDropDownClosingKey] is DropDownClosingEventHandler dropDownClosingEventHandler)
			{
				dropDownClosingEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownClosed(CommandEventArgs e)
		{
			if (base.Events[s_commandDropDownClosedKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownOpening(CommandCancelEventArgs e)
		{
			if (base.Events[s_commandDropDownOpeningKey] is CommandCancelEventHandler commandCancelEventHandler)
			{
				commandCancelEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownOpened(CommandEventArgs e)
		{
			if (base.Events[s_commandDropDownOpenedKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClick(CommandEventArgs e)
		{
			if (base.Events[s_commandClickKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		internal void AddLayoutCalculatedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _layoutCalculatedHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(previousHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _layoutCalculatedHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		internal void RemoveLayoutCalculatedHandler(EventHandler handler)
		{
			EventHandler eventHandler = _layoutCalculatedHandler;
			EventHandler previousHandler;
			do
			{
				previousHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(previousHandler, handler);
				eventHandler = Interlocked.CompareExchange(ref _layoutCalculatedHandler, value, previousHandler);
			}
			while ((object)eventHandler != previousHandler);
		}

		internal void AddLayoutChangedHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(s_layoutChangedKey, eventHandler);
		}

		internal void RemoveLayoutChangedHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(s_layoutChangedKey, eventHandler);
		}

		protected internal virtual void OnLayoutChanged(EventArgs e)
		{
			if (base.Events[s_layoutChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal bool TryCloseChildMenu(CommandCloseReason commandCloseReason)
		{
			if (GetChildMenu() != null)
			{
				if (GetChildMenu().GetParentMenuCommand() != null && GetChildMenu().GetParentMenuCommand() is DropDownCommand)
				{
					DropDownCommand dropDownCommand = GetChildMenu().GetParentMenuCommand() as DropDownCommand;
					if (dropDownCommand.CanCloseDropDown(commandCloseReason))
					{
						SetChildMenu(null);
						return true;
					}
					return false;
				}
				SetChildMenu(null);
				return true;
			}
			return true;
		}

		internal void ShowSuperTipFor(CommandBaseRenderer commandBaseRenderer)
		{
			if (!ShowToolTips)
			{
				return;
			}
			ButtonCommand buttonCommand = null;
			if (commandBaseRenderer != null)
			{
				buttonCommand = commandBaseRenderer.GetCommand() as ButtonCommand;
			}
			if (buttonCommand == null || buttonCommand.SuperTipSettings.IsEmpty)
			{
				HideSuperTip();
			}
			else if (buttonCommand != _superTipCommand)
			{
				HideSuperTip();
				_superTipCommand = buttonCommand;
				string text = "";
				Point point = new Point(commandBaseRenderer._bounds.X, commandBaseRenderer._bounds.Bottom);
				point = ((commandBaseRenderer.GetRenderHost() == null) ? PointToScreen(point) : commandBaseRenderer.GetRenderHost().PointToScreen(point));
				SuperTipSettings superTipSettings = buttonCommand._superTipSettings.Clone();
				superTipSettings.HeaderText += text;
				_mordexSuperTip.InitialDelay = 900;
				if (_superTipDelay <= 0)
				{
					_mordexSuperTip.ShowAlways = true;
				}
				else
				{
					_mordexSuperTip.ShowAlways = false;
					_mordexSuperTip.AutoPopDelay = _superTipDelay;
				}
				_mordexSuperTip.BodyWidth = SuperTipWidth;
				_mordexSuperTip.Show(superTipSettings, this, point, _superTipDelay);
			}
		}

		internal void HideSuperTip()
		{
			if (_mordexSuperTip != null)
			{
				_mordexSuperTip.HideActiveToolTip();
			}
			_superTipCommand = null;
		}

		internal RibbonWindowMessageHandler GetWindowMessageHandler()
		{
			return _windowMessageHandler;
		}

		internal void SetWindowMessageHandler(RibbonWindowMessageHandler ribbonWindowMessageHandler)
		{
			_windowMessageHandler = ribbonWindowMessageHandler;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InvalidateLayout();
		}

		protected override void Dispose(bool disposing)
		{
			SetWindowMessageHandler(null);
			if (disposing)
			{
				HideSuperTip();
				if (_childMenu != null)
				{
					CommandBaseEditorDialog openMenu = _childMenu;
					_childMenu = null;
					openMenu.Dispose();
				}
				for (int i = 0; i < _leftPanelCommands.Count; i++)
				{
					_leftPanelCommands[i].SetParentContainer(null);
					_leftPanelCommands[i].CommandsOwner = null;
					_leftPanelCommands[i].Dispose();
				}
				for (int j = 0; j < _rightPanelCommands.Count; j++)
				{
					_rightPanelCommands[j].SetParentContainer(null);
					_rightPanelCommands[j].CommandsOwner = null;
					_rightPanelCommands[j].Dispose();
				}
				if (_mordexSuperTip != null)
				{
					_mordexSuperTip.Dispose();
					_mordexSuperTip = null;
				}
				if (_components != null)
				{
					_components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MxRibbonStatusBar));
			_designerImageList = new ImageList(_components);
			SuspendLayout();
			ImageStrip.Load(_designerImageList, componentResourceManager, "imlDesign.ImageStrip");
			_designerImageList.TransparentColor = Color.Transparent;
			_designerImageList.Images.SetKeyName(0, "Separator.ico");
			_designerImageList.Images.SetKeyName(1, "ButtonGroup.ico");
			_designerImageList.Images.SetKeyName(2, "CheckBox.ico");
			_designerImageList.Images.SetKeyName(3, "ComboBox.ico");
			_designerImageList.Images.SetKeyName(4, "Command.ico");
			_designerImageList.Images.SetKeyName(5, "CommandBox.ico");
			_designerImageList.Images.SetKeyName(6, "CommandDropDown.ico");
			_designerImageList.Images.SetKeyName(7, "ContainerControl.ico");
			_designerImageList.Images.SetKeyName(8, "Delete2.ico");
			_designerImageList.Images.SetKeyName(9, "EditBox.ico");
			_designerImageList.Images.SetKeyName(10, "Gallery.ico");
			_designerImageList.Images.SetKeyName(11, "Label.ico");
			_designerImageList.Images.SetKeyName(12, "MoveDown.ico");
			_designerImageList.Images.SetKeyName(13, "MoveUp.ico");
			ResumeLayout(performLayout: false);
		}
	}
}
