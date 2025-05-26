using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MindTrack
{
    public static class TranslateService
    {
        private static readonly string endpoint = "http://127.0.0.1:5000/translate";
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> TranslateAsync(string text, string sourceLang, string targetLang)
        {
            try
            {
                var payload = new JObject
                {
                    ["q"] = text,
                    ["source"] = sourceLang,
                    ["target"] = targetLang,
                    ["format"] = "text"
                };

                var content = new StringContent(
                    payload.ToString(),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(endpoint, content);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(json);
                    return result["translatedText"]?.ToString() ?? string.Empty;
                }
                else
                {
                    throw new Exception($"Translation failed: {json}");
                }
            }
            catch (Exception ex)
            {
                return $"[Çeviri Hatası] {ex.Message}";
            }
        }

        // Overloaded methods for backward compatibility
        public static async Task<string> TranslateToEnglishAsync(string text, string endpointUrl = null)
        {
            return await TranslateAsync(text, "tr", "en");
        }

        public static async Task<string> TranslateToTurkishAsync(string text, string endpointUrl = null)
        {
            return await TranslateAsync(text, "en", "tr");
        }

        // Simple methods without endpoint parameter
        public static async Task<string> TranslateToEnglishAsync(string text)
        {
            return await TranslateAsync(text, "tr", "en");
        }

        public static async Task<string> TranslateToTurkishAsync(string text)
        {
            return await TranslateAsync(text, "en", "tr");
        }
    }
}