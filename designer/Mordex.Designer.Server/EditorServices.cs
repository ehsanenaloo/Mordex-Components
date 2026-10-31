#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Mordex.Designer.Server
{
	// Gives a classic UITypeEditor the services it expects, with dialogs shown by the server process.
	internal sealed class EditorServices : IServiceProvider, IWindowsFormsEditorService, ITypeDescriptorContext
	{
		private readonly IDesignerHost _host;

		public EditorServices(IDesignerHost host, object instance, PropertyDescriptor property)
		{
			_host = host;
			Instance = instance;
			PropertyDescriptor = property;
		}

		public IContainer? Container => _host.Container;

		public object? Instance { get; }

		public PropertyDescriptor? PropertyDescriptor { get; }

		public object? GetService(Type serviceType)
		{
			if (serviceType == typeof(IWindowsFormsEditorService) || serviceType == typeof(ITypeDescriptorContext))
			{
				return this;
			}
			object? service = _host.GetService(serviceType);
			if (service == null && serviceType == typeof(IExtenderListService))
			{
				return new ContainerExtenderList(_host);
			}
			return service;
		}

		// The server host does not publish an extender list, so build one from the components in the container.
		private sealed class ContainerExtenderList : IExtenderListService
		{
			private readonly IDesignerHost _host;

			public ContainerExtenderList(IDesignerHost host)
			{
				_host = host;
			}

			public IExtenderProvider[] GetExtenderProviders()
			{
				List<IExtenderProvider> providers = new List<IExtenderProvider>();
				foreach (IComponent component in _host.Container.Components)
				{
					if (component is IExtenderProvider provider)
					{
						providers.Add(provider);
					}
				}
				return providers.ToArray();
			}
		}

		public bool OnComponentChanging()
		{
			IComponentChangeService? changes = (IComponentChangeService?)_host.GetService(typeof(IComponentChangeService));
			changes?.OnComponentChanging(Instance!, PropertyDescriptor);
			return true;
		}

		public void OnComponentChanged()
		{
			IComponentChangeService? changes = (IComponentChangeService?)_host.GetService(typeof(IComponentChangeService));
			changes?.OnComponentChanged(Instance!, PropertyDescriptor, null, null);
		}

		public void CloseDropDown()
		{
			_dropDown?.Close();
		}

		private Form? _dropDown;

		public void DropDownControl(Control control)
		{
			using (Form form = new Form())
			using (System.Windows.Forms.Timer watcher = new System.Windows.Forms.Timer())
			{
				_dropDown = form;
				form.FormBorderStyle = FormBorderStyle.None;
				form.ShowInTaskbar = false;
				form.StartPosition = FormStartPosition.Manual;
				form.TopMost = true;
				form.KeyPreview = true;
				form.ClientSize = control.Size;
				form.Controls.Add(control);
				control.Dock = DockStyle.Fill;
				Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
				int x = Math.Max(area.Left, Math.Min(Cursor.Position.X, area.Right - form.Width));
				int y = Math.Max(area.Top, Math.Min(Cursor.Position.Y, area.Bottom - form.Height));
				form.Location = new Point(x, y);
				form.Shown += (sender, e) => form.Activate();
				form.Deactivate += (sender, e) => form.Close();
				form.KeyDown += (sender, e) =>
				{
					if (e.KeyCode == Keys.Escape)
					{
						form.Close();
					}
				};
				// The form can open without focus, so a click outside has to close it as well.
				watcher.Interval = 50;
				watcher.Tick += (sender, e) =>
				{
					if (AnyMouseButtonDown() && !form.Bounds.Contains(Cursor.Position))
					{
						form.Close();
					}
				};
				watcher.Start();
				form.ShowDialog();
				watcher.Stop();
				form.Controls.Remove(control);
				_dropDown = null;
			}
		}
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int key);

		private static bool AnyMouseButtonDown()
		{
			return (GetAsyncKeyState(0x01) & 0x8000) != 0 || (GetAsyncKeyState(0x02) & 0x8000) != 0;
		}

		public DialogResult ShowDialog(Form dialog)
		{
			return dialog.ShowDialog();
		}
	}
}
