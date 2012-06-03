using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins;
using FogCreek.FogBugz.Plugins.Api;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI;
using FogCreek.FogBugz.UI.Dialog;

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
