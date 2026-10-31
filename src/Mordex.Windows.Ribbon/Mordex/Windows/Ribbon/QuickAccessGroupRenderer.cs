using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessGroupRenderer : CommandBaseRenderer, IDropDownItem
	{
		private QuickAccessToolbarRenderer _quickAccessToolbar;

		private RibbonGroup _group;

		private bool _isPopupOpen;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				return AccessibleRole.ToolBar;
			}
		}

		public override string AccessibleDescription
		{
			get
			{
				if (_group != null)
				{
					if (_group.AccessibleDescription != null && _group.AccessibleDescription.Length > 0)
					{
						return GetCommand().AccessibleDescription;
					}
					if (_group.SuperTipSettings != null)
					{
						string text = _group.SuperTipSettings.Text;
						if (text != null && text.Length > 0)
						{
							return text;
						}
					}
				}
				return base.AccessibleDescription;
			}
		}

		public override string AccessibleName
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleName != null && GetCommand().AccessibleName.Length > 0)
				{
					return GetCommand().AccessibleName;
				}
				if (_group != null && _group.Text != null)
				{
					string text = _group.Text.Replace("&", "");
					if (text.Length > 0)
					{
						return text;
					}
				}
				return base.AccessibleName;
			}
		}

		public override string DefaultAction => "Open";

		public QuickAccessGroupRenderer(RibbonGroup group, ICommandRenderHost owner, QuickAccessToolbarRenderer qaBarOwner)
			: base(group, owner, null)
		{
			_quickAccessToolbar = qaBarOwner;
			_group = group;
		}

		public Size CreateSize()
		{
			return new Size(16, 16);
		}

		public override void DoDefaultAction()
		{
			ShowCollapsedPopup();
		}

		public override void Invalidate()
		{
			if (_quickAccessToolbar != null)
			{
				_quickAccessToolbar.Invalidate(update: true);
			}
		}

		public bool IsPopupOpen()
		{
			return _isPopupOpen;
		}

		public void SetPopupOpen(bool ribbonGroupRendererBoolean)
		{
			if (_isPopupOpen != ribbonGroupRendererBoolean)
			{
				_isPopupOpen = ribbonGroupRendererBoolean;
				Invalidate();
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				_hasImageContent = RibbonDrawingUtils.HasImageSource(_group.Icon, _group.Image, GetRenderHost().GetImageList(GetCommand()), _group.ImageIndex, _group.ImageKey);
				_fullItemWidth = CreateSize().Width + 6;
				SetLayoutWidth(_fullItemWidth);
			}
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() != null)
			{
				if (_hasImageContent)
				{
					int iconWidth = CreateSize().Width;
					int height = CreateSize().Height;
					_imageBounds = new Rectangle(left + 2, top + (itemHeight - height) / 2, iconWidth, height);
				}
				_bounds = new Rectangle(left, top, width, itemHeight);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawImageFromSources(graphics, _group.Icon, _group.Image, _group.ImageIndex, _group.ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, _group.IsEnabled, GetRenderHost().Ribbon.IsGlassCaptionActive(), colorScheme);
			}
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				ShowCollapsedPopup();
			}
		}

		public void ShowCollapsedPopup()
		{
			if (!IsPopupOpen())
			{
				CollapsedGroupPopup popup = new CollapsedGroupPopup(GetRenderHost(), _group.Commands, this, _group);
				SetPopupOpen(true);
				popup.ShowAt(_quickAccessToolbar.PointToScreen(this, new Point(_bounds.X, _bounds.Top)));
				SetPopupOpen(false);
			}
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int keyTipWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			int x = _bounds.X + (_bounds.Width - keyTipWidth) / 2;
			int y = _bounds.Bottom - 10;
			return _quickAccessToolbar.PointToScreen(this, new Point(x, y));
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			ShowCollapsedPopup();
		}
	}
}
