namespace Crest.Implementation
{
	public class ApplicationConfiguration
	{
		/// <summary>
		/// ApplicationConfiguration for the TerrainApprovals extension
		/// </summary>
		public ExtensionConfiguration<TerrainApprovalsTaskConfig>? TerrainApprovals;
	}
	
	public class ExtensionConfiguration<T>
	{
		/// <summary>
		/// A boolean indicating whether this extension should be enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// A mapping of task instances: user friendly name, and config
		/// </summary>
		public Dictionary<string, T> Tasks = new();
	}

	public class TerrainApprovalsTaskConfig
	{
		/// <summary>
		/// FriendlyName for Scouts Terrain. Should be in the format branch-memberID
		/// For example nsw-132323
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Password for Scouts Terrain
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// The incoming webhook URL for the jandi topic to post to
		/// </summary>
		public string JandiUrl { get; set; }
	}
}
