using WatiN.Core;

namespace Tests.FogBugzCategorizer.Plugins
{
	public static class WatiNExtensions
	{
		public static bool IsVisible(this Element element)
		{
			if (element.Style.Display == "none")
			{
				return false;
			}
			if (element.Parent != null)
			{
				return element.Parent.IsVisible();
			}
			return true;
		}
	}
}