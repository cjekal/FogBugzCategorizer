using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;

namespace FogBugzCategorizer.Plugins
{
	public class FogBugzCategorizer : Plugin, IPluginBugDisplay
	{
		public FogBugzCategorizer(CPluginApi api) : base(api)
		{
		}

		#region Implementation of IPluginBugDisplay

		public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayEditLeft(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		#endregion
	}
}