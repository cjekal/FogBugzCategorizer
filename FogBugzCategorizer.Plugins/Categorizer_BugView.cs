using System;
using System.Collections.Generic;
using System.Linq;
using FogBugz.Categorizer.Plugins.Business;
using FogBugz.Categorizer.Plugins.Business.DataProviders;
using FogBugzCategorizer.Plugins.FormHelpers;
using FogCreek.Core;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI.Dialog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FogBugzCategorizer.Plugins
{
	public partial class Categorizer : IPluginBugDisplay, IPluginJS, IPluginRawPageDisplay, IPluginCSS
	{
		private readonly ProjectsProvider _projectsProvider;
		private readonly TasksProvider _tasksProvider;
		private List<string> _authorizedCategorizers;

		public Categorizer(CPluginApi api) : this(api, new ProjectsProvider(), new TasksProvider()) 
		{
		}

		public Categorizer(CPluginApi api, ProjectsProvider projectsProvider, TasksProvider tasksProvider) : base(api)
		{
			_projectsProvider = projectsProvider;
			_tasksProvider = tasksProvider;
		}

		#region Implementation of IPluginBugDisplay

		public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
		{
			if (!AuthorizedCategorizers.Contains(UserName))
			{
				return null;
			}

			var displayItem = new CBugDisplayDialogItem("BugDisplayViewTop")
            {
				iColumnSpan = 4,
                sContent = CategorizerForms.GetBugzViewHtml()
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
			if (!AuthorizedCategorizers.Contains(UserName))
			{
				return null;
			}

			var jsInfo = new CJSInfo
			             	{
			             		rgsStaticFiles = new[] {Statics.CategorizerJS},
			             		sInlineJS = GetCategorizerScript()
			             	};
			return jsInfo;
		}

		#endregion

		#region Implementation of IPluginRawPageDisplay

		public string RawPageDisplay()
		{
			if (!AuthorizedCategorizers.Contains(UserName))
			{
				return null;
			}

			if (api.Request["Command"] == "LoadAll")
			{
				var projects = _projectsProvider.GetAll(api);
				var bugzId = Convert.ToInt32(api.Request["BugzId"]);
				var selected = _tasksProvider.GetSelected(api, bugzId);
				return JsonConvert.SerializeObject(new LoadAllResponse{ Projects = projects, Selected = selected });
			}

			if (api.Request["Command"] == "GetTasks")
			{
				var tasks = _tasksProvider.GetAll(api, Html.HtmlDecode(api.Request["Project"]));
				return JsonConvert.SerializeObject(tasks);
			}

			if (api.Request.HttpMethod == "POST")
			{
				var rawPost = api.Request.RawPost();
				var json = JObject.Parse(rawPost);

				if ((string) json["Command"] == "SaveCategories")
				{
					var fragments = json["Categories"].Children();
					var tasks = fragments.Select(f => JsonConvert.DeserializeObject<Task>(f.ToString())).ToList();
					var bugzId = (int) json["BugzId"];

					_tasksProvider.SaveSelected(api, bugzId, tasks, UserName);

					return "yay, done updating!";
				}
				return null;
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
			if (!AuthorizedCategorizers.Contains(UserName))
			{
				return null;
			}
			
			return new CCSSInfo { rgsStaticFiles = new[] { Statics.CategorizerCSS } };
		}

		#endregion

		private string GetCategorizerScript()
		{
			return CategorizerForms.GetBugzViewJS(api.Url.PluginRawPageUrl(Statics.PluginId), api.Bug.CurrentBug());
		}

		private string UserName
		{
			get { return api.Person.GetCurrentPerson().sFullName; }
		}

		protected List<string> AuthorizedCategorizers
		{
			get
			{
				if (_authorizedCategorizers == null)
				{
					_authorizedCategorizers = JsonConvert.DeserializeObject<List<string>>(api.Database.GetKeyValue("AuthorizedCategorizers"));
				}
				return _authorizedCategorizers;
			}
		} 
	}
}