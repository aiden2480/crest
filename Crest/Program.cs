using Crest.Integration;
using YamlDotNet.Core;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Crest;

public class Program
{
	async static Task Main()
	{
		var configPath = "config.yaml";
		var appConfig = GetValidAppConfiguration(configPath);
		var crest = new Crest(appConfig);

		await crest.RunForever();
	}

	internal static ApplicationConfiguration GetValidAppConfiguration(string configPath)
	{
		if (!File.Exists(configPath))
		{
			throw new FileNotFoundException($"You need to create a configuration file named {configPath}", configPath);
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

			throw new ArgumentException($"Unrecognised argument supplied: {unrecognisedArg} - please remove", e);
		}
		catch (YamlException e) when (e.InnerException != null && e.InnerException.Message.Contains("is not a valid YAML Boolean"))
		{
			var firstQuote = e.InnerException.Message.IndexOf('"');
			var secondQuote = e.InnerException.Message.IndexOf('"', firstQuote + 1);
			var invalidArg = e.InnerException.Message[firstQuote..(secondQuote + 1)];

			throw new ArgumentException($"Invalid boolean value {invalidArg} supplied - replace with yes/no", e.InnerException);
		}
		// todo does not throw when mandatory argument not supplied
		// todo should also have better error handling for subscribable_regions enum
	}
}
