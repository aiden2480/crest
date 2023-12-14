using Crest.Implementation;
using Crest.Integration;

namespace Crest.Extensions.TerrainApprovals
{
	class TerrainApprovalsTaskFactory : IScheduleTaskFactory
	{
		public IEnumerable<IScheduleTask> GetScheduleTasks(ApplicationConfiguration applicationConfig)
		{
			if (applicationConfig.TerrainApprovals is null) yield break;
			if (!applicationConfig.TerrainApprovals.Enabled) yield break;

			var extensionConfig = applicationConfig.TerrainApprovals;

			foreach (var (friendlyName, taskConfig) in extensionConfig.Tasks)
			{
				if (taskConfig.Username is null || taskConfig.Password is null || taskConfig.JandiUrl is null)
				{
					Console.WriteLine($"One of username, password, jandi_url was not supplied for task {friendlyName}, please check configuration");
					continue;
				}

				yield return new TerrainApprovalsTask(friendlyName, taskConfig);
			}
		}
	}
}
