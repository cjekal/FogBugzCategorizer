using System.Collections.Generic;

namespace FogBugz.Categorizer.Plugins.Business
{
	public interface IProjectProvider
	{
		List<Project> GetAll(string projectTaskLookupTableName);
	}

	public class ProjectProvider : IProjectProvider
	{
		public List<Project> GetAll(string projectTaskLookupTableName)
		{
			return new List<Project>();
		}
	}
}
