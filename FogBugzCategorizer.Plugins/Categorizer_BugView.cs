using System.IO;
using System.Xml;
using FogCreek.FogBugz;
using FogCreek.FogBugz.Plugins.Entity;
using FogCreek.FogBugz.Plugins.Interfaces;
using FogCreek.FogBugz.UI.Dialog;

namespace FogBugzCategorizer.Plugins
{
	public partial class CategorizerGridView : IPluginBugDisplay, IPluginJS, IPluginRawPageDisplay
	{
		#region Implementation of IPluginBugDisplay

		public CBugDisplayDialogItem[] BugDisplayViewLeft(CBug[] rgbug, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayViewTop(CBug[] rgbug, bool fPublic)
		{
			var displayItem = new CBugDisplayDialogItem("BugDisplayViewTop")
            {
				iColumnSpan = 4,
                sContent = @"<a id=""Categorizer"" href="""">Categorizer</a><div id=""CategorizerDiv""></div>"
			};

			return new[] { displayItem };
		}

		public CBugDisplayDialogItem[] BugDisplayEditLeft(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		public CBugDisplayDialogItem[] BugDisplayEditTop(CBug[] rgbug, BugEditMode nMode, bool fPublic)
		{
			return null;
		}

		#endregion

		#region Implementation of IPluginJS

		public CJSInfo JSInfo()
		{
			var jsInfo = new CJSInfo { rgsStaticFiles = new[] { Statics.JQuery, Statics.Categorizer }, sInlineJS = GetCategorizerScript() };
			return jsInfo;
		}

		private string GetCategorizerScript()
		{
			return string.Format(@"
var settings = {{
	url: '{0}'
}};
", api.Url.PluginRawPageUrl(PLUGIN_ID));
		}

		#endregion

		#region Implementation of IPluginRawPageDisplay

		public string RawPageDisplay()
		{
			var xml = new XmlDocument();
			var declaration = xml.CreateNode(XmlNodeType.XmlDeclaration, null, null);
			xml.AppendChild(declaration);

			var root = xml.CreateElement("projects");
			xml.AppendChild(root);

			CreateProjectElement(root, "project 1");
			CreateProjectElement(root, "project 2");
			CreateProjectElement(root, "project 3");

			StringWriter writer = new StringWriter();
			xml.WriteTo(new XmlTextWriter(writer));
			return writer.ToString();
		}

		public PermissionLevel RawPageVisibility()
		{
			return PermissionLevel.Normal;
		}

		#endregion

		private XmlElement CreateProjectElement(XmlElement projects, string projectName)
		{
			var project = projects.OwnerDocument.CreateElement("project");
			project.SetAttribute("name", projectName);
			projects.AppendChild(project);
			return project;
		}
	}
}