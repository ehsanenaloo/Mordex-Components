using System.ComponentModel;
using System.Configuration;

namespace Mordex.Windows.Common
{
	public class ApplicationSettings : ApplicationSettingsBase
	{
		[UserScopedSetting]
		public byte[] LayoutSettings
		{
			get
			{
				return (byte[])this["LayoutSettings"];
			}
			set
			{
				this["LayoutSettings"] = value;
			}
		}

		public byte[] DefaultLayoutSettings
		{
			get
			{
				return (byte[])Properties["LayoutSettings"].DefaultValue;
			}
			set
			{
				Properties["LayoutSettings"].DefaultValue = value;
			}
		}

		public ApplicationSettings(IComponent component, string settingsKey)
			: base(component, settingsKey)
		{
		}
	}
}
