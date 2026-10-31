using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	[DefaultEvent("ValueChanged")]
	public class ComboBase : EditBase
	{
		private GridMaskEdit _defaultEditControl;

		private bool _droppedDown;

		private ComboStyle _comboStyle;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new ButtonStyle ButtonStyle
		{
			get
			{
				return ButtonStyle.DownArrow;
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new Font ButtonFont
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Image ButtonImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override int ButtonImageIndex
		{
			get
			{
				return -1;
			}
			set
			{
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string ButtonImageKey
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new Size ButtonImageSize
		{
			get
			{
				return new Size(0, 0);
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new string ButtonText
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		protected override TextBox DefaultEditControl
		{
			get
			{
				UpdateDefaultEditControl(new GridMaskEdit());
				GetDefaultEditControl().AccessibleName = "EDIT";
				return _defaultEditControl;
			}
		}

		protected override bool IsDropDownPushed
		{
			get
			{
				if (base.Enabled)
				{
					return DroppedDown;
				}
				return false;
			}
		}

		[DefaultValue(false)]
		[Browsable(false)]
		public bool DroppedDown
		{
			get
			{
				return _droppedDown;
			}
			set
			{
				if (!base.ReadOnly && value != _droppedDown)
				{
					_droppedDown = value;
					if (value)
					{
						OnDropControl(mouseCapture: true);
					}
					else
					{
						OnHideDroppedControl(cancel: true);
					}
				}
			}
		}

		[Description("Specifies the style of the combo box.")]
		[DefaultValue(ComboStyle.Combo)]
		public ComboStyle ComboStyle
		{
			get
			{
				return _comboStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ComboStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _comboStyle)
				{
					_comboStyle = value;
					UpdateComboBase();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("FlatBorderHoverBehavior will be removed in future versions. Use HoverMode property instead.")]
		public FlatBorderHoverBehavior FlatBorderHoverBehavior
		{
			get
			{
				HoverMode hoverMode = base.HoverMode;
				if (hoverMode == HoverMode.Highlight)
				{
					return FlatBorderHoverBehavior.HighLight;
				}
				return FlatBorderHoverBehavior.None;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FlatBorderHoverBehavior), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == FlatBorderHoverBehavior.None)
				{
					base.HoverMode = HoverMode.None;
				}
				else
				{
					base.HoverMode = HoverMode.Highlight;
				}
			}
		}

		public ComboBase()
		{
			base.ButtonType = EditButtonType.SingleButton;
			base.ButtonStyle = EditButtonStyle.DownArrow;
		}

		private GridMaskEdit GetDefaultEditControl()
		{
			return _defaultEditControl;
		}

		private void UpdateDefaultEditControl(GridMaskEdit defaultEditControl)
		{
			if (_defaultEditControl != defaultEditControl)
			{
				if (_defaultEditControl != null)
				{
					_defaultEditControl.RemoveKeyPressedHandler(HandleKeyPressed);
					_defaultEditControl.MouseWheel -= OnDefaultEditControlMouseWheel;
				}
				_defaultEditControl = defaultEditControl;
				if (_defaultEditControl != null)
				{
					_defaultEditControl.AddKeyPressedHandler(HandleKeyPressed);
					_defaultEditControl.MouseWheel += OnDefaultEditControlMouseWheel;
				}
			}
		}

		private void OnDefaultEditControlMouseWheel(object sender, MouseEventArgs eventArgs)
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			HandledMouseEventArgs e = new HandledMouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
			OnMouseWheel(e);
			((HandledMouseEventArgs)eventArgs).Handled = e.Handled;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if ((keyData == Keys.F4 || keyData == (Keys.Down | Keys.Alt)) && base.Visible && !base.ReadOnly)
			{
				if (!DroppedDown)
				{
					OnDropControl(mouseCapture: true);
					return true;
				}
				DroppedDown = false;
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnDropControl(bool mouseCapture)
		{
			if (base.EditControl is MaskEdit)
			{
				((MaskEdit)base.EditControl)._isDropDownShown = true;
			}
			_droppedDown = true;
			_isDropDownOpen = true;
			Invalidate();
		}

		protected virtual void OnHideDroppedControl(bool cancel)
		{
			_droppedDown = false;
			_isDropDownOpen = false;
			if (base.EditControl is MaskEdit)
			{
				((MaskEdit)base.EditControl)._isDropDownShown = false;
			}
			Invalidate();
		}

		protected override void OnButtonMouseDown(EditButton editButton)
		{
			base.OnButtonMouseDown(editButton);
			if (!base.ReadOnly)
			{
				OnDropControl(mouseCapture: false);
			}
		}

		internal override void DoButtonClick(int index)
		{
			if (!base.ReadOnly)
			{
				OnDropControl(mouseCapture: false);
			}
		}

		private void UpdateComboBase()
		{
			if (_comboStyle == ComboStyle.Combo)
			{
				if (GetDefaultEditControl() == null)
				{
					UpdateDefaultEditControl(new GridMaskEdit());
				}
				base.TreatControlAreaAsButton = false;
			}
			else
			{
				if (GetDefaultEditControl() != null)
				{
					UpdateDefaultEditControl(null);
				}
				base.TreatControlAreaAsButton = true;
			}
			base.EditControl = GetDefaultEditControl();
		}

		private void HandleKeyPressed(object sender, EventArgs eventArgs)
		{
			OnKeyPressed();
		}

		protected internal virtual void OnKeyPressed()
		{
		}

		protected override void OnLeave(EventArgs e)
		{
			DroppedDown = false;
			base.OnLeave(e);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal virtual void OnDoMouseWheel(int delta)
		{
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			HandledMouseEventArgs handledMouseEventArgs = (HandledMouseEventArgs)e;
			if (!handledMouseEventArgs.Handled)
			{
				OnDoMouseWheel(handledMouseEventArgs.Delta);
				handledMouseEventArgs.Handled = true;
			}
		}
	}
}
