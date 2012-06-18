using System.Collections.Generic;
using System.Data;
using FogCreek.FogBugz.Plugins.Api;

namespace FogBugz.Categorizer.Plugins.Business.DataProviders
{
	public class ProjectsProvider
	{
		public List<Project> GetAll(CPluginApi api)
		{
			var projectTaskLookupTableName = Tables.GetPluginTableName(api, Tables.PROJECT_TASK_LOOKUP);
			var projectsQuery = api.Database.NewSelectQuery(projectTaskLookupTableName);
			projectsQuery.AddSelect(string.Format("{0}.Project", projectTaskLookupTableName));
			projectsQuery.AddOrderBy(string.Format("{0}.Project ASC", projectTaskLookupTableName));
			projectsQuery.Distinct = true;
			var projectsData = projectsQuery.GetDataSet();

			return new List<Project>(projectsData.Tables[0].AsEnumerable().Select(r => new Project { Name = r.Field<string>("Project") }));
		}
	}
}
