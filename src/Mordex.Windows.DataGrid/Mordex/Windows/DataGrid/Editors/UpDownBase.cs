using System;
using Mordex.Windows.DataGrid.Internal;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	public class UpDownBase : EditBase
	{
		private bool _showUpDownButtons = true;

		private UpDownStyle _upDownStyle;

		private bool _suppressTextChange;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
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

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[Description("Specifies the style of the updown control.")]
		[DefaultValue(UpDownStyle.UpDownCombo)]
		public UpDownStyle UpDownStyle
		{
			get
			{
				return _upDownStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(UpDownStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _upDownStyle)
				{
					_upDownStyle = value;
					OnUpDownStyleChanged();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
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

		public UpDownBase()
		{
			base.ButtonType = EditButtonType.UpDownButtons;
		}

		internal void UpdateUpDownBaseBoolean(bool upDownBaseBoolean)
		{
			if (_showUpDownButtons != upDownBaseBoolean)
			{
				_showUpDownButtons = upDownBaseBoolean;
				if (_showUpDownButtons)
				{
					base.ButtonType = EditButtonType.UpDownButtons;
					return;
				}
				base.ButtonStyle = EditButtonStyle.NoButton;
				base.ButtonType = EditButtonType.SingleButton;
			}
		}

		protected override void OnButtonClick(EditButton editButton)
		{
			switch (editButton)
			{
			case EditButton.DownButton:
				DownButton();
				break;
			case EditButton.UpButton:
				UpButton();
				break;
			}
		}

		protected virtual void UpButton()
		{
		}

		protected virtual void DownButton()
		{
		}

		protected virtual void OnUpDownStyleChanged()
		{
		}

		internal void SetSuppressTextChange(bool suppressTextChange)
		{
			_suppressTextChange = suppressTextChange;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (!base.ReadOnly)
			{
				switch (keyData)
				{
				case Keys.Up:
					UpButton();
					return true;
				case Keys.Down:
					DownButton();
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		internal bool IsMouseWheelEnabled()
		{
			return !base.ReadOnly;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
		}

		private bool TryStepByWheelDelta(int value)
		{
			if (!base.ReadOnly)
			{
				if (value < 0)
				{
					DownButton();
				}
				else if (value > 0)
				{
					UpButton();
				}
				return true;
			}
			return false;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WindowMessages.MouseWheel)
			{
				int wParam = 0;
				wParam = (int)((IntPtr.Size != 8) ? ((int)m.WParam) : ((long)m.WParam << 32 >> 32));
				wParam = (wParam >> 16) / 120;
				if (TryStepByWheelDelta(wParam))
				{
					return;
				}
			}
			base.WndProc(ref m);
		}
	}
}
