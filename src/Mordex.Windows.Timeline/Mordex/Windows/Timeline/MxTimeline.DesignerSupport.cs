using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		public void ResetComponentSettings()
		{
			if (!base.DesignMode)
			{
				GetApplicationSettings().Reset();
				LoadComponentSettings();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDesignTimeLayout()
		{
			if (Fields.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.Fields) == 0))
			{
				return true;
			}
			if (Groups.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.Groups) == 0))
			{
				return true;
			}
			if (GroupHeaderTotals.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.GroupHeaderTotals) == 0))
			{
				return true;
			}
			if (StoredFilters.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.StoredFilters) == 0))
			{
				return true;
			}
			if (FilterCondition != null && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.FilterCondition) == 0))
			{
				return true;
			}
			if (_internalImageList != null && _internalImageList.Images.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.InternalImageList) == 0))
			{
				return true;
			}
			if ((_workingHourSchema.WorkingHoursRange.Count > 0 || _workingHourSchema.Exceptions.Count > 0) && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.WorkingHourSchema) == 0))
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTooltipFont()
		{
			TooltipFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateRows();
				InvalidateLayout();
				if (DropDownCalendar != null)
				{
					DropDownCalendar.UpdateCalendarControlTextRendering();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTooltipFont()
		{
			return _tooltipFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeRowHeight()
		{
			return _timelineRowHeight != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetRowHeight()
		{
			RowHeight = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinDate()
		{
			return !_minDate.Equals(DateTime.MinValue.Date.AddMonths(1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinDate()
		{
			MinDate = DateTime.MinValue.Date.AddMonths(1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaxDate()
		{
			MaxDate = DateTime.MaxValue.Date.AddYears(-1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaxDate()
		{
			return !_maxDate.Equals(DateTime.MaxValue.Date.AddYears(-1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldFirstSerializeDate()
		{
			return _firstDate != DateTime.Today;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFirstDate()
		{
			FirstDate = DateTime.Today;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWorkStartTime()
		{
			return !_workStartTime.Equals(new TimeSpan(8, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWorkStartTime()
		{
			WorkStartTime = new TimeSpan(8, 0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWorkEndTime()
		{
			return !_workEndTime.Equals(new TimeSpan(17, 0, 0));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWorkEndTime()
		{
			WorkEndTime = new TimeSpan(17, 0, 0);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedFormatStyle()
		{
			return _selectedFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedFormatStyle()
		{
			if (_selectedFormatStyle.ShouldSerialize())
			{
				_selectedFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedInactiveFormatStyle()
		{
			return _selectedInactiveFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedInactiveFormatStyle()
		{
			if (_selectedInactiveFormatStyle.ShouldSerialize())
			{
				_selectedInactiveFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsFormatStyle()
		{
			return _itemsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsFormatStyle()
		{
			if (_itemsFormatStyle.ShouldSerialize())
			{
				_itemsFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLowerScaleTierFormatStyle()
		{
			return _lowerScaleTierFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLowerScaleTierFormatStyle()
		{
			if (_lowerScaleTierFormatStyle.ShouldSerialize())
			{
				_lowerScaleTierFormatStyle.Reset();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUpperScaleTiersFormatStyle()
		{
			return _upperScaleTiersFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUpperScaleTiersFormatStyle()
		{
			if (_upperScaleTiersFormatStyle.ShouldSerialize())
			{
				_upperScaleTiersFormatStyle.Reset();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsBarFormatStyle()
		{
			return _itemsBarFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsBarFormatStyle()
		{
			if (_itemsBarFormatStyle.ShouldSerialize())
			{
				_itemsBarFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeItemsEstimatedBarFormatStyle()
		{
			return _itemsEstimatedBarFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetItemsEstimatedBarFormatStyle()
		{
			if (_itemsEstimatedBarFormatStyle.ShouldSerialize())
			{
				_itemsEstimatedBarFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupRowsFormatStyle()
		{
			return _groupRowsFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupRowsFormatStyle()
		{
			if (ShouldSerializeGroupRowsFormatStyle())
			{
				_groupRowsFormatStyle.Reset();
				Invalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGridLineColor()
		{
			GridLineColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGridLineColor()
		{
			return !_gridLineColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWatermarkImage()
		{
			if (_watermarkImage == null)
			{
				return false;
			}
			return _watermarkImage.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWatermarkImage()
		{
			WatermarkImage.Reset();
		}
	}
}
