using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Tabs
{
	[Designer("Mordex.Windows.UI.Design.TabPageDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(false)]
	public class MxTabPage : ContainerControl, IMordexXmlLayoutsSupport
	{
		private const int WmHScroll = 0x0114;

		private const int WmVScroll = 0x0115;

		private const int WmMouseWheel = 0x020A;

		// Size used for a tab that shows only an image but has none.
		private static readonly Size DefaultImageSize = new Size(16, 16);

		// Space around the text along the tab strip and across it.
		private const int TextLengthPadding = 4;

		private const int TextThicknessPadding = 5;

		private const int ImagePadding = 6;

		private const int ImageThicknessPadding = 4;

		private const int TabLengthPadding = 6;

		private const int TabThicknessPadding = 2;

		// Office like tabs are slanted and need extra room along the strip.
		private const int OfficeTabExtraLength = 6;

		private Container _container = new Container();

		private TabPageCollection _ownerCollection;

		private bool _allowClose = true;

		private string _toolTipText = "";

		private string _key = "";

		internal int _index = -1;

		private Icon _icon;

		internal bool _isSelected;

		private Image _image;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private StateStyles _stateStyles;

		private bool _isTabVisible = true;

		private FormatStyle _panelFormatStyle;

		internal bool _isSizeDirty = true;

		private Size _tabSize = Size.Empty;

		private Bitmap _themeBackgroundCache;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new int TabIndex
		{
			get
			{
				return base.TabIndex;
			}
			set
			{
				base.TabIndex = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
			}
		}

		[Description("Specifies whether the user can close the tab page using the close button of the UITab control")]
		[DefaultValue(true)]
		public bool AllowClose
		{
			get
			{
				return _allowClose;
			}
			set
			{
				if (_allowClose != value)
				{
					_allowClose = value;
					if (_isSelected && _ownerCollection != null && _ownerCollection._tabControl.ShowCloseButton)
					{
						_ownerCollection._tabControl.UpdateCloseButtonEnabled();
					}
					if (_ownerCollection != null && _ownerCollection._tabControl._tabCloseLayout)
					{
						InvalidateTabSize();
					}
					_ownerCollection?._tabControl.InvalidateCaptionTabs();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Color BackColor
		{
			get
			{
				if (base.BackColor.A != byte.MaxValue)
				{
					return GetPaintBackColor();
				}
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[Localizable(true)]
		[Description("The text of the ToolTip for the tab page.")]
		[DefaultValue("")]
		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
			}
		}

		[DefaultValue("")]
		[Description("Represents the key used to identify an object in a collection")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[Browsable(false)]
		public int Index => _index;

		[Description("Represents the Icon displayed on this tab.")]
		[Localizable(true)]
		[DefaultValue(null)]
		public Icon Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					InvalidateTabSize();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		[Browsable(false)]
		public bool Selected
		{
			get
			{
				if (TabControl != null)
				{
					if (Equals(TabControl.SelectedTab))
					{
						return true;
					}
					return false;
				}
				return _isSelected;
			}
			set
			{
				if (Selected != value)
				{
					if (TabControl != null)
					{
						TabControl.ChangeSelection(this);
					}
					else
					{
						_isSelected = value;
					}
				}
			}
		}

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Description("Represents the Image displayed on this tab.")]
		[DefaultValue(null)]
		[Localizable(true)]
		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					InvalidateTabSize();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed on this tab")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[DefaultValue(-1)]
		[Localizable(true)]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					InvalidateTabSize();
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents the key accessor for the image in the ImageList displayed on this tab")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Localizable(true)]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					InvalidateTabSize();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the control's page in the TabControl.")]
		public StateStyles StateStyles
		{
			get
			{
				if (_stateStyles == null)
				{
					_stateStyles = new StateStyles(TabControl);
				}
				return _stateStyles;
			}
		}

		[Localizable(true)]
		[DefaultValue(true)]
		[Description("Indicates whether the tab page should be displayed in the Tab control.")]
		public bool TabVisible
		{
			get
			{
				return _isTabVisible;
			}
			set
			{
				if (_isTabVisible == value)
				{
					return;
				}
				if (!value && TabControl != null)
				{
					TabCancelEventArgs e = new TabCancelEventArgs(null, this);
					TabControl.RaiseClosingTab(e);
					if (e.Cancel)
					{
						return;
					}
				}
				_isTabVisible = value;
				if (!value && TabControl != null)
				{
					TabControl.RaiseTabClosed(this);
				}
				if (!_isTabVisible && Selected && TabControl != null)
				{
					TabControl.SelectNearestVisibleTab(Index);
					if (Selected)
					{
						TabControl.SelectedTab = null;
					}
				}
				InvalidateTabSize();
			}
		}

		[Description("Represents the format style properties used to draw the background of this control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle PanelFormatStyle
		{
			get
			{
				if (_panelFormatStyle == null)
				{
					_panelFormatStyle = new FormatStyle(TabControl);
				}
				return _panelFormatStyle;
			}
		}

		[Browsable(false)]
		public MxTabControl TabControl
		{
			get
			{
				if (_ownerCollection != null)
				{
					return _ownerCollection._tabControl;
				}
				return null;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_allowClose = (bool)LayoutReader.GetPropertyValue(reader, "AllowClose", typeof(bool), true);
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), "");
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), "");
			_panelFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "PanelFormatStyle", typeof(FormatStyle), null);
			if (_panelFormatStyle != null && _ownerCollection != null)
			{
				_panelFormatStyle._owner = _ownerCollection._tabControl;
			}
			_stateStyles = null;
			reader["StateStyles"]?.DeserializeObject(StateStyles);
			_isTabVisible = (bool)LayoutReader.GetPropertyValue(reader, "TabVisible", typeof(bool), true);
			Text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			_toolTipText = (string)LayoutReader.GetPropertyValue(reader, "ToolTipText", typeof(string), "");
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!_allowClose)
			{
				info.AddValue("AllowClose", _allowClose);
			}
			if (_icon != null)
			{
				info.AddValue("Icon", _icon);
			}
			if (_image != null)
			{
				info.AddValue("Image", _image);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_key.Length > 0)
			{
				info.AddValue("Key", _key);
			}
			if (_panelFormatStyle != null && !_panelFormatStyle.IsDefaultStyle())
			{
				info.AddValue("PanelFormatStyle", _panelFormatStyle);
			}
			if (_stateStyles != null && _stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (!_isTabVisible)
			{
				info.AddValue("TabVisible", _isTabVisible);
			}
			if (Text.Length > 0)
			{
				info.AddValue("Text", Text);
			}
			if (_toolTipText != null && _toolTipText.Length > 0)
			{
				info.AddValue("ToolTipText", _toolTipText);
			}
			if (base.Name.Length > 0)
			{
				info.AddValue("Name", base.Name);
			}
		}

		public MxTabPage()
			: this("", -1)
		{
		}

		public MxTabPage(string text)
			: this(text, -1)
		{
		}

		public MxTabPage(string text, int imageIndex)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			Text = text;
			ImageIndex = imageIndex;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
				{
					_container.Dispose();
				}
				ClearBackgroundCache();
				if (_ownerCollection != null)
				{
					Form form = RenderingHelper.FindForm(this);
					if (form != null && !form.Disposing && form.Visible)
					{
						_ownerCollection.Remove(this);
					}
				}
			}
			base.Dispose(disposing);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmHScroll || m.Msg == WmVScroll || m.Msg == WmMouseWheel)
			{
				Invalidate(invalidateChildren: true);
				Update();
			}
			base.WndProc(ref m);
		}

		internal TabPageCollection GetOwnerCollection()
		{
			return _ownerCollection;
		}

		internal void SetOwnerCollection(TabPageCollection collection)
		{
			_ownerCollection = collection;
			if (_stateStyles != null)
			{
				if (_ownerCollection != null)
				{
					_stateStyles._owner = _ownerCollection._tabControl;
				}
				else
				{
					_stateStyles._owner = null;
				}
			}
			if (_panelFormatStyle != null)
			{
				if (_ownerCollection != null)
				{
					_panelFormatStyle._owner = _ownerCollection._tabControl;
				}
				else
				{
					_panelFormatStyle._owner = null;
				}
			}
		}

		internal void MeasureTab(Graphics graphics)
		{
			bool ownsGraphics = false;
			if (!_isTabVisible)
			{
				return;
			}
			if (TabControl.SizeMode == TabSizeMode.Fixed)
			{
				if (TabControl.TabStripAlignment == TabStripAlignment.Top || TabControl.TabStripAlignment == TabStripAlignment.Bottom)
				{
					if (TabControl.ItemSize.Width > 0 && TabControl.ItemSize.Height > 0)
					{
						TabControl.GrowTabThickness(TabControl.ItemSize.Height);
						_tabSize = TabControl.ItemSize;
						_isSizeDirty = false;
						return;
					}
				}
				else if (TabControl.ItemSize.Height > 0 && TabControl.ItemSize.Width > 0)
				{
					TabControl.GrowTabThickness(TabControl.ItemSize.Width);
					_tabSize = TabControl.ItemSize;
					_isSizeDirty = false;
					return;
				}
			}
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			_tabSize = Size.Empty;
			bool showText = true;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!TabControl.UseCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetNoPadding(true);
			if (!TabControl._isTextHorizontal)
			{
				textLayoutRenderer.SetVertical(true);
			}
			textLayoutRenderer.SetShowHotkeyPrefix(true);
			if (TabControl.TabDisplay == TabDisplay.Image)
			{
				showText = false;
			}
			else if (TabControl.TabDisplay == TabDisplay.ImageAndTextOnSelected && !Selected)
			{
				showText = false;
			}
			int tabThickness = 0;
			if (showText)
			{
				Font font = TabControl.Font;
				StateStyles stateStyles = TabControl.CreateTabStateStyles(false);
				stateStyles = stateStyles.CreateMergedStateStyles(StateStyles);
				TabPageRenderer tabRenderer = TabControl._pageRenderers.FindTabPageItem(this);
				if (tabRenderer != null)
				{
					FormatStyle hotFormatStyle = TabControl.CreateTabFormatStyle(stateStyles, base.Enabled, tabRenderer.IsHot(), Selected);
					font = hotFormatStyle.ResolveFont(TabControl.Font);
				}
				else
				{
					FormatStyle normalFormatStyle = TabControl.CreateTabFormatStyle(stateStyles, base.Enabled, false, Selected);
					font = normalFormatStyle.ResolveFont(TabControl.Font);
				}
				_tabSize = textLayoutRenderer.MeasureText(graphics, Text, font, 0).ToSize();
				if (_tabSize.IsEmpty)
				{
					_tabSize = new Size(font.Height, font.Height);
				}
				if (TabControl._isHorizontalStrip)
				{
					tabThickness = _tabSize.Height + TextThicknessPadding;
					_tabSize.Width += TextLengthPadding;
				}
				else
				{
					tabThickness = _tabSize.Width + TextThicknessPadding;
					_tabSize.Height += TextLengthPadding;
				}
				stateStyles.DisposeFormatStyleResources();
			}
			if (TabControl.TabDisplay != TabDisplay.Text)
			{
				Size imageSize = Size.Empty;
				if (Icon != null)
				{
					imageSize = (TabControl.ImageSize.IsEmpty ? _icon.Size : TabControl.ImageSize);
				}
				else
				{
					Image image = TabControl.GetTabImage(this);
					if (image != null)
					{
						imageSize = (TabControl.ImageSize.IsEmpty ? image.Size : TabControl.ImageSize);
					}
				}
				if (imageSize.IsEmpty && (TabControl.TabDisplay == TabDisplay.Image || (TabControl.TabDisplay == TabDisplay.ImageAndTextOnSelected && !Selected)))
				{
					imageSize = DefaultImageSize;
				}
				if (!imageSize.IsEmpty)
				{
					_tabSize = new Size(GetTabSize().Width + imageSize.Width + ImagePadding, GetTabSize().Height + imageSize.Height + ImagePadding);
					if (TabControl._isHorizontalStrip != TabControl._isTextHorizontal)
					{
						tabThickness = ((!TabControl._isTextHorizontal) ? _tabSize.Height : _tabSize.Width);
					}
					else
					{
						int imageThickness = (!TabControl._isTextHorizontal) ? (imageSize.Width + ImageThicknessPadding) : (imageSize.Height + ImageThicknessPadding);
						if (imageThickness > tabThickness)
						{
							tabThickness = imageThickness;
						}
					}
				}
			}
			if (TabControl._isTextHorizontal)
			{
				_tabSize.Height += TabThicknessPadding;
				if (TabControl.ItemSize.Height > 0)
				{
					_tabSize.Height = TabControl.ItemSize.Height;
				}
				_tabSize.Width += TabLengthPadding;
				if (TabControl._tabCloseLayout && _allowClose)
				{
					_tabSize.Width += TabControl.TabCloseExtent;
				}
			}
			else
			{
				_tabSize.Height += TabLengthPadding;
				_tabSize.Width += TabThicknessPadding;
				if (TabControl.ItemSize.Width > 0)
				{
					_tabSize.Width = TabControl.ItemSize.Width;
				}
			}
			if (TabControl.SizeMode == TabSizeMode.Fixed)
			{
				if (TabControl._isTextHorizontal)
				{
					if (TabControl.ItemSize.Width > 0)
					{
						_tabSize.Width = TabControl.ItemSize.Width;
					}
					if (TabControl.ItemSize.Height > 0)
					{
						tabThickness = TabControl.ItemSize.Height;
						_tabSize.Height = tabThickness;
					}
				}
				else
				{
					if (TabControl.ItemSize.Height > 0)
					{
						_tabSize.Height = TabControl.ItemSize.Height;
					}
					if (TabControl.ItemSize.Width > 0)
					{
						tabThickness = TabControl.ItemSize.Width;
						_tabSize.Width = tabThickness;
					}
				}
			}
			TabControl.GrowTabThickness(tabThickness);
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
			textLayoutRenderer.ResetCache();
			_isSizeDirty = false;
		}

		internal Size GetTabSize()
		{
			return _tabSize;
		}

		internal void InvalidateTabSize()
		{
			if (_ownerCollection != null)
			{
				TabControl.ResetTabSizes();
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (TabControl != null)
			{
				TabControl.LayoutTabs(null);
				TabControl.Invalidate(invalidateChildren: true);
				TabControl.InvalidateCaptionTabs();
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			InvalidateTabSize();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			InvalidateTabSize();
		}

		internal Color GetEffectiveBackColor()
		{
			Color backColor = base.BackColor;
			if (backColor == SystemColors.Control && TabControl != null && (!VisualStyleSupport.IsThemingUsable(false)))
			{
				return RenderingHelper.GetSchemeColor(ColorName.Control);
			}
			return backColor;
		}

		private Color GetPaintBackColor()
		{
			Color color = GetEffectiveBackColor();
			if (color == Color.Transparent)
			{
				for (Control control = RenderingHelper.GetParentControl(this); control != null; control = RenderingHelper.GetParentControl(control.Parent))
				{
					color = control.BackColor;
					if (color != Color.Transparent)
					{
						return color;
					}
				}
			}
			return GetEffectiveBackColor();
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (TabControl != null)
			{
				if (TabControl._isLayoutDirty)
				{
					TabControl.LayoutTabs(e.Graphics);
				}
				{
					Color pageBack = base.BackColor == SystemColors.Control ? MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.TabActive) : base.BackColor;
					e.Graphics.FillRectangle(GdiCache.Brush(pageBack), base.ClientRectangle);
					return;
				}
			}
			else
			{
				base.OnPaintBackground(e);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (Selected)
			{
				if (TabControl != null)
				{
					TabControl.Invalidate();
				}
				Invalidate(invalidateChildren: true);
			}
			ClearBackgroundCache();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			ClearBackgroundCache();
		}

		internal void ClearBackgroundCache()
		{
			if (_themeBackgroundCache != null)
			{
				_themeBackgroundCache.Dispose();
				_themeBackgroundCache = null;
			}
		}
	}
}
