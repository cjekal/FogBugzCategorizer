using NUnit.Framework;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class BugzViewTests_Authorized
	{
		[Test]
		public virtual void AdministratorCanSeeCategorizer()
		{
			using (var browser = new Browser { AutoSignIn = true, UserName = "Administrator", Password = "yagni123" })
			{
				browser.GoTo(Bugz.Categorized.Url);
				var page = browser.Page<FogBugzPage>();
				Assert.That(page.Categorizer.Exists, Is.True, "Authorized users can see the FogBugz categorizer.");
			}
		}

		[Test]
		public virtual void MarkKingCanSeeCategorizer()
		{
			using (var browser = new Browser { AutoSignIn = true, UserName = "Mark King", Password = "yagni123" })
			{
				browser.GoTo(Bugz.Categorized.Url);
				var page = browser.Page<FogBugzPage>();
				Assert.That(page.Categorizer.Exists, Is.True, "Authorized users can see the FogBugz categorizer.");
			}
		}

		[Test]
		public virtual void AdministratorCanMakeAjaxRequests()
		{
			using (var browser = new Browser { AutoSignIn = true, UserName = "Administrator", Password = "yagni123" })
			{
				browser.GoTo(Bugz.RawUrl);
				Assert.That(browser.Body.Text, Is.Not.Empty, "Authorized users can make AJAX requests.");
			}
		}

		[Test]
		public virtual void MarkKingCanMakeAjaxRequests()
		{
			using (var browser = new Browser { AutoSignIn = true, UserName = "Mark King", Password = "yagni123" })
			{
				browser.GoTo(Bugz.RawUrl);
				Assert.That(browser.Body.Text, Is.Not.Empty, "Authorized users can make AJAX requests.");
			}
		}
	}
}