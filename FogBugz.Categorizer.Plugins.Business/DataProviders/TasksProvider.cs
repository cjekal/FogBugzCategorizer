using System.Collections.Generic;
using System.Data;
using System.Linq;
using FogCreek.FogBugz.Plugins.Api;

namespace FogBugz.Categorizer.Plugins.Business.DataProviders
{
	public class TasksProvider
	{
		public List<Task> GetSelected(CPluginApi api, int bugzId)
		{
			var splitTableName = Tables.GetPluginTableName(api, Tables.SPLIT_TABLE);
			var splitDetailsTableName = api.Database.PluginTableName(Statics.PluginId, Tables.SPLIT_DETAILS_TABLE);
			var selectedQuery = api.Database.NewSelectQuery(splitTableName);
			selectedQuery.AddInnerJoin(splitDetailsTableName, string.Format("{0}.Id = {1}.SplitId", splitTableName, splitDetailsTableName));
			selectedQuery.AddSelect(string.Format("{0}.Project", splitDetailsTableName));
			selectedQuery.AddSelect(string.Format("{0}.Task", splitDetailsTableName));
			selectedQuery.AddWhere(string.Format("{0}.ixBug = {1}", splitTableName, bugzId));
			selectedQuery.Distinct = true;
			var selectedData = selectedQuery.GetDataSet();

			return new List<Task>(selectedData.Tables[0].AsEnumerable().Select(r => new Task { Project = new Project { Name = r.Field<string>("Project") }, Name = r.Field<string>("Task") }));
		}

		public List<Task> GetAll(CPluginApi api, string projectName)
		{
			var projectTaskLookupTableName = api.Database.PluginTableName(Statics.PluginId, Tables.PROJECT_TASK_LOOKUP);
			var project = new Project { Name = projectName };
			if (string.IsNullOrEmpty(projectName))
			{
				return new List<Task>();
			}

			var tasksQuery = api.Database.NewSelectQuery(projectTaskLookupTableName);
			tasksQuery.AddSelect(string.Format("{0}.Task", projectTaskLookupTableName));
			tasksQuery.AddWhere(string.Format("{0}.Project = '{1}'", projectTaskLookupTableName, projectName));
			tasksQuery.AddOrderBy(string.Format("{0}.Task ASC", projectTaskLookupTableName));
			var tasksData = tasksQuery.GetDataSet();

			return new List<Task>(tasksData.Tables[0].AsEnumerable().Select(r => new Task { Name = r.Field<string>("Task"), Project = project }));
		}

		public void SaveSelected(CPluginApi api, int bugzId, List<Task> tasks, string userName)
		{
			var splitTableName = api.Database.PluginTableName(Statics.PluginId, Tables.SPLIT_TABLE);
			var splitDetailsTableName = api.Database.PluginTableName(Statics.PluginId, Tables.SPLIT_DETAILS_TABLE);

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

			foreach (var task in tasks.Distinct())
			{
				var taskInsertQuery = api.Database.NewInsertQuery(splitDetailsTableName);
				taskInsertQuery.InsertInt("SplitId", splitId);
				taskInsertQuery.InsertString("Project", task.Project.Name);
				taskInsertQuery.InsertString("Task", task.Name);
				taskInsertQuery.Execute();
			}
		}

		public void SaveTemplate(CPluginApi api, string templateName, List<Task> tasks, string userName)
		{
			var templateTableName = api.Database.PluginTableName(Statics.PluginId, Tables.TEMPLATE_TABLE);
			var templateDetailsTableName = api.Database.PluginTableName(Statics.PluginId, Tables.TEMPLATE_DETAILS_TABLE);

			var templateQuery = api.Database.NewSelectQuery(templateTableName);
			templateQuery.AddWhere(string.Format("{0}.Name = '{1}'", templateTableName, templateName));
			var templateData = templateQuery.GetDataSet();

			int templateId;
			if (templateData.Tables[0].Rows.Count == 0)
			{
				var insertTemplateQuery = api.Database.NewInsertQuery(templateTableName);
				insertTemplateQuery.InsertString("Name", templateName);
				insertTemplateQuery.InsertString("LastEditor", userName);
				templateId = insertTemplateQuery.Execute();
			}
			else
			{
				var updateTemplateQuery = api.Database.NewUpdateQuery(templateTableName);
				updateTemplateQuery.UpdateString("LastEditor", userName);
				updateTemplateQuery.AddWhere(string.Format("{0}.Name = '{1}'", templateTableName, templateName));
				updateTemplateQuery.Execute();

				templateId = templateData.Tables[0].Rows[0].Field<int>("Id");
				var deleteAllTemplateDetailsQuery = api.Database.NewDeleteQuery(templateDetailsTableName);
				deleteAllTemplateDetailsQuery.AddWhere(string.Format("{0}.TemplateId = {1}", templateDetailsTableName, templateId));
				deleteAllTemplateDetailsQuery.Execute();
			}
			
			foreach (var task in tasks.Distinct())
			{
				var taskInsertQuery = api.Database.NewInsertQuery(templateDetailsTableName);
				taskInsertQuery.InsertInt("TemplateId", templateId);
				taskInsertQuery.InsertString("Project", task.Project.Name);
				taskInsertQuery.InsertString("Task", task.Name);
				taskInsertQuery.Execute();
			}
		}

		public List<Template> GetTemplates(CPluginApi api)
		{
			var templatesTableName = Tables.GetPluginTableName(api, Tables.TEMPLATE_TABLE);
			var templatesQuery = api.Database.NewSelectQuery(templatesTableName);
			templatesQuery.Distinct = true;
			var templatesData = templatesQuery.GetDataSet();

			return new List<Template>(templatesData.Tables[0].AsEnumerable().Select(r => new Template { Name = r.Field<string>("Name") }));
		}
	}
}