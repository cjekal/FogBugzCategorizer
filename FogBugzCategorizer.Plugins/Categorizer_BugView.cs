using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FogBugz.Categorizer.Plugins.Business;
using FogCreek.Core;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI.Dialog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
<a id=""Categorizer"" class=""categorizer"">Categorizer</a>
<div id=""CategorizerDiv"" class=""categorizerContainer"" style=""display: none;"">
	<div class=""categorizerNotificationsContainer"">
		<div id=""CategorizerNotifications"" class=""categorizerNotifications"" style=""display: none;"">Please wait! System is busy!</div>
	</div>
	<div id=""CategorizerProjects"" class=""categorizerContent categorizerLeft""></div>
	<div id=""CategorizerTasks"" class=""categorizerContent categorizerRight""></div>
	<div id=""SelectedCategories"" class=""categorizerSelected categorizerBottom""></div>
	<div class=""categorizerSave"">
		<a id=""CategorizerSave"" class=""categorizer"">Save Selections</a>
	</div>
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
			             		rgsStaticFiles = new[] {Statics.CategorizerJS},
			             		sInlineJS = GetCategorizerScript()
			             	};
			return jsInfo;
		}

		#endregion

		#region Implementation of IPluginRawPageDisplay

		public string RawPageDisplay()
		{
			var projectTaskLookupTableName = GetPluginTableName(Tables.PROJECT_TASK_LOOKUP);
			var splitTableName = GetPluginTableName(Tables.SPLIT_TABLE);
			var splitDetailsTableName = GetPluginTableName(Tables.SPLIT_DETAILS_TABLE);

			if (api.Request["Command"] == "LoadAll")
			{
				var projectsQuery = api.Database.NewSelectQuery(projectTaskLookupTableName);
				projectsQuery.AddSelect(string.Format("{0}.Project", projectTaskLookupTableName));
				projectsQuery.AddOrderBy(string.Format("{0}.Project ASC", projectTaskLookupTableName));
				projectsQuery.Distinct = true;
				var projectsData = projectsQuery.GetDataSet();

				if (projectsData.Tables[0].Rows.Count == 0)
				{
					return null;
				}

				var projects = new List<Project>(projectsData.Tables[0].AsEnumerable().Select(r => new Project { Name = r.Field<string>("Project") }));

				var bugzId = Convert.ToInt32(api.Request["BugzId"]);

				var selectedQuery = api.Database.NewSelectQuery(splitTableName);
				selectedQuery.AddInnerJoin(splitDetailsTableName, string.Format("{0}.Id = {1}.SplitId", splitTableName, splitDetailsTableName));
				selectedQuery.AddSelect(string.Format("{0}.Project", splitDetailsTableName));
				selectedQuery.AddSelect(string.Format("{0}.Task", splitDetailsTableName));
				selectedQuery.AddWhere(string.Format("{0}.ixBug = {1}", splitTableName, bugzId));
				selectedQuery.Distinct = true;
				var selectedData = selectedQuery.GetDataSet();

				var selected = new List<Task>(selectedData.Tables[0].AsEnumerable().Select(r => new Task { Project = new Project { Name = r.Field<string>("Project") }, Name = r.Field<string>("Task") }));

				return JsonConvert.SerializeObject(new LoadAllResponse{ Projects = projects, Selected = selected });
			}

			if (api.Request["Command"] == "GetTasks")
			{
				string projectName = Html.HtmlDecode(api.Request["Project"]);
				Project project = new Project {Name = projectName};
				if (string.IsNullOrEmpty(projectName))
				{
					return null;
				}

				var tasksQuery = api.Database.NewSelectQuery(projectTaskLookupTableName);
				tasksQuery.AddSelect(string.Format("{0}.Task", projectTaskLookupTableName));
				tasksQuery.AddWhere(string.Format("{0}.Project = '{1}'", projectTaskLookupTableName, projectName));
				tasksQuery.AddOrderBy(string.Format("{0}.Task ASC", projectTaskLookupTableName));
				var tasksData = tasksQuery.GetDataSet();

				if (tasksData.Tables[0].Rows.Count == 0)
				{
					return null;
				}

				var tasks = new List<Task>(tasksData.Tables[0].AsEnumerable().Select(r => new Task { Name = r.Field<string>("Task"), Project = project }));

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
					var userName = api.Person.GetCurrentPerson().sFullName;

					var splitQuery = api.Database.NewSelectQuery(splitTableName);
					splitQuery.AddWhere(string.Format("{0}.ixBug = {1}", splitTableName, bugzId));
					var splitData = splitQuery.GetDataSet();

					int splitId;
					if (splitData.Tables[0].Rows.Count == 0)
					{
						var insertSplitQuery = api.Database.NewInsertQuery(splitTableName);
						insertSplitQuery.InsertInt("ixBug", bugzId);
						insertSplitQuery.InsertString("LastEditor", userName);
						splitId = insertSplitQuery.Execute();
					}
					else
					{
						var updateSplitQuery = api.Database.NewUpdateQuery(splitTableName);
						updateSplitQuery.UpdateString("LastEditor", userName);
						updateSplitQuery.AddWhere(string.Format("{0}.ixBug = {1}", splitTableName, bugzId));
						updateSplitQuery.Execute();

						splitId = splitData.Tables[0].Rows[0].Field<int>("Id");
						var deleteAllSplitDetailsQuery = api.Database.NewDeleteQuery(splitDetailsTableName);
						deleteAllSplitDetailsQuery.AddWhere(string.Format("{0}.SplitId = {1}", splitDetailsTableName, splitId));
						deleteAllSplitDetailsQuery.Execute();
					}

					foreach (var task in tasks)
					{
						var taskInsertQuery = api.Database.NewInsertQuery(splitDetailsTableName);
						taskInsertQuery.InsertInt("SplitId", splitId);
						taskInsertQuery.InsertString("Project", task.Project.Name);
						taskInsertQuery.InsertString("Task", task.Name);
						taskInsertQuery.Execute();
					}

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
			return new CCSSInfo {rgsStaticFiles = new[] {Statics.CategorizerCSS}};
		}

		#endregion

		private string GetCategorizerScript()
		{
			return string.Format(@"
var settings = {{
	url: '{0}',
	bugzId: {1}
}};
", api.Url.PluginRawPageUrl(Statics.PluginId), api.Bug.CurrentBug());
		}
	}
}