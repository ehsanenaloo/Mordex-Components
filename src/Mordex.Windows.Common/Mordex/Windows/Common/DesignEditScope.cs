using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Mordex.Windows.Common
{
	// An edit that a control makes to its own components at design time, for example when a command is added or
	// moved on the design surface. Begin opens a designer transaction and raises ComponentChanging before the
	// edit, End raises ComponentChanged and commits. Dispose without End cancels the transaction and raises no
	// ComponentChanged, so a failed edit leaves no undo step. A component that is not sited in design mode gets
	// an inactive scope, which does nothing.
	public sealed class DesignEditScope : IDisposable
	{
		private static readonly DesignEditScope s_inactive = new DesignEditScope(null, null, new List<KeyValuePair<object, MemberDescriptor>>());

		private readonly IComponentChangeService _changeService;

		private readonly DesignerTransaction _transaction;

		private readonly List<KeyValuePair<object, MemberDescriptor>> _members;

		private bool _isClosed;

		private DesignEditScope(IComponentChangeService changeService, DesignerTransaction transaction, List<KeyValuePair<object, MemberDescriptor>> members)
		{
			_changeService = changeService;
			_transaction = transaction;
			_members = members;
			_isClosed = changeService == null && transaction == null;
		}

		public bool IsActive => !_isClosed;

		// A null or empty member name reports the whole component.
		public static DesignEditScope Begin(object component, string memberName, string description)
		{
			if (!(GetDesignSite(component) is ISite site))
			{
				return s_inactive;
			}
			List<KeyValuePair<object, MemberDescriptor>> members = new List<KeyValuePair<object, MemberDescriptor>>
			{
				new KeyValuePair<object, MemberDescriptor>(component, FindMember(component, memberName))
			};
			return Open(site, description, members);
		}

		// Reports each component as a whole. The first one sited in design mode supplies the designer services.
		public static DesignEditScope BeginComponents(string description, params object[] components)
		{
			ISite site = null;
			List<KeyValuePair<object, MemberDescriptor>> members = new List<KeyValuePair<object, MemberDescriptor>>();
			HashSet<object> seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
			foreach (object component in components ?? Array.Empty<object>())
			{
				ISite componentSite = GetDesignSite(component);
				if (componentSite == null || !seen.Add(component))
				{
					continue;
				}
				site = site ?? componentSite;
				members.Add(new KeyValuePair<object, MemberDescriptor>(component, null));
			}
			return site == null ? s_inactive : Open(site, description, members);
		}

		public void End()
		{
			if (_isClosed)
			{
				return;
			}
			_isClosed = true;
			try
			{
				if (_changeService != null)
				{
					foreach (KeyValuePair<object, MemberDescriptor> member in _members)
					{
						_changeService.OnComponentChanged(member.Key, member.Value, null, null);
					}
				}
				_transaction?.Commit();
			}
			finally
			{
				CancelOpenTransaction();
			}
		}

		public void Dispose()
		{
			if (!_isClosed)
			{
				_isClosed = true;
				CancelOpenTransaction();
			}
		}

		private static DesignEditScope Open(ISite site, string description, List<KeyValuePair<object, MemberDescriptor>> members)
		{
			IDesignerHost host = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
			IComponentChangeService changeService = site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
			if (host == null && changeService == null)
			{
				return s_inactive;
			}
			DesignerTransaction transaction = host?.CreateTransaction(description);
			DesignEditScope scope = new DesignEditScope(changeService, transaction, members);
			try
			{
				if (changeService != null)
				{
					foreach (KeyValuePair<object, MemberDescriptor> member in members)
					{
						changeService.OnComponentChanging(member.Key, member.Value);
					}
				}
			}
			catch
			{
				scope.Dispose();
				throw;
			}
			return scope;
		}

		private static ISite GetDesignSite(object component)
		{
			ISite site = (component as IComponent)?.Site;
			return site != null && site.DesignMode ? site : null;
		}

		// Undo restores only the reported member, so a member that code serialization skips is reported as the whole component.
		private static MemberDescriptor FindMember(object component, string memberName)
		{
			if (string.IsNullOrEmpty(memberName))
			{
				return null;
			}
			PropertyDescriptor property = TypeDescriptor.GetProperties(component)[memberName];
			if (property == null || property.SerializationVisibility == DesignerSerializationVisibility.Hidden)
			{
				return null;
			}
			return property;
		}

		private void CancelOpenTransaction()
		{
			if (_transaction != null && !_transaction.Committed && !_transaction.Canceled)
			{
				_transaction.Cancel();
			}
		}
	}
}
