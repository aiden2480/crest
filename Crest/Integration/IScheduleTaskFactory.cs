using Crest.Implementation;

namespace Crest.Integration
{
	internal interface IScheduleTaskFactory
	{
		/// <summary>
		/// Reads the specified configuration and creates as many tasks as may be needed
		/// </summary>
		/// <param name="config">The parsed configuration object</param>
		/// <returns>An IEnumerable of the created tasks based on the config</returns>
		IEnumerable<IScheduleTask> GetScheduleTasks(ApplicationConfiguration config);
	}
}
