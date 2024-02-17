using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;

namespace Crest
{
	public class Crest
	{
		readonly ApplicationConfiguration AppConfig;

		public Crest(ApplicationConfiguration appConfig)
		{
			AppConfig = appConfig;
		}

		public async Task RunForever()
		{
			var taskConfigs = new List<ITaskConfig>();

			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = await schedulerFactory.GetScheduler();

			await scheduler.Start();

			foreach (var factory in ConfigFactories)
			{
				taskConfigs.AddRange(factory.GetValidConfigs(AppConfig));
			}

			if (!taskConfigs.Any())
			{
				Console.WriteLine("No tasks scheduled, please check configuration");
				return;
			}

			foreach (var task in taskConfigs)
			{
				var job = JobBuilder.Create()
					.WithIdentity(task.TaskName, task.ExtensionName)
					.OfType(task.JobRunnerType)
					.UsingJobData("config", JsonConvert.SerializeObject(task))
					.Build();

				var trigger = TriggerBuilder.Create()
					.WithIdentity(task.TaskName, task.ExtensionName)
					.WithCronSchedule(task.CronSchedule)
					.StartNow()
					.Build();

				await scheduler.ScheduleJob(job, trigger);

				var nextFireTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(trigger.GetNextFireTimeUtc().Value.DateTime, TimeZoneInfo.Local);
				Console.WriteLine($"[{task.ExtensionName}] Scheduled job {task.TaskName} has next run {nextFireTimeLocal}");
			}

			await Task.Delay(-1);
			await scheduler.Shutdown();
		}

		static IEnumerable<ITaskConfigFactory> ConfigFactories => new List<ITaskConfigFactory>()
		{
			new TerrainApprovalsTaskConfigFactory(),
			new ScoutEventCrawlerTaskConfigFactory(),
		};
	}
}
