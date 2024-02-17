using Crest.Test.Utilities;
using NUnit.Framework;

namespace Crest.Test;

public class ConfigurationTests
{
	[Test]
	public void TestErrorThrownIfConfigFileDoesNotExist()
	{
		var invalidFileLocation = "notconfig.yaml";
		Assert.That(File.Exists(invalidFileLocation), Is.False);

		var exception = Assert.Throws<FileNotFoundException>(() => Program.GetValidAppConfiguration(invalidFileLocation));
		Assert.That(exception.FileName, Is.EqualTo(invalidFileLocation));
		Assert.That(exception.Message, Is.EqualTo($"You need to create a configuration file named {invalidFileLocation}"));
	}

	[Test]
	public void TestUnrecognisedArgumentSupplied()
	{
		using var configFile = new TemporaryConfigFile("testconfig.yaml", "unrecognisedArg: false");

		var exception = Assert.Throws<ArgumentException>(() => Program.GetValidAppConfiguration(configFile.Filename));
		Assert.That(exception.Message, Is.EqualTo("Unrecognised argument supplied: 'unrecognisedArg' - please remove"));
	}

	[Test]
	public void TestInvalidBooleanValue()
	{
		using var configFile = new TemporaryConfigFile("testconfig.yaml", "terrain_approvals:\n  enabled: invalidBoolValue");

		var exception = Assert.Throws<ArgumentException>(() => Program.GetValidAppConfiguration(configFile.Filename));
		Assert.That(exception.Message, Is.EqualTo("Invalid boolean value \"invalidBoolValue\" supplied - replace with yes/no"));
	}

	[TestCaseSource(nameof(ValidYamlCases))]
	public void TestValidYaml(string filename)
	{
		var validYaml = File.ReadAllText(filename);
		using var configFile = new TemporaryConfigFile("testconfig.yaml", validYaml);

		Assert.DoesNotThrow(() => Program.GetValidAppConfiguration(configFile.Filename));
	}

	static IEnumerable<string> ValidYamlCases => new List<string>
	{
		"TestFiles/ValidYaml1.yaml",
		"TestFiles/ValidYaml2.yaml",
		"TestFiles/ValidYaml3.yaml",
	};
}
