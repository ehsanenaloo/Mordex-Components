using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(false)]
	public partial class MaskEdit : TextBox
	{
		private const int ClipboardRetryCount = 5;

		private const int ClipboardRetryDelay = 100;

		private struct MaskEditData
		{
			internal bool _isTextValid;

			internal string _rawText;

			internal bool _allowsLetter;

			internal bool _allowsDigit;

			internal bool _allowsSpace;

			internal bool _allowsSign;

			internal bool _allowsAnyChar;

			internal bool _isLiteral;
		}

		internal class MaskEditAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.Text;

			public override AccessibleObject Parent => base.Owner.Parent.AccessibilityObject;

			public override string Value
			{
				get
				{
					return ((MaskEdit)base.Owner).Text;
				}
				set
				{
					((MaskEdit)base.Owner).Text = value;
				}
			}

			internal MaskEditAccessibleObject(MaskEdit owner)
				: base(owner)
			{
			}
		}

		internal static MaskEdit _maskEdit;

		private bool _numeric;

		internal bool _isFirstEnter;

		private string _mask = "";

		internal bool _handlesReturnAndEscape = default;

		private bool _isTextChangeEnabled = true;

		private bool _isMaskDataSynced = true;

		private MaskEditData[] _maskSlots = new MaskEditData[0];

		private char _promptChar = '_';

		private bool _includeLiterals = true;

		private bool _hasMask;

		private bool _isInternalTextChange;

		internal bool _isDropHideAllowed = true;

		internal bool _isDropDownShown;

		private bool _isHandlingTextChange;

		internal bool _isTypingForward;

		internal bool _isBackspacePressed;

		private int _lastTextLength;

		private EventHandler _keyPressedHandler;

		private EventHandler _escapeKeyHandler;

		public MaskEdit()
		{
			base.AcceptsTab = false;
			_isFirstEnter = true;
		}

		internal MaskEdit(bool checkLicense)
		{
			_isFirstEnter = true;
		}
	}
}
