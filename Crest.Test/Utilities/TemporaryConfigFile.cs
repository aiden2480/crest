namespace Crest.TestUtilities;

class TemporaryConfigFile : IDisposable
{
	public readonly string Filename;

	public TemporaryConfigFile(string filename, string contents)
	{
		Filename = filename;

		File.WriteAllText(filename, contents);
	}

	public void Dispose()
	{
		File.Delete(Filename);
	}
}
