using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Design
{
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[Browsable(false)]
	public class SuperTipSettingsEditor : UITypeEditor
	{
		public SuperTipSettingsEditor()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}

		// The extender property is shown as "SuperTipSettings on superTip1", so the provider is matched by the end of the display name.
		// With no match, the only super tip on the form is used; with several and no match, none is.
		internal static MxSuperTip FindSuperTip(IExtenderProvider[] providers, string displayName)
		{
			if (providers == null)
			{
				return null;
			}
			string name = (displayName ?? string.Empty).ToUpperInvariant();
			MxSuperTip only = null;
			int count = 0;
			foreach (IExtenderProvider extenderProvider in providers)
			{
				if (!(extenderProvider is MxSuperTip superTip))
				{
					continue;
				}
				count++;
				only = superTip;
				string siteName = superTip.Site?.Name;
				if (!string.IsNullOrEmpty(siteName))
				{
					string upper = siteName.ToUpperInvariant();
					if (name.EndsWith(upper, StringComparison.Ordinal) || name.EndsWith("\"" + upper + "\"", StringComparison.Ordinal))
					{
						return superTip;
					}
				}
			}
			return count == 1 ? only : null;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (context != null && context.Instance != null && provider != null)
			{
				IExtenderListService extenderListService = provider.GetService(typeof(IExtenderListService)) as IExtenderListService;
				MxSuperTip mordexSuperTip = FindSuperTip(extenderListService?.GetExtenderProviders(), context.PropertyDescriptor?.DisplayName);
				SuperTipSettings superTipSettings = value as SuperTipSettings;
				if (superTipSettings == null)
				{
					superTipSettings = new SuperTipSettings();
				}
				MxSuperTip fallbackSuperTip = null;
				if (mordexSuperTip == null)
				{
					fallbackSuperTip = new MxSuperTip();
					if (superTipSettings.ImageListProvider != null)
					{
						fallbackSuperTip.ImageList = superTipSettings.ImageListProvider.ImageList;
					}
					mordexSuperTip = fallbackSuperTip;
				}
				try
				{
					using (SuperTipSettingsDialog superTipSettingsDialog = new SuperTipSettingsDialog())
					{
						if (superTipSettingsDialog.ShowDialog(context, provider, mordexSuperTip, superTipSettings) == DialogResult.OK)
						{
							value = superTipSettings;
							if (context.OnComponentChanging())
							{
								context.OnComponentChanged();
							}
						}
					}
				}
				finally
				{
					fallbackSuperTip?.Dispose();
				}
				return value;
			}
			return value;
		}
	}
}
