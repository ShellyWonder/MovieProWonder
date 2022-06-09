using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MovieProWonder.Enums;
using MovieProWonder.Models.Settings;
using MovieProWonder.Models.TMDB;
using MovieProWonder.Services.Interfaces;
using System.Runtime.Serialization.Json;

namespace MovieProWonder.Services
{
    public class TMDBMovieService : IRemoteMovieService

    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClient;

        #region constructor
        public TMDBMovieService(IOptions<AppSettings> appSettings, 
                                IHttpClientFactory httpClient)
        {
            _appSettings = appSettings.Value;
            _httpClient = httpClient;
        }
        #endregion

        #region Actor Detail Async
        public async Task<ActorDetail> ActorDetailAsync(int id)
        {
            //Step 1: Setup a default return object
            ActorDetail actorDetail = new();

            //Step 2: Assemble the full request uri string
            var query = $"{_appSettings.TMDBSettings.BaseUrl}/person/{id}";
            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieProSettings.TmDbApiKey },
                { "language", _appSettings.TMDBSettings.QueryOptions.Language}
            };
            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            //Step 3: Create a client and execute the request
            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);

            //Step 4: Return the ActorDetail object
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                var dcjs = new DataContractJsonSerializer(typeof(ActorDetail));
                actorDetail = (ActorDetail)dcjs.ReadObject(responseStream);
            }

            return actorDetail;
        }
        #endregion

        #region Movie Detail Async
        public async Task<MovieDetail> MovieDetailAsync(int id)
        {
            //create new instance of Movie Detail class to return
            MovieDetail movieDetail = new();
            //Assemble the full request uri string
            var query = $"{_appSettings.TMDBSettings.BaseUrl}/movie/{id}";
            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieProSettings.TmDbApiKey },
                { "language", _appSettings.TMDBSettings.QueryOptions.Language},
                { "append to response", _appSettings.TMDBSettings.QueryOptions.AppendToResponse}
            };
            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            //Create a client & execute request
            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            //execute request
            var response = await client.SendAsync(request);
            //return movieDetail object
            if (response.IsSuccessStatusCode)
            {
                //"using" keyword allows for better memory utilization
                using var responseStream = await response.Content.ReadAsStreamAsync();
                //new instance
                var dcjs = new DataContractJsonSerializer(typeof(MovieDetail));
                //read document stream & cast it into type of MovieDetail. Updates movieDetail obj value
                movieDetail = dcjs.ReadObject(responseStream) as MovieDetail;
               
            }

            return movieDetail;
        }
        #endregion

        #region Search Movies Async
        public async Task<MovieSearch> SearchMoviesAsync(MovieCategory category, int count)
        {
          //create new instance of Movie Search class to return
          MovieSearch movieSearch = new();
            //Assemble the full request uri string
            var query = $"{_appSettings.TMDBSettings.BaseUrl}/movie/{category}";
            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieProSettings.TmDbApiKey },
                { "language", _appSettings.TMDBSettings.QueryOptions.Language},
                { "page", _appSettings.TMDBSettings.QueryOptions.Page}
            };
            var requestUri = QueryHelpers.AddQueryString(query, queryParams);

            //Create a client & execute request
            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            //execute request
            var response = await client.SendAsync(request);
            //return MovieSearch object
            if (response.IsSuccessStatusCode)
            {
                //new instance
                var dcjs = new DataContractJsonSerializer(typeof(MovieSearch));
                //"using" keyword allows for better memory utilization
                using var responseStream = await response.Content.ReadAsStreamAsync();
                //read document stream & cast it into type of Movie Search. Updates MovieSearch obj value
                movieSearch = (MovieSearch)dcjs.ReadObject(responseStream);
                //works in count argument (how many results to a page
                movieSearch.results = movieSearch.results.Take(count).ToArray();
                //interating all results to update poster path
                movieSearch.results.ToList().ForEach(r => r.poster_path = $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.MovieProSettings.DefaultPosterSize}/{r.poster_path}");
            }

            return movieSearch;
        }
        #endregion
    }
}
