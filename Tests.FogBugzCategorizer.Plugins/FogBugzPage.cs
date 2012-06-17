using WatiN.Core;

namespace Tests.FogBugzCategorizer.Plugins
{
	public class FogBugzPage : Page
	{
		public Link Categorizer
		{
			get { return Document.Link("Categorizer"); }
		}

		public Div CategorizerContainer
		{
			get { return Document.Div("CategorizerDiv"); }
		}

		public Div Projects
		{
			get { return Document.Div("CategorizerProjects"); }
		}

		public Div Tasks
		{
			get { return Document.Div("CategorizerTasks"); }
		}

		public Div Selected
		{
			get { return Document.Div("SelectedCategories"); }
		}

		public Link Save
		{
			get { return Document.Link("CategorizerSave"); }
		}

		public Div Notifications
		{
			get { return Document.Div("CategorizerNotifications"); }
		}

		public bool AutoSignIn { get; set; }

		public bool IsOnLoginPage
		{
			get { return Document.Span("usertype").Exists && Document.Span("usertype").Text == "(Not logged on)"; }
		}
	}
}