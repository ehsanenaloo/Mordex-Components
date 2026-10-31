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
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			ReadLayoutState(info);
			Invalidate();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			SerializeNavigationBarState(info);
			if (_layouts.Count > 0)
			{
				info.AddValue("Layouts", _layouts);
			}
		}

		internal ApplicationSettings GetApplicationSettings()
		{
			if (_applicationSettings == null)
			{
				_applicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (!string.Equals(_applicationSettings.SettingsKey, SettingsKey, StringComparison.OrdinalIgnoreCase))
			{
				_applicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _applicationSettings;
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (_isInitializing)
				{
					MemoryStream memoryStream = new MemoryStream();
					SaveLayoutFile(memoryStream);
					byte[] defaultLayoutSettings = memoryStream.ToArray();
					GetApplicationSettings().DefaultLayoutSettings = defaultLayoutSettings;
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = null;
				}
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					MemoryStream layoutStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(layoutStream);
					layoutStream.Close();
					layoutStream.Dispose();
					layoutStream = null;
				}
			}
		}

		public void ResetComponentSettings()
		{
			if (!base.DesignMode)
			{
				GetApplicationSettings().Reset();
				LoadComponentSettings();
			}
		}

		public void SaveComponentSettings()
		{
			MemoryStream memoryStream = new MemoryStream();
			SaveLayoutFile(memoryStream);
			byte[] layoutSettings = memoryStream.ToArray();
			GetApplicationSettings().LayoutSettings = layoutSettings;
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}

		public static string GetNavigationBarVersion()
		{
			string localPath = typeof(MxNavigationBar).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private void ApplyCurrentLayout()
		{
			_lastLoadedLayout = _currentLayout;
			NavigationBarLayout navigationBarLayout = null;
			if (_currentLayout != null)
			{
				navigationBarLayout = _currentLayout.ResolveMergedLayout();
			}
			if (navigationBarLayout == null || navigationBarLayout.LayoutString == "")
			{
				ReadLayoutState(new LayoutReader());
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(navigationBarLayout);
				mordexLayoutReader["NavigationBar"]?.DeserializeObject(this);
				if (!_isInitializing)
				{
					Update();
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		public void LoadLayout(NavigationBarLayout layout)
		{
			if (layout == null || layout.LayoutString == "")
			{
				throw new ArgumentNullException();
			}
			if (!layout.IsResolvable())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.ResolveMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			mordexLayoutReader["NavigationBar"]?.DeserializeObject(this);
		}

		public NavigationBarLayout GetLayout()
		{
			NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			WriteLayoutState(mordexLayoutWriter);
			LayoutWriter.StoreInLayout("NavigationBar", mordexLayoutWriter, navigationBarLayout);
			return navigationBarLayout;
		}

		private void SerializeNavigationBarState(LayoutWriter mordexLayoutWriter)
		{
			if (BackColor != SystemColors.Control)
			{
				mordexLayoutWriter.AddValue("BackColor", BackColor);
			}
			if (ShouldSerializeBlendColor())
			{
				mordexLayoutWriter.AddValue("BlendColor", _blendColor);
			}
			if (ShouldSerializeDropLineColor())
			{
				mordexLayoutWriter.AddValue("DropLineColor", _dropLineColor);
			}
			if (ShouldSerializeFlatBorderColor())
			{
				mordexLayoutWriter.AddValue("FlatBorderColor", _flatBorderColor);
			}
			if (_groupsStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("GroupsStateStyles", _groupsStateStyles);
			}
			if (_itemsStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("ItemsStateStyles", _itemsStateStyles);
			}
			if (ShouldSerializeGroupsBackgroundFormatStyle())
			{
				mordexLayoutWriter.AddValue("GroupsBackgroundFormatStyle", _groupsBackgroundFormatStyle);
			}
			if (ShouldSerializeLargeImageSize())
			{
				mordexLayoutWriter.AddValue("LargeImageSize", _largeImageSize);
			}
			if (ShouldSerializeSmallImageSize())
			{
				mordexLayoutWriter.AddValue("SmallImageSize", _smallImageSize);
			}
			if (!_allowDragGroups)
			{
				mordexLayoutWriter.AddValue("AllowDragGroups", _allowDragGroups);
			}
			if (_alpha != -1)
			{
				mordexLayoutWriter.AddValue("Alpha", _alpha);
			}
			if (_alphaMode != AlphaMode.Opaque)
			{
				mordexLayoutWriter.AddValue("AlphaMode", _alphaMode);
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorAlphaMode", _backColorAlphaMode);
			}
			if (BackgroundImage != null)
			{
				mordexLayoutWriter.AddValue("BackgroundImage", BackgroundImage);
			}
			if (_backgroundImageDrawMode != ImageDrawMode.Stretched)
			{
				mordexLayoutWriter.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_borderStyle != BorderStyle.Light3D)
			{
				mordexLayoutWriter.AddValue("BorderStyle", _borderStyle);
			}
			if (_defaultBlend != 0f)
			{
				mordexLayoutWriter.AddValue("DefaultBlend", _defaultBlend);
			}
			if (_defaultGroupImageIndex != -1)
			{
				mordexLayoutWriter.AddValue("DefaultGroupImageIndex", _defaultGroupImageIndex);
			}
			if (_defaultGroupImageKey.Length > 0)
			{
				mordexLayoutWriter.AddValue("DefaultGroupImageKey", _defaultGroupImageKey);
			}
			if (_defaultItemLargeImageIndex != -1)
			{
				mordexLayoutWriter.AddValue("DefaultItemLargeImageIndex", _defaultItemLargeImageIndex);
			}
			if (_defaultItemLargeImageKey.Length > 0)
			{
				mordexLayoutWriter.AddValue("DefaultItemLargeImageKey", _defaultItemLargeImageKey);
			}
			if (_defaultItemSmallImageIndex != -1)
			{
				mordexLayoutWriter.AddValue("DefaultItemSmallImageIndex", _defaultItemSmallImageIndex);
			}
			if (_defaultItemSmallImageKey.Length > 0)
			{
				mordexLayoutWriter.AddValue("DefaultItemSmallImageKey", _defaultItemSmallImageKey);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering.Value);
			}
			if (!_useMnemonic)
			{
				mordexLayoutWriter.AddValue("UseMnemonic", _useMnemonic);
			}
			if (DragMode != DragMode.Automatic)
			{
				mordexLayoutWriter.AddValue("DragMode", _dragMode);
			}
			if (_dragStyle != DragStyle.DropLine)
			{
				mordexLayoutWriter.AddValue("DragStyle", _dragStyle);
			}
			if (Font != Control.DefaultFont)
			{
				mordexLayoutWriter.AddValue("Font", Font);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (_groupAppearance != GroupAppearance.Light3D)
			{
				mordexLayoutWriter.AddValue("GroupAppearance", _groupAppearance);
			}
			if (_groups.Count > 0)
			{
				mordexLayoutWriter.AddValue("Groups", _groups);
			}
			if (!_headerGroupVisible)
			{
				mordexLayoutWriter.AddValue("HeaderGroupVisible", _headerGroupVisible);
			}
			if (_itemAppearance != ItemAppearance.PopUp)
			{
				mordexLayoutWriter.AddValue("ItemAppearance", _itemAppearance);
			}
			if (!_keepSelection)
			{
				mordexLayoutWriter.AddValue("KeepSelection", _keepSelection);
			}
			if (_orientation != NavigationBarOrientation.Vertical)
			{
				mordexLayoutWriter.AddValue("Orientation", _orientation);
			}
			if (_ownerDrawnArea != OwnerDrawnArea.None)
			{
				mordexLayoutWriter.AddValue("OwnerDrawnArea", _ownerDrawnArea);
			}
			if (_scrollLocation != ScrollLocation.GroupsLevel)
			{
				mordexLayoutWriter.AddValue("ScrollLocation", _scrollLocation);
			}
			if (_selectionArea != SelectionArea.FullItem)
			{
				mordexLayoutWriter.AddValue("SelectionArea", _selectionArea);
			}
			if (_shadowOnHover)
			{
				mordexLayoutWriter.AddValue("ShadowOnHover", _shadowOnHover);
			}
			if (!_showToolTips)
			{
				mordexLayoutWriter.AddValue("ShowToolTips", _showToolTips);
			}
		}

		private void DeserializeFormatStyle(LayoutReader mordexLayoutReader, string propertyName, NavigationBarFormatStyle targetInstance)
		{
			targetInstance.Reset();
			mordexLayoutReader.DeserializeObject(propertyName, targetInstance);
		}

		private void DeserializeNavigationBarState(LayoutReader reader)
		{
			BackColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), SystemColors.Control);
			_blendColor = (Color)LayoutReader.GetPropertyValue(reader, "BlendColor", typeof(Color), Color.Empty);
			_dropLineColor = (Color)LayoutReader.GetPropertyValue(reader, "DropLineColor", typeof(Color), Color.Red);
			_flatBorderColor = (Color)LayoutReader.GetPropertyValue(reader, "FlatBorderColor", typeof(Color), Color.Empty);
			_useMnemonic = (bool)LayoutReader.GetPropertyValue(reader, "UseMnemonic", typeof(bool), true);
			object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
			if (propertyValue != null)
			{
				_useCompatibleTextRendering = (bool)propertyValue;
			}
			_itemsStateStyles.Reset();
			reader.DeserializeObject("ItemsStateStyles", _itemsStateStyles);
			reader["ItemsFormatStyle"]?.DeserializeObject(_itemsStateStyles.FormatStyle);
			reader["HoverItemFormatStyle"]?.DeserializeObject(_itemsStateStyles.HotFormatStyle);
			reader["SelectedItemFormatStyle"]?.DeserializeObject(_itemsStateStyles.SelectedFormatStyle);
			_groupsStateStyles.Reset();
			reader.DeserializeObject("GroupsStateStyles", _groupsStateStyles);
			reader["GroupsFormatStyle"]?.DeserializeObject(_groupsStateStyles.FormatStyle);
			DeserializeFormatStyle(reader, "GroupsBackgroundFormatStyle", _groupsBackgroundFormatStyle);
			_largeImageSize = (Size)LayoutReader.GetPropertyValue(reader, "LargeImageSize", typeof(Size), Size.Empty);
			_smallImageSize = (Size)LayoutReader.GetPropertyValue(reader, "SmallImageSize", typeof(Size), Size.Empty);
			RefreshFormatStyles();
			_allowDragGroups = (bool)LayoutReader.GetPropertyValue(reader, "AllowDragGroups", typeof(bool), true);
			_alpha = (int)LayoutReader.GetPropertyValue(reader, "Alpha", typeof(int), -1);
			_alphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "AlphaMode", typeof(AlphaMode), AlphaMode.Opaque);
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			BackgroundImage = (Image)LayoutReader.GetPropertyValue(reader, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (ImageDrawMode)LayoutReader.GetPropertyValue(reader, "BackgroundImageDrawMode", typeof(ImageDrawMode), ImageDrawMode.Stretched);
			_borderStyle = (BorderStyle)LayoutReader.GetPropertyValue(reader, "BorderStyle", typeof(BorderStyle), BorderStyle.Light3D);
			_defaultBlend = (float)LayoutReader.GetPropertyValue(reader, "DefaultBlend", typeof(float), 0f);
			_defaultGroupImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DefaultGroupImageIndex", typeof(int), -1);
			_defaultGroupImageKey = (string)LayoutReader.GetPropertyValue(reader, "DefaultGroupImageKey", typeof(string), "");
			_defaultItemLargeImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DefaultItemLargeImageIndex", typeof(int), -1);
			_defaultItemLargeImageKey = (string)LayoutReader.GetPropertyValue(reader, "DefaultItemLargeImageKey", typeof(string), "");
			_defaultItemSmallImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DefaultItemSmallImageIndex", typeof(int), -1);
			_defaultItemSmallImageKey = (string)LayoutReader.GetPropertyValue(reader, "DefaultItemSmallImageKey", typeof(string), "");
			_dragMode = (DragMode)LayoutReader.GetPropertyValue(reader, "DragMode", typeof(DragMode), DragMode.Automatic);
			_dragStyle = (DragStyle)LayoutReader.GetPropertyValue(reader, "DragStyle", typeof(DragStyle), DragStyle.DropLine);
			Font = (Font)LayoutReader.GetPropertyValue(reader, "Font", typeof(Font), Control.DefaultFont);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_groupAppearance = (GroupAppearance)LayoutReader.GetPropertyValue(reader, "GroupAppearance", typeof(GroupAppearance), GroupAppearance.Light3D);
			PropertyValue groupsValue = reader["Groups"];
			_groups.Clear();
			_groups = new NavigationGroupCollection(this);
			groupsValue?.DeserializeObject(_groups);
			_headerGroupVisible = (bool)LayoutReader.GetPropertyValue(reader, "HeaderGroupVisible", typeof(bool), true);
			_itemAppearance = (ItemAppearance)LayoutReader.GetPropertyValue(reader, "ItemAppearance", typeof(ItemAppearance), ItemAppearance.PopUp);
			_keepSelection = (bool)LayoutReader.GetPropertyValue(reader, "KeepSelection", typeof(bool), true);
			_orientation = (NavigationBarOrientation)LayoutReader.GetPropertyValue(reader, "Orientation", typeof(NavigationBarOrientation), NavigationBarOrientation.Vertical);
			_ownerDrawnArea = (OwnerDrawnArea)LayoutReader.GetPropertyValue(reader, "OwnerDrawnArea", typeof(OwnerDrawnArea), OwnerDrawnArea.None);
			ScrollLocation = (ScrollLocation)LayoutReader.GetPropertyValue(reader, "ScrollLocation", typeof(ScrollLocation), ScrollLocation.GroupsLevel);
			_selectionArea = (SelectionArea)LayoutReader.GetPropertyValue(reader, "SelectionArea", typeof(SelectionArea), SelectionArea.FullItem);
			_shadowOnHover = (bool)LayoutReader.GetPropertyValue(reader, "ShadowOnHover", typeof(bool), false);
			_showToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(bool), true);
			InvalidateLayout(true);
		}

		internal void WriteLayoutState(LayoutWriter mordexLayoutWriter)
		{
			SerializeNavigationBarState(mordexLayoutWriter);
		}

		internal void ReadLayoutState(LayoutReader mordexLayoutReader)
		{
			DeserializeNavigationBarState(mordexLayoutReader);
		}

		public void LoadLayoutFile(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["NavigationBarLayoutFile"];
			if (propertyValue != null)
			{
				CurrentLayout = null;
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				ReadLayoutState(layoutReader);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				NavigationBarLayout navigationBarLayout = _layouts.FindCurrentLayout();
				if (navigationBarLayout != null)
				{
					CurrentLayout = navigationBarLayout;
					return;
				}
				CurrentLayout = null;
				OnLayoutLoad(EventArgs.Empty);
			}
		}

		public void SaveLayoutFile(Stream stream)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			PropertyValue propertyValue = new PropertyValue("NavigationBarLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.Navigation.MxNavigationBar";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public NavigationBarLayout FindLayout(string key)
		{
			return FindLayoutByKey(Layouts, key);
		}

		private NavigationBarLayout FindLayoutByKey(NavigationBarLayoutCollection navigationBarLayoutCollection, string strB)
		{
			foreach (NavigationBarLayout item in navigationBarLayoutCollection)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return item;
				}
				NavigationBarLayout childMatch = FindLayoutByKey(item.ChildLayouts, strB);
				if (childMatch != null)
				{
					return childMatch;
				}
			}
			return null;
		}
	}
}
