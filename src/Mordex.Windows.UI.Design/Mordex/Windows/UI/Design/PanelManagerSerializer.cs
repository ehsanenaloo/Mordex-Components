using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using CodeDomSerializer = Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class PanelManagerSerializer : CodeDomSerializer
	{
		private MxDockManager _dockManager;

		public PanelManagerSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override string GetTargetComponentName(CodeStatement statement, CodeExpression expression, Type targetType)
		{
			return base.GetTargetComponentName(statement, expression, targetType);
		}

		public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
		{
			CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(MxDockManager).BaseType, typeof(CodeDomSerializer));
			object deserialized = codeDomSerializer.Deserialize(manager, codeObject);
			_dockManager = deserialized as MxDockManager;
			manager.SerializationComplete += OnSerializationComplete;
			return deserialized;
		}

		private void OnSerializationComplete(object sender, EventArgs eventArgs)
		{
			IDesignerSerializationManager designerSerializationManager = sender as IDesignerSerializationManager;
			if (_dockManager == null && designerSerializationManager != null && designerSerializationManager.GetService(typeof(IDesignerHost)) is IDesignerHost designerHost)
			{
				foreach (IComponent component in designerHost.Container.Components)
				{
					if (component is MxDockManager)
					{
						_dockManager = component as MxDockManager;
						break;
					}
				}
			}
			if (_dockManager != null && DesignHelper.GetCurrentDesigner(_dockManager) is DockManagerDesigner uiPanelManagerDesigner)
			{
				uiPanelManagerDesigner.PanelManagerDesigner_IsHostLoaded = true;
				DesignHelper.EndPanelManagerInitialize(_dockManager);
			}
			if (designerSerializationManager != null)
			{
				designerSerializationManager.SerializationComplete -= OnSerializationComplete;
			}
		}

		private CodeDomSerializer CreateSerializerFromTargetAssembly(Type tp)
		{
			Type type = tp.Assembly.GetType(GetType().FullName, throwOnError: true, ignoreCase: true);
			ConstructorInfo constructor = type.GetConstructor(new Type[0]);
			return (CodeDomSerializer)constructor.Invoke(null);
		}

		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			object serialized = null;
			try
			{
				Type type = value.GetType();
				while (type != null && string.Compare(type.FullName, typeof(MxDockManager).FullName, ignoreCase: true, CultureInfo.InvariantCulture) != 0)
				{
					type = type.BaseType;
				}
				if (type != typeof(MxDockManager) && string.Compare(type.FullName, typeof(MxDockManager).FullName, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					CodeDomSerializer properSerializer = CreateSerializerFromTargetAssembly(type);
					return properSerializer.Serialize(manager, value);
				}
				CodeDomSerializer codeDomSerializer = (CodeDomSerializer)manager.GetSerializer(type.BaseType, typeof(CodeDomSerializer));
				serialized = codeDomSerializer.Serialize(manager, value);
				MxDockManager uiPanelManager = (MxDockManager)value;
				string managerName = manager.GetName(value);
				if (serialized is CodeStatementCollection codeStatementCollection && !string.IsNullOrEmpty(managerName) && uiPanelManager.Panels.Count > 0)
				{
					SerializePanels(uiPanelManager, uiPanelManager.Panels, codeStatementCollection);
					codeStatementCollection.Add(new CodeCommentStatement(""));
					codeStatementCollection.Add(new CodeCommentStatement("Design Time Panel Info:"));
					codeStatementCollection.Add(new CodeCommentStatement(""));
					CodeFieldReferenceExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), managerName);
					CodeExpression panelInfoCall = new CodeMethodInvokeExpression(codeFieldReferenceExpression, "BeginPanelInfo");
					codeStatementCollection.Add(panelInfoCall);
					DesignHelper.GetSavedPanelInfo(uiPanelManager, out IEnumerable dockedPanels, out IEnumerable floatingPanels);
					AddRootDockPanels(dockedPanels, codeStatementCollection, codeFieldReferenceExpression);
					AddRootFloatingPanels(floatingPanels, codeStatementCollection, codeFieldReferenceExpression);
					panelInfoCall = new CodeMethodInvokeExpression(codeFieldReferenceExpression, "EndPanelInfo");
					codeStatementCollection.Add(panelInfoCall);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "PanelManagerSerializer.Serialize");
			}
			return serialized;
		}

		private void AddRootDockPanels(IEnumerable panels, CodeStatementCollection statements, CodeFieldReferenceExpression pm)
		{
			foreach (IPanelInfo panel in panels)
			{
				CodeExpression panelIdExpression = new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(panel.PanelId.ToString()));
				CodeExpression dockStyleExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(PanelDockStyle)), panel.DockStyle.ToString());
				CodeExpression sizeExpression = new CodeObjectCreateExpression(typeof(Size), new CodePrimitiveExpression(panel.Size.Width), new CodePrimitiveExpression(panel.Size.Height));
				CodeExpression stateExpression = new CodePrimitiveExpression(panel.CurrentState);
				if (panel.IsGroup)
				{
					CodeExpression isStaticGroupExpression = new CodePrimitiveExpression(panel.IsStaticGroup);
					CodeExpression groupStyleExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(PanelGroupStyle)), panel.GroupStyle.ToString());
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddDockPanelInfo", panelIdExpression, groupStyleExpression, dockStyleExpression, isStaticGroupExpression, sizeExpression, stateExpression);
					statements.Add(value);
					AddChildDockPanels(panel.Children, statements, pm);
				}
				else
				{
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddDockPanelInfo", panelIdExpression, dockStyleExpression, sizeExpression, stateExpression);
					statements.Add(value);
				}
			}
		}

		private void AddRootFloatingPanels(IEnumerable panels, CodeStatementCollection statements, CodeFieldReferenceExpression pm)
		{
			foreach (IPanelInfo panel in panels)
			{
				CodeExpression panelIdExpression = new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(panel.PanelId.ToString()));
				CodeExpression sizeExpression = new CodeObjectCreateExpression(typeof(Size), new CodePrimitiveExpression(panel.Size.Width), new CodePrimitiveExpression(panel.Size.Height));
				CodeExpression locationExpression = new CodeObjectCreateExpression(typeof(Point), new CodePrimitiveExpression(panel.FloatingLocation.X), new CodePrimitiveExpression(panel.FloatingLocation.Y));
				CodeExpression stateExpression = new CodePrimitiveExpression(panel.CurrentState);
				if (panel.IsGroup)
				{
					CodeExpression isStaticGroupExpression = new CodePrimitiveExpression(panel.IsStaticGroup);
					CodeExpression groupStyleExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(PanelGroupStyle)), panel.GroupStyle.ToString());
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddFloatingPanelInfo", panelIdExpression, groupStyleExpression, isStaticGroupExpression, locationExpression, sizeExpression, stateExpression);
					statements.Add(value);
					AddChildFloatingPanels(panel.Children, statements, pm);
				}
				else
				{
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddFloatingPanelInfo", panelIdExpression, locationExpression, sizeExpression, stateExpression);
					statements.Add(value);
				}
			}
		}

		private void AddChildDockPanels(IEnumerable panels, CodeStatementCollection statements, CodeFieldReferenceExpression pm)
		{
			foreach (IPanelInfo panel in panels)
			{
				CodeExpression panelIdExpression = new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(panel.PanelId.ToString()));
				CodeExpression parentIdExpression = new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(panel.Parent.PanelId.ToString()));
				CodeExpression tileSizeExpression = new CodePrimitiveExpression(panel.TileSize);
				CodeExpression stateExpression = new CodePrimitiveExpression(panel.CurrentState);
				if (panel.IsGroup)
				{
					CodeExpression isStaticGroupExpression = new CodePrimitiveExpression(panel.IsStaticGroup);
					CodeExpression groupStyleExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(PanelGroupStyle)), panel.GroupStyle.ToString());
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddDockPanelInfo", panelIdExpression, parentIdExpression, groupStyleExpression, isStaticGroupExpression, tileSizeExpression, stateExpression);
					statements.Add(value);
					AddChildDockPanels(panel.Children, statements, pm);
				}
				else
				{
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddDockPanelInfo", panelIdExpression, parentIdExpression, tileSizeExpression, stateExpression);
					statements.Add(value);
				}
			}
		}

		private void AddChildFloatingPanels(IEnumerable panels, CodeStatementCollection statements, CodeFieldReferenceExpression pm)
		{
			foreach (IPanelInfo panel in panels)
			{
				CodeExpression panelIdExpression = new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(panel.PanelId.ToString()));
				CodeExpression parentIdExpression = new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(panel.Parent.PanelId.ToString()));
				CodeExpression tileSizeExpression = new CodePrimitiveExpression(panel.TileSize);
				CodeExpression stateExpression = new CodePrimitiveExpression(panel.CurrentState);
				if (panel.IsGroup)
				{
					CodeExpression isStaticGroupExpression = new CodePrimitiveExpression(panel.IsStaticGroup);
					CodeExpression groupStyleExpression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(PanelGroupStyle)), panel.GroupStyle.ToString());
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddFloatingPanelInfo", panelIdExpression, parentIdExpression, groupStyleExpression, isStaticGroupExpression, tileSizeExpression, stateExpression);
					statements.Add(value);
					AddChildFloatingPanels(panel.Children, statements, pm);
				}
				else
				{
					CodeExpression value = new CodeMethodInvokeExpression(pm, "AddFloatingPanelInfo", panelIdExpression, parentIdExpression, tileSizeExpression, stateExpression);
					statements.Add(value);
				}
			}
		}

		private void SerializePanels(MxDockManager panelManager, DockPanelCollection panels, CodeStatementCollection statements)
		{
			foreach (DockPanelBase item in (IEnumerable)panels)
			{
				if (item.Site == null)
				{
					continue;
				}
				CodeFieldReferenceExpression targetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Site.Name);
				CodeExpression left = new CodePropertyReferenceExpression(targetObject, "Id");
				Guid id = item.Id;
				CodeStatement value = new CodeAssignStatement(left, new CodeObjectCreateExpression(typeof(Guid), new CodePrimitiveExpression(id.ToString())));
				statements.Add(value);
				if (item is MxDockPanelGroup)
				{
					if (((MxDockPanelGroup)item).StaticGroup)
					{
						targetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Site.Name);
						CodeExpression staticGroupTarget = new CodePropertyReferenceExpression(targetObject, "StaticGroup");
						value = new CodeAssignStatement(staticGroupTarget, new CodePrimitiveExpression(true));
						statements.Add(value);
					}
					SerializePanels(panelManager, ((MxDockPanelGroup)item).Panels, statements);
				}
				string ownerName = DesignHelper.GetPanelOwner(item)?.Site?.Name;
				if (string.IsNullOrEmpty(ownerName))
				{
					continue;
				}
				targetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), ownerName);
				CodeExpression panelsExpression = new CodePropertyReferenceExpression(targetObject, "Panels");
				panelsExpression = new CodeMethodInvokeExpression(panelsExpression, "Add", new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), item.Site.Name));
				statements.Add(panelsExpression);
			}
		}
	}
}
