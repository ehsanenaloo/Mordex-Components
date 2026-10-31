using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Mordex.Windows.DataGrid
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[CompilerGenerated]
	[DebuggerNonUserCode]
	internal class GridBuiltInImages
	{
		private static ResourceManager _resourceManager;

		internal GridBuiltInImages()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (object.ReferenceEquals(_resourceManager, null))
			{
				ResourceManager resourceManager = new ResourceManager("Mordex.Windows.DataGrid.GridBuiltInImages", typeof(GridBuiltInImages).Assembly);
				_resourceManager = resourceManager;
			}
			return _resourceManager;
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

		internal static Bitmap GetNextBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageNextBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetPreviousBlueImage()
		{
			object otherObject = GetResourceManager().GetObject("ImagePreviousBlue", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetRemoveFilterImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageRemoveFilter", null);
			return (Bitmap)otherObject;
		}

		internal static Bitmap GetSilverExpandImage()
		{
			object otherObject = GetResourceManager().GetObject("ImageSilverExpand", null);
			return (Bitmap)otherObject;
		}
	}
}
