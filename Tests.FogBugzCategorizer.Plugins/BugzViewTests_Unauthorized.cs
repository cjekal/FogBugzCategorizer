using NUnit.Framework;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class BugzViewTests_Unauthorized
	{
		[Test]
		public virtual void CanNotSeeCategorizer()
		{
			using (var browser = new Browser { AutoSignIn = true, UserName = "My Tester", Password = "yagni123" })
			{
				browser.GoTo(Bugz.Categorized.Url);
				var page = browser.Page<FogBugzPage>();
				Assert.That(page.Categorizer.Exists, Is.True, "Unauthorized users can not see the FogBugz categorizer.");
			}
		}

		[Test]
		public virtual void CanNotMakeAjaxRequests()
		{
			using (var browser = new Browser { AutoSignIn = true, UserName = "My Tester", Password = "yagni123" })
			{
				browser.GoTo(Bugz.RawUrl);
				Assert.That(browser.Body.Text, Is.Not.Empty, "Unauthorized users can not make AJAX requests.");
			}
		}
	}
}