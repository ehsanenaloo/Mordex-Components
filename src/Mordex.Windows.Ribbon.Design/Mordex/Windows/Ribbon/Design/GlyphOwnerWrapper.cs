using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class GlyphOwnerWrapper
	{
		private object _glyphOwner;

		internal Control OwnerControl => _glyphOwner as Control;

		internal GlyphOwnerWrapper(object glyphOwner)
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			if (!(glyphOwner is MxRibbon) && !(glyphOwner is MxRibbonStatusBar))
			{
				throw new InvalidOperationException();
			}
			_glyphOwner = glyphOwner;
		}

		internal Point PointToClient(Point screenPoint)
		{
			return OwnerControl?.PointToClient(screenPoint) ?? screenPoint;
		}

		internal CommandBase GetCommandAt(Point pt)
		{
			return DesignHelper.GetCommandAt(_glyphOwner, pt);
		}

		internal Rectangle GetCommandBounds(CommandBase command)
		{
			return DesignHelper.GetCommandBounds(_glyphOwner, command);
		}

		internal Rectangle GetBackstageBarBounds()
		{
			return DesignHelper.GetBackstageBarBounds(_glyphOwner);
		}
	}
}
