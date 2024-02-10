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

		void CancelFutureTriggersForThisTask()
		{
			// todo test this cancels properly
			Context.Scheduler.UnscheduleJob(Context.Trigger.Key);
		}

		#region State

		protected object GetState()
		{
			var allState = GetAllState();
			var key = Context.Trigger.Key.Group + "-" + Context.Trigger.Key.Name;

			return allState.ContainsKey(key) ? allState[key] : new object();
		}

		protected void SetState(object state)
		{
			var allState = GetAllState();
			var key = Context.Trigger.Key.Group + "-" + Context.Trigger.Key.Name;

			allState[key] = state;

			var serialised = JsonConvert.SerializeObject(allState);

			File.WriteAllText(StatePath, serialised);
		}

		private static Dictionary<string, object> GetAllState()
		{
			if (!File.Exists(StatePath))
			{
				return new Dictionary<string, object>();
			}

			var fileContents = File.ReadAllText(StatePath);

			return JsonConvert.DeserializeObject<Dictionary<string, object>>(fileContents);
		}

		static string StatePath
			=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "crest.programdata");

		#endregion
	}
}
