using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;

namespace Crest;

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
			var job = BuildJobForTask(task);
			var trigger = BuildTriggerForTask(task);

			await scheduler.ScheduleJob(job, trigger);
			var nextFireTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(trigger.GetNextFireTimeUtc().Value.DateTime, TimeZoneInfo.Local);

			Console.WriteLine($"[{task.ExtensionName}] Scheduled job {task.TaskName} has next run {nextFireTimeLocal}");
		}

		await Task.Delay(-1);
		await scheduler.Shutdown();
	}

	private static IJobDetail BuildJobForTask(ITaskConfig taskConfig)
	{
		return JobBuilder.Create()
			.WithIdentity(taskConfig.TaskName, taskConfig.ExtensionName)
			.OfType(taskConfig.JobRunnerType)
			.UsingJobData("config", JsonConvert.SerializeObject(taskConfig))
			.Build();
	}

	private static ITrigger BuildTriggerForTask(ITaskConfig taskConfig)
	{
		return TriggerBuilder.Create()
			.WithIdentity(taskConfig.TaskName, taskConfig.ExtensionName)
			.WithCronSchedule(taskConfig.CronSchedule)
			.StartNow()
			.Build();
	}

	private static IEnumerable<ITaskConfigFactory> ConfigFactories => new List<ITaskConfigFactory>()
	{
		new TerrainApprovalsTaskConfigFactory(),
		new ScoutEventCrawlerTaskConfigFactory(),
	};
}
