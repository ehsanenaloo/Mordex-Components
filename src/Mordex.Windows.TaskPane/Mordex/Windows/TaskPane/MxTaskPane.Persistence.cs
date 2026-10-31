using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		public TaskPaneLayout FindLayout(string key)
		{
			return FindLayoutRecursive(Layouts, key);
		}

		private TaskPaneLayout FindLayoutRecursive(TaskPaneLayoutCollection taskPaneLayoutCollection, string strB)
		{
			foreach (TaskPaneLayout item in taskPaneLayoutCollection)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return item;
				}
				TaskPaneLayout childMatch = FindLayoutRecursive(item.ChildLayouts, strB);
				if (childMatch != null)
				{
					return childMatch;
				}
			}
			return null;
		}

		private void LoadCurrentLayout()
		{
			_lastLoadedLayout = _currentLayout;
			TaskPaneLayout taskPaneLayout = null;
			if (_currentLayout != null)
			{
				taskPaneLayout = _currentLayout.ResolveEffectiveLayout();
			}
			if (taskPaneLayout == null || taskPaneLayout.LayoutString == "")
			{
				DeserializeTaskPaneState(new LayoutReader());
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(taskPaneLayout);
				mordexLayoutReader["TaskPane"]?.DeserializeObject(this);
				if (!_isInitializing)
				{
					Update();
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		public void LoadLayout(TaskPaneLayout layout)
		{
			if (layout == null || layout.LayoutString == "")
			{
				throw new ArgumentNullException();
			}
			if (!layout.CanBeLoaded())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.ResolveEffectiveLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			mordexLayoutReader["TaskPane"]?.DeserializeObject(this);
		}

		public TaskPaneLayout GetLayout()
		{
			TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			SerializeTaskPaneState(mordexLayoutWriter);
			LayoutWriter.StoreInLayout("TaskPane", mordexLayoutWriter, taskPaneLayout);
			return taskPaneLayout;
		}

		private void SerializeTaskPaneState(LayoutWriter mordexLayoutWriter)
		{
			if (ShouldSerializeBackColor())
			{
				mordexLayoutWriter.AddValue("BackColor", BackColor);
			}
			if (ShouldSerializeBlendColor())
			{
				mordexLayoutWriter.AddValue("BlendColor", _blendColor);
			}
			if (ShouldSerializeFlatBorderColor())
			{
				mordexLayoutWriter.AddValue("FlatBorderColor", _flatBorderColor);
			}
			if (_specialGroupsStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("SpecialGroupsStateStyles", _specialGroupsStateStyles);
			}
			if (_groupsStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("GroupsStateStyles", _groupsStateStyles);
			}
			if (_itemsStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("ItemsStateStyles", _itemsStateStyles);
			}
			if (ShouldSerializeBackgroundFormatStyle())
			{
				mordexLayoutWriter.AddValue("BackgroundFormatStyle", _backgroundFormatStyle);
			}
			if (_alpha != -1)
			{
				mordexLayoutWriter.AddValue("Alpha", _alpha);
			}
			if (_alphaMode != AlphaMode.Opaque)
			{
				mordexLayoutWriter.AddValue("AlphaMode", _alphaMode);
			}
			if (BackColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorAlphaMode", BackColorAlphaMode);
			}
			if (BackgroundImage != null)
			{
				mordexLayoutWriter.AddValue("BackgroundImage", BackgroundImage);
			}
			if (_backgroundImageDrawMode != ImageDrawMode.Stretched)
			{
				mordexLayoutWriter.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_borderStyle != BorderStyle.SunkenLight3D)
			{
				mordexLayoutWriter.AddValue("BorderStyle", _borderStyle);
			}
			if (Columns != 1)
			{
				mordexLayoutWriter.AddValue("Columns", _columns);
			}
			if (DefaultBlend != 0f)
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
			if (_defaultItemImageIndex != -1)
			{
				mordexLayoutWriter.AddValue("DefaultItemImageIndex", _defaultItemImageIndex);
			}
			if (_defaultItemImageKey.Length > 0)
			{
				mordexLayoutWriter.AddValue("DefaultItemImageKey", _defaultItemImageKey);
			}
			if (_effect != Effect.Fade)
			{
				mordexLayoutWriter.AddValue("Effect", _effect);
			}
			if (ForeColor != Control.DefaultForeColor)
			{
				mordexLayoutWriter.AddValue("ForeColor", ForeColor);
			}
			if (ForeColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (_groups.Count > 0)
			{
				mordexLayoutWriter.AddValue("Groups", _groups);
			}
			if (_groupSeparation != -1)
			{
				mordexLayoutWriter.AddValue("GroupSeparation", _groupSeparation);
			}
			if (_imageSize != Size.Empty)
			{
				mordexLayoutWriter.AddValue("ImageSize", _imageSize);
			}
			if (_ownerDrawnArea != OwnerDrawnArea.None)
			{
				mordexLayoutWriter.AddValue("OwnerDrawnArea", _ownerDrawnArea);
			}
			if (_rightMargin != -1)
			{
				mordexLayoutWriter.AddValue("RightMargin", _rightMargin);
			}
			if (_leftMargin != -1)
			{
				mordexLayoutWriter.AddValue("LeftMargin", _leftMargin);
			}
			if (_topMargin != -1)
			{
				mordexLayoutWriter.AddValue("TopMargin", _topMargin);
			}
			if (ScrollBarStyle.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("ScrollBarStyle", ScrollBarStyle);
			}
			if (!_showGroupBorder)
			{
				mordexLayoutWriter.AddValue("ShowGroupBorder", _showGroupBorder);
			}
			if (!_showToolTips)
			{
				mordexLayoutWriter.AddValue("ShowToolTips", _showToolTips);
			}
			if (_verticalScrollBar != VerticalScrollBar.Automatic)
			{
				mordexLayoutWriter.AddValue("VerticalScrollBar", _verticalScrollBar);
			}
			if (_columnSeparation != 10)
			{
				mordexLayoutWriter.AddValue("ColumnSeparation", _columnSeparation);
			}
			if (ShouldSerializeWatermarkImage())
			{
				mordexLayoutWriter.AddValue("WatermarkImage", _watermarkImage);
			}
			if (!_useMnemonic)
			{
				mordexLayoutWriter.AddValue("UseMnemonic", _useMnemonic);
			}
			if (_showFocusRectangle)
			{
				mordexLayoutWriter.AddValue("ShowFocusRectangle", _showFocusRectangle);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
		}

		private void ResetAndDeserializeFormatStyle(LayoutReader mordexLayoutReader, string propertyName, TaskPaneFormatStyle targetInstance)
		{
			targetInstance.Reset();
			mordexLayoutReader.DeserializeObject(propertyName, targetInstance);
		}

		private void DeserializeTaskPaneState(LayoutReader reader)
		{
			BackColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), SystemColors.Window);
			_blendColor = (Color)LayoutReader.GetPropertyValue(reader, "BlendColor", typeof(Color), Color.Empty);
			_flatBorderColor = (Color)LayoutReader.GetPropertyValue(reader, "FlatBorderColor", typeof(Color), Color.Empty);
			_itemsStateStyles.Reset();
			reader["ItemsStateStyles"]?.DeserializeObject(_itemsStateStyles);
			reader["ItemsFormatStyle"]?.DeserializeObject(_itemsStateStyles.FormatStyle);
			reader["HoverItemFormatStyle"]?.DeserializeObject(_itemsStateStyles.HotFormatStyle);
			reader["SelectedItemFormatStyle"]?.DeserializeObject(_itemsStateStyles.SelectedFormatStyle);
			_groupsStateStyles.Reset();
			reader["GroupsStateStyles"]?.DeserializeObject(_groupsStateStyles);
			reader["GroupsFormatStyle"]?.DeserializeObject(_groupsStateStyles.FormatStyle);
			reader["HoverGroupFormatStyle"]?.DeserializeObject(_groupsStateStyles.HotFormatStyle);
			_specialGroupsStateStyles.Reset();
			reader.DeserializeObject("SpecialGroupsStateStyles", _specialGroupsStateStyles);
			reader["SpecialGroupsFormatStyle"]?.DeserializeObject(_specialGroupsStateStyles.FormatStyle);
			reader["HoverSpecialGroupFormatStyle"]?.DeserializeObject(_specialGroupsStateStyles.HotFormatStyle);
			ResetAndDeserializeFormatStyle(reader, "BackgroundFormatStyle", BackgroundFormatStyle);
			_alpha = (int)LayoutReader.GetPropertyValue(reader, "Alpha", typeof(int), -1);
			_alphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "AlphaMode", typeof(AlphaMode), AlphaMode.Opaque);
			BackColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			BackgroundImage = (Image)LayoutReader.GetPropertyValue(reader, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (ImageDrawMode)LayoutReader.GetPropertyValue(reader, "BackgroundImageDrawMode", typeof(ImageDrawMode), ImageDrawMode.Stretched);
			_borderStyle = (BorderStyle)LayoutReader.GetPropertyValue(reader, "BorderStyle", typeof(BorderStyle), BorderStyle.SunkenLight3D);
			_columns = (int)LayoutReader.GetPropertyValue(reader, "Columns", typeof(int), 1);
			_defaultBlend = (float)LayoutReader.GetPropertyValue(reader, "DefaultBlend", typeof(float), 0f);
			_defaultGroupImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DefaultGroupImageIndex", typeof(int), -1);
			_defaultGroupImageKey = (string)LayoutReader.GetPropertyValue(reader, "DefaultGroupImageKey", typeof(string), "");
			_defaultItemImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DefaultItemImageIndex", typeof(int), -1);
			_defaultItemImageKey = (string)LayoutReader.GetPropertyValue(reader, "DefaultItemImageKey", typeof(string), "");
			_effect = (Effect)LayoutReader.GetPropertyValue(reader, "Effect", typeof(Effect), Effect.Fade);
			ForeColor = (Color)LayoutReader.GetPropertyValue(reader, "ForeColor", typeof(Color), Control.DefaultForeColor);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
			if (propertyValue != null)
			{
				_useCompatibleTextRendering = (bool)propertyValue;
			}
			PropertyValue groupsValue = reader["Groups"];
			TaskPaneGroupCollection taskPaneGroupCollection = new TaskPaneGroupCollection(this);
			if (groupsValue != null)
			{
				LayoutReader layoutReader = groupsValue.GetLayoutReader();
				if (layoutReader != null)
				{
					int count = layoutReader.Count;
					for (int i = 0; i < count; i++)
					{
						PropertyValue groupValue = layoutReader[i];
						TaskPaneGroup taskPaneGroup = new TaskPaneGroup(groupValue.Name);
						groupValue.DeserializeObject(taskPaneGroup);
						if (taskPaneGroup.Key.Length > 0)
						{
							TaskPaneGroup existingGroup = _groups.FindByKey(taskPaneGroup.Key);
							if (existingGroup != null)
							{
								groupValue.DeserializeObject(existingGroup);
								taskPaneGroup = existingGroup;
							}
						}
						taskPaneGroupCollection.AddWithoutNotify(taskPaneGroup);
						if (base.DesignMode && base.Container != null && taskPaneGroup.ContainerControl != null)
						{
							base.Container.Add(taskPaneGroup.ContainerControl);
						}
					}
				}
			}
			for (int j = 0; j < _groups.Count; j++)
			{
				TaskPaneGroup oldGroup = _groups[j];
				if (!taskPaneGroupCollection.Contains(oldGroup))
				{
					_groups.Remove(oldGroup);
					j--;
				}
				else
				{
					oldGroup._owner = taskPaneGroupCollection;
				}
			}
			_groups = taskPaneGroupCollection;
			_groups.ReindexGroups();
			_groupSeparation = (int)LayoutReader.GetPropertyValue(reader, "GroupSeparation", typeof(int), -1);
			_imageSize = (Size)LayoutReader.GetPropertyValue(reader, "ImageSize", typeof(Size), Size.Empty);
			// Older layout files wrote this value as "OwnerDrawArea".
			OwnerDrawnArea ownerDrawAreaValue = (OwnerDrawnArea)LayoutReader.GetPropertyValue(reader, "OwnerDrawArea", typeof(OwnerDrawnArea), OwnerDrawnArea.None);
			_ownerDrawnArea = (OwnerDrawnArea)LayoutReader.GetPropertyValue(reader, "OwnerDrawnArea", typeof(OwnerDrawnArea), ownerDrawAreaValue);
			_rightMargin = (int)LayoutReader.GetPropertyValue(reader, "RightMargin", typeof(int), -1);
			_leftMargin = (int)LayoutReader.GetPropertyValue(reader, "LeftMargin", typeof(int), -1);
			_topMargin = (int)LayoutReader.GetPropertyValue(reader, "TopMargin", typeof(int), -1);
			_useMnemonic = (bool)LayoutReader.GetPropertyValue(reader, "UseMnemonic", typeof(bool), true);
			_showGroupBorder = (bool)LayoutReader.GetPropertyValue(reader, "ShowGroupBorder", typeof(bool), true);
			_showToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(bool), true);
			_verticalScrollBar = (VerticalScrollBar)LayoutReader.GetPropertyValue(reader, "VerticalScrollBar", typeof(VerticalScrollBar), VerticalScrollBar.Automatic);
			_columnSeparation = (int)LayoutReader.GetPropertyValue(reader, "ColumnSeparation", typeof(int), 10);
			_showFocusRectangle = (bool)LayoutReader.GetPropertyValue(reader, "ShowFocusRectangle", typeof(bool), false);
			ResetWatermarkImage();
			reader.DeserializeObject("WatermarkImage", _watermarkImage);
			reader.DeserializeObject("ScrollBarStyle", ScrollBarStyle);
			InvalidateLayout();
		}

		public void LoadLayoutFile(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["TaskPaneLayoutFile"];
			if (propertyValue != null)
			{
				CurrentLayout = null;
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				DeserializeTaskPaneState(layoutReader);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				TaskPaneLayout taskPaneLayout = _layouts.FindCurrentLayout();
				if (taskPaneLayout != null)
				{
					CurrentLayout = taskPaneLayout;
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
			PropertyValue propertyValue = new PropertyValue("TaskPaneLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.TaskPane.MxTaskPane";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			DeserializeTaskPaneState(info);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			SerializeTaskPaneState(info);
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
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}

		public static string GetTaskPaneVersion()
		{
			string localPath = typeof(MxTaskPane).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}
	}
}
