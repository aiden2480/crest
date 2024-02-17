using NUnit.Framework;

namespace Crest.Test.Utilities;

public abstract class DeleteProgramDataBeforeTest
{
	protected static readonly string ProgramDataLocation = "mockcrest.programdata";

	[SetUp]
	public void SetUp()
	{
		if (File.Exists(ProgramDataLocation))
		{
			File.Delete(ProgramDataLocation);
		}
	}
}
