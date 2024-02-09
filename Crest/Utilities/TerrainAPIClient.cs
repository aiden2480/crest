using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Crest.Utilities
{
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

		public IEnumerable<JToken> GetPendingApprovals(string unitID)
			=> GetResultsList($"https://achievements.terrain.scouts.com.au/units/{unitID}/submissions?status=pending");

		public IEnumerable<JToken> GetFinalisedApprovals(string unitID)
			=> GetResultsList($"https://achievements.terrain.scouts.com.au/units/{unitID}/submissions?status=finalised");

		public IEnumerable<JToken> GetMemberAchievements(string memberID)
			=> GetResultsList($"https://achievements.terrain.scouts.com.au/members/{memberID}/achievements");

		IEnumerable<JToken> GetResultsList(string url)
		{
			var response = Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)).Result;
			Console.WriteLine(url);

			if (!response.IsSuccessStatusCode)
			{
				return new List<JToken>();
			}

			var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
			if (responseJson.TryGetValue("results", out var results))
			{
				return results.ToObject<IEnumerable<JToken>>();
			}

			return null;
		}
	}
}
