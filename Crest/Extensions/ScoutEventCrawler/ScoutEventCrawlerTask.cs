using Crest.Integration;
using Crest.Utilities;

namespace Crest.Extensions.TerrainApprovals
{
	public class ScoutEventCrawlerTask : ScheduleTask<ScoutEventCrawlerTaskConfig>
	{
		public override void Run(ScoutEventCrawlerTaskConfig config)
		{
			var client = new ScoutEventAPIClient();
			var regions = config.SubscribedRegions.Select(r => client.ScanRegion(r));
		}
	}
}
