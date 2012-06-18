using System.Collections.Generic;
using System.Linq;
using FogBugz.Categorizer.Plugins.Business;
using NUnit.Framework;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class BugzViewTests_EdgeCases
	{
		private Browser _browser;
		private FogBugzPage _page;

		[TestFixtureSetUp]
		public virtual void BeforeEachFixture()
		{
			_browser = new Browser { AutoSignIn = true, UserName = "Administrator", Password = "yagni123" };
			_page = _browser.Page<FogBugzPage>();
		}

		[SetUp]
		public virtual void BeforeEachTest()
		{
			FogBugzDB.Restore();
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
		public virtual void UniqueTasksAreSavedWhenDuplicateTasksArePosted()
		{
			_page.Categorizer.Click();
			_page.Selected.WaitUntil(s => s.Divs.Count > 0);
			var selectedTask = _page.Selected.Divs.Last();
			_browser.RunScript(string.Format("var copiedSelected = $('<div />').html('{0}').appendTo('#{1}');", selectedTask.InnerHtml, _page.Selected.Id));
			_browser.RunScript(string.Format("copiedSelected.data('task', $('#{0}').find('div:contains(\"{1}\")').data('task'));", _page.Selected.Id, selectedTask.Text));
			_page.Save.Click();
			WaitForNotificationsToToggle();

			var allSelectedTasks = FogBugzDB.GetSelected(Bugz.Categorized.Id);
			var duplicateSelectedTasks = allSelectedTasks.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
			Assert.That(duplicateSelectedTasks, Is.Empty, "Any duplicate tasks should only be saved once.\r\n{0}", allSelectedTasks);
		}

		private void WaitForNotificationsToToggle()
		{
			_page.Notifications.WaitUntil(n => n.IsVisible());
			_page.Notifications.WaitUntil(n => !n.IsVisible());
		}
	}
}