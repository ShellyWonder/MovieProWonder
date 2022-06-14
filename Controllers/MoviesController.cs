using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProWonder.Data;
using MovieProWonder.Models;
using MovieProWonder.Models.Database;
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

        #region Details
        public async Task<IActionResult> Details(int? id, bool local = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie movie = new();
            if (local)
            {
                //Get the Movie data straight from the DB
                movie = await _context.Movie.Include(m => m.Cast)
                                            .Include(m => m.Crew)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                //Get the movie data from the TMDB API
                var movieDetail = await _tmdbMovieService.MovieDetailAsync((int)id);
                movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);
            }

            if (movie == null)
            {
                return NotFound();
            }

            ViewData["Local"] = local;
            return View(movie);

        }

        #endregion

        //CRUD OPERATIONS
        #region GET Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["'CollectionId"] = new SelectList(_context.Collection, "Id", "Name");
            return View();
        }
        #endregion

        #region POSTCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie, int collectionId)
        {
            if (ModelState.IsValid)
            {
                movie.PosterType = movie.PosterFile?.ContentType;
                movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);

                movie.BackdropType = movie.BackdropFile?.ContentType;
                movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);

                _context.Add(movie);
                await _context.SaveChangesAsync();

                await AddToMovieCollection(movie.Id, collectionId);

                return RedirectToAction("Index", "MovieCollections");
            }
            return View(movie);
        }
                #endregion


        #region GET/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }
        #endregion

        #region POST/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (movie.PosterFile is not null)
                    {
                        movie.PosterType = movie.PosterFile.ContentType;
                        movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);
                    }
                    if (movie.BackdropFile is not null)
                    {
                        movie.BackdropType = movie.BackdropFile.ContentType;
                        movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Movies", new { id = movie.Id, local = true });
            }
            return View(movie);
        }
        #endregion

        #region GET/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }
        #endregion

        #region POST/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
           
                _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction("Library", "Movies");
        }
        #endregion

        #region MovieExists
        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
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
