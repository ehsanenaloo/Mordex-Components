using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using CodeDomSerializer = Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class EditContextMenuSerializer : CodeDomSerializer
	{
		public EditContextMenuSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(EditContextMenu).BaseType, typeof(CodeDomSerializer));
			return codeDomSerializer.Deserialize(manager, codeObject);
		}

		private CodeDomSerializer CreateSerializerFromTargetAssembly(Type tp)
		{
			Type type = tp.Assembly.GetType(GetType().FullName, throwOnError: true, ignoreCase: true);
			ConstructorInfo constructor = type.GetConstructor(new Type[0]);
			return (CodeDomSerializer)constructor.Invoke(null);
		}

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			object statements = null;
			try
			{
				if (value.GetType() != typeof(EditContextMenu) && string.Compare(value.GetType().FullName, typeof(EditContextMenu).FullName, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					CodeDomSerializer properSerializer = CreateSerializerFromTargetAssembly(value.GetType());
					return properSerializer.Serialize(manager, value);
				}
				CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(value.GetType().BaseType, typeof(CodeDomSerializer));
				statements = codeDomSerializer.Serialize(manager, value);
				CodeStatementCollection codeStatementCollection = null;
				if (statements is CodeStatementCollection)
				{
					codeStatementCollection = (CodeStatementCollection)statements;
					for (int i = codeStatementCollection.Count - 1; i >= 0; i--)
					{
						if (codeStatementCollection[i] is CodeExpressionStatement)
						{
							CodeExpressionStatement codeExpressionStatement = (CodeExpressionStatement)codeStatementCollection[i];
							if (codeExpressionStatement.Expression is CodeMethodInvokeExpression)
							{
								CodeMethodInvokeExpression codeMethodInvokeExpression = (CodeMethodInvokeExpression)codeExpressionStatement.Expression;
								if (codeMethodInvokeExpression.Method.TargetObject is CodeCastExpression)
								{
									CodeCastExpression codeCastExpression = (CodeCastExpression)codeMethodInvokeExpression.Method.TargetObject;
									if (string.Compare(codeCastExpression.TargetType.BaseType, typeof(ISupportInitialize).FullName, ignoreCase: true) == 0)
									{
										codeStatementCollection.RemoveAt(i);
									}
								}
							}
						}
					}
				}
				EditContextMenu contextMenu = (EditContextMenu)value;
				IList controlsList = DesignHelper.GetControlsList(contextMenu);
				MxCommandManager parentManager = DesignHelper.GetParentManager(contextMenu);
				string parentName = parentManager != null ? manager.GetName(parentManager) : null;
				if (codeStatementCollection != null && !string.IsNullOrEmpty(parentName) && controlsList != null && controlsList.Count > 0)
				{
					foreach (Control item in controlsList)
					{
						string itemName = manager.GetName(item);
						if (string.IsNullOrEmpty(itemName))
						{
							continue;
						}
						CodeFieldReferenceExpression targetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), parentName);
						CodePropertyReferenceExpression menuProperty = new CodePropertyReferenceExpression(targetObject, "EditContextMenu");
						CodeFieldReferenceExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), itemName);
						CodePrimitiveExpression codePrimitiveExpression = new CodePrimitiveExpression(true);
						CodeExpression setUseMenuCall = new CodeMethodInvokeExpression(menuProperty, "SetUseMordexEditMenu", codeFieldReferenceExpression, codePrimitiveExpression);
						codeStatementCollection.Add(setUseMenuCall);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "EditContextMenuSerializer.Serialize");
			}
			return statements;
		}
	}
}
