using FogCreek.FogBugz.Plugins.Api;

namespace FogBugz.Categorizer.Plugins.Business
{
	public class Tables
	{
		public const string SPLIT_FILTER_TABLE = "ARPC_SPLIT_FILTER";
		public const string HOURS_FILTER_TABLE = "ARPC_HOURS_FILTER";
		public const string HOURS_TABLE = "ARPC_HOURS";
		public const string SPLIT_TABLE = "ARPC_SPLIT";
		public const string SPLIT_DETAILS_TABLE = "ARPC_SPLIT_DETAILS";
		public const string PROJECT_TASK_LOOKUP = "ARPC_PROJECT_TASK_LOOKUP";

		public static string GetPluginTableName(CPluginApi api, string tableName)
		{
			return api.Database.PluginTableName(Statics.PluginId, tableName);
		}
	}
}