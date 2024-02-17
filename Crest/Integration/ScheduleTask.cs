using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;

namespace Crest.Integration;

public abstract class ScheduleTask<TConfig> : IJob where TConfig : ITaskConfig
{
	private IJobExecutionContext Context;

	/// <summary>
	/// Runs just before <see cref="Run(TConfig)"/> and returns a bool indicating whether any further instances of this task should run.
	/// For example, returns false if credentials have become invalid.
	/// </summary>
	/// <param name="config">Configuration object of type <see cref="TConfig"/> for this task</param>
	/// <returns></returns>
	public virtual bool ShouldRun(TConfig config) => true;

	/// <summary>
	/// The main run function for this task. Run only if <see cref="ShouldRun(TConfig)"/> returns true. <paramref name="config"/> has been pre-parsed
	/// </summary>
	/// <param name="config">Configuration object of type <see cref="TConfig"/> for this task</param>
	public abstract void Run(TConfig config);

	public Task Execute(IJobExecutionContext context)
	{
		Context = context;

		var configString = context.MergedJobDataMap.GetString("config");
		var config = JsonConvert.DeserializeObject<TConfig>(configString);

		if (!ShouldRun(config))
		{
			CancelFutureTriggersForThisTask();
			return Task.CompletedTask;
		}

		Run(config);
		return Task.CompletedTask;
	}

	void CancelFutureTriggersForThisTask()
	{
		// todo test this cancels properly
		Context.Scheduler.UnscheduleJob(Context.Trigger.Key);
	}

	#region State

	internal TState GetState<TState>(TState def = default)
	{
		var allState = GetAllState<JToken>();

		
		if (!allState.ContainsKey(StateKey))
		{
			return def;
		}

		try
		{
			return allState[StateKey].ToObject<TState>() ?? def;
		}
		catch (FormatException)
		{
			return def;
		}
	}

	internal void SetState<TState>(TState state)
	{
		var allState = GetAllState<object>();
		allState[StateKey] = state;

		var serialised = JsonConvert.SerializeObject(allState);

		File.WriteAllText(StatePath, serialised);
	}

	private Dictionary<string, TDeserialise> GetAllState<TDeserialise>()
	{
		if (!File.Exists(StatePath))
		{
			return new Dictionary<string, TDeserialise>();
		}

		var fileContents = File.ReadAllText(StatePath);

		return JsonConvert.DeserializeObject<Dictionary<string, TDeserialise>>(fileContents);
	}

	internal virtual string StatePath
		=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "crest.programdata");

	internal virtual string StateKey
		=> Context.Trigger.Key.Group + "-" + Context.Trigger.Key.Name;

	#endregion
}
