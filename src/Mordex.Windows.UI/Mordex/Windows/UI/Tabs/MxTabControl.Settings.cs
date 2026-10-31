using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Tabs
{
	public partial class MxTabControl
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			ReadTabLayout(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			WriteTabLayout(info);
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
			else if (string.Compare(_applicationSettings.SettingsKey, SettingsKey, ignoreCase: true) != 0)
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
					((ISupportInitialize)this).EndInit();
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
					MemoryStream settingsStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(settingsStream);
					settingsStream.Close();
					settingsStream.Dispose();
					settingsStream = null;
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
			try
			{
				GetApplicationSettings().Save();
			}
			catch (Exception ex) when (ex is ConfigurationException || ex is IOException || ex is UnauthorizedAccessException)
			{
				Trace.TraceError("MxTabControl: could not save component settings. " + ex.Message);
			}
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}

		private void InitializeCollections()
		{
			_tabPages = new TabPageCollection(this);
			_pageRenderers = new MxCollection(this);
			_panelFormatStyle = new FormatStyle(this);
			_layouts = new TabLayoutCollection(this);
		}

		private void ApplyCurrentLayout()
		{
			TabLayout uiTabLayout = null;
			if (_currentLayout != null)
			{
				uiTabLayout = _currentLayout.GetMergedLayout();
			}
			if (uiTabLayout == null || uiTabLayout.LayoutString == "")
			{
				ReadTabLayout(new LayoutReader());
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(uiTabLayout);
				mordexLayoutReader["UITab"]?.DeserializeObject(this);
				if (!_isInitializing)
				{
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		public void LoadLayout(TabLayout layout)
		{
			if (layout == null)
			{
				throw new ArgumentNullException();
			}
			if (!layout.CanBeLoaded())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.GetMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			mordexLayoutReader["UITab"]?.DeserializeObject(this);
		}

		public void LoadLayoutFile(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["UITabLayoutFile"];
			if (propertyValue != null)
			{
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				ReadTabLayout(layoutReader);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				TabLayout uiTabLayout = _layouts.FindCurrentLayout();
				if (uiTabLayout != null)
				{
					CurrentLayout = uiTabLayout;
				}
				else
				{
					CurrentLayout = null;
					OnLayoutLoad(EventArgs.Empty);
				}
			}
		}

		public void SaveLayoutFile(Stream stream)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			PropertyValue propertyValue = new PropertyValue("UITabLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.Tabs.MxTabControl";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public TabLayout GetLayout()
		{
			TabLayout uiTabLayout = new TabLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			WriteTabLayout(mordexLayoutWriter);
			LayoutWriter.StoreInLayout("UITab", mordexLayoutWriter, uiTabLayout);
			return uiTabLayout;
		}

		internal void WriteTabLayout(LayoutWriter mordexLayoutWriter)
		{
			if (_firstTabOffset != DefaultFirstTabOffset)
			{
				mordexLayoutWriter.AddValue("FirstTabOffset", _firstTabOffset);
			}
			if (!FocusOnClick)
			{
				mordexLayoutWriter.AddValue("FocusOnClick", FocusOnClick);
			}
			if (_imageSize != DefaultImageSize)
			{
				mordexLayoutWriter.AddValue("ImageSize", _imageSize);
			}
			if (!_itemSize.IsEmpty)
			{
				mordexLayoutWriter.AddValue("ItemSize", _itemSize);
			}
			if (_multiLine)
			{
				mordexLayoutWriter.AddValue("MultiLine", _multiLine);
			}
			if (_panelFormatStyle != null && !_panelFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("PanelFormatStyle", _panelFormatStyle);
			}
			if (SelectedIndex != -1)
			{
				mordexLayoutWriter.AddValue("SelectedIndex", SelectedIndex);
			}
			if (_showCloseButton)
			{
				mordexLayoutWriter.AddValue("ShowCloseButton", _showCloseButton);
			}
			if (!_showFocusRectangle)
			{
				mordexLayoutWriter.AddValue("ShowFocusRectangle", _showFocusRectangle);
			}
			if (!_showTabs)
			{
				mordexLayoutWriter.AddValue("ShowTabs", _showTabs);
			}
			if (!_showToolTips)
			{
				mordexLayoutWriter.AddValue("ShowToolTips", _showToolTips);
			}
			if (TabDisplay != TabDisplay.ImageAndText)
			{
				mordexLayoutWriter.AddValue("TabDisplay", TabDisplay);
			}
			if (_tabPages != null && _tabPages.Count > 0)
			{
				mordexLayoutWriter.AddValue("TabPages", _tabPages);
			}
			if (_tabStateStyles != null && !_tabStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("TabStateStyles", _tabStateStyles);
			}
			if (_tabStripAlignment != TabStripAlignment.Top)
			{
				mordexLayoutWriter.AddValue("TabStripAlignment", _tabStripAlignment);
			}
			if (_tabStripFormatStyle != null && !_tabStripFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("TabStripFormatStyle", _tabStripFormatStyle);
			}
			if (_tabStripOffset != DefaultTabStripOffset)
			{
				mordexLayoutWriter.AddValue("TabStripOffset", _tabStripOffset);
			}
			if (_textOrientation != TextOrientation.Default)
			{
				mordexLayoutWriter.AddValue("TextOrientation", _textOrientation);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
			ResetTabSizes();
			InvalidateLayout();
		}

		internal void ReadTabLayout(LayoutReader reader)
		{
			PropertyValue propertyValue = reader["TabPages"];
			TabPageCollection uiTabPageCollection = new TabPageCollection(this);
			if (propertyValue != null)
			{
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				if (layoutReader != null)
				{
					for (int i = 0; i < layoutReader.Count; i++)
					{
						PropertyValue pageValue = layoutReader[i];
						LayoutReader pageReader = pageValue.GetLayoutReader();
						string text = (string)LayoutReader.GetPropertyValue(pageReader, "Name", typeof(string), "");
						MxTabPage uiTabPage = null;
						if (text.Length > 0)
						{
							uiTabPage = _tabPages.FindByName(text);
						}
						else if (i < _tabPages.Count && _tabPages[i].Name.Length == 0)
						{
							// Unnamed pages can only be matched by position.
							uiTabPage = _tabPages[i];
						}
						if (uiTabPage == null || uiTabPageCollection.Contains(uiTabPage))
						{
							uiTabPage = new MxTabPage(pageValue.Name);
						}
						pageValue.DeserializeObject(uiTabPage);
						uiTabPageCollection.Add(uiTabPage);
					}
				}
			}
			for (int j = 0; j < _tabPages.Count; j++)
			{
				MxTabPage tabPage = _tabPages[j];
				if (!uiTabPageCollection.Contains(tabPage))
				{
					_tabPages.Remove(tabPage);
					j--;
				}
			}
			_tabPages = uiTabPageCollection;
			_tabPages.ReindexPages();
			object compatibleRenderingValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
			if (compatibleRenderingValue != null)
			{
				_useCompatibleTextRendering = (bool)compatibleRenderingValue;
			}
			else
			{
				_useCompatibleTextRendering = null;
			}
			_firstTabOffset = (int)LayoutReader.GetPropertyValue(reader, "FirstTabOffset", typeof(int), DefaultFirstTabOffset);
			_focusOnClick = (bool)LayoutReader.GetPropertyValue(reader, "FocusOnClick", typeof(bool), true);
			_imageSize = (Size)LayoutReader.GetPropertyValue(reader, "ImageSize", typeof(Size), DefaultImageSize);
			_itemSize = (Size)LayoutReader.GetPropertyValue(reader, "ItemSize", typeof(Size), Size.Empty);
			_multiLine = (bool)LayoutReader.GetPropertyValue(reader, "MultiLine", typeof(bool), false);
			_panelFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "PanelFormatStyle", typeof(FormatStyle), null);
			if (_panelFormatStyle != null)
			{
				_panelFormatStyle._owner = this;
			}
			else
			{
				_panelFormatStyle = new FormatStyle(this);
			}
			_showCloseButton = (bool)LayoutReader.GetPropertyValue(reader, "ShowCloseButton", typeof(bool), false);
			_showFocusRectangle = (bool)LayoutReader.GetPropertyValue(reader, "ShowFocusRectangle", typeof(bool), true);
			_showTabs = (bool)LayoutReader.GetPropertyValue(reader, "ShowTabs", typeof(bool), true);
			_showToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(bool), true);
			TabDisplay = (TabDisplay)LayoutReader.GetPropertyValue(reader, "TabDisplay", typeof(TabDisplay), TabDisplay.ImageAndText);
			_tabStateStyles = null;
			reader["TabStateStyles"]?.DeserializeObject(TabsStateStyles);
			_tabStripAlignment = (TabStripAlignment)LayoutReader.GetPropertyValue(reader, "TabStripAlignment", typeof(TabStripAlignment), TabStripAlignment.Top);
			_tabStripFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "TabStripFormatStyle", typeof(FormatStyle), null);
			if (_tabStripFormatStyle != null)
			{
				_tabStripFormatStyle._owner = this;
			}
			_tabStripOffset = (int)LayoutReader.GetPropertyValue(reader, "TabStripOffset", typeof(int), DefaultTabStripOffset);
			_textOrientation = (TextOrientation)LayoutReader.GetPropertyValue(reader, "TextOrientation", typeof(TextOrientation), TextOrientation.Default);
			int savedSelectedIndex = (int)LayoutReader.GetPropertyValue(reader, "SelectedIndex", typeof(int), -1);
			if (savedSelectedIndex != -1)
			{
				if (_tabPages.Count > savedSelectedIndex)
				{
					SelectedIndex = savedSelectedIndex;
				}
				else
				{
					SelectedIndex = -1;
				}
			}
			else
			{
				SelectedIndex = -1;
			}
			ResetTabSizes();
			InvalidateLayout();
		}
	}
}
