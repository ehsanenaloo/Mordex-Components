namespace Mordex.Data
{
	public interface IMordexCustomGroup
	{
		IMordexCustomGroupRowCollection GroupRows { get; }

		string Key { get; set; }

		CustomGroupType CustomGroupType { get; set; }

		IMordexField[] CompositeFields { get; set; }
	}
}
