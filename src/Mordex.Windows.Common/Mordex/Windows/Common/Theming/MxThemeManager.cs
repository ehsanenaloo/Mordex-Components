using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Mordex.Windows.Common
{
	/// <summary>A control that repaints when the default theme changes. The manager holds listeners weakly.</summary>
	public interface IThemeListener
	{
		void OnThemeChanged();
	}

	/// <summary>
	/// Holds the themes of the application, the default theme, and the resolved themes for painting. Changing the default
	/// theme tells each listener once, on the thread that added it.
	/// </summary>
	/// <remarks>
	/// Each thread gets its own resolved themes, so the brushes and pens of one resolved theme are never used by two
	/// threads. Selecting a theme with <see cref="DefaultTheme"/> does not register it; <see cref="Register"/> does.
	/// <para/>
	/// The Standard theme reads the colors of Windows. It is rebuilt when Windows colors, the display or high contrast
	/// change, and the listeners are told. Other themes keep their own colors in high contrast. With
	/// <see cref="FollowWindows"/> on, high contrast switches the default theme to Standard and the earlier theme comes
	/// back when high contrast ends.
	/// </remarks>
	public static class MxThemeManager
	{
		private sealed class ListenerEntry
		{
			internal volatile bool Removed;

			internal ThreadOwner Owner;
		}

		// A thread that added a listener or a handler, and the context that runs work on it.
		private sealed class ThreadOwner
		{
			internal readonly Thread Thread;

			internal readonly SynchronizationContext Context;

			internal ThreadOwner(Thread thread, SynchronizationContext context)
			{
				Thread = thread;
				Context = context;
			}
		}

		private static readonly object Gate = new object();

		private static readonly List<MxTheme> Themes = new List<MxTheme>();

		// Registration order. Dead and removed entries are dropped in bulk, so adding is cheap.
		private static readonly List<(WeakReference<IThemeListener> Reference, ListenerEntry Entry)> Listeners = new List<(WeakReference<IThemeListener>, ListenerEntry)>();

		private static readonly ConditionalWeakTable<IThemeListener, ListenerEntry> Entries = new ConditionalWeakTable<IThemeListener, ListenerEntry>();

		private static readonly List<(EventHandler Handler, ThreadOwner Owner)> ChangedHandlers = new List<(EventHandler, ThreadOwner)>();

		// Resolved themes of this thread. They live as long as their theme and the thread. Nothing here holds a theme the
		// application dropped.
		[ThreadStatic]
		private static ConditionalWeakTable<MxTheme, Dictionary<int, ResolvedTheme>> _resolved;

		[ThreadStatic]
		private static ThreadOwner _owner;

		private static MxTheme _default;

		// The theme built from SystemColors, or null when the application replaced it.
		private static MxTheme _systemTheme;

		private static MxTheme _beforeHighContrast;

		private static bool _followWindows;

		private static bool _systemEventsHooked;

		private static int _addsSinceCleanup;

		// Replaced by tests to simulate a change of the Windows colors.
		internal static Func<MxTheme> SystemThemeFactory = BuiltInThemes.Standard;

		static MxThemeManager()
		{
			foreach (MxTheme theme in BuiltInThemes.All())
			{
				Themes.Add(theme);
			}
			_systemTheme = Themes.Find(theme => theme.Name == "Standard");
			_default = _systemTheme;
		}

		/// <summary>Raised after the default theme changed, on the thread that added the handler.</summary>
		public static event EventHandler DefaultThemeChanged
		{
			add
			{
				if (value == null)
				{
					return;
				}
				ThreadOwner owner = CurrentOwner();
				lock (Gate)
				{
					ChangedHandlers.Add((value, owner));
				}
			}
			remove
			{
				if (value == null)
				{
					return;
				}
				lock (Gate)
				{
					int index = ChangedHandlers.FindLastIndex(item => item.Handler == value);
					if (index >= 0)
					{
						ChangedHandlers.RemoveAt(index);
					}
				}
			}
		}

		/// <summary>Raised when a listener or a handler of <see cref="DefaultThemeChanged"/> throws. The others still run.</summary>
		public static event EventHandler<ThreadExceptionEventArgs> ListenerFailed;

		/// <summary>
		/// The theme that controls use when they have no theme of their own. Setting it selects the theme without adding it
		/// to <see cref="RegisteredThemes"/>, so previews of many changed copies of one theme keep nothing alive.
		/// </summary>
		public static MxTheme DefaultTheme
		{
			get
			{
				lock (Gate)
				{
					return _default;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}
				lock (Gate)
				{
					if (ReferenceEquals(_default, value))
					{
						return;
					}
					_default = value;
				}
				RaiseChanged();
			}
		}

		/// <summary>The registered themes, in registration order.</summary>
		public static IReadOnlyList<MxTheme> RegisteredThemes
		{
			get
			{
				lock (Gate)
				{
					return Themes.ToArray();
				}
			}
		}

		public static void Register(MxTheme theme)
		{
			if (theme == null)
			{
				throw new ArgumentNullException(nameof(theme));
			}
			bool defaultReplaced = false;
			lock (Gate)
			{
				int index = Themes.FindIndex(existing => string.Equals(existing.Name, theme.Name, StringComparison.OrdinalIgnoreCase));
				if (index >= 0)
				{
					if (ReferenceEquals(Themes[index], _default))
					{
						_default = theme;
						defaultReplaced = true;
					}
					if (ReferenceEquals(Themes[index], _systemTheme))
					{
						_systemTheme = null;
					}
					Themes[index] = theme;
				}
				else
				{
					Themes.Add(theme);
				}
			}
			if (defaultReplaced)
			{
				RaiseChanged();
			}
		}

		/// <summary>The registered theme with the name, or null.</summary>
		public static MxTheme Find(string name)
		{
			lock (Gate)
			{
				return Themes.Find(theme => string.Equals(theme.Name, name, StringComparison.OrdinalIgnoreCase));
			}
		}

		/// <summary>
		/// The resolved theme of a theme at a DPI for the calling thread. It is made on the first request of the thread and
		/// kept. Do not pass it to another thread.
		/// </summary>
		public static ResolvedTheme Resolve(MxTheme theme, int dpi = 96)
		{
			if (theme == null)
			{
				throw new ArgumentNullException(nameof(theme));
			}
			ConditionalWeakTable<MxTheme, Dictionary<int, ResolvedTheme>> table = _resolved ?? (_resolved = new ConditionalWeakTable<MxTheme, Dictionary<int, ResolvedTheme>>());
			Dictionary<int, ResolvedTheme> byDpi = table.GetValue(theme, key => new Dictionary<int, ResolvedTheme>());
			if (!byDpi.TryGetValue(dpi, out ResolvedTheme resolved))
			{
				resolved = new ResolvedTheme(theme, dpi);
				byDpi[dpi] = resolved;
			}
			bool hookNeeded;
			lock (Gate)
			{
				hookNeeded = !_systemEventsHooked && ReferenceEquals(theme, _systemTheme);
			}
			if (hookNeeded)
			{
				HookSystemEvents();
			}
			return resolved;
		}

		/// <summary>The resolved default theme at a DPI.</summary>
		public static ResolvedTheme ResolveDefault(int dpi = 96)
		{
			return Resolve(DefaultTheme, dpi);
		}

		public static void AddListener(IThemeListener listener)
		{
			if (listener == null)
			{
				throw new ArgumentNullException(nameof(listener));
			}
			ThreadOwner owner = CurrentOwner();
			lock (Gate)
			{
				if (Entries.TryGetValue(listener, out _))
				{
					return;
				}
				// Compact after as many adds as there are entries, so the cost of a cleanup is spread over the adds.
				if (++_addsSinceCleanup >= Math.Max(64, Listeners.Count))
				{
					CompactListeners();
				}
				ListenerEntry entry = new ListenerEntry { Owner = owner };
				Entries.Add(listener, entry);
				Listeners.Add((new WeakReference<IThemeListener>(listener), entry));
			}
			HookSystemEvents();
		}

		public static void RemoveListener(IThemeListener listener)
		{
			if (listener == null)
			{
				return;
			}
			lock (Gate)
			{
				if (Entries.TryGetValue(listener, out ListenerEntry entry))
				{
					entry.Removed = true;
					Entries.Remove(listener);
				}
			}
		}

		private static void CompactListeners()
		{
			_addsSinceCleanup = 0;
			Listeners.RemoveAll(item => item.Entry.Removed || !item.Reference.TryGetTarget(out _));
		}

		/// <summary>
		/// When true, the default theme follows the Windows setting for light or dark apps: it switches to the theme of the
		/// same family and the matching kind, when the family has one. It is off by default.
		/// </summary>
		public static bool FollowWindows
		{
			get
			{
				lock (Gate)
				{
					return _followWindows;
				}
			}
			set
			{
				bool apply;
				lock (Gate)
				{
					if (_followWindows == value)
					{
						return;
					}
					_followWindows = value;
					apply = value;
				}
				if (apply)
				{
					HookSystemEvents();
					ApplyWindowsKind();
				}
			}
		}

		/// <summary>True when Windows is set to dark apps.</summary>
		public static bool WindowsUsesDarkApps()
		{
			try
			{
				object value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
				return value is int light && light == 0;
			}
			catch (Exception ex) when (ex is System.Security.SecurityException || ex is UnauthorizedAccessException || ex is System.IO.IOException)
			{
				return false;
			}
		}

		/// <summary>The accent color of Windows, or empty when it cannot be read.</summary>
		public static Color WindowsAccentColor()
		{
			try
			{
				object value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "ColorizationColor", null);
				if (value is int argb)
				{
					return Color.FromArgb(255, Color.FromArgb(argb));
				}
			}
			catch (Exception ex) when (ex is System.Security.SecurityException || ex is UnauthorizedAccessException || ex is System.IO.IOException)
			{
			}
			return Color.Empty;
		}

		/// <summary>The theme of the same family as <paramref name="current"/> with the wanted kind, or null.</summary>
		internal static MxTheme FindVariant(MxTheme current, ThemeKind kind)
		{
			lock (Gate)
			{
				if (current.Kind == kind)
				{
					return current;
				}
				return Themes.Find(theme => theme.Kind == kind && string.Equals(theme.Family, current.Family, StringComparison.OrdinalIgnoreCase));
			}
		}

		// The handlers run on the thread of SystemEvents. Changes reach the listeners through RaiseChanged, on their own threads.
		private static void HookSystemEvents()
		{
			lock (Gate)
			{
				if (_systemEventsHooked)
				{
					return;
				}
				_systemEventsHooked = true;
			}
			SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
			SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
		}

		private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color || e.Category == UserPreferenceCategory.VisualStyle)
			{
				OnSystemColorsChanged();
			}
		}

		private static void OnDisplaySettingsChanged(object sender, EventArgs e)
		{
			OnSystemColorsChanged();
		}

		internal static void OnSystemColorsChanged()
		{
			bool follow;
			lock (Gate)
			{
				follow = _followWindows;
			}
			bool rebuilt = RebuildSystemTheme();
			if (follow)
			{
				ApplyWindowsKind();
			}
			if (rebuilt)
			{
				RaiseChanged();
			}
		}

		// Builds the Standard theme again. When the colors are the same nothing changes. Otherwise the new theme takes the
		// place of the old one, the default included, and its resolved themes are made new on the next request.
		private static bool RebuildSystemTheme()
		{
			MxTheme fresh = SystemThemeFactory();
			lock (Gate)
			{
				MxTheme old = _systemTheme;
				if (old == null || old.ColorsNoCopy.SequenceEqual(fresh.ColorsNoCopy))
				{
					return false;
				}
				int index = Themes.IndexOf(old);
				if (index >= 0)
				{
					Themes[index] = fresh;
				}
				if (ReferenceEquals(_default, old))
				{
					_default = fresh;
				}
				if (ReferenceEquals(_beforeHighContrast, old))
				{
					_beforeHighContrast = fresh;
				}
				_systemTheme = fresh;
				return true;
			}
		}

		private static void ApplyWindowsKind()
		{
			MxTheme target = null;
			lock (Gate)
			{
				if (SystemInformation.HighContrast)
				{
					if (_systemTheme == null || ReferenceEquals(_default, _systemTheme))
					{
						return;
					}
					_beforeHighContrast = _default;
					target = _systemTheme;
				}
				else if (_beforeHighContrast != null)
				{
					target = _beforeHighContrast;
					_beforeHighContrast = null;
				}
			}
			if (target == null)
			{
				target = FindVariant(DefaultTheme, WindowsUsesDarkApps() ? ThemeKind.Dark : ThemeKind.Light);
			}
			if (target != null)
			{
				DefaultTheme = target;
			}
		}

		// The owner of the calling thread. A thread whose Windows Forms context changed gets a new owner. A thread without
		// one gets a context of its own, which runs posted work while the thread runs a message loop.
		private static ThreadOwner CurrentOwner()
		{
			SynchronizationContext current = SynchronizationContext.Current;
			ThreadOwner owner = _owner;
			if (owner != null && (ReferenceEquals(owner.Context, current) || !(current is WindowsFormsSynchronizationContext)))
			{
				return owner;
			}
			owner = new ThreadOwner(Thread.CurrentThread, current as WindowsFormsSynchronizationContext ?? new WindowsFormsSynchronizationContext());
			_owner = owner;
			return owner;
		}

		private sealed class Delivery
		{
			internal readonly List<ListenerEntry> Entries = new List<ListenerEntry>();

			internal readonly List<IThemeListener> Listeners = new List<IThemeListener>();

			internal readonly List<EventHandler> Handlers = new List<EventHandler>();
		}

		private static void RaiseChanged()
		{
			List<(ThreadOwner Owner, Delivery Work)> deliveries = new List<(ThreadOwner, Delivery)>();
			Delivery For(ThreadOwner owner)
			{
				foreach ((ThreadOwner known, Delivery work) in deliveries)
				{
					if (ReferenceEquals(known, owner))
					{
						return work;
					}
				}
				Delivery added = new Delivery();
				deliveries.Add((owner, added));
				return added;
			}
			lock (Gate)
			{
				foreach ((WeakReference<IThemeListener> reference, ListenerEntry entry) in Listeners)
				{
					if (!entry.Removed && reference.TryGetTarget(out IThemeListener listener))
					{
						Delivery work = For(entry.Owner);
						work.Entries.Add(entry);
						work.Listeners.Add(listener);
					}
				}
				CompactListeners();
				foreach ((EventHandler handler, ThreadOwner owner) in ChangedHandlers)
				{
					For(owner).Handlers.Add(handler);
				}
			}
			foreach ((ThreadOwner owner, Delivery work) in deliveries)
			{
				Deliver(owner, work);
			}
		}

		// Runs the work on the owner thread: at once when it is the calling thread, else through its context. A thread that
		// ended has no controls left to tell.
		private static void Deliver(ThreadOwner owner, Delivery work)
		{
			if (ReferenceEquals(owner.Thread, Thread.CurrentThread))
			{
				Notify(work);
				return;
			}
			if (!owner.Thread.IsAlive)
			{
				return;
			}
			try
			{
				owner.Context.Post(_ => Notify(work), null);
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is InvalidAsynchronousStateException)
			{
				// The message loop of the thread ended while the thread still runs.
			}
		}

		private static void Notify(Delivery work)
		{
			for (int i = 0; i < work.Listeners.Count; i++)
			{
				if (work.Entries[i].Removed)
				{
					continue;
				}
				try
				{
					work.Listeners[i].OnThemeChanged();
				}
				catch (Exception ex)
				{
					ReportFailure(ex);
				}
			}
			foreach (EventHandler handler in work.Handlers)
			{
				try
				{
					handler(null, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					ReportFailure(ex);
				}
			}
		}

		private static void ReportFailure(Exception ex)
		{
			Debug.WriteLine("Theme listener failed: " + ex);
			try
			{
				ListenerFailed?.Invoke(null, new ThreadExceptionEventArgs(ex));
			}
			catch (Exception inner)
			{
				Debug.WriteLine("Theme failure handler failed: " + inner);
			}
		}
	}
}
