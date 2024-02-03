namespace Crest.Integration
{
	internal interface ITaskConfigFactory
	{
		/// <summary>
		/// Checks the <paramref name="applicationConfig"/> for any tasks defined under this extension, and validates them, e.g. checks credentials are correct
		/// </summary>
		/// <param name="config">The parsed configuration object</param>
		/// <returns>An iterable of task configurations for this extension</returns>
		IEnumerable<ITaskConfig> GetValidConfigs(ApplicationConfiguration applicationConfig);
	}
}
