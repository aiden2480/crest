using Quartz.Logging;
using Quartz.Util;

namespace Crest.Utilities;

public class ConsoleLogProvider : ILogProvider
{
	public Quartz.Logging.Logger GetLogger(string name) => (level, func, exception, parameters) =>
	{
		if (func is null)
		{
			return true;
		}

		var severity = level.ToString().ToUpper();
		var message = func.Invoke().FormatInvariant(parameters);

		Logger.AddLog(severity, message, "Quartz");

		return true;
	};

	public IDisposable OpenNestedContext(string message)
		=> throw new NotImplementedException();

	public IDisposable OpenMappedContext(string key, object value, bool destructure)
		=> throw new NotImplementedException();
}
