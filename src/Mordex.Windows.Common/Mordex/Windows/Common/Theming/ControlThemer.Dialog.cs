using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	public static partial class ControlThemer
	{
		private static readonly ConditionalWeakTable<Form, DialogFrame> DialogFrames = new ConditionalWeakTable<Form, DialogFrame>();

		/// <summary>
		/// Does what <see cref="Attach"/> does and also gives the form the themed caption and frame of
		/// <see cref="MxThemedFormFrame"/>. A form that cannot be resized keeps its client size when the frame changes, so a
		/// dialog laid out for the Windows caption still shows all of its controls. Safe to call more than once for a form.
		/// </summary>
		public static void AttachDialog(Form form)
		{
			if (form == null)
			{
				throw new ArgumentNullException(nameof(form));
			}
			Attach(form);
			if (!DialogFrames.TryGetValue(form, out _))
			{
				DialogFrames.Add(form, new DialogFrame(form));
			}
		}

		/// <summary>The frame that <see cref="AttachDialog"/> gave a form, or null.</summary>
		internal static MxThemedFormFrame DialogFrameOf(Form form)
		{
			return form != null && DialogFrames.TryGetValue(form, out DialogFrame dialog) ? dialog.Frame : null;
		}

		private sealed class DialogFrame
		{
			private readonly Form _form;

			// The size of the frame around the client area the last time it was measured, or empty before the first time.
			private Size _chrome;

			internal readonly MxThemedFormFrame Frame;

			internal DialogFrame(Form form)
			{
				_form = form;
				// The size of the Windows frame has to be read before the themed frame takes the window, so this handler is
				// added before the frame adds its own.
				form.HandleCreated += OnHandleCreated;
				form.Disposed += OnDisposed;
				MxThemedFormFrame.FrameApplied += OnFrameApplied;
				if (form.IsHandleCreated)
				{
					_chrome = Chrome();
				}
				Frame = new MxThemedFormFrame { Form = form };
			}

			private Size Chrome()
			{
				return _form.Size - _form.ClientSize;
			}

			private bool KeepsClientSize => _form.FormBorderStyle != FormBorderStyle.Sizable && _form.FormBorderStyle != FormBorderStyle.SizableToolWindow && _form.WindowState == FormWindowState.Normal;

			private void OnHandleCreated(object sender, EventArgs e)
			{
				_chrome = Chrome();
			}

			private void OnFrameApplied(object sender, EventArgs e)
			{
				if (!ReferenceEquals(sender, _form) || _form.IsDisposed || !_form.IsHandleCreated)
				{
					return;
				}
				Size chrome = Chrome();
				if (!_chrome.IsEmpty && chrome != _chrome && KeepsClientSize)
				{
					_form.Size += chrome - _chrome;
				}
				_chrome = Chrome();
			}

			private void OnDisposed(object sender, EventArgs e)
			{
				MxThemedFormFrame.FrameApplied -= OnFrameApplied;
				_form.HandleCreated -= OnHandleCreated;
				_form.Disposed -= OnDisposed;
				Frame.Dispose();
			}
		}
	}
}
