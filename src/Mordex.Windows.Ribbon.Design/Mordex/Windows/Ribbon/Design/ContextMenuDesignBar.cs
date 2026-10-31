using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon.Design
{
	internal class ContextMenuDesignBar : Control
	{
		private Color _tabStripColor;

		private Color _selectedTabBackgroundColor;

		private Color _selectedTabBackgroundGradientColor;

		private Color _tabBorderColor;

		private Color _borderLightColor;

		private Color _borderColor;

		private Color _textColor;

		private const int CornerSize = 6;

		private const int TabLeft = 5;

		private const int TabTextPadding = 10;

		public ContextMenuDesignBar(string text)
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			Text = text;
			LoadColors();
		}

		private void LoadColors()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			_tabStripColor = theme.GetColor(ThemeRole.Surface);
			_selectedTabBackgroundColor = theme.GetColor(ThemeRole.SurfaceRaised);
			_selectedTabBackgroundGradientColor = theme.GetColor(ThemeRole.TabActive);
			_tabBorderColor = theme.GetColor(ThemeRole.Border);
			_borderLightColor = theme.GetColor(ThemeRole.SurfaceRaised);
			_borderColor = theme.GetColor(ThemeRole.Border);
			_textColor = theme.GetColor(ThemeRole.Text);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			using (Brush brush = new SolidBrush(_tabStripColor))
			{
				eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
			}
			DrawSelectedBackground(eventArgs.Graphics);
		}

		private void DrawSelectedBackground(Graphics graphics)
		{
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			try
			{
				int width = (int)graphics.MeasureString(Text, Font).Width + TabTextPadding;
				Rectangle rectangle = new Rectangle(TabLeft, 0, width, base.Height);
				rectangle.Height++;
				Rectangle rect = rectangle;
				rect.Inflate(-1, -1);
				rect.Height++;
				if (rect.Height <= 0 || rect.Width <= 0)
				{
					return;
				}
				using (Brush brush = new LinearGradientBrush(rect, _selectedTabBackgroundColor, _selectedTabBackgroundGradientColor, LinearGradientMode.ForwardDiagonal))
				{
					graphics.FillRectangle(brush, rect);
				}
				using (Pen pen = new Pen(_tabBorderColor))
				{
					graphics.DrawArc(pen, rectangle.X, rectangle.Y, CornerSize, CornerSize, -180, 90);
					graphics.DrawLine(pen, rectangle.X + 2, rectangle.Y, rectangle.Right - 3, rectangle.Y);
					graphics.DrawArc(pen, rectangle.Right - 7, rectangle.Y, CornerSize, CornerSize, -90, 90);
					graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom - 1);
					graphics.DrawLine(pen, rectangle.X, rectangle.Bottom - 1, rectangle.X, rectangle.Y + 2);
				}
				using (Pen innerBorderPen = new Pen(Color.FromArgb(120, _borderLightColor)))
				{
					rectangle.Inflate(-1, -1);
					graphics.DrawArc(innerBorderPen, rectangle.X, rectangle.Y, CornerSize, CornerSize, -180, 90);
					graphics.DrawArc(innerBorderPen, rectangle.Right - 7, rectangle.Y, CornerSize, CornerSize, -90, 90);
					graphics.DrawLine(innerBorderPen, rectangle.X, rectangle.Y + 2, rectangle.X, rectangle.Bottom);
					graphics.DrawLine(innerBorderPen, rectangle.Right - 1, rectangle.Y + 2, rectangle.Right - 1, rectangle.Bottom);
					graphics.DrawLine(innerBorderPen, rectangle.X, rectangle.Bottom - 1, rectangle.X - 1, rectangle.Bottom);
					graphics.DrawLine(innerBorderPen, rectangle.Right - 1, rectangle.Bottom - 1, rectangle.Right, rectangle.Bottom);
				}
				using (Pen highlightPen = new Pen(Color.FromArgb(80, MxThemeManager.DefaultTheme.GetColor(ThemeRole.SurfaceRaised))))
				{
					graphics.DrawLine(highlightPen, rectangle.X + 2, rectangle.Y, rectangle.Right - 3, rectangle.Y);
					rectangle.Inflate(-1, -1);
					rectangle.Height++;
					graphics.DrawLine(highlightPen, rectangle.X, rectangle.Y + 1, rectangle.X, rectangle.Bottom);
					graphics.DrawLine(highlightPen, rectangle.Right - 1, rectangle.Y + 1, rectangle.Right - 1, rectangle.Bottom);
					graphics.DrawArc(highlightPen, rectangle.X, rectangle.Y, CornerSize, CornerSize, -180, 90);
					graphics.DrawArc(highlightPen, rectangle.Right - 7, rectangle.Y, CornerSize, CornerSize, -90, 90);
				}
				rectangle = base.Bounds;
				rectangle.X--;
				Color border = _borderColor;
				using (Brush shadowBrush = new LinearGradientBrush(rectangle, Color.FromArgb(10, border), Color.FromArgb(20, border), LinearGradientMode.Vertical))
				using (Pen shadowPen = new Pen(shadowBrush))
				{
					graphics.DrawLine(shadowPen, rectangle.X, rectangle.Y + 2, rectangle.X, rectangle.Bottom - 1);
					graphics.DrawLine(shadowPen, rectangle.Right + 2, rectangle.Y + 4, rectangle.Right + 2, rectangle.Bottom - 1);
				}
				using (Brush shadowBrush = new LinearGradientBrush(rectangle, Color.FromArgb(5, border), Color.FromArgb(15, border), LinearGradientMode.Vertical))
				using (Pen shadowPen = new Pen(shadowBrush))
				{
					graphics.DrawLine(shadowPen, rectangle.X - 1, rectangle.Y + 3, rectangle.X - 1, rectangle.Bottom - 1);
					graphics.DrawArc(shadowPen, rectangle.X, rectangle.Y, CornerSize, CornerSize, -180, 90);
				}
				rectangle.X++;
				using (Brush shadowBrush = new LinearGradientBrush(rectangle, Color.FromArgb(50, border), Color.FromArgb(60, border), LinearGradientMode.Vertical))
				using (Pen shadowPen = new Pen(shadowBrush))
				{
					graphics.DrawLine(shadowPen, rectangle.Right, rectangle.Y + 2, rectangle.Right, rectangle.Bottom - 1);
					graphics.DrawArc(shadowPen, rectangle.Right - 7, rectangle.Y, CornerSize, CornerSize, -90, 90);
				}
				graphics.SmoothingMode = smoothingMode;
				rectangle = new Rectangle(TabLeft, 0, width, base.Height);
				using (StringFormat stringFormat = new StringFormat())
				{
					stringFormat.LineAlignment = StringAlignment.Center;
					stringFormat.Alignment = StringAlignment.Center;
					using (Brush textBrush = new SolidBrush(_textColor))
					{
						graphics.DrawString(Text, Font, textBrush, rectangle, stringFormat);
					}
				}
			}
			finally
			{
				graphics.SmoothingMode = smoothingMode;
			}
		}
	}
}
