using System;
using System.Drawing;
using System.Media;
using System.Text;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// A message box that takes the default theme and the themed caption. It has the same buttons, icons, default button,
	/// Esc key, Ctrl+C copy and dialog results as <see cref="MessageBox"/>, so each Show call of the Windows box can be
	/// changed to it one for one.
	/// </summary>
	public static class MxMessageBox
	{
		// Tests set this to look at the box and give a result without showing a modal window.
		internal static Func<Form, DialogResult> ShowHook;

		public static DialogResult Show(string text)
		{
			return Show(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption)
		{
			return Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
		{
			return Show(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return Show(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return Show(null, text, caption, buttons, icon, defaultButton);
		}

		public static DialogResult Show(IWin32Window owner, string text)
		{
			return Show(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption)
		{
			return Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
		{
			return Show(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return Show(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}

		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			using (Form box = Create(text, caption, buttons, icon, defaultButton))
			{
				Func<Form, DialogResult> hook = ShowHook;
				if (hook != null)
				{
					return hook(box);
				}
				PlaySound(icon);
				IWin32Window parent = owner ?? Form.ActiveForm;
				box.StartPosition = parent != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
				return parent != null ? box.ShowDialog(parent) : box.ShowDialog();
			}
		}

		internal static Form Create(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return new BoxForm(text ?? string.Empty, caption ?? string.Empty, buttons, icon, defaultButton);
		}

		private static void PlaySound(MessageBoxIcon icon)
		{
			switch (icon)
			{
			case MessageBoxIcon.Hand:
				SystemSounds.Hand.Play();
				break;
			case MessageBoxIcon.Question:
				SystemSounds.Question.Play();
				break;
			case MessageBoxIcon.Exclamation:
				SystemSounds.Exclamation.Play();
				break;
			case MessageBoxIcon.Asterisk:
				SystemSounds.Asterisk.Play();
				break;
			default:
				SystemSounds.Beep.Play();
				break;
			}
		}

		private sealed class BoxForm : Form
		{
			private readonly string _text;

			private readonly string _caption;

			private readonly Button[] _buttons;

			private readonly bool _canCancel;

			private readonly Bitmap _image;

			private readonly TableLayoutPanel _layout;

			internal BoxForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			{
				_text = text;
				_caption = caption;
				SuspendLayout();
				AutoScaleMode = AutoScaleMode.Font;
				AutoScaleDimensions = new SizeF(7f, 15f);
				Text = caption;
				AccessibleRole = AccessibleRole.Dialog;
				FormBorderStyle = FormBorderStyle.FixedDialog;
				MinimizeBox = false;
				MaximizeBox = false;
				ShowIcon = false;
				ShowInTaskbar = false;
				KeyPreview = true;
				Padding = new Padding(0);

				TableLayoutPanel layout = _layout = new TableLayoutPanel
				{
					AutoSize = true,
					AutoSizeMode = AutoSizeMode.GrowAndShrink,
					ColumnCount = 1,
					RowCount = 2,
					Location = Point.Empty,
					Margin = new Padding(0),
					Padding = new Padding(0),
				};
				layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
				layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				TableLayoutPanel body = new TableLayoutPanel
				{
					AutoSize = true,
					AutoSizeMode = AutoSizeMode.GrowAndShrink,
					ColumnCount = 2,
					RowCount = 1,
					Padding = new Padding(16, 20, 24, 20),
					Margin = new Padding(0),
					Anchor = AnchorStyles.Left | AnchorStyles.Top,
				};
				body.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
				body.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
				Icon glyph = IconFor(icon);
				if (glyph != null)
				{
					_image = glyph.ToBitmap();
					PictureBox picture = new PictureBox
					{
						Name = "icon",
						Image = _image,
						SizeMode = PictureBoxSizeMode.Zoom,
						Size = new Size(32, 32),
						Margin = new Padding(0, 0, 12, 0),
						AccessibleRole = AccessibleRole.Graphic,
						AccessibleName = icon.ToString(),
					};
					body.Controls.Add(picture, 0, 0);
				}
				Label message = new Label
				{
					Name = "message",
					Text = text,
					AutoSize = true,
					UseMnemonic = false,
					MaximumSize = new Size(420, 0),
					MinimumSize = new Size(glyph != null ? 0 : 120, glyph != null ? 32 : 0),
					TextAlign = ContentAlignment.MiddleLeft,
					Margin = new Padding(0),
					AccessibleRole = AccessibleRole.StaticText,
					AccessibleName = text,
				};
				body.Controls.Add(message, 1, 0);

				FlowLayoutPanel row = new FlowLayoutPanel
				{
					Name = "buttons",
					AutoSize = true,
					AutoSizeMode = AutoSizeMode.GrowAndShrink,
					FlowDirection = FlowDirection.LeftToRight,
					WrapContents = false,
					Anchor = AnchorStyles.Right | AnchorStyles.Top,
					Padding = new Padding(12, 10, 12, 10),
					Margin = new Padding(0),
				};
				_buttons = CreateButtons(buttons);
				foreach (Button button in _buttons)
				{
					row.Controls.Add(button);
				}
				layout.Controls.Add(body, 0, 0);
				layout.Controls.Add(row, 0, 1);
				Controls.Add(layout);

				int index = defaultButton == MessageBoxDefaultButton.Button3 ? 2 : defaultButton == MessageBoxDefaultButton.Button2 ? 1 : 0;
				Button accept = _buttons[Math.Min(index, _buttons.Length - 1)];
				AcceptButton = accept;
				ActiveControl = accept;
				Button cancel = Array.Find(_buttons, b => b.DialogResult == DialogResult.Cancel);
				if (cancel == null && _buttons.Length == 1)
				{
					cancel = _buttons[0];
				}
				_canCancel = cancel != null;
				CancelButton = cancel;
				ControlBox = _canCancel;
				ResumeLayout(true);
				ClientSize = _layout.PreferredSize;
				ControlThemer.AttachDialog(this);
			}

			private static Icon IconFor(MessageBoxIcon icon)
			{
				switch (icon)
				{
				case MessageBoxIcon.Hand:
					return SystemIcons.Error;
				case MessageBoxIcon.Question:
					return SystemIcons.Question;
				case MessageBoxIcon.Exclamation:
					return SystemIcons.Warning;
				case MessageBoxIcon.Asterisk:
					return SystemIcons.Information;
				default:
					return null;
				}
			}

			private static Button[] CreateButtons(MessageBoxButtons buttons)
			{
				switch (buttons)
				{
				case MessageBoxButtons.OKCancel:
					return new[] { Make("OK", DialogResult.OK), Make("Cancel", DialogResult.Cancel) };
				case MessageBoxButtons.AbortRetryIgnore:
					return new[] { Make("&Abort", DialogResult.Abort), Make("&Retry", DialogResult.Retry), Make("&Ignore", DialogResult.Ignore) };
				case MessageBoxButtons.YesNoCancel:
					return new[] { Make("&Yes", DialogResult.Yes), Make("&No", DialogResult.No), Make("Cancel", DialogResult.Cancel) };
				case MessageBoxButtons.YesNo:
					return new[] { Make("&Yes", DialogResult.Yes), Make("&No", DialogResult.No) };
				case MessageBoxButtons.RetryCancel:
					return new[] { Make("&Retry", DialogResult.Retry), Make("Cancel", DialogResult.Cancel) };
				case MessageBoxButtons.CancelTryContinue:
					return new[] { Make("Cancel", DialogResult.Cancel), Make("&Try Again", DialogResult.TryAgain), Make("&Continue", DialogResult.Continue) };
				default:
					return new[] { Make("OK", DialogResult.OK) };
				}
			}

			private static Button Make(string text, DialogResult result)
			{
				return new Button
				{
					Text = text,
					DialogResult = result,
					AutoSize = true,
					MinimumSize = new Size(88, 26),
					Margin = new Padding(8, 0, 0, 0),
					UseVisualStyleBackColor = true,
				};
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				// Scaling and the themed frame both run before this, so the frame is measured as it is now.
				Size = _layout.PreferredSize + (Size - ClientSize);
			}

			protected override void OnFormClosing(FormClosingEventArgs e)
			{
				// Like the Windows message box, a box without a Cancel button closes only through one of its buttons.
				if (!_canCancel && DialogResult == DialogResult.None && e.CloseReason == CloseReason.UserClosing)
				{
					e.Cancel = true;
				}
				base.OnFormClosing(e);
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				if (e.Control && (e.KeyCode == Keys.C || e.KeyCode == Keys.Insert))
				{
					try
					{
						Clipboard.SetText(CopyText());
					}
					catch (System.Runtime.InteropServices.ExternalException)
					{
					}
					e.Handled = true;
				}
				base.OnKeyDown(e);
			}

			// The text that Ctrl+C copies, in the layout of the Windows message box.
			internal string CopyText()
			{
				const string line = "---------------------------";
				StringBuilder builder = new StringBuilder();
				builder.Append(line).Append("\r\n").Append(_caption).Append("\r\n").Append(line).Append("\r\n");
				builder.Append(_text).Append("\r\n").Append(line).Append("\r\n");
				foreach (Button button in _buttons)
				{
					builder.Append(button.Text.Replace("&", string.Empty)).Append("   ");
				}
				builder.Append("\r\n").Append(line).Append("\r\n");
				return builder.ToString();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_image?.Dispose();
				}
			}
		}
	}
}
