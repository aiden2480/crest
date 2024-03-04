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
		using (var logger = Logger.Instance)
		{
			Assert.That(File.Exists(logFilePath), Is.True);

			logger.Debug("debug");
			logger.Info("information");
			logger.Warn("warning");
			logger.Error("error");
		}

		Assert.Multiple(() =>
		{
			Assert.That(File.Exists(logFilePath), Is.True);

			var logContents = File.ReadAllText(logFilePath).Split(Environment.NewLine);
			Assert.That(logContents[0], Does.EndWith("[DEBUG] debug"));
			Assert.That(logContents[1], Does.EndWith("[INFO]  information"));
			Assert.That(logContents[2], Does.EndWith("[WARN]  warning"));
			Assert.That(logContents[3], Does.EndWith("[ERROR] error"));
		});
	}

	[Test]
	public void TestDisposingAndCreatingNewLoggerCreatesNewLogFile()
	{
		var initialLoggerFilePath = Logger.Instance.FilePath;
		
		Logger.Instance.Dispose();
		Thread.Sleep(1000);

		Assert.That(Logger.Instance.FilePath, Is.Not.EqualTo(initialLoggerFilePath));
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
