using NUnit.Framework;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class BugzViewTests_Unauthorized
	{
		private Browser _browser;
		private FogBugzPage _page;

		[TestFixtureSetUp]
		public virtual void BeforeEachFixture()
		{
			_browser = new Browser { AutoSignIn = true };
			_page = _browser.Page<FogBugzPage>();
		}

		[SetUp]
		public virtual void BeforeEachTest()
		{
			_browser.GoTo(Bugz.Categorized.Url);
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
		public virtual void Layout()
		{
			Assert.That(_page.Categorizer.Exists, Is.False, "A user without access to categorization is not allowed to categorize!");
			Assert.That(_page.Projects.Exists, Is.False, "A user without access to categorization is not allowed to categorize!");
			Assert.That(_page.CategorizerContainer.Exists, Is.False, "A user without access to categorization is not allowed to categorize!");
			Assert.That(_page.Tasks.Exists, Is.False, "A user without access to categorization is not allowed to categorize!");
			Assert.That(_page.Save.Exists, Is.False, "A user without access to categorization is not allowed to categorize!");
			Assert.That(_page.Selected.Exists, Is.False, "A user without access to categorization is not allowed to categorize!");
		}
	}
}