using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Imgur.API;
using NepBot.Resources.Extensions;
using System.IO;
using NepBot.Resources.Database;
using System.DrawingCore;

namespace NepBot.Resources.Code_Implements
{
    public static class ImgurImplement
    {
        static ImgurClient client;
        static OAuth2Endpoint end;
        static IOAuth2Token token;
        static ImageEndpoint endpoint;
        const string gachaAlbum = "d26GVrD";

        /// <summary>
        /// Hopefully make it so I only gotta do these server calls to imgur when I open the bot, rather than per every method call. (it didn't work)
        /// </summary>
        /// <returns></returns>
        public static async Task ImgurDataSetup()
        {
            //await InitiateClient(false);
        }

        private static bool disable = false;

        private static async Task InitiateClient(bool getPin = true)
        {
            if (disable)
                return;
            var data = File.OpenText(Program.DataPath("imgflip pins", "txt")).ReadToEnd().Split('|');
            client = new ImgurClient($"{data[0]}", $"{data[1]}");
            end = new OAuth2Endpoint(client);
            string g = end.GetAuthorizationUrl(Imgur.API.Enums.OAuth2ResponseType.Token);
            //System.Net.WebClient wc = new System.Net.WebClient();
            //byte[] raw = wc.DownloadData(g);

            //string webData = System.Text.Encoding.UTF8.GetString(raw);
            //Console.WriteLine(g);
            //ExtensionMethods.WriteToLog(ExtensionMethods.LogType.CustomMessage, null, g + "\n\n\n" + webData);
            if (getPin)
            {
                token = await end.GetTokenByPinAsync($"{data[2]}");
                client.SetOAuth2Token(token);
            }
            endpoint = new ImageEndpoint(client);
            disable = true;
        }

        /// <summary>
        /// Used to find the URL of an imgur image from my album.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GrabURLCode(string url)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 20; i < 27; i++)
            {
                sb.Append(url[i]);
            }
            return sb.ToString();
        }

        public static async Task<string> UploadImage(CardTypes ct)
        {
            try
            {
                await InitiateClient();
                IImage image;
                using (var fs = new FileStream(ct.path, FileMode.Open))
                {
                    fs.Position = 0;
                    image = await endpoint.UploadImageStreamAsync(fs, gachaAlbum);
                }
                return GrabURLCode(image.Link);
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, "From Upload Image: " + ct.path);
            }
            return null;
        }

        /// <summary>
        /// Gets image from its ID assigned.
        /// </summary>
        /// <param name="imageID"></param>
        /// <returns></returns>
        public static async Task<string> GetImage(string imageID)
        {
            try
            {
                await InitiateClient(false);
                disable = false;
                var image = await endpoint.GetImageAsync(imageID);
                return image.Link;
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, $"From Get Image: {imageID}");
                throw;
            }
        }
    }
}