using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.DataGrid.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ImageIndexEditor : UITypeEditor
	{
		private IWindowsFormsEditorService edSvc;

		private ImageList imageList;

		private Image _defaultItemImage;

		private bool useImageKeys;

		private void FillImageIndexList(MxDataGrid grid, int imageIndex, Image defaultImage, ListBox editor)
		{
			editor.DrawItem += listBox_DrawItem;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			editor.ItemHeight = (int)editor.Font.GetHeight() + 4;
			int selectedIndex = -1;
			this._defaultItemImage = defaultImage;
			imageList = DesignHelper.GetImageList(grid);
			editor.DisplayMember = "Text";
			if (defaultImage != null)
			{
				editor.Items.Add(new GridValueListItem(-2, "(Default)"));
			}
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

		private void FillImageKeyList(MxDataGrid grid, string imageKey, ListBox editor)
		{
			editor.DrawItem += listBox_DrawItem;
			editor.DrawMode = DrawMode.OwnerDrawFixed;
			editor.ItemHeight = (int)editor.Font.GetHeight() + 4;
			int selectedIndex = -1;
			imageList = DesignHelper.GetImageList(grid);
			editor.DisplayMember = "Text";
			editor.Items.Clear();
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
					MxDataGrid grid = null;
					object propertyOwner = context.Instance;
					if (context.Instance is ICustomTypeDescriptor)
					{
						propertyOwner = ((ICustomTypeDescriptor)context.Instance).GetPropertyOwner(context.PropertyDescriptor);
					}
					if (propertyOwner is GridColumn)
					{
						grid = ((GridColumn)propertyOwner).DataGrid;
					}
					else if (propertyOwner is GridTable)
					{
						grid = ((GridTable)propertyOwner).DataGrid;
					}
					else if (propertyOwner is GridColumnSet)
					{
						GridTable table = ((GridColumnSet)propertyOwner).Table;
						if (table != null)
						{
							grid = table.DataGrid;
						}
					}
					else
					{
						if (!(propertyOwner is MxDataGrid))
						{
							throw new InvalidOperationException();
						}
						grid = (MxDataGrid)propertyOwner;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += listBox_MouseUp;
					Image image = null;
					if (context.PropertyDescriptor.Name == "ErrorImageIndex")
					{
						Bitmap errorImage = DesignHelper.ErrorImage;
						errorImage.MakeTransparent(Color.Magenta);
						image = errorImage;
					}
					useImageKeys = !(value is int);
					if (value is int)
					{
						FillImageIndexList(grid, (int)value, image, listBox);
					}
					else
					{
						FillImageKeyList(grid, value as string, listBox);
					}
					try
					{
						edSvc.DropDownControl(listBox);
						if (listBox.SelectedItem != null)
						{
							value = ((GridValueListItem)listBox.SelectedItem).Value;
						}
					}
					finally
					{
						listBox.Dispose();
						image?.Dispose();
						_defaultItemImage = null;
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
			if (context != null)
			{
				if (context.Instance is GridColumn)
				{
					if (((GridColumn)context.Instance).DataGrid != null)
					{
						return true;
					}
				}
				else if (context.Instance is GridTable)
				{
					if (((GridTable)context.Instance).DataGrid != null)
					{
						return true;
					}
				}
				else if (context.Instance is GridColumnSet)
				{
					GridTable table = ((GridColumnSet)context.Instance).Table;
					if (table != null && table.DataGrid != null)
					{
						return true;
					}
				}
				else if (context.Instance is MxDataGrid)
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
			MxDataGrid dataGrid = null;
			if (e.Context.Instance is GridColumn)
			{
				GridColumn gridColumn = (GridColumn)e.Context.Instance;
				dataGrid = gridColumn.DataGrid;
			}
			else if (e.Context.Instance is GridTable)
			{
				dataGrid = ((GridTable)e.Context.Instance).DataGrid;
			}
			else if (e.Context.Instance is GridColumnSet)
			{
				GridTable table = ((GridColumnSet)e.Context.Instance).Table;
				if (table != null)
				{
					dataGrid = table.DataGrid;
				}
			}
			else if (e.Context.Instance is MxDataGrid)
			{
				dataGrid = (MxDataGrid)e.Context.Instance;
			}
			if (dataGrid == null)
			{
				return;
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
			ImageList imageList = DesignHelper.GetImageList(dataGrid);
			int indexOfKey = -1;
			if (e.Value is int)
			{
				indexOfKey = (int)e.Value;
			}
			else if (e.Value is string && imageList != null && imageList.Images.ContainsKey((string)e.Value))
			{
				indexOfKey = imageList.Images.IndexOfKey((string)e.Value);
			}
			if (imageList != null)
			{
				if (indexOfKey >= 0 && indexOfKey < imageList.Images.Count)
				{
					imageList.Draw(e.Graphics, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height, indexOfKey);
				}
			}
			if (indexOfKey == -2)
			{
				using (Bitmap errorImage = DesignHelper.ErrorImage)
				{
					errorImage.MakeTransparent(Color.Magenta);
					e.Graphics.DrawImage(errorImage, e.Bounds, 0, 0, errorImage.Width, errorImage.Height, GraphicsUnit.Pixel);
				}
			}
		}

		private void DrawImageIndexItem(Graphics pen, Rectangle bounds, int imageIndex, Color foreColor, ImageList imageList)
		{
			Rectangle rectangle = EditBoxImageIndexEditor.DrawItemBorder(rectangle: new Rectangle(bounds.Left + 1, bounds.Top + 1, 18, bounds.Height - 2), graphics: pen, pen: SystemPens.WindowFrame);
			pen.FillRectangle(SystemBrushes.Window, rectangle);
			string text = "";
			switch (imageIndex)
			{
			case -2:
				pen.DrawImage(_defaultItemImage, rectangle, 0, 0, 16, 16, GraphicsUnit.Pixel);
				text = "(default)";
				break;
			case -1:
				text = "(none)";
				break;
			default:
				if (imageList != null && imageIndex >= 0 && imageIndex < imageList.Images.Count)
				{
					imageList.Draw(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, imageIndex);
				}
				text = ((!useImageKeys) ? ("Image[" + imageIndex + "]") : (imageList != null && imageIndex < imageList.Images.Keys.Count ? imageList.Images.Keys[imageIndex] : ""));
				break;
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
			DrawImageIndexItem(eventArgs.Graphics, eventArgs.Bounds, imageIndex, eventArgs.ForeColor, imageList);
		}
	}
}
