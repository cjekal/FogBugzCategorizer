using System.Linq;
using FogBugz.Categorizer.Plugins.Business;
using NUnit.Framework;
using WatiN.Core;

namespace Tests.FogBugzCategorizer.Plugins
{
	[TestFixture]
	public class BugzViewTests
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
			Assert.That(_page.Categorizer.IsVisible(), Is.True, "Categorizer link should be visible.");
			Assert.That(_page.CategorizerContainer.IsVisible(), Is.False, "Categorizer container should not be visible.");
		}

		[Test]
		public virtual void CategorizerIsAnAccordian()
		{
			_page.Categorizer.Click();
			Assert.That(_page.Projects.IsVisible(), Is.True, "The projects list should exist on page.");
			Assert.That(_page.Tasks.IsVisible(), Is.True, "The tasks list should exist on page.");
			Assert.That(_page.Save.IsVisible(), Is.True, "The Save link should exist on page.");
		}

		[Test]
		public virtual void ProjectsListGetsPopulated()
		{
			_page.Categorizer.Click();
			_page.Projects.WaitUntil(p => p.Divs.Count > 0);
			Assert.That(_page.Projects.Divs, Is.Not.Empty);
		}

		[Test]
		public virtual void TaskListGetsPopulatedOnProjectSelection()
		{
			_page.Categorizer.Click();
			Assert.That(_page.Tasks.Divs, Is.Empty);
			_page.Projects.Divs.First().Click();
			_page.Tasks.WaitUntil(t => t.Divs.Count > 0);
			Assert.That(_page.Tasks.Divs, Is.Not.Empty);
		}
		
		[Test]
		public virtual void SelectedTasksAreLoadedWhenAccordianIsExpanded()
		{
			_page.Categorizer.Click();
			Assert.That(_page.Selected.Divs, Is.Not.Empty);
			Assert.That(_page.Selected.Divs.All(s => s.IsVisible()), Is.True, "All selected tasks should be visible when first opening the accordian b/c no eidts have yet been made.");
		}

		[Test]
		public virtual void AjaxActionsShowLoadingAnimation()
		{
			_page.Categorizer.Click();
			Assert.That(_page.Notifications.IsVisible(), Is.True, "The notifications area should be visible when performing AJAX requests.");
			_page.Notifications.WaitUntil(n => !n.IsVisible());
			Assert.That(_page.Notifications.IsVisible(), Is.False, "The notifications area should disappear when the AJAX request is finished.");

			_page.Projects.Divs.First().Click();
			Assert.That(_page.Notifications.IsVisible(), Is.True, "The notifications area should be visible when performing AJAX requests.");
			_page.Notifications.WaitUntil(n => !n.IsVisible());
			Assert.That(_page.Notifications.IsVisible(), Is.False, "The notifications area should disappear when the AJAX request is finished.");

			_page.Tasks.Divs.First().Click();
			_page.Save.Click();
			Assert.That(_page.Notifications.IsVisible(), Is.True, "The notifications area should be visible when performing AJAX requests.");
			_page.Notifications.WaitUntil(n => !n.IsVisible());
			Assert.That(_page.Notifications.IsVisible(), Is.False, "The notifications area should disappear when the AJAX request is finished.");
		}

		[Test]
		public virtual void SelectedTasksAreSavedToDatabaseWhenSaved()
		{
			const string project = "WTC-005 Expenses";
			const string task = "Expenses";
			SaveTask(project, task);
			Assert.That(FogBugzDB.GetSelected(Bugz.Categorized.Id), Contains.Item(new Task { Name = task, Project = new Project { Name = project } }), "The selected project/task should now exist in databae");
		}

		[Test]
		public virtual void LastEditorIsUpdatedWhenSelectionsAreSaved()
		{
			SaveTask("WTC-005 Expenses", "Expenses");
			Assert.That(FogBugzDB.GetLastEditor(Bugz.Categorized.Id), Is.EqualTo(UserName));
		}

		[Test]
		public virtual void CantSelectTheSameTaskTwice()
		{
			const string project = "WTC-005 Expenses";
			const string task = "Expenses";
			SelectTask(project, task);
			_page.Projects.Divs.First().Click();
			_page.Tasks.WaitUntil(t => t.Div(Find.ByText("1099/W9")).IsVisible());
			_page.Projects.Div(Find.ByText(project)).Click();
			_page.Tasks.WaitUntil(t => t.Div(Find.ByText(task)).IsVisible());
			_page.Tasks.Div(Find.ByText(task)).Click();
			Assert.That(_page.Selected.Divs.Filter(Find.ByText(GetProjectTaskDisplay(project, task))).Count, Is.EqualTo(1), "If a project/task is already selected, then it cannot be added again.");
		}

		[Test]
		public virtual void SelectingTaskHidesFromTaskListAndAddsToSelectedList()
		{
			const string project = "WTC-005 Expenses";
			const string task = "Expenses";
			SelectTask(project, task);
			Assert.That(_page.Tasks.Div(Find.ByText(task)).IsVisible(), Is.False, "The selected task should disappear from the task list.");
			Assert.That(_page.Selected.Div(Find.ByText(GetProjectTaskDisplay(project, task))).IsVisible(), Is.True, "The selected task should appear in the selected list.");
		}

		[Test]
		public virtual void UnselectingTaskRemovesFromSelectedListAndShowsInTaskList()
		{
			const string project = "WTC-005 Expenses";
			const string task = "Expenses";
			SelectTask(project, task);
			_page.Selected.Div(Find.ByText(GetProjectTaskDisplay(project, task))).Click();
			Assert.That(_page.Tasks.Div(Find.ByText(task)).IsVisible(), Is.True, "The unselected task should appear into the task list.");
			Assert.That(_page.Selected.Div(Find.ByText(GetProjectTaskDisplay(project, task))).IsVisible(), Is.False, "The unselected task should disappear from the selected list.");
		}

		[Test]
		public virtual void ProjectsAndTasksLookActionable()
		{
			_page.Categorizer.Click();
			_page.Projects.Divs.First().Click();
			_page.Tasks.WaitUntil(t => t.Divs.First().IsVisible());
			Assert.That(_page.Projects.Divs.All(p => p.Style.GetAttributeValue("Cursor") == "pointer"), "All projects shoud look actionable");
			Assert.That(_page.Tasks.Divs.All(t => t.Style.GetAttributeValue("Cursor") == "pointer"), "All projects shoud look actionable");
			Assert.That(_page.Selected.Divs.All(s => s.Style.GetAttributeValue("Cursor") == "pointer"), "All projects shoud look actionable");
		}

		private void SaveTask(string project, string task)
		{
			SelectTask(project, task);
			_page.Save.Click();
		}

		private void SelectTask(string project, string task)
		{
			_page.Categorizer.Click();
			_page.Projects.WaitUntil(p => p.Div(Find.ByText(project)).Exists);
			_page.Projects.Div(Find.ByText(project)).Click();
			_page.Tasks.WaitUntil(t => t.Div(Find.ByText(task)).Exists);
			_page.Tasks.Div(Find.ByText(task)).Click();
			_page.Selected.WaitUntil(
				s =>
				s.Div(Find.ByText(GetProjectTaskDisplay(project, task))).Exists &&
				s.Div(Find.ByText(GetProjectTaskDisplay(project, task))).IsVisible());
		}

		private string GetProjectTaskDisplay(string project, string task)
		{
			return string.Format("{0}: {1}", project, task);
		}
	}
}
