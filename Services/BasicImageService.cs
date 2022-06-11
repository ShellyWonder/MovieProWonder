using MovieProWonder.Services.Interfaces;

namespace MovieProWonder.Services
{
    public class BasicImageService : IImageService
    {
        private readonly IHttpClientFactory _httpClient;

        #region constructor
        public BasicImageService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }
        #endregion
        #region DecodeImage
        public string DecodeImage(byte[] poster, string contentType)
        {
            if (poster == null) return null;
            var posterImage = Convert.ToBase64String(poster);
            return $"data:{contentType};base64,{posterImage}";
        }
        #endregion
        #region EncodeImageAsync
        public async Task<byte[]> EncodeImageAsync(IFormFile poster)
        {
            if (poster == null) return null;

            using var ms = new MemoryStream();
            await poster.CopyToAsync(ms);
            return ms.ToArray();
        }
        #endregion
        #region EncodeImageUrlAsync
        public  async Task<byte[]> EncodeImageUrlAsync(string imageUrl)
        {
            var client = _httpClient.CreateClient();
            var response = await client.GetAsync(imageUrl);
            using Stream stream = await response.Content.ReadAsStreamAsync();

            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        #endregion
    }
}
