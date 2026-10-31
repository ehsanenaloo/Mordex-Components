using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation.Design
{
	public class AutoFormatDialog : Form
	{
		private MxNavigationBar _previewBar;

		private ListBox _styleList;

		private Label _label;

		private Label _styleCaptionLabel;

		private Button _okButton;

		private Button _cancelButton;

		private ImageList _imageList;

		private ImageList _styleImageList;

		private IContainer _iContainer;

		private MxNavigationBar _navigationBar;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public MxNavigationBar NavigationBar
		{
			get
			{
				return _navigationBar;
			}
			set
			{
				_navigationBar = value;
			}
		}

		public AutoFormatDialog()
		{
			InitializeControls();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			try
			{
				_imageList.Images.Add(NavigationBarState.GetCabIcon());
				_imageList.Images.Add(NavigationBarState.GetCameraIcon());
				_imageList.Images.Add(NavigationBarState.GetFaxIcon());
				_styleImageList.Images.Add(NavigationBarState.GetCabIcon());
				_styleImageList.Images.Add(NavigationBarState.GetCameraIcon());
				_styleImageList.Images.Add(NavigationBarState.GetFaxIcon());
				_styleList.Items.Add(NavigationBarStyle.CommonDialog);
				_styleList.Items.Add(NavigationBarStyle.Office2000);
				_styleList.Items.Add(NavigationBarStyle.Office2003);
				_styleList.Items.Add(NavigationBarStyle.OfficeXP);
				_styleList.Items.Add(NavigationBarStyle.OutlookExpress);
				_styleList.Items.Add(NavigationBarStyle.VisualStudio);
				_styleList.Items.Add(NavigationBarStyle.VS2005);
				_styleList.Items.Add(NavigationBarStyle.VS2010);
				NavigationGroup navigationBarGroup = _previewBar.Groups[0];
				for (int i = 0; i < navigationBarGroup.Items.Count; i++)
				{
					navigationBarGroup.Items[i].SmallImageIndex = i;
					navigationBarGroup.Items[i].LargeImageIndex = i;
				}
				_previewBar.SetNavigationBarStyle(NavigationBarStyle.VisualStudio);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			NavigationGroup navigationBarGroup = new NavigationGroup();
			NavigationItem navigationBarItem = new NavigationItem();
			NavigationItem secondItem = new NavigationItem();
			NavigationItem thirdItem = new NavigationItem();
			NavigationGroup secondGroup = new NavigationGroup();
			_previewBar = new MxNavigationBar();
			_styleImageList = new ImageList(_iContainer);
			_imageList = new ImageList(_iContainer);
			_styleList = new ListBox();
			_label = new Label();
			_styleCaptionLabel = new Label();
			_okButton = new Button();
			_cancelButton = new Button();
			((ISupportInitialize)_previewBar).BeginInit();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_previewBar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_previewBar.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			navigationBarItem.Text = "Item 1";
			navigationBarItem.ToolTipText = null;
			secondItem.Text = "Item 2";
			secondItem.ToolTipText = null;
			thirdItem.Text = "Item 3";
			thirdItem.ToolTipText = null;
			navigationBarGroup.Items.AddRange(new NavigationItem[3] { navigationBarItem, secondItem, thirdItem });
			navigationBarGroup.Text = "Group 1";
			secondGroup.Text = "Group 2";
			_previewBar.Groups.AddRange(new NavigationGroup[2] { navigationBarGroup, secondGroup });
			_previewBar.LargeImageList = _styleImageList;
			_previewBar.LargeImageSize = new Size(32, 32);
			_previewBar.Location = new Point(152, 24);
			_previewBar.Name = "navigationBar1";
			_previewBar.Size = new Size(152, 264);
			_previewBar.SmallImageList = _imageList;
			_previewBar.TabIndex = 0;
			_previewBar.Text = "navigationBar1";
			_styleImageList.ColorDepth = ColorDepth.Depth32Bit;
			_styleImageList.ImageSize = new Size(32, 32);
			_styleImageList.TransparentColor = Color.Transparent;
			_imageList.ColorDepth = ColorDepth.Depth32Bit;
			_imageList.ImageSize = new Size(16, 16);
			_imageList.TransparentColor = Color.Transparent;
			_styleList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			_styleList.Location = new Point(8, 24);
			_styleList.Name = "listBox1";
			_styleList.Size = new Size(128, 264);
			_styleList.TabIndex = 1;
			_styleList.SelectedIndexChanged += OnStyleListSelectedIndexChanged;
			_label.AutoSize = true;
			_label.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_label.Location = new Point(8, 8);
			_label.Name = "label1";
			_label.Size = new Size(57, 13);
			_label.TabIndex = 2;
			_label.Text = "Formats:";
			_styleCaptionLabel.AutoSize = true;
			_styleCaptionLabel.Font = new Font("Tahoma", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			_styleCaptionLabel.Location = new Point(152, 8);
			_styleCaptionLabel.Name = "label2";
			_styleCaptionLabel.Size = new Size(55, 13);
			_styleCaptionLabel.TabIndex = 3;
			_styleCaptionLabel.Text = "Preview:";
			_okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_okButton.Location = new Point(136, 299);
			_okButton.Name = "okButton";
			_okButton.Size = new Size(80, 24);
			_okButton.TabIndex = 4;
			_okButton.Text = "OK";
			_okButton.Click += OnOkButtonClick;
			_cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			_cancelButton.DialogResult = DialogResult.Cancel;
			_cancelButton.Location = new Point(224, 299);
			_cancelButton.Name = "cancelButton";
			_cancelButton.Size = new Size(80, 24);
			_cancelButton.TabIndex = 5;
			_cancelButton.Text = "Cancel";
			_cancelButton.Click += OnCancelButtonClick;
			base.AcceptButton = _okButton;
			base.CancelButton = _cancelButton;
			base.ClientSize = new Size(312, 335);
			base.Controls.Add(_cancelButton);
			base.Controls.Add(_okButton);
			base.Controls.Add(_styleCaptionLabel);
			base.Controls.Add(_label);
			base.Controls.Add(_styleList);
			base.Controls.Add(_previewBar);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.Name = "AutoFormatDialog";
			Text = "autoFormatDialog";
			((ISupportInitialize)_previewBar).EndInit();
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private void OnStyleListSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_previewBar.SetNavigationBarStyle((NavigationBarStyle)_styleList.SelectedItem);
		}

		private void OnOkButtonClick(object sender, EventArgs eventArgs)
		{
			if (_navigationBar != null && _styleList.SelectedItem != null)
			{
				base.DialogResult = DialogResult.OK;
				_navigationBar.SetNavigationBarStyle((NavigationBarStyle)_styleList.SelectedItem);
			}
			Close();
		}

		private void OnCancelButtonClick(object sender, EventArgs eventArgs)
		{
			Close();
		}
	}
}
