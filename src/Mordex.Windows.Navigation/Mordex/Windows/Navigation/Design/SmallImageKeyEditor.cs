using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Windows.Navigation.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class SmallImageKeyEditor : UITypeEditor
	{
		private IWindowsFormsEditorService _iWindowsFormsEditorService;

		private ImageList _imageList;

		private void UpdateImageList(MxNavigationBar navigationBar, string currentImageKey, ListBox listBox)
		{
			listBox.DrawItem += OnControlDrawItem;
			listBox.DrawMode = DrawMode.OwnerDrawFixed;
			listBox.ItemHeight = (int)listBox.Font.GetHeight() + 4;
			int selectedListIndex = -1;
			_imageList = navigationBar.SmallImageList;
			listBox.Items.Add(new ON("", "(none)"));
			if (_imageList != null)
			{
				for (int i = 0; i < _imageList.Images.Count; i++)
				{
					string imageKey = _imageList.Images.Keys[i];
					listBox.Items.Add(new ON(imageKey, imageKey));
					if (imageKey == currentImageKey)
					{
						selectedListIndex = listBox.Items.Count - 1;
					}
				}
			}
			if (selectedListIndex == -1)
			{
				listBox.SelectedIndex = 0;
			}
			else
			{
				listBox.SelectedIndex = selectedListIndex;
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
					MxNavigationBar navigationBar = null;
					object propertyOwner = null;
					propertyOwner = ((!(context.Instance is ICustomTypeDescriptor)) ? context.Instance : ((ICustomTypeDescriptor)context.Instance).GetPropertyOwner(context.PropertyDescriptor));
					if (propertyOwner is NavigationGroup)
					{
						navigationBar = ((NavigationGroup)propertyOwner).NavigationBar;
					}
					else if (propertyOwner is NavigationItem)
					{
						navigationBar = ((NavigationItem)propertyOwner).NavigationBar;
					}
					else
					{
						if (!(propertyOwner is MxNavigationBar))
						{
							throw new InvalidOperationException();
						}
						navigationBar = (MxNavigationBar)propertyOwner;
					}
					if (navigationBar == null)
					{
						return value;
					}
					ListBox listBox = new ListBox();
					listBox.MouseUp += OnControlMouseUp;
					UpdateImageList(navigationBar, (string)value, listBox);
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
				if (context.Instance is NavigationGroup)
				{
					if (((NavigationGroup)context.Instance).NavigationBar != null)
					{
						return true;
					}
				}
				else if (context.Instance is NavigationItem && ((NavigationItem)context.Instance).NavigationBar != null)
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
			MxNavigationBar navigationBar = null;
			if (e.Context.Instance is NavigationGroup)
			{
				navigationBar = ((NavigationGroup)e.Context.Instance).NavigationBar;
			}
			else
			{
				if (!(e.Context.Instance is NavigationItem))
				{
					return;
				}
				navigationBar = ((NavigationItem)e.Context.Instance).NavigationBar;
			}
			if (navigationBar == null)
			{
				return;
			}
			string imageKey = (string)e.Value;
			if (imageKey != null && imageKey.Length > 0 && navigationBar.SmallImageList != null)
			{
				try
				{
					Image previewImage = navigationBar.SmallImageList.Images[imageKey];
					e.Graphics.DrawImage(previewImage, e.Bounds, 0, 0, previewImage.Width, previewImage.Height, GraphicsUnit.Pixel);
					return;
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
				}
			}
			e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
		}

		private void DrawListEntry(Graphics graphics, Rectangle bounds, string imageKey, Color textColor, ImageList imageList)
		{
			Rectangle contentBounds = new Rectangle(bounds.Left + 1, bounds.Top + 1, 18, bounds.Height - 2);
			contentBounds = NavigationGraphicsHelper.DrawBorderRectangle(graphics, SystemPens.WindowFrame, contentBounds);
			graphics.FillRectangle(SystemBrushes.Window, contentBounds);
			string labelText = "";
			if (imageKey == null || imageKey.Length == 0)
			{
				labelText = "(none)";
			}
			else
			{
				if (imageList != null)
				{
					graphics.DrawImage(imageList.Images[imageKey], contentBounds, 0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height, GraphicsUnit.Pixel);
				}
				labelText = imageKey;
			}
			if (labelText.Length > 0)
			{
				Font labelFont = new Font("Tahoma", 8f);
				contentBounds = new Rectangle(contentBounds.Right + 1, bounds.Top, bounds.Width - contentBounds.Width - 2, bounds.Height);
				using StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap);
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
				Brush textBrush = new SolidBrush(textColor);
				graphics.DrawString(labelText, labelFont, textBrush, contentBounds, stringFormat);
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
				ON entry = (ON)((ListBox)sender).Items[eventArgs.Index];
				string imageKey = (string)entry.Object;
				DrawListEntry(eventArgs.Graphics, eventArgs.Bounds, imageKey, eventArgs.ForeColor, _imageList);
			}
		}
	}
}
