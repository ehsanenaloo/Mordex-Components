using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Mordex.Windows.TaskPane.Design
{
	public class DesignHelper
	{

		private static bool? _isDesignAssemblyAvailable = null;

		public static DialogResult ShowGroupsDesigner(MxTaskPane taskPane)
		{
			GroupsDesigner groupsDesigner = new GroupsDesigner();
			groupsDesigner.TaskPane = taskPane;
			DialogResult dialogResult = groupsDesigner.ShowDialog();
			groupsDesigner.Dispose();
			return dialogResult;
		}

		public static void ShowAboutBox(MxTaskPane taskPane)
		{
			TaskPaneAboutDialog taskPaneAboutDialog = new TaskPaneAboutDialog();
			taskPaneAboutDialog.ShowAbout(taskPane);
			taskPaneAboutDialog.Dispose();
		}

		public static Rectangle GetScrollBarBounds(MxTaskPane taskPane)
		{
			return taskPane._verticalScrollbar.Visible ? taskPane._verticalScrollbar.Bounds : Rectangle.Empty;
		}

		public static Control GetScrollBar(MxTaskPane taskPane)
		{
			return taskPane._verticalScrollbar;
		}

		public static bool ScrollBarCapture(MxTaskPane explorer)
		{
			return explorer._verticalScrollbar._pressedArea != -1;
		}

		public static void ReleaseScrollBarCapture(MxTaskPane explorer)
		{
			explorer._verticalScrollbar._pressedArea = -1;
		}

		internal static void EnsureDesignAssemblyLoaded()
		{
			if (!_isDesignAssemblyAvailable.HasValue)
			{
				try
				{
					Assembly.Load("Mordex.Windows.TaskPane.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null");
					_isDesignAssemblyAvailable = true;
				}
				catch (Exception ex) when (ex is System.IO.FileNotFoundException || ex is System.IO.FileLoadException || ex is BadImageFormatException)
				{
					_isDesignAssemblyAvailable = false;
				}
			}
			if (!_isDesignAssemblyAvailable.Value)
			{
				throw new InvalidOperationException("This Form uses a Mordex WinForms Control that was not properly installed in this machine.  The Form Designer was prevented from being shown in order to avoid a possible corruption of the Form Settings. Please install the Mordex WinForms Controls using the Controls Setup in order to edit this Form.");
			}
		}
	}
}
