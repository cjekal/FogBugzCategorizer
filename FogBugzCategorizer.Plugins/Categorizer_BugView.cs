﻿using System.Collections.Generic;
using System.Data;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI.Dialog;
using Newtonsoft.Json;

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
	<a id=""CategorizerSave"" href="""">Save Selections</a>
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
			var projectTaskLookupTableName = GetPluginTableName(Tables.PROJECT_TASK_LOOKUP);

			if (api.Request["Command"] == "GetProjects")
			{
				var projectsQuery = api.Database.NewSelectQuery(projectTaskLookupTableName);
				projectsQuery.AddSelect(string.Format("{0}.Project", projectTaskLookupTableName));
				projectsQuery.AddOrderBy(string.Format("{0}.Project ASC", projectTaskLookupTableName));
				var projectsData = projectsQuery.GetDataSet();

				if (projectsData.Tables[0].Rows.Count == 0)
				{
					return null;
				}

				var projects = new List<Project>(projectsData.Tables[0].AsEnumerable().Select(r => r.Field<string>("Project")).Distinct().Select(p => new Project{Name = p}));

				return JsonConvert.SerializeObject(projects);
			}

			if (api.Request["Command"] == "GetTasks")
			{
				string projectName = api.Request["Project"];
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

				var tasks = new List<Task>(tasksData.Tables[0].AsEnumerable().Select(r => r.Field<string>("Task")).Distinct().Select(t => new Task { Name = t, Project = project }));

				return JsonConvert.SerializeObject(tasks);
			}

			if (api.Request["Command"] == "SaveCategories")
			{
				string categories = Html.HtmlDecode(api.Request.RawPost());
				if (string.IsNullOrEmpty(categories))
				{
					return null;
				}

				var bugId = api.Bug.CurrentBug();
				var userName = api.Person.GetCurrentPerson().sFullName;
				var splitTableName = GetPluginTableName(Tables.SPLIT_TABLE);
				var splitDetailsTableName = GetPluginTableName(Tables.SPLIT_DETAILS_TABLE);

				var splitQuery = api.Database.NewSelectQuery(splitTableName);
				splitQuery.AddWhere(string.Format("{0}.ixBug = {1}", splitTableName, bugId));
				var splitData = splitQuery.GetDataSet();

				int splitId;
				if (splitData.Tables[0].Rows.Count == 0)
				{
					var insertSplitQuery = api.Database.NewInsertQuery(splitTableName);
					insertSplitQuery.InsertInt("ixBug", bugId);
					insertSplitQuery.InsertString("LastEditor", userName);
					splitId = insertSplitQuery.Execute();
				}
				else
				{
					var updateSplitQuery = api.Database.NewUpdateQuery(splitTableName);
					updateSplitQuery.UpdateString("LastEditor", userName);
					updateSplitQuery.AddWhere(string.Format("{0}.ixBug = {1}", splitTableName, bugId));
					updateSplitQuery.Execute();

					splitId = splitData.Tables[0].Rows[0].Field<int>("Id");
					var deleteAllSplitDetailsQuery = api.Database.NewDeleteQuery(splitDetailsTableName);
					deleteAllSplitDetailsQuery.AddWhere(string.Format("{0}.SplitId = {1}", splitDetailsTableName, splitId));
					deleteAllSplitDetailsQuery.Execute();
				}

				var tasks = JsonConvert.DeserializeObject<List<Task>>(categories);
				foreach(var task in tasks)
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