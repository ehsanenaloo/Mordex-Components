using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// A theme made ready for painting at one DPI. It holds the colors, brushes and pens of the theme, made once and kept, so
	/// a paint method reads them and creates nothing.
	/// </summary>
	/// <remarks>
	/// The objects are shared by every control that uses the theme. A control must not change them or dispose them. GDI+
	/// objects are for one thread at a time, so use a resolved theme on the UI thread. The manager owns the instance.
	/// </remarks>
	public sealed partial class ResolvedTheme : IDisposable
	{
		// Shapes and gradients depend on the size of a control. A resize can ask for many sizes, so these caches are bounded.
		private const int LargestShapeCache = 64;

		private readonly SolidBrush[] _brushes;

		private readonly Pen[] _pens;

		private readonly SolidBrush[] _officeBrushes;

		private readonly Pen[] _officePens;

		// Guards the three caches below: a print or DrawToBitmap on a worker thread can use the theme while the UI thread paints.
		private readonly object _cacheLock = new object();

		private readonly Dictionary<(ThemeRole, int), Pen> _widePens = new Dictionary<(ThemeRole, int), Pen>();

		private readonly Dictionary<(ThemeRole, ThemeRole, int, int, LinearGradientMode), LinearGradientBrush> _gradients = new Dictionary<(ThemeRole, ThemeRole, int, int, LinearGradientMode), LinearGradientBrush>();

		private static readonly bool[] FaceRoles = BuildFaceRoles();

		private readonly Dictionary<(ThemeRole, int, int, LinearGradientMode), LinearGradientBrush> _faceGradients = new Dictionary<(ThemeRole, int, int, LinearGradientMode), LinearGradientBrush>();

		private readonly Dictionary<(int, int, int), LinearGradientBrush> _glossBrushes = new Dictionary<(int, int, int), LinearGradientBrush>();

		private readonly Dictionary<(int, int, int), GraphicsPath> _roundedPaths = new Dictionary<(int, int, int), GraphicsPath>();

		private bool _disposed;

		internal ResolvedTheme(MxTheme theme, int dpi)
		{
			Theme = theme;
			Dpi = dpi;
			Scale = dpi / 96f;
			_brushes = new SolidBrush[MxTheme.RoleCount];
			_pens = new Pen[MxTheme.RoleCount];
			int officeLength = theme.OfficePaletteNoCopy == null ? 0 : theme.OfficePaletteNoCopy.Length;
			_officeBrushes = new SolidBrush[officeLength];
			_officePens = new Pen[officeLength];
		}

		private static bool[] BuildFaceRoles()
		{
			bool[] roles = new bool[MxTheme.RoleCount];
			foreach (ThemeRole role in new[] { ThemeRole.SurfaceRaised, ThemeRole.SurfaceHot, ThemeRole.SurfacePressed, ThemeRole.Header, ThemeRole.HeaderHot, ThemeRole.HeaderPressed, ThemeRole.TabActive, ThemeRole.TabInactive, ThemeRole.TabHot, ThemeRole.ToolbarBack, ThemeRole.StatusBarBack, ThemeRole.Accent, ThemeRole.AccentHot, ThemeRole.AccentPressed })
			{
				roles[(int)role] = true;
			}
			return roles;
		}

		public MxTheme Theme { get; }

		public int Dpi { get; }

		/// <summary>The factor from 96 DPI pixels to device pixels.</summary>
		public float Scale { get; }

		public ThemeShape Shape => Theme.Shape;

		/// <summary>A size in 96 DPI pixels as device pixels.</summary>
		public int Scaled(int pixels)
		{
			return (int)Math.Round(pixels * Scale);
		}

		public Color Color(ThemeRole role)
		{
			return Theme.ColorsNoCopy[(int)role];
		}

		public SolidBrush Brush(ThemeRole role)
		{
			SolidBrush brush = _brushes[(int)role];
			if (brush == null)
			{
				ThrowIfDisposed();
				brush = new SolidBrush(Theme.ColorsNoCopy[(int)role]);
				_brushes[(int)role] = brush;
			}
			return brush;
		}

		/// <summary>A pen one device pixel wide.</summary>
		public Pen Pen(ThemeRole role)
		{
			Pen pen = _pens[(int)role];
			if (pen == null)
			{
				ThrowIfDisposed();
				pen = new Pen(Theme.ColorsNoCopy[(int)role], 1f);
				_pens[(int)role] = pen;
			}
			return pen;
		}

		/// <summary>A pen that is <paramref name="width"/> pixels wide at 96 DPI, scaled to this DPI.</summary>
		public Pen Pen(ThemeRole role, int width)
		{
			if (width <= 1 && Scale <= 1f)
			{
				return Pen(role);
			}
			(ThemeRole, int) key = (role, width);
			lock (_cacheLock)
			{
				if (!_widePens.TryGetValue(key, out Pen pen))
				{
					ThrowIfDisposed();
					pen = new Pen(Theme.ColorsNoCopy[(int)role], Math.Max(1, Scaled(width)));
					_widePens[key] = pen;
				}
				return pen;
			}
		}

		/// <summary>A slot of the classic Office palette of the theme. The theme must have one.</summary>
		public Color OfficeColor(int slot)
		{
			return Theme.OfficePaletteNoCopy[slot];
		}

		public SolidBrush OfficeBrush(int slot)
		{
			SolidBrush brush = _officeBrushes[slot];
			if (brush == null)
			{
				ThrowIfDisposed();
				brush = new SolidBrush(Theme.OfficePaletteNoCopy[slot]);
				_officeBrushes[slot] = brush;
			}
			return brush;
		}

		public Pen OfficePen(int slot)
		{
			Pen pen = _officePens[slot];
			if (pen == null)
			{
				ThrowIfDisposed();
				pen = new Pen(Theme.OfficePaletteNoCopy[slot], 1f);
				_officePens[slot] = pen;
			}
			return pen;
		}

		/// <summary>Fills a rectangle with a gradient between two roles. The theme keeps the brush for that size.</summary>
		public void FillGradient(Graphics graphics, Rectangle bounds, ThemeRole from, ThemeRole to, LinearGradientMode mode)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			if (Theme.Shape.IsFlat)
			{
				graphics.FillRectangle(Brush(from), bounds);
				return;
			}
			(ThemeRole, ThemeRole, int, int, LinearGradientMode) key = (from, to, bounds.Width, bounds.Height, mode);
			// The brush is used inside the lock: a full cache disposes its brushes.
			lock (_cacheLock)
			{
				if (!_gradients.TryGetValue(key, out LinearGradientBrush brush))
				{
					ThrowIfDisposed();
					if (_gradients.Count >= LargestShapeCache)
					{
						DisposeAll(_gradients.Values);
						_gradients.Clear();
					}
					brush = new LinearGradientBrush(new Rectangle(0, 0, bounds.Width, bounds.Height), Theme.ColorsNoCopy[(int)from], Theme.ColorsNoCopy[(int)to], mode);
					_gradients[key] = brush;
				}
				// The brush lies at the origin; the graphics moves to the rectangle, so one brush serves every position.
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.FillRectangle(brush, 0, 0, bounds.Width, bounds.Height);
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		/// <summary>Fills a rounded rectangle. A radius of 0 gives a plain rectangle.</summary>
		public void FillRounded(Graphics graphics, Rectangle bounds, ThemeRole role, int radius)
		{
			ThemeStyle style = Theme.Style;
			if (style.FaceGradient != ThemeGradient.None && FaceRoles[(int)role] && bounds.Width > 1 && bounds.Height > 1)
			{
				FillFace(graphics, bounds, role, radius, LinearGradientMode.Vertical);
				return;
			}
			if (radius <= 0)
			{
				graphics.FillRectangle(Brush(role), bounds);
				return;
			}
			lock (_cacheLock)
			{
				GraphicsPath path = GetRoundedPath(bounds.Width, bounds.Height, Scaled(radius));
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.FillPath(Brush(role), path);
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		/// <summary>
		/// Fills a face in the style of the theme: a gradient from a lighter end to the face color, and a light band over the
		/// upper half when the style asks for gloss. A theme without a face gradient gets a plain fill. <paramref name="mode"/>
		/// says where the light end lies: the top for vertical, the left for horizontal.
		/// </summary>
		public void FillFace(Graphics graphics, Rectangle bounds, ThemeRole role, int radius, LinearGradientMode mode)
		{
			FillFace(graphics, bounds, role, radius, mode, true);
		}

		internal void FillFace(Graphics graphics, Rectangle bounds, ThemeRole role, int radius, LinearGradientMode mode, bool allowGloss)
		{
			ThemeStyle style = Theme.Style;
			if (style.FaceGradient == ThemeGradient.None || bounds.Width <= 1 || bounds.Height <= 1)
			{
				FillRoundedPlain(graphics, bounds, role, radius);
				return;
			}
			if (mode == LinearGradientMode.Vertical && style.FaceGradient == ThemeGradient.Horizontal)
			{
				mode = LinearGradientMode.Horizontal;
			}
			(ThemeRole, int, int, LinearGradientMode) key = (role, bounds.Width, bounds.Height, mode);
			// The cached brushes are used inside the lock: a full cache disposes its brushes. The lock is re-entrant.
			lock (_cacheLock)
			{
				if (!_faceGradients.TryGetValue(key, out LinearGradientBrush brush))
				{
					ThrowIfDisposed();
					if (_faceGradients.Count >= LargestShapeCache)
					{
						DisposeAll(_faceGradients.Values);
						_faceGradients.Clear();
					}
					Color face = Theme.ColorsNoCopy[(int)role];
					brush = NewFaceBrush(face, bounds.Width, bounds.Height, mode);
					_faceGradients[key] = brush;
				}
				graphics.TranslateTransform(bounds.X, bounds.Y);
				if (radius <= 0)
				{
					graphics.FillRectangle(brush, 0, 0, bounds.Width, bounds.Height);
				}
				else
				{
					graphics.FillPath(brush, GetRoundedPath(bounds.Width, bounds.Height, Scaled(radius)));
				}
				if (allowGloss && style.Gloss && style.Modern.FaceKind != ThemeFaceKind.Glass && mode == LinearGradientMode.Vertical && bounds.Height > 3)
				{
					int inset = Math.Max(1, Scaled(radius) / 2);
					int glossHeight = bounds.Height / 2;
					int top = style.GlossAlpha > 0 ? style.GlossAlpha : 120;
					int bottom = style.GlossBand ? top : 30;
					(int, int, int) glossKey = (glossHeight, top, bottom);
					if (!_glossBrushes.TryGetValue(glossKey, out LinearGradientBrush gloss))
					{
						if (_glossBrushes.Count >= LargestShapeCache)
						{
							DisposeAll(_glossBrushes.Values);
							_glossBrushes.Clear();
						}
						gloss = new LinearGradientBrush(new Rectangle(0, 0, 2, glossHeight), System.Drawing.Color.FromArgb(top, 255, 255, 255), System.Drawing.Color.FromArgb(bottom, 255, 255, 255), LinearGradientMode.Vertical);
						_glossBrushes[glossKey] = gloss;
					}
					graphics.FillRectangle(gloss, inset, 1, bounds.Width - inset * 2, glossHeight);
				}
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		internal void FillRoundedPlain(Graphics graphics, Rectangle bounds, ThemeRole role, int radius)
		{
			if (radius <= 0 || bounds.Width <= 1 || bounds.Height <= 1)
			{
				graphics.FillRectangle(Brush(role), bounds);
				return;
			}
			lock (_cacheLock)
			{
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.FillPath(Brush(role), GetRoundedPath(bounds.Width, bounds.Height, Scaled(radius)));
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		/// <summary>Draws the outline of a rounded rectangle. The outline lies inside the bounds.</summary>
		public void DrawRounded(Graphics graphics, Rectangle bounds, ThemeRole role, int radius)
		{
			if (radius <= 0)
			{
				if (Theme.Style.Bevel == ThemeBevel.Raised && (role == ThemeRole.Border || role == ThemeRole.BorderPressed) && bounds.Width > 3 && bounds.Height > 3)
				{
					DrawBevel(graphics, bounds, role == ThemeRole.BorderPressed);
					return;
				}
				graphics.DrawRectangle(Pen(role), bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
				return;
			}
			lock (_cacheLock)
			{
				GraphicsPath path = GetRoundedPath(bounds.Width - 1, bounds.Height - 1, Scaled(radius));
				graphics.TranslateTransform(bounds.X, bounds.Y);
				graphics.DrawPath(Pen(role), path);
				graphics.TranslateTransform(-bounds.X, -bounds.Y);
			}
		}

		/// <summary>Draws a 3D edge: light on the top and left, dark on the bottom and right, reversed when sunken.</summary>
		public void DrawBevel(Graphics graphics, Rectangle bounds, bool sunken)
		{
			Pen light = LightEdgePen;
			Pen dark = Pen(ThemeRole.BorderStrong);
			Pen shade = Pen(ThemeRole.Border);
			int right = bounds.Right - 1;
			int bottom = bounds.Bottom - 1;
			Pen topLeft = sunken ? dark : light;
			Pen bottomRight = sunken ? light : dark;
			graphics.DrawLine(topLeft, bounds.X, bounds.Y, right - 1, bounds.Y);
			graphics.DrawLine(topLeft, bounds.X, bounds.Y, bounds.X, bottom - 1);
			graphics.DrawLine(bottomRight, bounds.X, bottom, right, bottom);
			graphics.DrawLine(bottomRight, right, bounds.Y, right, bottom);
			if (!sunken)
			{
				graphics.DrawLine(shade, bounds.X + 1, bottom - 1, right - 1, bottom - 1);
				graphics.DrawLine(shade, right - 1, bounds.Y + 1, right - 1, bottom - 1);
			}
		}

		private GraphicsPath GetRoundedPath(int width, int height, int radius)
		{
			(int, int, int) key = (width, height, radius);
			if (!_roundedPaths.TryGetValue(key, out GraphicsPath path))
			{
				ThrowIfDisposed();
				if (_roundedPaths.Count >= LargestShapeCache)
				{
					DisposeAll(_roundedPaths.Values);
					_roundedPaths.Clear();
				}
				path = new GraphicsPath();
				int diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(width, height)));
				path.AddArc(0, 0, diameter, diameter, 180, 90);
				path.AddArc(width - diameter, 0, diameter, diameter, 270, 90);
				path.AddArc(width - diameter, height - diameter, diameter, diameter, 0, 90);
				path.AddArc(0, height - diameter, diameter, diameter, 90, 90);
				path.CloseFigure();
				_roundedPaths[key] = path;
			}
			return path;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(nameof(ResolvedTheme));
			}
		}

		private static void DisposeAll<T>(IEnumerable<T> items) where T : IDisposable
		{
			foreach (T item in items)
			{
				item.Dispose();
			}
		}

		private static void DisposeNonNull<T>(T[] items) where T : class, IDisposable
		{
			foreach (T item in items)
			{
				item?.Dispose();
			}
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			DisposeNonNull(_brushes);
			DisposeNonNull(_pens);
			DisposeNonNull(_officeBrushes);
			DisposeNonNull(_officePens);
			lock (_cacheLock)
			{
				DisposeAll(_widePens.Values);
				DisposeAll(_gradients.Values);
				DisposeAll(_faceGradients.Values);
				DisposeAll(_glossBrushes.Values);
				DisposeAll(_roundedPaths.Values);
				DisposeStyleObjects();
				DisposeOfficeObjects();
				_widePens.Clear();
				_faceGradients.Clear();
				_glossBrushes.Clear();
				_gradients.Clear();
				_roundedPaths.Clear();
			}
		}
	}
}
