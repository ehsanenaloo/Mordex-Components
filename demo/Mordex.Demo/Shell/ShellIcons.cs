using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Demo
{
	/// <summary>Small flat icons drawn in code, so the demo needs no image files.</summary>
	public static class ShellIcons
	{
		/// <summary>A small picture of a window in the colors of a theme: caption, toolbar, body, selection and accent.</summary>
		public static Bitmap CreateThemeSwatch(Mordex.Windows.Common.MxTheme theme, int size)
		{
			Bitmap bitmap = new Bitmap(size, size);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				float u = size / 32f;
				Color Pick(Mordex.Windows.Common.ThemeRole role) => theme.GetColor(role);
				Rectangle window = new Rectangle((int)(2 * u), (int)(3 * u), (int)(28 * u), (int)(26 * u));
				using (Brush body = new SolidBrush(Pick(Mordex.Windows.Common.ThemeRole.Surface)))
				using (Brush caption = new SolidBrush(Pick(Mordex.Windows.Common.ThemeRole.Caption)))
				using (Brush toolbar = new SolidBrush(Pick(Mordex.Windows.Common.ThemeRole.ToolbarBack)))
				using (Brush selection = new SolidBrush(Pick(Mordex.Windows.Common.ThemeRole.Selection)))
				using (Brush accent = new SolidBrush(Pick(Mordex.Windows.Common.ThemeRole.Accent)))
				using (Brush text = new SolidBrush(Pick(Mordex.Windows.Common.ThemeRole.TextMuted)))
				using (Pen border = new Pen(Pick(Mordex.Windows.Common.ThemeRole.BorderStrong), Math.Max(1f, u)))
				{
					g.FillRectangle(body, window);
					g.FillRectangle(caption, window.X, window.Y, window.Width, (int)(6 * u));
					g.FillRectangle(toolbar, window.X, window.Y + (int)(6 * u), window.Width, (int)(5 * u));
					g.FillRectangle(selection, window.X + (int)(3 * u), window.Y + (int)(14 * u), (int)(14 * u), (int)(3 * u));
					g.FillRectangle(text, window.X + (int)(3 * u), window.Y + (int)(19 * u), (int)(11 * u), (int)(2 * u));
					g.FillRectangle(accent, window.Right - (int)(8 * u), window.Bottom - (int)(8 * u), (int)(5 * u), (int)(5 * u));
					g.DrawRectangle(border, window);
				}
			}
			return bitmap;
		}

		public static Bitmap Create(string kind, int size)
		{
			Bitmap bitmap = new Bitmap(size, size);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				float u = size / 16f;
				Color accent = AccentFor(kind);
				using (Brush fill = new SolidBrush(accent))
				using (Pen line = new Pen(Color.White, Math.Max(1f, u * 1.2f)))
				{
					g.FillRectangle(fill, 1 * u, 1 * u, 14 * u, 14 * u);
					switch (kind)
					{
					case "grid":
						for (int i = 1; i < 3; i++)
						{
							g.DrawLine(line, 1 * u, (1 + i * 14f / 3) * u, 15 * u, (1 + i * 14f / 3) * u);
							g.DrawLine(line, (1 + i * 14f / 3) * u, 1 * u, (1 + i * 14f / 3) * u, 15 * u);
						}
						break;
					case "calendar":
						g.DrawLine(line, 1 * u, 5 * u, 15 * u, 5 * u);
						g.FillRectangle(Brushes.White, 4 * u, 7.5f * u, 2.5f * u, 2.5f * u);
						g.FillRectangle(Brushes.White, 9 * u, 7.5f * u, 2.5f * u, 2.5f * u);
						g.FillRectangle(Brushes.White, 4 * u, 11 * u, 2.5f * u, 2.5f * u);
						break;
					case "timeline":
						g.FillRectangle(Brushes.White, 3 * u, 4 * u, 6 * u, 2.5f * u);
						g.FillRectangle(Brushes.White, 6 * u, 8 * u, 7 * u, 2.5f * u);
						g.FillRectangle(Brushes.White, 4 * u, 12 * u, 5 * u, 2f * u);
						break;
					case "tree":
						g.DrawLine(line, 4 * u, 4 * u, 4 * u, 12 * u);
						g.DrawLine(line, 4 * u, 6 * u, 11 * u, 6 * u);
						g.DrawLine(line, 4 * u, 12 * u, 11 * u, 12 * u);
						break;
					case "filter":
						g.FillPolygon(Brushes.White, new[] { new PointF(3 * u, 4 * u), new PointF(13 * u, 4 * u), new PointF(9 * u, 9 * u), new PointF(9 * u, 13 * u), new PointF(7 * u, 12 * u), new PointF(7 * u, 9 * u) });
						break;
					case "edit":
						g.DrawLine(line, 4 * u, 12 * u, 12 * u, 4 * u);
						g.FillRectangle(Brushes.White, 3 * u, 12 * u, 3 * u, 1.5f * u);
						break;
					case "export":
						g.DrawLine(line, 8 * u, 3 * u, 8 * u, 10 * u);
						g.DrawLine(line, 5 * u, 7 * u, 8 * u, 10 * u);
						g.DrawLine(line, 11 * u, 7 * u, 8 * u, 10 * u);
						g.DrawLine(line, 4 * u, 13 * u, 12 * u, 13 * u);
						break;
					case "nav":
						g.FillRectangle(Brushes.White, 3 * u, 3 * u, 4 * u, 10 * u);
						g.DrawLine(line, 9 * u, 5 * u, 13 * u, 5 * u);
						g.DrawLine(line, 9 * u, 8 * u, 13 * u, 8 * u);
						g.DrawLine(line, 9 * u, 11 * u, 13 * u, 11 * u);
						break;
					case "input":
						g.DrawRectangle(line, 3 * u, 5 * u, 10 * u, 6 * u);
						g.DrawLine(line, 5 * u, 8 * u, 8 * u, 8 * u);
						break;
					case "layout":
						g.DrawLine(line, 8 * u, 1 * u, 8 * u, 15 * u);
						g.DrawLine(line, 1 * u, 8 * u, 8 * u, 8 * u);
						break;
					default:
						g.FillEllipse(Brushes.White, 5 * u, 5 * u, 6 * u, 6 * u);
						break;
					}
				}
			}
			return bitmap;
		}

		private static Color AccentFor(string kind)
		{
			switch (kind)
			{
			case "grid":
			case "filter":
			case "tree":
			case "edit":
			case "export":
				return Color.FromArgb(43, 110, 188);
			case "calendar":
			case "timeline":
				return Color.FromArgb(46, 150, 110);
			case "nav":
			case "input":
				return Color.FromArgb(204, 112, 40);
			default:
				return Color.FromArgb(126, 87, 170);
			}
		}

		/// <summary>The icon kind for a navigation group name.</summary>
		public static string KindForGroup(string group)
		{
			switch (group)
			{
			case "Data grid":
				return "grid";
			case "Scheduling":
				return "calendar";
			case "Navigation":
				return "nav";
			case "Inputs":
				return "input";
			case "Output and help":
				return "default";
			default:
				return "layout";
			}
		}
	}
}
