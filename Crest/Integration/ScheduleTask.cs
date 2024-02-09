using Newtonsoft.Json;
using Quartz;

namespace Crest.Integration
{
	public abstract class ScheduleTask<T> : IJob where T : ITaskConfig
	{
		private IJobExecutionContext Context;

		/// <summary>
		/// Runs just before <see cref="Run(T)"/> and returns a bool indicating whether any further instances of this task should run.
		/// For example, returns false if credentials have become invalid.
		/// </summary>
		/// <param name="config">Configuration object of type <see cref="T"/> for this task</param>
		/// <returns></returns>
		public virtual bool ShouldRun(T config) => true;

		/// <summary>
		/// The main run function for this task. Run only if <see cref="ShouldRun(T)"/> returns true. <paramref name="config"/> has been pre-parsed
		/// </summary>
		/// <param name="config">Configuration object of type <see cref="T"/> for this task</param>
		public abstract void Run(T config);

		public Task Execute(IJobExecutionContext context)
		{
			Context = context;

			var configString = context.MergedJobDataMap.GetString("config");
			var config = JsonConvert.DeserializeObject<T>(configString);

			if (!ShouldRun(config))
			{
				CancelFutureTriggersForThisTask();
				return Task.CompletedTask;
			}

			Run(config);
			return Task.CompletedTask;
		}

		public void CancelFutureTriggersForThisTask()
		{
			Context.Scheduler.PauseTrigger(Context.Trigger.Key);
		}
	}
}
