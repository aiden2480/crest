using Crest.Extensions.TerrainApprovals;
using Crest.Utilities;

namespace Crest.Integration;

public class ApplicationConfiguration
{
	/// <summary>
	/// Configuration for the TerrainApprovalsTask extension
	/// </summary>
	public ExtensionConfiguration<TerrainApprovalsTaskConfig> TerrainApprovals;

	/// <summary>
	/// Configuration for the ScoutEventCrawler extension
	/// </summary>
	public ExtensionConfiguration<ScoutEventCrawlerTaskConfig> ScoutEventCrawler;
}

public class ExtensionConfiguration<T> where T : ITaskConfig
{
	/// <summary>
	/// A boolean indicating whether this extension should be enabled
	/// </summary>
	public bool Enabled = true;

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

	/// <summary>
	/// The ID of the unit to gather pending and finalised approvals from
	/// </summary>
	public Guid UnitId { get; set; }

	/// <summary>
	/// The number of days to look backwards when searching for approvals
	/// </summary>
	public int LookbackDays { get; set; }
}

public class ScoutEventCrawlerTaskConfig : ITaskConfig
{
	public string ExtensionName => "ScoutEventCrawlerTask";

	public string TaskName { get; set; }

	public Type JobRunnerType => typeof(ScoutEventCrawlerTask);

	public string CronSchedule { set; get; }

	/// <summary>
	/// The incoming webhook URL for the jandi topic to post to
	/// </summary>
	public string JandiUrl { get; set; }

	/// <summary>
	/// A list of regions to subscribe to
	/// </summary>
	public List<SubscribableRegion> SubscribedRegions = new();
}
