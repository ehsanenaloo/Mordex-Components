using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.StatusBar;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ImageKeyEditor : UITypeEditor
	{
		private const int ItemVerticalPadding = 4;

		private const int ThumbnailWidth = 18;

		private const float ItemFontSize = 8f;

		private IWindowsFormsEditorService _editorService;

		private ImageList _imageList;

		private void FillImageList(ImageList imageList, string selectedKey, ListBox listBox)
		{
			listBox.DrawItem += OnControlDrawItem;
			listBox.DrawMode = DrawMode.OwnerDrawFixed;
			listBox.ItemHeight = (int)listBox.Font.GetHeight() + ItemVerticalPadding;
			int selectedIndex = -1;
			_imageList = imageList;
			listBox.Items.Add(new ON("", "(none)"));
			if (imageList != null)
			{
				for (int i = 0; i < imageList.Images.Count; i++)
				{
					string text = imageList.Images.Keys[i];
					listBox.Items.Add(new ON(text, text));
					if (text == selectedKey)
					{
						selectedIndex = listBox.Items.Count - 1;
					}
				}
			}
			if (selectedIndex == -1)
			{
				listBox.SelectedIndex = 0;
			}
			else
			{
				listBox.SelectedIndex = selectedIndex;
			}
			listBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				_editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_editorService != null)
				{
					ImageList imageList = null;
					object owner = null;
					owner = ((!(context.Instance is ICustomTypeDescriptor)) ? context.Instance : ((ICustomTypeDescriptor)context.Instance).GetPropertyOwner(context.PropertyDescriptor));
					if (owner is MxCommand)
					{
						imageList = ((MxCommand)owner).GetHostElement()?.GetImageList(smallIcons: true, null);
					}
					else if (owner is MxStatusBarPanel)
					{
						imageList = ((MxStatusBarPanel)owner)._owner?._statusBar?.ImageList;
					}
					else if (owner is MxTabPage)
					{
						imageList = ((MxTabPage)owner).TabControl?.ImageList;
					}
					else if (owner is DockPanelBase)
					{
						imageList = ((DockPanelBase)owner).PanelManager?.ImageList;
					}
					else if (owner is MxButton)
					{
						imageList = ((MxButton)owner).ImageList;
					}
					else if (owner is MxGroupBox)
					{
						imageList = ((MxGroupBox)owner).ImageList;
					}
					else if (owner is CheckBase)
					{
						imageList = ((CheckBase)owner).ImageList;
					}
					else
					{
						if (!(owner is ComboBoxItem))
						{
							throw new InvalidOperationException();
						}
						imageList = ((ComboBoxItem)owner).GetOwnerComboBase().ImageList;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					FillImageList(imageList, (string)value, listBox);
					_editorService.DropDownControl(listBox);
					if (listBox.SelectedItem != null)
					{
						value = ((ON)listBox.SelectedItem).Object;
					}
				}
			}
			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.DropDown;
			}
			return base.GetEditStyle(context);
		}

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_editorService != null)
			{
				_editorService.CloseDropDown();
			}
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			if (context != null)
			{
				object owner = null;
				owner = ((!(context.Instance is ICustomTypeDescriptor)) ? context.Instance : ((ICustomTypeDescriptor)context.Instance).GetPropertyOwner(context.PropertyDescriptor));
				if (owner is MxCommand)
				{
					return true;
				}
				if (owner is MxStatusBarPanel)
				{
					return true;
				}
				if (owner is MxTabPage)
				{
					return true;
				}
				if (owner is DockPanelBase)
				{
					return true;
				}
				if (owner is MxButton)
				{
					return true;
				}
				if (owner is MxGroupBox)
				{
					return true;
				}
				if (owner is CheckBase)
				{
					return true;
				}
				if (owner is ComboBoxItem)
				{
					return true;
				}
			}
			return false;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			if (e.Context == null || e.Context.Instance == null)
			{
				return;
			}
			ImageList imageList = null;
			object owner = null;
			owner = ((!(e.Context.Instance is ICustomTypeDescriptor)) ? e.Context.Instance : ((ICustomTypeDescriptor)e.Context.Instance).GetPropertyOwner(e.Context.PropertyDescriptor));
			if (owner is MxCommand)
			{
				imageList = ((MxCommand)owner).GetHostElement()?.GetImageList(smallIcons: true, null);
			}
			else if (owner is MxStatusBarPanel)
			{
				imageList = ((MxStatusBarPanel)owner)._owner?._statusBar?.ImageList;
			}
			else if (owner is MxTabPage)
			{
				imageList = ((MxTabPage)owner).TabControl?.ImageList;
			}
			else if (owner is DockPanelBase)
			{
				imageList = ((DockPanelBase)owner).PanelManager?.ImageList;
			}
			else if (owner is MxButton)
			{
				imageList = ((MxButton)owner).ImageList;
			}
			else if (owner is MxGroupBox)
			{
				imageList = ((MxGroupBox)owner).ImageList;
			}
			else if (owner is CheckBase)
			{
				imageList = ((CheckBase)owner).ImageList;
			}
			else if (owner is ComboBoxItem)
			{
				imageList = ((ComboBoxItem)owner).GetOwnerComboBase().ImageList;
			}
			if (imageList == null)
			{
				return;
			}
			string text = (string)e.Value;
			if (text.Length > 0 && imageList.Images.ContainsKey(text))
			{
				try
				{
					Image image = imageList.Images[text];
					e.Graphics.DrawImage(image, e.Bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
					return;
				}
				catch
				{
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}

		private void DrawImageEntry(Graphics graphics, Rectangle itemBounds, string imageKey, Color color, ImageList imageList)
		{
			Rectangle rectangle = new Rectangle(itemBounds.Left + 1, itemBounds.Top + 1, ThumbnailWidth, itemBounds.Height - 2);
			rectangle = RenderingHelper.DrawRectangleOutline(graphics, SystemPens.WindowFrame, rectangle);
			graphics.FillRectangle(SystemBrushes.Window, rectangle);
			string text = "";
			if (imageKey.Length == 0)
			{
				text = "(none)";
			}
			else
			{
				if (imageList != null)
				{
					graphics.DrawImage(imageList.Images[imageKey], rectangle, 0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
				text = imageKey;
			}
			if (text.Length > 0)
			{
				Font font = new Font("Tahoma", ItemFontSize);
				rectangle = new Rectangle(rectangle.Right + 1, itemBounds.Top, itemBounds.Width - rectangle.Width - 2, itemBounds.Height);
				StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap);
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				Brush brush = new SolidBrush(color);
				graphics.DrawString(text, font, brush, rectangle, stringFormat);
				brush.Dispose();
				brush = null;
				font.Dispose();
			}
		}

		private void OnControlDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index != -1)
			{
				ON oN = (ON)((ListBox)sender).Items[eventArgs.Index];
				string text = (string)oN.Object;
				DrawImageEntry(eventArgs.Graphics, eventArgs.Bounds, text, eventArgs.ForeColor, _imageList);
			}
		}
	}
}
