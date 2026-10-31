using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Ribbon.Design
{
	// Shared code of the image index and image key drop-down editors.
	internal static class ImageListEditorSupport
	{
		private const string NoneText = "(none)";

		private const string ListFontName = "Tahoma";

		private const float ListFontSize = 8f;

		private const int ThumbnailWidth = 18;

		private const int ItemHeightPadding = 4;

		internal static object GetPropertyOwner(ITypeDescriptorContext context)
		{
			if (context.Instance is ICustomTypeDescriptor descriptor)
			{
				return descriptor.GetPropertyOwner(context.PropertyDescriptor);
			}
			return context.Instance;
		}

		internal static bool IsSmallImageOwner(object owner)
		{
			return owner is GalleryItem || owner is CommandBase || owner is RibbonTab || owner is RibbonContextualTab;
		}

		internal static ImageList GetSmallImageList(object owner)
		{
			switch (owner)
			{
			case GalleryItem galleryItem:
				return galleryItem.GetParentContainer() is Gallery gallery ? gallery.ImageList : null;
			case RibbonTab tab:
				return tab.Ribbon?.ImageList;
			case RibbonContextualTab contextualTab:
				return contextualTab.Ribbon?.ImageList;
			case CommandBase command:
				if (command.Ribbon != null)
				{
					return command.Ribbon.ImageList;
				}
				if (command.GetOwningStatusBar() != null)
				{
					return command.GetOwningStatusBar().ImageList;
				}
				return command.FindContextMenuOwner()?.ImageList;
			default:
				return null;
			}
		}

		internal static ImageList GetLargeImageList(object owner)
		{
			if (!(owner is CommandBase command))
			{
				return null;
			}
			if (command.Ribbon != null)
			{
				return command.Ribbon.LargeImageList;
			}
			return command.FindContextMenuOwner()?.LargeImageList;
		}

		internal static object ShowDropDown(IWindowsFormsEditorService service, ImageList imageList, object currentValue, bool byKey)
		{
			using ListBox listBox = new ListBox();
			listBox.DrawMode = DrawMode.OwnerDrawFixed;
			listBox.ItemHeight = (int)listBox.Font.GetHeight() + ItemHeightPadding;
			listBox.BorderStyle = BorderStyle.None;
			listBox.MouseUp += (sender, e) => service.CloseDropDown();
			listBox.DrawItem += (sender, e) => DrawListItem(listBox, e, imageList, byKey);
			int selectedIndex = 0;
			listBox.Items.Add(new DictionaryEntry(NoneText, byKey ? null : (object)(-1)));
			if (imageList != null)
			{
				for (int i = 0; i < imageList.Images.Count; i++)
				{
					string key = byKey ? imageList.Images.Keys[i] : null;
					object itemValue = byKey ? key : (object)i;
					listBox.Items.Add(new DictionaryEntry(byKey ? key : "Image[" + i + "]", itemValue));
					if (Equals(itemValue, currentValue))
					{
						selectedIndex = listBox.Items.Count - 1;
					}
				}
			}
			listBox.SelectedIndex = selectedIndex;
			service.DropDownControl(listBox);
			if (listBox.SelectedItem != null)
			{
				return ((DictionaryEntry)listBox.SelectedItem).Value;
			}
			return currentValue;
		}

		internal static void PaintValue(PaintValueEventArgs e, ImageList imageList, bool byKey)
		{
			if (imageList == null)
			{
				return;
			}
			using (Image image = FindImage(imageList, e.Value, byKey))
			{
				if (image != null)
				{
					try
					{
						e.Graphics.DrawImage(image, e.Bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
						return;
					}
					catch (Exception ex) when (ex is ArgumentException || ex is ExternalException)
					{
						// Draw failures fall back to the plain background below.
					}
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}

		// ImageList.Images returns a new copy on every call, so the caller owns the result.
		private static Image FindImage(ImageList imageList, object value, bool byKey)
		{
			if (byKey)
			{
				return value is string key && key.Length > 0 && imageList.Images.ContainsKey(key) ? imageList.Images[key] : null;
			}
			return value is int index && index >= 0 && index < imageList.Images.Count ? imageList.Images[index] : null;
		}

		private static void DrawListItem(ListBox listBox, DrawItemEventArgs e, ImageList imageList, bool byKey)
		{
			e.DrawBackground();
			if (e.Index < 0)
			{
				return;
			}
			DictionaryEntry entry = (DictionaryEntry)listBox.Items[e.Index];
			string text = (string)entry.Key;
			if (text.Length == 0)
			{
				text = NoneText;
			}
			Rectangle bounds = e.Bounds;
			Rectangle thumbnail = new Rectangle(bounds.Left + 1, bounds.Top + 1, ThumbnailWidth, bounds.Height - 2);
			thumbnail = RibbonDrawingUtils.DrawRectangleOutline(e.Graphics, SystemPens.WindowFrame, thumbnail);
			e.Graphics.FillRectangle(SystemBrushes.Window, thumbnail);
			if (imageList != null)
			{
				using Image image = FindImage(imageList, entry.Value, byKey);
				if (image != null)
				{
					e.Graphics.DrawImage(image, thumbnail, 0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
			}
			Rectangle textBounds = new Rectangle(thumbnail.Right + 1, bounds.Top, bounds.Width - thumbnail.Width - 2, bounds.Height);
			using Font font = new Font(ListFontName, ListFontSize);
			using StringFormat format = new StringFormat(StringFormatFlags.NoWrap)
			{
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Center
			};
			using Brush brush = new SolidBrush(e.ForeColor);
			e.Graphics.DrawString(text, font, brush, textBounds, format);
		}
	}
}
