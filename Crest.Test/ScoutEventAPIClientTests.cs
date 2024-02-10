using Crest.Utilities;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace Crest.Test
{
	public class ScoutEventAPIClientTests
	{
		[Test]
		public void TestParsesValidData()
		{
			// Arrange
			var document = new HtmlDocument();
			var mockClient = new Mock<ScoutEventAPIClient> { CallBase = true };
			var client = mockClient.Object;

			document.Load("TestFiles/ScoutEventExample.html");
			mockClient.Setup(c => c.GetHtmlDocument(It.IsAny<string>())).Returns(document);

			// Act
			var region = client.ScanRegion(SubscribableRegion.south_metropolitan);

			// Assert
			Assert.Multiple(() =>
			{
				Assert.That(region, Is.Not.Null);
				Assert.That(region.Name, Is.EqualTo("South Metropolitan Region"));
				Assert.That(region.Link, Is.EqualTo("https://events.nsw.scouts.com.au/region/sm"));
				Assert.That(region.Events, Has.Count.EqualTo(41));

				var item = region.Events[0]; // Closed event
				Assert.That(item.Name, Is.EqualTo("Adventurous Activity Guide Indentureship Program"));
				Assert.That(item.Id, Is.EqualTo(1186));
				Assert.That(item.Info, Is.EqualTo("Saturday 20th Jan (12am) to Saturday 23rd Nov (12am)"));
				Assert.That(item.RegistrationStatus, Is.EqualTo("Closed"));
				Assert.That(item.RegistrationSeverity, Is.EqualTo("danger"));
				Assert.That(item.Link, Is.EqualTo("https://events.nsw.scouts.com.au/event/1186"));
				Assert.That(item.Emoji, Is.EqualTo("⛔")); 
				
				var item2 = region.Events[7]; // Open event
				Assert.That(item2.Name, Is.EqualTo("Zero to Hero Abseil program for Venturers"));
				Assert.That(item2.Id, Is.EqualTo(1177));
				Assert.That(item2.Info, Is.EqualTo("Saturday 24th Feb (8am) to Sunday 25th Feb (3pm)\nAttendance is also required at a theory session"));
				Assert.That(item2.RegistrationStatus, Is.EqualTo("Open (Limited Spaces)"));
				Assert.That(item2.RegistrationSeverity, Is.EqualTo("success"));
				Assert.That(item2.Link, Is.EqualTo("https://events.nsw.scouts.com.au/event/1177"));
				Assert.That(item2.Emoji, Is.EqualTo("✅"));

				var item3 = region.Events[40]; // Pending event
				Assert.That(item3.Name, Is.EqualTo("Fortress Creek Canyon"));
				Assert.That(item3.Id, Is.EqualTo(1410));
				Assert.That(item3.Info, Is.EqualTo("Saturday 14th December\n8am to 4pm"));
				Assert.That(item3.RegistrationStatus, Is.EqualTo("Pending"));
				Assert.That(item3.RegistrationSeverity, Is.EqualTo("warning"));
				Assert.That(item3.Link, Is.EqualTo("https://events.nsw.scouts.com.au/event/1410"));
				Assert.That(item3.Emoji, Is.EqualTo("⚠️"));
			});
		}

		[TestCaseSource(nameof(Branches))]
		public void TestLiveBranchUrl(SubscribableRegion regionEnum, string regionName, string regionUrl)
		{
			// Arrange
			var client = new ScoutEventAPIClient();

			// Act
			var region = client.ScanRegion(regionEnum);

			// Assert
			Assert.Multiple(() =>
			{
				Assert.That(region, Is.Not.Null);

				Assert.That(region.Name, Is.EqualTo(regionName));
				Assert.That(region.Link, Is.EqualTo(regionUrl));
			});
		}

		static IEnumerable<object[]> Branches() => new List<object[]>
		{
			new object[] { SubscribableRegion.state, "Scouts NSW", "https://events.nsw.scouts.com.au/state/nsw" },
			new object[] { SubscribableRegion.south_metropolitan, "South Metropolitan Region", "https://events.nsw.scouts.com.au/region/sm" },
			new object[] { SubscribableRegion.sydney_north, "Sydney North Region", "https://events.nsw.scouts.com.au/region/sn" },
			new object[] { SubscribableRegion.greater_western_sydney, "Greater Western Sydney Region", "https://events.nsw.scouts.com.au/region/gws" },
			new object[] { SubscribableRegion.hume, "Hume Region", "https://events.nsw.scouts.com.au/region/hume" },
			new object[] { SubscribableRegion.south_coast_tablelands, "South Coast and Tablelands Region", "https://events.nsw.scouts.com.au/region/sct" },
			new object[] { SubscribableRegion.swash, "Water Activities Centre", "https://events.nsw.scouts.com.au/region/water-activities-centre" }
		};
	}
}
