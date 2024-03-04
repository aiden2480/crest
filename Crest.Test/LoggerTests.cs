using NUnit.Framework;

namespace Crest.Utilities;

public class LoggerTests
{
	[Test]
	public void TestLoggerCreatesFile_AndContentsFormatIsCorrect()
	{
		var fileSafeNow = DateTime.Now.ToString("dd-MMM-yy-HHmmss");
		var logFilePath = Path.GetFullPath($"crest-{fileSafeNow}.log");

		using (new SuppressStdout())
		using (Logger.CreateNewInstance())
		{
			Assert.That(File.Exists(logFilePath), Is.True);

			Logger.Debug("debug");
			Logger.Info("information");
			Logger.Warn("warning", "testCategory");
			Logger.Error("error", "testCategory");
		}

		Assert.Multiple(() =>
		{
			Assert.That(File.Exists(logFilePath), Is.True);

			var logContents = File.ReadAllText(logFilePath).Split(Environment.NewLine);
			Assert.That(logContents[0], Does.EndWith("[DEBUG] | debug"));
			Assert.That(logContents[1], Does.EndWith("[INFO]  | information"));
			Assert.That(logContents[2], Does.EndWith("[WARN]  | testCategory -> warning"));
			Assert.That(logContents[3], Does.EndWith("[ERROR] | testCategory -> error"));
		});
	}

	[Test]
	public void TestDisposingAndCreatingNewLoggerCreatesNewLogFile()
	{
		string initialLoggerFilePath;
		string finalLoggerFilePath;

		using (Logger.CreateNewInstance())
		{
			initialLoggerFilePath = Logger.Instance.FilePath;
		}

		Thread.Sleep(1000);

		using (Logger.CreateNewInstance())
		{
			finalLoggerFilePath = Logger.Instance.FilePath;
		}

		Assert.That(finalLoggerFilePath, Is.Not.EqualTo(initialLoggerFilePath));
	}
}

class SuppressStdout : IDisposable
{
	readonly TextWriter InitialConsoleOut;

	public SuppressStdout()
	{
		InitialConsoleOut = Console.Out;
		Console.SetOut(new StringWriter());
	}

	public void Dispose()
	{
		Console.Out.Dispose();
		Console.SetOut(InitialConsoleOut);
	}
}
