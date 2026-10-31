using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class CustomizeForm
	{
		private void OnCloseClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnControlTextChanged(object sender, EventArgs eventArgs)
		{
			if (lvwToolbars == null || lvwToolbars.IsDisposed)
			{
				return;
			}
			for (int i = 0; i < lvwToolbars.Items.Count; i++)
			{
				ListViewItem listViewItem = lvwToolbars.Items[i];
				if (listViewItem.Tag == sender)
				{
					MxCommandBar uiCommandBar = (MxCommandBar)listViewItem.Tag;
					listViewItem.Text = uiCommandBar.Text;
				}
			}
		}

		private void OnControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (lvwToolbars == null || lvwToolbars.IsDisposed)
			{
				return;
			}
			for (int i = 0; i < lvwToolbars.Items.Count; i++)
			{
				ListViewItem listViewItem = lvwToolbars.Items[i];
				if (listViewItem.Tag == sender)
				{
					MxCommandBar uiCommandBar = (MxCommandBar)listViewItem.Tag;
					if (listViewItem.Checked != uiCommandBar.Visible)
					{
						listViewItem.Selected = uiCommandBar.Visible;
					}
				}
			}
		}

		private void OnControlCommandBarAdded(object sender, CommandBarEventArgs eventArgs)
		{
			ListViewItem listViewItem = new ListViewItem(eventArgs.CommandBar.Text);
			listViewItem.Checked = eventArgs.CommandBar.Visible;
			listViewItem.Tag = eventArgs.CommandBar;
			if (lvwToolbars != null && !lvwToolbars.IsDisposed)
			{
				lvwToolbars.Items.Add(listViewItem);
			}
			eventArgs.CommandBar.VisibleChanged += OnControlVisibleChanged;
			eventArgs.CommandBar.TextChanged += OnControlTextChanged;
		}

		private void OnControlCommandBarRemoved(object sender, CommandBarEventArgs eventArgs)
		{
			eventArgs.CommandBar.VisibleChanged -= OnControlVisibleChanged;
			eventArgs.CommandBar.TextChanged -= OnControlTextChanged;
			if (lvwToolbars == null || lvwToolbars.IsDisposed)
			{
				return;
			}
			foreach (ListViewItem item in lvwToolbars.Items)
			{
				if (item.Tag == eventArgs.CommandBar)
				{
					lvwToolbars.Items.Remove(item);
					break;
				}
			}
		}

		private void OnNewClick(object sender, EventArgs eventArgs)
		{
			CommandManager.ProcessUICommandBar(null);
		}

		private void OnRenameClick(object sender, EventArgs eventArgs)
		{
			if (lvwToolbars.SelectedItems.Count > 0 && lvwToolbars.SelectedItems[0].Tag is MxCommandBar)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)lvwToolbars.SelectedItems[0].Tag;
				CommandManager.ProcessUICommandBar(uiCommandBar);
			}
		}

		private void OnResetClick(object sender, EventArgs eventArgs)
		{
			if (lvwToolbars.SelectedItems.Count > 0 && lvwToolbars.SelectedItems[0].Tag is MxCommandBar)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)lvwToolbars.SelectedItems[0].Tag;
				string text = _commandManager.BuiltInTexts[CommandsBuiltInText.ResetToolBarWarning];
				string caption = "";
				if (CommandManager.GetForm() != null)
				{
					caption = CommandManager.GetForm().Text;
				}
				DialogResult dialogResult = ((text.IndexOf("{0}") < 0) ? Mordex.Windows.Common.MxMessageBox.Show(this, text, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) : Mordex.Windows.Common.MxMessageBox.Show(this, string.Format(text, uiCommandBar.Text), caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation));
				if (dialogResult == DialogResult.OK)
				{
					uiCommandBar.ResetCommands();
				}
			}
		}

		private void OnDeleteClick(object sender, EventArgs eventArgs)
		{
			if (lvwToolbars.SelectedItems.Count > 0 && lvwToolbars.SelectedItems[0].Tag is MxCommandBar)
			{
				MxCommandBar commandBar = (MxCommandBar)lvwToolbars.SelectedItems[0].Tag;
				CommandManager.CommandBars.Remove(commandBar);
			}
		}

		private void OnResetUsageDataClick(object sender, EventArgs eventArgs)
		{
			string text = _commandManager.BuiltInTexts[CommandsBuiltInText.ResetUsageDataMessage];
			string caption = "";
			if (CommandManager.GetForm() != null)
			{
				caption = CommandManager.GetForm().Text;
			}
			DialogResult dialogResult = Mordex.Windows.Common.MxMessageBox.Show(this, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.Yes)
			{
				CommandManager.ResetUsageData();
			}
		}

		private void OnKeyboardClick(object sender, EventArgs eventArgs)
		{
			CommandManager.ShowKeyboardDialog();
		}

		private void OnAlwaysShowChanged(object sender, EventArgs eventArgs)
		{
			_commandManager.AlwaysShowFullMenus = chkAlwaysShow.Checked;
			if (chkAlwaysShow.Checked)
			{
				chkShowAfterDelay.Enabled = false;
			}
			else
			{
				chkShowAfterDelay.Enabled = true;
			}
		}

		private void OnShowAfterDelayChanged(object sender, EventArgs eventArgs)
		{
			if (chkShowAfterDelay.Checked)
			{
				CommandManager.ShowFullMenuDelay = DefaultShowFullMenuDelay;
			}
			else
			{
				CommandManager.ShowFullMenuDelay = -1;
			}
		}

		private void OnLargeIconsChanged(object sender, EventArgs eventArgs)
		{
			if (chkLargeIcons.Checked)
			{
				CommandManager.View = View.LargeIcons;
			}
			else
			{
				CommandManager.View = View.SmallIcons;
			}
		}

		private void OnShortcutTipsChanged(object sender, EventArgs eventArgs)
		{
			CommandManager.ShowShortcutInToolTips = chkShortcutTips.Checked;
		}

		private void OnFontNameChanged(object sender, EventArgs eventArgs)
		{
			CommandManager.UseFontInFontNames = chkFontName.Checked;
		}

		private void OnScreenTipsChanged(object sender, EventArgs eventArgs)
		{
			CommandManager.ShowToolTips = chkScreenTips.Checked;
			if (chkScreenTips.Checked)
			{
				chkShortcutTips.Enabled = true;
			}
			else
			{
				chkShortcutTips.Enabled = false;
			}
		}

		private void OnAnimationSelectedValueChanged(object sender, EventArgs eventArgs)
		{
			CommandManager.MenuAnimation = (DropDownAnimation)jcboAnimation.SelectedItem.Value;
		}

		private void OnListViewSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (lvwToolbars.SelectedItems.Count > 0)
			{
				if (lvwToolbars.SelectedItems[0].Tag is MxCommandBar)
				{
					MxCommandBar uiCommandBar = (MxCommandBar)lvwToolbars.SelectedItems[0].Tag;
					if (uiCommandBar.AllowDelete)
					{
						btnDelete.Enabled = true;
					}
					if (uiCommandBar.AllowRename)
					{
						btnRename.Enabled = true;
					}
					if (uiCommandBar.Commands._originalCommands != null && !uiCommandBar.Commands._isMerging)
					{
						btnReset.Enabled = true;
					}
					else
					{
						btnReset.Enabled = false;
					}
				}
			}
			else
			{
				btnRename.Enabled = false;
				btnDelete.Enabled = false;
				btnReset.Enabled = false;
			}
		}

		private void OnListViewItemCheck(object sender, ItemCheckEventArgs eventArgs)
		{
			MxCommandBar uiCommandBar = (MxCommandBar)lvwToolbars.Items[eventArgs.Index].Tag;
			if (eventArgs.NewValue != CheckState.Checked)
			{
				if (uiCommandBar.CanClose())
				{
					uiCommandBar.Visible = false;
				}
			}
			else
			{
				uiCommandBar.Visible = eventArgs.NewValue == CheckState.Checked;
			}
			ListViewItem listViewItem = lvwToolbars.Items[eventArgs.Index];
			listViewItem.Focused = true;
			listViewItem.Selected = true;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_isLoopDone = true;
			base.OnFormClosing(e);
			if (CommandManager.GetForm() != null)
			{
				_commandManager.GetForm().Activate();
			}
		}

		private void OnListBoxDrawItem(object sender, DrawItemEventArgs eventArgs)
		{
			if (eventArgs.Bounds.Width <= 0 || eventArgs.Bounds.Height <= 0)
			{
				return;
			}
			// An item is drawn only after MeasureItem has added its layout state.
			CommandLayoutState uiCommandLayoutState = _commandListItems.FindItemByIndex(eventArgs.Index) as CommandLayoutState;
			if (uiCommandLayoutState == null)
			{
				return;
			}
			using (Bitmap bitmap = new Bitmap(eventArgs.Bounds.Width, eventArgs.Bounds.Height))
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				uiCommandLayoutState.SetPressed((eventArgs.State & DrawItemState.Selected) != 0);
				((FormatStyleLayoutState)uiCommandLayoutState).SetEnabled(true);
				_commandItemRenderer.DrawItem(graphics, uiCommandLayoutState, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
				eventArgs.Graphics.DrawImage(bitmap, eventArgs.Bounds, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
			}
		}

		private void OnListBoxMeasureItem(object sender, MeasureItemEventArgs eventArgs)
		{
			if (eventArgs.Index < 0 || eventArgs.Index >= lbCommands.Items.Count || !(lbCommands.Items[eventArgs.Index] is MxCommand uiCommand))
			{
				return;
			}
			CommandLayoutState uiCommandLayoutState = new CommandLayoutState(uiCommand, uiCommand.GetEffectiveText());
			if (uiCommandLayoutState._caption.Length == 0)
			{
				uiCommandLayoutState._caption = uiCommandLayoutState.GetCommand().Key;
			}
			eventArgs.ItemHeight = _commandItemRenderer.MeasureItemHeight(uiCommand, lbCommands.Font);
			_commandListItems.AddItem(uiCommandLayoutState);
		}

		private void OnListBoxMouseDown(object sender, MouseEventArgs eventArgs)
		{
			if (lbCommands.SelectedIndex >= 0)
			{
				SetSelectedBar(null);
				MxCommand uiCommand = (MxCommand)lbCommands.Items[lbCommands.SelectedIndex];
				if (uiCommand != null)
				{
					CommandDragState uiCommandCollectionLayoutState = new CommandDragState(_commandManager, uiCommand);
					uiCommandCollectionLayoutState.RunDragLoop();
				}
				lbCommands.Invalidate();
			}
		}

		private void OnListBoxSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			if (lbCategories.SelectedItem != null)
			{
				ON oN = (ON)lbCategories.SelectedItem;
				CommandCategory uiCommandCategory = null;
				if (oN.Object != null)
				{
					uiCommandCategory = (CommandCategory)oN.Object;
				}
				FillCommandList(uiCommandCategory);
			}
		}

		private void OnDescriptionClick(object sender, EventArgs eventArgs)
		{
			if (_tipHandler != null)
			{
				DisposeTipHandler();
			}
			else if (CommandManager.GetSelectedCommand() != null)
			{
				string text = CommandManager.GetSelectedCommand().Description;
				if (text.Length <= 0)
				{
					text = _commandManager.BuiltInTexts[CommandsBuiltInText.NoDescriptionAvailable];
				}
				_tipHandler = new WindowMessageHandler(Font, CommandManager.UseCompatibleTextRendering);
				_tipHandler._isTipRequested = true;
				Point point = btnDescription.PointToScreen(new Point(0, 8));
				_tipHandler._anchorPoint = point;
				_tipHandler.RequestShow(text, this);
			}
		}

		private void OnModifySelectionClick(object sender, EventArgs eventArgs)
		{
			if (_commandManager.GetSelectedCommand() != null)
			{
				Point point = btnModifySelection.PointToScreen(new Point(0, 0));
				_commandManager.ShowCommandMenu(point, btnModifySelection.Height);
			}
		}
	}
}
