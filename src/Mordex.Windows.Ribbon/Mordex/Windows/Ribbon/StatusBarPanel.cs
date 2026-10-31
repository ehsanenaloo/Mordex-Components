using System.ComponentModel;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public class StatusBarPanel : ButtonCommand
	{
		private HorizontalAlignment _textAlignment;

		private HorizontalAlignment _imageAlignment;

		private StatusBarPanelAutoSize _autoSizeMode = StatusBarPanelAutoSize.None;

		internal int _width = 100;

		private PanelBehavior _panelBehavior;

		internal override CommandType CommandType => CommandType.StatusBarPanel;

		[DefaultValue(PanelBehavior.Button)]
		[Description("Specifies the appearance and behavior of the StatusBarPanel")]
		public PanelBehavior PanelBehavior
		{
			get
			{
				return _panelBehavior;
			}
			set
			{
				if (_panelBehavior != value)
				{
					_panelBehavior = value;
					NotifyTextChanged("PanelBehabior");
				}
			}
		}

		[Localizable(false)]
		[Description("Indicates whether the status bar panel is autosized.")]
		[DefaultValue(StatusBarPanelAutoSize.None)]
		public StatusBarPanelAutoSize AutoSize
		{
			get
			{
				return _autoSizeMode;
			}
			set
			{
				if (_autoSizeMode != value)
				{
					_autoSizeMode = value;
					NotifyTextChanged("AutoSize");
				}
			}
		}

		[DefaultValue(100)]
		[Description("Represents the width of the status bar panel whithin the UIStatusBar control.")]
		[Localizable(true)]
		public int Width
		{
			get
			{
				return _width;
			}
			set
			{
				if (_width != value)
				{
					_width = value;
					NotifyTextChanged("Width");
				}
			}
		}

		[Description("Determines the horizontal alignment of the text.")]
		[DefaultValue(HorizontalAlignment.Left)]
		public HorizontalAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (_textAlignment != value)
				{
					_textAlignment = value;
					NotifyTextChanged("TextAlignment");
				}
			}
		}

		[DefaultValue(HorizontalAlignment.Left)]
		[Description("Determines the horizontal alignment of the image.")]
		public HorizontalAlignment ImageAlignment
		{
			get
			{
				return _imageAlignment;
			}
			set
			{
				if (_imageAlignment != value)
				{
					_imageAlignment = value;
					NotifyTextChanged("ImageAlignment");
				}
			}
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_panelBehavior = PanelBehavior.Button;
			_width = 100;
			_textAlignment = HorizontalAlignment.Left;
			_imageAlignment = HorizontalAlignment.Left;
		}
	}
}
