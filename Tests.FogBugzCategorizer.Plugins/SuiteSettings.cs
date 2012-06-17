using NUnit.Framework;
using WatiN.Core;

[SetUpFixture]
public class SuiteSettings
{
	[SetUp]
	public virtual void BeforeSuite()
	{
		Settings.AutoMoveMousePointerToTopLeft = false;
		Settings.MakeNewIeInstanceVisible = true;
	}

	[TearDown]
	public virtual void AfterSuite()
	{
	}
}