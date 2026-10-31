using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridFormatStyle
	{
		public bool IsReadOnly()
		{
			return _isReadOnly;
		}

		internal void SetReadOnly(bool condition)
		{
			_isReadOnly = condition;
		}

		private void ThrowIfReadOnly()
		{
			if (_isReadOnly)
			{
				throw new InvalidOperationException("Properties can not be changed if the GridFormatStyle is read only.");
			}
		}

		internal bool IsInCollection()
		{
			if (_owner != null && _owner is GridFormatStyleCollection)
			{
				return true;
			}
			return false;
		}

		internal object GetOwner()
		{
			return _owner;
		}

		public void Reset()
		{
			ThrowIfReadOnly();
			_alpha = -1;
			_appearance = ComputeAppearance();
			_backgroundColor = GetDefaultBackColor();
			_backColorGradient = GetDefaultBackColorGradient();
			_textColor = GetDefaultForeColor();
			_backColorAlphaMode = AlphaMode.Empty;
			_backgroundImage = null;
			_backgroundImageDrawMode = BackgroundImageDrawMode.Empty;
			ReleaseBaseFont();
			ReleaseResolvedFont();
			_fontBold = TriState.Empty;
			_fontItalic = TriState.Empty;
			_fontName = "";
			_fontSize = 0f;
			_fontStrikeout = TriState.Empty;
			_fontUnderline = ComputeTriState();
			_foreColorAlphaMode = AlphaMode.Empty;
			_imageHorizontalAlignment = ImageHorizontalAlignment.Empty;
			_imageVerticalAlignment = ImageVerticalAlignment.Empty;
			_lineAlignment = ComputeTextAlignment();
			_textAlignment = GetDefaultTextAlignment();
			_floatValue = 1f;
			_blend = -1f;
			_blendGradient = -1f;
			_backgroundGradientMode = BackgroundGradientMode.Empty;
		}

		internal void ApplyPredefinedColors()
		{
			SetDefaultBackColor(GetPredefinedBackColor());
			SetDefaultBackColorGradient(GetPredefinedBackColorGradient());
			SetDefaultForeColor(GetPredefinedForeColor());
			if (_predefinedStyle == PredefinedStyle.SelectedFormatStyle)
			{
				_selectionBlend = _owner?.DataGrid?.GetThemedSelectionBlend();
				_selectionBlendColor = _baseBackColor;
			}
		}

		// The back color came from another style, so its selection gradient (or none) comes along.
		private void TakeSelectionBlend(GridFormatStyle formatStyle)
		{
			_selectionBlend = formatStyle._selectionBlend;
			_selectionBlendColor = formatStyle._selectionBlendColor;
		}

		// True when the back of this style is the gradient of the theme selection.
		private bool UsesSelectionBlend()
		{
			return _selectionBlend != null && !HasGradient() && BackColor == _selectionBlendColor && (_alpha == -1 || _alpha == 255) && GetEffectiveBlend() == 0f;
		}

		internal bool IsDefaultStyle()
		{
			if (_alpha != -1)
			{
				return false;
			}
			if (_appearance != ComputeAppearance())
			{
				return false;
			}
			if (_hasExplicitBackColor)
			{
				return false;
			}
			if (_backgroundColor != GetDefaultBackColor())
			{
				return false;
			}
			if (!_backColorGradient.IsEmpty)
			{
				return false;
			}
			if (!_textColor.IsEmpty)
			{
				return false;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_backgroundImage != null)
			{
				return false;
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				return false;
			}
			if (_baseFont != null)
			{
				return false;
			}
			if (_fontBold != TriState.Empty)
			{
				return false;
			}
			if (_fontItalic != TriState.Empty)
			{
				return false;
			}
			if (_fontName != null && _fontName.Length > 0)
			{
				return false;
			}
			if (_fontSize != 0f)
			{
				return false;
			}
			if (_fontStrikeout != TriState.Empty)
			{
				return false;
			}
			if (_fontUnderline != ComputeTriState())
			{
				return false;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_imageHorizontalAlignment != ImageHorizontalAlignment.Empty)
			{
				return false;
			}
			if (_imageVerticalAlignment != ImageVerticalAlignment.Empty)
			{
				return false;
			}
			if (_lineAlignment != ComputeTextAlignment())
			{
				return false;
			}
			if (_textAlignment != GetDefaultTextAlignment())
			{
				return false;
			}
			if (_blend != -1f)
			{
				return false;
			}
			if (_blendGradient != -1f)
			{
				return false;
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return false;
			}
			return true;
		}

		internal float GetFloatValue()
		{
			return _floatValue;
		}

		internal void UpdateFloatValue(float floatValue)
		{
			if (!(floatValue > 0f && floatValue <= 1f))
			{
				throw new ArgumentOutOfRangeException(nameof(floatValue));
			}
			if (floatValue != _floatValue)
			{
				_floatValue = floatValue;
				ReleaseResolvedFont();
			}
		}

		internal GridFormatStyle CloneWithDefaults(int alpha, AlphaMode backColorAlphaMode, AlphaMode foreColorAlphaMode, Color color)
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle(this);
			gridFormatStyle._defaultBaseColor = color;
			if (gridFormatStyle._alpha == -1)
			{
				gridFormatStyle._alpha = alpha;
			}
			if (gridFormatStyle._backColorAlphaMode == AlphaMode.Empty)
			{
				gridFormatStyle._backColorAlphaMode = backColorAlphaMode;
			}
			if (gridFormatStyle._foreColorAlphaMode == AlphaMode.Empty)
			{
				gridFormatStyle._foreColorAlphaMode = foreColorAlphaMode;
			}
			return gridFormatStyle;
		}

		internal GridFormatStyle MergeWith(GridFormatStyle formatStyle, bool condition, bool otherCondition)
		{
			if (formatStyle == null || (formatStyle._predefinedStyle == PredefinedStyle.None && formatStyle.IsDefaultStyle()))
			{
				return this;
			}
			GridFormatStyle gridFormatStyle = new GridFormatStyle(this);
			if (formatStyle._floatValue != 1f)
			{
				gridFormatStyle._floatValue = formatStyle._floatValue;
			}
			if (!condition)
			{
				if (formatStyle._alpha != -1)
				{
					gridFormatStyle._alpha = formatStyle._alpha;
				}
				if (formatStyle._appearance != Appearance.Empty)
				{
					gridFormatStyle._appearance = formatStyle._appearance;
				}
				if (otherCondition)
				{
					if (!formatStyle.BackColor.IsEmpty)
					{
						gridFormatStyle.BackColor = formatStyle.BackColor;
						gridFormatStyle.TakeSelectionBlend(formatStyle);
					}
					if (formatStyle.BackColorGradient != Color.Empty)
					{
						gridFormatStyle.BackColorGradient = formatStyle.BackColorGradient;
					}
				}
				else
				{
					if (!formatStyle._backgroundColor.IsEmpty)
					{
						gridFormatStyle.BackColor = formatStyle._backgroundColor;
						gridFormatStyle.TakeSelectionBlend(formatStyle);
					}
					if (formatStyle._backColorGradient != Color.Empty)
					{
						gridFormatStyle.BackColorGradient = formatStyle._backColorGradient;
					}
				}
				if (formatStyle._backColorAlphaMode != AlphaMode.Empty)
				{
					gridFormatStyle._backColorAlphaMode = formatStyle._backColorAlphaMode;
				}
				if (formatStyle._backgroundImage != null)
				{
					gridFormatStyle._backgroundImage = formatStyle._backgroundImage;
				}
				if (formatStyle._backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
				{
					gridFormatStyle._backgroundImageDrawMode = formatStyle._backgroundImageDrawMode;
				}
				if (formatStyle._textAlignment != TextAlignment.Empty)
				{
					gridFormatStyle._textAlignment = formatStyle._textAlignment;
				}
				if (formatStyle._lineAlignment != TextAlignment.Empty)
				{
					gridFormatStyle._lineAlignment = formatStyle._lineAlignment;
				}
				if (formatStyle._blend != -1f)
				{
					gridFormatStyle._blend = formatStyle._blend;
				}
				if (formatStyle._blendGradient != -1f)
				{
					gridFormatStyle._blendGradient = formatStyle._blendGradient;
				}
				if (formatStyle._backgroundGradientMode != BackgroundGradientMode.Empty)
				{
					gridFormatStyle._backgroundGradientMode = formatStyle._backgroundGradientMode;
				}
			}
			if (otherCondition)
			{
				if (!formatStyle.ForeColor.IsEmpty)
				{
					gridFormatStyle.ForeColor = formatStyle.ForeColor;
				}
			}
			else if (!formatStyle._textColor.IsEmpty)
			{
				gridFormatStyle.ForeColor = formatStyle._textColor;
			}
			if (formatStyle._foreColorAlphaMode != AlphaMode.Empty)
			{
				gridFormatStyle._foreColorAlphaMode = formatStyle._foreColorAlphaMode;
			}
			if (formatStyle._imageHorizontalAlignment != ImageHorizontalAlignment.Empty)
			{
				gridFormatStyle._imageHorizontalAlignment = formatStyle._imageHorizontalAlignment;
			}
			if (formatStyle._imageVerticalAlignment != ImageVerticalAlignment.Empty)
			{
				gridFormatStyle._imageVerticalAlignment = formatStyle._imageVerticalAlignment;
			}
			if (formatStyle._baseFont != null)
			{
				Font shared = GridFontCache.Acquire(formatStyle._baseFont);
				gridFormatStyle.ReleaseBaseFont();
				gridFormatStyle._baseFont = shared;
				gridFormatStyle._isPooledBaseFont = true;
			}
			if (formatStyle._fontBold != TriState.Empty)
			{
				gridFormatStyle._fontBold = formatStyle._fontBold;
			}
			if (formatStyle._fontItalic != TriState.Empty)
			{
				gridFormatStyle._fontItalic = formatStyle._fontItalic;
			}
			if (formatStyle._fontName != "")
			{
				gridFormatStyle._fontName = formatStyle._fontName;
			}
			if (formatStyle._fontSize != 0f)
			{
				gridFormatStyle._fontSize = formatStyle._fontSize;
			}
			if (formatStyle._fontStrikeout != TriState.Empty)
			{
				gridFormatStyle._fontStrikeout = formatStyle._fontStrikeout;
			}
			if (formatStyle._fontUnderline != TriState.Empty)
			{
				gridFormatStyle._fontUnderline = formatStyle._fontUnderline;
			}
			return gridFormatStyle;
		}

		internal GridFormatStyle ApplyOverrides(GridFormatStyle gridFormatStyle)
		{
			return MergeWith(gridFormatStyle, false, true);
		}

		internal Color GetBlendedBackColor()
		{
			if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
			{
				return BlendColors(GetBaseColor(), BackColor, GetEffectiveBlend());
			}
			if (!BackColor.IsEmpty)
			{
				return Color.FromArgb(255, BackColor);
			}
			if (DataGrid != null)
			{
				return DataGrid.BackColor;
			}
			return SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorGradient()
		{
			ThrowIfReadOnly();
			_backColorGradient = GetDefaultBackColorGradient();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColor()
		{
			ThrowIfReadOnly();
			_backgroundColor = GetDefaultBackColor();
			NotifyOwnerLayoutChanged();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetForeColor()
		{
			ThrowIfReadOnly();
			_textColor = GetDefaultForeColor();
		}

		// Gives the style a fixed copy of its resolved font when it has none. The style owns the copy, unlike a font set through Font.
		internal void PinResolvedFont(Font defaultFont)
		{
			if (_baseFont != null)
			{
				return;
			}
			Font resolved = ResolveFont(defaultFont);
			if (resolved != _baseFont)
			{
				AdoptFont((Font)resolved.Clone());
			}
		}

		internal void AdoptFont(Font font)
		{
			ThrowIfReadOnly();
			if (font != _baseFont)
			{
				ReleaseBaseFont();
				ReleaseResolvedFont();
				_baseFont = font;
				NotifyOwnerLayoutChanged();
			}
		}

		internal Font ResolveFont(Font otherFont)
		{
			Font source = _baseFont ?? otherFont;
			if (_floatValue == 1f && !HasFontOverrides())
			{
				return source;
			}
			if (_resolvedFont == null || _resolvedFontSource != source)
			{
				Font resolvedFont = BuildFont(source, _floatValue);
				ReleaseResolvedFont();
				_resolvedFont = resolvedFont;
				_resolvedFontSource = source;
			}
			return _resolvedFont;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAppearance()
		{
			Appearance = ComputeAppearance();
		}

		private bool HasFontOverrides()
		{
			if (_fontName != null && _fontName.Length > 0)
			{
				return true;
			}
			if (float.IsFinite(_fontSize) && _fontSize > 0f)
			{
				return true;
			}
			return _fontBold != TriState.Empty || _fontItalic != TriState.Empty || _fontStrikeout != TriState.Empty || _fontUnderline != TriState.Empty;
		}

		private Font BuildFont(Font source, float scale)
		{
			return BuildFont(source, _fontName, _fontSize, _fontBold, _fontItalic, _fontStrikeout, _fontUnderline, scale);
		}

		private static Font BuildFont(Font source, string fontName, float fontSize, TriState fontBold, TriState fontItalic, TriState fontStrikeout, TriState fontUnderline, float scale)
		{
			string name = (fontName != null && fontName.Length > 0) ? fontName : source.Name;
			// A size read from a layout is not validated by the FontSize setter, so a non-finite one is ignored here.
			float sizeInPoints = (float.IsFinite(fontSize) && fontSize > 0f) ? fontSize : source.SizeInPoints;
			FontStyle fontStyle = FontStyle.Regular;
			if (ResolveTriState(fontBold, source.Bold))
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ResolveTriState(fontItalic, source.Italic))
			{
				fontStyle |= FontStyle.Italic;
			}
			if (ResolveTriState(fontStrikeout, source.Strikeout))
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (ResolveTriState(fontUnderline, source.Underline))
			{
				fontStyle |= FontStyle.Underline;
			}
			return GridFontCache.Acquire(name, sizeInPoints * scale, fontStyle);
		}

		private static bool ResolveTriState(TriState value, bool inherited)
		{
			if (value == TriState.Empty)
			{
				return inherited;
			}
			return value != TriState.False;
		}

		private void ReleaseBaseFont()
		{
			if (_baseFont != null)
			{
				if (_isPooledBaseFont)
				{
					GridFontCache.Release(_baseFont);
				}
				else if (!_isUserFont)
				{
					GridTextBatch.FlushIfUses(_baseFont);
					_baseFont.Dispose();
				}
			}
			_baseFont = null;
			_isUserFont = false;
			_isPooledBaseFont = false;
		}

		private void ReleaseResolvedFont()
		{
			if (_resolvedFont != null)
			{
				GridFontCache.Release(_resolvedFont);
			}
			_resolvedFont = null;
			_resolvedFontSource = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFontUnderline()
		{
			ThrowIfReadOnly();
			FontUnderline = ComputeTriState();
		}

		private void NotifyOwnerLayoutChanged()
		{
			if (_owner != null)
			{
				_owner.InvalidateFormatStyleLayout();
			}
		}

		// A style kept for one paint pass by the grid: row painting disposes it as it does any merged style, so it ignores that.
		internal bool _isPassShared;

		internal void ReleasePassShared()
		{
			_isPassShared = false;
			DisposeResources();
		}

		internal void DisposeResources()
		{
			if (_isPassShared)
			{
				return;
			}
			if (_backBrush != null)
			{
				_backBrush.Dispose();
				_backBrush = null;
			}
			if (_gridFormatStyleBrush != null)
			{
				_gridFormatStyleBrush.Dispose();
				_gridFormatStyleBrush = null;
			}
			if (_baseFont != null && !_isUserFont)
			{
				ReleaseBaseFont();
			}
			ReleaseResolvedFont();
		}

		private bool HasGradient()
		{
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return _backgroundGradientMode != BackgroundGradientMode.Solid;
			}
			return false;
		}

		internal Brush GetOrCreateBackBrush(Rectangle rectangle)
		{
			if (_backBrush == null || HasGradient() || UsesSelectionBlend())
			{
				if (_backBrush != null)
				{
					_backBrush.Dispose();
					_backBrush = null;
				}
				if (UsesSelectionBlend())
				{
					_backBrush = GridSelectionGradient.CreateBrush(_selectionBlend, rectangle);
				}
				else if (HasGradient())
				{
					rectangle.X--;
					rectangle.Width++;
					if (rectangle.Width > 0 && rectangle.Height > 0)
					{
						_backBrush = GridHelper.CreateGradientBrush(BlendColors(GetBaseColor(), BackColor, GetEffectiveBlend()), BlendColors(GetBaseColor(), BackColorGradient, GetEffectiveGradientBlend()), rectangle, _backgroundGradientMode, _alpha, _backColorAlphaMode);
					}
				}
				else if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
				{
					_backBrush = GridHelper.CreateAlphaBrush(BlendColors(GetBaseColor(), BackColor, GetEffectiveBlend()), _alpha, _backColorAlphaMode);
				}
				else
				{
					_backBrush = GridHelper.CreateAlphaBrush(BackColor, _alpha, _backColorAlphaMode);
				}
			}
			return _backBrush;
		}

		internal Brush CreateBackBrush(Rectangle rectangle)
		{
			Brush result = null;
			if (UsesSelectionBlend())
			{
				result = GridSelectionGradient.CreateBrush(_selectionBlend, rectangle);
			}
			else if (!HasGradient())
			{
				result = ((GetEffectiveBlend() == 0f || BackColor.IsEmpty) ? GridHelper.CreateAlphaBrush(BackColor, _alpha, _backColorAlphaMode) : GridHelper.CreateAlphaBrush(BlendColors(GetBaseColor(), BackColor, GetEffectiveBlend()), _alpha, _backColorAlphaMode));
			}
			else
			{
				rectangle.X--;
				rectangle.Width++;
				rectangle.Y--;
				rectangle.Height++;
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					result = GridHelper.CreateGradientBrush(BlendColors(GetBaseColor(), BackColor, GetEffectiveBlend()), BlendColors(GetBaseColor(), BackColorGradient, GetEffectiveGradientBlend()), rectangle, _backgroundGradientMode, _alpha, _backColorAlphaMode);
				}
			}
			return result;
		}

		internal GridFormatStyle CreatePrintStyle(MxDataGridPrintDocument gridPrintDocument)
		{
			GridFormatStyle gridFormatStyle = new GridFormatStyle(this);
			if (!gridPrintDocument.PrintCellBackground)
			{
				gridFormatStyle.BackColor = Color.Empty;
				gridFormatStyle.BackColorGradient = Color.Empty;
				gridFormatStyle.BackgroundGradientMode = BackgroundGradientMode.Solid;
				gridFormatStyle.BackgroundImage = null;
				if ((double)gridFormatStyle._textColor.GetBrightness() > 0.85)
				{
					gridFormatStyle._textColor = Color.Black;
				}
			}
			if (gridPrintDocument.TranslateSystemColors)
			{
				_translateSystemColors = true;
				gridFormatStyle.BackColor = TranslateSystemColor(gridFormatStyle.BackColor);
				gridFormatStyle.BackColorGradient = TranslateSystemColor(gridFormatStyle._backColorGradient);
				gridFormatStyle.ForeColor = TranslateSystemColor(_textColor);
				if ((double)gridFormatStyle._textColor.GetBrightness() > 0.85 && !gridPrintDocument.PrintCellBackground)
				{
					gridFormatStyle._textColor = Color.Black;
				}
			}
			return gridFormatStyle;
		}

		internal Color TranslateSystemColor(Color color)
		{
			if (color.IsSystemColor)
			{
				switch (color.ToKnownColor())
				{
				case KnownColor.AppWorkspace:
				case KnownColor.ControlDark:
				case KnownColor.Desktop:
				case KnownColor.GrayText:
					return Color.Gray;
				case KnownColor.ActiveCaptionText:
				case KnownColor.HighlightText:
				case KnownColor.InactiveCaption:
				case KnownColor.Info:
				case KnownColor.Window:
					return Color.White;
				case KnownColor.ActiveBorder:
				case KnownColor.Control:
				case KnownColor.ControlLight:
				case KnownColor.InactiveBorder:
				case KnownColor.Menu:
				case KnownColor.ScrollBar:
					return Color.LightGray;
				case KnownColor.ActiveCaption:
				case KnownColor.ControlDarkDark:
				case KnownColor.ControlText:
				case KnownColor.Highlight:
				case KnownColor.HotTrack:
				case KnownColor.InactiveCaptionText:
				case KnownColor.InfoText:
				case KnownColor.MenuText:
				case KnownColor.WindowFrame:
				case KnownColor.WindowText:
					return Color.Black;
				case KnownColor.ControlLightLight:
					return Color.FromArgb(244, 244, 244);
				}
			}
			return color;
		}

		private Color BlendColors(Color color, Color otherColor, float floatValue)
		{
			if (color == Color.Empty)
			{
				return otherColor;
			}
			if (otherColor == Color.Empty)
			{
				return color;
			}
			int red = (int)((float)(int)color.R * floatValue + (float)(int)otherColor.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)otherColor.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)otherColor.B * (1f - floatValue));
			return Color.FromArgb(red, green, blue);
		}

		internal Brush GetForeBrush()
		{
			if (_gridFormatStyleBrush == null)
			{
				_gridFormatStyleBrush = GridHelper.CreateAlphaBrush(ForeColor, _alpha, _foreColorAlphaMode);
			}
			return _gridFormatStyleBrush;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTextAlignment()
		{
			ThrowIfReadOnly();
			TextAlignment = GetDefaultTextAlignment();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLineAlignment()
		{
			ThrowIfReadOnly();
			LineAlignment = ComputeTextAlignment();
		}

		internal Color GetDefaultBackColorGradient()
		{
			return _defaultBackColorGradient;
		}

		internal void SetDefaultBackColorGradient(Color backColorGradient)
		{
			if (_defaultBackColorGradient != backColorGradient)
			{
				if (_backColorGradient == _defaultBackColorGradient)
				{
					_backColorGradient = backColorGradient;
				}
				_defaultBackColorGradient = backColorGradient;
			}
		}

		internal Color GetDefaultForeColor()
		{
			return _baseForeColor;
		}

		internal void SetDefaultForeColor(Color foreColor)
		{
			if (_baseForeColor != foreColor)
			{
				if (_textColor == _baseForeColor)
				{
					_textColor = foreColor;
				}
				_baseForeColor = foreColor;
			}
		}

		internal Color GetDefaultBackColor()
		{
			return _baseBackColor;
		}

		internal void SetDefaultBackColor(Color backColor)
		{
			if (_baseBackColor != backColor)
			{
				if (_backgroundColor == _baseBackColor)
				{
					_backgroundColor = backColor;
				}
				_baseBackColor = backColor;
				_hasExplicitBackColor = _backgroundColor != _baseBackColor;
			}
		}

		private Color GetPredefinedBackColor()
		{
			if (_predefinedStyle != PredefinedStyle.None && _owner != null && _owner.DataGrid != null)
			{
				Color color = _owner.DataGrid.GetPredefinedStyleBackColor(_predefinedStyle);
				if (color != Color.Empty)
				{
					return color;
				}
			}
			switch (_predefinedStyle)
			{
			case PredefinedStyle.RowFormatStyle:
				return SystemColors.Window;
			case PredefinedStyle.HeaderFormatStyle:
			case PredefinedStyle.GroupByBoxInfoFormatStyle:
			case PredefinedStyle.GroupRowFormatStyle:
			case PredefinedStyle.CardCaptionFormatStyle:
			case PredefinedStyle.SelectedInactiveFormatStyle:
				return SystemColors.Control;
			case PredefinedStyle.GroupByBoxFormatStyle:
				return SystemColors.ControlDark;
			case PredefinedStyle.AlternatingRowFormatStyle:
				return SystemColors.Control;
			case PredefinedStyle.SelectedFormatStyle:
				return SystemColors.Highlight;
			default:
				return Color.Empty;
			}
		}

		private Appearance ComputeAppearance()
		{
			PredefinedStyle predefinedStyle = _predefinedStyle;
			if (predefinedStyle == PredefinedStyle.HeaderFormatStyle)
			{
				return Appearance.RaisedLight;
			}
			return Appearance.Empty;
		}

		private Color GetPredefinedBackColorGradient()
		{
			if (_predefinedStyle != PredefinedStyle.None && _owner != null && _owner.DataGrid != null)
			{
				return _owner.DataGrid.GetPredefinedStyleGradientColor(_predefinedStyle);
			}
			return Color.Empty;
		}

		private Color GetPredefinedForeColor()
		{
			if (_predefinedStyle != PredefinedStyle.None && _owner != null && _owner.DataGrid != null)
			{
				Color color = _owner.DataGrid.GetPredefinedStyleForeColor(_predefinedStyle);
				if (color != Color.Empty)
				{
					return color;
				}
			}
			switch (_predefinedStyle)
			{
			case PredefinedStyle.RowFormatStyle:
				return SystemColors.WindowText;
			case PredefinedStyle.HeaderFormatStyle:
			case PredefinedStyle.GroupRowFormatStyle:
			case PredefinedStyle.CardCaptionFormatStyle:
			case PredefinedStyle.SelectedInactiveFormatStyle:
				return SystemColors.ControlText;
			case PredefinedStyle.SelectedFormatStyle:
				return SystemColors.HighlightText;
			case PredefinedStyle.PreviewRowFormatStyle:
			case PredefinedStyle.LinkFormatStyle:
				return Color.Blue;
			case PredefinedStyle.RowWithErrorsFormatStyle:
				return Color.Red;
			default:
				return Color.Empty;
			}
		}

		private TriState ComputeTriState()
		{
			if (_predefinedStyle == PredefinedStyle.LinkFormatStyle)
			{
				return TriState.True;
			}
			return TriState.Empty;
		}

		private TextAlignment ComputeTextAlignment()
		{
			switch (_predefinedStyle)
			{
			case PredefinedStyle.RowFormatStyle:
			case PredefinedStyle.AlternatingRowFormatStyle:
			case PredefinedStyle.GroupByBoxInfoFormatStyle:
			case PredefinedStyle.GroupRowFormatStyle:
			case PredefinedStyle.CardCaptionFormatStyle:
			case PredefinedStyle.CardColumnHeaderFormatStyle:
				return TextAlignment.Center;
			default:
				return TextAlignment.Empty;
			}
		}

		private TextAlignment GetDefaultTextAlignment()
		{
			switch (_predefinedStyle)
			{
			case PredefinedStyle.RowFormatStyle:
			case PredefinedStyle.AlternatingRowFormatStyle:
			case PredefinedStyle.GroupByBoxInfoFormatStyle:
			case PredefinedStyle.GroupRowFormatStyle:
			case PredefinedStyle.CardCaptionFormatStyle:
			case PredefinedStyle.CardColumnHeaderFormatStyle:
				return TextAlignment.Near;
			default:
				return TextAlignment.Empty;
			}
		}

		internal float GetEffectiveBlend()
		{
			if (_blend == -1f)
			{
				if (DataGrid != null)
				{
					return DataGrid.DefaultBlend;
				}
				return 0f;
			}
			return _blend;
		}

		internal float GetEffectiveGradientBlend()
		{
			if (_blendGradient == -1f)
			{
				if (DataGrid != null)
				{
					return DataGrid.DefaultBlend;
				}
				return 0f;
			}
			return _blendGradient;
		}

		internal Color GetBaseColor()
		{
			Color color = _defaultBaseColor;
			if (color == Color.Empty)
			{
				color = SystemColors.Window;
			}
			if (_translateSystemColors)
			{
				color = TranslateSystemColor(color);
			}
			return color;
		}

		internal Color GetEffectiveBackColor()
		{
			if (BackColor.IsEmpty && GetEffectiveBlend() == 0f)
			{
				return BackColor;
			}
			Color color = ((GetEffectiveBlend() != 0f) ? BlendColors(GetBaseColor(), BackColor, GetEffectiveBlend()) : BackColor);
			switch (_backColorAlphaMode)
			{
			case AlphaMode.Transparent:
				color = Color.FromArgb(0, color);
				break;
			case AlphaMode.UseAlpha:
				if (_alpha != 255 && _alpha != -1)
				{
					color = Color.FromArgb(_alpha, color);
				}
				break;
			}
			return color;
		}

		internal bool IsTranslucent()
		{
			if (_backColorAlphaMode == AlphaMode.Transparent || _backColorAlphaMode == AlphaMode.UseAlpha)
			{
				return true;
			}
			if (!_backgroundColor.IsEmpty && _backgroundColor.A != byte.MaxValue)
			{
				return true;
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty && _backgroundGradientMode != BackgroundGradientMode.Solid && !_backColorGradient.IsEmpty && _backColorGradient.A != byte.MaxValue)
			{
				return true;
			}
			return false;
		}
	}
}
