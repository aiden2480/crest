using Crest.Integration;
using Crest.Utilities;

namespace Crest.Extensions.TerrainApprovals;

class ScoutEventCrawlerTaskConfigFactory : ITaskConfigFactory
{
	public IEnumerable<ITaskConfig> GetValidConfigs(ApplicationConfiguration applicationConfig)
	{
		if (applicationConfig.ScoutEventCrawler is null) yield break;
		if (!applicationConfig.ScoutEventCrawler.Enabled) yield break;

		var extensionConfig = applicationConfig.ScoutEventCrawler;

		foreach (var taskConfig in extensionConfig.Tasks)
		{
			if (!taskConfig.SubscribedRegions.Any())
			{
				Logger.Warn("This task does not have any subscribed regions", taskConfig.TaskName);
				continue;
			}

			yield return taskConfig;
		}
	}
}
