using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Crest.Utilities;

public class TerrainAPIClient
{
	readonly HttpClient Client;

	public TerrainAPIClient()
	{
		Client = new HttpClient();
	}

	public bool Login(string username, string password, out string loginFailureReason)
	{
		var url = "https://cognito-idp.ap-southeast-2.amazonaws.com";

		var body = new
		{
			ClientId = "6v98tbc09aqfvh52fml3usas3c",
			AuthFlow = "USER_PASSWORD_AUTH",
			AuthParameters = new
			{
				USERNAME = username,
				PASSWORD = password
			},
		};

		using var request = new HttpRequestMessage(HttpMethod.Post, url);

		request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
		request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-amz-json-1.1");
		request.Content.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");

		var response = Client.Send(request);
		var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
		
		if (responseJson.TryGetValue("AuthenticationResult", out var authTokenResult))
		{
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authTokenResult["IdToken"].ToString());

			loginFailureReason = null;

			return true;
		}

		if (responseJson.TryGetValue("__type", out var type) && responseJson.TryGetValue("message", out var message))
		{
			loginFailureReason = $"{type}: {message}";
			return false;
		}

		loginFailureReason = "An unknown error occurred: " + responseJson.ToString();
		return false;
	}

	public IEnumerable<Approval> GetPendingApprovals(string unitID)
		=> GetResultsList<Approval>($"https://achievements.terrain.scouts.com.au/units/{unitID}/submissions?status=pending");

	public IEnumerable<Approval> GetFinalisedApprovals(string unitID)
		=> GetResultsList<Approval>($"https://achievements.terrain.scouts.com.au/units/{unitID}/submissions?status=finalised");

	public IEnumerable<Achievement> GetMemberAchievements(string memberID)
		=> GetResultsList<Achievement>($"https://achievements.terrain.scouts.com.au/members/{memberID}/achievements");

	IEnumerable<T> GetResultsList<T>(string url)
	{
		var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)).Result;

		if (!response.IsSuccessStatusCode)
		{
			return new List<T>();
		}

		var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);

		if (responseJson.TryGetValue("results", out var results))
		{
			var tokens = results.ToObject<IEnumerable<JToken>>();

			return tokens.Select(token => (T) Activator.CreateInstance(typeof(T), token));
		}

		return new List<T>();
	}
}

public class Approval
{
	public readonly string MemberFirstName;

	public readonly string MemberLastName;

	public readonly string MemberId;

	public readonly string SubmissionOutcome;

	public readonly string SubmissionType;

	public readonly DateTime SubmissionDate;

	public readonly string AchievementType;

	public readonly string AchievementId;

	public Approval(JToken jToken)
	{
		MemberFirstName = jToken["member"]["first_name"].ToString();
		MemberLastName = jToken["member"]["last_name"].ToString();
		MemberId = jToken["member"]["id"].ToString();

		// SubmissionOutcome will be empty if this approval is still pending
		SubmissionOutcome = jToken["submission"]["outcome"]?.ToString();
		SubmissionType = jToken["submission"]["type"].ToString();
		SubmissionDate = DateTime.ParseExact(jToken["submission"]["date"].ToString(), "dd-MMM-yyyy h:mm:ss tt", null);
		
		AchievementType = jToken["achievement"]["type"].ToString();
		AchievementId = jToken["achievement"]["id"].ToString();
	}
}

public class Achievement
{
	public readonly string Id;

	public readonly string Branch;

	public readonly string Stream;

	public readonly string Stage;

	public readonly string SIAProjectName;

	public readonly string SIASelection;

	public Achievement(JToken jToken)
	{
		Id = jToken["id"].ToString();

		// All three for OAS, stage only for milestone
		Branch = jToken["achievement_meta"]?["branch"]?.ToString();
		Stream = jToken["achievement_meta"]?["stream"]?.ToString();
		Stage = jToken["achievement_meta"]?["stage"]?.ToString();

		// Only used for SIA
		SIAProjectName = jToken["answers"]?["project_name"]?.ToString();
		SIASelection = jToken["answers"]?["special_interest_area_selection"]?.ToString();
	}
}
