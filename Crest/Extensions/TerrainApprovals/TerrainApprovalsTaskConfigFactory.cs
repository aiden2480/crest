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
					Console.WriteLine($"Error occurred for task '{taskConfig.TaskName}': " + loginFailureReason);
					continue;
				}

				var jandiClient = new JandiAPIClient();
				var isJandiWebhookUrlValid = jandiClient.IsValidIncomingWebhookURL(taskConfig.JandiUrl);

				if (!isJandiWebhookUrlValid)
				{
					Console.WriteLine("Jandi URL matches expected url format but is invalid - has URL been set correctly?");
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
				Console.WriteLine($"One of username, password, jandi_url, cron_schedule was not supplied for task {taskConfig.TaskName}, please check configuration");
				return false;
			}

			if (taskConfig.LookbackDays <= 0)
			{
				Console.WriteLine("lookback_days must be supplied and greater than zero");
				return false;
			}

			if (taskConfig.UnitId.Equals(Guid.Empty))
			{
				Console.WriteLine("unit_id must be supplied. This can be retrieved from the Terrain website");
				return false;
			}

			if (!UsernameRegex.IsMatch(taskConfig.Username))
			{
				Console.WriteLine($"Username is in an unexpected format: '{taskConfig.Username}'");
				valid = false;
			}

			if (!JandiWebhookRegex.IsMatch(taskConfig.JandiUrl))
			{
				Console.WriteLine($"Jandi URL is in an unexpected format: '{taskConfig.JandiUrl}'");
				valid = false;
			}

			return valid;
		}
	}
}
