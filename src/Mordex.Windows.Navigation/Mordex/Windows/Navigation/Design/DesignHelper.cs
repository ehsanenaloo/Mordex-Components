using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation.Design
{
	public static class DesignHelper
	{

		private static bool? _isDesignAssemblyAvailable = null;

		public static void ShowAboutBox(MxNavigationBar navigationBar)
		{
			NavigationBarAboutDialog navigationBarAboutDialog = new NavigationBarAboutDialog();
			navigationBarAboutDialog.ShowAbout(navigationBar);
			navigationBarAboutDialog.Dispose();
		}

		public static DialogResult ShowGroupsDesigner(MxNavigationBar navigationBar)
		{
			GroupsDesigner groupsDesigner = new GroupsDesigner();
			groupsDesigner.NavigationBar = navigationBar;
			DialogResult dialogResult = groupsDesigner.ShowDialog();
			groupsDesigner.Dispose();
			return dialogResult;
		}

		public static Rectangle GetGroupBounds(NavigationGroup group)
		{
			return group.GetBounds();
		}

		public static void ScrollItemsUp(MxNavigationBar navigationBar)
		{
			navigationBar.ScrollToPreviousItem();
		}

		public static void ScrollItemsDown(MxNavigationBar navigationBar)
		{
			navigationBar.ScrollToNextItem();
		}

		public static bool IsPointOverScrollDown(MxNavigationBar navigationBar, Point pt)
		{
			if (navigationBar._scrollNextButton.IsButtonEnabled())
			{
				return navigationBar._scrollNextButton.Bounds.Contains(pt);
			}
			return false;
		}

		public static bool IsPointOverScrollUp(MxNavigationBar navigationBar, Point pt)
		{
			if (navigationBar._scrollPreviousButton.IsButtonEnabled())
			{
				return navigationBar._scrollPreviousButton.Bounds.Contains(pt);
			}
			return false;
		}

		internal static void EnsureDesignAssemblyLoaded()
		{
			if (!_isDesignAssemblyAvailable.HasValue)
			{
				try
				{
					Assembly.Load("Mordex.Windows.Navigation.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null");
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
