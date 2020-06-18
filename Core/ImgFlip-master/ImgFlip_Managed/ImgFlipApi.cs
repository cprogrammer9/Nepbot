using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace ImgFlip
{
    public class ImgFlipApi
    {
        /// <summary>
        /// Disabled C-tor
        /// </summary>
        private ImgFlipApi()
        {
        }

        /// <summary>
        /// API username
        /// </summary>
        private SecureString _username;

        /// <summary>
        /// API password
        /// </summary>
        private SecureString _password;

        /// <summary>
        /// Meme id cache
        /// </summary>
        MemeIdCache _memeIdCache = null;



        /// <summary>
        /// Ensures the cache is populated
        /// </summary>
        private async Task EnsureMemeIdCache()
        {
            if (_memeIdCache == null)
            {
                _memeIdCache = new MemeIdCache();

                ImgFlipResponse flipResponse = await CallImgFlipApi("https://api.imgflip.com/get_memes");
                if (!flipResponse.Success)
                {
                    throw new InvalidOperationException(flipResponse.ErrorMessage);
                }

                _memeIdCache.Refresh(flipResponse.Data.Memes);
            }
        }

        /// <summary>
        /// Creates a new isntance of the ImgFlip meme generator
        /// </summary>
        /// <param name="username">API username</param>
        /// <param name="password">API password</param>
        public static ImgFlipApi Create(SecureString username, SecureString password)
        {
            ImgFlipApi generator = new ImgFlipApi()
            {
                _username = username,
                _password = password
            };

            return generator;
        }

        /// <summary>
        /// Generates the query string for meme creation
        /// </summary>
        /// <param name="templateId">ID of the meme template</param>
        /// <param name="firstLine">top line</param>
        /// <param name="secondLine">bottom line</param>
        /// <returns>the url of the correspinding meme</returns>
        private string GenerateQueryString(string templateId, string firstLine, string secondLine, string thirdLine = "")
        {
            string queryTemplate = (thirdLine != "") ? "https://api.imgflip.com/caption_image?template_id={0}&text0={1}&text1={2}&text3={3}&username={4}&password={5}"
                : "https://api.imgflip.com/caption_image?template_id={0}&text0={1}&text1={2}&username={3}&password={4}";


            if (thirdLine == "")
                return string.Format(
                    queryTemplate,
                    templateId,
                    firstLine,
                    secondLine,
                    Utilities.MakeStringFromSecureString(_username),
                    Utilities.MakeStringFromSecureString(_password));
            return string.Format(
                    queryTemplate,
                    templateId,
                    firstLine,
                    secondLine,
                    thirdLine,
                    Utilities.MakeStringFromSecureString(_username),
                    Utilities.MakeStringFromSecureString(_password));
        }

        /// <summary>
        /// Asynchronously calls an ImgFlip API and returns the response
        /// </summary>
        /// <param name="queryString">query string to send</param>
        /// <param name="retries">number of times to retry on error</param>
        /// <returns>response from ImgFlip</returns>
        private async Task<ImgFlipResponse> CallImgFlipApi(string queryString, int retries = 3)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(queryString);
            WebResponse response = null;
            string payload = string.Empty;
            bool retry = retries > 0;

            try
            {
                await Task.Run(
                    () =>
                    {
                        response = request.GetResponse();
                    });


                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    payload = await reader.ReadToEndAsync();
                }

                retry = false;
            }
            catch
            {
                // Eat the exception on retry
                if (!retry)
                {
                    throw;
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
            }

            ImgFlipResponse flipResponse;
            if (retry)
            {
                flipResponse = await CallImgFlipApi(queryString, retries - 1);
            }
            else
            {
                flipResponse = JsonConvert.DeserializeObject<ImgFlipResponse>(payload);
            }

            return flipResponse;
        }

        /// <summary>
        /// Gets the list of memes that match a certain name / pattern
        /// </summary>
        /// <param name="memeName">name of the meme</param>
        /// <returns></returns>
        public async Task<List<string>> GetMemeNameMatches(string memeName)
        {
            await EnsureMemeIdCache();

            return _memeIdCache.GetMemeNameMatches(memeName);
        }

        public async Task<string> Testicles(string input)
        {
            await EnsureMemeIdCache();
            return _memeIdCache.GetMemeIdFromName(input);
        }
        /// <summary>
        /// Asynchronously generates a meme with the characteristics passed in
        /// </summary>
        /// <param name="memeName">name of the meme</param>
        /// <param name="firstLine">top line</param>
        /// <param name="secondLine">bottom line</param>
        /// <returns>the url of the corresponding meme</returns>
        public async Task<string> Generate(string memeName, string firstLine, string secondLine, string thirdLine = "")
        {
            await EnsureMemeIdCache();
            string memeId = _memeIdCache.GetMemeIdFromName(memeName);

            if (string.IsNullOrEmpty(firstLine))
            {
                firstLine = " ";
            }

            if (string.IsNullOrEmpty(secondLine))
            {
                secondLine = " ";
            }

            string queryString = (thirdLine == "") ? GenerateQueryString(memeId, firstLine, secondLine) : GenerateQueryString(memeId, firstLine, secondLine, thirdLine);
            ImgFlipResponse flipResponse = await CallImgFlipApi(queryString);

            if (!flipResponse.Success)
            {
                return flipResponse.ErrorMessage;
            }

            return flipResponse.Data.Url;
        }

        /// <summary>
        /// Abstracts a caching mechanism for the id vs name for memes
        /// </summary>
        internal class MemeIdCache
        {
            /// <summary>
            /// The memes
            /// </summary>
            private List<ImgFlipMeme> _memes = new List<ImgFlipMeme>();

            /// <summary>
            /// Refreshes the cache
            /// </summary>
            /// <param name="memes">new memes to cache</param>
            public void Refresh(List<ImgFlipMeme> memes)
            {
                _memes.Clear();
                _memes.AddRange(memes);
            }

            /// <summary>
            /// Gets the list of memes that match a certain name / pattern
            /// </summary>
            /// <param name="name">name of the meme</param>
            /// <returns></returns>
            public List<string> GetMemeNameMatches(string name)
            {
                name = name.ToLowerInvariant();
                List<ImgFlipMeme> matches = _memes.FindAll(
                    meme =>
                    {
                        return meme.Name.ToLowerInvariant().Contains(name);
                    });

                return matches.Select(meme => meme.Name).ToList();
            }


            /// <summary>
            /// Gets the meme id corresponding to the name. Makes increasingly forgiving searches for the meme based on the name.
            /// </summary>
            /// <param name="name">name of the meme</param>
            /// <returns>a template id</returns>
            public string GetMemeIdFromName(string name)
            {
                string id = string.Empty;
                name = name.ToLowerInvariant();

                // First try case insensitive search
                try
                {
                    ImgFlipMeme match = _memes.First(
                        meme =>
                        {
                            return string.Equals(meme.Name, name, StringComparison.OrdinalIgnoreCase);
                        });

                    id = match.Id;
                }
                catch (InvalidOperationException)
                {
                    // no match
                }

                // Next just try containment
                try
                {
                    ImgFlipMeme match = _memes.First(
                        meme =>
                        {
                            return meme.Name.ToLowerInvariant().Contains(name);
                        });

                    id = match.Id;
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException("Holy nepperoni I can't find any memes with that name! Ohh the horror!");
                }

                return id;
            }
        }
    }
}
