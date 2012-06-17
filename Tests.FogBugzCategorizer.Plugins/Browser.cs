using WatiN.Core;

namespace Tests.FogBugzCategorizer.Plugins
{
	public class Browser : IE
	{
		public bool AutoSignIn { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }

		public override void GoTo(string url)
		{
			if (Url == url)
			{
				Refresh();
				return;
			}

			base.GoTo(url);

			if (AutoSignIn && IsOnLoginPage)
			{
				TextField("sPerson").TypeText(UserName);
				TextField(Find.ByName("sPassword")).TypeText(Password);
				Button("Button_OK").Click();
				base.GoTo(url);
			}
		}

		public bool IsOnLoginPage { get { return Span("usertype").Exists && Span("usertype").Text == "(Not logged on)"; } }
	}
}