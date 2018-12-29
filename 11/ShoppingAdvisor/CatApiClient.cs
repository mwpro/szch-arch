using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;

namespace ShoppingAdvisor
{
    public class CatApiClient
    {
        private readonly string _catApiKey;
        private const string CatApiUrl = "https://api.thecatapi.com";

        public CatApiClient(string catApiKey)
        {
            _catApiKey = catApiKey;
        }

        private HttpClient HttpClientFactory()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(CatApiUrl),
                DefaultRequestHeaders =
                {
                    { "x-api-key", _catApiKey }
                }
            };
        }

        public async Task<(string content, string contentType)> GetRandomCatPhotoAsBase64()
        {
            var randomCatImage = await GetRandomCatImage();
            var localFileName = Path.GetTempFileName();
            using (var wc = new WebClient())
            {
                wc.DownloadFile(randomCatImage.Url, localFileName);
            }

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(randomCatImage.Url, out var contentType))
            {
                contentType = "image/jpeg";
            }
            var imageAsBase64 = Convert.ToBase64String(File.ReadAllBytes(localFileName));

            File.Delete(localFileName);
            return (imageAsBase64, contentType);
        }

        private async Task<CatImage> GetRandomCatImage()
        {
            using (var httpClient = HttpClientFactory())
            {
                var response = await httpClient.GetAsync("/v1/images/search?mime_types=jpg,png");
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Could not get a random cat photo, status {response.StatusCode}");

                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CatImage>>(responseString).First();
            }
        }

        private class CatImage
        {
            public List<string> Breeds { get; set; }
            public string Id { get; set; }
            public string Url { get; set; }
        }
    }
}
