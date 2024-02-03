using Crest.Integration;
using Newtonsoft.Json;
using Quartz;

namespace Crest.Extensions.TerrainApprovals
{
	public class TerrainApprovalsTask : IJob
	{
		public Task Execute(IJobExecutionContext context)
		{
			var configString = context.MergedJobDataMap.GetString("config");
			var config = JsonConvert.DeserializeObject<TerrainApprovalsTaskConfig>(configString);

			Console.WriteLine(config.Username);

			return Task.CompletedTask;
		}
	}
}
