using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		public void SetNavigationBarStyle(NavigationBarStyle style)
		{
			if (!Enum.IsDefined(typeof(NavigationBarStyle), style))
			{
				throw new InvalidEnumArgumentException();
			}
			_foreColorAlphaMode = AlphaMode.Empty;
			_backColorAlphaMode = AlphaMode.Empty;
			switch (style)
			{
			case NavigationBarStyle.Office2000:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyOffice2000Style();
				break;
			case NavigationBarStyle.OfficeXP:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyOfficeXpStyle();
				break;
			case NavigationBarStyle.OutlookExpress:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyOutlookExpressStyle();
				break;
			case NavigationBarStyle.VisualStudio:
				UpdateAlpha(NavigationBarView.SmallIcons, Alignment.Near);
				ApplyVisualStudioStyle();
				break;
			case NavigationBarStyle.CommonDialog:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyCommonDialogStyle();
				break;
			case NavigationBarStyle.Office2003:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyOffice2003Style();
				break;
			case NavigationBarStyle.Office2007:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyOffice2007Style();
				break;
			case NavigationBarStyle.Office2010:
				UpdateAlpha(NavigationBarView.LargeIcons, Alignment.Center);
				ApplyOffice2010Style();
				break;
			case NavigationBarStyle.VS2005:
				UpdateAlpha(NavigationBarView.SmallIcons, Alignment.Near);
				ApplyVs2005Style();
				break;
			case NavigationBarStyle.VS2010:
				UpdateAlpha(NavigationBarView.SmallIcons, Alignment.Near);
				ApplyVs2005Style();
				break;
			}
			RefreshFormatStyles();
			InvalidateLayout(true);
		}

		private void UpdateAlpha(NavigationBarView navigationBarView, Alignment alignment)
		{
			GroupAppearance = GroupAppearance.Light3D;
			HeaderGroupVisible = true;
			BorderStyle = BorderStyle.Light3D;
			ItemsStateStyles.Reset();
			GroupsBackgroundFormatStyle.Reset();
			GroupsStateStyles.Reset();
			ResetBlendColor();
			_alpha = -1;
			DragStyle = DragStyle.DropLine;
			foreach (NavigationGroup item in _groups)
			{
				item.View = navigationBarView;
				item.ImageAlignment = Alignment.Near;
				item.TextAlignment = alignment;
				item.ResetItemsStateStyles();
				item.ResetStateStyles();
				item.ResetBackgroundFormatStyle();
				foreach (NavigationItem childItem in item.Items)
				{
					childItem.ImageAlignment = Alignment.Near;
					childItem.TextAlignment = Alignment.Near;
					childItem.ResetStateStyles();
				}
			}
		}

		private void ApplyVisualStudioStyle()
		{
			_dragStyle = DragStyle.InvertedRect;
			ScrollLocation = ScrollLocation.GroupsLevel;
			KeepSelection = true;
			_itemAppearance = ItemAppearance.PopUp;
			_selectionArea = SelectionArea.FullItem;
			_shadowOnHover = false;
			InvalidateLayout(true);
		}

		private void ApplyOutlookExpressStyle()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = true;
			_selectionArea = SelectionArea.FullItem;
			_itemAppearance = ItemAppearance.PopUp;
			_shadowOnHover = false;
		}

		private void ApplyOffice2000Style()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = false;
			_selectionArea = SelectionArea.Image;
			_itemAppearance = ItemAppearance.PopUp;
			_itemAppearance = ItemAppearance.PopUp;
			_shadowOnHover = false;
		}

		private void ApplyOfficeXpStyle()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = false;
			_selectionArea = SelectionArea.Image;
			_itemAppearance = ItemAppearance.Flat;
			_shadowOnHover = true;
		}

		private void ApplyOffice2003Style()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = true;
			_selectionArea = SelectionArea.Image;
			_shadowOnHover = true;
			_itemAppearance = ItemAppearance.Flat;
		}

		private void ApplyOffice2007Style()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = true;
			_selectionArea = SelectionArea.Image;
			_shadowOnHover = true;
			_itemAppearance = ItemAppearance.Flat;
		}

		private void ApplyOffice2010Style()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = true;
			_selectionArea = SelectionArea.Image;
			_shadowOnHover = true;
			_itemAppearance = ItemAppearance.Flat;
		}

		private void ApplyVs2005Style()
		{
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = true;
			_selectionArea = SelectionArea.FullItem;
			_shadowOnHover = true;
			_itemAppearance = ItemAppearance.Flat;
		}

		private void ApplyCommonDialogStyle()
		{
			HeaderGroupVisible = false;
			ScrollLocation = ScrollLocation.ItemsLevel;
			KeepSelection = true;
			_selectionArea = SelectionArea.FullItem;
			_itemAppearance = ItemAppearance.Flat;
			_shadowOnHover = true;
		}
	}
}
