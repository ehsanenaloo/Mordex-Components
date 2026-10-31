using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Design
{
	internal class ImageEditor : UserControl, ITypeDescriptorContext, IServiceProvider
	{
		private Color borderColor;

		internal object imageOwner;

		private IServiceProvider provider;

		internal int imageMode;

		private MxSuperTip superTipOwner;

		internal PropertyDescriptor propImage;

		internal PropertyDescriptor propImageIndex;

		internal PropertyDescriptor propImageKey;

		private IContainer components = default;

		private Button browseButton;

		private Image Image
		{
			get
			{
				Image result = null;
				if (imageOwner != null)
				{
					switch (imageMode)
					{
					case 0:
						if (propImage != null)
						{
							result = (Image)propImage.GetValue(imageOwner);
						}
						break;
					case 1:
						if (propImageKey != null)
						{
							string text = (string)propImageKey.GetValue(imageOwner);
							if (text.Length > 0 && superTipOwner != null && superTipOwner.ImageList != null && superTipOwner.ImageList.Images.ContainsKey(text))
							{
								result = superTipOwner.ImageList.Images[text];
							}
						}
						break;
					case 2:
						if (propImageIndex != null)
						{
							int imageIndex = (int)propImageIndex.GetValue(imageOwner);
							if (imageIndex != -1 && superTipOwner != null && superTipOwner.ImageList != null && imageIndex >= 0 && imageIndex < superTipOwner.ImageList.Images.Count)
							{
								result = superTipOwner.ImageList.Images[imageIndex];
							}
						}
						break;
					}
				}
				return result;
			}
		}

		IContainer ITypeDescriptorContext.Container => superTipOwner?.Container;

		object ITypeDescriptorContext.Instance => superTipOwner;

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => propImage;

		internal ImageEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
			borderColor = SystemColors.ControlDark;
		}

		internal void Edit(object propOwner, MxSuperTip superTipOwner, string imagePropertyName, string imageIndexPropertyName, string imageKeyPropertyName, ITypeDescriptorContext context, IServiceProvider provider)
		{
			this.superTipOwner = superTipOwner;
			imageOwner = propOwner;
			this.provider = provider;
			imageMode = 0;
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(propOwner);
			propImage = properties.Find(imagePropertyName, ignoreCase: true);
			propImageIndex = properties.Find(imageIndexPropertyName, ignoreCase: true);
			propImageKey = properties.Find(imageKeyPropertyName, ignoreCase: true);
			DetectImageMode();
		}

		private void DetectImageMode()
		{
			if (propImage.ShouldSerializeValue(imageOwner))
			{
				imageMode = 0;
			}
			else if (propImageKey.ShouldSerializeValue(imageOwner))
			{
				imageMode = 1;
			}
			else if (propImageIndex.ShouldSerializeValue(imageOwner))
			{
				imageMode = 2;
			}
			Invalidate();
		}

		private void ImageEditor_Load(object sender, EventArgs eventArgs)
		{
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			base.OnPaint(eventArgs);
			using (Brush backBrush = new SolidBrush(BackColor))
			{
				eventArgs.Graphics.FillRectangle(backBrush, base.ClientRectangle);
			}
			int thumbnailSize = base.Height - 6;
			using Pen pen = new Pen(borderColor);
			using StringFormat stringFormat = new StringFormat();
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			_ = base.ClientRectangle;
			string text;
			switch (imageMode)
			{
			case 1:
				text = "ImageKey:";
				break;
			case 2:
				text = "ImageIndex:";
				break;
			default:
				text = "Image:";
				break;
			}
			Rectangle rectangle = new Rectangle(4, 3, thumbnailSize, thumbnailSize);
			Image image = Image;
			bool hasImage = image != null;
			if (image != null)
			{
				eventArgs.Graphics.DrawImage(image, rectangle);
				// Image keys and indexes resolve through ImageList.Images, which returns a new bitmap each time.
				if (imageMode != 0)
				{
					image.Dispose();
				}
				image = null;
			}
			rectangle.Width--;
			rectangle.Height--;
			eventArgs.Graphics.DrawRectangle(pen, Rectangle.Inflate(rectangle, 1, 1));
			rectangle = new Rectangle(base.ClientRectangle.X + thumbnailSize + 6, base.ClientRectangle.Top + 1, base.ClientRectangle.Width - thumbnailSize - 8, base.ClientRectangle.Height - 2);
			using Brush textBrush = new SolidBrush(ForeColor);
			string valueText = "";
			if (!hasImage)
			{
				valueText = "(none)";
			}
			else
			{
				switch (imageMode)
				{
				case 0:
					valueText = propImage.GetValue(imageOwner)?.GetType().Name ?? "";
					break;
				case 1:
					valueText = (string)propImageKey.GetValue(imageOwner);
					break;
				case 2:
				{
					int imageIndex = (int)propImageIndex.GetValue(imageOwner);
					valueText = "Image[" + imageIndex + "]";
					break;
				}
				}
			}
			valueText = text + " " + valueText;
			eventArgs.Graphics.DrawString(valueText, Font, textBrush, rectangle, stringFormat);
			rectangle = base.ClientRectangle;
			rectangle.Width--;
			rectangle.Height--;
			eventArgs.Graphics.DrawRectangle(pen, rectangle);
		}

		internal void ClearImage()
		{
			if (imageOwner != null)
			{
				propImage.SetValue(imageOwner, null);
				propImageIndex.SetValue(imageOwner, -1);
				propImageKey.SetValue(imageOwner, "");
			}
		}

		internal void EditImage()
		{
			if (imageOwner != null && propImage != null && propImage.GetEditor(typeof(UITypeEditor)) is UITypeEditor uiTypeEditor)
			{
				propImage.SetValue(imageOwner, uiTypeEditor.EditValue(this, provider, propImage.GetValue(imageOwner)));
			}
		}

		internal void EditImageKey()
		{
			if (imageOwner != null)
			{
				ImageIndexDialog imageIndexDialog = new ImageIndexDialog();
				string text = imageIndexDialog.EditImageKey(FindForm(), superTipOwner.ImageList, (string)propImageKey.GetValue(imageOwner));
				if (text != null)
				{
					propImageKey.SetValue(imageOwner, text);
				}
				imageIndexDialog.Dispose();
			}
		}

		internal void EditImageIndex()
		{
			if (imageOwner != null)
			{
				ImageIndexDialog imageIndexDialog = new ImageIndexDialog();
				object selectedIndex = imageIndexDialog.EditImageIndex(FindForm(), superTipOwner.ImageList, (int)propImageIndex.GetValue(imageOwner));
				if (selectedIndex != null)
				{
					propImageIndex.SetValue(imageOwner, selectedIndex);
				}
				imageIndexDialog.Dispose();
			}
		}

		private void OnBrowseClicked(object sender, EventArgs eventArgs)
		{
			using ImageChooser imageChooser = new ImageChooser();
			if (imageChooser.ShowDialog(this) == DialogResult.OK)
			{
				DetectImageMode();
			}
		}

		void ITypeDescriptorContext.OnComponentChanged()
		{
		}

		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return true;
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return superTipOwner?.Site?.GetService(serviceType) ?? provider?.GetService(serviceType);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.browseButton = new System.Windows.Forms.Button();
			base.SuspendLayout();
			this.browseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.browseButton.Location = new System.Drawing.Point(211, 0);
			this.browseButton.Name = "button1";
			this.browseButton.Size = new System.Drawing.Size(24, 22);
			this.browseButton.TabIndex = 0;
			this.browseButton.Text = "...";
			this.browseButton.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(OnBrowseClicked);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			base.Controls.Add(this.browseButton);
			base.Name = "ImageEditor";
			base.Size = new System.Drawing.Size(235, 22);
			base.Load += new System.EventHandler(ImageEditor_Load);
			base.ResumeLayout(false);
		}
	}
}
