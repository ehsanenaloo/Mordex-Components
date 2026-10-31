using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Design
{
	internal class ImageIndexDialog : Form
	{
		private ImageList imageList;

		private bool isUsingKeys;

		private IContainer components = default;

		private ListBox imagesListBox;

		private Button btnCancel;

		private Button btnOK;

		internal ImageIndexDialog()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			InitializeComponent();
			base.StartPosition = FormStartPosition.CenterScreen;
		}

		private void ImageIndexDialog_Load(object sender, EventArgs eventArgs)
		{
		}

		internal string EditImageKey(Form owner, ImageList imageList, string imageKey)
		{
			this.imageList = imageList;
			imagesListBox.Items.Clear();
			imagesListBox.Items.Add("");
			if (imageList != null)
			{
				StringEnumerator enumerator = imageList.Images.Keys.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						if (current.Length > 0)
						{
							imagesListBox.Items.Add(current);
						}
					}
				}
				finally
				{
					if (enumerator is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
			isUsingKeys = true;
			imagesListBox.ItemHeight = 18;
			imagesListBox.DrawMode = DrawMode.OwnerDrawFixed;
			imagesListBox.DrawItem += OnImageDrawItem;
			imagesListBox.SelectedItem = imageKey;
			if (ShowDialog(owner) == DialogResult.OK)
			{
				return imagesListBox.SelectedItem as string;
			}
			return null;
		}

		internal object EditImageIndex(Form owner, ImageList imageList, int imageIndex)
		{
			this.imageList = imageList;
			imagesListBox.Items.Clear();
			imagesListBox.Items.Add(-1);
			if (imageList != null)
			{
				for (int i = 0; i < imageList.Images.Count; i++)
				{
					imagesListBox.Items.Add(i);
				}
			}
			isUsingKeys = false;
			imagesListBox.ItemHeight = 18;
			imagesListBox.DrawMode = DrawMode.OwnerDrawFixed;
			imagesListBox.DrawItem += OnImageDrawItem;
			imagesListBox.SelectedItem = imageIndex;
			if (ShowDialog(owner) == DialogResult.OK)
			{
				return imagesListBox.SelectedItem;
			}
			return null;
		}

		private void OnImageDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			if (eventArgs.Index < 0)
			{
				return;
			}
			// ImageList.Images[...] returns a new bitmap on every call, so the copy is disposed after drawing.
			Image image = null;
			Rectangle rectangle = new Rectangle(eventArgs.Bounds.X + 2, eventArgs.Bounds.Y + 2, eventArgs.Bounds.Height - 4, eventArgs.Bounds.Height - 4);
			eventArgs.DrawBackground();
			string text;
			if (isUsingKeys)
			{
				text = (string)imagesListBox.Items[eventArgs.Index];
				if (text != null && text.Length > 0)
				{
					if (imageList != null)
					{
						image = imageList.Images[text];
					}
				}
				else
				{
					text = "(none)";
				}
			}
			else
			{
				int imageIndex = (int)imagesListBox.Items[eventArgs.Index];
				if (imageIndex == -1)
				{
					text = "(none)";
				}
				else
				{
					text = "Image[" + imageIndex + "]";
					if (imageList != null)
					{
						image = imageList.Images[imageIndex];
					}
				}
			}
			if (image != null)
			{
				eventArgs.Graphics.DrawImage(image, rectangle);
				image.Dispose();
			}
			eventArgs.Graphics.DrawRectangle(Pens.Black, rectangle);
			rectangle = new Rectangle(rectangle.Right + 2, eventArgs.Bounds.Y, eventArgs.Bounds.Width - rectangle.Width - 4, eventArgs.Bounds.Height);
			using (Brush brush = new SolidBrush(eventArgs.ForeColor))
			using (StringFormat stringFormat = new StringFormat())
			{
				stringFormat.LineAlignment = StringAlignment.Near;
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				eventArgs.Graphics.DrawString(text, eventArgs.Font, brush, rectangle, stringFormat);
			}
		}

		private void OnCancelClicked(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.Cancel;
		}

		private void OnOkClicked(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.OK;
		}

		private void OnImageDoubleClicked(object sender, EventArgs eventArgs)
		{
			base.DialogResult = DialogResult.OK;
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
			this.imagesListBox = new System.Windows.Forms.ListBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			base.SuspendLayout();
			this.imagesListBox.FormattingEnabled = true;
			this.imagesListBox.Location = new System.Drawing.Point(4, 4);
			this.imagesListBox.Name = "listBox1";
			this.imagesListBox.Size = new System.Drawing.Size(210, 186);
			this.imagesListBox.TabIndex = 0;
			this.imagesListBox.DoubleClick += new System.EventHandler(OnImageDoubleClicked);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(115, 193);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(63, 30);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(OnCancelClicked);
			this.btnOK.Location = new System.Drawing.Point(41, 193);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(68, 30);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(OnOkClicked);
			base.AcceptButton = this.btnOK;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.btnCancel;
			base.ClientSize = new System.Drawing.Size(219, 229);
			base.Controls.Add(this.btnOK);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.imagesListBox);
			base.Name = "ImageIndexDialog";
			this.Text = "ImageIndexDialog";
			base.Load += new System.EventHandler(ImageIndexDialog_Load);
			base.ResumeLayout(false);
		}
	}
}
