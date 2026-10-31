using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mordex.Demo
{
	public sealed class MainForm : Form
	{
		private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };

		public MainForm()
		{
			Text = "Mordex Demo";
			Size = new Size(1000, 700);
			Controls.Add(_tabs);
		}

		public int LoadAll(StringBuilder log)
		{
			int failures = 0;
			foreach (CatalogEntry entry in ControlCatalog.All())
			{
				try
				{
					Control control = entry.Create();
					control.Dock = DockStyle.Fill;
					TabPage page = new TabPage(entry.Name);
					page.Controls.Add(control);
					_tabs.TabPages.Add(page);
					_tabs.SelectedTab = page;
					control.CreateControl();
					Application.DoEvents();
					string problem = entry.Verify == null ? null : entry.Verify(control);
					if (problem != null)
					{
						failures++;
						log.AppendLine("FAIL " + entry.Name + ": " + problem);
					}
					else
					{
						log.AppendLine("OK   " + entry.Name);
					}
				}
				catch (Exception ex)
				{
					failures++;
					log.AppendLine("FAIL " + entry.Name + ": " + ex.GetType().Name + ": " + ex.Message + Environment.NewLine + ex.StackTrace);
				}
			}
			return failures;
		}
	}

	internal static class Program
	{
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr window, out ScreenRect rect);

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		private struct ScreenRect
		{
			public int Left, Top, Right, Bottom;
		}


		[STAThread]
		private static int Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			bool smoke = Array.IndexOf(args, "--smoke") >= 0;
			if (smoke || Array.IndexOf(args, "--shell-picture") >= 0 || Array.IndexOf(args, "--pages") >= 0 || Array.IndexOf(args, "--screenshots") >= 0)
			{
				// These runs are unattended: an unhandled error goes to the console and ends the run, never to a dialog.
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				Application.ThreadException += (sender, e) =>
				{
					Console.Error.WriteLine("Unhandled exception: " + e.Exception);
					Environment.Exit(3);
				};
			}
			int pagesIndex = Array.IndexOf(args, "--pages");
			int shotsIndex = Array.IndexOf(args, "--screenshots");
			if (pagesIndex >= 0 || shotsIndex >= 0)
			{
				// --pages [filter] checks the demo pages; --screenshots <folder> [filter] also writes one PNG for each.
				int at = Math.Max(pagesIndex, shotsIndex);
				string folder = shotsIndex >= 0 && shotsIndex + 1 < args.Length ? args[shotsIndex + 1] : null;
				string filter = at + (shotsIndex >= 0 ? 2 : 1) < args.Length ? args[at + (shotsIndex >= 0 ? 2 : 1)] : null;
				StringBuilder pageLog = new StringBuilder();
				int failed = PageHarness.Run(pageLog, folder, filter);
				Console.Out.Write(pageLog.ToString());
				return failed == 0 ? 0 : 1;
			}
			int shellShot = Array.IndexOf(args, "--shell-picture");
			if (shellShot >= 0 && shellShot + 1 < args.Length)
			{
				// --shell-picture <file> [page index] [theme name] writes a picture of the whole window with one page open.
				using (DemoShell shell = new DemoShell())
				{
					shell.ShowInTaskbar = false;
					shell.Size = new System.Drawing.Size(1360, 780);
					shell.Show();
					// A window started from a console is not the active window; the picture shows the active frame.
					SendMessage(shell.Handle, 0x0086, new IntPtr(1), IntPtr.Zero);
					int index = shellShot + 2 < args.Length ? (int.TryParse(args[shellShot + 2], out int parsed) ? parsed : shell.IndexOfPage(args[shellShot + 2])) : 0;
					if (shellShot + 3 < args.Length)
					{
						Mordex.Windows.Common.MxTheme named = Mordex.Windows.Common.MxThemeManager.Find(args[shellShot + 3]);
						if (named != null)
						{
							shell.UseThemedLook(named);
						}
					}
					shell.OpenPage(index);
					for (int i = 0; i < 6; i++)
					{
						Application.DoEvents();
					}
					if (shellShot + 4 < args.Length && args[shellShot + 4] == "rtl")
					{
						shell.RightToLeft = RightToLeft.Yes;
						shell.RightToLeftLayout = true;
						for (int i = 0; i < 4; i++)
						{
							Application.DoEvents();
						}
					}
					if (shellShot + 4 < args.Length && args[shellShot + 4].StartsWith("captiontabs", StringComparison.Ordinal))
					{
						// Turns on the tabs in the title bar of the Tabs page.
						System.Collections.Generic.Stack<Control> pending = new System.Collections.Generic.Stack<Control>();
						pending.Push(shell);
						while (pending.Count > 0)
						{
							Control current = pending.Pop();
							if (current is Mordex.Windows.UI.Tabs.MxTabControl tabs && tabs.TabPages.Count == 6)
							{
								if (args[shellShot + 4] == "captiontabsmany")
								{
									for (int extra = 0; extra < 30; extra++)
									{
										tabs.TabPages.Add(new Mordex.Windows.UI.Tabs.MxTabPage("Extra tab " + extra));
									}
									tabs.SelectedTab = tabs.TabPages[tabs.TabPages.Count - 1];
								}
								tabs.ShowAddTabButton = true;
								tabs.TabsInCaption = true;
								break;
							}
							foreach (Control child in current.Controls)
							{
								pending.Push(child);
							}
						}
						for (int i = 0; i < 6; i++)
						{
							Application.DoEvents();
						}
					}
					if (shellShot + 4 < args.Length && args[shellShot + 4] == "system")
					{
						shell.SetFrameMode(Mordex.Windows.Common.FormFrameMode.System);
						for (int i = 0; i < 4; i++)
						{
							Application.DoEvents();
						}
					}
					{
						// The screen holds the frame that Windows composes; DrawToBitmap cannot take it.
						shell.TopMost = true;
						shell.Activate();
						for (int i = 0; i < 6; i++)
						{
							System.Threading.Thread.Sleep(60);
							Application.DoEvents();
						}
						SendMessage(shell.Handle, 0x0086, new IntPtr(1), IntPtr.Zero);
						Application.DoEvents();
						// A window that Windows scales for a high DPI screen is smaller in its own units than on the screen.
						IntPtr previousAwareness = SetThreadDpiAwarenessContext(new IntPtr(-4));
						GetWindowRect(shell.Handle, out ScreenRect rect);
						System.Drawing.Rectangle physical = System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
						using (System.Drawing.Bitmap screen = new System.Drawing.Bitmap(physical.Width, physical.Height))
						{
							using (System.Drawing.Graphics sg = System.Drawing.Graphics.FromImage(screen))
							{
								sg.CopyFromScreen(physical.Location, System.Drawing.Point.Empty, physical.Size);
							}
							screen.Save(args[shellShot + 1], System.Drawing.Imaging.ImageFormat.Png);
						}
						SetThreadDpiAwarenessContext(previousAwareness);
					}
				}
				return 0;
			}
			if (!smoke)
			{
				Application.Run(new DemoShell());
				return 0;
			}
			StringBuilder log = new StringBuilder();
			int failures;
			using (MainForm form = new MainForm())
			{
				form.ShowInTaskbar = false;
				form.Show();
				failures = form.LoadAll(log);
			}
			using (DemoShell shell = new DemoShell())
			{
				shell.ShowInTaskbar = false;
				shell.Show();
				failures += shell.ShowEachPage(log);
			}
			Console.Out.Write(log.ToString());
			return failures == 0 ? 0 : 1;
		}
	}
}
