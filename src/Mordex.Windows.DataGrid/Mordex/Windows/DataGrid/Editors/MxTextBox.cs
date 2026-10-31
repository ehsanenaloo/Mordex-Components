using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Accessibility;

namespace Mordex.Windows.DataGrid.Editors
{
	[DefaultBindingProperty("Text")]
	[DefaultEvent("TextChanged")]
	[ToolboxItem(true)]
	public class MxTextBox : EditBase
	{
		internal class TextBoxAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.Text;

			public override string Value
			{
				get
				{
					return GetOwnerTextBox().Text;
				}
				set
				{
					GetOwnerTextBox().Text = value;
				}
			}

			internal TextBoxAccessibleObject(MxTextBox ctl)
				: base(ctl)
			{
			}

			private MxTextBox GetOwnerTextBox()
			{
				return (MxTextBox)base.Owner;
			}

			public override int GetChildCount()
			{
				MxTextBox editBox = GetOwnerTextBox();
				int childCount = 0;
				if (editBox.EditControl != null)
				{
					childCount++;
				}
				if (editBox.ButtonStyle != EditButtonStyle.NoButton)
				{
					childCount++;
				}
				return childCount;
			}

			public override AccessibleObject GetChild(int index)
			{
				MxTextBox editBox = GetOwnerTextBox();
				if (editBox.TextBox != null)
				{
					if (index == 0)
					{
						return editBox.TextBox.AccessibilityObject;
					}
					index--;
				}
				if (editBox.ButtonStyle != EditButtonStyle.NoButton && index == 0)
				{
					return editBox.ComputeAccessibleObject();
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		private AccessibleObject _customAccessibleObject;

		private AccessibleObject _accessibleObjectCache;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public override bool HasText
		{
			get
			{
				return base.HasText;
			}
			set
			{
				base.HasText = value;
			}
		}

		protected override TextBox DefaultEditControl => new EditBaseTextBox(this);

		[Description("Indicates whether pressing ENTER in a multiline EditControl creates a new line of text in the control or activates the default button for the form.")]
		[DefaultValue(false)]
		public bool AcceptsReturn
		{
			get
			{
				return base.TextBox.AcceptsReturn;
			}
			set
			{
				base.TextBox.AcceptsReturn = value;
			}
		}

		[Description("Indicates whether pressing the TAB key in a multiline edit box control types a TAB character in the control instead of moving the focus to the next control in the tab order.")]
		[DefaultValue(false)]
		public bool AcceptsTab
		{
			get
			{
				return base.TextBox.AcceptsTab;
			}
			set
			{
				base.TextBox.AcceptsTab = value;
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether this is a multiline text box control.")]
		public bool Multiline
		{
			get
			{
				return base.TextBox.Multiline;
			}
			set
			{
				if (base.TextBox.Multiline != value)
				{
					base.TextBox.Multiline = value;
					RefreshLayout();
				}
			}
		}

		[Description("Indicates whether a multiline edit box control automatically wraps words to the beginning of the next line when necessary.")]
		[DefaultValue(true)]
		public bool WordWrap
		{
			get
			{
				return base.TextBox.WordWrap;
			}
			set
			{
				base.TextBox.WordWrap = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Represents the lines of text in a text box control.")]
		public string[] Lines
		{
			get
			{
				return base.TextBox.Lines;
			}
			set
			{
				base.TextBox.Lines = value;
			}
		}

		[DefaultValue('\0')]
		[Description("Represents the character used to mask characters of a password in a single-line edit box control.")]
		public char PasswordChar
		{
			get
			{
				return base.TextBox.PasswordChar;
			}
			set
			{
				base.TextBox.PasswordChar = value;
			}
		}

		[Description("Specifies which scroll bars should appear in a multiline TextBox control.")]
		[DefaultValue(System.Windows.Forms.ScrollBars.None)]
		public System.Windows.Forms.ScrollBars ScrollBars
		{
			get
			{
				return base.TextBox.ScrollBars;
			}
			set
			{
				base.TextBox.ScrollBars = value;
			}
		}

		[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design", typeof(UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public AutoCompleteStringCollection AutoCompleteCustomSource
		{
			get
			{
				return base.TextBox.AutoCompleteCustomSource;
			}
			set
			{
				base.TextBox.AutoCompleteCustomSource = value;
			}
		}

		[DefaultValue(AutoCompleteMode.None)]
		public AutoCompleteMode AutoCompleteMode
		{
			get
			{
				return base.TextBox.AutoCompleteMode;
			}
			set
			{
				base.TextBox.AutoCompleteMode = value;
			}
		}

		[DefaultValue(AutoCompleteSource.None)]
		public AutoCompleteSource AutoCompleteSource
		{
			get
			{
				return base.TextBox.AutoCompleteSource;
			}
			set
			{
				base.TextBox.AutoCompleteSource = value;
			}
		}

		protected override AccessibleObject AccessibleObjectCore
		{
			get
			{
				if (_customAccessibleObject == null)
				{
					_customAccessibleObject = new TextBoxAccessibleObject(this);
				}
				return _customAccessibleObject;
			}
		}

		public MxTextBox()
		{
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return AccessibleObjectCore;
		}

		internal AccessibleObject ComputeAccessibleObject()
		{
			if (_accessibleObjectCache == null)
			{
				_accessibleObjectCache = new EditBaseAccessibleObject(this);
			}
			return _accessibleObjectCache;
		}
	}
}
