using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// The about box shared by the design-time dialogs of every module.
	public class MxAboutDialog : Form
	{
		private const int HeaderHeight = 92;

		private readonly Image logo;

		private readonly string product;

		private readonly string version;

		public MxAboutDialog(Assembly assembly)
		{
			product = ReadAssemblyAttribute(assembly, typeof(AssemblyProductAttribute), a => ((AssemblyProductAttribute)a).Product, assembly.GetName().Name);
			string description = ReadAssemblyAttribute(assembly, typeof(AssemblyDescriptionAttribute), a => ((AssemblyDescriptionAttribute)a).Description, string.Empty);
			string copyright = ReadAssemblyAttribute(assembly, typeof(AssemblyCopyrightAttribute), a => ((AssemblyCopyrightAttribute)a).Copyright, string.Empty);
			version = ReadAssemblyAttribute(assembly, typeof(AssemblyFileVersionAttribute), a => ((AssemblyFileVersionAttribute)a).Version, assembly.GetName().Version.ToString());
			using (Stream stream = typeof(MxAboutDialog).Assembly.GetManifestResourceStream("MxLogo.png"))
			{
				if (stream != null)
				{
					// A decoded image keeps reading its stream, so keep a copy.
					using Image decoded = Image.FromStream(stream);
					logo = new Bitmap(decoded);
				}
			}

			AutoScaleDimensions = new SizeF(96f, 96f);
			AutoScaleMode = AutoScaleMode.Dpi;
			Font = SystemFonts.MessageBoxFont;
			Text = "About " + product;
			ClientSize = new Size(440, 252);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			ShowIcon = false;
			// The designer shows this dialog from another process without an owner window, so it must open on top of the host.
			TopMost = true;
			ShowInTaskbar = true;
			StartPosition = FormStartPosition.CenterScreen;
			BackColor = Color.White;
			DoubleBuffered = true;

			HeaderPanel header = new HeaderPanel(logo, product) { Dock = DockStyle.Top, Height = HeaderHeight };
			TableLayoutPanel body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(24, 16, 24, 16), BackColor = Color.White };
			body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			Label descriptionLabel = new Label { Text = description, AutoSize = true, MaximumSize = new Size(392, 0), Margin = new Padding(0, 0, 0, 10), ForeColor = Color.FromArgb(60, 60, 60) };
			Label versionLabel = new Label { Text = "Version " + version, AutoSize = true, Margin = new Padding(0, 0, 0, 4), ForeColor = Color.FromArgb(60, 60, 60) };
			Label copyrightLabel = new Label { Text = copyright, AutoSize = true, MaximumSize = new Size(392, 0), ForeColor = Color.FromArgb(110, 110, 110) };
			Button closeButton = new Button { Text = "Close", DialogResult = DialogResult.OK, FlatStyle = FlatStyle.System, Size = new Size(84, 28), Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
			body.Controls.Add(descriptionLabel, 0, 0);
			body.Controls.Add(versionLabel, 0, 1);
			body.Controls.Add(copyrightLabel, 0, 2);
			body.Controls.Add(closeButton, 0, 3);
			Controls.Add(body);
			Controls.Add(header);
			AcceptButton = closeButton;
			CancelButton = closeButton;
			Mordex.Windows.Common.ControlThemer.AttachDialog(this);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			Activate();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				logo?.Dispose();
			}
			base.Dispose(disposing);
		}

		private static string ReadAssemblyAttribute(Assembly assembly, Type type, Func<object, string> read, string fallback)
		{
			object[] found = assembly.GetCustomAttributes(type, false);
			string value = found.Length == 0 ? null : read(found[0]);
			return string.IsNullOrWhiteSpace(value) ? fallback : value;
		}

		private sealed class HeaderPanel : Panel
		{
			private readonly Image logo;

			private readonly string product;

			public HeaderPanel(Image logo, string product)
			{
				this.logo = logo;
				this.product = product;
				DoubleBuffered = true;
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);
				Rectangle header = ClientRectangle;
				using (LinearGradientBrush brush = new LinearGradientBrush(header, Color.FromArgb(35, 135, 224), Color.FromArgb(10, 70, 140), LinearGradientMode.Horizontal))
				{
					e.Graphics.FillRectangle(brush, header);
				}
				e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				int pad = LogicalToDeviceUnits(24);
				int logoSize = LogicalToDeviceUnits(56);
				int top = (header.Height - logoSize) / 2;
				if (logo != null)
				{
					e.Graphics.DrawImage(logo, new Rectangle(pad, top, logoSize, logoSize));
				}
				int textLeft = pad + logoSize + LogicalToDeviceUnits(16);
				using (Font titleFont = new Font(Font.FontFamily, 16f, FontStyle.Bold))
				using (Font subFont = new Font(Font.FontFamily, 9f))
				using (SolidBrush white = new SolidBrush(Color.White))
				using (SolidBrush soft = new SolidBrush(Color.FromArgb(210, 230, 250)))
				{
					e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
					e.Graphics.DrawString(product, titleFont, white, textLeft, top + LogicalToDeviceUnits(4));
					e.Graphics.DrawString("Mordex Components", subFont, soft, textLeft, top + LogicalToDeviceUnits(34));
				}
			}
		}
	}
}
