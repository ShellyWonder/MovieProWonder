using MovieProWonder.Models.Database;
using MovieProWonder.Models.TMDB;
using System.Collections.ObjectModel;

namespace MovieProWonder.Models.ViewModels
{
    public class LandingPageVM
    {
        public List<Collection>CustomCollections { get; set; }
        public MovieSearch NowPlaying { get; set; }
        public MovieSearch Popular { get; set; }
        public MovieSearch TopRated { get; set; }
        public MovieSearch Upcoming { get; set; }
    }
}
