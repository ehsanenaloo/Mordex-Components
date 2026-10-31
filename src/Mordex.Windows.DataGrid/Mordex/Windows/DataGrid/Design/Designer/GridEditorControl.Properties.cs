using Mordex.Windows.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;
using Microsoft.Win32;

namespace Mordex.Windows.DataGrid.Design.Designer
{
	internal partial class GridEditorControl
	{
		object IGridElement.SelectedObject
		{
			get
			{
				return _selectedCollection;
			}
			set
			{
				if (value != null && !(value is ICollection))
				{
					throw new ArgumentException();
				}
				if (_selectedCollection != value)
				{
					_selectedCollection = (ICollection)value;
					InitializeCollectionEditor();
				}
			}
		}

		Image IGridElement.ItemImage
		{
			get
			{
				return _itemImage;
			}
			set
			{
				_itemImage = value;
				int itemHeight = (int)_itemListBox.Font.GetHeight() + 2;
				if (_itemImage != null && _itemImage.Height + 2 > itemHeight)
				{
					itemHeight = _itemImage.Height + 2;
				}
				_itemListBox.ItemHeight = itemHeight;
				_itemListBox.Refresh();
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		event EventHandler IGridElement.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(_dataChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dataChangedEventKey, value);
			}
		}

		event EventHandler IGridElement.RefreshTree
		{
			add
			{
				base.Events.AddHandler(_gridEditorControlValue, value);
			}
			remove
			{
				base.Events.RemoveHandler(_gridEditorControlValue, value);
			}
		}
	}
}
