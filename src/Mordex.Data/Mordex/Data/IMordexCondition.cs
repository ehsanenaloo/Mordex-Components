namespace Mordex.Data
{
	public interface IMordexCondition
	{
		IMordexField Field { get; set; }

		ConditionOperator ConditionOperator { get; set; }

		bool IsComposite { get; }

		LogicalOperator LogicalOperator { get; set; }

		bool Negation { get; set; }

		object Value1 { get; set; }

		object Value2 { get; set; }

		IMordexField ValueField1 { get; set; }

		IMordexField ValueField2 { get; set; }

		IMordexConditionCollection Conditions { get; }

		bool Enabled { get; set; }

		IMordexCondition AddCondition();

		void AddCondition(IMordexCondition condition);

		void AddCondition(LogicalOperator logicalOperator, IMordexCondition condition);

		void RemoveCondition(IMordexCondition condition);

		void RemoveConditionAt(int index);

		void Clear();
	}
}
