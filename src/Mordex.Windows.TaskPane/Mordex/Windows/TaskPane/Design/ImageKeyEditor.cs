using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.TaskPane.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class ImageKeyEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private ImageList _imageList;

		private void UpdateImageList(MxTaskPane taskPane, string selectedImageKey, ListBox listBox)
		{
			listBox.DrawItem += OnControlDrawItem;
			listBox.DrawMode = DrawMode.OwnerDrawFixed;
			listBox.ItemHeight = (int)listBox.Font.GetHeight() + 4;
			int selectedIndex = -1;
			_imageList = taskPane.ImageList;
			listBox.Items.Add(new ON("", "(none)"));
			if (_imageList != null)
			{
				for (int i = 0; i < _imageList.Images.Count; i++)
				{
					string imageKey = _imageList.Images.Keys[i];
					listBox.Items.Add(new ON(imageKey, imageKey));
					if (imageKey == selectedImageKey)
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
				_iWindowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (_iWindowsFormsEditorService != null)
				{
					MxTaskPane taskPane = null;
					if (context.Instance is TaskPaneGroup)
					{
						taskPane = ((TaskPaneGroup)context.Instance).TaskPane;
					}
					else if (context.Instance is TaskPaneItem)
					{
						taskPane = ((TaskPaneItem)context.Instance).TaskPane;
					}
					else
					{
						if (!(context.Instance is MxTaskPane))
						{
							throw new InvalidOperationException();
						}
						taskPane = (MxTaskPane)context.Instance;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					UpdateImageList(taskPane, (string)value, listBox);
					_iWindowsFormsEditorService.DropDownControl(listBox);
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
			if (_iWindowsFormsEditorService != null)
			{
				_iWindowsFormsEditorService.CloseDropDown();
			}
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			if (context != null)
			{
				if (context.Instance is TaskPaneItem)
				{
					if (((TaskPaneItem)context.Instance).TaskPane != null)
					{
						return true;
					}
				}
				else if (context.Instance is TaskPaneGroup)
				{
					if (((TaskPaneGroup)context.Instance).TaskPane != null)
					{
						return true;
					}
				}
				else if (context.Instance is MxTaskPane)
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
			MxTaskPane taskPane = null;
			if (e.Context.Instance is TaskPaneItem)
			{
				taskPane = ((TaskPaneItem)e.Context.Instance).TaskPane;
			}
			else if (e.Context.Instance is TaskPaneGroup)
			{
				taskPane = ((TaskPaneGroup)e.Context.Instance).TaskPane;
			}
			else
			{
				if (!(e.Context.Instance is MxTaskPane))
				{
					return;
				}
				taskPane = (MxTaskPane)e.Context.Instance;
			}
			if (taskPane == null)
			{
				return;
			}
			string imageKey = (string)e.Value;
			if (imageKey != null && imageKey.Length > 0 && taskPane.ImageList != null)
			{
				try
				{
					Image image = taskPane.ImageList.Images[imageKey];
					e.Graphics.DrawImage(image, e.Bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
					return;
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}

		private void DrawImageItem(Graphics graphics, Rectangle itemBounds, string imageKey, Color textColor, ImageList imageList)
		{
			Rectangle contentBounds = new Rectangle(itemBounds.Left + 1, itemBounds.Top + 1, 18, itemBounds.Height - 2);
			contentBounds = TaskPanePaintHelper.DrawOutline(graphics, SystemPens.WindowFrame, contentBounds);
			graphics.FillRectangle(SystemBrushes.Window, contentBounds);
			string label = "";
			if (imageKey == null || imageKey == "")
			{
				label = "(none)";
			}
			else
			{
				if (imageList != null)
				{
					graphics.DrawImage(imageList.Images[imageKey], contentBounds, 0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
				label = imageKey;
			}
			if (label.Length > 0)
			{
				Font labelFont = new Font("Tahoma", 8f);
				contentBounds = new Rectangle(contentBounds.Right + 1, itemBounds.Top, itemBounds.Width - contentBounds.Width - 2, itemBounds.Height);
				using StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap);
				stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				Brush textBrush = new SolidBrush(textColor);
				graphics.DrawString(label, labelFont, textBrush, contentBounds, stringFormat);
				textBrush.Dispose();
				textBrush = null;
				labelFont.Dispose();
			}
		}

		private void OnControlDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			eventArgs.DrawBackground();
			if (eventArgs.Index != -1)
			{
				ON listItem = (ON)((ListBox)sender).Items[eventArgs.Index];
				string imageKey = (string)listItem.Object;
				DrawImageItem(eventArgs.Graphics, eventArgs.Bounds, imageKey, eventArgs.ForeColor, _imageList);
			}
		}
	}
}
