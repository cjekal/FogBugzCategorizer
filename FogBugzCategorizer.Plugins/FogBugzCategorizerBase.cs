using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;

namespace FogBugzCategorizer.Plugins
{
    public abstract class FogBugzCategorizerBase : Plugin
    {
        protected const string PLUGIN_ID = "FogBugzCategorizer@arpc.com";

        protected FogBugzCategorizerBase(CPluginApi api) : base(api)
        {
        }
    }

	public class Tables
	{
		public const string SPLIT_FILTER_TABLE = "ARPC_SPLIT_FILTER";
		public const string HOURS_FILTER_TABLE = "ARPC_HOURS_FILTER";
		public const string HOURS_TABLE = "ARPC_HOURS";
		public const string SPLIT_TABLE = "ARPC_SPLIT";
	}

	public enum HoursFilterType
	{
		Yes,
		No,
		Unset
	}
}
