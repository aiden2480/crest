using Crest.Integration;
using Crest.Utilities;
using System.Text.RegularExpressions;

namespace Crest.Extensions.TerrainApprovals;

class ScoutEventCrawlerTaskConfigFactory : ITaskConfigFactory
{
	readonly Regex JandiWebhookRegex = new(@"^https:\/\/wh\.jandi\.com\/connect-api\/webhook\/\d+\/\w+$");

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

			if (!JandiWebhookRegex.IsMatch(taskConfig.JandiUrl))
			{
				Logger.Warn($"Jandi URL is in an unexpected format: '{taskConfig.JandiUrl}'", taskConfig.TaskName);
				continue;
			}

			var isJandiWebhookUrlValid = JandiAPIClient.IsValidIncomingWebhookURL(taskConfig.JandiUrl);

			if (!isJandiWebhookUrlValid)
			{
				Logger.Warn("Jandi URL matches expected url format but is invalid - has URL been set correctly?", taskConfig.TaskName);
				continue;
			}

			yield return taskConfig;
		}
	}
}
