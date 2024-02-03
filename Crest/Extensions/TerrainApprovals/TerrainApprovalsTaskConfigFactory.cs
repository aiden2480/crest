using Crest.Integration;
using Crest.Utilities;
using Newtonsoft.Json;
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

				var response = TerrainAPIClient.Login(taskConfig.Username, taskConfig.Password);

				if (!response.IsSuccessStatusCode)
				{
					var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result);
					Console.WriteLine($"Error occurred for task '{taskConfig.TaskName}': " + json["message"]);
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
