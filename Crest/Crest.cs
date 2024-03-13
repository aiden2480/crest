using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Crest.Utilities;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using static Quartz.Logging.LogProvider;

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
		// Logging
		using var logger = Logger.CreateNewInstance();

		if (AppConfig.DebugMode)
		{
			SetCurrentLogProvider(new ConsoleLogProvider());
		}

		// Get config objects
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
			Logger.Error("No tasks scheduled, or all have been skipped due to invalid configuration");
			return;
		}

		// Schedule jobs in Quartz
		foreach (var config in taskConfigs)
		{
			var job = BuildJobForTask(config);
			var trigger = BuildTriggerForTask(config);

			await scheduler.ScheduleJob(job, trigger);
			var nextFireTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(trigger.GetNextFireTimeUtc().Value.DateTime, TimeZoneInfo.Local);

			Logger.Info($"Newly scheduled job has next run {nextFireTimeLocal}", config.TaskName);
		}

		// Run forever
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
