using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Controls
{
	public class ComboBoxItem : IMordexXmlLayoutsSupport
	{
		internal int _originalIndex = -1;

		internal int _listIndex = -1;

		private ComboBoxItemCollection _ownerCollection;

		private int _indentLevel;

		private FormatStyle _formatStyle;

		private object _dataRow;

		private object _value;

		private string _text;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private Image _image;

		private AccessibleObject _accessibleObject;

		internal bool _isSeparator;

		internal FontFamily _fontFamily;

		string IMordexXmlLayoutsSupport.Identifier
		{
			get
			{
				if (RenderingHelper.IsNullOrDbNull(_value))
				{
					return null;
				}
				return _value.ToString();
			}
		}

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public int Index => _listIndex;

		[Description("Indicates the indent level of the item.")]
		[DefaultValue(0)]
		public int IndentLevel
		{
			get
			{
				return _indentLevel;
			}
			set
			{
				if (_indentLevel != value)
				{
					_indentLevel = value;
					RefreshOwner();
				}
			}
		}

		[Description("Represents the format style used to draw the item")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new FormatStyle();
				}
				return _formatStyle;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		public object DataRow
		{
			get
			{
				if (_ownerCollection != null && _ownerCollection._owner != null && !_ownerCollection._owner.IsDataSourceEmpty() && _ownerCollection._owner is MxComboBox)
				{
					MxComboBox uiComboBox = _ownerCollection._owner as MxComboBox;
					return uiComboBox.GetSourceObject(this);
				}
				return _dataRow;
			}
			set
			{
				if (_dataRow != value)
				{
					_dataRow = value;
				}
			}
		}

		[Localizable(true)]
		[Description("The value associated to this UIComboBoxItem.")]
		[DefaultValue(null)]
		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		public object Value
		{
			get
			{
				if (_value == null && DataRow != null)
				{
					if (GetOwnerComboBase() != null && GetOwnerComboBase() is MxComboBox)
					{
						MxComboBox uiComboBox = (MxComboBox)GetOwnerComboBase();
						if (uiComboBox.GetActiveCurrencyManager() != null)
						{
							return uiComboBox.GetMemberValue(DataRow);
						}
						if (uiComboBox.ValueMember.Length > 0)
						{
							try
							{
								PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(DataRow);
								PropertyDescriptor propertyDescriptor = properties.Find(uiComboBox.ValueMember, ignoreCase: true);
								if (propertyDescriptor != null)
								{
									return propertyDescriptor.GetValue(DataRow);
								}
							}
							catch (Exception ex) when (!(ex is OutOfMemoryException))
							{
								// A property getter of the bound row can throw; use the row itself as the value.
							}
						}
					}
					return DataRow;
				}
				return _value;
			}
			set
			{
				if (_ownerCollection != null && value == null)
				{
					throw new ArgumentNullException();
				}
				if (GetOwnerComboBase() != null && !GetOwnerComboBase().IsDataSourceEmpty())
				{
					throw new InvalidOperationException("Can't modify the Value property when the DataSource property of the owner UIComboBox is set");
				}
				if (value != _value)
				{
					_value = value;
					RefreshOwner();
				}
			}
		}

		[Localizable(true)]
		[Description("The text associated with a value in this UIComboBoxItem.")]
		[DefaultValue(null)]
		public string Text
		{
			get
			{
				if (_text == null)
				{
					if (DataRow != null)
					{
						if (GetOwnerComboBase() != null && GetOwnerComboBase() is MxComboBox)
						{
							MxComboBox uiComboBox = (MxComboBox)GetOwnerComboBase();
							if (uiComboBox.GetActiveCurrencyManager() != null)
							{
								return uiComboBox.GetDisplayText(DataRow);
							}
							if (uiComboBox.DisplayMember.Length > 0)
							{
								try
								{
									PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(DataRow);
									PropertyDescriptor propertyDescriptor = properties.Find(uiComboBox.DisplayMember, ignoreCase: true);
									if (propertyDescriptor != null)
									{
										object value = propertyDescriptor.GetValue(DataRow);
										if (value != null)
										{
											return value.ToString();
										}
										return "";
									}
								}
								catch (Exception ex) when (!(ex is OutOfMemoryException))
								{
									// A property getter of the bound row can throw; show the row text instead.
								}
							}
						}
						return string.Concat(DataRow);
					}
					return "";
				}
				return _text;
			}
			set
			{
				if (GetOwnerComboBase() != null && !GetOwnerComboBase().IsDataSourceEmpty())
				{
					throw new InvalidOperationException("Can't modify the Text property when the DataSource property of the owner UIComboBox is set");
				}
				if (value != _text)
				{
					_text = value;
					RefreshOwner();
				}
			}
		}

		[Description("Represents the image list index value of the image displayed by the item.")]
		[Localizable(true)]
		[DefaultValue(-1)]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (value != _imageIndex)
				{
					_imageIndex = value;
					_imageKey = "";
					RefreshOwner();
				}
			}
		}

		[Description("The key accessor for the image in the ImageList displayed by the item.")]
		[DefaultValue("")]
		[Localizable(true)]
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
				if (value != _imageKey)
				{
					_imageKey = value;
					_imageIndex = -1;
					RefreshOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Description("The image that is displayed on the UICommand.")]
		[Localizable(true)]
		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (value != _image)
				{
					_image = value;
					RefreshOwner();
				}
			}
		}

		[Description("Specifies whether the item should be presents as a separator line between two items")]
		public bool IsSeparator
		{
			get
			{
				return _isSeparator;
			}
			set
			{
				if (_isSeparator != value)
				{
					_isSeparator = value;
					RefreshOwner();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_image = (Image)LayoutReader.GetPropertyValue(info, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), string.Empty);
			string text = (string)LayoutReader.GetPropertyValue(info, "ValueType", typeof(string), "");
			_indentLevel = (int)LayoutReader.GetPropertyValue(info, "IndentLevel", typeof(int), 0);
			_value = LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
			if (text.Length > 0 && _value != null)
			{
				_value = LayoutTypePolicy.ConvertOrKeepText((string)_value, text);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_image != null)
			{
				info.AddValue("Image", _image);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_indentLevel != 0)
			{
				info.AddValue("IndentLevel", _indentLevel);
			}
			if (_text != string.Empty)
			{
				info.AddValue("Text", _text);
			}
			if (_value != null)
			{
				if (!(_value is string))
				{
					info.AddValue("ValueType", _value.GetType().FullName);
				}
				info.AddValue("Value", _value);
			}
		}

		public ComboBoxItem(object value)
			: this(value, -1)
		{
		}

		public ComboBoxItem()
		{
		}

		public ComboBoxItem(string text)
		{
			_text = text;
		}

		public ComboBoxItem(string text, object value)
		{
			_text = text;
			_dataRow = value;
		}

		public ComboBoxItem(string text, object value, int imageIndex)
		{
			_text = text;
			_dataRow = value;
			_imageIndex = imageIndex;
		}

		public ComboBoxItem(object value, int imageIndex)
		{
			_dataRow = value;
			_imageIndex = imageIndex;
		}

		internal ComboBoxItemCollection GetOwnerCollection()
		{
			return _ownerCollection;
		}

		internal void SetOwnerCollection(ComboBoxItemCollection ownerCollection)
		{
			if (ownerCollection != _ownerCollection)
			{
				_ownerCollection = ownerCollection;
			}
		}

		internal Image GetDisplayImage(ImageList imageList)
		{
			if (_image != null)
			{
				return _image;
			}
			int listIndex = GetImageListIndex(imageList);
			if (listIndex >= 0)
			{
				return imageList.Images[listIndex];
			}
			return null;
		}

		internal int GetImageListIndex(ImageList imageList)
		{
			if (_image == null && imageList != null)
			{
				if (_imageKey.Length > 0 && imageList.Images.ContainsKey(_imageKey))
				{
					return imageList.Images.IndexOfKey(_imageKey);
				}
				if (_imageIndex >= 0 && _imageIndex < imageList.Images.Count)
				{
					return _imageIndex;
				}
			}
			return -1;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new ComboBoxItemAccessibleObject(this);
			}
			return _accessibleObject;
		}

		internal void RefreshOwner()
		{
			MxComboBase uiComboBase = GetOwnerComboBase();
			if (uiComboBase == null)
			{
				return;
			}
			if (uiComboBase is MxComboBox)
			{
				MxComboBox uiComboBox = (MxComboBox)uiComboBase;
				if (uiComboBox.SelectedItem == this)
				{
					if (Image != null)
					{
						uiComboBox.SetImage(Image);
					}
					else
					{
						uiComboBox.SetImageIndex(ImageIndex);
						uiComboBox.SetImageKey(ImageKey);
					}
					uiComboBox.Text = Text;
				}
			}
			uiComboBase.InvalidateLayout();
		}

		public Rectangle GetBounds()
		{
			if (GetOwnerComboBase() != null)
			{
				return GetOwnerComboBase()._formatStyleRenderer.GetItemBounds(this);
			}
			return Rectangle.Empty;
		}

		internal MxComboBase GetOwnerComboBase()
		{
			if (_ownerCollection == null)
			{
				return null;
			}
			return _ownerCollection._owner;
		}
	}
}
