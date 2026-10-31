using System;
using System.ComponentModel.Design;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class GalleryDesigner : CommandDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerbCollection verbs = base.Verbs;
				verbs.Add(new DesignerVerb("New Gallery Item", OnNewItem));
				return verbs;
			}
		}

		public Gallery Gallery => base.Component as Gallery;

		public GalleryDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnNewItem(object sender, EventArgs eventArgs)
		{
			if (Gallery == null)
			{
				return;
			}
			DesignerChange.Run(Gallery, "GalleryItems", "New Gallery Item", () =>
			{
				GalleryItem galleryItem = new GalleryItem();
				Gallery.GalleryItems.Add(galleryItem);
				DesignerDragHelper.NameFromSite(galleryItem, nameFromSite: true);
			});
		}
	}
}
