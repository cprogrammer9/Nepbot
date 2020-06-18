/*using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

namespace Test_Managed
{
    /// <summary>
    /// Very simple test for ImgFlip_Managed
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ImgFlip.ImgFlipApi imgFlipApi = ImgFlip.ImgFlipApi.Create(
                ImgFlip.Utilities.MakeSecureStringFromString(ConfigurationManager.AppSettings["ImgFlipLogin"]),
                ImgFlip.Utilities.MakeSecureStringFromString(ConfigurationManager.AppSettings["ImgFlipPassword"]));

            Console.WriteLine();
            Console.WriteLine("Querying memes with \"Cat\" in their name:");
            List<String> memes = imgFlipApi.GetMemeNameMatches("Cat").Result;
            foreach (string meme in memes)
            {
                Console.WriteLine(" => " + meme);
            }

            Console.WriteLine();
            Console.WriteLine("Making a generic Doge meme:");
            string url = imgFlipApi.Generate("doge", "very test", "much work").Result;
            Console.WriteLine(" => " + url);

            Console.WriteLine();
            Process.Start("iexplore.exe", url);
        }
    }
}*/
