using System;
using System.ComponentModel;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class SeparatorCommand : CommandBase, IKeyTipProvider, IInvalidatable
	{
		private string _title;

		private RibbonForegroundStyle _foregroundStyle;

		[Description("The text to be displayed (rather than a horizontal line).")]
		[DefaultValue("")]
		[Localizable(true)]
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_title != value)
				{
					_title = value;
					NotifyTextChanged("Title");
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the the color and FontStyle used to draw the command\u00b4s text.")]
		public RibbonForegroundStyle ForegroundStyle => _foregroundStyle;

		internal override CommandType CommandType => CommandType.Separator;

		bool IKeyTipProvider.HasKeyTip => false;

		string IKeyTipProvider.KeyTip => "";

		public SeparatorCommand()
		{
		}

		public SeparatorCommand(string key)
			: base(key)
		{
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_title = "";
			_foregroundStyle = new RibbonForegroundStyle(this);
		}

		void IInvalidatable.Invalidate()
		{
			InvalidateLayout();
		}

		public override object Clone()
		{
			SeparatorCommand separatorCommand = new SeparatorCommand();
			separatorCommand.Enabled = Enabled;
			separatorCommand.Key = base.Key;
			separatorCommand.Tag = base.Tag;
			separatorCommand._title = _title;
			separatorCommand.Visible = base.Visible;
			separatorCommand._foregroundStyle = _foregroundStyle.CloneForOwner(separatorCommand);
			separatorCommand.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			return separatorCommand;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_foregroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _foregroundStyle);
			}
			if (_title.Length > 0)
			{
				writer.AddValue("Title", _title);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			PropertyValue propertyValue = reader["ForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_foregroundStyle);
			}
			else
			{
				_foregroundStyle.ResetAll();
			}
			_title = (string)LayoutReader.GetPropertyValue(reader, "Title", typeof(string), "");
		}
	}
}
