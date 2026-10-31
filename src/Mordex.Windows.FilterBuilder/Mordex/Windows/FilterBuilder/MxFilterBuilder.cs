using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.FilterBuilder
{
	[Designer("Mordex.Windows.FilterBuilder.Design.FilterBuilderDesigner, Mordex.Windows.FilterBuilder.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxFilterBuilder : MxScrollPanel, IFilterBuilderElement
	{
		public delegate void FetchFieldValuesEventHandler(object sender, FetchFieldValuesEventArgs e);

		private const int DefaultEditControlWidth = 150;

		private const int MouseReleasePollInterval = 50;

		private static readonly object FetchFieldValuesKey = new object();

		private static readonly object FilterConditionChangedKey = new object();

		private MxCommandManager _commandManager;

		private bool _autoApply;

		private bool _sortFieldList = true;

		private bool _automaticHeightResize;

		private bool _filterConditionOperators;

		private ISupportFilterBuilder _sourceControl;

		private bool _showChildTables = true;

		private IMordexTable _table;

		private Timer _mouseReleaseTimer;

		private IMordexCondition _filterCondition;

		private BuiltInTextList _builtInTextList;

		private AvailableLogicalOperators _availableLogicalOperators;

		private AvailableConditionOperators _availableConditionOperators;

		private ConditionOperator _defaultConditionOperator;

		private bool _allowNegateRootCondition;

		private int _editControlWidth;

		private bool _allowFilterByFieldValue;

		private bool _isDisposing;

		int IFilterBuilderElement.RowHeight => RowHeight;

		BuiltInTextList IFilterBuilderElement.BuiltInTextList => BuiltInTextList;

		MxFilterBuilder IFilterBuilderElement.FilterBuilder => this;

		IMordexCondition IFilterBuilderElement.FilterCondition => _filterCondition;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Control ChildControl
		{
			get
			{
				return base.ChildControl;
			}
			set
			{
				base.ChildControl = value;
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the FilterCondition will be automaically applied to the SourceControl when it changes")]
		public bool AutoApply
		{
			get
			{
				return _autoApply;
			}
			set
			{
				_autoApply = value;
			}
		}

		[Description("Specifies the minimum size that the control should have before displaying the scollbar.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size MinSize
		{
			get
			{
				return base.MinSize;
			}
			set
			{
				if (base.MinSize != value)
				{
					base.MinSize = value;
					ApplyAutomaticHeight();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifies whether the field list should be alphabetically sorted.")]
		public bool SortFieldList
		{
			get
			{
				return _sortFieldList;
			}
			set
			{
				if (_sortFieldList != value)
				{
					_sortFieldList = value;
					ResetFieldMenu(reloadConditions: true);
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the height of the control automatically adjusts when the FilterCondition assigned to the control is changed.")]
		public bool AutomaticHeightResize
		{
			get
			{
				return _automaticHeightResize;
			}
			set
			{
				if (_automaticHeightResize != value)
				{
					_automaticHeightResize = value;
					if (value)
					{
						ApplyAutomaticHeight();
					}
				}
			}
		}

		[DefaultValue(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("The value of this property has no effect. Use the property AvailableConditionOperators instead.")]
		[Browsable(false)]
		public bool FilterConditionOperators
		{
			get
			{
				return _filterConditionOperators;
			}
			set
			{
				if (_filterConditionOperators != value)
				{
					_filterConditionOperators = value;
					AvailableConditionOperators = _filterConditionOperators ? AvailableConditionOperators.Default : AvailableConditionOperators.All;
				}
			}
		}

		[Description("Represents the associated control from where the FilterBuilder will fill its Fields and FilterCondition properties. Available options for this property are the DataGrid and Timeline controls.")]
		[DefaultValue(null)]
		public ISupportFilterBuilder SourceControl
		{
			get
			{
				return _sourceControl;
			}
			set
			{
				if (_sourceControl != value)
				{
					if (_sourceControl != null)
					{
						_sourceControl.RootTableChanged -= OnSourceControlRootTableChanged;
					}
					_sourceControl = value;
					if (_sourceControl != null)
					{
						_sourceControl.RootTableChanged += OnSourceControlRootTableChanged;
						Table = _sourceControl.RootTable;
					}
					else
					{
						Table = null;
					}
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifies whether the user can choose a field from a child table to filter the SourceControl")]
		public bool ShowChildTables
		{
			get
			{
				return _showChildTables;
			}
			set
			{
				if (_showChildTables != value)
				{
					_showChildTables = value;
					ResetFieldMenu(reloadConditions: true);
				}
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[Description("Represents the Table of the DataGrid control that is going to be filtered")]
		public IMordexTable Table
		{
			get
			{
				return _table;
			}
			set
			{
				if (_table == value)
				{
					return;
				}
				if (!BelongsToSourceControl(value))
				{
					throw new ArgumentException("The table must belong to the SourceControl");
				}
				if (_table != null)
				{
					_table.ColumnsChanged -= OnTableColumnsChanged;
					_table.FilterConditionChanged -= OnTableFilterConditionChanged;
				}
				ResetFieldMenu(reloadConditions: false);
				FilterCondition = null;
				_table = value;
				if (_table == null)
				{
					return;
				}
				_table.ColumnsChanged += OnTableColumnsChanged;
				_table.FilterConditionChanged += OnTableFilterConditionChanged;
				IMordexCondition tableCondition = _table.FilterCondition != null ? CloneTableCondition() : null;
				if (tableCondition != null)
				{
					FilterCondition = tableCondition;
				}
				else
				{
					RebuildRootPanel();
				}
			}
		}

		[Description("Represents the filter condition that should be applied to the SourceControl.")]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IMordexCondition FilterCondition
		{
			get
			{
				return CloneCondition(_filterCondition);
			}
			set
			{
				if (_filterCondition != value)
				{
					_filterCondition = value;
					if (!_isDisposing)
					{
						RebuildRootPanel();
					}
				}
			}
		}

		[Editor(typeof(BuiltInTextListEditor), typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public BuiltInTextList BuiltInTextList => _builtInTextList;

		[Localizable(true)]
		[Browsable(false)]
		public string BuiltInTextsData
		{
			get
			{
				return BuiltInTextList.ToXml();
			}
			set
			{
				if (BuiltInTextList.ToXml() != value)
				{
					BuiltInTextList.LoadFromXml(value);
					OnBuiltInTextsReplaced();
				}
			}
		}

		[Description("Indicates which logical operators are available in the FilterBuilder.")]
		[DefaultValue(AvailableLogicalOperators.And | AvailableLogicalOperators.AndNot | AvailableLogicalOperators.Or | AvailableLogicalOperators.OrNot)]
		public AvailableLogicalOperators AvailableLogicalOperators
		{
			get
			{
				return _availableLogicalOperators;
			}
			set
			{
				if (_availableLogicalOperators != value)
				{
					_availableLogicalOperators = value;
					_rootPanel?.RefreshLogicalOperatorMenus();
				}
			}
		}

		[DefaultValue(AvailableConditionOperators.Default)]
		[Description("Indicates which condition operators are available in the FilterBuilder.")]
		public AvailableConditionOperators AvailableConditionOperators
		{
			get
			{
				return _availableConditionOperators;
			}
			set
			{
				if (_availableConditionOperators != value)
				{
					_availableConditionOperators = value;
					_rootPanel?.RefreshOperatorMenus();
				}
			}
		}

		[Description("Specifies the default condition operator for a new FilterCondition.")]
		[DefaultValue(ConditionOperator.Equal)]
		public ConditionOperator DefaultConditionOperator
		{
			get
			{
				return _defaultConditionOperator;
			}
			set
			{
				_defaultConditionOperator = value;
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether a button will be pressent in the FilterBuilder to indicate that the whole condition should be negated")]
		public bool AllowNegateRootCondition
		{
			get
			{
				return _allowNegateRootCondition;
			}
			set
			{
				if (_allowNegateRootCondition != value)
				{
					_allowNegateRootCondition = value;
					RebuildRootPanel();
					RefreshConditionLayout();
				}
			}
		}

		[DefaultValue(DefaultEditControlWidth)]
		[Description("Specifies the width of the control used to edit the value of the FilterCondition.")]
		public int EditControlWidth
		{
			get
			{
				return _editControlWidth;
			}
			set
			{
				if (_editControlWidth != value)
				{
					_editControlWidth = value;
					RefreshConditionLayout();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the user can choose another field to compare the value in the FilterCondition")]
		public bool AllowFilterByFieldValue
		{
			get
			{
				return _allowFilterByFieldValue;
			}
			set
			{
				if (_allowFilterByFieldValue != value)
				{
					_allowFilterByFieldValue = value;
					RefreshConditionLayout();
				}
			}
		}

		[Description("Event fired when a new field was selected for the FilterCondition. Use this event to populate a ValueList with a list of possible values.")]
		public event FetchFieldValuesEventHandler FetchFieldValues
		{
			add
			{
				base.Events.AddHandler(FetchFieldValuesKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(FetchFieldValuesKey, value);
			}
		}

		[Description("Event fired when the FilterCondition property is changed.")]
		public event EventHandler FilterConditionChanged
		{
			add
			{
				base.Events.AddHandler(FilterConditionChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(FilterConditionChangedKey, value);
			}
		}

		internal bool HasFilterCondition => _filterCondition != null;

		internal bool IsInDesignMode => base.DesignMode;

		internal MxCommandManager CommandManager => _commandManager;

		public MxFilterBuilder()
		{
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			BackColor = Color.Transparent;
			InitializeComponents();
		}

		void IFilterBuilderElement.OnChildConditionAdded(IMordexCondition condition)
		{
		}

		void IFilterBuilderElement.OnChildConditionRemoved(ConditionPanel panel)
		{
			if (_rootPanel == panel)
			{
				FilterCondition = null;
			}
			((IFilterBuilderElement)this).OnConditionChanged(_filterCondition);
		}

		void IFilterBuilderElement.OnConditionChanged(IMordexCondition condition)
		{
			if (_autoApply)
			{
				ApplyFilter();
			}
			OnFilterConditionChanged(EventArgs.Empty);
		}

		void IFilterBuilderElement.InvalidateConditions()
		{
			if (_autoApply)
			{
				ApplyFilter();
			}
			OnFilterConditionChanged(EventArgs.Empty);
			RefreshConditionLayout();
		}

		private static IMordexCondition CloneCondition(IMordexCondition condition)
		{
			return condition == null ? null : ((ICloneable)(MordexConditionBase)condition).Clone() as IMordexCondition;
		}

		private IMordexCondition CloneTableCondition()
		{
			if (_table == null || _table.FilterCondition == null)
			{
				return null;
			}
			return CloneCondition(_table.FilterCondition.FilterCondition as MordexConditionBase);
		}

		private bool BelongsToSourceControl(IMordexTable table)
		{
			if (table == null || _table == null || _sourceControl == null)
			{
				return true;
			}
			IMordexTable root = _sourceControl.RootTable;
			if (root == null)
			{
				return true;
			}
			for (IMordexTable current = table; current != null; current = current.ParentTable)
			{
				if (current == root)
				{
					return true;
				}
			}
			return false;
		}

		public IMordexCondition GetCurrentCondition()
		{
			if (_rootPanel != null && _rootPanel.ContainsFocus)
			{
				return _rootPanel.FindFocusedCondition();
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBuiltInTextsData()
		{
			return BuiltInTextList.HasCustomTexts();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBuiltInTextsData()
		{
			BuiltInTextList.Reset();
		}

		public void Reload()
		{
			RebuildRootPanel();
		}

		// Applies the condition to the table without echoing the change back into the builder.
		public void ApplyFilter()
		{
			if (_table == null)
			{
				return;
			}
			_table.FilterConditionChanged -= OnTableFilterConditionChanged;
			try
			{
				_table.FilterCondition = _filterCondition != null ? (MordexConditionBase)FilterCondition : null;
			}
			finally
			{
				_table.FilterConditionChanged += OnTableFilterConditionChanged;
			}
		}

		protected virtual void OnFetchFieldValues(FetchFieldValuesEventArgs e)
		{
			if (base.Events[FetchFieldValuesKey] is FetchFieldValuesEventHandler handler)
			{
				handler(this, e);
			}
		}

		protected virtual void OnFilterConditionChanged(EventArgs e)
		{
			if (base.Events[FilterConditionChangedKey] is EventHandler handler)
			{
				handler(this, e);
			}
		}

		internal FetchFieldValuesEventArgs RaiseFetchFieldValues(IMordexCondition condition)
		{
			FetchFieldValuesEventArgs e = new FetchFieldValuesEventArgs(condition);
			OnFetchFieldValues(e);
			return e;
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			SetRowHeight(Font.Height + RowPaddingAfterFontChange);
			Reload();
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			RefreshConditionLayout();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			ShowAddConditionMenu(e.Button);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			ApplyThemeColors();
		}

		private void OnTransparentTableLayoutPanelMouseUp(object sender, MouseEventArgs eventArgs)
		{
			ShowAddConditionMenu(eventArgs.Button);
		}

		private void ShowAddConditionMenu(MouseButtons button)
		{
			if (button == MouseButtons.Right && _filterCondition == null && Table != null)
			{
				AddConditionMenu.Show(this);
			}
		}

		private void ApplyAutomaticHeight()
		{
			if (_automaticHeightResize)
			{
				base.Height = Math.Max(MinSize.Height, RowHeight);
			}
		}

		private void OnSourceControlRootTableChanged(object sender, EventArgs eventArgs)
		{
			IMordexTable root = _sourceControl.RootTable;
			if (_table == root)
			{
				return;
			}
			if (_table == null || _table.ParentTable == null || _table.ParentTable != root)
			{
				Table = root;
			}
		}

		private void OnTableColumnsChanged(object sender, EventArgs eventArgs)
		{
			ResetFieldMenu(reloadConditions: true);
		}

		private void OnTableFilterConditionChanged(object sender, EventArgs eventArgs)
		{
			if (!AutoApply)
			{
				return;
			}
			if (base.ContainsFocus && Control.MouseButtons != MouseButtons.None)
			{
				StartMouseReleaseTimer();
			}
			else if (_table != null && _table.FilterCondition != null)
			{
				IMordexCondition tableCondition = CloneTableCondition();
				if (tableCondition != null)
				{
					FilterCondition = tableCondition;
				}
			}
			else
			{
				FilterCondition = null;
			}
		}

		// Reloading while the mouse button is down would destroy the control that is being clicked, so wait for the release.
		private void StartMouseReleaseTimer()
		{
			if (_mouseReleaseTimer == null)
			{
				_mouseReleaseTimer = new Timer();
				_mouseReleaseTimer.Tick += OnMouseReleaseTimerTick;
			}
			_mouseReleaseTimer.Interval = MouseReleasePollInterval;
			_mouseReleaseTimer.Start();
		}

		private void StopMouseReleaseTimer()
		{
			if (_mouseReleaseTimer == null)
			{
				return;
			}
			Timer timer = _mouseReleaseTimer;
			_mouseReleaseTimer = null;
			timer.Stop();
			timer.Tick -= OnMouseReleaseTimerTick;
			timer.Dispose();
		}

		private void OnMouseReleaseTimerTick(object sender, EventArgs eventArgs)
		{
			if (Control.MouseButtons == MouseButtons.None)
			{
				StopMouseReleaseTimer();
				OnTableFilterConditionChanged(null, EventArgs.Empty);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_isDisposing = true;
				StopMouseReleaseTimer();
				StopListeningToThemes();
				SourceControl = null;
				if (_table != null)
				{
					_table.ColumnsChanged -= OnTableColumnsChanged;
					_table.FilterConditionChanged -= OnTableFilterConditionChanged;
					_table = null;
				}
				if (_hostPanel != null)
				{
					_hostPanel.MouseUp -= OnTransparentTableLayoutPanelMouseUp;
				}
				if (_negateButton != null)
				{
					_negateButton.Click -= OnNegateButtonClick;
				}
			}
			base.Dispose(disposing);
			if (disposing)
			{
				DisposeMenus();
				_commandManager?.Dispose();
				_commandManager = null;
			}
		}
	}
}