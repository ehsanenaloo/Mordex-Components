using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// Follows the form and the themed frame of a control that draws into a caption. Control.ParentChanged is raised only on
	/// the control whose parent changed, so a move of a panel that holds the control to another form is seen only by watching
	/// every parent up to the form. The owner calls <see cref="Update"/> from its own OnParentChanged.
	/// </summary>
	internal sealed class FormBindingWatch
	{
		private readonly Control _control;
		private readonly Action _formChanged;
		private readonly Action _frameChanged;
		private readonly EventHandler _moved;
		private readonly Action _slotVacated;
		private readonly List<Control> _chain = new List<Control>();
		private Form _form;
		private bool _on;

		/// <param name="control">The control that draws into the caption.</param>
		/// <param name="formChanged">Called after a parent of the control moved the control to another form, or out of every form.</param>
		/// <param name="frameChanged">Called after a frame took a style for the form of the control, or let it go.</param>
		/// <param name="slotVacated">Optional. Called after the caption slot of the frame of the form became free.</param>
		/// <param name="moved">Optional. Called when the control or a parent moves, changes size or visibility, or gets a new parent.</param>
		internal FormBindingWatch(Control control, Action formChanged, Action frameChanged, EventHandler moved = null, Action slotVacated = null)
		{
			_slotVacated = slotVacated;
			_control = control;
			_formChanged = formChanged;
			_frameChanged = frameChanged;
			_moved = moved;
		}

		/// <summary>The form that held the control when the watch last looked, or null.</summary>
		internal Form Form => _form;

		internal bool IsOn => _on;

		internal void Start()
		{
			if (!_on)
			{
				_on = true;
				MxThemedFormFrame.FrameApplied += OnFrameApplied;
				MxThemedFormFrame.CaptionSlotVacated += OnSlotVacated;
			}
			Rebuild();
		}

		internal void Stop()
		{
			Unhook();
			_form = null;
			if (_on)
			{
				_on = false;
				MxThemedFormFrame.FrameApplied -= OnFrameApplied;
				MxThemedFormFrame.CaptionSlotVacated -= OnSlotVacated;
			}
		}

		/// <summary>Watches the current parents of the control. Does nothing while the watch is off.</summary>
		internal void Update()
		{
			if (_on)
			{
				Rebuild();
			}
		}

		private void Rebuild()
		{
			Unhook();
			Control control = _control;
			while (control != null && !(control is Form))
			{
				if (_moved != null)
				{
					control.LocationChanged += _moved;
					control.SizeChanged += _moved;
					control.VisibleChanged += _moved;
				}
				// The control reports its own new parent through Update.
				if (!ReferenceEquals(control, _control))
				{
					control.ParentChanged += OnAncestorParentChanged;
				}
				_chain.Add(control);
				control = control.Parent;
			}
			_form = control as Form;
		}

		private void Unhook()
		{
			foreach (Control control in _chain)
			{
				if (_moved != null)
				{
					control.LocationChanged -= _moved;
					control.SizeChanged -= _moved;
					control.VisibleChanged -= _moved;
				}
				control.ParentChanged -= OnAncestorParentChanged;
			}
			_chain.Clear();
		}

		private void OnAncestorParentChanged(object sender, EventArgs e)
		{
			if (!_on)
			{
				return;
			}
			Form previous = _form;
			Rebuild();
			if (!ReferenceEquals(previous, _form))
			{
				_formChanged();
			}
			_moved?.Invoke(sender, e);
		}

		private void OnSlotVacated(object sender, EventArgs e)
		{
			if (_on && _slotVacated != null && sender != null && ReferenceEquals(sender, _form))
			{
				_slotVacated();
			}
		}

		private void OnFrameApplied(object sender, EventArgs e)
		{
			// Frames of other forms, perhaps on other threads, raise the same event.
			if (_on && sender != null && ReferenceEquals(sender, _form))
			{
				_frameChanged();
			}
		}
	}
}
