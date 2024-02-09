using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Crest.Utilities
{
	public class JandiAPIClient
	{
		public static bool IsValidIncomingWebhookURL(string url)
		{
			var response = SendMessage(url, new JandiMessage());
			var responseBody = response.Content.ReadAsStringAsync().Result;

			return response.StatusCode == HttpStatusCode.BadRequest &&
				responseBody == "{\"code\":40052,\"msg\":\"Invalid payload - body\"}";
		}

		public static HttpResponseMessage SendMessage(string url, JandiMessage message)
		{
			var client = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(SerialiseJandiMessage(message), Encoding.UTF8, "application/json")
			};

			request.Headers.Add("Accept", "application/vnd.tosslab.jandi-v2+json");
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			return client.Send(request);
		}

		private static string SerialiseJandiMessage(JandiMessage message)
		{
			return JsonConvert.SerializeObject(message, new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			});
		}
	}

	public class JandiMessage
	{
		public string Body { get; set; }

		public string ConnectColor { get; set; }

		public List<JandiConnect> ConnectInfo = new();
	}

	public class JandiConnect
	{
		public string Title { get; set; }

		public string Description { get; set; }
	}
}
