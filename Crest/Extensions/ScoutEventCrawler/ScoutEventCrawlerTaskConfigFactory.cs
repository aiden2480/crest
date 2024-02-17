using Crest.Integration;

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
				Console.WriteLine("This doesn't have any subscribed regions, whoops");
				continue;
			}

			yield return taskConfig;
		}
	}

	private bool IsValid(ScoutEventCrawlerTaskConfig taskConfig)
	{
		return true;
	}
}
