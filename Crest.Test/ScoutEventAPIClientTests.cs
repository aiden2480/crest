﻿using Crest.Utilities;
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

			var url = "https://events.nsw.scouts.com.au/region/sm";
			var client = mockClient.Object;

			document.Load("TestFiles/ScoutEventExample.html");
			mockClient.Setup(c => c.GetHtmlDocument(url)).Returns(document);

			// Act
			var region = client.ScanRegionUrl(url);

			// Assert
			Assert.Multiple(() =>
			{
				Assert.That(region, Is.Not.Null);
				Assert.That(region.Name, Is.EqualTo("South Metropolitan Region"));
				Assert.That(region.Link, Is.EqualTo(url));
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

		[Test]
		public void TestThrowsErrorForInvalidRegionUrl()
		{
			// Arrange
			var invalidUrl = "https://google.com";
			var client = new ScoutEventAPIClient();

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => client.ScanRegionUrl(invalidUrl));
			Assert.That(exception.Message, Is.EqualTo($"URL {invalidUrl} should start with https://events.nsw.scouts.com.au (Parameter 'regionUrl')"));
		}

		[TestCaseSource(nameof(Branches))]
		public void TestLiveBranchUrl(string branchName, string branchUrl)
		{
			// Arrange
			var client = new ScoutEventAPIClient();

			// Act
			var region = client.ScanRegionUrl(branchUrl);

			// Assert
			Assert.Multiple(() =>
			{
				Assert.That(region, Is.Not.Null);

				Assert.That(region.Name, Is.EqualTo(branchName));
				Assert.That(region.Link, Is.EqualTo(branchUrl));
			});
		}

		static IEnumerable<string[]> Branches() => new List<string[]>
		{
			new [] { "Scouts NSW", "https://events.nsw.scouts.com.au/state/nsw" },
			new [] { "South Metropolitan Region", "https://events.nsw.scouts.com.au/region/sm" },
			new [] { "Sydney North Region", "https://events.nsw.scouts.com.au/region/sn" },
			new [] { "Greater Western Sydney Region", "https://events.nsw.scouts.com.au/region/gws" },
			new [] { "Hume Region", "https://events.nsw.scouts.com.au/region/hume" },
			new [] { "South Coast and Tablelands Region", "https://events.nsw.scouts.com.au/region/sct" },
			new [] { "Water Activities Centre", "https://events.nsw.scouts.com.au/region/water-activities-centre" }
		};
	}
}
