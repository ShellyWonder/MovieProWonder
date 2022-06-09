using MovieProWonder.Models.Database;
using MovieProWonder.Models.TMDB;

namespace MovieProWonder.Services.Interfaces
{
    public interface IDataMappingService
    {
        Task<Movie> MapMovieDetailAsync(MovieDetail movie);
        ActorDetail MapActorDetail(ActorDetail actor);
    }
}
