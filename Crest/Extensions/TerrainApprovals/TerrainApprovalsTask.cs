using Crest.Integration;
using Crest.Utilities;
using System.Globalization;

namespace Crest.Extensions.TerrainApprovals
{
	public class TerrainApprovalsTask : ScheduleTask<TerrainApprovalsTaskConfig>
	{
		readonly TerrainAPIClient TerrainClient;

		public TerrainApprovalsTask()
		{
			TerrainClient = new TerrainAPIClient();
		}

		public override bool ShouldRun(TerrainApprovalsTaskConfig config)
		{
			var loginSucceeded = TerrainClient.Login(config.Username, config.Password, out var loginFailureReason);

			if (!loginSucceeded)
			{
				Console.WriteLine(loginFailureReason);
			}

			return loginSucceeded;
		}

		public override void Run(TerrainApprovalsTaskConfig config)
		{
			var jandiClient = new JandiAPIClient();

			var pendingApprovals = TerrainClient.GetPendingApprovals(config.UnitId.ToString());
			var finalisedApprovals = TerrainClient.GetFinalisedApprovals(config.UnitId.ToString())
				.Where(a => (DateTime.Now - a.SubmissionDate).TotalDays <= config.LookbackDays)
				.Where(a => a.SubmissionOutcome.Equals("approved"));

			var pendingGroups = GroupApprovals(pendingApprovals);
			var finalisedGroups = GroupApprovals(finalisedApprovals);

			var pendingEmbed = GetJandiEmbed("Pending approval requests", "#FAC11B", "No pending approvals", pendingGroups);
			var finalisedEmbed = GetJandiEmbed($"Approved in the last {config.LookbackDays} days", "#2ECC71", "No recent approvals", finalisedGroups);

			jandiClient.SendMessage(config.JandiUrl, pendingEmbed);
			jandiClient.SendMessage(config.JandiUrl, finalisedEmbed);
		}

		Dictionary<string, string> GroupApprovals(IEnumerable<Approval> approvals)
		{
			var groups = new Dictionary<string, string>();

			foreach (var approval in approvals)
			{
				var memberName = approval.MemberFirstName + " " + approval.MemberLastName;
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

		string GetApprovalDescription(Approval approval)
		{
			return approval.AchievementType switch
			{
				"intro_scouting" => "⚜️ Introduction to Scouting",
				"intro_section" => "🗣️ Introduction to Section",
				"course_reflection" => "📚 Personal Development Course",
				"adventurous_journey" => $"🚀 Adventurous Journey ({approval.SubmissionType})",
				"personal_reflection" => "📐 Personal Reflection",
				"peak_award" => "⭐ Peak Award",

				"outdoor_adventure_skill" => GetOASDescription(approval),
				"special_interest_area" => GetSIADescription(approval),
				"milestone" => GetMilestoneDescription(approval),

				_ => "Unknown achievement",
			};
		}

		string GetOASDescription(Approval approval)
		{
			var achievement = GetAchievementMeta(approval);

			if (achievement == null)
			{
				return string.Empty;
			}

			var textInfo = CultureInfo.CurrentCulture.TextInfo;
			var branch = textInfo.ToTitleCase(achievement.Branch.Replace("-", " "));

			var emoji = achievement.Stream switch
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

			return $"{emoji} {branch} stage {achievement.Stage}";
		}

		string GetSIADescription(Approval approval)
		{
			var achievement = GetAchievementMeta(approval);

			if (achievement == null)
			{
				return string.Empty;
			}

			var projectName = achievement.SIAProjectName.Trim();

			var category = achievement.SIASelection switch
			{
				"sia_adventure_sport" => "🏈 Adventure & Sport",
				"sia_art_literature" => "🎭 Arts & Literature",
				"sia_better_world" => "🌏 Creating a Better World",
				"sia_environment" => "♻️ Environment",
				"sia_growth_development" => "🌱 Growth & Development",
				"sia_stem_innovation" => "🔎 STEM & Innovation",

				_ => "❓ Unknown",
			};

			return $"{category} SIA - {projectName} ({approval.SubmissionType})";
		}

		string GetMilestoneDescription(Approval approval)
		{
			var achievement = GetAchievementMeta(approval);

			if (achievement == null)
			{
				return string.Empty;
			}

			return $"👣 Milestone {achievement.Stage}";
		}

		Achievement GetAchievementMeta(Approval approval)
			=> TerrainClient.GetMemberAchievements(approval.MemberId).FirstOrDefault(a => a.Id == approval.AchievementId);

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
