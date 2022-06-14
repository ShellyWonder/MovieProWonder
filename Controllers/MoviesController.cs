using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProWonder.Data;
using MovieProWonder.Models;
using MovieProWonder.Models.Settings;
using MovieProWonder.Services.Interfaces;

namespace MovieProWonder.Controllers
{

    public class MoviesController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _tmdbMappingService;

        #region constructor
        public MoviesController(IOptions<AppSettings> appSettings,
                                ApplicationDbContext context,
                                IImageService imageService,
                                IRemoteMovieService tmdbMovieService,
                                IDataMappingService tmdbMappingService)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _imageService = imageService;
            _tmdbMovieService = tmdbMovieService;
            _tmdbMappingService = tmdbMappingService;
        }
        #endregion
        #region Get Import
        [HttpGet]
        public async Task<IActionResult> Import()
        {
            var movies = await _context.Movie.ToListAsync();
            return View(movies);
        }
        #endregion

        #region HttpPost Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id)
        {
            //if movie id already exists in db:
            if (_context.Movie.Any(m => m.MovieId == id))
            {
                var localMovie = await _context.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
                return RedirectToAction("Details", "Movies", new { id = localMovie.Id, local = true });
            }

            //if not, get raw data from API:
            var MovieDetail = await _tmdbMovieService.MovieDetailAsync(id);

            //run data through mapping procedure
            var movie = await _tmdbMappingService.MapMovieDetailAsync(MovieDetail);

            //add new movie:
            _context.Add(movie);
            await _context.SaveChangesAsync();

            //add new movie to default "all collection"
            await AddToMovieCollection(movie.Id, _appSettings.MovieProSettings.DefaultCollection.Name);



            return RedirectToAction("Import");
        }
        #endregion

        #region Library

        public async Task<IActionResult> Library()
        {
            //grabs all movies previously stored in the database
            var movies = await _context.Movie.ToListAsync();
            return View(movies);
        }
        #endregion


        #region private Add to movie collection
        private async Task AddToMovieCollection(int movieId, string collectionName)
        {
            var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);
            _context.Add(
               new MovieCollection()
               {
                   CollectionId = collection.Id,
                   MovieId = movieId
               }
               );
            await _context.SaveChangesAsync();
        }
        #endregion

        #region overload private Add to movie collection
        private async Task AddToMovieCollection(int movieId, int collectionId)
        {
            _context.Add(
              new MovieCollection()
              {
                  CollectionId = collectionId,
                  MovieId = movieId
              }
              );
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
