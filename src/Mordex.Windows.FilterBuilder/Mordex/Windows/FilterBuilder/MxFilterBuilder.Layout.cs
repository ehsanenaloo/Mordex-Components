using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.FilterBuilder
{
	public partial class MxFilterBuilder
	{
		private const int ScrollStepSize = 15;

		private const int InitialRowPadding = 16;

		private const int RowPaddingAfterFontChange = 14;

		private const int MinSizePadding = 16;

		private const int NegateButtonTextPadding = 14;

		private const int NegateButtonHeightInset = 9;

		private static readonly Padding RootMargin = new Padding(8, 8, 8, 0);

		private static readonly Padding RootMarginWithNegateButton = new Padding(5, 8, 8, 0);

		private static readonly Padding NegateButtonMargin = new Padding(8, 13, 0, 0);

		private TransparentTableLayoutPanel _hostPanel;

		private ConditionPanel _rootPanel;

		private MxButton _negateButton;

		private int _rowHeight;

		internal int RowHeight => _rowHeight;

		private void InitializeComponents()
		{
			_commandManager = new MxCommandManager();
			_editControlWidth = DefaultEditControlWidth;
			_builtInTextList = new BuiltInTextList(this);
			_filterConditionOperators = true;
			_availableLogicalOperators = AvailableLogicalOperators.And | AvailableLogicalOperators.AndNot | AvailableLogicalOperators.Or | AvailableLogicalOperators.OrNot;
			_availableConditionOperators = AvailableConditionOperators.Default;
			_defaultConditionOperator = ConditionOperator.Equal;
			_rowHeight = Font.Height + InitialRowPadding;
			_hostPanel = new TransparentTableLayoutPanel();
			_hostPanel.MouseUp += OnTransparentTableLayoutPanelMouseUp;
			_hostPanel.ColumnCount = 1;
			_hostPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			ChildControl = _hostPanel;
			base.ScrollMode = ScrollMode.Both;
			base.ScrollStep = ScrollStepSize;
			ApplyThemeColors();
			MinSize = Size.Empty;
			base.InnerAreaStyle = PanelInnerAreaStyle.UseFormatStyle;
		}

		private void SetRowHeight(int rowHeight)
		{
			if (_rowHeight != rowHeight)
			{
				_rowHeight = rowHeight;
				RefreshConditionLayout();
			}
		}

		internal void RefreshConditionLayout()
		{
			_rootPanel?.RefreshLayoutRecursive();
		}

		// Creates a root condition from the source control or the table.
		internal void CreateRootCondition()
		{
			IMordexCondition condition = null;
			if (_sourceControl != null)
			{
				condition = _sourceControl.CreateCondition();
			}
			else if (_table != null)
			{
				condition = _table.CreateCondition();
			}
			if (condition != null)
			{
				condition.ConditionOperator = DefaultConditionOperator;
				FilterCondition = condition;
			}
		}

		// Throws away the root panel and builds it again from the current table and condition.
		internal void RebuildRootPanel()
		{
			if (_rootPanel != null)
			{
				_rootPanel.MinimumSizeUpdated -= OnRootPanelMinimumSizeChanged;
				_rootPanel.Dispose();
				_rootPanel = null;
			}
			ResetFieldMenu(reloadConditions: false);
			if (_table == null)
			{
				RemoveNegateButton();
				return;
			}
			if (_allowNegateRootCondition)
			{
				EnsureNegateButton();
				UpdateNegateButtonText();
			}
			else
			{
				RemoveNegateButton();
			}
			_rootPanel = new ConditionPanel(this, _filterCondition);
			_rootPanel.Margin = _allowNegateRootCondition ? RootMarginWithNegateButton : RootMargin;
			_rootPanel.Dock = DockStyle.Top;
			_hostPanel.Controls.Add(_rootPanel, _hostPanel.ColumnCount - 1, 0);
			OnRootPanelMinimumSizeChanged(_rootPanel, EventArgs.Empty);
			_rootPanel.MinimumSizeUpdated += OnRootPanelMinimumSizeChanged;
		}

		private void OnRootPanelMinimumSizeChanged(object sender, EventArgs eventArgs)
		{
			if (sender is ConditionPanel panel)
			{
				MinSize = new Size(panel.MinimumSize.Width + MinSizePadding, panel.MinimumSize.Height + MinSizePadding);
			}
			else
			{
			ApplyThemeColors();
			MinSize = Size.Empty;
			}
		}

		private void EnsureNegateButton()
		{
			if (_negateButton != null)
			{
				return;
			}
			_negateButton = new MxButton();
			_negateButton.ShowFocusRectangle = false;
			_negateButton.TextHorizontalAlignment = TextAlignment.Center;
			_negateButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_negateButton.Height = RowHeight - NegateButtonHeightInset;
			_negateButton.Margin = NegateButtonMargin;
			_negateButton.Click += OnNegateButtonClick;
			UpdateNegateButtonText();
			_hostPanel.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Absolute, NegateButtonColumnWidth()));
			_hostPanel.Controls.Add(_negateButton, 0, 0);
			_hostPanel.ColumnCount++;
		}

		private void RemoveNegateButton()
		{
			if (_negateButton == null)
			{
				return;
			}
			_negateButton.Click -= OnNegateButtonClick;
			_negateButton.Dispose();
			_negateButton = null;
			_hostPanel.ColumnCount--;
			_hostPanel.ColumnStyles.RemoveAt(0);
		}

		private void UpdateNegateButtonText()
		{
			bool negated = _filterCondition != null && _filterCondition.Negation;
			_negateButton.Text = _builtInTextList[negated ? BuiltInText.NegationText : BuiltInText.AssertConditionText];
			_negateButton.ToolTipText = _builtInTextList[negated ? BuiltInText.AssertTooltip : BuiltInText.NegateTooltip];
		}

		// Wide enough for the longer of the two button texts.
		private int NegateButtonColumnWidth()
		{
			int negationWidth = TextRenderer.MeasureText(_builtInTextList[BuiltInText.NegationText], Font).Width;
			int assertionWidth = TextRenderer.MeasureText(_builtInTextList[BuiltInText.AssertConditionText], Font).Width;
			return Math.Max(negationWidth, assertionWidth) + NegateButtonTextPadding;
		}

		private void OnNegateButtonClick(object sender, EventArgs eventArgs)
		{
			if (_filterCondition == null)
			{
				return;
			}
			_filterCondition.Negation = !_filterCondition.Negation;
			UpdateNegateButtonText();
			if (_autoApply)
			{
				ApplyFilter();
			}
			OnFilterConditionChanged(EventArgs.Empty);
		}

		internal void OnBuiltInTextChanged(BuiltInText text)
		{
			if (_allowNegateRootCondition && (text == BuiltInText.AssertConditionText || text == BuiltInText.NegationText))
			{
				RefreshTexts();
			}
			if (_rootPanel != null)
			{
				_rootPanel.RefreshTextsOnPaint = true;
			}
			Invalidate(invalidateChildren: true);
		}

		private void OnBuiltInTextsReplaced()
		{
			RebuildRootPanel();
			if (_rootPanel != null)
			{
				_rootPanel.RefreshTextsOnPaint = true;
			}
			Invalidate(invalidateChildren: true);
		}

		internal void RefreshTexts()
		{
			if (_negateButton != null)
			{
				UpdateNegateButtonText();
				_hostPanel.ColumnStyles[0].Width = NegateButtonColumnWidth();
			}
			_rootPanel?.RefreshTexts();
		}
	}
}