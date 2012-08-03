namespace FogBugzCategorizer.Plugins.FormHelpers
{
	public static class CategorizerForms
	{
		public static string GetBugzViewHtml()
		{
			return @"
<a id=""Categorizer"" class=""categorizer"">Categorizer</a>
<div id=""CategorizerDiv"" class=""categorizerContainer"" style=""display: none;"">
	<label for=""selectedTemplate"">Choose Template:</label>
	<select id=""selectedTemplate"" name=""selectedTemplate"">
		<option selected=""selected""></option>
	</select>
	<div class=""categorizerNotificationsContainer"">
		<div id=""CategorizerNotifications"" class=""categorizerNotifications"" style=""display: none;"">Please wait! System is busy!</div>
	</div>
	<div id=""CategorizerProjects"" class=""categorizerContent categorizerLeft""></div>
	<div id=""CategorizerTasks"" class=""categorizerContent categorizerRight""></div>
	<div id=""SelectedCategories"" class=""categorizerSelected categorizerBottom""></div>
	<div class=""categorizerSave"">
		<a id=""CategorizerSave"" class=""categorizer"">Save Selections</a>
	</div>
	<div>New Template Name:</div>
	<input id=""newTemplateName"" name=""newTemplateName"" type=""text""></input>
	<div class=""categorizerSave"">
		<a id=""TemplateSave"" class=""categorizer"">Save Template</a>
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
