using System.Drawing;
using System.Globalization;
using System.Resources;

namespace Mordex.Windows.DataGrid
{
	internal class ResourceImages
	{
		private static ResourceManager _resourceManager;

		internal ResourceImages()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (object.ReferenceEquals(_resourceManager, null))
			{
				ResourceManager resourceManager = new ResourceManager(typeof(GridBuiltInImages));
				_resourceManager = resourceManager;
			}
			return _resourceManager;
		}

		internal static Bitmap GetChevronDownImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageChevronDown", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetChevronUpImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageChevronUp", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetCurrentRowImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageCurrentRow", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetDownXpImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageDownXP", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetEditingImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageEditing", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetErrorImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageError", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilter", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFirstBlackImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFirstBlack", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFirstBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFirstBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFirstGreenImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFirstGreen", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetLastBlackImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageLastBlack", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetLastBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageLastBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetLastGreenImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageLastGreen", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNewRecordBlackImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNewRecordBlack", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNewRecordBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNewRecordBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNewRecordGreenImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNewRecordGreen", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNewXpImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNewXP", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNextBlackImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNextBlack", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNextBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNextBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetNextGreenImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNextGreen", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetPreviousBlackImage()
		{
			object otherObject = GetResourceManager().GetObject("ImagePreviousBlack", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetPreviousBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImagePreviousBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetPreviousGreenImage()
		{
			object otherObject = GetResourceManager().GetObject("ImagePreviousGreen", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetRemoveFilterImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageRemoveFilter", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetSumImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageSum", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetUpXPImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageUpXP", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterBeginsWithImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterBeginsWith", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterClearImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterClear", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterContainsImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterContains", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterEndsWithImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterEndsWith", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterEqualImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterEqual", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterGreaterThanImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterGreaterThan", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterGreaterThanorEqualsToImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterGreaterThanorEqualsTo", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterLessThanImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterLessThan", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterLessThanorEqualsToImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterLessThanorEqualsTo", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetFilterNotEqualsImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageFilterNotEquals", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetEmptyImage()
		{
			object otherObject = GetResourceManager().GetObject("EmptyImage", null);
			return (Bitmap)otherObject;
		}
	}
}
