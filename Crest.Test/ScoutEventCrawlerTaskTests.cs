using Crest.Extensions.TerrainApprovals;
using Crest.Integration;
using Crest.TestUtilities;
using Crest.Utilities;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Crest;

public class ScoutEventCrawlerTaskTests : DeleteProgramDataBeforeTest
{
	[Test]
	public void TestSingularRegion_NoPreviouslySeenEvents()
	{
		var taskConfig = new ScoutEventCrawlerTaskConfig()
		{
			JandiUrl = "https://jandiurl.com",
			SubscribedRegions = { SubscribableRegion.south_metropolitan }
		};

		var expectedJandiMessage = new JandiMessage()
		{
			Body = "New ScoutEvent listings",
			ConnectColor = "#84BC48",
			ConnectInfo = new List<JandiConnect>
			{
				new() { Title = "⚜️ [South Metropolitan Region](https://events.nsw.scouts.com.au/region/sm)", Description = "[Paddling Day](https://events.nsw.scouts.com.au/event/123) • Open ✅\nSaturday 23rd December to Sunday 24th December\n\n[Abseiling](https://events.nsw.scouts.com.au/event/789) • Open (Restricted) ⚠️\nMarch 15th" },
			}
		};

		AssertJandiMessageIsSent(taskConfig, expectedJandiMessage);
	}

	[Test]
	public void TestSingularRegion_AllEventsPreviouslySeen()
	{
		var previouslySeenEvents = new List<int> { 123, 789 };

		var taskConfig = new ScoutEventCrawlerTaskConfig()
		{
			JandiUrl = "https://jandiurl.com",
			SubscribedRegions = { SubscribableRegion.south_metropolitan }
		};

		var expectedJandiMessage = new JandiMessage()
		{
			Body = "New ScoutEvent listings",
			ConnectColor = "#84BC48",
			ConnectInfo = new List<JandiConnect>
			{
				new() { Title = null, Description = "No new events posted for any subscribed regions" },
			}
		};

		AssertJandiMessageIsSent(taskConfig, expectedJandiMessage, previouslySeenEvents);
	}

	[Test]
	public void TestMultipleRegions_SomePreviouslySeenOrDuplicateEvents()
	{
		var previouslySeenEvents = new List<int> { 123 };

		var taskConfig = new ScoutEventCrawlerTaskConfig()
		{
			JandiUrl = "https://jandiurl.com",
			SubscribedRegions = { SubscribableRegion.south_metropolitan, SubscribableRegion.sydney_north }
		};

		var expectedJandiMessage = new JandiMessage()
		{
			Body = "New ScoutEvent listings",
			ConnectColor = "#84BC48",
			ConnectInfo = new List<JandiConnect>
			{
				new() { Title = "⚜️ [South Metropolitan Region](https://events.nsw.scouts.com.au/region/sm)", Description = "[Abseiling](https://events.nsw.scouts.com.au/event/789) • Open (Restricted) ⚠️\nMarch 15th" },
				new() { Title = "⚜️ [Sydney North Region](https://events.nsw.scouts.com.au/region/sn)", Description = "[Canyoning](https://events.nsw.scouts.com.au/event/1178) • Open ✅\nOctober 20th" },
			}
		};

		AssertJandiMessageIsSent(taskConfig, expectedJandiMessage, previouslySeenEvents);
	}

	[Test]
	public void TestManyRegions_SomePreviouslySeenOrDuplicateEvents()
	{
		var previouslySeenEvents = new List<int> { 123, 134, 789 };

		var taskConfig = new ScoutEventCrawlerTaskConfig()
		{
			JandiUrl = "https://jandiurl.com",
			SubscribedRegions = { SubscribableRegion.sydney_north, SubscribableRegion.state, SubscribableRegion.south_metropolitan }
		};

		var expectedJandiMessage = new JandiMessage()
		{
			Body = "New ScoutEvent listings",
			ConnectColor = "#84BC48",
			ConnectInfo = new List<JandiConnect>
			{
				new() { Title = "⚜️ [Sydney North Region](https://events.nsw.scouts.com.au/region/sn)", Description = "[Canyoning](https://events.nsw.scouts.com.au/event/1178) • Open ✅\nOctober 20th" },
				new() { Title = "⚜️ [Scouts NSW](https://events.nsw.scouts.com.au/state/nsw)", Description = "[Caving](https://events.nsw.scouts.com.au/event/1783) • Pending ⚠️\nNovember 4th" },
			}
		};

		AssertJandiMessageIsSent(taskConfig, expectedJandiMessage, previouslySeenEvents);
	}

	#region Helpers

	private static void AssertJandiMessageIsSent(ScoutEventCrawlerTaskConfig taskConfig, JandiMessage expectedJandiMessage, List<int> previouslySeenEvents)
	{
		// Arrange
		CreateMocks(out var task, out var mockScoutEventClient);
		task.SetState(previouslySeenEvents);

		var mockSouthMetRegion = new Region("South Metropolitan Region", "https://events.nsw.scouts.com.au/region/sm", new List<Event>
		{
			new("Paddling Day", 123, "Saturday 23rd December to Sunday 24th December", "Open", "success"),
			new("Rock Climbing", 456, "Sunday 1st Jan to Monday 2nd Jan", "Closed", "danger"), // none should have 456 because it is closed
			new("Abseiling", 789, "March 15th", "Open (Restricted)", "warning"),
		});

		var mockSydneyNorthRegion = new Region("Sydney North Region", "https://events.nsw.scouts.com.au/region/sn", new List<Event>
		{
			new("Rock Climbing", 456, "Sunday 1st Jan to Monday 2nd Jan", "Closed", "danger"), // none should have 456 because it is closed
			new("Canyoning", 1178, "October 20th", "Open", "success"),
		});

		var mockStateRegion = new Region("Scouts NSW", "https://events.nsw.scouts.com.au/state/nsw", new List<Event>
		{
			new("Cycling", 134, "May 4th to May 5th", "Open", "success"),
			new("Caving", 1783, "November 4th", "Pending", "warning"),
			new("Diving", 1345, "April 21st", "Closed", "danger"), // none should have 1345 because it is closed
		});

		mockScoutEventClient.Setup(c => c.ScanRegion(SubscribableRegion.south_metropolitan)).Returns(mockSouthMetRegion);
		mockScoutEventClient.Setup(c => c.ScanRegion(SubscribableRegion.sydney_north)).Returns(mockSydneyNorthRegion);
		mockScoutEventClient.Setup(c => c.ScanRegion(SubscribableRegion.state)).Returns(mockStateRegion);

		// Setup Asserts
		int messagesSent = 0;

		using var sendMessageOverride = new JandiAPISendMessageOverrideForTest((url, message) =>
		{
			Assert.Multiple(() =>
			{
				Assert.That(++messagesSent, Is.EqualTo(1));
				Assert.That(url, Is.EqualTo(taskConfig.JandiUrl));
				Assert.That(Serialise(message), Is.EqualTo(Serialise(expectedJandiMessage)));
			});
		});

		// Act
		task.Run(taskConfig);
	}

	private static void AssertJandiMessageIsSent(ScoutEventCrawlerTaskConfig taskConfig, JandiMessage expectedJandiMessage)
		=> AssertJandiMessageIsSent(taskConfig, expectedJandiMessage, new List<int>());

	private static void CreateMocks(out ScoutEventCrawlerTask task, out Mock<ScoutEventAPIClient> mockScoutEventClient)
	{
		mockScoutEventClient = new Mock<ScoutEventAPIClient>(behavior: MockBehavior.Strict);
		var mockTask = new Mock<ScoutEventCrawlerTask>(mockScoutEventClient.Object) { CallBase = true };
		task = mockTask.Object;

		mockTask.SetupGet(m => m.StatePath).Returns(ProgramDataLocation);
		mockTask.SetupGet(t => t.StateKey).Returns("taskName-ScoutEventCrawlerTask");
	}

	private static string Serialise(JandiMessage message)
		=> JsonConvert.SerializeObject(message);

	#endregion
}

class JandiAPISendMessageOverrideForTest : IDisposable
{
	readonly Func<string, JandiMessage, HttpResponseMessage> InitialSendMessage;

	public JandiAPISendMessageOverrideForTest(Action<string, JandiMessage> newFunc)
	{
		InitialSendMessage = JandiAPIClient.SendMessage;

		JandiAPIClient.SendMessage = (url, message) =>
		{
			newFunc.Invoke(url, message);

			return null; 
		};
	}

	public void Dispose()
	{
		JandiAPIClient.SendMessage = InitialSendMessage;
	}
}
