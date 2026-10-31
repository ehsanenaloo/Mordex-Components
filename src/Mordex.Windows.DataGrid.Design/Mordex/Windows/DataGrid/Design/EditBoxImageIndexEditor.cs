using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class EditBoxImageIndexEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		private ImageList imageList;

		private bool useImageKeys;

		private void FillImageIndexList(EditBase editBase, int imageIndex, ListBox editor)
		{
			editor.DrawItem += listBox_DrawItem;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			editor.ItemHeight = (int)editor.Font.GetHeight() + 4;
			int selectedIndex = -1;
			imageList = editBase.ImageList;
			editor.DisplayMember = "Text";
			editor.Items.Add(new GridValueListItem(-1, "(none)"));
			if (imageList != null)
			{
				for (int i = 0; i < imageList.Images.Count; i++)
				{
					editor.Items.Add(new GridValueListItem(i, "Image[" + i + "]"));
					if (i == imageIndex)
					{
						selectedIndex = editor.Items.Count - 1;
					}
				}
			}
			if (selectedIndex == -1)
			{
				editor.SelectedIndex = 0;
			}
			else
			{
				editor.SelectedIndex = selectedIndex;
			}
			editor.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		private void FillImageKeyList(EditBase editBase, string imageKey, ListBox editor)
		{
			editor.DrawItem += listBox_DrawItem;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			editor.ItemHeight = (int)editor.Font.GetHeight() + 4;
			int selectedIndex = -1;
			imageList = editBase.ImageList;
			editor.DisplayMember = "Text";
			editor.Items.Add(new GridValueListItem("", "(none)"));
			if (imageList != null)
			{
				for (int i = 0; i < imageList.Images.Keys.Count; i++)
				{
					string text = imageList.Images.Keys[i];
					editor.Items.Add(new GridValueListItem(text, text));
					if (string.Compare(text, imageKey, ignoreCase: true) == 0)
					{
						selectedIndex = editor.Items.Count - 1;
					}
				}
			}
			if (selectedIndex == -1)
			{
				editor.SelectedIndex = 0;
			}
			else
			{
				editor.SelectedIndex = selectedIndex;
			}
			editor.BorderStyle = System.Windows.Forms.BorderStyle.None;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null)
				{
					EditBase editBase = null;
					if (!(context.Instance is EditBase))
					{
						throw new InvalidOperationException();
					}
					editBase = (EditBase)context.Instance;
					using (ListBox listBox = new ListBox())
					{
						listBox.MouseUp += listBox_MouseUp;
						useImageKeys = !(value is int);
						if (value is int)
						{
							FillImageIndexList(editBase, (int)value, listBox);
						}
						else
						{
							FillImageKeyList(editBase, value as string, listBox);
						}
						edSvc.DropDownControl(listBox);
						if (listBox.SelectedItem != null)
						{
							value = ((GridValueListItem)listBox.SelectedItem).Value;
						}
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

		private void listBox_MouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (edSvc != null)
			{
				edSvc.CloseDropDown();
			}
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance is EditBase)
			{
				return true;
			}
			return false;
		}

		public override void PaintValue(PaintValueEventArgs e)
		{
			if (e.Context == null || e.Context.Instance == null)
			{
				return;
			}
			EditBase editBase = null;
			if (!(e.Context.Instance is EditBase))
			{
				return;
			}
			editBase = (EditBase)e.Context.Instance;
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
			int indexOfKey = -1;
			ImageList imageList = editBase.ImageList;
			if (imageList != null)
			{
				indexOfKey = e.Value is int index ? index : (e.Value is string key ? imageList.Images.IndexOfKey(key) : -1);
				if (indexOfKey >= 0 && indexOfKey < imageList.Images.Count)
				{
					imageList.Draw(e.Graphics, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height, indexOfKey);
				}
			}
		}

		internal static Rectangle DrawItemBorder(Graphics graphics, Pen pen, Rectangle rectangle)
		{
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			rectangle.Inflate(-1, -1);
			return rectangle;
		}

		private void DrawImageListItem(Graphics pen, Rectangle bounds, int imageIndex, Color foreColor, ImageList imageList)
		{
			Rectangle rectangle = DrawItemBorder(rectangle: new Rectangle(bounds.Left + 1, bounds.Top + 1, 18, bounds.Height - 2), graphics: pen, pen: SystemPens.WindowFrame);
			pen.FillRectangle(SystemBrushes.Window, rectangle);
			string text = "";
			if (imageIndex == -1)
			{
				text = "(none)";
			}
			else
			{
				if (imageList != null && imageIndex >= 0 && imageIndex < imageList.Images.Count)
				{
					imageList.Draw(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, imageIndex);
				}
				text = ((!useImageKeys) ? ("Image[" + imageIndex + "]") : (imageList != null && imageIndex < imageList.Images.Keys.Count ? imageList.Images.Keys[imageIndex] : ""));
			}
			if (text.Length > 0)
			{
				rectangle = new Rectangle(rectangle.Right + 1, bounds.Top, bounds.Width - rectangle.Width - 2, bounds.Height);
				using (Font font = new Font("Tahoma", 8f))
				{
					TextRenderer.DrawText(pen, text, font, rectangle, foreColor, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
				}
			}
		}

		private void listBox_DrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index == -1)
			{
				return;
			}
			GridValueListItem gridValueListItem = (GridValueListItem)((ListBox)sender).Items[eventArgs.Index];
			int imageIndex = -1;
			if (gridValueListItem.Value is int index)
			{
				imageIndex = index;
			}
			else if (imageList != null && gridValueListItem.Value is string key && key.Length > 0)
			{
				imageIndex = imageList.Images.IndexOfKey(key);
			}
			DrawImageListItem(eventArgs.Graphics, eventArgs.Bounds, imageIndex, eventArgs.ForeColor, imageList);
		}
	}
}
