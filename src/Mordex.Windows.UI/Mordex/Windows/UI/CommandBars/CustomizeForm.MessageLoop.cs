using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class CustomizeForm
	{
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmActivateApp)
			{
				if (_tipHandler != null)
				{
					DisposeTipHandler();
				}
				if (!(m.WParam != IntPtr.Zero))
				{
					foreach (MxCommandBar commandBar in CommandManager.CommandBars)
					{
						commandBar.SetChildMenu(null);
					}
				}
			}
			base.WndProc(ref m);
		}

		private bool IsDialogWindow(IntPtr windowHandle)
		{
			if (NativeMethods.IsChild(base.Handle, windowHandle))
			{
				return true;
			}
			if (((IDropListOwner)jcboAnimation).DropWindowHandle == windowHandle)
			{
				return true;
			}
			return IsEditorDialogWindow(windowHandle);
		}

		private bool IsEditorDialogWindow(IntPtr windowHandle)
		{
			foreach (CommandBarEditorDialog item in _commandManager.GetEditorDialogs())
			{
				if (item.Handle == windowHandle)
				{
					return true;
				}
			}
			return false;
		}

		private void UpdateDragCursor()
		{
			foreach (MxCommandBar commandBar in _commandManager.CommandBars)
			{
				Point pt = commandBar.PointToClient(Cursor.Position);
				if (commandBar._gripItem != null && commandBar._gripItem._bounds.Contains(pt))
				{
					if (_previousCursor == IntPtr.Zero)
					{
						_previousCursor = NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, CursorSizeAll));
					}
					return;
				}
			}
			NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, CursorArrow));
			_previousCursor = IntPtr.Zero;
		}

		internal void RunMessageLoop()
		{
			try
			{
				NativeMethods.NativeMsg nativeMessage = default(NativeMethods.NativeMsg);
				while (!_isLoopDone)
				{
					if (!NativeMethods.WaitMessage())
					{
						_isLoopDone = true;
						break;
					}
					while (!_isLoopDone && NativeMethods.PeekMessage(ref nativeMessage, IntPtr.Zero, 0u, 0u, 0u))
					{
						bool shouldSwallow = false;
						if (base.Handle != nativeMessage._hwnd && !IsDialogWindow(nativeMessage._hwnd))
						{
							switch (nativeMessage._message)
							{
							case WmMouseMove:
								UpdateDragCursor();
								shouldSwallow = true;
								RenderingHelper.SetMouseCapture(this, true);
								break;
							case WmNcLButtonUp:
							case WmNcRButtonUp:
							case WmNcMButtonUp:
							case WmNcXButtonUp:
							case WmLButtonUp:
							case WmRButtonUp:
							case WmMButtonUp:
							case WmXButtonUp:
								shouldSwallow = true;
								break;
							case WmLButtonDown:
								shouldSwallow = true;
								break;
							case WmNcLButtonDown:
							case WmNcRButtonDown:
							case WmNcMButtonDown:
							case WmNcXButtonDown:
							case WmRButtonDown:
							case WmMButtonDown:
							case WmXButtonDown:
								shouldSwallow = true;
								break;
							case WmKeyDown:
							case WmKeyUp:
							case WmSysKeyDown:
							case WmSysKeyUp:
								shouldSwallow = true;
								break;
							case WmSetCursor:
								shouldSwallow = true;
								break;
							}
						}
						else if (base.Handle == nativeMessage._hwnd)
						{
							Point position = Cursor.Position;
							IntPtr intPtr = NativeMethods.WindowFromPoint(new NativeMethods.NativePoint
							{
								_x = position.X,
								_y = position.Y
							});
							bool isOverForm = intPtr == base.Handle || NativeMethods.IsChild(base.Handle, intPtr);
							if (!base.Bounds.Contains(position) || !isOverForm)
							{
								switch (nativeMessage._message)
								{
								case WmMouseMove:
									if (GetSelectedBar() != null && GetSelectedBar()._messageFilter != null)
									{
										_selectedBar._messageFilter.UpdateDrag(Cursor.Position);
										break;
									}
									shouldSwallow = true;
									foreach (CommandBarEditorDialog item in _commandManager.GetEditorDialogs())
									{
										if (item.Bounds.Contains(position))
										{
											if (!item.GetCommandBar().ContainsScreenPoint(position))
											{
												RenderingHelper.SetMouseCapture(this, false);
												shouldSwallow = false;
											}
											break;
										}
									}
									if (shouldSwallow)
									{
										UpdateDragCursor();
										RenderingHelper.SetMouseCapture(this, true);
									}
									break;
								case WmNcLButtonUp:
								case WmNcRButtonUp:
								case WmNcMButtonUp:
								case WmNcXButtonUp:
								case WmLButtonUp:
								case WmRButtonUp:
								case WmMButtonUp:
								case WmXButtonUp:
									if (GetSelectedBar() != null && GetSelectedBar()._messageFilter != null)
									{
										GetSelectedBar()._messageFilter.EndDrag();
									}
									shouldSwallow = true;
									break;
								case WmLButtonDown:
								{
									Point clickPosition = Cursor.Position;
									shouldSwallow = true;
									bool isBarHit = false;
									if (GetSelectedBar() != null && _selectedBar.ContainsScreenPoint(clickPosition))
									{
										_selectedBar.BeginCustomizeDrag(clickPosition);
										RenderingHelper.SetMouseCapture(this, true);
										isBarHit = true;
										break;
									}
									if (!isBarHit)
									{
										foreach (MxCommandBar commandBar in CommandManager.CommandBars)
										{
											if (commandBar.ContainsScreenPoint(clickPosition))
											{
												SetSelectedBar(commandBar);
												_selectedBar.BeginCustomizeDrag(clickPosition);
												RenderingHelper.SetMouseCapture(this, true);
												isBarHit = true;
												break;
											}
										}
									}
									if (isBarHit)
									{
										break;
									}
									foreach (CommandBarEditorDialog editorDialog in CommandManager.GetEditorDialogs())
									{
										if (editorDialog.Bounds.Contains(clickPosition))
										{
											RenderingHelper.SetMouseCapture(this, false);
											shouldSwallow = false;
										}
									}
									break;
								}
								case WmRButtonDown:
								{
									shouldSwallow = true;
									Point clickPosition = Cursor.Position;
									if (GetSelectedBar() != null && _selectedBar.ContainsScreenPoint(clickPosition))
									{
										RenderingHelper.SetMouseCapture(this, true);
										NativeMethods.NativeMsg consumedMessage = default(NativeMethods.NativeMsg);
										NativeMethods.TryGetMessage(ref consumedMessage);
										_selectedBar.SelectCustomizeCommandAt(Cursor.Position);
										RenderingHelper.SetMouseCapture(this, true);
										break;
									}
									foreach (MxCommandBar hitBar in CommandManager.CommandBars)
									{
										if (hitBar.ContainsScreenPoint(clickPosition))
										{
											RenderingHelper.SetMouseCapture(this, true);
											NativeMethods.NativeMsg consumedBarMessage = default(NativeMethods.NativeMsg);
											NativeMethods.TryGetMessage(ref consumedBarMessage);
											SetSelectedBar(hitBar);
											_selectedBar.SelectCustomizeCommandAt(clickPosition);
											RenderingHelper.SetMouseCapture(this, true);
											break;
										}
									}
									break;
								}
								case WmNcLButtonDown:
								case WmNcRButtonDown:
								case WmNcMButtonDown:
								case WmNcXButtonDown:
								case WmMButtonDown:
								case WmXButtonDown:
									shouldSwallow = true;
									break;
								}
							}
							else if (nativeMessage._message == WmMouseMove)
							{
								if (!IsEditorDialogWindow(nativeMessage._hwnd))
								{
									RenderingHelper.SetMouseCapture(this, false);
								}
							}
							else if (_tipHandler != null)
							{
								switch (nativeMessage._message)
								{
								case WmNcLButtonUp:
								case WmNcRButtonUp:
								case WmNcMButtonUp:
								case WmNcXButtonUp:
								case WmLButtonUp:
								case WmRButtonUp:
								case WmMButtonUp:
								case WmXButtonUp:
									DisposeTipHandler();
									break;
								}
							}
							else if (nativeMessage._message == WmKeyDown)
							{
								KeyEventArgs e = new KeyEventArgs(Keys.N | Keys.Alt);
								base.OnKeyDown(e);
							}
						}
						else if (nativeMessage._message == WmMouseMove)
						{
							if (!IsEditorDialogWindow(nativeMessage._hwnd))
							{
								RenderingHelper.SetMouseCapture(this, false);
							}
						}
						else if (_tipHandler != null)
						{
							switch (nativeMessage._message)
							{
							case WmNcLButtonUp:
							case WmNcRButtonUp:
							case WmNcMButtonUp:
							case WmNcXButtonUp:
							case WmLButtonUp:
							case WmRButtonUp:
							case WmMButtonUp:
							case WmXButtonUp:
								if (nativeMessage._hwnd != btnDescription.Handle)
								{
									DisposeTipHandler();
								}
								break;
							}
						}
						if (shouldSwallow)
						{
							NativeMethods.NativeMsg swallowedMessage = default(NativeMethods.NativeMsg);
							if (!NativeMethods.TryGetMessage(ref swallowedMessage))
							{
								_isLoopDone = true;
							}
						}
						else
						{
							if (!NativeMethods.TryGetMessage(ref nativeMessage))
							{
								_isLoopDone = true;
								break;
							}
							if (nativeMessage._message == WmKeyDown || nativeMessage._message == WmChar)
							{
								NativeMethods.TranslateMessage(ref nativeMessage);
								NativeMethods.DispatchMessage(ref nativeMessage);
								return;
							}
							if (!NativeMethods.IsDialogMessage(base.Handle, ref nativeMessage))
							{
								Message msg = Message.Create(nativeMessage._hwnd, nativeMessage._message, nativeMessage._wParam, nativeMessage._lParam);
								if (PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
								{
									NativeMethods.TranslateMessage(ref nativeMessage);
									NativeMethods.DispatchMessage(ref nativeMessage);
								}
							}
						}
					}
				}
			}
			catch (ObjectDisposedException)
			{
				// The form was closed while the loop was running; that simply ends the loop.
				_isLoopDone = true;
			}
			catch
			{
				_isLoopDone = true;
				throw;
			}
		}
	}
}
