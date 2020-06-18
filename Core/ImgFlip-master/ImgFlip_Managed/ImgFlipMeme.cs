using Newtonsoft.Json;

namespace ImgFlip
{
    /// <summary>
    /// Abstracts a meme. Conveniently serializable to JSON spewed out by ImgFlip API.
    /// </summary>
    internal struct ImgFlipMeme
    {
        /// <summary>
        /// Id of the template. A random unique string.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id;

        /// <summary>
        /// A non-unique name for the template
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name;

        /// <summary>
        /// URL of the base image
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url;

        /// <summary>
        /// Width of the base image
        /// </summary>
        [JsonProperty(PropertyName = "width")]
        public int Width;

        /// <summary>
        /// Height of the base image
        /// </summary>
        [JsonProperty(PropertyName = "height")]
        public int Height;
    }
}
