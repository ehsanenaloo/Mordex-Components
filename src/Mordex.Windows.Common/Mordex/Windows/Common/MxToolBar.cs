using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	public enum MxToolBarAppearance
	{
		Normal,
		Flat
	}

	public enum MxToolBarTextAlign
	{
		Underneath,
		Right
	}

	public enum MxToolBarButtonStyle
	{
		PushButton,
		ToggleButton,
		Separator,
		DropDownButton
	}

	public class MxToolBarButtonClickEventArgs : EventArgs
	{
		public MxToolBarButton Button { get; }

		public MxToolBarButtonClickEventArgs(MxToolBarButton button)
		{
			Button = button;
		}
	}

	public delegate void MxToolBarButtonClickEventHandler(object sender, MxToolBarButtonClickEventArgs e);

	public class MxToolBarButton : ToolStripButton
	{
		private MxToolBarButtonStyle _style;

		public MxToolBarButton()
		{
			DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
		}

		public MxToolBarButton(string text)
			: this()
		{
			Text = text;
		}

		[DefaultValue(MxToolBarButtonStyle.PushButton)]
		public MxToolBarButtonStyle Style
		{
			get { return _style; }
			set
			{
				_style = value;
				CheckOnClick = value == MxToolBarButtonStyle.ToggleButton;
				if (value == MxToolBarButtonStyle.Separator)
				{
					AutoSize = false;
					Width = 8;
					Enabled = false;
				}
				Invalidate();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Pushed
		{
			get { return Checked; }
			set { Checked = value; }
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_style == MxToolBarButtonStyle.Separator)
			{
				int x = Width / 2;
				Pen pen = GdiCache.Pen(SystemColors.ControlDark);
				e.Graphics.DrawLine(pen, x, 3, x, Height - 4);
				return;
			}
			base.OnPaint(e);
		}
	}

	public class MxToolBar : ToolStrip
	{
		public sealed class ButtonCollection : IEnumerable
		{
			private readonly MxToolBar _owner;

			internal ButtonCollection(MxToolBar owner)
			{
				_owner = owner;
			}

			public int Count => _owner.Items.Count;

			public MxToolBarButton this[int index] => (MxToolBarButton)_owner.Items[index];

			public int IndexOf(MxToolBarButton button)
			{
				return _owner.Items.IndexOf(button);
			}

			public int Add(MxToolBarButton button)
			{
				_owner.Items.Add(button);
				return _owner.Items.Count - 1;
			}

			public void AddRange(MxToolBarButton[] buttons)
			{
				_owner.Items.AddRange(buttons);
			}

			public void Clear()
			{
				_owner.Items.Clear();
			}

			public IEnumerator GetEnumerator()
			{
				return _owner.Items.GetEnumerator();
			}
		}

		private readonly ButtonCollection _buttons;

		private MxToolBarTextAlign _textAlign;

		private TextImageRelation _textImageRelation = TextImageRelation.ImageAboveText;

		public MxToolBar()
		{
			_buttons = new ButtonCollection(this);
			GripStyle = ToolStripGripStyle.Hidden;
			ItemClicked += OnToolStripItemClicked;
		}

		public event MxToolBarButtonClickEventHandler ButtonClick;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ButtonCollection Buttons => _buttons;

		[DefaultValue(MxToolBarAppearance.Normal)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MxToolBarAppearance Appearance { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Divider { get; set; } = true;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DropDownArrows { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Wrappable { get; set; } = true;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size ButtonSize { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ShowToolTips
		{
			get { return ShowItemToolTips; }
			set { ShowItemToolTips = value; }
		}

		[DefaultValue(MxToolBarTextAlign.Underneath)]
		public MxToolBarTextAlign TextAlign
		{
			get { return _textAlign; }
			set
			{
				_textAlign = value;
				_textImageRelation = value == MxToolBarTextAlign.Right ? TextImageRelation.ImageBeforeText : TextImageRelation.ImageAboveText;
				foreach (ToolStripItem item in Items)
				{
					item.TextImageRelation = _textImageRelation;
				}
			}
		}

		protected override void OnItemAdded(ToolStripItemEventArgs e)
		{
			base.OnItemAdded(e);
			e.Item.TextImageRelation = _textImageRelation;
		}

		private void OnToolStripItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			MxToolBarButton button = e.ClickedItem as MxToolBarButton;
			if (button != null && button.Style != MxToolBarButtonStyle.Separator && ButtonClick != null)
			{
				ButtonClick(this, new MxToolBarButtonClickEventArgs(button));
			}
		}
	}
}
