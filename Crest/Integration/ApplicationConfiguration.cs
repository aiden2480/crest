using Crest.Extensions.TerrainApprovals;

namespace Crest.Integration
{
	public class ApplicationConfiguration
	{
		/// <summary>
		/// Configuration for the TerrainApprovalsTask extension
		/// </summary>
		public ExtensionConfiguration<TerrainApprovalsTaskConfig> TerrainApprovals;
	}

	public class ExtensionConfiguration<T> where T : ITaskConfig
	{
		/// <summary>
		/// A boolean indicating whether this extension should be enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// A list of task-specific configurations for this extension, such as Terrain username/passwords
		/// </summary>
		public List<T> Tasks = new();
	}

	public class TerrainApprovalsTaskConfig : ITaskConfig
	{
		public string ExtensionName => "TerrainApprovalsTask";

		public string TaskName { get; set; }

		public Type JobRunnerType => typeof(TerrainApprovalsTask);

		public string CronSchedule { set; get; }

		/// <summary>
		/// Username for Scouts Terrain. Should be in the format branch-memberID. For example nsw-132323
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Password for Scouts Terrain
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// The incoming webhook URL for the jandi topic to post to
		/// </summary>
		public string JandiUrl { get; set; }
	}
}
