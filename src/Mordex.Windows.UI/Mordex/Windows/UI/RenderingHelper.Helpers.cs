#pragma warning disable CS0618 // Bridges the obsolete Office2007 API to the current one.
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI
{
	internal partial class RenderingHelper
	{
		// True on Windows 7 and every later version.
		internal static bool IsWindows7()
		{
			return IsWindows7OrLater(Environment.OSVersion.Version);
		}

		internal static bool IsWindows7OrLater(Version version)
		{
			return version >= Windows7Version;
		}

		internal static Form FindRootForm(MxDockManager uiPanelManager)
		{
			if (uiPanelManager.ContainerControl == null)
			{
				return null;
			}
			Form form = FindForm(uiPanelManager.ContainerControl);
			if (form is MxFloatingWindow)
			{
				DockPanelBase panel = ((MxFloatingWindow)form).Panel;
				if (panel != null && panel.PanelManager != null)
				{
					form = FindRootForm(panel.PanelManager);
				}
			}
			return form;
		}

		internal static Form FindForm(Control control)
		{
			return control.FindForm();
		}

		internal static Control GetTopLevelControl(Control control)
		{
			return control.TopLevelControl;
		}

		internal static Graphics CreateGraphicsFromHdc(IntPtr hdc)
		{
			return Graphics.FromHdc(hdc);
		}

		internal static Control GetParentControl(Control control)
		{
			return control.Parent;
		}

		internal static void SetMouseCapture(Control control, bool capture)
		{
			control.Capture = capture;
		}

		internal static void ReleaseGraphicsHdc(Graphics graphics, IntPtr hdc)
		{
			graphics.ReleaseHdc(hdc);
		}

		internal static bool TryFocusControl(Control control)
		{
			return control.Focus();
		}

		public static bool AreColorsEqual(Color color, Color otherColor)
		{
			if (color.A == otherColor.A && color.R == otherColor.R && color.G == otherColor.G && color.B == otherColor.B)
			{
				return true;
			}
			return false;
		}

		internal static int CompareValuesCurrentCulture(object value, object other, IComparer comparer, bool ignoreCase)
		{
			int result = 0;
			try
			{
				if (comparer == null)
				{
					if (value == DBNull.Value)
					{
						value = null;
					}
					if (other == DBNull.Value)
					{
						other = null;
					}
					if (value is string && other is string)
					{
						result = string.Compare((string)value, (string)other, ignoreCase, CultureInfo.CurrentCulture);
					}
					else if (value != null)
					{
						if (other != null)
						{
							if (!(value is IComparable))
							{
								result = ((!value.Equals(other)) ? 1 : 0);
							}
							else if (other.GetType() == value.GetType())
							{
								result = ((IComparable)value).CompareTo(other);
							}
							else if (!typeof(IConvertible).IsAssignableFrom(other.GetType()))
							{
								result = ((!(value is string)) ? 1 : string.Compare((string)value, other.ToString(), ignoreCase));
							}
							else
							{
								try
								{
									object convertedValue = Convert.ChangeType(other, value.GetType());
									result = ((IComparable)value).CompareTo(convertedValue);
								}
								catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException || ex is ArgumentException)
								{
									// A value that cannot be converted sorts after the other value.
									result = 1;
								}
							}
						}
						else
						{
							result = 1;
						}
					}
					else if (other != null)
					{
						result = -1;
					}
				}
				else
				{
					result = comparer.Compare(value, other);
				}
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// Compare runs inside a sort, where any failure would abort the whole sort; values that cannot be compared sort last.
				result = 1;
			}
			return result;
		}

		public static int CompareValuesInvariantCulture(object value, object other, IComparer comparer, bool ignoreCase)
		{
			int result = 0;
			try
			{
				if (comparer == null)
				{
					if (value == DBNull.Value)
					{
						value = null;
					}
					if (other == DBNull.Value)
					{
						other = null;
					}
					if (value is string && other is string)
					{
						result = string.Compare((string)value, (string)other, ignoreCase, CultureInfo.InvariantCulture);
					}
					else if (value != null)
					{
						result = ((other == null) ? 1 : ((value is IComparable) ? ((IComparable)value).CompareTo(other) : ((!value.Equals(other)) ? 1 : 0)));
					}
					else if (other != null)
					{
						result = -1;
					}
				}
				else
				{
					result = comparer.Compare(value, other);
				}
			}
			catch (Exception ex) when (!(ex is OutOfMemoryException))
			{
				// Compare runs inside a sort, where any failure would abort the whole sort; values that cannot be compared sort last.
				result = 1;
			}
			return result;
		}

		public static bool IsNullOrDbNull(object value)
		{
			if (value == null)
			{
				return true;
			}
			if (Convert.IsDBNull(value))
			{
				return true;
			}
			return false;
		}

		public static MouseEventArgs TranslateMouseEvent(Control control, Control targetControl, MouseEventArgs eventArgs)
		{
			Point p = new Point(eventArgs.X, eventArgs.Y);
			p = control.PointToScreen(p);
			p = targetControl.PointToClient(p);
			return new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
		}
	}
}
