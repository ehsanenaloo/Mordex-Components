using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerAppointment
	{
		int ISchedulerFormatStyleOwner.DefaultAlpha => -1;

		AlphaMode ISchedulerFormatStyleOwner.BackColorAlphaMode => AlphaMode.Empty;

		float ISchedulerFormatStyleOwner.DefaultBlend => -1f;

		Color ISchedulerFormatStyleOwner.BlendColor => Color.Empty;

		AlphaMode ISchedulerFormatStyleOwner.ForeColorAlphaMode => AlphaMode.Empty;

		public SchedulerFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
				}
				return _formatStyle;
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (!(_text != value))
				{
					return;
				}
				if (Scheduler != null)
				{
					if (!Scheduler.ValidateAppointmentChange(AppointmentChange.Text))
					{
						return;
					}
					if (_recurrenceState == RecurrenceState.Occurrence)
					{
						string text = _text;
						_text = value;
						if (!EnsureRecurrenceException() || Scheduler == null)
						{
							return;
						}
						_text = text;
					}
					AppointmentChangeCancelEventArgs e = new AppointmentChangeCancelEventArgs(this, AppointmentChange.Text, value);
					_scheduler.RaiseAppointmentChanging(e);
					if (!e.Cancel)
					{
						value = (string)e.ProposedValue;
						_text = value;
						if (Scheduler.IsFieldBound(AppointmentChange.Text))
						{
							UpdateSchedulerAppointment();
						}
						else
						{
							InvalidateLayout();
						}
						if (_recurrenceState == RecurrenceState.Master)
						{
							_scheduler._isAppointmentCacheStale = true;
						}
						_scheduler.NotifyAppointmentChanged(this, AppointmentChange.Text);
					}
				}
				else
				{
					_text = value;
				}
			}
		}

		public Color BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				if (_borderColor != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_borderColor = value;
					RefreshAppearance(true);
				}
			}
		}

		public string ImageKey1
		{
			get
			{
				return _firstImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_firstImageKey != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_firstImageKey = value;
					_firstImageIndex = -1;
					RefreshAppearance(true);
				}
			}
		}

		public string ImageKey2
		{
			get
			{
				return _secondImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_secondImageKey != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_secondImageKey = value;
					_secondImageIndex = -1;
					RefreshAppearance(true);
				}
			}
		}

		public string ImageKey3
		{
			get
			{
				return _thirdImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_thirdImageKey != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_thirdImageKey = value;
					_thirdImageIndex = -1;
					RefreshAppearance(true);
				}
			}
		}

		public string ImageKey4
		{
			get
			{
				return _fourthImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_fourthImageKey != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_fourthImageKey = value;
					_fourthImageIndex = -1;
					RefreshAppearance(true);
				}
			}
		}

		public int ImageIndex1
		{
			get
			{
				return _firstImageIndex;
			}
			set
			{
				if (_firstImageIndex != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_firstImageIndex = value;
					_firstImageKey = "";
					RefreshAppearance(true);
				}
			}
		}

		public int ImageIndex2
		{
			get
			{
				return _secondImageIndex;
			}
			set
			{
				if (_secondImageIndex != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_secondImageIndex = value;
					_secondImageKey = "";
					RefreshAppearance(true);
				}
			}
		}

		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					_visible = value;
					if (_recurrenceState != RecurrenceState.Occurrence && Scheduler != null)
					{
						RefreshAppearance(true);
						Scheduler.InvalidateAppointmentCacheAndLayout();
						Scheduler.UpdateSchedule();
					}
				}
			}
		}

		public int ImageIndex3
		{
			get
			{
				return _thirdImageIndex;
			}
			set
			{
				if (_thirdImageIndex != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_thirdImageIndex = value;
					_thirdImageKey = "";
					RefreshAppearance(true);
				}
			}
		}

		public int ImageIndex4
		{
			get
			{
				return _fourthImageIndex;
			}
			set
			{
				if (_fourthImageIndex != value && (_recurrenceState != RecurrenceState.Occurrence || EnsureRecurrenceException()))
				{
					_fourthImageIndex = value;
					_fourthImageKey = "";
					RefreshAppearance(true);
				}
			}
		}
	}
}
