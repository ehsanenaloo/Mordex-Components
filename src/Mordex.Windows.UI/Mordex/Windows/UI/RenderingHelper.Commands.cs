#pragma warning disable CS0618 // Bridges the obsolete Office2007 API to the current one.
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI
{
	internal partial class RenderingHelper
	{
		public static string GetShortcutText(Shortcut shortcut)
		{
			char c = (char)(shortcut & (Shortcut)KeyCodeMask);
			if (c >= '0' && c <= '9')
			{
				string text = "";
				int modifiers = (int)((long)shortcut & ModifierMask);
				if ((modifiers & (int)Keys.Control) != 0)
				{
					text += "Ctrl+";
				}
				if ((modifiers & (int)Keys.Shift) != 0)
				{
					text += "Shift+";
				}
				if ((modifiers & (int)Keys.Alt) != 0)
				{
					text += "Alt+";
				}
				return text + c;
			}
			return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys)shortcut);
		}

		public static int FindCommandImageIndex(ImageList imageList, CommandLayoutState uiCommandLayoutState, Mordex.Windows.UI.CommandBars.View view)
		{
			MxCommand uiCommand = uiCommandLayoutState.GetCommand();
			if (uiCommand != null && uiCommand.GetEffectiveImage() == null && imageList != null)
			{
				if (!((FormatStyleLayoutState)uiCommandLayoutState).IsEnabled())
				{
					if (view == Mordex.Windows.UI.CommandBars.View.SmallIcons)
					{
						if (uiCommand.GetEffectiveDisabledImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveDisabledImageKey()))
						{
							return imageList.Images.IndexOfKey(uiCommand.GetEffectiveDisabledImageKey());
						}
						if (uiCommand.GetEffectiveDisabledImageIndex() >= 0 && uiCommand.GetEffectiveDisabledImageIndex() < imageList.Images.Count)
						{
							return uiCommand.GetEffectiveDisabledImageIndex();
						}
					}
					else
					{
						if (uiCommand.GetEffectiveLargeDisabledImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveLargeDisabledImageKey()))
						{
							return imageList.Images.IndexOfKey(uiCommand.GetEffectiveLargeDisabledImageKey());
						}
						if (uiCommand.GetEffectiveLargeDisabledImageIndex() >= 0 && uiCommand.GetEffectiveLargeDisabledImageIndex() < imageList.Images.Count)
						{
							return uiCommand.GetEffectiveLargeDisabledImageIndex();
						}
					}
				}
				else
				{
					if (((FormatStyleLayoutState)uiCommandLayoutState).IsHot())
					{
						if (view == Mordex.Windows.UI.CommandBars.View.SmallIcons)
						{
							if (uiCommand.GetEffectiveHotImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveHotImageKey()))
							{
								return imageList.Images.IndexOfKey(uiCommand.GetEffectiveHotImageKey());
							}
							if (uiCommand.GetEffectiveHotImageIndex() >= 0 && uiCommand.GetEffectiveHotImageIndex() < imageList.Images.Count)
							{
								return uiCommand.GetEffectiveHotImageIndex();
							}
						}
						else
						{
							if (uiCommand.GetEffectiveLargeHotImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveLargeHotImageKey()))
							{
								return imageList.Images.IndexOfKey(uiCommand.GetEffectiveLargeHotImageKey());
							}
							if (uiCommand.GetEffectiveLargeHotImageIndex() >= 0 && uiCommand.GetEffectiveLargeHotImageIndex() < imageList.Images.Count)
							{
								return uiCommand.GetEffectiveLargeHotImageIndex();
							}
						}
					}
					if (uiCommand.IsChecked)
					{
						if (view == Mordex.Windows.UI.CommandBars.View.SmallIcons)
						{
							if (uiCommand.GetEffectiveSelectedImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveSelectedImageKey()))
							{
								return imageList.Images.IndexOfKey(uiCommand.GetEffectiveSelectedImageKey());
							}
							if (uiCommand.GetEffectiveSelectedImageIndex() >= 0 && uiCommand.GetEffectiveSelectedImageIndex() < imageList.Images.Count)
							{
								return uiCommand.GetEffectiveSelectedImageIndex();
							}
						}
						else
						{
							if (uiCommand.GetEffectiveLargeSelectedImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveLargeSelectedImageKey()))
							{
								return imageList.Images.IndexOfKey(uiCommand.GetEffectiveLargeSelectedImageKey());
							}
							if (uiCommand.GetEffectiveLargeSelectedImageIndex() >= 0 && uiCommand.GetEffectiveLargeSelectedImageIndex() < imageList.Images.Count)
							{
								return uiCommand.GetEffectiveLargeSelectedImageIndex();
							}
						}
					}
				}
				if (view == Mordex.Windows.UI.CommandBars.View.SmallIcons)
				{
					if (uiCommand.GetEffectiveImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveImageKey()))
					{
						return imageList.Images.IndexOfKey(uiCommand.GetEffectiveImageKey());
					}
					if (uiCommand.GetEffectiveImageIndex() >= 0 && uiCommand.GetEffectiveImageIndex() < imageList.Images.Count)
					{
						return uiCommandLayoutState.GetCommand().GetEffectiveImageIndex();
					}
				}
				else
				{
					if (uiCommand.GetEffectiveLargeImageKey().Length > 0 && imageList.Images.ContainsKey(uiCommand.GetEffectiveLargeImageKey()))
					{
						return imageList.Images.IndexOfKey(uiCommand.GetEffectiveLargeImageKey());
					}
					if (uiCommand.GetEffectiveLargeImageIndex() >= 0 && uiCommandLayoutState.GetCommand().GetEffectiveLargeImageIndex() < imageList.Images.Count)
					{
						return uiCommandLayoutState.GetCommand().GetEffectiveLargeImageIndex();
					}
				}
			}
			return -1;
		}

		public static Image GetCommandImage(ImageList imageList, CommandLayoutState uiCommandLayoutState, Mordex.Windows.UI.CommandBars.View view)
		{
			if (uiCommandLayoutState.GetCommand().GetEffectiveImage() != null)
			{
				return uiCommandLayoutState.GetCommand().GetEffectiveImage();
			}
			try
			{
				int imageIndex = FindCommandImageIndex(imageList, uiCommandLayoutState, view);
				if (imageIndex != -1)
				{
					return imageList.Images[imageIndex];
				}
			}
			catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
			{
				// The image list can change while a command paints; draw the command without an image.
				return null;
			}
			return null;
		}

		public static string GetCommandNodeText(object value)
		{
			string text = "";
			if (value is MxCommandManager)
			{
				text = "CommandManager";
			}
			else if (value is CommandBarCollection)
			{
				text = "CommandBars";
			}
			else if (value is MxCommandCollection)
			{
				text = "Commands";
			}
			else if (value is ContextMenuCollection)
			{
				text = "ContextMenus";
			}
			else if (value is MxCommandBar)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)value;
				text = "[" + uiCommandBar.GetBarIndex() + "]";
				if (uiCommandBar.Key.Length > 0)
				{
					text = text + " - '" + uiCommandBar.Key + "'";
				}
			}
			else if (value is MxContextMenu)
			{
				MxContextMenu uiContextMenu = (MxContextMenu)value;
				text = "[" + uiContextMenu.Index + "]";
				if (uiContextMenu.Key.Length > 0)
				{
					text = text + " - '" + uiContextMenu.Key + "'";
				}
			}
			else if (value is MxCommand)
			{
				MxCommand uiCommand = (MxCommand)value;
				text = "[" + uiCommand.GetLayoutIndex() + "]";
				if (uiCommand.Key.Length > 0)
				{
					text = text + " - '" + uiCommand.Key + "'";
				}
			}
			else if (value is CommandCategory)
			{
				CommandCategory uiCommandCategory = (CommandCategory)value;
				if (uiCommandCategory.CategoryName.Length > 0)
				{
					text = uiCommandCategory.CategoryName;
				}
			}
			return text;
		}
	}
}
