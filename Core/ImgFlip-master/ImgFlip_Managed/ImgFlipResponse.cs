using Newtonsoft.Json;

namespace ImgFlip
{
    /// <summary>
    /// Response from ImgFlip when generating meme
    /// </summary>
    internal struct ImgFlipResponse
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        [JsonProperty(PropertyName = "success")]
        public bool Success;

        /// <summary>
        /// If successful, contains the payload
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public ImgFlipResponseData Data;

        /// <summary>
        /// On failure, an error message reported by the API
        /// </summary>
        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage;
    }
}
