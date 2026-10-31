using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public class MdiChildTab : IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private const int SC_SIZE = 0xF000;

		private const int SC_MOVE = 0xF010;

		private const int SC_RESTORE = 0xF120;

		private const int MF_ENABLED = 0;

		private const int MF_GRAYED = 1;

		private CustomPropertyCollection _customProperties;

		private StateStyles _stateStyles;

		private MxMdiTabGroup _tabGroup;

		private Form _form;

		private FormBorderStyle _formBorderStyle;

		internal Rectangle _originalBounds;

		private Icon _icon;

		private Image _tabImage;

		private int _imageIndex;

		private string _imageKey;

		private bool _hadMaximizeBox;

		private bool _hadMinimizeBox;

		private string _text;

		private string _toolTipText;

		internal bool _isSelected;

		public MxMdiTabGroup Group => _tabGroup;

		public Form Form => _form;

		public StateStyles StateStyles => _stateStyles;

		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
			}
		}

		public string Text
		{
			get
			{
				if (_text == null)
				{
					if (_form == null)
					{
						return "";
					}
					return _form.Text;
				}
				return _text;
			}
			set
			{
				if (_text != value)
				{
					_text = value;
					if (_tabGroup != null)
					{
						_tabGroup.RebuildLayout(true);
					}
				}
			}
		}

		public Icon Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					if (_tabGroup != null)
					{
						_tabGroup.RebuildLayout(true);
					}
				}
			}
		}

		public Image Image
		{
			get
			{
				return _tabImage;
			}
			set
			{
				if (_tabImage != value)
				{
					_tabImage = value;
					if (_tabGroup != null)
					{
						_tabGroup.RebuildLayout(true);
					}
				}
			}
		}

		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					if (_tabGroup != null)
					{
						_tabGroup.RebuildLayout(true);
					}
				}
			}
		}

		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					if (_tabGroup != null)
					{
						_tabGroup.RebuildLayout(true);
					}
				}
			}
		}

		Control IFormatStyleOwner.Owner => _tabGroup;

		public CustomPropertyCollection CustomProperties => _customProperties;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal MdiChildTab(MxMdiTabGroup tabStrip, Form frm)
		{
			if (frm != null)
			{
				_hadMaximizeBox = frm.MaximizeBox;
				_hadMinimizeBox = frm.MinimizeBox;
				frm.MaximizeBox = false;
				frm.MinimizeBox = false;
				IntPtr intPtr = NativeMethods.GetSystemMenu(frm.Handle, IntPtr.Zero);
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.EnableMenuItem(intPtr, SC_RESTORE, MF_GRAYED);
					NativeMethods.EnableMenuItem(intPtr, SC_MOVE, MF_GRAYED);
					NativeMethods.EnableMenuItem(intPtr, SC_SIZE, MF_GRAYED);
				}
			}
			else
			{
				_hadMaximizeBox = true;
				_hadMinimizeBox = true;
			}
			_customProperties = new CustomPropertyCollection();
			_text = null;
			_toolTipText = "";
			_stateStyles = new StateStyles(this);
			_icon = null;
			_tabImage = null;
			_imageIndex = -1;
			_imageKey = "";
			_tabGroup = tabStrip;
			AttachForm(frm);
			FitFormToGroup();
		}

		internal void SetTabGroup(MxMdiTabGroup tabStrip)
		{
			_tabGroup = tabStrip;
		}

		internal void AttachForm(Form form)
		{
			_form = form;
			if (_form != null)
			{
				_originalBounds = _form.Bounds;
				_formBorderStyle = _form.FormBorderStyle;
				if (_formBorderStyle != FormBorderStyle.None)
				{
					_form.FormBorderStyle = FormBorderStyle.None;
				}
				_form.Resize += OnFormMove;
				_form.Move += OnFormMove;
				_form.TextChanged += OnFormTextChanged;
				_form.StartPosition = FormStartPosition.Manual;
			}
		}

		public void MoveTo(MxMdiTabGroup group)
		{
			if (_tabGroup != group)
			{
				if (_tabGroup.PanelManager != group.PanelManager)
				{
					throw new InvalidOperationException();
				}
				_tabGroup.GetTabGroupHost().MoveFormToGroup(group, _form);
			}
		}

		internal void FitFormToGroup()
		{
			if (_form != null && _tabGroup != null && !_tabGroup._isDeserializing)
			{
				_form.WindowState = FormWindowState.Normal;
				_form.Region = null;
				_form.MinimumSize = Size.Empty;
				_form.MaximumSize = Size.Empty;
				Rectangle formRectangle = _tabGroup.FormRectangle;
				if (formRectangle.Width < 0)
				{
					formRectangle.Width = 0;
				}
				if (formRectangle.Height < 0)
				{
					formRectangle.Height = 0;
				}
				if (!formRectangle.IsEmpty && formRectangle != _form.Bounds)
				{
					_form.Size = formRectangle.Size;
					_form.Location = formRectangle.Location;
				}
			}
		}

		internal void RestoreForm()
		{
			_tabGroup = null;
			if (_form == null)
			{
				return;
			}
			if (!_form.IsDisposed && !_form.Disposing)
			{
				_form.MaximizeBox = _hadMaximizeBox;
				_form.MinimizeBox = _hadMinimizeBox;
				IntPtr intPtr = NativeMethods.GetSystemMenu(_form.Handle, IntPtr.Zero);
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.EnableMenuItem(intPtr, SC_RESTORE, MF_ENABLED);
					NativeMethods.EnableMenuItem(intPtr, SC_MOVE, MF_ENABLED);
					NativeMethods.EnableMenuItem(intPtr, SC_SIZE, MF_ENABLED);
				}
			}
			_form.Resize -= OnFormMove;
			_form.Move -= OnFormMove;
			_form.TextChanged -= OnFormTextChanged;
			_form.FormBorderStyle = _formBorderStyle;
			_form.Bounds = _originalBounds;
			_form = null;
		}

		private void OnFormMove(object sender, EventArgs eventArgs)
		{
			FitFormToGroup();
		}

		private void OnFormTextChanged(object sender, EventArgs eventArgs)
		{
			if (Form != null)
			{
				Text = Form.Text;
			}
		}

		void IFormatStyleOwner.PropertyChange()
		{
			if (_tabGroup != null)
			{
				_tabGroup.RebuildLayout(true);
			}
		}

		internal Size GetImageSize()
		{
			if (_tabGroup != null && _tabGroup.PanelManager != null && !_tabGroup.PanelManager.TabbedMdiSettings.ImageSize.IsEmpty)
			{
				return _tabGroup.PanelManager.TabbedMdiSettings.ImageSize;
			}
			if (_icon != null)
			{
				return _icon.Size;
			}
			if (_tabImage != null)
			{
				return _tabImage.Size;
			}
			if (_tabGroup != null && _tabGroup.PanelManager != null && _tabGroup.PanelManager.ImageList != null)
			{
				return _tabGroup.PanelManager.ImageList.ImageSize;
			}
			if (_tabGroup != null && _tabGroup.PanelManager != null && _tabGroup.PanelManager.TabbedMdiSettings.UseFormIcons)
			{
				return SystemInformation.SmallIconSize;
			}
			return Size.Empty;
		}

		internal CommandImageSource CreateImageSource()
		{
			CommandImageSource imageSource = new CommandImageSource();
			if (_icon != null)
			{
				imageSource._icon = _icon;
			}
			else if (_tabImage != null)
			{
				imageSource._image = _tabImage;
			}
			else if (_tabGroup != null && _tabGroup.PanelManager != null)
			{
				imageSource._imageList = _tabGroup.PanelManager.ImageList;
				imageSource._imageIndex = _imageIndex;
				imageSource._imageKey = _imageKey;
			}
			if (_tabGroup != null && _tabGroup.PanelManager != null && _tabGroup.PanelManager.TabbedMdiSettings.UseFormIcons && _form != null)
			{
				imageSource._icon = _form.Icon;
			}
			return imageSource;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), null);
			_toolTipText = (string)LayoutReader.GetPropertyValue(reader, "ToolTipText", typeof(string), "");
			PropertyValue propertyValue = reader["StateStyles"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_stateStyles);
			}
			else
			{
				_stateStyles.Reset();
			}
			propertyValue = reader["CustomProperties"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_customProperties);
			}
			else
			{
				_customProperties.Clear();
			}
			_isSelected = (bool)LayoutReader.GetPropertyValue(reader, "IsSelected", typeof(bool), false);
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_tabImage = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), "");
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_text != null)
			{
				info.AddValue("Text", _text);
			}
			if (_icon != null)
			{
				info.AddValue("Icon", _icon);
			}
			if (_tabImage != null)
			{
				info.AddValue("Image", _tabImage);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_toolTipText != null && _toolTipText.Length > 0)
			{
				info.AddValue("ToolTipText", _toolTipText);
			}
			if (_tabGroup.SelectedTab == this)
			{
				info.AddValue("IsSelected", true);
			}
			if (_stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (_customProperties.Count > 0)
			{
				info.AddValue("CustomProperties", _customProperties);
			}
		}
	}
}
