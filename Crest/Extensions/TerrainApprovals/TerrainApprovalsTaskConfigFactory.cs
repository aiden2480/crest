using Crest.Integration;
using Crest.Utilities;
using System.Text.RegularExpressions;

namespace Crest.Extensions.TerrainApprovals
{
	class TerrainApprovalsTaskConfigFactory : ITaskConfigFactory
	{
		readonly Regex UsernameRegex = new(@"^(?:act|nsw|nt|qld|sa|tas|vic|wa|aus)-\d+$");

		readonly Regex JandiWebhookRegex = new(@"^https:\/\/wh\.jandi\.com\/connect-api\/webhook\/\d+\/\w+$");

		public IEnumerable<ITaskConfig> GetValidConfigs(ApplicationConfiguration applicationConfig)
		{
			if (applicationConfig.TerrainApprovals is null) yield break;
			if (!applicationConfig.TerrainApprovals.Enabled) yield break;

			var extensionConfig = applicationConfig.TerrainApprovals;

			foreach (var taskConfig in extensionConfig.Tasks)
			{
				if (!IsValid(taskConfig))
				{
					continue;
				}

				var areTerrainCredentialsValid = new TerrainAPIClient().Login(taskConfig.Username, taskConfig.Password, out var loginFailureReason);

				if (!areTerrainCredentialsValid)
				{
					Logger.Warn($"Could not log into Terrain: " + loginFailureReason, taskConfig.TaskName);
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

		private bool IsValid(TerrainApprovalsTaskConfig taskConfig)
		{
			bool valid = true;

			if (new string[] { taskConfig.Username, taskConfig.Password, taskConfig.JandiUrl, taskConfig.CronSchedule }.Contains(null))
			{
				Logger.Warn($"One of username, password, jandi_url, cron_schedule was not supplied, please check configuration", taskConfig.TaskName);
				return false;
			}

			if (taskConfig.LookbackDays <= 0)
			{
				Logger.Warn("lookback_days must be supplied and greater than zero", taskConfig.TaskName);
				return false;
			}

			if (taskConfig.UnitId.Equals(Guid.Empty))
			{
				Logger.Warn("unit_id must be supplied. This can be retrieved from the Terrain website", taskConfig.TaskName);
				return false;
			}

			if (!UsernameRegex.IsMatch(taskConfig.Username))
			{
				Logger.Warn($"Username is in an unexpected format: '{taskConfig.Username}'", taskConfig.TaskName);
				valid = false;
			}

			if (!JandiWebhookRegex.IsMatch(taskConfig.JandiUrl))
			{
				Logger.Warn($"Jandi URL is in an unexpected format: '{taskConfig.JandiUrl}'", taskConfig.TaskName);
				valid = false;
			}

			return valid;
		}
	}
}
