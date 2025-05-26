using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MindTrack
{
    public static class LmStudioService
    {
        public static async Task<string> SendMessageAsync(string message, string endpoint)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var payload = new
                    {
                        messages = new[] { new { role = "user", content = message } },
                        max_tokens = 256
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    var response = await client.PostAsync(endpoint, new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json"));
                    if (!response.IsSuccessStatusCode)
                        return "[AI Error] Service unreachable.";
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    return obj.choices[0].message.content.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"[AI Error] {ex.Message}";
            }
        }
    }
}