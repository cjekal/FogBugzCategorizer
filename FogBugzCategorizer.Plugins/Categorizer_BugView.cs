using System.Collections.Generic;
using FogCreek.Core;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI.Dialog;

namespace FogBugzCategorizer.Plugins
{
	public partial class Categorizer : IPluginBugDisplay, IPluginJS, IPluginRawPageDisplay, IPluginCSS
	{
		#region Implementation of IPluginBugDisplay

		public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
		{
			var displayItem = new CBugDisplayDialogItem("BugDisplayViewTop")
            {
				iColumnSpan = 4,
                sContent = @"
<a id=""Categorizer"" href="""">Categorizer</a>
<div id=""CategorizerDiv"" class=""categorizerContainer"" style=""display: none;"">
	<div id=""CategorizerProjects"" class=""categorizerContent categorizerLeft""></div>
	<div id=""CategorizerTasks"" class=""categorizerContent categorizerRight""></div>
	<div id=""SelectedCategories"" class=""categorizerSelected categorizerBottom""></div>
</div>"
			};

			return new[] { displayItem };
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

		#region Implementation of IPluginJS

		public CJSInfo JSInfo()
		{
			var jsInfo = new CJSInfo
			             	{
			             		rgsStaticFiles = new[] {Statics.JQuery, Statics.CategorizerJS},
			             		sInlineJS = GetCategorizerScript()
			             	};
			return jsInfo;
		}

		private string GetCategorizerScript()
		{
			return string.Format(@"
var settings = {{
	url: '{0}'
}};
", api.Url.PluginRawPageUrl(PLUGIN_ID));
		}

		#endregion

		#region Implementation of IPluginRawPageDisplay

		public string RawPageDisplay()
		{
			if (api.Request["Command"] == "GetProjects")
			{
				var projects = new List<Project>
			                         	{
			                         		new Project {Name = "Project 1"},
			                         		new Project {Name = "Project 2"},
			                         		new Project {Name = "Project 3"},
			                         	};
				return Json.Serialize(projects);
			}

			if (api.Request["Command"] == "GetTasks")
			{
				string projectName = api.Request["Project"];
				Project project = new Project {Name = projectName};
				if (projectName == null)
				{
					return null;
				}

				var tasks = new List<Task>
				            	{
				            		new Task {Name = "Task 1", Project = project},
				            		new Task {Name = "Task 2", Project = project},
				            		new Task {Name = "Task 3", Project = project}
				            	};
				return Json.Serialize(tasks);
			}

			return null;
		}

		public PermissionLevel RawPageVisibility()
		{
			return PermissionLevel.Normal;
		}

		#endregion

		#region Implementation of IPluginCSS

		public CCSSInfo CSSInfo()
		{
			return new CCSSInfo {rgsStaticFiles = new[] {Statics.CategorizerCSS}};
		}

		#endregion
	}
}