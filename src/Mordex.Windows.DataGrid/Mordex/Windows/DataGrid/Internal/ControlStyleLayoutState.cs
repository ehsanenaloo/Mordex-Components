using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class ControlStyleLayoutState
	{
		private MxColorSchemeAppearanceState _mordexColorSchemeAppearanceState;

		private MxColorSchemeAppearanceState _controlStyleLayoutStateMordexColorSchemeAppearanceState;

		private MxColorSchemeAppearanceState _groupRowsStyle;

		private MxColorSchemeAppearanceState _headersStyle;

		private MxColorSchemeAppearanceState _cardsStyle;

		private MxColorSchemeAppearanceState _editControlsStyle;

		private MxColorSchemeAppearanceState _controlBorderStyle;

		private MxColorSchemeAppearanceState _checkBoxesStyle;

		private MxColorSchemeAppearanceState _treeGlyphsStyle;

		private CellRendererBase[] _gridRenderer5Array;

		private ControlStyle _owner;

		private GridlinePenRenderer _gridLinePenRenderer;

		internal Rectangle _defaultBounds;

		internal TextAlignment _textAlignment;

		internal ControlStyle _appliedControlStyle;

		internal Color _treelineColor;

		internal bool _showTreelines;

		internal ControlStyleLayoutState(ControlStyle owner)
		{
			_treelineColor = SystemColors.Control;
			_showTreelines = true;
			_defaultBounds = new Rectangle(0, 0, 100, 100);
			_textAlignment = TextAlignment.Center;
			_owner = owner;
			_gridRenderer5Array = new CellRendererBase[23];
		}

		internal void ReleaseAll()
		{
			DisposeRenderers();
			_cardsStyle = null;
			_checkBoxesStyle = null;
			_controlBorderStyle = null;
			_editControlsStyle = null;
			_appliedControlStyle = null;
			_controlStyleLayoutStateMordexColorSchemeAppearanceState = null;
			_mordexColorSchemeAppearanceState = null;
			_groupRowsStyle = null;
			_headersStyle = null;
			_treeGlyphsStyle = null;
			_owner = null;
			_gridRenderer5Array = null;
		}

		internal GridlinePenRenderer GetGridLinePenRenderer()
		{
			if (_gridLinePenRenderer == null)
			{
				Color color = Color.Empty;
				if (GetGridlinesStyle()._isThemed)
				{
					color = GetGridlinesStyle().GetResolvedAppearance().GetGridColor(GridColorKey.Gridline);
				}
				else if (_owner._printDocument is MxDataGrid)
				{
					color = ((MxDataGrid)_owner._printDocument).GridLineColor;
				}
				else if (_owner._printDocument is MxDataGridPrintDocument)
				{
					color = ((MxDataGridPrintDocument)_owner._printDocument).DataGrid.GridLineColor;
				}
				_gridLinePenRenderer = new GridlinePenRenderer(color, _owner);
			}
			return _gridLinePenRenderer;
		}

		internal MxColorSchemeAppearanceState GetControlBorderStyle()
		{
			if (_controlBorderStyle == null)
			{
				_controlBorderStyle = _owner.ResolveAppearanceState(GridStyleArea.ControlBorder);
			}
			return _controlBorderStyle;
		}

		internal MxColorSchemeAppearanceState GetTreeGlyphsStyle()
		{
			if (_treeGlyphsStyle == null)
			{
				_treeGlyphsStyle = _owner.ResolveAppearanceState(GridStyleArea.TreeGliphs);
			}
			return _treeGlyphsStyle;
		}

		internal MxColorSchemeAppearanceState GetGroupRowsStyle()
		{
			if (_groupRowsStyle == null)
			{
				_groupRowsStyle = _owner.ResolveAppearanceState(GridStyleArea.GroupRows);
			}
			return _groupRowsStyle;
		}

		internal MxColorSchemeAppearanceState GetEditControlsStyle()
		{
			if (_editControlsStyle == null)
			{
				_editControlsStyle = _owner.ResolveAppearanceState(GridStyleArea.EditControls);
			}
			return _editControlsStyle;
		}

		internal MxColorSchemeAppearanceState GetHeadersStyle()
		{
			if (_headersStyle == null)
			{
				_headersStyle = _owner.ResolveAppearanceState(GridStyleArea.Headers);
			}
			return _headersStyle;
		}

		internal MxColorSchemeAppearanceState GetCardsStyle()
		{
			if (_cardsStyle == null)
			{
				_cardsStyle = _owner.ResolveAppearanceState(GridStyleArea.Cards);
			}
			return _cardsStyle;
		}

		internal MxColorSchemeAppearanceState GetCheckBoxesStyle()
		{
			if (_checkBoxesStyle == null)
			{
				_checkBoxesStyle = _owner.ResolveAppearanceState(GridStyleArea.CheckBoxes);
			}
			return _checkBoxesStyle;
		}

		internal MxColorSchemeAppearanceState GetGroupByBoxStyle()
		{
			if (_mordexColorSchemeAppearanceState == null)
			{
				_mordexColorSchemeAppearanceState = _owner.ResolveAppearanceState(GridStyleArea.GroupByBox);
			}
			return _mordexColorSchemeAppearanceState;
		}

		internal MxColorSchemeAppearanceState GetGridlinesStyle()
		{
			if (_controlStyleLayoutStateMordexColorSchemeAppearanceState == null)
			{
				_controlStyleLayoutStateMordexColorSchemeAppearanceState = _owner.ResolveAppearanceState(GridStyleArea.Gridlines);
			}
			return _controlStyleLayoutStateMordexColorSchemeAppearanceState;
		}

		internal void ResetCachedStyles()
		{
			_mordexColorSchemeAppearanceState = null;
			_controlStyleLayoutStateMordexColorSchemeAppearanceState = null;
			_groupRowsStyle = null;
			_headersStyle = null;
			_cardsStyle = null;
			_editControlsStyle = null;
			_controlBorderStyle = null;
			_checkBoxesStyle = null;
			_treeGlyphsStyle = null;
			DisposeRenderers();
		}

		internal void DisposeRenderers()
		{
			for (int i = 0; i < _gridRenderer5Array.Length; i++)
			{
				if (_gridRenderer5Array[i] != null)
				{
					_gridRenderer5Array[i].ReleaseResources();
					_gridRenderer5Array[i] = null;
				}
			}
			if (_gridLinePenRenderer != null)
			{
				_gridLinePenRenderer.DisposePen();
				_gridLinePenRenderer = null;
			}
		}

		internal CellRendererBase GetRenderer(GridGroupLayoutState gridGroupLayoutState)
		{
			switch (gridGroupLayoutState._type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.CardCell:
			{
				GridCell gridCell = gridGroupLayoutState.GetCell();
				if (gridCell != null)
				{
					if (gridCell.CellDisplayType == ColumnType.CheckBox)
					{
						return GetCheckBoxCellRenderer();
					}
					if (gridCell.CellDisplayType == ColumnType.BoundImage)
					{
						return GetImageCellRenderer();
					}
					if (gridGroupLayoutState._column.MultipleValues)
					{
						return GetMultiValueCellRenderer();
					}
				}
				return GetGridlinesRenderer();
			}
			case LayoutItemType.PreviewRow:
				return GetEmptyAreaRenderer();
			case LayoutItemType.CellColumnButton:
			case LayoutItemType.CellDropDownButton:
			case LayoutItemType.FilterClearButton:
			case LayoutItemType.FilterRowDropDown:
			case LayoutItemType.FreeButton:
				return GetButtonControlRenderer();
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
			case LayoutItemType.CellUpDownBackground:
				return GetSpinButtonRenderer();
			case LayoutItemType.GroupByBoxBackground:
				return GetGroupByBoxContentRenderer();
			case LayoutItemType.CardSelector:
				return GetCheckBoxCellRenderer();
			case LayoutItemType.GroupByBoxInfo:
			case LayoutItemType.GroupByBoxTableHeader:
				return GetGroupByBoxRenderer();
			case LayoutItemType.FilterRowCell:
			{
				GridCell gridCell = gridGroupLayoutState.GetCell();
				if (gridCell != null && gridCell.CellDisplayType == ColumnType.CheckBox && ((gridGroupLayoutState._column.EditType == EditType.CheckBox && gridGroupLayoutState._column.FilterEditType == FilterEditType.SameAsEditType) || gridGroupLayoutState._column.FilterEditType == FilterEditType.CheckBox) && !GridHelper.IsNullOrDBNull(gridGroupLayoutState.GetCellValue()))
				{
					return GetCheckBoxCellRenderer();
				}
				return GetGridlinesRenderer();
			}
			case LayoutItemType.GroupHeaderCell:
				return GetTextCellRenderer();
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.CardHeader:
				return GetGridlinesRenderer();
			case LayoutItemType.EmptyCell:
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.GroupFooter)
				{
					return GetGroupRowSecondaryRenderer();
				}
				return GetGridlinesRenderer();
			case LayoutItemType.CardCaption:
				return GetCardCellRenderer();
			case LayoutItemType.GroupHeaderRow:
				return GetGroupRowRenderer();
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.GroupIndent:
				return GetGroupRowSecondaryRenderer();
			case LayoutItemType.GroupExpandBox:
				return GetFormatStyleRenderer(gridGroupLayoutState);
			case LayoutItemType.NewRowSeparator:
			case LayoutItemType.TotalRowSeparator:
			case LayoutItemType.ColumnSetHeader:
			case LayoutItemType.EmptyHeader:
			case LayoutItemType.TableHeader:
				return GetHeaderCellRenderer();
			case LayoutItemType.RowHeader:
				return GetHeaderFilterRenderer();
			case LayoutItemType.ColumnHeader:
				if (gridGroupLayoutState._column != null && gridGroupLayoutState._column.ActAsSelector && gridGroupLayoutState._column.UseHeaderSelector)
				{
					return GetCheckBoxColumnHeaderRenderer();
				}
				return GetHeaderGroupRenderer();
			case LayoutItemType.GroupByBoxHeader:
				return GetHeaderSetRenderer();
			case LayoutItemType.ExpandBox:
			case LayoutItemType.SelfReferencingExpandBox:
				return GetTreeGlyphRenderer();
			case LayoutItemType.CardExpandBox:
				return GetCardRenderer();
			case LayoutItemType.Connector:
				return GetTreelineRenderer();
			case LayoutItemType.RowSelector:
				return GetCheckBoxHeaderRenderer();
			default:
				return null;
			}
		}

		private CellRendererBase GetGroupByBoxRenderer()
		{
			if (_gridRenderer5Array[1] == null)
			{
				PlainCellRenderer plainCellRenderer = new PlainCellRenderer(_owner, GetGroupByBoxStyle());
				_gridRenderer5Array[1] = plainCellRenderer;
			}
			return _gridRenderer5Array[1];
		}

		private CellRendererBase GetGridlinesRenderer()
		{
			if (_gridRenderer5Array[0] == null)
			{
				CellRendererBase cellRendererBase = new CellRendererBase(_owner, GetGridlinesStyle());
				_gridRenderer5Array[0] = cellRendererBase;
			}
			return _gridRenderer5Array[0];
		}

		private CellRendererBase GetGroupRowRenderer()
		{
			if (_gridRenderer5Array[2] == null)
			{
				LayoutStateCellRenderer layoutStateCellRenderer = new LayoutStateCellRenderer(_owner, GetGroupRowsStyle());
				layoutStateCellRenderer._gradientBounds = _defaultBounds;
				_gridRenderer5Array[2] = layoutStateCellRenderer;
			}
			return _gridRenderer5Array[2];
		}

		private CellRendererBase GetGroupRowSecondaryRenderer()
		{
			if (_gridRenderer5Array[3] == null)
			{
				GroupRowBackgroundRenderer groupRowBackgroundRenderer = new GroupRowBackgroundRenderer(_owner, GetGroupRowsStyle());
				groupRowBackgroundRenderer._gradientBounds = _defaultBounds;
				_gridRenderer5Array[3] = groupRowBackgroundRenderer;
			}
			return _gridRenderer5Array[3];
		}

		private CellRendererBase GetFormatStyleRenderer(GridGroupLayoutState gridGroupLayoutState)
		{
			GridFormatStyleRenderer gridFormatStyleRenderer = (GridFormatStyleRenderer)_gridRenderer5Array[4];
			if (gridFormatStyleRenderer == null)
			{
				gridFormatStyleRenderer = new GridFormatStyleRenderer(_owner, GetGroupRowsStyle());
				gridFormatStyleRenderer._gradientBounds = _defaultBounds;
				_gridRenderer5Array[4] = gridFormatStyleRenderer;
			}
			if (gridFormatStyleRenderer._sourceTable != gridGroupLayoutState.GetOwnerTable())
			{
				if (gridGroupLayoutState.GetOwnerTable() != null)
				{
					gridFormatStyleRenderer._gridFormatStyle = gridGroupLayoutState.GetOwnerTable().GetGroupRowFormatStyle(((IGridEnvironment)_owner).IsPrinting());
					if (_owner._printDocument is MxDataGridPrintDocument)
					{
						gridFormatStyleRenderer._gridFormatStyle = gridFormatStyleRenderer._gridFormatStyle.CreatePrintStyle((MxDataGridPrintDocument)_owner._printDocument);
					}
				}
				gridFormatStyleRenderer._sourceTable = gridGroupLayoutState.GetOwnerTable();
			}
			return gridFormatStyleRenderer;
		}

		private CellRendererBase GetHeaderCellRenderer()
		{
			if (_gridRenderer5Array[5] == null)
			{
				HeaderCellRendererBase headerCellRendererBase = new HeaderCellRendererBase(_owner, GetHeadersStyle());
				headerCellRendererBase._gridRenderer16Boolean = _owner.ShouldDrawHeaderBorder();
				_gridRenderer5Array[5] = headerCellRendererBase;
			}
			return _gridRenderer5Array[5];
		}

		private CellRendererBase GetHeaderGroupRenderer()
		{
			if (_gridRenderer5Array[6] == null)
			{
				ColumnHeaderCellRenderer columnHeaderCellRenderer = new ColumnHeaderCellRenderer(_owner, GetHeadersStyle());
				columnHeaderCellRenderer._gridRenderer16Boolean = _owner.ShouldDrawHeaderBorder();
				_gridRenderer5Array[6] = columnHeaderCellRenderer;
			}
			return _gridRenderer5Array[6];
		}

		private CellRendererBase GetHeaderSetRenderer()
		{
			if (_gridRenderer5Array[22] == null)
			{
				ColumnHeaderCellRenderer headerSetRenderer = new ColumnHeaderCellRenderer(_owner, GetHeadersStyle());
				headerSetRenderer._gridRenderer16Boolean = _owner.ShouldDrawHeaderBorder();
				_gridRenderer5Array[22] = headerSetRenderer;
			}
			return _gridRenderer5Array[22];
		}

		private CellRendererBase GetHeaderFilterRenderer()
		{
			if (_gridRenderer5Array[7] == null)
			{
				RowHeaderCellRenderer rowHeaderCellRenderer = new RowHeaderCellRenderer(_owner, GetHeadersStyle());
				rowHeaderCellRenderer._gridRenderer16Boolean = _owner.ShouldDrawHeaderBorder();
				_gridRenderer5Array[7] = rowHeaderCellRenderer;
			}
			return _gridRenderer5Array[7];
		}

		private CellRendererBase GetTreelineRenderer()
		{
			if (_gridRenderer5Array[9] == null)
			{
				TreelineRenderer treelineRenderer = new TreelineRenderer(_owner, GetGridlinesStyle(), _treelineColor, _showTreelines);
				_gridRenderer5Array[9] = treelineRenderer;
			}
			return _gridRenderer5Array[9];
		}

		private CellRendererBase GetTreeGlyphRenderer()
		{
			if (_gridRenderer5Array[8] == null)
			{
				GroupExpanderRenderer groupExpanderRenderer = new GroupExpanderRenderer(_owner, GetTreeGlyphsStyle(), _treelineColor, _showTreelines);
				_gridRenderer5Array[8] = groupExpanderRenderer;
			}
			return _gridRenderer5Array[8];
		}

		private CellRendererBase GetCheckBoxHeaderRenderer()
		{
			if (_gridRenderer5Array[21] == null)
			{
				ButtonCellRenderer buttonCellRenderer = new ButtonCellRenderer(_owner, GetCheckBoxesStyle());
				_gridRenderer5Array[21] = buttonCellRenderer;
			}
			return _gridRenderer5Array[21];
		}

		private CellRendererBase GetCheckBoxCellRenderer()
		{
			if (_gridRenderer5Array[10] == null)
			{
				AlignedTextCellRenderer alignedTextCellRenderer = new AlignedTextCellRenderer(_owner, GetCheckBoxesStyle());
				alignedTextCellRenderer.SetTextAlignment(_textAlignment);
				_gridRenderer5Array[10] = alignedTextCellRenderer;
			}
			return _gridRenderer5Array[10];
		}

		private CellRendererBase GetCheckBoxColumnHeaderRenderer()
		{
			if (_gridRenderer5Array[11] == null)
			{
				CheckBoxHeaderCellRenderer checkBoxHeaderCellRenderer = new CheckBoxHeaderCellRenderer(_owner, GetHeadersStyle(), GetCheckBoxesStyle());
				checkBoxHeaderCellRenderer._gridRenderer16Boolean = _owner.ShouldDrawHeaderBorder();
				_gridRenderer5Array[11] = checkBoxHeaderCellRenderer;
			}
			return _gridRenderer5Array[11];
		}

		private CellRendererBase GetCardRenderer()
		{
			if (_gridRenderer5Array[12] == null)
			{
				CardCaptionButtonRenderer cardCaptionButtonRenderer = new CardCaptionButtonRenderer(_owner, GetCardsStyle());
				_gridRenderer5Array[12] = cardCaptionButtonRenderer;
			}
			return _gridRenderer5Array[12];
		}

		private CellRendererBase GetCardCellRenderer()
		{
			if (_gridRenderer5Array[13] == null)
			{
				RoundedRowRenderer roundedRowRenderer = new RoundedRowRenderer(_owner, GetCardsStyle());
				_gridRenderer5Array[13] = roundedRowRenderer;
			}
			return _gridRenderer5Array[13];
		}

		internal CellRendererBase GetButtonControlRenderer()
		{
			if (_gridRenderer5Array[14] == null)
			{
				ButtonControlRenderer buttonControlRenderer = new ButtonControlRenderer(_owner, GetEditControlsStyle());
				if (_appliedControlStyle != null)
				{
					buttonControlRenderer._buttonControlStyle = _appliedControlStyle;
				}
				else
				{
					buttonControlRenderer._buttonControlStyle = _owner;
				}
				_gridRenderer5Array[14] = buttonControlRenderer;
			}
			return _gridRenderer5Array[14];
		}

		private CellRendererBase GetImageCellRenderer()
		{
			if (_gridRenderer5Array[18] == null)
			{
				ImageCellRenderer imageCellRenderer = new ImageCellRenderer(_owner, GetGridlinesStyle());
				_gridRenderer5Array[18] = imageCellRenderer;
			}
			return _gridRenderer5Array[18];
		}

		private CellRendererBase GetTextCellRenderer()
		{
			if (_gridRenderer5Array[20] == null)
			{
				FilledCellRenderer filledCellRenderer = new FilledCellRenderer(_owner, GetGridlinesStyle());
				_gridRenderer5Array[20] = filledCellRenderer;
			}
			return _gridRenderer5Array[20];
		}

		private CellRendererBase GetMultiValueCellRenderer()
		{
			if (_gridRenderer5Array[19] == null)
			{
				ListCellRenderer listCellRenderer = new ListCellRenderer(_owner, GetGridlinesStyle());
				_gridRenderer5Array[19] = listCellRenderer;
			}
			return _gridRenderer5Array[19];
		}

		private CellRendererBase GetSpinButtonRenderer()
		{
			if (_gridRenderer5Array[17] == null)
			{
				SpinButtonRenderer spinButtonRenderer = new SpinButtonRenderer(_owner, GetEditControlsStyle());
				if (_appliedControlStyle != null)
				{
					spinButtonRenderer._buttonControlStyle = _appliedControlStyle;
				}
				else
				{
					spinButtonRenderer._buttonControlStyle = _owner;
				}
				_gridRenderer5Array[17] = spinButtonRenderer;
			}
			return _gridRenderer5Array[17];
		}

		private CellRendererBase GetGroupByBoxContentRenderer()
		{
			if (_gridRenderer5Array[15] == null)
			{
				GroupByBoxRenderer groupByBoxRenderer = new GroupByBoxRenderer(_owner, GetGroupByBoxStyle());
				_gridRenderer5Array[15] = groupByBoxRenderer;
			}
			return _gridRenderer5Array[15];
		}

		private CellRendererBase GetEmptyAreaRenderer()
		{
			if (_gridRenderer5Array[16] == null)
			{
				CustomTextCellRenderer customTextCellRenderer = new CustomTextCellRenderer(_owner, GetGridlinesStyle());
				_gridRenderer5Array[16] = customTextCellRenderer;
			}
			return _gridRenderer5Array[16];
		}
	}
}
