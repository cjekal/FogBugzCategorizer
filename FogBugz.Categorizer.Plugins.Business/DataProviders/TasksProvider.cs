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
	}
}