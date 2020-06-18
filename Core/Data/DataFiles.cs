using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ImgFlip;
using System.Net.Http;
using System.Drawing;
using ConvenienceMethods;
using Discord.WebSocket;
using System.Linq;
using NepBot.Resources.Database;
using NepBot.Data;
using System.Reflection;
using Passive.Services.DatabaseService;
using System.Text;
using NepBot.Resources.Games;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Extensions;

namespace NepBot.Core.Data
{
    public static class DataFiles
    {
        public static List<string> imageNames = new List<string>();
        public static List<string> imagePaths = new List<string>();
        public static List<string> imageURLs = new List<string>();
        public static List<Image> images = new List<Image>();

        /// <summary>
        /// Loads memes locally from hard drive to be used.
        /// </summary>
        public static void LoadMemeDirectory()
        {
            string[] files = Directory.GetFiles(Assembly.GetEntryAssembly().Location.Replace(@"bin\Debug\NepBot.exe", $@"Data\Meme Base Images"), "*.*", SearchOption.TopDirectoryOnly);
            

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo f = new FileInfo(files[i]);
                imagePaths.Add(files[i]);
                imageNames.Add(Path.GetFileNameWithoutExtension(files[i]).ToLower());
                images.Add(Image.FromFile(files[i]));
            }
        }

        /// <summary>
        /// Gets the meme image stored locally by its file name and returns it as a bitmap.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Bitmap ReturnImage(string image)
        {
            image = image.ToLower();
            for (int i = 0; i < imageNames.Count; i++)
            {
                if (imageNames[i] == image)
                {
                    return new Bitmap(images[i]);
                }
            }
            return null;
        }
    }
}
