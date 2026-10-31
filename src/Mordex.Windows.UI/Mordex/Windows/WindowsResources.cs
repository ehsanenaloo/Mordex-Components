using System.Drawing;
using System.Resources;

namespace Mordex.Windows
{
	internal class WindowsResources
	{
		private static ResourceManager _resourceManager;

		internal WindowsResources()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (_resourceManager == null)
			{
				ResourceManager resourceManager = new ResourceManager(typeof(ResourceFile));
				_resourceManager = resourceManager;
			}
			return _resourceManager;
		}

		internal static Bitmap ActiveFiles()
		{
			return (Bitmap)GetResourceManager().GetObject("ActiveFiles", null);
		}

		internal static Bitmap ArrowTBLow()
		{
			return (Bitmap)GetResourceManager().GetObject("ArrowTBLow", null);
		}

		internal static Bitmap Check()
		{
			return (Bitmap)GetResourceManager().GetObject("Check", null);
		}

		internal static Bitmap ChevronDownXP()
		{
			return (Bitmap)GetResourceManager().GetObject("ChevronDownXP", null);
		}

		internal static Bitmap ChevronOutlook()
		{
			return (Bitmap)GetResourceManager().GetObject("ChevronOutlook", null);
		}

		internal static Bitmap ChevronRight()
		{
			return (Bitmap)GetResourceManager().GetObject("ChevronRight", null);
		}

		internal static Bitmap Close()
		{
			return (Bitmap)GetResourceManager().GetObject("Close", null);
		}

		internal static Bitmap CloseVs2010()
		{
			return (Bitmap)GetResourceManager().GetObject("Close2010", null);
		}

		internal static Bitmap Copy()
		{
			return (Bitmap)GetResourceManager().GetObject("Copy", null);
		}

		internal static Bitmap Cut()
		{
			return (Bitmap)GetResourceManager().GetObject("Cut", null);
		}

		internal static Bitmap DownArrow()
		{
			return (Bitmap)GetResourceManager().GetObject("DownArrow", null);
		}

		internal static byte[] DragPage()
		{
			return (byte[])GetResourceManager().GetObject("DragPage", null);
		}

		internal static Bitmap DropTargetAll()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetAll", null);
		}

		internal static Bitmap DropTargetBottom()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetBottom", null);
		}

		internal static Bitmap DropTargetLeft()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetLeft", null);
		}

		internal static Bitmap DropTargetRight()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetRight", null);
		}

		internal static Bitmap DropTargetStatic()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetStatic", null);
		}

		internal static Bitmap Vs2010DropTargetAll()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetAll2010", null);
		}

		internal static Bitmap Vs2010DropTargetBottom()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetBottom2010", null);
		}

		internal static Bitmap Vs2010DropTargetLeft()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetLeft2010", null);
		}

		internal static Bitmap Vs2010DropTargetRight()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetRight2010", null);
		}

		internal static Bitmap Vs2010DropTargetStatic()
		{
			return (Bitmap)GetResourceManager().GetObject("DropTargetStatic2010", null);
		}

		internal static Bitmap CloseImage()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageClose", null);
		}

		internal static Bitmap DownArrowImage()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageDownArrow", null);
		}

		internal static Bitmap OfficePinImage()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageOfficePin", null);
		}

		internal static Bitmap OfficeUnpinImage()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageOfficeUnpin", null);
		}

		internal static Bitmap OfficePinVs2010()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageOfficePin2010", null);
		}

		internal static Bitmap OfficeUnpinVs2010()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageOfficeUnPin2010", null);
		}

		internal static Bitmap PinImage()
		{
			return (Bitmap)GetResourceManager().GetObject("ImagePin", null);
		}

		internal static Bitmap UnpinImage()
		{
			return (Bitmap)GetResourceManager().GetObject("ImageUnpin", null);
		}

		internal static Bitmap Minimize()
		{
			return (Bitmap)GetResourceManager().GetObject("Minimize", null);
		}

		internal static byte[] NoDragPage()
		{
			return (byte[])GetResourceManager().GetObject("NoDragPage", null);
		}

		internal static Bitmap OutPanelDownArrow()
		{
			return (Bitmap)GetResourceManager().GetObject("OutPanelDownArrow", null);
		}

		internal static Bitmap OutPanelUpArrow()
		{
			return (Bitmap)GetResourceManager().GetObject("OutPanelUpArrow", null);
		}

		internal static Bitmap Paste()
		{
			return (Bitmap)GetResourceManager().GetObject("Paste", null);
		}

		internal static Bitmap Restore()
		{
			return (Bitmap)GetResourceManager().GetObject("Restore", null);
		}

		internal static byte[] ToolBarDrag()
		{
			return (byte[])GetResourceManager().GetObject("ToolBarDrag", null);
		}

		internal static byte[] ToolBarInvalidDrag()
		{
			return (byte[])GetResourceManager().GetObject("ToolBarInvalidDrag", null);
		}

		internal static byte[] ToolBarMove()
		{
			return (byte[])GetResourceManager().GetObject("ToolBarMove", null);
		}

		internal static Bitmap TTFont()
		{
			return (Bitmap)GetResourceManager().GetObject("TTFont", null);
		}

		internal static Bitmap Undo()
		{
			return (Bitmap)GetResourceManager().GetObject("Undo", null);
		}

		internal static Bitmap UpArrow()
		{
			return (Bitmap)GetResourceManager().GetObject("UpArrow", null);
		}
	}
}
