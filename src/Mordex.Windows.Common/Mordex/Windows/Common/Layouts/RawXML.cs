using System.Runtime.InteropServices;

namespace Mordex.Windows.Common.Layouts
{
	[ComVisible(false)]
	public struct RawXML
	{
		private string _xml;

		public string XML
		{
			get
			{
				return _xml;
			}
			set
			{
				_xml = value;
			}
		}

		public RawXML(string xml)
		{
			_xml = xml;
		}

		public override int GetHashCode()
		{
			return _xml.GetHashCode();
		}

		public override bool Equals(object other)
		{
			return ((RawXML)other)._xml == _xml;
		}
	}
}
