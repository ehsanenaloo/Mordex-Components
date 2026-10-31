using System.Drawing;
using System.Globalization;
using System.Resources;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerImageResources
	{
		private static ResourceManager _resourceManager;

		internal SchedulerImageResources()
		{
		}

		internal static ResourceManager GetResourceManager()
		{
			if (object.ReferenceEquals(_resourceManager, null))
			{
				ResourceManager resourceManager = new ResourceManager(typeof(ResourceFile));
				_resourceManager = resourceManager;
			}
			return _resourceManager;
		}

		internal static byte[] GetArrowCursorBytes()
		{
			object resource = GetResourceManager().GetObject("Arrow", null);
			return (byte[])resource;
		}

		internal static Bitmap GetDayClockBitmap()
		{
			object resource = GetResourceManager().GetObject("clockday", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetNightClockBitmap()
		{
			object resource = GetResourceManager().GetObject("clocknight", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetLeftArrowBitmap()
		{
			object resource = GetResourceManager().GetObject("LeftXP", null);
			return (Bitmap)resource;
		}

		// Shared by all controls and kept for the life of the process.
		private static Bitmap _transparentLeftArrowBitmap;

		private static Bitmap _transparentRightArrowBitmap;

		internal static Bitmap GetTransparentLeftArrowBitmap()
		{
			return _transparentLeftArrowBitmap ??= SchedulerRenderer.MakeMagentaTransparent(GetLeftArrowBitmap());
		}

		internal static Bitmap GetTransparentRightArrowBitmap()
		{
			return _transparentRightArrowBitmap ??= SchedulerRenderer.MakeMagentaTransparent(GetRightArrowBitmap());
		}

		internal static Bitmap GetRecurrenceBitmap()
		{
			object resource = GetResourceManager().GetObject("Recurrence", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetRecurrenceExceptionBitmap()
		{
			object resource = GetResourceManager().GetObject("RecurrenceException", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetReminderBitmap()
		{
			object resource = GetResourceManager().GetObject("Reminder", null);
			return (Bitmap)resource;
		}

		internal static Bitmap GetRightArrowBitmap()
		{
			object resource = GetResourceManager().GetObject("RightXP", null);
			return (Bitmap)resource;
		}
	}
}
