using Crest.Extensions.TerrainApprovals;
using Crest.Implementation;
using Crest.Integration;
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
			var config = GetValidConfiguration();
			Console.WriteLine(config.TerrainApprovals!.Tasks);
			var tasks = new List<IScheduleTask>();

			var schedulerFactory = new StdSchedulerFactory();
			var schedule = await schedulerFactory.GetScheduler();

			await schedule.Start();

			foreach (var factory in Factories)
			{
				tasks.AddRange(factory.GetScheduleTasks(config));
			}

			if (!tasks.Any())
			{
				Console.WriteLine("No tasks scheduled, please check configuration");
				return;
			}

			foreach (var task in tasks)
			{
				task.OneTimeSetup();

				await schedule.ScheduleJob(task.Job, task.Trigger);
			}

			Console.CancelKeyPress += async delegate
			{
				await schedule.Shutdown();
				Environment.Exit(0);
			};

			await Task.Delay(-1);
		}

		static IEnumerable<IScheduleTaskFactory> Factories => new List<IScheduleTaskFactory>()
		{
			new TerrainApprovalsTaskFactory(),
		};

		static ApplicationConfiguration GetValidConfiguration()
		{
			ApplicationConfiguration config;
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
				config = deserialiser.Deserialize<ApplicationConfiguration>(yaml);
			}
			catch (YamlException e) when (e.Message.Contains("not found on type"))
			{
				var firstQuote = e.Message.IndexOf("'");
				var secondQuote = e.Message.IndexOf("'", firstQuote + 1);
				var unrecognisedArg = e.Message[firstQuote..(secondQuote + 1)];

				throw new ArgumentException($"Unrecognised argument supplied: {unrecognisedArg} - please remove\n\n", e);
			}

			return config;
		}
	}
}
