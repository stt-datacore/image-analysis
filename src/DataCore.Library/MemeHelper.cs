using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DataCore.Library
{
    class ImgFlipResult
    {
        public bool success { get; set; }
        public ImgFlipResultData data { get; set; }
    }

    class ImgFlipResultData
    {
        public ImgFlipResultMeme[] memes { get; set; }
    }

    class ImgFlipResultMeme
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int box_count { get; set; }
    }

    class ImgFlipCaptionResult
    {
        public bool success { get; set; }
        public ImgFlipCaptionResultData data { get; set; }
        public string error_message { get; set; }
    }

    class ImgFlipCaptionResultData
    {
        public string url { get; set; }
        public string page_url { get; set; }
    }

    public static class MemeHelper
    {
        private static List<ImgFlipResultMeme> _memeConfig;

        private static List<ImgFlipResultMeme> GetMemeConfig()
        {
            if (_memeConfig == null || _memeConfig.Count == 0)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var content = client.DownloadString($"https://api.imgflip.com/get_memes");
                        var imgFlipResult = JsonConvert.DeserializeObject<ImgFlipResult>(content);
                        if ((imgFlipResult == null) || !imgFlipResult.success)
                        {
                            _memeConfig = null;
                        }

                        _memeConfig = new List<ImgFlipResultMeme>(imgFlipResult.data.memes);
                    }
                }
                catch
                {
                    _memeConfig = null;
                }
            }

            return _memeConfig;
        }

        public static List<string> ListTemplates()
        {
            return new List<string>(GetMemeConfig().Select(meme => meme.name));
        }

        public static string GenerateMeme(string template, string username, string password, string text0, string text1)
        {
            // TODO: Better search perhaps?
            var foundMatching = GetMemeConfig().First(meme => meme.name.IndexOf(template, StringComparison.CurrentCultureIgnoreCase) >= 0);
            if (foundMatching == null)
            {
                return null;
            }

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string postString = $"template_id={foundMatching.id}&username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}&text0={Uri.EscapeDataString(text0)}&text1={Uri.EscapeDataString(text1)}";
                    var content = client.UploadString("https://api.imgflip.com/caption_image", postString);
                    var imgFlipResult = JsonConvert.DeserializeObject<ImgFlipCaptionResult>(content);
                    if ((imgFlipResult == null) || !imgFlipResult.success)
                    {
                        return null;
                    }

                    return imgFlipResult.data.url;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}