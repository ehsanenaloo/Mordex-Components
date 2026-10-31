using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Timeline.Design
{
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public class ImageKeyEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private ImageList _imageList;

		private void UpdateImageList(MxTimeline timeline, string textArgument, ListBox listBox)
		{
			listBox.DrawItem += OnControlDrawItem;
			listBox.DrawMode = DrawMode.OwnerDrawFixed;
			listBox.ItemHeight = (int)listBox.Font.GetHeight() + 4;
			int selectedIndex = -1;
			_imageList = timeline.GetImageList();
			listBox.DisplayMember = "Text";
			listBox.Items.Add(new TimelineValueListItem("", "(none)"));
			if (_imageList != null)
			{
				for (int i = 0; i < _imageList.Images.Count; i++)
				{
					string text = _imageList.Images.Keys[i];
					listBox.Items.Add(new TimelineValueListItem(text, text));
					if (text == textArgument)
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
					MxTimeline timeline = null;
					if (context.Instance is MxTimeline)
					{
						timeline = (MxTimeline)context.Instance;
					}
					else if (context.Instance is TimelineField)
					{
						timeline = ((TimelineField)context.Instance).Timeline;
					}
					else
					{
						if (!(context.Instance is TimelineFormatCondition))
						{
							return value;
						}
						timeline = ((TimelineFormatCondition)context.Instance).Timeline;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					UpdateImageList(timeline, (string)value, listBox);
					_iWindowsFormsEditorService.DropDownControl(listBox);
					if (listBox.SelectedItem != null)
					{
						value = ((TimelineValueListItem)listBox.SelectedItem).Value;
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
				if (context.Instance is MxTimeline)
				{
					return true;
				}
				if (context.Instance is TimelineFormatCondition)
				{
					return true;
				}
				if (context.Instance is TimelineField)
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
			MxTimeline timeline = null;
			if (e.Context.Instance is MxTimeline)
			{
				timeline = (MxTimeline)e.Context.Instance;
			}
			else if (e.Context.Instance is TimelineField)
			{
				timeline = ((TimelineField)e.Context.Instance).Timeline;
			}
			else if (e.Context.Instance is TimelineFormatCondition)
			{
				timeline = ((TimelineFormatCondition)e.Context.Instance).Timeline;
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
			if (timeline != null)
			{
				string text = (string)e.Value;
				ImageList imageList = timeline.GetImageList();
				if (imageList != null && text.Length > 0 && imageList.Images.ContainsKey(text))
				{
					e.Graphics.DrawImage(imageList.Images[text], e.Bounds, 0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
			}
		}

		private void DrawText(Graphics graphics, Rectangle rectangleArgument, string key, Color color, ImageList imageList)
		{
			Rectangle rectangle = new Rectangle(rectangleArgument.Left + 1, rectangleArgument.Top + 1, 18, rectangleArgument.Height - 2);
			rectangle = TimelineUtilities.DrawFlatBorder(graphics, SystemPens.WindowFrame, rectangle);
			graphics.FillRectangle(SystemBrushes.Window, rectangle);
			string text = "";
			if (key.Length == 0)
			{
				text = "(none)";
			}
			else
			{
				if (imageList != null && imageList.Images.ContainsKey(key))
				{
					graphics.DrawImage(imageList.Images[key], rectangle, 0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
				text = key;
			}
			if (text.Length > 0)
			{
				Font font = new Font("Tahoma", 8f);
				rectangle = new Rectangle(rectangle.Right + 1, rectangleArgument.Top, rectangleArgument.Width - rectangle.Width - 2, rectangleArgument.Height);
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
				TimelineValueListItem timelineValueListItem = (TimelineValueListItem)((ListBox)sender).Items[eventArgs.Index];
				string text = (string)timelineValueListItem.Value;
				DrawText(eventArgs.Graphics, eventArgs.Bounds, text, eventArgs.ForeColor, _imageList);
			}
		}
	}
}
