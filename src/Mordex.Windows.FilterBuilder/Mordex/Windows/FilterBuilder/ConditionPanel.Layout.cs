using System;
using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Mordex.Windows.FilterBuilder
{
	internal partial class ConditionPanel
	{
		private const int ConnectorTopInset = 5;

		private const int ConnectorBranchLength = 12;

		private const int ConnectorRowOffset = 3;

		private const int ConnectorLeftOffset = 10;

		private const int ConnectorRightOffset = 11;

		private void RefreshControls()
		{
			if (Condition == null)
			{
				return;
			}
			base.RowStyles[0].Height = CurrentRowHeight;
			_fieldButton.Height = ButtonHeight;
			_operatorButton.Height = ButtonHeight;
			_addButton.Height = ButtonHeight;
			string fieldText = _field == null ? _texts[BuiltInText.ChooseFieldText] : FilterFields.DisplayName(_field);
			_fieldButton.Text = fieldText.Trim();
			int fieldTextWidth = TextRenderer.MeasureText(fieldText, Font, Size.Empty, TextFormatFlags.NoPrefix).Width;
			base.ColumnStyles[FieldColumn].Width = fieldTextWidth + ButtonTextPadding;
			string operatorText = FilterBuilderTexts.ConditionOperatorText(_conditionOperator, _texts);
			base.ColumnStyles[OperatorColumn].Width = TextRenderer.MeasureText(operatorText, Font).Width + ButtonTextPadding;
			_operatorButton.Text = operatorText;
			int editControlWidth = _owner.FilterBuilder.EditControlWidth;
			base.ColumnStyles[FirstValueColumn].Width = _firstEditor == null ? 0f : editControlWidth;
			base.ColumnStyles[SecondValueColumn].Width = _secondEditor == null ? 0f : editControlWidth;
			base.ColumnStyles[AddColumn].Width = IconButtonWidth;
			base.ColumnStyles[RemoveColumn].Width = IconButtonWidth;
			LayoutChildRows();
			if (base.Parent != null)
			{
				UpdateMinimumSize();
			}
		}

		private void LayoutChildRows()
		{
			int totalHeight = CurrentRowHeight;
			for (int i = 0; i < _childRows.Count; i++)
			{
				ChildRow row = _childRows[i];
				string text = FilterBuilderTexts.LogicalOperatorText(row.Panel.Condition.LogicalOperator, row.Panel.Condition.Negation, _texts);
				row.Host.ColumnStyles[0].Width = TextRenderer.MeasureText(text, Font).Width + LogicalButtonTextPadding;
				row.LogicalButton.Text = text;
				base.RowStyles[i + 1].Height = row.Panel.Height + ChildRowGap;
				row.LogicalButton.Height = ButtonHeight;
				totalHeight += row.Panel.Height + ChildRowGap;
			}
			base.Height = totalHeight;
		}

		internal void RefreshLayoutRecursive()
		{
			foreach (ChildRow row in _childRows)
			{
				row.Panel.RefreshLayoutRecursive();
			}
			UpdateMinimumSize();
			RefreshControls();
		}

		private int ComputeMinimumHeight()
		{
			int height = CurrentRowHeight;
			foreach (ChildRow row in _childRows)
			{
				height += row.Panel.ComputeMinimumHeight() + ChildRowGap;
			}
			return height;
		}

		// Width of every column except the last one, which fills the rest.
		private int ComputeFixedColumnsWidth()
		{
			if (base.ColumnCount <= 0)
			{
				return 0;
			}
			int width = 0;
			for (int i = 0; i < base.ColumnCount - 1; i++)
			{
				width += (int)base.ColumnStyles[i].Width;
			}
			return width + ChildRowGap;
		}

		private void UpdateMinimumSize()
		{
			if (base.Disposing || base.IsDisposed)
			{
				return;
			}
			int width = ComputeFixedColumnsWidth();
			int height = ComputeMinimumHeight();
			foreach (ChildRow row in _childRows)
			{
				int childRight;
				if (RightToLeft != RightToLeft.Yes)
				{
					Point screen = row.Panel.PointToScreen(new Point(row.Panel.MinimumSize.Width, row.Panel.Top));
					childRight = PointToClient(screen).X + MinimumSizePadding;
				}
				else
				{
					childRight = row.Panel.MinimumSize.Width + (int)row.Host.ColumnStyles[0].Width + MinimumSizePadding;
				}
				width = Math.Max(width, childRight);
			}
			MinimumSize = new Size(width, height);
		}

		private void OnChildMinimumSizeChanged(object sender, EventArgs eventArgs)
		{
			if (!(sender is ConditionPanel child) || !child.IsHandleCreated)
			{
				return;
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				UpdateMinimumSize();
				return;
			}
			Point local = PointToClient(child.PointToScreen(new Point(child.MinimumSize.Width, child.MinimumSize.Height)));
			int width = local.X + MinimumSizePadding;
			int height = local.Y + MinimumSizePadding;
			if (width > MinimumSize.Width && height > MinimumSize.Height)
			{
				MinimumSize = new Size(width, height);
			}
			else
			{
				UpdateMinimumSize();
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (RefreshTextsOnPaint)
			{
				_owner.FilterBuilder.RefreshTexts();
				RefreshTextsOnPaint = false;
			}
			DrawThemedBackground(eventArgs.Graphics);
			DrawChildConnectors(eventArgs.Graphics);
		}

		private void DrawChildConnectors(Graphics graphics)
		{
			if (Condition == null || Condition.Conditions.Count <= 0 || base.RowStyles.Count <= 0)
			{
				return;
			}
			bool rightToLeft = RightToLeft == RightToLeft.Yes;
			int x = rightToLeft ? base.ClientRectangle.Right - ConnectorRightOffset : base.ClientRectangle.Left + ConnectorLeftOffset;
			int branchStep = rightToLeft ? -ConnectorBranchLength : ConnectorBranchLength;
			int previousY = CurrentRowHeight - ConnectorTopInset;
			int rowTop = (int)base.RowStyles[0].Height;
			Pen pen = GdiCache.Pen(_owner.FilterBuilder.ConditionBorderColor);
			for (int i = 1; i < base.RowCount; i++)
			{
				int branchY = rowTop + CurrentRowHeight / 2 + ConnectorRowOffset;
				graphics.DrawLine(pen, x, previousY, x, branchY);
				graphics.DrawLine(pen, x, branchY, x + branchStep, branchY);
				rowTop += (int)base.RowStyles[i].Height;
				previousY = branchY;
			}
		}

		private void DrawThemedBackground(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			int radius = theme.Shape.CornerRadius;
			SmoothingMode previousMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			theme.FillRounded(graphics, base.ClientRectangle, ThemeRole.SurfaceAlt, radius);
			theme.DrawRounded(graphics, base.ClientRectangle, ThemeRole.Border, radius);
			graphics.SmoothingMode = previousMode;
		}
	}
}
