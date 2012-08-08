using System.Collections.Generic;
using System.Data;
using System.Linq;
using FogCreek.FogBugz.Plugins.Api;

namespace FogBugz.Categorizer.Plugins.Business.DataProviders
{
	public class TemplatesProvider
	{
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
			templatesQuery.AddOrderBy(string.Format("{0}.Name ASC", templatesTableName));
			var templatesData = templatesQuery.GetDataSet();

			var templates = new List<Template>(templatesData.Tables[0].AsEnumerable().Select(r => new Template {Name = r.Field<string>("Name")}));
			templates.Insert(0, new Template{Name = string.Empty});
			return templates;
		}

		public List<Task> GetTemplateTasks(CPluginApi api, string template)
		{
			var templateTableName = Tables.GetPluginTableName(api, Tables.TEMPLATE_TABLE);
			var templateDetailsTableName = Tables.GetPluginTableName(api, Tables.TEMPLATE_DETAILS_TABLE);
			var templateTasksQuery = api.Database.NewSelectQuery(templateTableName);
			templateTasksQuery.AddInnerJoin(templateDetailsTableName, string.Format("{0}.Id = {1}.TemplateId", templateTableName, templateDetailsTableName));
			templateTasksQuery.AddSelect(string.Format("{0}.Project", templateDetailsTableName));
			templateTasksQuery.AddSelect(string.Format("{0}.Task", templateDetailsTableName));
			templateTasksQuery.AddWhere(string.Format("{0}.Name = '{1}'", templateTableName, template));
			templateTasksQuery.Distinct = true;
			var templateTasksData = templateTasksQuery.GetDataSet();

			return new List<Task>(templateTasksData.Tables[0].AsEnumerable().Select(r => new Task { Project = new Project { Name = r.Field<string>("Project") }, Name = r.Field<string>("Task") }));
		}
	}
}
