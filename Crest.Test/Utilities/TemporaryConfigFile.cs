namespace Crest.Test.Utilities;

class TemporaryConfigFile : IDisposable
{
	public readonly string Filename;

	public readonly string Contents;

	public TemporaryConfigFile(string filename, string contents)
	{
		Filename = filename;
		Contents = contents;

		Setup();
	}

	void Setup()
	{
		File.WriteAllText(Filename, Contents);
	}

	public void Dispose()
	{
		File.Delete(Filename);
	}
}
