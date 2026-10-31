using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommand
	{
		[Localizable(true)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed by the command.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		public int ImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _imageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.ImageIndex;
				}
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList displayed on the command.")]
		[Localizable(true)]
		public string ImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _imageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.ImageKey;
				}
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
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Localizable(true)]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed by the command when it is in large icons view.")]
		public int LargeImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _largeImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.LargeImageIndex;
				}
				return _largeImageIndex;
			}
			set
			{
				if (_largeImageIndex != value)
				{
					_largeImageIndex = value;
					_largeImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		[Description("Represents the key accessor for the image in the ImageList displayed by the command when it is in large icons view.")]
		public string LargeImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _largeImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.LargeImageKey;
				}
				return _largeImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeImageKey != value)
				{
					_largeImageKey = value;
					_largeImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Description("Represents the image list index value of the image displayed by the command when the mouse is over it.")]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		public int HotImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _hotImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.HotImageIndex;
				}
				return _hotImageIndex;
			}
			set
			{
				if (_hotImageIndex != value)
				{
					_hotImageIndex = value;
					_hotImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		[Description("Represents the key accessor for the image in the ImageList displayed by the command when the mouse is over it.")]
		public string HotImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _hotImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.HotImageKey;
				}
				return _hotImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_hotImageKey != value)
				{
					_hotImageKey = value;
					_hotImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Localizable(true)]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed by the command when it is in large icons view and the mouse is over it.")]
		public int LargeHotImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _largeHotImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.LargeHotImageIndex;
				}
				return _largeHotImageIndex;
			}
			set
			{
				if (_largeHotImageIndex != value)
				{
					_largeHotImageIndex = value;
					_largeHotImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Description("Represents the key accessor for the image in the ImageList displayed by the command when it is in large icons view and the mouse is over it.")]
		public string LargeHotImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _largeHotImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.LargeHotImageKey;
				}
				return _largeHotImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeHotImageKey != value)
				{
					_largeHotImageKey = value;
					_largeHotImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Description("Represents the image list index value of the image displayed by the command when it is disabled.")]
		public int DisabledImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _disabledImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.DisabledImageIndex;
				}
				return _disabledImageIndex;
			}
			set
			{
				if (_disabledImageIndex != value)
				{
					_disabledImageIndex = value;
					_disabledImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList displayed by the command when it is disabled.")]
		public string DisabledImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _disabledImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.DisabledImageKey;
				}
				return _disabledImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_disabledImageKey != value)
				{
					_disabledImageKey = value;
					_disabledImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed by the command when it is in large icons view and disabled.")]
		public int LargeDisabledImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _largeDisabledImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.LargeDisabledImageIndex;
				}
				return _largeDisabledImageIndex;
			}
			set
			{
				if (_largeDisabledImageIndex != value)
				{
					_largeDisabledImageIndex = value;
					_largeDisabledImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList on the command when it is in large icons view and disabled.")]
		public string LargeDisabledImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _largeDisabledImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.LargeDisabledImageKey;
				}
				return _largeDisabledImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeDisabledImageKey != value)
				{
					_largeDisabledImageKey = value;
					_largeDisabledImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed by the UICommand when it is selected.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Localizable(true)]
		public int SelectedImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _selectedImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.SelectedImageIndex;
				}
				return _selectedImageIndex;
			}
			set
			{
				if (_selectedImageIndex != value)
				{
					_selectedImageIndex = value;
					_selectedImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Description("The key accessor for the image in the ImageList displayed by the UICommand when it is selected.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		public string SelectedImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _selectedImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.SelectedImageKey;
				}
				return _selectedImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_selectedImageKey != value)
				{
					_selectedImageKey = value;
					_selectedImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Description("The image list index value of the image displayed by the UICommand when it is in large icons view and the command is selected.")]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		public int LargeSelectedImageIndex
		{
			get
			{
				if (!IsDesignerEditing() && _largeSelectedImageIndex == -1 && _baseCommand != null)
				{
					return _baseCommand.LargeSelectedImageIndex;
				}
				return _largeSelectedImageIndex;
			}
			set
			{
				if (_largeSelectedImageIndex != value)
				{
					_largeSelectedImageIndex = value;
					_largeSelectedImageKey = "";
					NotifyInvalidated();
				}
			}
		}

		[Description("The key accessor for the image in the ImageList displayed by the UICommand when it is in large icons view and the command is selected.")]
		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		public string LargeSelectedImageKey
		{
			get
			{
				if (!IsDesignerEditing() && _largeSelectedImageKey.Length == 0 && _baseCommand != null)
				{
					return _baseCommand.LargeSelectedImageKey;
				}
				return _largeSelectedImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_largeSelectedImageKey != value)
				{
					_largeSelectedImageKey = value;
					_largeSelectedImageIndex = -1;
					NotifyInvalidated();
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the number of days the command could pass without being clicked before it became infrequent.")]
		[DefaultValue(-1)]
		public int DaysForInfrequency
		{
			get
			{
				if (!IsDesignerEditing() && _baseCommand != null)
				{
					return _baseCommand.DaysForInfrequency;
				}
				return _daysForInfrequency;
			}
			set
			{
				if (DaysForInfrequency != value)
				{
					if (_baseCommand != null)
					{
						_baseCommand.DaysForInfrequency = value;
					}
					else
					{
						_daysForInfrequency = value;
					}
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the Color in the image that will be replaced for the selected color of the UIColorPicker control associated with the command")]
		public Color ImageReplaceableColor
		{
			get
			{
				if (!IsDesignerEditing() && _imageReplaceableColor.IsEmpty && _baseCommand != null)
				{
					return _baseCommand._imageReplaceableColor;
				}
				return _imageReplaceableColor;
			}
			set
			{
				if (_imageReplaceableColor != value)
				{
					_imageReplaceableColor = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("The image that is displayed on the UICommand.")]
		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		public Image Image
		{
			get
			{
				if (!IsDesignerEditing() && _commandImage == null && _baseCommand != null)
				{
					return _baseCommand.Image;
				}
				return _commandImage;
			}
			set
			{
				if (_commandImage != value)
				{
					_commandImage = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Represents the key used to identify an object in a collection")]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Key
		{
			get
			{
				if (_baseCommand != null && CommandType != CommandType.Separator)
				{
					return _baseCommand.Key;
				}
				return _key;
			}
			set
			{
				if (Key != value)
				{
					if (_baseCommand != null && CommandType != CommandType.Separator)
					{
						throw new InvalidOperationException();
					}
					if (value.Length <= 0)
					{
						throw new ArgumentException("The Command Key cannot be blank.");
					}
					_key = value;
				}
			}
		}
	}
}
