using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Crest.Utilities
{
	public static class TerrainAPIClient
	{
		public static HttpResponseMessage Login(string username, string password)
		{
			var client = new HttpClient();
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

			return client.Send(request);
		}
	}
}
