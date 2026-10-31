using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Mordex.Windows.Common.Design
{
	// One component member that a design-time edit changes. A null member name means the whole component.
	public readonly struct ChangeTarget
	{
		public ChangeTarget(object component, string memberName)
		{
			Component = component;
			MemberName = memberName;
		}

		public object Component { get; }

		public string MemberName { get; }
	}

	// Runs a design-time edit the way the undo engine and the code serializer expect:
	// inside a host transaction, with ComponentChanging raised before the edit and ComponentChanged after it.
	public static class DesignerChange
	{
		public static bool Run(IComponent component, string memberName, string description, Func<bool> change)
		{
			return Run(component?.Site, description, change, new ChangeTarget(component, memberName));
		}

		public static void Run(IComponent component, string memberName, string description, Action change)
		{
			if (change == null)
			{
				throw new ArgumentNullException(nameof(change));
			}
			Run(component, memberName, description, () =>
			{
				change();
				return true;
			});
		}

		// The edit returns false to abandon the change; the transaction is then cancelled and no ComponentChanged is raised.
		public static bool Run(IServiceProvider services, string description, Func<bool> change, params ChangeTarget[] targets)
		{
			if (change == null)
			{
				throw new ArgumentNullException(nameof(change));
			}
			IDesignerHost host = services?.GetService(typeof(IDesignerHost)) as IDesignerHost;
			IComponentChangeService changeService = services?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
			List<KeyValuePair<object, MemberDescriptor>> members = Resolve(targets);
			DesignerTransaction transaction = host?.CreateTransaction(description);
			try
			{
				if (changeService != null)
				{
					foreach (KeyValuePair<object, MemberDescriptor> member in members)
					{
						changeService.OnComponentChanging(member.Key, member.Value);
					}
				}
				if (!change())
				{
					return false;
				}
				if (changeService != null)
				{
					foreach (KeyValuePair<object, MemberDescriptor> member in members)
					{
						changeService.OnComponentChanged(member.Key, member.Value, null, null);
					}
				}
				transaction?.Commit();
				return true;
			}
			finally
			{
				if (transaction != null && !transaction.Committed && !transaction.Canceled)
				{
					transaction.Cancel();
				}
			}
		}

		// Sets a property through its descriptor, so a sited component raises its own change events.
		public static void SetProperty(object component, string propertyName, object value)
		{
			if (component == null)
			{
				return;
			}
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)[propertyName];
			if (property == null || property.IsReadOnly)
			{
				throw new ArgumentException("No writable property " + propertyName + " on " + component.GetType().Name + ".", nameof(propertyName));
			}
			property.SetValue(component, value);
		}

		// True for a component inherited read-only from a base form. Unlike a ComponentChanging probe, this raises no change events,
		// so it is safe to call on every mouse move.
		public static bool IsReadOnly(object component)
		{
			if (component == null)
			{
				return false;
			}
			InheritanceAttribute inheritance = TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)] as InheritanceAttribute;
			return inheritance != null && inheritance.InheritanceLevel == InheritanceLevel.InheritedReadOnly;
		}

		private static List<KeyValuePair<object, MemberDescriptor>> Resolve(ChangeTarget[] targets)
		{
			List<KeyValuePair<object, MemberDescriptor>> members = new List<KeyValuePair<object, MemberDescriptor>>();
			if (targets == null)
			{
				return members;
			}
			HashSet<object> seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
			foreach (ChangeTarget target in targets)
			{
				if (target.Component == null || !seen.Add(target.Component))
				{
					continue;
				}
				MemberDescriptor member = string.IsNullOrEmpty(target.MemberName) ? null : TypeDescriptor.GetProperties(target.Component)[target.MemberName];
				members.Add(new KeyValuePair<object, MemberDescriptor>(target.Component, member));
			}
			return members;
		}
	}
}
