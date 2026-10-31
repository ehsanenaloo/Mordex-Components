using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>A control that sets its own parts when a themer switches a tree of controls between the classic look and a theme.</summary>
	public interface IThemeAware
	{
		void ApplyLook(bool themed);
	}

	/// <summary>
	/// Switches a tree of controls between its own look and the look of a theme. A suite control with a
	/// <c>UseDefaultTheme</c> property gets it set; the ordinary Windows Forms controls get the theme colors, the native
	/// look of Windows for their scroll bars, and an owner-drawn header for list views. Each property is saved before the
	/// themer first changes it and is given back when the theme is turned off; a property the themer never changed is left
	/// alone. <see cref="Attach"/> does this for a form and keeps doing it while the form is open.
	/// </summary>
	public static partial class ControlThemer
	{
		// What a control looked like before the themer changed it. Each group is saved when the themer first changes it and
		// is cleared when it is given back, so a change the developer made in between is saved again the next time.
		private sealed class Original
		{
			public object UseDefaultTheme;
			public bool ColorsSet;
			// Empty when the control took the color from its parent.
			public Color BackColor;
			public Color ForeColor;
			// Null when the button did not set it and follows its back color.
			public bool? UseVisualStyleBackColor;
			public bool OwnerDraw;
			public bool OwnerDrawSet;
			public bool ButtonSet;
			public FlatStyle FlatStyle;
			public int BorderSize;
			public Color BorderColor;
			public Color MouseOverBackColor;
			public Color MouseDownBackColor;
			public Color[] GridColors;
			public bool FontApplied;
		}

		private static readonly ConditionalWeakTable<Control, Original> Originals = new ConditionalWeakTable<Control, Original>();

		/// <summary>True when a theme should change the ordinary controls: every theme except the plain Standard one.</summary>
		public static bool IsThemed(MxTheme theme)
		{
			return theme != null && !string.Equals(theme.Family, "Standard", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Applies the default theme to the form and its controls now, and again after every change of the default theme and
		/// after a control is added, until the form is disposed. Safe to call more than once for a form.
		/// </summary>
		public static void Attach(Form form)
		{
			if (form == null)
			{
				throw new ArgumentNullException(nameof(form));
			}
			if (Followers.TryGetValue(form, out _))
			{
				return;
			}
			Followers.Add(form, new Follower(form));
		}

		private static readonly ConditionalWeakTable<Form, Follower> Followers = new ConditionalWeakTable<Form, Follower>();

		/// <summary>Applies a theme to a control and the controls inside it. A form given here is not colored itself, only its children.</summary>
		public static void Apply(Control root, MxTheme theme, bool themed, bool dialog = false)
		{
			if (root == null)
			{
				return;
			}
			ApplyFont(root, theme, themed);
			if (!(root is Form))
			{
				ApplyOne(root, theme, themed, false, dialog);
			}
			ApplyTabPageBack(root, theme, themed);
			foreach (Control child in root.Controls)
			{
				Apply(child, theme, themed, dialog);
			}
		}

		/// <summary>Applies a theme to a form, its title bar and everything in it.</summary>
		public static void ApplyToForm(Form form, MxTheme theme, bool themed)
		{
			ApplyFont(form, theme, themed);
			ApplyOne(form, theme, themed, true, true);
			Apply(form, theme, themed, true);
			if (form.IsHandleCreated)
			{
				ThemeWindow.Apply(form, themed ? theme : MxThemeManager.Find("Standard") ?? theme);
			}
		}

		// A theme with a font gives it to a control whose font the developer did not set. A child that inherits the font of
		// its parent needs nothing. Resetting the property brings the first font back.
		private static void ApplyFont(Control control, MxTheme theme, bool themed)
		{
			Original original = Originals.GetValue(control, key => new Original());
			ThemeFont? wanted = themed && theme != null ? theme.Style.Detail.Font : null;
			if (wanted == null)
			{
				if (original.FontApplied)
				{
					original.FontApplied = false;
					System.ComponentModel.TypeDescriptor.GetProperties(control)["Font"]?.ResetValue(control);
				}
				return;
			}
			Font target = ThemeFonts.Get(wanted.Value);
			if (!original.FontApplied)
			{
				System.ComponentModel.PropertyDescriptor property = System.ComponentModel.TypeDescriptor.GetProperties(control)["Font"];
				if (property == null || property.ShouldSerializeValue(control))
				{
					return;
				}
				if (control.Parent != null && ReferenceEquals(control.Parent.Font, target))
				{
					return;
				}
				original.FontApplied = true;
			}
			if (!ReferenceEquals(control.Font, target))
			{
				control.Font = target;
			}
		}

		private sealed class Follower
		{
			private readonly Form _form;
			private readonly ConditionalWeakTable<Control, object> _hooked = new ConditionalWeakTable<Control, object>();

			public Follower(Form form)
			{
				_form = form;
				MxThemeManager.DefaultThemeChanged += OnThemeChanged;
				form.Disposed += OnDisposed;
				form.HandleCreated += OnHandleCreated;
				Hook(form);
				Refresh();
			}

			private void OnHandleCreated(object sender, EventArgs e)
			{
				Refresh();
			}

			private void Hook(Control control)
			{
				if (_hooked.TryGetValue(control, out _))
				{
					return;
				}
				_hooked.Add(control, null);
				control.ControlAdded += OnControlAdded;
				foreach (Control child in control.Controls)
				{
					Hook(child);
				}
			}

			private void OnControlAdded(object sender, ControlEventArgs e)
			{
				Hook(e.Control);
				MxTheme theme = MxThemeManager.DefaultTheme;
				Apply(e.Control, theme, IsThemed(theme), true);
			}

			private void OnThemeChanged(object sender, EventArgs e)
			{
				if (_form.IsDisposed)
				{
					return;
				}
				if (_form.InvokeRequired)
				{
					if (_form.IsHandleCreated)
					{
						_form.BeginInvoke(new Action(Refresh));
					}
					return;
				}
				Refresh();
			}

			private void Refresh()
			{
				if (_form.IsDisposed)
				{
					return;
				}
				MxTheme theme = MxThemeManager.DefaultTheme;
				ApplyToForm(_form, theme, IsThemed(theme));
				_form.Invalidate(true);
			}

			private void OnDisposed(object sender, EventArgs e)
			{
				MxThemeManager.DefaultThemeChanged -= OnThemeChanged;
				_form.Disposed -= OnDisposed;
				_form.HandleCreated -= OnHandleCreated;
			}
		}

		// A control can hide the property of its base class with another type (the filter builder does), which makes a plain
		// lookup by name ambiguous; the property of the most derived class wins.
		private static PropertyInfo FindProperty(Type type, string name)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				PropertyInfo property = current.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				if (property != null)
				{
					return property;
				}
			}
			return null;
		}

		private static void ApplyOne(Control control, MxTheme theme, bool themed, bool force = false, bool dialog = false)
		{
			if (control is IThemeAware aware)
			{
				aware.ApplyLook(themed);
			}
			Original original = Originals.GetValue(control, key => new Original());
			bool suiteControl = false;
			PropertyInfo useTheme = FindProperty(control.GetType(), "UseDefaultTheme");
			if (useTheme != null && useTheme.CanWrite && useTheme.PropertyType == typeof(bool))
			{
				suiteControl = true;
				if (original.UseDefaultTheme == null)
				{
					original.UseDefaultTheme = useTheme.GetValue(control);
				}
				useTheme.SetValue(control, themed ? true : original.UseDefaultTheme);
			}
			if (suiteControl || (!force && control.GetType().Namespace != "System.Windows.Forms"))
			{
				return;
			}
			// Turning the theme off for a control that never had it changes nothing.
			if (themed || original.ColorsSet)
			{
				ApplyColors(control, original, theme, themed, dialog);
				if (control is TextBoxBase || control is ListBox || control is ListView || control is TreeView || control is ComboBox || control is PropertyGrid || (control is ScrollableControl scrollable && scrollable.AutoScroll))
				{
					ThemeWindow.ApplyNativeLook(control, themed ? NativeLookFor(control, theme) : null);
				}
				if (control is ListView list)
				{
					ApplyListView(list, original, themed);
				}
			}
			bool painted = ApplyControlLook(control, theme, themed);
			ApplyListLook(control, theme, themed);
			if (control is ButtonBase button)
			{
				// The visual style of a push button draws a light frame of its own around the face, which shows as a
				// bright double line on a dark theme; there the button gets the flat face of a dialog button too.
				bool flatDialogButton = themed && (dialog || theme.Kind == ThemeKind.Dark) && !(control is CheckBox) && !(control is RadioButton);
				ApplyButton(button, original, theme, flatDialogButton, painted);
			}
		}

		// A property grid keeps its own colors for its parts; it is saved with the colors of the control.
		private static readonly (Func<PropertyGrid, Color> Get, Action<PropertyGrid, Color> Set, ThemeRole Role)[] GridParts =
		{
			(g => g.ViewBackColor, (g, c) => g.ViewBackColor = c, ThemeRole.InputBack),
			(g => g.ViewForeColor, (g, c) => g.ViewForeColor = c, ThemeRole.Text),
			(g => g.LineColor, (g, c) => g.LineColor = c, ThemeRole.GridLine),
			(g => g.CategoryForeColor, (g, c) => g.CategoryForeColor = c, ThemeRole.TextMuted),
			(g => g.CategorySplitterColor, (g, c) => g.CategorySplitterColor = c, ThemeRole.Border),
			(g => g.HelpBackColor, (g, c) => g.HelpBackColor = c, ThemeRole.Surface),
			(g => g.HelpForeColor, (g, c) => g.HelpForeColor = c, ThemeRole.Text),
			(g => g.CommandsBackColor, (g, c) => g.CommandsBackColor = c, ThemeRole.Surface),
			(g => g.CommandsForeColor, (g, c) => g.CommandsForeColor = c, ThemeRole.Text),
			(g => g.DisabledItemForeColor, (g, c) => g.DisabledItemForeColor = c, ThemeRole.TextDisabled),
		};

		private static void ApplyColors(Control control, Original original, MxTheme theme, bool themed, bool dialog)
		{
			ButtonBase button = control as ButtonBase;
			PropertyGrid grid = control as PropertyGrid;
			if (themed)
			{
				if (!original.ColorsSet)
				{
					original.BackColor = OwnColor(control, "BackColor");
					original.ForeColor = OwnColor(control, "ForeColor");
					if (button != null)
					{
						System.ComponentModel.PropertyDescriptor visualStyle = System.ComponentModel.TypeDescriptor.GetProperties(button)["UseVisualStyleBackColor"];
						original.UseVisualStyleBackColor = visualStyle != null && !visualStyle.ShouldSerializeValue(button) ? (bool?)null : button.UseVisualStyleBackColor;
					}
					if (grid != null)
					{
						original.GridColors = new Color[GridParts.Length];
						for (int i = 0; i < GridParts.Length; i++)
						{
							original.GridColors[i] = GridParts[i].Get(grid);
						}
					}
					original.ColorsSet = true;
				}
				bool input = control is TextBoxBase || control is ComboBox || control is ListBox || control is ListView || control is TreeView || (dialog && control is NumericUpDown);
				control.BackColor = theme.GetColor(input ? ThemeRole.InputBack : ThemeRole.Surface);
				control.ForeColor = theme.GetColor(ThemeRole.Text);
				if (grid != null)
				{
					foreach (var part in GridParts)
					{
						part.Set(grid, theme.GetColor(part.Role));
					}
				}
				return;
			}
			// Setting the back color of a button turns its visual style back color off, so that comes back last.
			if (original.BackColor.IsEmpty)
			{
				control.ResetBackColor();
			}
			else
			{
				control.BackColor = original.BackColor;
			}
			if (original.ForeColor.IsEmpty)
			{
				control.ResetForeColor();
			}
			else
			{
				control.ForeColor = original.ForeColor;
			}
			if (button != null)
			{
				if (original.UseVisualStyleBackColor.HasValue)
				{
					button.UseVisualStyleBackColor = original.UseVisualStyleBackColor.Value;
				}
				else
				{
					System.ComponentModel.TypeDescriptor.GetProperties(button)["UseVisualStyleBackColor"]?.ResetValue(button);
				}
			}
			if (grid != null && original.GridColors != null)
			{
				for (int i = 0; i < GridParts.Length; i++)
				{
					GridParts[i].Set(grid, original.GridColors[i]);
				}
			}
			original.GridColors = null;
			original.ColorsSet = false;
		}

		// The color the control has of its own, or empty when it shows the color of its parent. The parent may already
		// have the theme color, so the inherited value must not be saved.
		private static Color OwnColor(Control control, string name)
		{
			System.ComponentModel.PropertyDescriptor property = System.ComponentModel.TypeDescriptor.GetProperties(control)[name];
			if (property != null && !property.ShouldSerializeValue(control))
			{
				return Color.Empty;
			}
			return name == "BackColor" ? control.BackColor : control.ForeColor;
		}

		// A dialog button gets a flat face in the theme colors; a button that the theme paints itself gets a flat face
		// without a border. Both change the same properties, so they share one saved state.
		private static void ApplyButton(ButtonBase button, Original original, MxTheme theme, bool flatDialogButton, bool painted)
		{
			FlatButtonAppearance appearance = button.FlatAppearance;
			if (!flatDialogButton && !painted)
			{
				if (original.ButtonSet)
				{
					button.FlatStyle = original.FlatStyle;
					appearance.BorderSize = original.BorderSize;
					appearance.BorderColor = original.BorderColor;
					appearance.MouseOverBackColor = original.MouseOverBackColor;
					appearance.MouseDownBackColor = original.MouseDownBackColor;
					original.ButtonSet = false;
				}
				return;
			}
			if (!original.ButtonSet)
			{
				original.FlatStyle = button.FlatStyle;
				original.BorderSize = appearance.BorderSize;
				original.BorderColor = appearance.BorderColor;
				original.MouseOverBackColor = appearance.MouseOverBackColor;
				original.MouseDownBackColor = appearance.MouseDownBackColor;
				original.ButtonSet = true;
			}
			button.FlatStyle = FlatStyle.Flat;
			appearance.BorderSize = painted ? 0 : original.BorderSize;
			appearance.BorderColor = flatDialogButton ? theme.GetColor(ThemeRole.Border) : original.BorderColor;
			appearance.MouseOverBackColor = flatDialogButton ? theme.GetColor(ThemeRole.SurfaceHot) : original.MouseOverBackColor;
			appearance.MouseDownBackColor = flatDialogButton ? theme.GetColor(ThemeRole.SurfacePressed) : original.MouseDownBackColor;
		}

		// The list, the edit box and the buttons inside a property grid are internal controls that the walk does not visit.
		private static void ApplyNativeLookToDescendants(Control parent, MxTheme theme)
		{
			foreach (Control child in parent.Controls)
			{
				ThemeWindow.ApplyNativeLook(child, theme);
				ApplyNativeLookToDescendants(child, theme);
			}
		}

		// The header of a list view is drawn by Windows and has one color, so a themed list draws its header itself.
		private static void ApplyListView(ListView list, Original original, bool themed)
		{
			if (!original.OwnerDrawSet)
			{
				original.OwnerDraw = list.OwnerDraw;
				original.OwnerDrawSet = true;
			}
			list.DrawColumnHeader -= DrawHeader;
			list.DrawItem -= DrawDefaultItem;
			list.DrawSubItem -= DrawDefaultSubItem;
			if (themed)
			{
				list.DrawColumnHeader += DrawHeader;
				list.DrawItem += DrawDefaultItem;
				list.DrawSubItem += DrawDefaultSubItem;
				list.OwnerDraw = true;
			}
			else
			{
				list.OwnerDraw = original.OwnerDraw;
			}
			list.Invalidate(true);
		}

		private static void DrawHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(((Control)sender).DeviceDpi);
			e.Graphics.FillRectangle(theme.Brush(ThemeRole.Header), e.Bounds);
			e.Graphics.DrawLine(theme.Pen(ThemeRole.Border), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
			e.Graphics.DrawLine(theme.Pen(ThemeRole.Border), e.Bounds.Right - 1, e.Bounds.Top + 3, e.Bounds.Right - 1, e.Bounds.Bottom - 4);
			Rectangle text = Rectangle.Inflate(e.Bounds, -6, 0);
			TextFormatFlags align = e.Header.TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : e.Header.TextAlign == HorizontalAlignment.Center ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left;
			TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, text, theme.Color(ThemeRole.HeaderText), align | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		}

		private static void DrawDefaultItem(object sender, DrawListViewItemEventArgs e)
		{
			// In the details view the sub items draw the item; the other views use the default, except a selected item in an
			// older look.
			e.DrawDefault = ((ListView)sender).View != View.Details && !DrawListViewItem(e);
		}

		private static void DrawDefaultSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			e.DrawDefault = !DrawListViewSubItem(e);
		}
	}
}
