using NUnit.Framework;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class BugzViewTests_NewCase
	{
		private Browser _browser;
		private FogBugzPage _page;
		private const string UserName = "Administrator";

		[TestFixtureSetUp]
		public virtual void BeforeEachFixture()
		{
			_browser = new Browser { AutoSignIn = true, UserName = UserName, Password = "yagni123" };
			_page = _browser.Page<FogBugzPage>();
		}

		[SetUp]
		public virtual void BeforeEachTest()
		{
			FogBugzDB.Restore();
			_browser.GoTo(Bugz.NewCase.Url);
		}

		[TearDown]
		public virtual void AfterEachTest()
		{
		}

		[TestFixtureTearDown]
		public virtual void AfterEachFixture()
		{
			_browser.Dispose();
			_browser = null;
		}

		[Test]
		public virtual void NewCaseDoesNotHaveAnySelectedTasks()
		{
			_page.Categorizer.Click();
			_page.Selected.WaitUntil(s => s.IsVisible());
			Assert.That(_page.Selected.Divs, Is.Empty, "a new page should definitely have nothing selected.");
		}
	}
}