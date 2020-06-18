using Newtonsoft.Json;
using System.Collections.Generic;

namespace ImgFlip
{
    /// <summary>
    /// Abstracts the JSON playload returned by the ImgFlip API on success
    /// </summary>
    internal struct ImgFlipResponseData
    {
        /// <summary>
        /// The memes returned by the get_memes API
        /// </summary>
        [JsonProperty(PropertyName = "memes")]
        public List<ImgFlipMeme> Memes;

        /// <summary>
        /// The url of the created meme for the caption_image API
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url;

        /// <summary>
        /// A page created for the created meme for the caption_image API
        /// </summary>
        [JsonProperty(PropertyName = "page_url")]
        public string PageUrl;
    }
}
