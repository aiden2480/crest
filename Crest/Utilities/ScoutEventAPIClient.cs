using HtmlAgilityPack;
using Quartz.Util;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Crest.Test")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Crest.Utilities
{
	public class ScoutEventAPIClient
	{
		internal static readonly string BASE_URL = "https://events.nsw.scouts.com.au";

		public Region ScanRegion(SubscribableRegion region)
		{
			var regionUrl = GetRegionUrl(region);
			var doc = GetHtmlDocument(regionUrl);
			var events = new List<Event>();

			var regionName = doc.DocumentNode.SelectSingleNode("//h1").InnerText.Trim();
			var eventTables = doc.DocumentNode.SelectNodes("//a[starts-with(@href, '/event/') and .//table[contains(@class, 'events')]]");

			foreach (var eventTable in eventTables)
			{
				var aTag = eventTable.SelectSingleNode(".//a");

				var name = aTag.InnerText;
				var id = int.Parse(aTag.GetAttributeValue("href", null).Split("/").Last());

				var infoTexts = eventTable.SelectSingleNode(".//div[@class='text-gray-600 mb-2']").ChildNodes
					.Select(n => n.InnerText.Trim())
					.Where(n => !n.IsNullOrWhiteSpace());

				var info = string.Join("\n", infoTexts);

				var registrationNode = eventTable.SelectSingleNode(".//span[contains(@class, 'badge')]");
				string registrationStatus, registrationSeverity;

				// Some events don't have registration status https://events.nsw.scouts.com.au/event/1271
				if (registrationNode != null)
				{
					registrationStatus = registrationNode.InnerText;
					registrationSeverity = registrationNode
						.GetClasses()
						.First(c => c.StartsWith("bg-"))
						.Split("-")
						.ElementAt(1);
				}
				else
				{
					registrationStatus = "Unknown";
					registrationSeverity = "unknown";
				}

				events.Add(new Event(name, id, info, registrationStatus, registrationSeverity));
			}

			return new Region(regionName, regionUrl, events);
		}

		private static string GetRegionUrl(SubscribableRegion region) => region switch
		{
			SubscribableRegion.state				  => BASE_URL + "/state/nsw",
			SubscribableRegion.south_metropolitan	  => BASE_URL + "/region/sm",
			SubscribableRegion.sydney_north			  => BASE_URL + "/region/sn",
			SubscribableRegion.greater_western_sydney => BASE_URL + "/region/gws",
			SubscribableRegion.hume					  => BASE_URL + "/region/hume",
			SubscribableRegion.south_coast_tablelands => BASE_URL + "/region/sct",
			SubscribableRegion.swash				  => BASE_URL + "/region/water-activities-centre",

			_ => throw new NotImplementedException()
		};

		internal virtual HtmlDocument GetHtmlDocument(string url)
			=> new HtmlWeb().Load(url);
	}

	public class Region
	{
		public readonly string Name;

		public readonly string Link;

		public readonly List<Event> Events;

		public Region(string name, string link, List<Event> events)
		{
			Name = name;
			Link = link;
			Events = events;
		}
	}

	public class Event
	{
		public readonly string Name;

		public readonly int Id;

		public readonly string Info;

		public readonly string RegistrationStatus;

		public readonly string RegistrationSeverity;

		public Event(string name, int id, string info, string registrationStatus, string registrationSeverity)
		{
			Name = name;
			Id = id;
			Info = info;
			RegistrationStatus = registrationStatus;
			RegistrationSeverity = registrationSeverity;
		}

		public string Link => $"{ScoutEventAPIClient.BASE_URL}/event/{Id}";

		public string Emoji => RegistrationSeverity switch
		{
			"success" => "✅",
			"warning" => "⚠️",
			"danger" => "⛔",

			_ => "❓"
		};
	}

	public enum SubscribableRegion
	{
		state,
		south_metropolitan,
		sydney_north,
		greater_western_sydney,
		hume,
		south_coast_tablelands,
		swash,
	}
}
