using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieProWonder.Data;
using MovieProWonder.Enums;
using MovieProWonder.Models;
using MovieProWonder.Models.ViewModels;
using MovieProWonder.Services.Interfaces;
using System.Diagnostics;

namespace MovieProWonder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IRemoteMovieService _tmdbMovieService;

        #region constructor
        public HomeController(ILogger<HomeController> logger,
                                ApplicationDbContext context,
                                IRemoteMovieService tmdbMovieService)
        {
            _logger = logger;
            _context = context;
            _tmdbMovieService = tmdbMovieService;
        }
        #endregion
        #region Index
        public async Task<IActionResult> Index()
        {
            const int count = 16;
            var data = new LandingPageVM()
            {
                CustomCollections = await _context.Collection
                                            .Include(c => c.MovieCollections)
                                            .ThenInclude(mc => mc.Movie)
                                            .ToListAsync(),

                NowPlaying = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count),
                Popular = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.popular, count),
                TopRated = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.top_rated, count),
                Upcoming = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.upcoming, count)
            };
            return View(data);
        }
        #endregion
        #region Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}