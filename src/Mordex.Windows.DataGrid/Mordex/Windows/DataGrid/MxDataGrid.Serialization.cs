using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class MxDataGrid
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			ReadGridLayoutByMode(info, (PersistenceMode)0);
		}

		internal void ReadGridLayoutByMode(LayoutReader reader, PersistenceMode otherPersistenceMode)
		{
			bool isInitializing = _isInitializing;
			_isInitializing = true;
			PersistenceMode persistenceMode = otherPersistenceMode;
			if (otherPersistenceMode == (PersistenceMode)0)
			{
				persistenceMode = PersistenceMode.All;
			}
			bool condition = true;
			bool otherCondition = true;
			bool nextCondition = true;
			bool previousCondition = true;
			bool firstCondition = true;
			bool secondCondition = true;
			if (persistenceMode != 0)
			{
				condition = (persistenceMode & PersistenceMode.TableStructure) != 0;
				otherCondition = (persistenceMode & PersistenceMode.FormatStyles) != 0;
				nextCondition = (persistenceMode & PersistenceMode.DropDowns) != 0;
				previousCondition = (persistenceMode & PersistenceMode.InternalImageList) != 0;
				firstCondition = (persistenceMode & PersistenceMode.AppearanceProperties) != 0;
				secondCondition = (persistenceMode & PersistenceMode.OtherProperties) != 0;
			}
			if (otherCondition)
			{
				_formatStyleCollection.Clear();
				reader.DeserializeObject("FormatStyles", _formatStyleCollection);
			}
			if (nextCondition)
			{
				foreach (GridDropDown item in _dropDowns)
				{
					item.Dispose();
				}
				_dropDowns.Clear();
				reader.DeserializeObject("DropDowns", _dropDowns);
			}
			if (previousCondition)
			{
				DeserializeGridState(reader);
			}
			if (firstCondition)
			{
				ReadAppearanceProperties(reader);
			}
			if (secondCondition)
			{
				ReadBehaviorProperties(reader);
			}
			if (condition)
			{
				PropertyValue propertyValue = reader["RootTable"];
				if (propertyValue != null)
				{
					GridTable gridTable = new GridTable();
					gridTable.UpdateDataGrid(this);
					propertyValue.DeserializeObject(gridTable);
					ReplaceRootTable(gridTable, true);
				}
				else
				{
					ReplaceRootTable(null, true);
				}
				if (RootTable != null)
				{
					int otherPropertyValue = (int)LayoutReader.GetPropertyValue(reader, "SearchColumnIndex", typeof(int), -1);
					if (otherPropertyValue >= 0 && otherPropertyValue < RootTable.Columns.Count)
					{
						_searchColumn = RootTable.Columns[otherPropertyValue];
					}
				}
			}
			if (!isInitializing)
			{
				((ISupportInitialize)this).EndInit();
			}
			else
			{
				_isInitializing = isInitializing;
			}
			if (_columnAutoResize)
			{
				MarkTablesForAutoResize();
			}
			ReadFilterRowCells(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			SerializeGridState(info);
			if (_layouts.Count > 0)
			{
				info.AddValue("Layouts", _layouts);
			}
		}

		internal void SerializeGridState(LayoutWriter mordexLayoutWriter)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.TableStructure) == 0)
				{
					WriteTableStructure(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.FormatStyles) == 0)
				{
					WriteFormatStyleCollection(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.DropDowns) == 0)
				{
					WriteDropDownCollection(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.InternalImageList) == 0)
				{
					WriteInternalImageList(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.AppearanceProperties) == 0)
				{
					WriteAppearanceProperties(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.OtherProperties) == 0)
				{
					WriteBehaviorProperties(mordexLayoutWriter);
				}
			}
			else
			{
				WriteTableStructure(mordexLayoutWriter);
				WriteFormatStyleCollection(mordexLayoutWriter);
				WriteDropDownCollection(mordexLayoutWriter);
				WriteInternalImageList(mordexLayoutWriter);
				WriteAppearanceProperties(mordexLayoutWriter);
				WriteBehaviorProperties(mordexLayoutWriter);
			}
			if (_filterMode != FilterMode.None)
			{
				WriteFilterRowCells(mordexLayoutWriter);
			}
		}

		internal void WriteLayoutByMode(LayoutWriter mordexLayoutWriter, PersistenceMode persistenceMode)
		{
			if ((persistenceMode & PersistenceMode.TableStructure) != 0)
			{
				WriteTableStructure(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.FormatStyles) != 0)
			{
				WriteFormatStyleCollection(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.DropDowns) != 0)
			{
				WriteDropDownCollection(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.InternalImageList) != 0)
			{
				WriteInternalImageList(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.AppearanceProperties) != 0)
			{
				WriteAppearanceProperties(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.OtherProperties) != 0)
			{
				WriteBehaviorProperties(mordexLayoutWriter);
			}
		}

		private void WriteTableStructure(LayoutWriter mordexLayoutWriter)
		{
			if (RootTable != null)
			{
				mordexLayoutWriter.AddValue("RootTable", RootTable);
			}
			if (_searchColumn != null)
			{
				mordexLayoutWriter.AddValue("SearchColumnIndex", _searchColumn.Index);
			}
		}

		private void WriteFormatStyleCollection(LayoutWriter mordexLayoutWriter)
		{
			if (_formatStyleCollection.Count > 0)
			{
				mordexLayoutWriter.AddValue("FormatStyles", _formatStyleCollection);
			}
		}

		private void WriteDropDownCollection(LayoutWriter mordexLayoutWriter)
		{
			if (_dropDowns.Count > 0)
			{
				mordexLayoutWriter.AddValue("DropDowns", _dropDowns);
			}
		}

		private void WriteInternalImageList(LayoutWriter mordexLayoutWriter)
		{
			if (_imageList == null || _imageList.Images.Count <= 0)
			{
				return;
			}
			mordexLayoutWriter.AddValue("ImageList.ColorDepth", _imageList.ColorDepth);
			mordexLayoutWriter.AddValue("ImageList.ImageSize", _imageList.ImageSize);
			mordexLayoutWriter.AddValue("ImageList.ImageStrip", ImageStrip.ToStrip(_imageList));
			mordexLayoutWriter.AddValue("ImageList.TransparentColor", _imageList.TransparentColor);
			for (int i = 0; i < _imageList.Images.Keys.Count; i++)
			{
				string text = _imageList.Images.Keys[i];
				if (text != null && text.Length > 0)
				{
					mordexLayoutWriter.AddValue("ImageList.ImageKey" + i, text);
				}
			}
		}

		private void DeserializeGridState(LayoutReader reader)
		{
			if (_imageList != null)
			{
				_imageList = null;
			}
			_imageList = new ImageList();
			_imageList.ColorDepth = (ColorDepth)LayoutReader.GetPropertyValue(reader, "ImageList.ColorDepth", typeof(ColorDepth), _imageList.ColorDepth);
			_imageList.ImageSize = (Size)LayoutReader.GetPropertyValue(reader, "ImageList.ImageSize", typeof(Size), _imageList.ImageSize);
			try
			{
				ImageStrip.FromStrip(_imageList, (Image)LayoutReader.GetPropertyValue(reader, "ImageList.ImageStrip", typeof(Image), null));
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			_imageList.TransparentColor = (Color)LayoutReader.GetPropertyValue(reader, "ImageList.TransparentColor", typeof(Color), _imageList.TransparentColor);
			for (int i = 0; i < _imageList.Images.Count; i++)
			{
				string name = (string)LayoutReader.GetPropertyValue(reader, "ImageList.ImageKey" + i, typeof(string), "");
				_imageList.Images.SetKeyName(i, name);
			}
		}

		private void WriteAppearanceProperties(LayoutWriter mordexLayoutWriter)
		{
			if (_emptyGridInfoAlignment != ContentAlignment.TopCenter)
			{
				mordexLayoutWriter.AddValue("EmptyGridInfoAlignment", _emptyGridInfoAlignment);
			}
			if (!_tableHeaderFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("TableHeaderFormatStyle", _tableHeaderFormatStyle);
			}
			if (!_rowWithErrorsFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("RowWithErrorsFormatStyle", _rowWithErrorsFormatStyle);
			}
			if (!_linkFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("LinkFormatStyle", _linkFormatStyle);
			}
			if (!_alternatingRowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("AlternatingRowFormatStyle", _alternatingRowFormatStyle);
			}
			if (!_cardCaptionFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("CardCaptionFormatStyle", _cardCaptionFormatStyle);
			}
			if (!_cardColumnHeaderFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("CardColumnHeaderFormatStyle", _cardColumnHeaderFormatStyle);
			}
			if (!_filterRowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("FilterRowFormatStyle", _filterRowFormatStyle);
			}
			if (!_groupByBoxFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("GroupByBoxFormatStyle", _groupByBoxFormatStyle);
			}
			if (!_groupByBoxInfoFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("GroupByBoxInfoFormatStyle", _groupByBoxInfoFormatStyle);
			}
			if (!_groupRowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("GroupRowFormatStyle", _groupRowFormatStyle);
			}
			if (!_groupTotalRowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("GroupTotalRowFormatStyle", _groupTotalRowFormatStyle);
			}
			if (!_headerFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("HeaderFormatStyle", _headerFormatStyle);
			}
			if (!_rowHeaderFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("RowHeaderFormatStyle", _rowHeaderFormatStyle);
			}
			if (!_previewRowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("PreviewRowFormatStyle", _previewRowFormatStyle);
			}
			if (!_rowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("RowFormatStyle", _rowFormatStyle);
			}
			if (!_selectedFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
			if (!_selectedInactiveFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("SelectedInactiveFormatStyle", _selectedInactiveFormatStyle);
			}
			if (!_totalRowFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("TotalRowFormatStyle", _totalRowFormatStyle);
			}
			if (!_focusCellFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("FocusCellFormatStyle", _focusCellFormatStyle);
			}
			mordexLayoutWriter.AddValue("WatermarkImage", _watermarkImage);
			if (_alternatingColors)
			{
				mordexLayoutWriter.AddValue("AlternatingColors", _alternatingColors);
			}
			if (BackgroundImage != null)
			{
				mordexLayoutWriter.AddValue("BackgroundImage", BackgroundImage);
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Center)
			{
				mordexLayoutWriter.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_defaultAlphaLevel != -1)
			{
				mordexLayoutWriter.AddValue("DefaultAlphaLevel", _defaultAlphaLevel);
			}
			if (_defaultAlphaMode != AlphaMode.Opaque)
			{
				mordexLayoutWriter.AddValue("DefaultAlphaMode", _defaultAlphaMode);
			}
			if (_defaultBackColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("DefaultBackColorAlphaMode", _defaultBackColorAlphaMode);
			}
			if (_defaultForeColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("DefaultForeColorAlphaMode", _defaultForeColorAlphaMode);
			}
			if (_defaultBlend != 0f)
			{
				mordexLayoutWriter.AddValue("DefaultBlend", _defaultBlend);
			}
			if (!_flatBorderInHeaders)
			{
				mordexLayoutWriter.AddValue("FlatBordersInHeaders", _flatBorderInHeaders);
			}
			if (_gridLineColor != SystemColors.ControlDark)
			{
				mordexLayoutWriter.AddValue("GridLineColor", _gridLineColor);
			}
			if (GridLineStyle != GridLineStyle.SmallDots)
			{
				mordexLayoutWriter.AddValue("GridLineStyle", _gridLineStyle);
			}
			if (_treeLineColor != SystemColors.ControlDark)
			{
				mordexLayoutWriter.AddValue("TreeLineColor", _treeLineColor);
			}
			if (_blendColor != SystemColors.Window)
			{
				mordexLayoutWriter.AddValue("BlendColor", _blendColor);
			}
			if (base.BorderStyle != BorderStyle.SunkenLight3D)
			{
				mordexLayoutWriter.AddValue("BorderStyle", base.BorderStyle);
			}
			if (ShouldSerializeFlatBorderColor())
			{
				mordexLayoutWriter.AddValue("FlatBorderColor", base.FlatBorderColor);
			}
			if (!base.BackColor.Equals(SystemColors.Window))
			{
				mordexLayoutWriter.AddValue("BackColor", base.BackColor);
			}
			if (_groupExpandBoxStyle != ExpandBoxStyle.UseDefault)
			{
				mordexLayoutWriter.AddValue("GroupExpandBoxStyle", _groupExpandBoxStyle);
			}
			if (_hierarchicalExpandBoxStyle != ExpandBoxStyle.UseDefault)
			{
				mordexLayoutWriter.AddValue("HierarchicalExpandBoxStyle", _hierarchicalExpandBoxStyle);
			}
			if (_treeCollapseGlyph != null)
			{
				mordexLayoutWriter.AddValue("TreeCollapseGlyph", _treeCollapseGlyph);
			}
			if (_treeExpandGlyph != null)
			{
				mordexLayoutWriter.AddValue("TreeExpandGlyph", _treeExpandGlyph);
			}
			if (GridHelper.GetParentSafe(this) != null && Font != GridHelper.GetParentSafe(this).Font)
			{
				mordexLayoutWriter.AddValue("Font", Font);
			}
			if (ShouldSerializeScrollBarWidth())
			{
				mordexLayoutWriter.AddValue("ScrollBarWidth", base.ScrollBarWidth);
			}
			if (base.ControlStyle.HasNonDefaultSettings())
			{
				mordexLayoutWriter.AddValue("ControlStyle", _gridControlStyle);
			}
			if (_hasEditorsControlStyle)
			{
				mordexLayoutWriter.AddValue("HasEditorsControlStyle", _hasEditorsControlStyle);
				if (_editorsControlStyle.HasNonDefaultSettings())
				{
					mordexLayoutWriter.AddValue("EditorsControlStyle", _editorsControlStyle);
				}
			}
		}

		private static void ReadFormatStyle(LayoutReader mordexLayoutReader, string propertyName, GridFormatStyle targetInstance)
		{
			targetInstance.Reset();
			mordexLayoutReader.DeserializeObject(propertyName, targetInstance);
		}

		private void ReadAppearanceProperties(LayoutReader reader)
		{
			ReadFormatStyle(reader, "TableHeaderFormatStyle", _tableHeaderFormatStyle);
			ReadFormatStyle(reader, "RowWithErrorsFormatStyle", _rowWithErrorsFormatStyle);
			ReadFormatStyle(reader, "LinkFormatStyle", _linkFormatStyle);
			ReadFormatStyle(reader, "AlternatingRowFormatStyle", _alternatingRowFormatStyle);
			ReadFormatStyle(reader, "CardCaptionFormatStyle", _cardCaptionFormatStyle);
			ReadFormatStyle(reader, "CardColumnHeaderFormatStyle", _cardColumnHeaderFormatStyle);
			ReadFormatStyle(reader, "FilterRowFormatStyle", _filterRowFormatStyle);
			ReadFormatStyle(reader, "GroupByBoxFormatStyle", _groupByBoxFormatStyle);
			ReadFormatStyle(reader, "GroupByBoxInfoFormatStyle", _groupByBoxInfoFormatStyle);
			ReadFormatStyle(reader, "GroupRowFormatStyle", _groupRowFormatStyle);
			ReadFormatStyle(reader, "GroupTotalRowFormatStyle", _groupTotalRowFormatStyle);
			ReadFormatStyle(reader, "HeaderFormatStyle", _headerFormatStyle);
			ReadFormatStyle(reader, "RowHeaderFormatStyle", _rowHeaderFormatStyle);
			ReadFormatStyle(reader, "PreviewRowFormatStyle", _previewRowFormatStyle);
			ReadFormatStyle(reader, "RowFormatStyle", _rowFormatStyle);
			ReadFormatStyle(reader, "SelectedFormatStyle", _selectedFormatStyle);
			ReadFormatStyle(reader, "SelectedInactiveFormatStyle", _selectedInactiveFormatStyle);
			ReadFormatStyle(reader, "TotalRowFormatStyle", _totalRowFormatStyle);
			ReadFormatStyle(reader, "FocusCellFormatStyle", _focusCellFormatStyle);
			_emptyGridInfoAlignment = (ContentAlignment)LayoutReader.GetPropertyValue(reader, "EmptyGridInfoAlignment", typeof(ContentAlignment), ContentAlignment.TopCenter);
			UpdateWatermarkImage();
			reader.DeserializeObject("WatermarkImage", _watermarkImage);
			_alternatingColors = (bool)LayoutReader.GetPropertyValue(reader, "AlternatingColors", typeof(bool), false);
			BackgroundImage = (Image)LayoutReader.GetPropertyValue(reader, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (BackgroundImageDrawMode)LayoutReader.GetPropertyValue(reader, "BackgroundImageDrawMode", typeof(BackgroundImageDrawMode), BackgroundImageDrawMode.Center);
			_defaultAlphaLevel = (int)LayoutReader.GetPropertyValue(reader, "DefaultAlphaLevel", typeof(int), -1);
			_defaultAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultAlphaMode", typeof(AlphaMode), AlphaMode.Opaque);
			_defaultBackColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultBackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_defaultForeColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_defaultBlend = (float)LayoutReader.GetPropertyValue(reader, "DefaultBlend", typeof(float), 0f);
			_flatBorderInHeaders = (bool)LayoutReader.GetPropertyValue(reader, "FlatBordersInHeaders", typeof(bool), true);
			_gridLineColor = (Color)LayoutReader.GetPropertyValue(reader, "GridLineColor", typeof(Color), SystemColors.ControlDark);
			_gridLineStyle = (GridLineStyle)LayoutReader.GetPropertyValue(reader, "GridLineStyle", typeof(GridLineStyle), GridLineStyle.SmallDots);
			_treeLineColor = (Color)LayoutReader.GetPropertyValue(reader, "TreeLineColor", typeof(Color), SystemColors.ControlDark);
			_blendColor = (Color)LayoutReader.GetPropertyValue(reader, "BlendColor", typeof(Color), SystemColors.Window);
			base.BorderStyle = (BorderStyle)LayoutReader.GetPropertyValue(reader, "BorderStyle", typeof(BorderStyle), BorderStyle.SunkenLight3D);
			base.FlatBorderColor = (Color)LayoutReader.GetPropertyValue(reader, "FlatBorderColor", typeof(Color), SystemColors.WindowFrame);
			base.BackColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), SystemColors.Window);
			_groupExpandBoxStyle = (ExpandBoxStyle)LayoutReader.GetPropertyValue(reader, "GroupExpandBoxStyle", typeof(ExpandBoxStyle), ExpandBoxStyle.UseDefault);
			_hierarchicalExpandBoxStyle = (ExpandBoxStyle)LayoutReader.GetPropertyValue(reader, "HierarchicalExpandBoxStyle", typeof(ExpandBoxStyle), ExpandBoxStyle.UseDefault);
			_treeCollapseGlyph = (Image)LayoutReader.GetPropertyValue(reader, "TreeCollapseGlyph", typeof(Image), null);
			_treeExpandGlyph = (Image)LayoutReader.GetPropertyValue(reader, "TreeExpandGlyph", typeof(Image), null);
			int otherPropertyValue = (int)LayoutReader.GetPropertyValue(reader, "ScrollBarWidth", typeof(int), -1);
			if (otherPropertyValue != -1)
			{
				base.ScrollBarWidth = otherPropertyValue;
			}
			Font defaultValue = null;
			Font = (Font)LayoutReader.GetPropertyValue(reader, "Font", typeof(Font), defaultValue);
			_gridControlStyle.Reset();
			reader.DeserializeObject("ControlStyle", _gridControlStyle);
			_hasEditorsControlStyle = (bool)LayoutReader.GetPropertyValue(reader, "HasEditorsControlStyle", typeof(bool), false);
			_editorsControlStyle.Reset();
			reader.DeserializeObject("EditorsControlStyle", _editorsControlStyle);
		}

		private void ReadFilterRowCells(LayoutReader reader)
		{
			if (RootTable == null || _gridRow == null)
			{
				return;
			}
			FilterRowUpdateMode filterRowUpdateMode = FilterRowUpdateMode;
			FilterRowUpdateMode = FilterRowUpdateMode.Default;
			for (int i = 0; i < _gridRow.Cells.Count; i++)
			{
				object propertyValue = LayoutReader.GetPropertyValue(reader, "FilterCell" + i, typeof(string), null);
				if (propertyValue != null)
				{
					if (_gridRow.Cells[i].Column.Type != null && _gridRow.Cells[i].Column.CompareTarget == ColumnCompareTarget.Value)
					{
						propertyValue = PropertyValue.ConvertValue((string)propertyValue, _gridRow.Cells[i].Column.Type);
					}
					_gridRow.Cells[i].AssignValue(propertyValue, true);
					_showRowPreview = false;
				}
			}
			FilterRowUpdateMode = filterRowUpdateMode;
		}

		private void WriteFilterRowCells(LayoutWriter mordexLayoutWriter)
		{
			if (RootTable == null || _gridRow == null)
			{
				return;
			}
			foreach (GridCell item in (IEnumerable)_gridRow.Cells)
			{
				if (item.Value != null && item.Text.Length > 0)
				{
					mordexLayoutWriter.AddValue("FilterCell" + item.Column.Index, item.Value);
				}
			}
		}

		private void WriteBehaviorProperties(LayoutWriter mordexLayoutWriter)
		{
			if (BoundMode != BoundMode.Bound)
			{
				mordexLayoutWriter.AddValue("BoundMode", BoundMode);
			}
			if (_invalidValueAction != InvalidValueAction.DiscardChanges)
			{
				mordexLayoutWriter.AddValue("InvalidValueAction", _invalidValueAction);
			}
			if (!_showErrors)
			{
				mordexLayoutWriter.AddValue("ShowErrors", _showErrors);
			}
			if (_updateMode != UpdateMode.RowUpdate)
			{
				mordexLayoutWriter.AddValue("UpdateMode", _updateMode);
			}
			if (_incrementalSearchMode != IncrementalSearchMode.None)
			{
				mordexLayoutWriter.AddValue("IncrementalSearchMode", _incrementalSearchMode);
			}
			if (AllowAddNew != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("AllowAddNew", AllowAddNew);
			}
			if (!AllowCardSizing)
			{
				mordexLayoutWriter.AddValue("AllowCardSizing", _allowCardSizing);
			}
			if (!_allowColumnDrag)
			{
				mordexLayoutWriter.AddValue("AllowColumnDrag", _allowColumnDrag);
			}
			if (_allowDelete != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("AllowDelete", _allowDelete);
			}
			if (_allowEdit != InheritableBoolean.True)
			{
				mordexLayoutWriter.AddValue("AllowEdit", _allowEdit);
			}
			if (_allowRemoveColumns != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("AllowRemoveColumns", _allowRemoveColumns);
			}
			if (_autoEdit)
			{
				mordexLayoutWriter.AddValue("AutoEdit", _autoEdit);
			}
			if (!_automaticSort)
			{
				mordexLayoutWriter.AddValue("AutomaticSort", _automaticSort);
			}
			if (_cardCaptionPrefix != string.Empty)
			{
				mordexLayoutWriter.AddValue("CardCaptionPrefix", _cardCaptionPrefix);
			}
			if (!_cardBorders)
			{
				mordexLayoutWriter.AddValue("CardBorders", _cardBorders);
			}
			if (!_cardHeaders)
			{
				mordexLayoutWriter.AddValue("CardHeaders", _cardHeaders);
			}
			if (_cardSpacing != 12)
			{
				mordexLayoutWriter.AddValue("CardSpacing", _cardSpacing);
			}
			if (_cardInnerSpacing != 0)
			{
				mordexLayoutWriter.AddValue("CardInnerSpacing", _cardInnerSpacing);
			}
			if (_cardViewGridlines != CardViewGridlines.None)
			{
				mordexLayoutWriter.AddValue("CardViewGridlines", _cardViewGridlines);
			}
			if (_cardWidth != 250)
			{
				mordexLayoutWriter.AddValue("CardWidth", _cardWidth);
			}
			if (_cellToolTip != CellToolTip.UseCellToolTipText)
			{
				mordexLayoutWriter.AddValue("CellToolTip", _cellToolTip);
			}
			if (_cellToolTipText != "")
			{
				mordexLayoutWriter.AddValue("CellToolTipText", _cellToolTipText);
			}
			if (_columnAutoResize)
			{
				mordexLayoutWriter.AddValue("ColumnAutoResize", _columnAutoResize);
			}
			if (_columnHeaders != InheritableBoolean.True)
			{
				mordexLayoutWriter.AddValue("ColumnHeaders", _columnHeaders);
			}
			if (_columnSetHeaders != InheritableBoolean.True)
			{
				mordexLayoutWriter.AddValue("ColumnSetHeaders", _columnSetHeaders);
			}
			if (!_continuousScroll)
			{
				mordexLayoutWriter.AddValue("ContinuousScroll", _continuousScroll);
			}
			if (_emptyRows)
			{
				mordexLayoutWriter.AddValue("EmptyRows", _emptyRows);
			}
			if (!_eventErrorHandling)
			{
				mordexLayoutWriter.AddValue("EventErrorHandling", _eventErrorHandling);
			}
			if (!_expandableCards)
			{
				mordexLayoutWriter.AddValue("ExpandableCards", _expandableCards);
			}
			if (_expandableGroups != InheritableBoolean.True)
			{
				mordexLayoutWriter.AddValue("ExpandableGroups", _expandableGroups);
			}
			if (_filterMode != FilterMode.None)
			{
				mordexLayoutWriter.AddValue("FilterMode", _filterMode);
			}
			if (_focusStyle != FocusStyle.Dotted)
			{
				mordexLayoutWriter.AddValue("FocusStyle", _focusStyle);
			}
			if (_frozenColumns != 0)
			{
				mordexLayoutWriter.AddValue("FrozenColumns", _frozenColumns);
			}
			if (_gridLines != GridLines.Both)
			{
				mordexLayoutWriter.AddValue("GridLines", _gridLines);
			}
			if (!_groupByBoxVisible)
			{
				mordexLayoutWriter.AddValue("GroupByBoxVisible", _groupByBoxVisible);
			}
			if (_groupIndent != 16)
			{
				mordexLayoutWriter.AddValue("GroupIndent", _groupIndent);
			}
			if (GroupMode != GroupMode.Expanded)
			{
				mordexLayoutWriter.AddValue("GroupMode", GroupMode);
			}
			if (GroupTotals != GroupTotals.Never)
			{
				mordexLayoutWriter.AddValue("GroupTotals", GroupTotals);
			}
			if (_hideColumnsWhenGrouped != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("HideColumnsWhenGrouped", _hideColumnsWhenGrouped);
			}
			if (_hideSelection != HideSelection.HideSelection)
			{
				mordexLayoutWriter.AddValue("HideSelection", _hideSelection);
			}
			if (_hierarchical)
			{
				mordexLayoutWriter.AddValue("Hierarchical", _hierarchical);
			}
			if (_indent != 18)
			{
				mordexLayoutWriter.AddValue("Indent", _indent);
			}
			if (NewRowPosition != NewRowPosition.TopRow)
			{
				mordexLayoutWriter.AddValue("NewRowPosition", NewRowPosition);
			}
			if (_repeatHeaders != InheritableBoolean.True)
			{
				mordexLayoutWriter.AddValue("RepeatHeaders", _repeatHeaders);
			}
			if (_rowHeaders != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("RowHeaders", _rowHeaders);
			}
			if (_selectionMode != SelectionMode.SingleSelection)
			{
				mordexLayoutWriter.AddValue("SelectionMode", _selectionMode);
			}
			if (!_showEmptyFields)
			{
				mordexLayoutWriter.AddValue("ShowEmptyFields", _showEmptyFields);
			}
			if (_tableSpacing != 4)
			{
				mordexLayoutWriter.AddValue("TableSpacing", _tableSpacing);
			}
			if (TotalRow != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("TotalRow", TotalRow);
			}
			if (!_treeLines)
			{
				mordexLayoutWriter.AddValue("TreeLines", _treeLines);
			}
			if (_view != View.TableView)
			{
				mordexLayoutWriter.AddValue("View", _view);
			}
			if (base.RecordNavigator)
			{
				mordexLayoutWriter.AddValue("RecordNavigator", base.RecordNavigator);
			}
			if (base.ScrollBars != ScrollBars.Automatic)
			{
				mordexLayoutWriter.AddValue("ScrollBars", base.ScrollBars);
			}
			if (_tabKeyBehavior != TabKeyBehavior.ColumnNavigation)
			{
				mordexLayoutWriter.AddValue("TabKeyBehavior", _tabKeyBehavior);
			}
			if (_enterKeyBehavior != EnterKeyBehavior.NextRow)
			{
				mordexLayoutWriter.AddValue("EnterKeyBehavior", _enterKeyBehavior);
			}
			if (_newRowEnterKeyBehavior != NewRowEnterKeyBehavior.Default)
			{
				mordexLayoutWriter.AddValue("NewRowEnterKeyBehavior", _newRowEnterKeyBehavior);
			}
			if (!_acceptsEscape)
			{
				mordexLayoutWriter.AddValue("AcceptsEscape", _acceptsEscape);
			}
			if (_tableHeaders != InheritableBoolean.False)
			{
				mordexLayoutWriter.AddValue("TableHeaders", _tableHeaders);
			}
			if (!_updateOnLeave)
			{
				mordexLayoutWriter.AddValue("UpdateOnLeave", _updateOnLeave);
			}
			if (_delayEditionOnClick)
			{
				mordexLayoutWriter.AddValue("DelayEditionOnClick", _delayEditionOnClick);
			}
			if (_cellSelectionMode != CellSelectionMode.EntireRow)
			{
				mordexLayoutWriter.AddValue("CellSelectionMode", _cellSelectionMode);
			}
			if (_builtInTexts.HasCustomTexts())
			{
				mordexLayoutWriter.AddValue("BuiltInTexts", _builtInTexts);
			}
			if (AllowChildTableGroups)
			{
				mordexLayoutWriter.AddValue("AllowChildTableGroups", AllowChildTableGroups);
			}
			if (DynamicFiltering)
			{
				mordexLayoutWriter.AddValue("DynamicFiltering", DynamicFiltering);
			}
			if (_totalRowPosition != TotalRowPosition.BottomScrollable)
			{
				mordexLayoutWriter.AddValue("TotalRowPosition", _totalRowPosition);
			}
			if (_rowHeaderContent != RowHeaderContent.RowHeaderText)
			{
				mordexLayoutWriter.AddValue("RowHeaderContent", _rowHeaderContent);
			}
			if (RowCheckStateBehavior != RowCheckStateBehavior.CheckIndividualRows)
			{
				mordexLayoutWriter.AddValue("RowCheckStateBehavior", RowCheckStateBehavior);
			}
			if (_useGroupRowSelector)
			{
				mordexLayoutWriter.AddValue("UseGroupRowSelector", _useGroupRowSelector);
			}
			if (!_selectOnExpand)
			{
				mordexLayoutWriter.AddValue("SelectOnExpand", _selectOnExpand);
			}
			if (_columnAutoSizeMode != ColumnAutoSizeMode.AllCellsAndHeader)
			{
				mordexLayoutWriter.AddValue("ColumnAutoSizeMode", _columnAutoSizeMode);
			}
			if (_filterRowButtonStyle != FilterRowButtonStyle.ClearButton)
			{
				mordexLayoutWriter.AddValue("FilterRowButtonStyle", _filterRowButtonStyle);
			}
			if (_defaultFilterRowComparison != FilterConditionOperator.Default)
			{
				mordexLayoutWriter.AddValue("DefaultFilterRowComparison", _defaultFilterRowComparison);
			}
			if (_filterRowUpdateMode != FilterRowUpdateMode.Default)
			{
				mordexLayoutWriter.AddValue("FilterRowUpdateMode", _filterRowUpdateMode);
			}
		}

		private void ReadBehaviorProperties(LayoutReader reader)
		{
			BoundMode = (BoundMode)LayoutReader.GetPropertyValue(reader, "BoundMode", typeof(BoundMode), BoundMode.Bound);
			_useGroupRowSelector = (bool)LayoutReader.GetPropertyValue(reader, "UseGroupRowSelector", typeof(bool), false);
			RowCheckStateBehavior = (RowCheckStateBehavior)LayoutReader.GetPropertyValue(reader, "RowCheckStateBehavior", typeof(RowCheckStateBehavior), RowCheckStateBehavior.CheckIndividualRows);
			_rowHeaderContent = (RowHeaderContent)LayoutReader.GetPropertyValue(reader, "RowHeaderContent", typeof(RowHeaderContent), RowHeaderContent.RowHeaderText);
			DynamicFiltering = (bool)LayoutReader.GetPropertyValue(reader, "DynamicFiltering", typeof(bool), false);
			_invalidValueAction = (InvalidValueAction)LayoutReader.GetPropertyValue(reader, "InvalidValueAction", typeof(InvalidValueAction), InvalidValueAction.DiscardChanges);
			_showErrors = (bool)LayoutReader.GetPropertyValue(reader, "ShowErrors", typeof(bool), true);
			_updateMode = (UpdateMode)LayoutReader.GetPropertyValue(reader, "UpdateMode", typeof(UpdateMode), UpdateMode.RowUpdate);
			_incrementalSearchMode = (IncrementalSearchMode)LayoutReader.GetPropertyValue(reader, "IncrementalSearchMode", typeof(IncrementalSearchMode), IncrementalSearchMode.None);
			AllowAddNew = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowAddNew", typeof(InheritableBoolean), InheritableBoolean.False);
			AllowCardSizing = (bool)LayoutReader.GetPropertyValue(reader, "AllowCardSizing", typeof(bool), true);
			_allowColumnDrag = (bool)LayoutReader.GetPropertyValue(reader, "AllowColumnDrag", typeof(bool), true);
			_allowDelete = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowDelete", typeof(InheritableBoolean), InheritableBoolean.False);
			_allowEdit = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowEdit", typeof(InheritableBoolean), InheritableBoolean.True);
			_allowRemoveColumns = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowRemoveColumns", typeof(InheritableBoolean), InheritableBoolean.False);
			_autoEdit = (bool)LayoutReader.GetPropertyValue(reader, "AutoEdit", typeof(bool), false);
			_automaticSort = (bool)LayoutReader.GetPropertyValue(reader, "AutomaticSort", typeof(bool), true);
			_cardCaptionPrefix = (string)LayoutReader.GetPropertyValue(reader, "CardCaptionPrefix", typeof(string), string.Empty);
			_cardBorders = (bool)LayoutReader.GetPropertyValue(reader, "CardBorders", typeof(bool), true);
			_cardHeaders = (bool)LayoutReader.GetPropertyValue(reader, "CardHeaders", typeof(bool), true);
			_cardSpacing = (int)LayoutReader.GetPropertyValue(reader, "CardSpacing", typeof(int), 12);
			_cardInnerSpacing = (int)LayoutReader.GetPropertyValue(reader, "CardInnerSpacing", typeof(int), 0);
			_cardWidth = (int)LayoutReader.GetPropertyValue(reader, "CardWidth", typeof(int), 250);
			_cardViewGridlines = (CardViewGridlines)LayoutReader.GetPropertyValue(reader, "CardViewGridlines", typeof(CardViewGridlines), CardViewGridlines.None);
			_cellToolTip = (CellToolTip)LayoutReader.GetPropertyValue(reader, "CellToolTip", typeof(CellToolTip), CellToolTip.UseCellToolTipText);
			_cellToolTipText = (string)LayoutReader.GetPropertyValue(reader, "CellToolTipText", typeof(string), "");
			_columnAutoResize = (bool)LayoutReader.GetPropertyValue(reader, "ColumnAutoResize", typeof(bool), false);
			_columnHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ColumnHeaders", typeof(InheritableBoolean), InheritableBoolean.True);
			_columnSetHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ColumnSetHeaders", typeof(InheritableBoolean), InheritableBoolean.True);
			_continuousScroll = (bool)LayoutReader.GetPropertyValue(reader, "ContinuousScroll", typeof(bool), true);
			_emptyRows = (bool)LayoutReader.GetPropertyValue(reader, "EmptyRows", typeof(bool), false);
			_eventErrorHandling = (bool)LayoutReader.GetPropertyValue(reader, "EventErrorHandling", typeof(bool), true);
			_expandableCards = (bool)LayoutReader.GetPropertyValue(reader, "ExpandableCards", typeof(bool), true);
			_expandableGroups = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ExpandableGroups", typeof(InheritableBoolean), InheritableBoolean.True);
			_filterMode = (FilterMode)LayoutReader.GetPropertyValue(reader, "FilterMode", typeof(FilterMode), FilterMode.None);
			SetFilterRowInfoText((string)LayoutReader.GetPropertyValue(reader, "FilterRowInfoText", typeof(string), ""));
			_focusStyle = (FocusStyle)LayoutReader.GetPropertyValue(reader, "FocusStyle", typeof(FocusStyle), FocusStyle.Dotted);
			_frozenColumns = (int)LayoutReader.GetPropertyValue(reader, "FrozenColumns", typeof(int), 0);
			_gridLines = (GridLines)LayoutReader.GetPropertyValue(reader, "GridLines", typeof(GridLines), GridLines.Both);
			_groupByBoxVisible = (bool)LayoutReader.GetPropertyValue(reader, "GroupByBoxVisible", typeof(bool), true);
			_groupIndent = (int)LayoutReader.GetPropertyValue(reader, "GroupIndent", typeof(int), 16);
			GroupMode = (GroupMode)LayoutReader.GetPropertyValue(reader, "GroupMode", typeof(GroupMode), GroupMode.Expanded);
			GroupTotals = (GroupTotals)LayoutReader.GetPropertyValue(reader, "GroupTotals", typeof(GroupTotals), GroupTotals.Never);
			_hideColumnsWhenGrouped = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "HideColumnsWhenGrouped", typeof(InheritableBoolean), InheritableBoolean.False);
			_hideSelection = (HideSelection)LayoutReader.GetPropertyValue(reader, "HideSelection", typeof(HideSelection), HideSelection.HideSelection);
			_hierarchical = (bool)LayoutReader.GetPropertyValue(reader, "Hierarchical", typeof(bool), false);
			_indent = (int)LayoutReader.GetPropertyValue(reader, "Indent", typeof(int), 18);
			NewRowPosition = (NewRowPosition)LayoutReader.GetPropertyValue(reader, "NewRowPosition", typeof(NewRowPosition), NewRowPosition.TopRow);
			_totalRowPosition = (TotalRowPosition)LayoutReader.GetPropertyValue(reader, "TotalRowPosition", typeof(TotalRowPosition), TotalRowPosition.BottomScrollable);
			ApplyNavigatorCaptionAndText((string)LayoutReader.GetPropertyValue(reader, "RecordNavigatorText", typeof(string), "Record:|of"));
			_repeatHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "RepeatHeaders", typeof(InheritableBoolean), InheritableBoolean.True);
			_rowHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "RowHeaders", typeof(InheritableBoolean), InheritableBoolean.False);
			_selectionMode = (SelectionMode)LayoutReader.GetPropertyValue(reader, "SelectionMode", typeof(SelectionMode), SelectionMode.SingleSelection);
			_showEmptyFields = (bool)LayoutReader.GetPropertyValue(reader, "ShowEmptyFields", typeof(bool), true);
			_tableSpacing = (int)LayoutReader.GetPropertyValue(reader, "TableSpacing", typeof(int), 4);
			TotalRow = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "TotalRow", typeof(InheritableBoolean), InheritableBoolean.False);
			_treeLines = (bool)LayoutReader.GetPropertyValue(reader, "TreeLines", typeof(bool), true);
			_view = (View)LayoutReader.GetPropertyValue(reader, "View", typeof(View), View.TableView);
			base.RecordNavigator = (bool)LayoutReader.GetPropertyValue(reader, "RecordNavigator", typeof(bool), false);
			base.ScrollBars = (ScrollBars)LayoutReader.GetPropertyValue(reader, "ScrollBars", typeof(ScrollBars), ScrollBars.Automatic);
			_tabKeyBehavior = (TabKeyBehavior)LayoutReader.GetPropertyValue(reader, "TabKeyBehavior", typeof(TabKeyBehavior), TabKeyBehavior.ColumnNavigation);
			_enterKeyBehavior = (EnterKeyBehavior)LayoutReader.GetPropertyValue(reader, "EnterKeyBehavior", typeof(EnterKeyBehavior), EnterKeyBehavior.NextRow);
			_newRowEnterKeyBehavior = (NewRowEnterKeyBehavior)LayoutReader.GetPropertyValue(reader, "NewRowEnterKeyBehavior", typeof(NewRowEnterKeyBehavior), NewRowEnterKeyBehavior.Default);
			_acceptsEscape = (bool)LayoutReader.GetPropertyValue(reader, "AcceptsEscape", typeof(bool), true);
			_imageSource = (ImageSource)LayoutReader.GetPropertyValue(reader, "ImageSource", typeof(ImageSource), ImageSource.ExternalImageList);
			_tableHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "TableHeaders", typeof(InheritableBoolean), InheritableBoolean.False);
			_updateOnLeave = (bool)LayoutReader.GetPropertyValue(reader, "UpdateOnLeave", typeof(bool), true);
			_delayEditionOnClick = (bool)LayoutReader.GetPropertyValue(reader, "DelayEditionOnClick", typeof(bool), false);
			_cellSelectionMode = (CellSelectionMode)LayoutReader.GetPropertyValue(reader, "CellSelectionMode", typeof(CellSelectionMode), CellSelectionMode.EntireRow);
			AllowChildTableGroups = (bool)LayoutReader.GetPropertyValue(reader, "AllowChildTableGroups", typeof(bool), false);
			_selectOnExpand = (bool)LayoutReader.GetPropertyValue(reader, "SelectOnExpand", typeof(bool), true);
			ColumnAutoSizeMode = (ColumnAutoSizeMode)LayoutReader.GetPropertyValue(reader, "ColumnAutoSizeMode", typeof(ColumnAutoSizeMode), ColumnAutoSizeMode.AllCellsAndHeader);
			_filterRowUpdateMode = (FilterRowUpdateMode)LayoutReader.GetPropertyValue(reader, "FilterRowUpdateMode", typeof(FilterRowUpdateMode), FilterRowUpdateMode.Default);
			_filterRowButtonStyle = (FilterRowButtonStyle)LayoutReader.GetPropertyValue(reader, "FilterRowButtonStyle", typeof(FilterRowButtonStyle), FilterRowButtonStyle.ClearButton);
			_defaultFilterRowComparison = (FilterConditionOperator)LayoutReader.GetPropertyValue(reader, "DefaultFilterRowComparison", typeof(FilterConditionOperator), FilterConditionOperator.Default);
			PropertyValue propertyValue = reader["BuiltInTexts"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(BuiltInTexts);
			}
			else
			{
				_builtInTexts.Reset();
			}
			ApplyRecordNavigatorBuiltInText();
		}

		public void LoadLayout(GridLayout layout)
		{
			if (layout == null || string.IsNullOrEmpty(layout.LayoutString))
			{
				throw new ArgumentNullException();
			}
			if (!layout.HasFullLayout())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			if (!UpdateData())
			{
				throw new InvalidOperationException("The pending edit is not valid and must be corrected or cancelled before a layout can be loaded.");
			}
			layout = layout.GetMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			PropertyValue propertyValue = mordexLayoutReader["GridLayoutData"];
			if (propertyValue != null)
			{
				_row = null;
				if (this is GridDropDown)
				{
					propertyValue.DeserializeObject(this);
				}
				else
				{
					ReadGridLayoutByMode(propertyValue.GetLayoutReader(), layout.GetPersistenceMode());
				}
				SetDataSourceAndMember(_dataSource, _dataMember, true);
				RefreshRowCountAndInvalidate();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDesignTimeLayout()
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			if (RootTable != null && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.TableStructure) == 0))
			{
				return true;
			}
			if (_formatStyleCollection.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.FormatStyles) == 0))
			{
				return true;
			}
			if (_dropDowns.Count > 0 && (_currentLayout == null || (_currentLayout.GetPersistenceMode() & PersistenceMode.DropDowns) == 0))
			{
				return true;
			}
			if (_currentLayout == null && _imageList != null && _imageList.Images.Count > 0)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTreeLineColor()
		{
			return !_treeLineColor.Equals(SystemColors.ControlDark);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGridLineColor()
		{
			return !_gridLineColor.Equals(SystemColors.ControlDark);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.Equals(SystemColors.Window);
		}

		public void LoadLayoutFile(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			// Parse first so a malformed file leaves the grid untouched.
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["GridLayoutFile"];
			if (propertyValue != null)
			{
				if (!UpdateData())
				{
					throw new InvalidOperationException("The pending edit is not valid and must be corrected or cancelled before a layout can be loaded.");
				}
				_row = null;
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				ReadGridLayoutByMode(layoutReader, (PersistenceMode)0);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				GridLayout gridLayout = _layouts.FindCurrentLayout();
				if (gridLayout != null)
				{
					_layouts.MarkCurrentLayout(gridLayout);
					gridLayout._dataSource = _dataSource;
					gridLayout._dataMember = _dataMember;
					CurrentLayout = gridLayout;
					ReadFilterRowCells(layoutReader);
				}
				else
				{
					CurrentLayout = null;
					OnLayoutLoad(EventArgs.Empty);
				}
				RefreshRowCountAndInvalidate();
			}
		}

		public void SaveLayoutFile(Stream stream)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			PropertyValue propertyValue = new PropertyValue("GridLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.DataGrid.MxDataGrid";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBuiltInTextsData()
		{
			if (_currentLayout == null)
			{
				return BuiltInTexts.HasCustomTexts();
			}
			return false;
		}

		internal void SaveCurrentRowBeforeSuspend()
		{
			_isCurrentCellVisible = false;
			_isNoRowSelected = Row == -3;
			_mordexRow = null;
			_lastRecordPosition = 0;
			if (_row == null)
			{
				return;
			}
			bool isLayoutValid = _isLayoutValid;
			_isLayoutValid = true;
			Rectangle cellBounds = GetCellBounds(Row, null);
			_isLayoutValid = isLayoutValid;
			_isCurrentCellVisible = !cellBounds.IsEmpty;
			if (_row.RowType == RowType.NewRecord)
			{
				_lastRecordPosition = -1;
			}
			else if (_row.RowType != RowType.Record)
			{
				if (_row.RowType == RowType.FilterRow)
				{
					_lastRecordPosition = -2;
				}
				else if (_row != null)
				{
					_mordexRow = _row.GetLogicalRow();
				}
			}
		}

		private ApplicationSettings GetApplicationSettings()
		{
			if (_mordexApplicationSettings == null)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (_mordexApplicationSettings.SettingsKey != SettingsKey)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _mordexApplicationSettings;
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (_isInitializing)
				{
					MemoryStream memoryStream = new MemoryStream();
					SaveLayoutFile(memoryStream);
					memoryStream.Flush();
					memoryStream.Position = 0L;
					GetApplicationSettings().DefaultLayoutSettings = memoryStream.ToArray();
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = null;
				}
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					MemoryStream otherMemoryStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(otherMemoryStream);
					otherMemoryStream.Close();
					otherMemoryStream.Dispose();
					otherMemoryStream = null;
				}
			}
		}

		public void ResetComponentSettings()
		{
			if (!base.DesignMode)
			{
				GetApplicationSettings().Reset();
				LoadComponentSettings();
			}
		}

		public void SaveComponentSettings()
		{
			MemoryStream memoryStream = new MemoryStream();
			SaveLayoutFile(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			GetApplicationSettings().LayoutSettings = memoryStream.ToArray();
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}
	}
}
