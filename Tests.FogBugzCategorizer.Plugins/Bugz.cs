namespace Tests.FogBugzCategorizer.Plugins
{
	public class Bugz
	{
		public int Id { get; private set; }
		public string Url { get; private set; }

		private static Bugz _categorized;
		private static Bugz _newCase;

		private Bugz()
		{
		}
		
		public static string RawUrl{ get { return "http://localhost/fogbugz/default.asp?pg=pgPluginRaw&ixPlugin=25&Command=LoadAll"; } }

		public static Bugz Categorized
		{
			get
			{
				if (_categorized == null)
				{
					_categorized = new Bugz {Id = 1, Url = "http://localhost/fogbugz/default.asp?1"};
				}
				return _categorized;
			}
		}

		public static Bugz NewCase
		{
			get
			{
				if (_newCase == null)
				{
					_newCase = new Bugz {Id = 2, Url = "http://localhost/fogbugz/default.asp?2"};
				}
				return _newCase;
			}
		}
	}
}