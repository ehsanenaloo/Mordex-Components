using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation
{
	/// <summary>
	/// Runs a short animation from a UI timer so the message loop keeps running between frames.
	/// The step callback gets the eased progress in the range 0..1 and is always called once with 1 at the end.
	/// </summary>
	internal sealed class StepAnimator : IDisposable
	{
		internal const int FrameIntervalMs = 15;

		private readonly Timer _timer;

		private readonly Stopwatch _clock = new Stopwatch();

		private readonly int _durationMs;

		private readonly Action<double> _step;

		private readonly Action _finished;

		private bool _completed;

		internal StepAnimator(int durationMs, Action<double> step, Action finished)
		{
			_durationMs = Math.Max(1, durationMs);
			_step = step;
			_finished = finished;
			_timer = new Timer { Interval = FrameIntervalMs };
			_timer.Tick += OnTick;
		}

		internal bool IsRunning => !_completed;

		internal static double EaseOut(double progress)
		{
			double remaining = 1.0 - progress;
			return 1.0 - remaining * remaining;
		}

		internal void Start()
		{
			_clock.Start();
			_timer.Start();
		}

		/// <summary>Jumps to the last frame and runs the finish callback. Does nothing when already completed.</summary>
		internal void Complete()
		{
			if (_completed)
			{
				return;
			}
			_completed = true;
			_timer.Stop();
			_clock.Stop();
			try
			{
				_step(1.0);
			}
			finally
			{
				_finished();
			}
		}

		/// <summary>Stops without drawing the last frame or running the finish callback.</summary>
		internal void Cancel()
		{
			_completed = true;
			_timer.Stop();
			_clock.Stop();
		}

		public void Dispose()
		{
			Cancel();
			_timer.Tick -= OnTick;
			_timer.Dispose();
		}

		private void OnTick(object sender, EventArgs e)
		{
			if (_completed)
			{
				return;
			}
			double progress = _clock.ElapsedMilliseconds / (double)_durationMs;
			if (progress >= 1.0)
			{
				Complete();
				return;
			}
			_step(EaseOut(progress));
		}
	}
}
