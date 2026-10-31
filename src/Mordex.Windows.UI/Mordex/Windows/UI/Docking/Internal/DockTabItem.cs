using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockTabItem
	{
		private const int ImageTextGap = 4;

		private const int TextTrailingGap = 4;

		private const int CrossAxisPadding = 4;

		private DockPanelBase _panel;

		private ItemOrientation _itemOrientation;

		private CommandImageSource _imageSource;

		private Point _location;

		private string _text;

		internal bool _isHot;

		internal bool _isSelected;

		internal bool IsSelected => (_parentControl is MxDockPanelGroup group) ? group.SelectedPanel == _panel : _isSelected;

		private Control _parentControl;

		private AccessibleObject _accessibleObject;

		private bool _showImage;

		private bool _showText;

		private Size _itemSize;

		internal DockTabItem(DockPanelBase panel, Control parentControl)
		{
			_panel = panel;
			_text = panel.Text;
			SetImageSource(panel.GetImageSource());
			_itemOrientation = ItemOrientation.Horizontal;
			_location = Point.Empty;
			_showImage = true;
			_showText = true;
			_parentControl = parentControl;
		}

		internal Control GetParentControl()
		{
			return _parentControl;
		}

		internal DockPanelBase GetPanel()
		{
			return _panel;
		}

		public string GetAccessibleName()
		{
			if (_panel != null)
			{
				string text = _panel.AccessibleName;
				if (text == null || text.Length == 0)
				{
					text = _panel.Text;
				}
				if (text == null || text.Length == 0)
				{
					text = _panel.Name;
				}
				return text;
			}
			return "";
		}

		public AccessibleStates GetAccessibleStates()
		{
			AccessibleStates accessibleStates = AccessibleStates.Focusable;
			if (IsSelected)
			{
				accessibleStates |= AccessibleStates.Checked;
			}
			if (_isHot)
			{
				accessibleStates |= AccessibleStates.HotTracked;
			}
			return accessibleStates;
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new DockPanelItemAccessibleObject(this);
			}
			return _accessibleObject;
		}

		public string GetDefaultActionName()
		{
			if (GetPanel() != null)
			{
				return "Switch";
			}
			return "Press";
		}

		public void SelectPanel()
		{
			if (_parentControl != null && _parentControl is MxDockPanelGroup uiPanelGroup)
			{
				uiPanelGroup.SelectedPanel = GetPanel();
			}
		}

		internal Point GetLocation()
		{
			return _location;
		}

		internal void SetLocation(Point point)
		{
			_location = point;
		}

		internal CommandImageSource GetImageSource()
		{
			return _imageSource;
		}

		internal void SetImageSource(CommandImageSource imageSource)
		{
			_imageSource = imageSource;
		}

		internal bool GetShowImage()
		{
			return _showImage;
		}

		internal void SetShowImage(bool showImage)
		{
			_showImage = showImage;
		}

		internal bool GetShowText()
		{
			return _showText;
		}

		internal void SetShowText(bool showText)
		{
			_showText = showText;
		}

		internal Size MeasureContent(Graphics graphics, FormatStyle uiFormatStyle, Font baseFont, int integer, ItemOrientation itemOrientation)
		{
			if (_panel == null || _panel.PanelManager == null)
			{
				return Size.Empty;
			}
			_itemOrientation = itemOrientation;
			Font font = uiFormatStyle.ResolveFont(baseFont);
			bool hasText = _showText && _text != null && _text.Length != 0;
			Size size = Size.Empty;
			if (hasText)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_panel.PanelManager.UseCompatibleTextRendering);
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				textLayoutRenderer.SetWordWrap(false);
				if (itemOrientation == ItemOrientation.Vertical)
				{
					textLayoutRenderer.SetVertical(true);
				}
				SizeF sizeF = textLayoutRenderer.MeasureText(graphics, _text, font, 0);
				size = new Size((int)sizeF.Width + 1, (int)sizeF.Height + 1);
				textLayoutRenderer.ResetCache();
			}
			Size imageSize = Size.Empty;
			if (_showImage && GetPanel().PanelManager != null)
			{
				if (!GetImageSource().IsEmpty())
				{
					imageSize = _panel.PanelManager.GetPanelImageSize(GetImageSource());
				}
				else if (!hasText)
				{
					imageSize = _panel.PanelManager.GetSmallImageSize();
				}
			}
			Size result;
			if (_itemOrientation == ItemOrientation.Horizontal)
			{
				int totalWidth = 0;
				int trailingGap = 0;
				totalWidth += imageSize.Width;
				if (imageSize.Width > 0)
				{
					totalWidth += ImageTextGap;
					if (size.Width == 0)
					{
						trailingGap = ImageTextGap;
					}
				}
				totalWidth += size.Width;
				if (size.Width > 0)
				{
					totalWidth += TextTrailingGap;
					trailingGap = TextTrailingGap;
				}
				totalWidth += trailingGap;
				int height = size.Height;
				if (height < imageSize.Height)
				{
					height = imageSize.Height;
				}
				height += CrossAxisPadding;
				result = new Size(totalWidth, height);
			}
			else
			{
				int totalHeight = 0;
				int trailingGap = 0;
				totalHeight += imageSize.Height;
				if (imageSize.Height > 0)
				{
					totalHeight += ImageTextGap;
					if (size.Height == 0)
					{
						trailingGap = ImageTextGap;
					}
				}
				totalHeight += size.Height;
				if (size.Height > 0)
				{
					totalHeight += TextTrailingGap;
					trailingGap = TextTrailingGap;
				}
				totalHeight += trailingGap;
				int width = size.Width;
				if (width < imageSize.Width)
				{
					width = imageSize.Width;
				}
				width += CrossAxisPadding;
				result = new Size(width, totalHeight);
			}
			return result;
		}

		internal Size GetItemSize()
		{
			return _itemSize;
		}

		internal void SetItemSize(Size size)
		{
			_itemSize = size;
		}

		internal Rectangle GetBounds()
		{
			return new Rectangle(_location, _itemSize);
		}
	}
}
