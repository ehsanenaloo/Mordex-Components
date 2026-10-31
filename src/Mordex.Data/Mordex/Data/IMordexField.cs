using System;
using System.Collections;
using System.Drawing;

namespace Mordex.Data
{
	public interface IMordexField
	{
		string Key { get; set; }

		TypeCode DataTypeCode { get; set; }

		bool CompareIgnoreCase { get; set; }

		string DataMember { get; set; }

		CompareTarget CompareTarget { get; set; }

		bool MultipleValues { get; set; }

		IMordexTable Table { get; }

		string MultipleValuesDataMember { get; set; }

		string DisplayName { get; set; }

		ColumnBoundMode BoundMode { get; set; }

		GroupInterval GroupInterval { get; set; }

		IComparer SortComparer { get; set; }

		FormatMode FormatMode { get; set; }

		string FormatString { get; set; }

		string NullText { get; set; }

		string Mask { get; set; }

		IComparer GroupComparer { get; set; }

		string GroupEmptyString { get; set; }

		string GroupFormatString { get; set; }

		FormatMode GroupFormatMode { get; set; }

		IMordexCustomGroup CustomGroup { get; set; }

		FilterEditType FilterEditType { get; set; }

		FilterEditType DefaultFilterEditType { get; }

		IMordexValueItemCollection ValueList { get; }

		bool HasValueList { get; set; }

		string FilterBuilderDisplayName { get; set; }

		Image GetDisplayImage();

		IMordexValueItemCollection GetFilterValueList();
	}
}
