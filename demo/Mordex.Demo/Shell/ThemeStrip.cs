using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Demo
{
	/// <summary>
	/// A small strip in the page header: a list of the looks and the switch that follows the light or dark setting of
	/// Windows. It lets a visitor change the theme from any page.
	/// </summary>
	public sealed class ThemeStrip : Panel
	{
		private const string Classic = "Classic look";

		private readonly ComboBox _list = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
		private readonly CheckBox _follow = new CheckBox { Text = "Follow Windows light/dark", AutoSize = false, Dock = DockStyle.Top, Height = 24 };
		private bool _updating;

		public ThemeStrip()
		{
			Width = 250;
			Padding = new Padding(0, 6, 12, 0);
			_list.Items.Add(Classic);
			foreach (MxTheme theme in MxThemeManager.RegisteredThemes)
			{
				if (theme.Family != "Standard")
				{
					_list.Items.Add(theme.Name);
				}
			}
			_list.SelectedIndex = 0;
			_list.SelectedIndexChanged += (sender, e) =>
			{
				if (!_updating)
				{
					ThemeChosen?.Invoke(this, _list.SelectedIndex <= 0 ? null : MxThemeManager.Find((string)_list.SelectedItem));
				}
			};
			_follow.CheckedChanged += (sender, e) =>
			{
				if (!_updating)
				{
					FollowChanged?.Invoke(this, _follow.Checked);
				}
			};
			Controls.Add(_follow);
			Controls.Add(_list);
			_list.BringToFront();
		}

		/// <summary>Raised with the chosen theme, or null for the classic look.</summary>
		public event EventHandler<MxTheme> ThemeChosen;

		public event EventHandler<bool> FollowChanged;

		/// <summary>Shows the current look without raising the events.</summary>
		public void ShowState(bool themed, MxTheme theme, bool follow)
		{
			_updating = true;
			try
			{
				int index = themed && theme != null ? _list.Items.IndexOf(theme.Name) : 0;
				_list.SelectedIndex = index < 0 ? 0 : index;
				_follow.Checked = follow;
			}
			finally
			{
				_updating = false;
			}
		}
	}
}
