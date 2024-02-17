using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Crest.Utilities;
using Moq;
using NUnit.Framework;

namespace Crest.Test
{
	public class ScoutEventCrawlerTaskTests : DeleteProgramDataBeforeTest
	{
		[Test]
		public void TestOneRegion_NoPreviouslySeenEvents()
		{
			// Arrange input data
			var taskConfig = new ScoutEventCrawlerTaskConfig()
			{
				JandiUrl = "https://jandiurl.com",
				SubscribedRegions = { SubscribableRegion.south_metropolitan }
			};

			var mockRegion = new Region("South Metropolitan Region", "https://events.nsw.scouts.com.au/region/sm", new List<Event>
			{
				new("Paddling Day", 123, "Saturday 23rd December to Sunday 24th December", "Open", "success"),
				new("Rock Climbing", 456, "Sunday 1st Jan to Monday 2nd Jan", "Closed", "danger"),
				new("Abseiling", 789, "March 15th", "Open (Restricted)", "warning"),
			});

			// Arrange mocks
			var mockScoutEventClient = new Mock<ScoutEventAPIClient>();
			var mockJandiClient = new Mock<JandiAPIClient>();
			var task = MockScoutEventCrawlerTask(mockJandiClient, mockScoutEventClient);

			mockScoutEventClient.Setup(c => c.ScanRegion(SubscribableRegion.south_metropolitan)).Returns(mockRegion);
			mockJandiClient.Setup(c => c.SendMessage(It.IsAny<string>(), It.IsAny<JandiMessage>()))
				.Callback((string url, JandiMessage message) => AssertJandiMessagePosted(url, message));

			// Act
			task.Run(taskConfig);

			// Assert
			mockJandiClient.Verify(c => c.SendMessage(It.IsAny<string>(), It.IsAny<JandiMessage>()), Times.Once);
		}
	
		private static ScoutEventCrawlerTask MockScoutEventCrawlerTask(Mock<JandiAPIClient> mockJandiClient, Mock<ScoutEventAPIClient> mockScoutEventClient)
		{
			var mockTask = new Mock<ScoutEventCrawlerTask>(mockJandiClient.Object, mockScoutEventClient.Object) { CallBase = true };

			mockTask.SetupGet(m => m.StatePath).Returns(ProgramDataLocation);
			mockTask.SetupGet(t => t.StateKey).Returns("crawler1-ScoutEventCrawlerTask");

			return mockTask.Object;
		}

		private static void AssertJandiMessagePosted(string url, JandiMessage message)
		{
			Assert.Multiple(() =>
			{
				Assert.That(url, Is.EqualTo("https://jandiurl.com"));

				Assert.That(message.Body, Is.EqualTo("New ScoutLink events"));
				Assert.That(message.ConnectColor, Is.EqualTo("#84BC48"));
				Assert.That(message.ConnectInfo, Has.Count.EqualTo(1));

				var connect = message.ConnectInfo[0];
				Assert.That(connect.Title, Is.EqualTo("⚜️ [South Metropolitan Region](https://events.nsw.scouts.com.au/region/sm)"));
				Assert.That(connect.Description, Is.EqualTo("[Paddling Day](https://events.nsw.scouts.com.au/event/123) • Open ✅\nSaturday 23rd December to Sunday 24th December\n\n[Abseiling](https://events.nsw.scouts.com.au/event/789) • Open (Restricted) ⚠️\nMarch 15th"));
			});
		}
	}
}
