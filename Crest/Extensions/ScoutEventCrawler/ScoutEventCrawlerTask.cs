using Crest.Integration;
using Crest.Utilities;

namespace Crest.Extensions.TerrainApprovals;

public class ScoutEventCrawlerTask : ScheduleTask<ScoutEventCrawlerTaskConfig>
{
	internal readonly ScoutEventAPIClient ScoutEventClient;

	internal ScoutEventCrawlerTask(ScoutEventAPIClient scoutEventClient)
	{
		ScoutEventClient = scoutEventClient;
	}

	public ScoutEventCrawlerTask() : this(new ScoutEventAPIClient()) { }

	public override void Run(ScoutEventCrawlerTaskConfig config)
	{
		var regions = config.SubscribedRegions
			.Select(r => ScoutEventClient.ScanRegion(r))
			.ToList();
		var seenIDs = GetState(def: new List<int>());

		JandiAPIClient.SendMessage(config.JandiUrl, GetJandiMessage(regions, seenIDs));
		
		SetState(seenIDs);
	}

	internal static JandiMessage GetJandiMessage(List<Region> regions, List<int> seenIDs)
	{
		var message = new JandiMessage
		{
			Body = "New ScoutLink events",
			ConnectColor = "#84BC48",
		};

		foreach (var region in regions)
		{
			var regionDescription = new List<string>();

			foreach (var item in region.Events)
			{
				var alreadySeenThisEvent = seenIDs.Contains(item.Id);

				if (item.IsClosed || alreadySeenThisEvent)
				{
					continue;
				}

				seenIDs.Add(item.Id);
				regionDescription.Add($"[{item.Name}]({item.Link}) • {item.RegistrationStatus} {item.Emoji}\n{item.Info}");
			}

			if (regionDescription.Any())
			{
				message.ConnectInfo.Add(new JandiConnect
				{
					Title = $"⚜️ [{region.Name}]({region.Link})",
					Description = string.Join("\n\n", regionDescription)
				});
			}
		}

		if (!message.ConnectInfo.Any())
		{
			message.ConnectInfo.Add(new JandiConnect
			{
				Description = "No new events posted for any subscribed regions"
			});
		}

		return message;
	}
}
