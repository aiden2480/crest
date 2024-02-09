using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Crest
{
	public class Program
	{
		static async Task Main()
		{
			//LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

			var appConfig = GetValidAppConfiguration();
			var taskConfigs = new List<ITaskConfig>();

			var schedulerFactory = new StdSchedulerFactory();
			var scheduler = await schedulerFactory.GetScheduler();

			await scheduler.Start();

			foreach (var factory in ConfigFactories)
			{
				taskConfigs.AddRange(factory.GetValidConfigs(appConfig));
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
		};

		public static ApplicationConfiguration GetValidAppConfiguration()
		{
			var configPath = "config.yaml";

			if (!File.Exists(configPath))
			{
				throw new FileNotFoundException("You need to create a configuration file", configPath);
			}

			// Read and attempt to deserialise, throw an error if we can't
			var yaml = File.ReadAllText(configPath);
			var deserialiser = new DeserializerBuilder()
				.WithNamingConvention(UnderscoredNamingConvention.Instance)
				.Build();

			try
			{
				return deserialiser.Deserialize<ApplicationConfiguration>(yaml);
			}
			catch (YamlException e) when (e.Message.Contains("not found on type"))
			{
				var firstQuote = e.Message.IndexOf("'");
				var secondQuote = e.Message.IndexOf("'", firstQuote + 1);
				var unrecognisedArg = e.Message[firstQuote..(secondQuote + 1)];

				throw new ArgumentException($"Unrecognised argument supplied: {unrecognisedArg} - please remove\n\n", e);
			}
			catch (YamlException e) when (e.InnerException != null && e.InnerException.Message.Contains("is not a valid YAML Boolean"))
			{
				var firstQuote = e.InnerException.Message.IndexOf('"');
				var secondQuote = e.InnerException.Message.IndexOf('"', firstQuote + 1);
				var invalidArg = e.InnerException.Message[firstQuote..(secondQuote + 1)];

				throw new ArgumentException($"Invalid boolean value {invalidArg} supplied - replace with yes/no\n\n", e.InnerException);
			}
		}
	}
}
