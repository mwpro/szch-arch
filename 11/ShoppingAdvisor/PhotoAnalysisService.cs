using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ShoppingAdvisor
{
    public class PhotoAnalysisService
    {
        private readonly IComputerVisionClient _computerVisionClient;

        public PhotoAnalysisService(IComputerVisionClient computerVisionClient)
        {
            _computerVisionClient = computerVisionClient;
        }

        public async Task<ImageAnalysis> AnalyzePhoto(string imageUrl)
        {
            var uri = new Uri(imageUrl);
            if (PhotoNeedsToBeDownloaded(uri))
            {
                return await DownloadAndAnalyzePhoto(uri);
            }
            else
            {
                var analysis = await _computerVisionClient.AnalyzeImageAsync(
                    imageUrl, AnalysisFeatures);
                return analysis;
            }
        }

        private static bool PhotoNeedsToBeDownloaded(Uri uri) // we can't give a link to localhost
        {
            return uri.Host.Equals("localhost");
        }

        private async Task<ImageAnalysis> DownloadAndAnalyzePhoto(Uri uri)
        {
            var localFileName = Path.GetTempFileName();
            using (var wc = new WebClient())
            {
                wc.DownloadFile(uri, localFileName);
            }

            var fileInfo = new FileInfo(localFileName);
            if (fileInfo.Length > 4000000) // we need to resize too big photos
            {
                using (var image = new Bitmap(localFileName))
                {
                    var resized = new Bitmap(image.Width / 2, image.Height / 2);
                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.DrawImage(image, 0, 0, image.Width / 2, image.Height / 2);
                        image.Dispose();
                        resized.Save(localFileName, ImageFormat.Jpeg);
                    }
                }
            }

            using (var imageStream = File.OpenRead(localFileName))
            {
                var analysis = await _computerVisionClient.AnalyzeImageInStreamAsync(
                    imageStream, AnalysisFeatures);
                File.Delete(localFileName);
                return analysis;
            }
        }

        private static readonly List<VisualFeatureTypes> AnalysisFeatures =
            new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Tags
            };
    }
}
