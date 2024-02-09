using Crest.Integration;
using Crest.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using System.Globalization;

namespace Crest.Extensions.TerrainApprovals
{
	public class TerrainApprovalsTask : IJob
	{
		readonly TerrainAPIClient TerrainClient;

		public TerrainApprovalsTask()
		{
			TerrainClient = new TerrainAPIClient();
		}

		public Task Execute(IJobExecutionContext context)
		{
			var configString = context.MergedJobDataMap.GetString("config");
			var config = JsonConvert.DeserializeObject<TerrainApprovalsTaskConfig>(configString);

			if (!TerrainClient.Login(config.Username, config.Password, out var loginFailureReason))
			{
				Console.WriteLine(loginFailureReason);
				context.Scheduler.PauseTrigger(context.Trigger.Key);

				return Task.CompletedTask;
			}

			var pendingApprovals = TerrainClient.GetPendingApprovals(config.UnitId.ToString());
			var finalisedApprovals = TerrainClient.GetFinalisedApprovals(config.UnitId.ToString())
				.Where(a => (DateTime.Now - ParseApprovalDateString(a)).TotalDays <= config.LookbackDays)
				.Where(a => a["submission"]["outcome"].ToString().Equals("approved"));

			var pendingGroups = GroupApprovals(pendingApprovals);
			var finalisedGroups = GroupApprovals(finalisedApprovals);

			var pendingEmbed = GetJandiEmbed("Pending approval requests", "#FAC11B", "No pending approvals", pendingGroups);
			var finalisedEmbed = GetJandiEmbed($"Approved in the last {config.LookbackDays} days", "#2ECC71", "No recent approvals", finalisedGroups);

			JandiAPIClient.SendMessage(config.JandiUrl, pendingEmbed);
			JandiAPIClient.SendMessage(config.JandiUrl, finalisedEmbed);

			return Task.CompletedTask;
		}

		static DateTime ParseApprovalDateString(JToken approval)
			=> DateTime.ParseExact(approval["submission"]["date"].ToString(), "dd-MMM-yyyy h:mm:ss tt", null);

		Dictionary<string, string> GroupApprovals(IEnumerable<JToken> approvals)
		{
			var groups = new Dictionary<string, string>();

			foreach (var approval in approvals)
			{
				var memberName = approval["member"]["first_name"] + " " + approval["member"]["last_name"];
				var approvalDescription = GetApprovalDescription(approval);

				if (approvalDescription == string.Empty)
				{
					continue;
				}

				if (!groups.ContainsKey(memberName))
				{
					groups[memberName] = approvalDescription;
				}
				else
				{
					groups[memberName] += "\n" + approvalDescription;
				}
			}

			return groups;
		}

		string GetApprovalDescription(JToken approval)
		{
			return approval["achievement"]["type"].ToString() switch
			{
				"intro_scouting" => "⚜️ Introduction to Scouting",
				"intro_section" => "🗣️ Introduction to Section",
				"course_reflection" => "📚 Personal Development Course",
				"adventurous_journey" => "🚀 Adventurous Journey",
				"personal_reflection" => "📐 Personal Reflection",
				"peak_award" => "⭐ Peak Award",

				"outdoor_adventure_skill" => GetOASDescription(approval),
				"special_interest_area" => GetSIADescription(approval),
				"milestone" => GetMilestoneDescription(approval),

				_ => "Unknown achievement",
			};
		}

		string GetOASDescription(JToken approval)
		{
			var achievementData = GetAchievementMeta(approval);

			if (achievementData == null)
			{
				return string.Empty;
			}

			var textInfo = CultureInfo.CurrentCulture.TextInfo;
			var branch = textInfo.ToTitleCase(achievementData["achievement_meta"]["branch"]
				.ToString().Replace("-", " "));

			var stage = achievementData["achievement_meta"]["stage"].ToString();
			var emoji = achievementData["achievement_meta"]["stream"].ToString() switch
			{
				"alpine" => "❄️",
				"aquatics" => "🏊",
				"boating" => "⛵",
				"bushcraft" => "🏞️",
				"bushwalking" => "🥾",
				"camping" => "⛺",
				"cycling" => "🚲",
				"paddling" => "🛶",
				"vertical" => "🧗",

				_ => "⭐",
			};

			return $"{emoji} {branch} stage {stage}";
		}

		string GetSIADescription(JToken approval)
		{
			var achievementData = GetAchievementMeta(approval);

			if (achievementData == null)
			{
				return string.Empty;
			}

			var projectName = achievementData["answers"]["project_name"]
				.ToString().Trim();

			var submissionType = approval["submission"]["type"].ToString();
			var category = achievementData["answers"]["special_interest_area_selection"].ToString() switch
			{
				"sia_adventure_sport" => "🏈 Adventure & Sport",
				"sia_art_literature" => "🎭 Arts & Literature",
				"sia_better_world" => "🌏 Creating a Better World",
				"sia_environment" => "♻️ Environment",
				"sia_growth_development" => "🌱 Growth & Development",
				"sia_stem_innovation" => "🔎 STEM & Innovation",

				_ => "❓ Unknown",
			};

			return $"{category} SIA - {projectName} ({submissionType})";
		}

		string GetMilestoneDescription(JToken approval)
		{
			var achievementData = GetAchievementMeta(approval);

			if (achievementData == null)
			{
				return string.Empty;
			}

			var stage = achievementData["achievement_meta"]["stage"].ToString();

			return $"👣 Milestone {stage}";
		}

		JToken GetAchievementMeta(JToken approval)
			=> TerrainClient.GetMemberAchievements(approval["member"]["id"].ToString())
				.FirstOrDefault(a => a["id"].ToString() == approval["achievement"]["id"].ToString());

		static JandiMessage GetJandiEmbed(string body, string colour, string emptyApprovalsMessage, Dictionary<string, string> approvals)
		{
			var jandiEmbed = new JandiMessage
			{
				Body = body,
				ConnectColor = colour,
			};

			foreach (var key in approvals.Keys.OrderBy(a => a))
			{
				jandiEmbed.ConnectInfo.Add(new JandiConnect
				{
					Title = key,
					Description = approvals[key]
				});
			}

			if (!jandiEmbed.ConnectInfo.Any())
			{
				jandiEmbed.ConnectInfo.Add(new JandiConnect { Description = emptyApprovalsMessage });
			}

			return jandiEmbed;
		}
	}
}
