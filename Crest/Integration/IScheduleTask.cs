using Quartz;

namespace Crest.Integration
{
	internal interface IScheduleTask : IJob
	{
		/// <summary>
		/// The unique name associated with this task
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The job associated with this task
		/// </summary>
		IJobDetail Job { get; }

		/// <summary>
		/// The trigger associated with this task
		/// </summary>
		ITrigger Trigger { get; }

		/// <summary>
		/// Runs any one-time setup that may be necessary prior to the task's first execution
		/// </summary>
		void OneTimeSetup();
	}
}
