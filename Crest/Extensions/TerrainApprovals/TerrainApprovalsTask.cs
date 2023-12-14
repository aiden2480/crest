using Crest.Implementation;
using Quartz;

namespace Crest.Extensions.TerrainApprovals
{
	internal class TerrainApprovalsTask : ScheduleTaskBase
	{
		readonly string FriendlyName;

		readonly TerrainApprovalsTaskConfig Config;

		public TerrainApprovalsTask(string friendlyName, TerrainApprovalsTaskConfig config)
		{
			FriendlyName = friendlyName;
			Config = config;
		}

		public override string Name
			=> GetType().Name + "-" + FriendlyName;

		public override void OneTimeSetup()
		{
			Console.WriteLine("One time setup");
		}

		public override Task Execute(IJobExecutionContext context)
		{
			Console.WriteLine($"Execute - {FriendlyName}");

			return Task.CompletedTask;
		}
	}
}
