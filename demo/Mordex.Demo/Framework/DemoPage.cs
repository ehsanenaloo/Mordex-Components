using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Mordex.Demo
{
	/// <summary>
	/// One page of the demo. A page shows one feature area of the suite with the shared sample data. The shell finds
	/// every non-abstract subclass with a parameterless constructor.
	/// </summary>
	public abstract class DemoPage : UserControl, Mordex.Windows.Common.IThemeAware
	{
		protected DemoPage()
		{
			Dock = DockStyle.Fill;
			DoubleBuffered = true;
		}

		/// <summary>The name in the navigation, for example "Grouping and totals".</summary>
		public abstract string Title { get; }

		/// <summary>The navigation group: Data grid, Scheduling, Navigation, Inputs or Layout.</summary>
		public abstract string Group { get; }

		/// <summary>One or two plain sentences under the page title.</summary>
		public abstract string Summary { get; }

		/// <summary>The order inside the group. Lower comes first.</summary>
		public virtual int Order => 100;

		/// <summary>Called after the window changed between the classic look and a theme, for pages that set colors themselves.</summary>
		public virtual void OnLookChanged()
		{
		}

		/// <summary>Called when the window moves to the themed look or back. A page that holds a component with its own visual style sets it here.</summary>
		public virtual void ApplyLook(bool themed)
		{
		}

		/// <summary>Called each time the page is shown. Keep it cheap.</summary>
		public virtual void OnShown()
		{
		}

		/// <summary>Puts the demo control in the page and, when given, the option panel at its right.</summary>
		protected void Compose(Control content, Control options = null)
		{
			SuspendLayout();
			Controls.Clear();
			content.Dock = DockStyle.Fill;
			Controls.Add(content);
			if (options != null)
			{
				options.Dock = DockStyle.Right;
				Controls.Add(options);
				options.BringToFront();
				content.BringToFront();
			}
			ResumeLayout(true);
		}

		public static IList<DemoPage> CreateAll()
		{
			List<DemoPage> pages = new List<DemoPage>();
			foreach (Type type in typeof(DemoPage).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(DemoPage).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null))
			{
				pages.Add((DemoPage)Activator.CreateInstance(type));
			}
			string[] groupOrder = { "Data grid", "Scheduling", "Navigation", "Inputs", "Layout", "Output and help", "Themes" };
			return pages
				.OrderBy(p => Array.IndexOf(groupOrder, p.Group) is int i && i >= 0 ? i : groupOrder.Length)
				.ThenBy(p => p.Order)
				.ThenBy(p => p.Title, StringComparer.Ordinal)
				.ToList();
		}
	}
}
