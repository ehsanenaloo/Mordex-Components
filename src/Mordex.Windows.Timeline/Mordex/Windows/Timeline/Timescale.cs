using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class Timescale : IMordexXmlLayoutsSupport
	{

		private MxTimeline _timeline;

		private Font _defaultFont;

		private TimelineInterval _defaultInterval = TimelineInterval.Month;

		private int _count = 1;

		private string _customFormat = "";

		private TimelineFormatStyle _formatStyle;

		private Font _customFont;

		private TimelineInterval _interval = TimelineInterval.Month;

		private bool _hasFocus;

		private Rectangle _bounds = Rectangle.Empty;

		private int _height;

		private bool _isLayoutDirty = true;

		private int _savedCount = 1;

		private TimelineInterval _timelineInterval;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Specifies the frequency of unit labels on the timescale.")]
		[DefaultValue(1)]
		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentException();
				}
				if (_count != value)
				{
					int previousCount = _count;
					_count = value;
					if (!AreTiersValid())
					{
						_count = previousCount;
						throw new ArgumentException("An upper tier has less time per tick than one of the tiers below it.");
					}
					InvalidateTimescale();
				}
			}
		}

		[DefaultValue("")]
		[Description("Indicates the custom date-time format string for this timescale")]
		public string CustomFormat
		{
			get
			{
				return _customFormat;
			}
			set
			{
				if (_customFormat != value)
				{
					_customFormat = value;
					InvalidateTimescale();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the TimelineFormatStyle used to draw this Timescale in the Timeline control.")]
		public TimelineFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new TimelineFormatStyle(_timeline, PredefinedStyle.None);
				}
				return _formatStyle;
			}
		}

		[Description("Represents the font of the text displayed by the timescale.")]
		public Font Font
		{
			get
			{
				if (_customFont != null)
				{
					return _customFont;
				}
				return _defaultFont;
			}
			set
			{
				if (_customFont != value)
				{
					_customFont = value;
					InvalidateTimescale();
				}
			}
		}

		[Localizable(true)]
		[Description("Specifies the time interval in which the timescale is divided.")]
		public TimelineInterval Interval
		{
			get
			{
				return _interval;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TimelineInterval), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_interval != value)
				{
					TimelineInterval timelineInterval = _interval;
					_interval = value;
					if (!AreTiersValid())
					{
						_interval = timelineInterval;
						throw new ArgumentException("An upper tier has less time per tick than one of the tiers below it.");
					}
					InvalidateTimescale();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_count != 1)
			{
				info.AddValue("Count", _count);
			}
			if (_customFormat.Length > 0)
			{
				info.AddValue("CustomFormat", _customFormat);
			}
			if (_customFont != null)
			{
				info.AddValue("Font", _customFont);
			}
			if (_interval != _defaultInterval)
			{
				info.AddValue("Interval", _interval);
			}
			if (_formatStyle != null && _formatStyle.ShouldSerialize())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_count = Math.Max(1, (int)LayoutReader.GetPropertyValue(info, "Count", typeof(int), 1));
			_customFormat = (string)LayoutReader.GetPropertyValue(info, "CustomFormat", typeof(string), "");
			object propertyValue = LayoutReader.GetPropertyValue(info, "Font", typeof(Font), null);
			if (propertyValue != null)
			{
				_customFont = (Font)propertyValue;
			}
			else
			{
				_customFont = null;
			}
			_interval = (TimelineInterval)LayoutReader.GetPropertyValue(info, "Interval", typeof(TimelineInterval), _defaultInterval);
			info.DeserializeObject("FormatStyle", FormatStyle);
			_isLayoutDirty = true;
		}

		internal bool HasNonDefaultSettings()
		{
			if (_count != 1)
			{
				return true;
			}
			if (_customFormat.Length > 0)
			{
				return true;
			}
			if (_customFont != null)
			{
				return true;
			}
			if (_interval != _defaultInterval)
			{
				return true;
			}
			if (_formatStyle != null && _formatStyle.ShouldSerialize())
			{
				return true;
			}
			return false;
		}

		internal Timescale(MxTimeline timeline, Font defaultFont, TimelineInterval defaultInterval)
		{
			_timeline = timeline;
			_defaultFont = defaultFont;
			_defaultInterval = defaultInterval;
			_interval = defaultInterval;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFont()
		{
			return _customFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFont()
		{
			Font = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeInterval()
		{
			return _interval != _defaultInterval;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetInterval()
		{
			Interval = _defaultInterval;
		}

		internal bool HasFocus()
		{
			return _hasFocus;
		}

		internal void SetHasFocus(bool hasFocus)
		{
			_hasFocus = hasFocus;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle rectangle)
		{
			_bounds = rectangle;
		}

		internal int GetHeight()
		{
			return _height;
		}

		internal void SetHeight(int height)
		{
			_height = height;
		}

		internal bool IsLayoutDirty()
		{
			return _isLayoutDirty;
		}

		internal void SetLayoutDirty(bool isLayoutDirty)
		{
			_isLayoutDirty = isLayoutDirty;
		}

		private void InvalidateTimescale()
		{
			_isLayoutDirty = true;
			if (_timeline != null)
			{
				_timeline.InvalidateTimescale();
			}
		}

		private bool AreTiersValid()
		{
			if (_timeline != null)
			{
				return _timeline.AreTimescaleTiersValid(_timeline.TimescaleTiers);
			}
			return true;
		}

		internal void UpdateTimescale()
		{
			_savedCount = _count;
			_timelineInterval = _interval;
		}

		internal void RestoreSavedSettings()
		{
			_count = _savedCount;
			_interval = _timelineInterval;
			_isLayoutDirty = true;
		}

		internal Font GetDefaultFont()
		{
			return _defaultFont;
		}
	}
}
