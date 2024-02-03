using Quartz;

namespace Crest.Integration
{
	public interface ITaskConfig
	{
		/// <summary>
		/// The name of the extension that owns this task config, e.g. TerrainApprovalsTask
		/// </summary>
		public string ExtensionName { get; }

		/// <summary>
		/// Returns the name of the specific task that is being run, e.g. Scouts Weekly Terrain Approvals
		/// </summary>
		public string TaskName { get; }

		/// <summary>
		/// The type associated with this configuration class. The type must implement <see cref="IJob"/> and will receive this config object as a parameter
		/// </summary>
		public Type JobRunnerType { get; }

		/// <summary>
		/// The cron schedule this task should be run with
		/// </summary>
		public string CronSchedule { get; }
	}
}
