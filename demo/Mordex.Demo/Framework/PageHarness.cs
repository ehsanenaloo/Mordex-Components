using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mordex.Demo
{
	/// <summary>Loads every demo page in a plain form, to check them or to write one picture of each.</summary>
	public static class PageHarness
	{
		/// <summary>Creates, shows and checks every page. Returns the number of pages that failed.</summary>
		public static int Run(StringBuilder log, string screenshotFolder, string filter)
		{
			int failures = 0;
			if (screenshotFolder != null)
			{
				Directory.CreateDirectory(screenshotFolder);
			}
			using (PictureHost form = new PictureHost(new Point(0, 0), new Size(1360, 860)))
			{
				form.Show();
				foreach (DemoPage page in DemoPage.CreateAll())
				{
					if (filter != null && page.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 && page.GetType().Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
					{
						page.Dispose();
						continue;
					}
					try
					{
						form.Controls.Clear();
						form.Controls.Add(page);
						page.CreateControl();
						page.OnShown();
						for (int i = 0; i < 4; i++)
						{
							Application.DoEvents();
						}
						if (screenshotFolder != null)
						{
							form.DropFocus();
							using (Bitmap bitmap = new Bitmap(page.Width, page.Height))
							{
								page.DrawToBitmap(bitmap, new Rectangle(Point.Empty, page.Size));
								bitmap.Save(Path.Combine(screenshotFolder, Safe(page.Group + " - " + page.Title) + ".png"), ImageFormat.Png);
							}
						}
						log.AppendLine("OK   " + page.Group + " / " + page.Title);
					}
					catch (Exception ex)
					{
						failures++;
						log.AppendLine("FAIL " + page.Group + " / " + page.Title + ": " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
					}
					finally
					{
						form.Controls.Clear();
						page.Dispose();
					}
				}
			}
			return failures;
		}

		private static string Safe(string name)
		{
			foreach (char c in Path.GetInvalidFileNameChars())
			{
				name = name.Replace(c, '_');
			}
			return name;
		}
	}
}
