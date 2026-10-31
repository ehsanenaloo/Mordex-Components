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
		private void OnContainerControlLayout(object sender, LayoutEventArgs eventArgs)
		{
			OrderContainerControls();
		}

		private void OnContainerControlSystemColorsChanged(object sender, EventArgs eventArgs)
		{
			RefreshAppearance();
			OnSystemColorsChanged(eventArgs);
		}

		private void RefreshAppearance()
		{
			_commandBarsAppearanceState.ApplyVisualStyle();
			_isToolbarsMenuDirty = true;
			RefreshRebars();
		}

		internal Form GetForm()
		{
			return _form;
		}

		internal void UpdateForm(Form form)
		{
			if (_form == form)
			{
				return;
			}
			if (_form != null)
			{
				foreach (Control formControl in _form.Controls)
				{
					if (formControl is MdiClient)
					{
						MdiClientHook.RemoveMenuUser((MdiClient)formControl);
					}
				}
				Application.RemoveMessageFilter(this);
				_form.MdiChildActivate -= OnFormMdiChildActivate;
				_form.ControlAdded -= OnFormControlAdded;
				_form.ControlRemoved -= OnFormControlRemoved;
				_form.Activated -= OnFormActivated;
			}
			_form = form;
			if (_form == null)
			{
				return;
			}
			Application.AddMessageFilter(this);
			foreach (Control childControl in _form.Controls)
			{
				if (childControl is MdiClient)
				{
					MdiClientHook.AddMenuUser((MdiClient)childControl);
					break;
				}
			}
			_form.MdiChildActivate += OnFormMdiChildActivate;
			_form.ControlAdded += OnFormControlAdded;
			_form.ControlRemoved += OnFormControlRemoved;
			_form.Activated += OnFormActivated;
		}

		private MxCommandManager FindCommandManagerInForm(Form parentForm)
		{
			if (parentForm == null)
			{
				return null;
			}
			foreach (Control control in parentForm.Controls)
			{
				if (control is MxCommandBarHost)
				{
					return ((MxCommandBarHost)control).CommandManager;
				}
			}
			Form[] ownedForms = parentForm.OwnedForms;
			foreach (Form form in ownedForms)
			{
				if (form is CommandBarEditorDialog)
				{
					return ((CommandBarEditorDialog)form).GetCommandBar()?.CommandManager;
				}
			}
			return null;
		}

		private void OnFormActivated(object sender, EventArgs eventArgs)
		{
			if (GetActiveBar() != null && GetActiveBar().GetChildMenu() == null)
			{
				SetActiveBar(null);
			}
		}

		private void OnFormControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			if (eventArgs.Control is MdiClient)
			{
				MdiClientHook.RemoveMenuUser((MdiClient)eventArgs.Control);
			}
		}

		private void OnFormControlAdded(object sender, ControlEventArgs eventArgs)
		{
			if (!(eventArgs.Control is MdiClient))
			{
				return;
			}
			foreach (MxCommandBar commandBar in CommandBars)
			{
				if (commandBar.CommandBarType == CommandBarType.Menu)
				{
					MdiClientHook.AddMenuUser((MdiClient)eventArgs.Control);
					break;
				}
			}
		}

		private void OnFormMdiChildActivate(object sender, EventArgs eventArgs)
		{
			if (_containerControl != GetForm() || !GetForm().IsMdiContainer)
			{
				return;
			}
			if (GetForm().ActiveMdiChild != null && AllowMerge)
			{
				MxCommandManager uiCommandManager = FindCommandManagerInForm(GetForm().ActiveMdiChild);
				if (uiCommandManager != null && uiCommandManager.AllowMerge)
				{
					SetMergedManager(uiCommandManager);
					RefreshRebars();
					return;
				}
			}
			if (AllowMerge)
			{
				SetMergedManager(null);
			}
			if (FindMenuBar() == null)
			{
				GetForm().Width++;
				GetForm().Width--;
			}
		}

		private void OnContainerControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (_containerControl != null)
			{
				if (_containerControl is Form)
				{
					UpdateForm(_containerControl as Form);
				}
				UpdateForm(RenderingHelper.FindForm(_containerControl));
			}
			else
			{
				UpdateForm(null);
			}
		}

		private void OnContainerControlResize(object sender, EventArgs eventArgs)
		{
			RefreshRebars();
		}

		internal EditorDialogCollection GetEditorDialogs()
		{
			if (_editorDialogs == null)
			{
				_editorDialogs = new EditorDialogCollection(this);
			}
			return _editorDialogs;
		}

		internal string GetUniqueCommandBarName(string requestedName)
		{
			if (IsCommandBarKeyUnique(null, requestedName))
			{
				return requestedName;
			}
			int suffix = 0;
			string text = "uiCommandBar" + suffix;
			while (!IsCommandBarKeyUnique(null, text))
			{
				suffix++;
				text = "uiCommandBar" + suffix;
			}
			return text;
		}
	}
}
