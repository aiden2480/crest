namespace Crest.Utilities;

public sealed class Logger : IDisposable
{
	private readonly StreamWriter LogWriter;
	internal readonly string FilePath;
	internal static Logger Instance;

	private Logger()
	{
		var fileSafeNow = DateTime.Now.ToString("dd-MMM-yy-HHmmss");
		var logFilePath = Path.GetFullPath($"crest-{fileSafeNow}.log");

		FilePath = Path.GetFullPath(logFilePath);
		LogWriter = new StreamWriter(FilePath);
	}

	public void Dispose()
	{
		Instance = null;
		LogWriter?.Dispose();
	}

	public static Logger CreateNewInstance()
	{
		Instance?.Dispose();
		Instance = new Logger();

		return Instance;
	}

	public static void Debug(string message, string category = null)
		=> AddLog("DEBUG", message, category);

	public static void Info(string message, string category = null)
		=> AddLog("INFO", message, category);

	public static void Warn(string message, string category = null)
		=> AddLog("WARN", message, category);

	public static void Error(string message, string category = null)
		=> AddLog("ERROR", message, category);

	static void AddLog(string severity, string message, string category)
	{
		if (Instance == null)
		{
			CreateNewInstance();
			Debug("New instance was just created, this should not have happened", "logger");
		}

		var prefix = $"{DateTime.Now} [{severity}]".PadRight(31);
		var formattedMessage = string.IsNullOrEmpty(category)
			? $"{prefix} | {message}"
			: $"{prefix} | {category} -> {message}";

		lock (Instance.LogWriter)
		{
			Console.WriteLine(formattedMessage);
			Instance.LogWriter.WriteLine(formattedMessage);
		}
	}
}
