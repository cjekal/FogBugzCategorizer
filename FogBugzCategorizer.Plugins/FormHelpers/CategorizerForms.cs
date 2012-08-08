using FogCreek.FogBugz.UI;

namespace FogBugzCategorizer.Plugins.FormHelpers
{
	public static class CategorizerForms
	{
		public static string GetBugzViewHtml()
		{
			return @"
<a id=""Categorizer"" class=""categorizer"">Categorizer</a>
<div id=""CategorizerDiv"" class=""categorizerContainer"" style=""display: none;"">
	<div class=""templateContainer"">
		<div class=""templateControls"">
			<div id=""TemplateDropdownContainer"" style=""width: 300px""></div>
		</div>
		<div class=""templateLabels"">Template:</div>
		<div style=""clear: both;""></div>
	</div>
	<div class=""categorizerNotificationsContainer"">
		<div id=""CategorizerNotifications"" class=""categorizerNotifications"" style=""display: none;"">Please wait! System is busy!</div>
	</div>
	<div id=""CategorizerProjects"" class=""categorizerContent categorizerLeft""></div>
	<div id=""CategorizerTasks"" class=""categorizerContent categorizerRight""></div>
	<div id=""SelectedCategories"" class=""categorizerSelected categorizerBottom""></div>
	<div class=""categorizerSave"">
		<a id=""CategorizerSave"" class=""categorizer"">Save Selections</a>
	</div>
	<div class=""templateContainer"" style=""border: 1px"">
		<div class=""templateControls small-margins"">
			<input id=""newTemplateName"" name=""newTemplateName"" type=""text""></input>
			<a id=""TemplateSave"" class=""categorizer"">Save Template</a>
		</div>
		<div class=""templateLabels small-margins"">New Template Name:</div>
		<div style=""clear: both;""></div>
	</div>
</div>
";

		}

		public static string GetBugzViewJS(string url, int bugzId)
		{
			return string.Format(@"
var settings = {{
	url: '{0}',
	bugzId: {1}
}};
", url, bugzId);
		}
	}
}
