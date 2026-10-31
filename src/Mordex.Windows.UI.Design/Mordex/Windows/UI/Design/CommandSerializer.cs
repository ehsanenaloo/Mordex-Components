using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using CodeDomSerializer = Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class CommandSerializer : CodeDomSerializer
	{
		public CommandSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			try
			{
				CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(MxCommand).BaseType, typeof(CodeDomSerializer));
				if (codeObject is CodeStatementCollection)
				{
					CodeStatementCollection codeStatementCollection = (CodeStatementCollection)codeObject;
					foreach (CodeStatement item in codeStatementCollection)
					{
						if (!(item is CodeExpressionStatement))
						{
							continue;
						}
						CodeExpressionStatement codeExpressionStatement = (CodeExpressionStatement)item;
						if (!(codeExpressionStatement.Expression is CodeMethodInvokeExpression))
						{
							continue;
						}
						CodeMethodInvokeExpression codeMethodInvokeExpression = (CodeMethodInvokeExpression)codeExpressionStatement.Expression;
						if (string.Compare(codeMethodInvokeExpression.Method.MethodName, "AddRange", ignoreCase: true) == 0)
						{
							string[] childCommandNames = GetChildCommandNames(codeMethodInvokeExpression);
							string[] childNames = childCommandNames;
							foreach (string name in childNames)
							{
								manager.GetInstance(name);
							}
						}
						else if (string.Compare(codeMethodInvokeExpression.Method.MethodName, "Add", ignoreCase: true) == 0 && codeMethodInvokeExpression.Parameters[0] is CodeVariableReferenceExpression)
						{
							CodeVariableReferenceExpression codeVariableReferenceExpression = (CodeVariableReferenceExpression)codeMethodInvokeExpression.Parameters[0];
							manager.GetInstance(codeVariableReferenceExpression.VariableName);
						}
					}
				}
				return codeDomSerializer.Deserialize(manager, codeObject);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		private string[] GetChildCommandNames(CodeMethodInvokeExpression mie)
		{
			ArrayList arrayList = new ArrayList();
			if (mie.Parameters[0] is CodeArrayCreateExpression)
			{
				CodeArrayCreateExpression codeArrayCreateExpression = (CodeArrayCreateExpression)mie.Parameters[0];
				foreach (CodeExpression initializer in codeArrayCreateExpression.Initializers)
				{
					if (initializer is CodeVariableReferenceExpression)
					{
						CodeVariableReferenceExpression codeVariableReferenceExpression = (CodeVariableReferenceExpression)initializer;
						arrayList.Add(codeVariableReferenceExpression.VariableName);
					}
				}
			}
			string[] names = new string[arrayList.Count];
			for (int i = 0; i < names.Length; i++)
			{
				names[i] = (string)arrayList[i];
			}
			return names;
		}

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer));
			return codeDomSerializer.Serialize(manager, value);
		}
	}
}
