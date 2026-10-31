using System;
using System.Drawing;

namespace Mordex.Data
{
	public class MordexValueItemBase : IMordexValueItem
	{
		private object _value;

		private string _text;

		private Image _image;

		private int _imageIndex;

		private string _imageKey;

		private MordexValueItemCollectionBase _ownerCollection;

		protected object BaseValue
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					if (_ownerCollection != null && value == null)
					{
						throw new ArgumentNullException();
					}
					if (_ownerCollection != null)
					{
						_ownerCollection.UnregisterKey(_value, this);
					}
					_value = value;
					if (_ownerCollection != null)
					{
						_ownerCollection.RegisterKey(_value, this);
					}
					OnPropertyChanged();
				}
			}
		}

		protected string BaseText
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_text != value)
				{
					_text = value;
					OnPropertyChanged();
				}
			}
		}

		protected int BaseImageIndex
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
					if (_imageIndex != -1)
					{
						_imageKey = "";
					}
					OnPropertyChanged();
				}
			}
		}

		protected string BaseImageKey
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
					if (_imageKey.Length > 0)
					{
						_imageIndex = -1;
					}
					OnPropertyChanged();
				}
			}
		}

		protected Image BaseImage
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					OnPropertyChanged();
				}
			}
		}

		protected MordexValueItemCollectionBase BaseOwner => _ownerCollection;

		Image IMordexValueItem.Image
		{
			get
			{
				return BaseImage;
			}
			set
			{
				BaseImage = value;
			}
		}

		object IMordexValueItem.Value
		{
			get
			{
				return BaseValue;
			}
			set
			{
				BaseValue = value;
			}
		}

		string IMordexValueItem.Text
		{
			get
			{
				return BaseText;
			}
			set
			{
				BaseText = value;
			}
		}

		object IMordexValueItem.DataRow => this;

		int IMordexValueItem.ImageIndex
		{
			get
			{
				return BaseImageIndex;
			}
			set
			{
				BaseImageIndex = value;
			}
		}

		internal static object GetValueOrNull(MordexValueItemBase mordexValueItemBase)
		{
			return mordexValueItemBase?._value;
		}

		public MordexValueItemBase()
		{
			_text = "";
			_imageIndex = -1;
			_imageKey = "";
		}

		public MordexValueItemBase(object value, string text, int imageIndex)
		{
			if (text == null)
			{
				text = "";
			}
			_value = value;
			_text = text;
			_imageIndex = imageIndex;
			_imageKey = "";
		}

		public MordexValueItemBase(object value, string text, string imageKey)
		{
			if (text == null)
			{
				text = "";
			}
			_value = value;
			_text = text;
			_imageIndex = -1;
			_imageKey = imageKey;
		}

		public MordexValueItemBase(object value, string text, Image image)
		{
			if (text == null)
			{
				text = "";
			}
			_value = value;
			_text = text;
			_image = image;
			_imageIndex = -1;
			_imageKey = "";
		}

		protected virtual void OnPropertyChanged()
		{
		}

		internal MordexValueItemCollectionBase GetOwnerCollection()
		{
			return _ownerCollection;
		}

		internal void SetOwnerCollection(MordexValueItemCollectionBase mordexValueItemCollectionBase)
		{
			_ownerCollection = mordexValueItemCollectionBase;
		}
	}
}
