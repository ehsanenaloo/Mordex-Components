using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	public static class TextRenderingOptions
	{
		private static bool? _defaultUseCompatibleTextRendering;

		/// <summary>The value of <see cref="Label.UseCompatibleTextRendering"/> in this process. Controls use it when their own property is not set.</summary>
		public static bool DefaultUseCompatibleTextRendering
		{
			get
			{
				if (!_defaultUseCompatibleTextRendering.HasValue)
				{
					using (Label label = new Label())
					{
						_defaultUseCompatibleTextRendering = label.UseCompatibleTextRendering;
					}
				}
				return _defaultUseCompatibleTextRendering.Value;
			}
		}
	}
}
