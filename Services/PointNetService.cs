using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinFormsApp1.Services
{
    public class PointNetResult
    {
        [JsonProperty("prediction")]
        public string Prediction { get; set; }

        [JsonProperty("is_deboite")]
        public bool IsDeboite { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("prob_normal")]
        public double ProbNormal { get; set; }

        [JsonProperty("prob_deboite")]
        public double ProbDeboite { get; set; }
    }

    public class PointNetService
    {
        private readonly HttpClient client;
        private readonly string apiUrl;

        public PointNetService(string apiUrl = "http://localhost:5000")
        {
            this.apiUrl = apiUrl;
            this.client = new HttpClient();
            this.client.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> IsAvailable()
        {
            try
            {
                var response = await client.GetAsync($"{apiUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PointNetResult> PredictAsync(string csvPath)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"Fichier CSV introuvable: {csvPath}");

            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(File.ReadAllBytes(csvPath));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
                content.Add(fileContent, "file", Path.GetFileName(csvPath));

                var response = await client.PostAsync($"{apiUrl}/predict", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PointNetResult>(json);

                return result;
            }
        }
    }
}