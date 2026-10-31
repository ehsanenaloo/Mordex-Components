using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridDropDown
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			DeserializeGridDropDownState(info, false);
		}

		private void DeserializeGridDropDownState(LayoutReader reader, bool condition)
		{
			SetContainer();
			InitializeDropDown();
			if (!condition)
			{
				ReadGridLayoutByMode(reader, (PersistenceMode)0);
			}
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), string.Empty);
			_displayMember = (string)LayoutReader.GetPropertyValue(reader, "DisplayMember", typeof(string), string.Empty);
			_valueMember = (string)LayoutReader.GetPropertyValue(reader, "ValueMember", typeof(string), string.Empty);
			_replaceValues = (bool)LayoutReader.GetPropertyValue(reader, "ReplaceValues", typeof(bool), true);
			_visibleRows = (int)LayoutReader.GetPropertyValue(reader, "VisibleRows", typeof(int), 8);
			base.HideSelection = HideSelection.Highlight;
			base.AllowEdit = InheritableBoolean.False;
			base.GroupByBoxVisible = false;
			base.ExpandableGroups = InheritableBoolean.False;
			base.Visible = false;
			if (base.RootTable == null)
			{
				base.RootTable = new GridTable();
			}
			int propertyValue = (int)LayoutReader.GetPropertyValue(reader, "ImageColumnIndex", typeof(int), -1);
			if (propertyValue >= 0 && propertyValue < Columns.Count)
			{
				_imageColumn = Columns[propertyValue];
			}
			else
			{
				_imageColumn = null;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			WriteLayoutByMode(info, PersistenceMode.All);
			SerializeGridDropDownState(info);
		}

		private void SerializeGridDropDownState(LayoutWriter mordexLayoutWriter)
		{
			if (_key != string.Empty)
			{
				mordexLayoutWriter.AddValue("Key", _key);
			}
			if (_displayMember != string.Empty)
			{
				mordexLayoutWriter.AddValue("DisplayMember", _displayMember);
			}
			if (_valueMember != string.Empty)
			{
				mordexLayoutWriter.AddValue("ValueMember", _valueMember);
			}
			if (!_replaceValues)
			{
				mordexLayoutWriter.AddValue("ReplaceValues", _replaceValues);
			}
			if (_visibleRows != 8)
			{
				mordexLayoutWriter.AddValue("VisibleRows", _visibleRows);
			}
			if (_imageColumn != null)
			{
				mordexLayoutWriter.AddValue("ImageColumnIndex", _imageColumn.Index);
			}
		}

		private bool IsNotInEditorDialog()
		{
			return !(GridHelper.GetParentSafe(this) is DropDownListForm);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
				{
					_container.Dispose();
				}
				if (GetHorizontalScrollBar() != null)
				{
					GetHorizontalScrollBar().MouseUp -= OnScrollBarMouseUp;
				}
				if (GetVScroll() != null)
				{
					GetVScroll().MouseUp -= OnScrollBarMouseUp;
				}
				if (_gridDropDownTimer != null)
				{
					_gridDropDownTimer.Tick -= OnAutoScrollTimerTick;
					_gridDropDownTimer.Dispose();
					_gridDropDownTimer = null;
				}
			}
			base.Dispose(disposing);
		}

		private void SetContainer()
		{
			_container = new Container();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_dropListOwner == null && IsNotInEditorDialog())
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, base.ClientRectangle);
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!base.UseCompatibleTextRendering);
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				textLayoutRenderer.DrawTextWithColor(e.Graphics, "GridDropDown control can not be used as a standalone control", base.ClientRectangle, Font, SystemColors.WindowText);
				textLayoutRenderer.ReleaseStringFormat();
			}
			else
			{
				base.OnPaint(e);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (IsDropDownOpenState())
			{
				return HandleNavigationKey(keyData);
			}
			return base.ProcessDialogKey(keyData);
		}

		internal bool HandleNavigationKey(Keys keys)
		{
			int value = 0;
			int otherRow = -1;
			switch (keys)
			{
			case Keys.Prior:
				otherRow = ((base.Row != -1) ? (base.Row - _visibleRows + 1) : 0);
				if (otherRow < 0)
				{
					otherRow = 0;
				}
				if (otherRow >= base.RowCount)
				{
					otherRow = base.RowCount - 1;
				}
				value = -1;
				break;
			case Keys.Next:
				if (base.Row == -1)
				{
					otherRow = 0;
				}
				else
				{
					otherRow = base.Row + _visibleRows - 1;
					if (otherRow >= base.RowCount)
					{
						otherRow = base.RowCount - 1;
					}
				}
				value = 1;
				break;
			case Keys.Down:
				MoveNext();
				Update();
				break;
			case Keys.Up:
				MovePrevious();
				Update();
				break;
			case Keys.Home:
				base.FirstRow = 0;
				otherRow = 0;
				if (base.RowCount == 0)
				{
					otherRow = -1;
				}
				value = -1;
				break;
			case Keys.End:
				otherRow = base.RowCount - 1;
				value = 1;
				break;
			case Keys.Return:
			case Keys.F4:
			case Keys.Up | Keys.Alt:
				if (IsDropDownOpenState())
				{
					HideDropDown(base.Row < 0);
				}
				break;
			case Keys.Escape:
				HideDropDown(true);
				break;
			default:
				return false;
			}
			_isSelectionSyncEnabled = false;
			if (otherRow != -1)
			{
				base.Row = otherRow;
				Update();
			}
			else
			{
				otherRow = base.Row;
			}
			GridRow row = GetRow();
			if (row != null && row.RowType != RowType.Record)
			{
				switch (value)
				{
				case -1:
					MovePrevious();
					if (GetRow().RowType != RowType.Record)
					{
						MoveFirst();
					}
					Update();
					break;
				case 1:
					MoveNext();
					if (GetRow().RowType != RowType.Record)
					{
						MoveLast();
					}
					Update();
					break;
				}
			}
			_isSelectionSyncEnabled = true;
			return true;
		}

		internal PropertyDescriptor ComputePropertyDescriptor(string name)
		{
			if (base.CurrencyManager == null)
			{
				return null;
			}
			if (name == "")
			{
				return null;
			}
			PropertyDescriptorCollection itemProperties = base.CurrencyManager.GetItemProperties();
			return itemProperties.Find(name, ignoreCase: true);
		}

		internal GridColumn FindColumnByKey(string key)
		{
			if (!string.IsNullOrEmpty(key) && Columns.Contains(key))
			{
				return Columns[key];
			}
			return null;
		}

		private void StartDeferredShowTimer()
		{
			_deferredShowTimer = new Timer();
			_deferredShowTimer.Interval = Timing.DeferredActionInterval;
			_deferredShowTimer.Tick += OnDeferredShowTimerTick;
			_deferredShowTimer.Enabled = true;
		}

		private void OnDeferredShowTimerTick(object sender, EventArgs eventArgs)
		{
			if (Visible)
			{
				GridHelper.SetCapture(this, true);
			}
			if (_deferredShowTimer != null)
			{
				_deferredShowTimer.Stop();
				_deferredShowTimer.Tick -= OnDeferredShowTimerTick;
				_deferredShowTimer.Dispose();
				_deferredShowTimer = null;
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WindowMessages.CancelMode)
			{
				HideDropDown(true);
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		internal void BuildLookupCache()
		{
			if (!_isLookupCacheEnabled || _hashtable != null || base.CurrencyManager == null)
			{
				return;
			}
			GridColumn gridColumn = FindColumnByKey(_valueMember);
			GridColumn columnByKey = FindColumnByKey(_displayMember);
			if (gridColumn != null && columnByKey != null)
			{
				long ticks = DateTime.Now.Ticks;
				MordexRow[] dataRows = GetRowSourceManager().RootRow.GetDataRows(RootTable, includeFilteredRows: true);
				_hashtable = new Hashtable(dataRows.Length);
				for (int i = 0; i < dataRows.Length; i++)
				{
					object rowValue = gridColumn.GetRowValue(dataRows[i]);
					if (rowValue == null)
					{
						rowValue = DBNull.Value;
					}
					if (!_hashtable.Contains(new CaseInsensitiveKey(rowValue)))
					{
						GridRow gridRow = GetGridRowForLogicalRow(dataRows[i]);
						if (gridRow != null)
						{
							_hashtable.Add(new CaseInsensitiveKey(rowValue), gridRow.Cells[columnByKey].Text);
						}
					}
				}
				RebuildRows(false);
				ticks = DateTime.Now.Ticks - ticks;
				return;
			}
			PropertyDescriptor propertyDescriptor = ComputePropertyDescriptor(_valueMember);
			PropertyDescriptor otherPropertyDescriptor = ComputePropertyDescriptor(_displayMember);
			if (propertyDescriptor == null || otherPropertyDescriptor == null)
			{
				return;
			}
			_hashtable = new Hashtable(base.CurrencyManager.Count);
			for (int j = 0; j < base.CurrencyManager.Count; j++)
			{
				object value = propertyDescriptor.GetValue(base.CurrencyManager.List[j]);
				if (value == null)
				{
					value = DBNull.Value;
				}
				if (!_hashtable.Contains(new CaseInsensitiveKey(value)))
				{
					_hashtable.Add(new CaseInsensitiveKey(value), GridHelper.ToSafeString(otherPropertyDescriptor.GetValue(base.CurrencyManager.List[j])));
				}
			}
		}

		internal void EnsureGraphicsReady()
		{
			if (!base.IsHandleCreated)
			{
				Graphics graphics = CreateGraphics();
				graphics.Dispose();
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (_isCellDropDown)
			{
				NativeMethods.SetProp(base.Handle, "PROP_ISCOMBODROPDOWN", new IntPtr(1));
			}
			else
			{
				NativeMethods.SetProp(base.Handle, "PROP_ISCOMBODROPDOWN", new IntPtr(1));
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (_isCellDropDown)
			{
				NativeMethods.RemoveProp(base.Handle, "PROP_ISCELLDROPDOWN");
			}
			else
			{
				NativeMethods.RemoveProp(base.Handle, "PROP_ISCELLDROPDOWN");
			}
			base.OnHandleDestroyed(e);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new DropDownAccessibleObject(this);
		}
	}
}
