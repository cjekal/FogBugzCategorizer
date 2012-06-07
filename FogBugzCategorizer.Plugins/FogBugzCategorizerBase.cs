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
}
