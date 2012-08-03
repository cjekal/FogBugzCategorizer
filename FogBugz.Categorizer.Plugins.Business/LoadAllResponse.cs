using System.Collections.Generic;

namespace FogBugz.Categorizer.Plugins.Business
{
	public class LoadAllResponse
	{
		public List<Project> Projects { get; set; }
		public List<Task> Selected { get; set; }
		public List<Template> Templates { get; set; } 
	}
}