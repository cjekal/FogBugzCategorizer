using FogBugzCategorizer.Plugins.FormHelpers;
using NUnit.Framework;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class CategorizerFormsTests
	{
		[Test]
		public virtual void GetBugzViewHtmlGeneratesAllControlsThatClientSideUses()
		{
			var html = CategorizerForms.GetBugzViewHtml();
			Assert.That(html, Is.StringContaining(@"<a id=""Categorizer"""), "The categorizer link to open/close accordian must exist.");
			Assert.That(html, Is.StringContaining(@"<div id=""CategorizerDiv"""), "The categorizer container that is the accordian must exist.");
			Assert.That(html, Is.StringContaining(@"<div id=""CategorizerNotifications"""), "The categorizer notifications area must exist.");
			Assert.That(html, Is.StringContaining(@"<div id=""CategorizerProjects"""), "The projects list must exist.");
			Assert.That(html, Is.StringContaining(@"<div id=""CategorizerTasks"""), "The tasks list must exist.");
			Assert.That(html, Is.StringContaining(@"<div id=""SelectedCategories"""), "The list of selected tasks must exist.");
			Assert.That(html, Is.StringContaining(@"<a id=""CategorizerSave"""), "The save button must exist.");
		}

		[Test]
		public virtual void GetBugzViewJSGeneratesAllClientSideSettings()
		{
			var url = "http://soCoolUrl.com/fogbugz/default.asp?pgPlugin=1";
			var bugzId = 123;
			string js = CategorizerForms.GetBugzViewJS(url, bugzId);
			Assert.That(js, Contains.Substring(string.Format("url: '{0}'", url)), "The clientside settings should contain the raw plugin page url.");
			Assert.That(js, Contains.Substring("bugzId: " + bugzId), "The clientside settings should contain the FogBugz ID.");
		}
	}
}
