using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Crest.Test.Utilities;
using Crest.Utilities;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Crest.Test;

public class ScoutEventCrawlerTaskTests : DeleteProgramDataBeforeTest
{
	[Test]
	public void TestOneRegion_NoPreviouslySeenEvents()
	{
		// Arrange
		CreateMocks(out var task, out var mockJandiClient, out var mockScoutEventClient);

		var taskConfig = new ScoutEventCrawlerTaskConfig()
		{
			JandiUrl = "https://jandiurl.com",
			SubscribedRegions = { SubscribableRegion.south_metropolitan }
		};

		var mockSouthMetRegion = new Region("South Metropolitan Region", "https://events.nsw.scouts.com.au/region/sm", new List<Event>
		{
			new("Paddling Day", 123, "Saturday 23rd December to Sunday 24th December", "Open", "success"),
			new("Rock Climbing", 456, "Sunday 1st Jan to Monday 2nd Jan", "Closed", "danger"),
			new("Abseiling", 789, "March 15th", "Open (Restricted)", "warning"),
		});

		var expectedJandiMessage = new JandiMessage()
		{
			Body = "New ScoutLink events",
			ConnectColor = "#84BC48",
			ConnectInfo = new List<JandiConnect>
			{
				new() { Title = "⚜️ [South Metropolitan Region](https://events.nsw.scouts.com.au/region/sm)", Description = "[Paddling Day](https://events.nsw.scouts.com.au/event/123) • Open ✅\nSaturday 23rd December to Sunday 24th December\n\n[Abseiling](https://events.nsw.scouts.com.au/event/789) • Open (Restricted) ⚠️\nMarch 15th" },
			}
		};

		mockScoutEventClient.Setup(c => c.ScanRegion(SubscribableRegion.south_metropolitan)).Returns(mockSouthMetRegion);

		// Act
		task.Run(taskConfig);

		// Assert
		mockJandiClient.Verify(c => c.SendMessage(
			taskConfig.JandiUrl,
			It.Is((JandiMessage message) => Serialise(message) == Serialise(expectedJandiMessage))
		), Times.Once);
	}

	#region Helpers

	private static void CreateMocks(out ScoutEventCrawlerTask task, out Mock<JandiAPIClient> mockJandiClient, out Mock<ScoutEventAPIClient> mockScoutEventClient, bool verbose = false)
	{
		mockScoutEventClient = new Mock<ScoutEventAPIClient>();
		mockJandiClient = new Mock<JandiAPIClient>();
		var mockTask = new Mock<ScoutEventCrawlerTask>(mockJandiClient.Object, mockScoutEventClient.Object) { CallBase = true };
		task = mockTask.Object;

		mockTask.SetupGet(m => m.StatePath).Returns(ProgramDataLocation);
		mockTask.SetupGet(t => t.StateKey).Returns("crawler1-ScoutEventCrawlerTask");

		if (verbose)
		{
			mockJandiClient
				.Setup(c => c.SendMessage(It.IsAny<string>(), It.IsAny<JandiMessage>()))
				.Callback(JandiCallback);
		}
	}

	private static string Serialise(JandiMessage message)
		=> JsonConvert.SerializeObject(message);

	private static void JandiCallback(string url, JandiMessage message)
	{
		var formattedMessage = JsonConvert.SerializeObject(message, Formatting.Indented);
		Console.WriteLine($"SendMessage called for url {url}\nmessage {formattedMessage}");
	}

	#endregion
}
