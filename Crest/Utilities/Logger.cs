namespace Crest.Utilities;

public sealed class Logger : IDisposable
{
	public readonly string FilePath;
	private readonly StreamWriter LogWriter;
	private static Logger InternalInstance;

	public static Logger Instance
		=> InternalInstance ??= new();

	private Logger()
	{
		var fileSafeNow = DateTime.Now.ToString("dd-MMM-yy-HHmmss");
		var logFilePath = Path.GetFullPath($"crest-{fileSafeNow}.log");

		FilePath = Path.GetFullPath(logFilePath);
		LogWriter = new StreamWriter(FilePath);
	}

	public void Dispose()
	{
		InternalInstance = null;
		LogWriter?.Dispose();
	}

	public static Logger CreateNewInstance()
	{
		InternalInstance?.Dispose();
		InternalInstance = new Logger();

		return InternalInstance;
	}

	public void Debug(string message)
		=> AddLog("DEBUG", message);

	public void Info(string message)
		=> AddLog("INFO", message);

	public void Warn(string message)
		=> AddLog("WARN", message);

	public void Error(string message)
		=> AddLog("ERROR", message);

	void AddLog(string severity, string message)
	{
		var prefix = $"{DateTime.Now} [{severity}]".PadRight(31);
		var formattedMessage = $"{prefix} {message}";

		lock (LogWriter)
		{
			Console.WriteLine(formattedMessage);
			LogWriter.WriteLine(formattedMessage);
		}
	}
}
