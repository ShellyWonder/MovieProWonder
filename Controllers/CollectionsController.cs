using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProWonder.Data;
using MovieProWonder.Models.Database;
using MovieProWonder.Models.Settings;

namespace MovieProWonder.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AppSettings _appSettings;

        #region constructor
        public CollectionsController(ApplicationDbContext context,
                                        IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        #endregion

        #region get collections
        // GET: Collections
        public async Task<IActionResult> Index()
        {
            //defaultCollectionName represents "All"
            var defaultCollectionName = _appSettings.MovieProSettings.DefaultCollection.Name;
            var collections = await _context.Collection
                                .Where(c => c.Name!= defaultCollectionName)
                                .ToListAsync();
            return View(collections);
        }
        #endregion
 
        #region Post Create
        // POST: Collections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Collection collection)
        {
            if (ModelState.IsValid)
            {
                _context.Add(collection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(collection);
        }
        #endregion

        #region Get Edit
        // GET: Collections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Collection == null)
            {
                return NotFound();
            }

            var collection = await _context.Collection.FindAsync(id);
            if (collection == null)
            {
                return NotFound();
            }
            return View(collection);
        }
        #endregion

        #region POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Collection collection)
        {
            if (id != collection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollectionExists(collection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(collection);
        }
        #endregion

        #region GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Collection == null)
            {
                return NotFound();
            }

            var collection = await _context.Collection
                .FirstOrDefaultAsync(m => m.Id == id);
            if (collection == null)
            {
                return NotFound();
            }

            return View(collection);
        }
        #endregion

        #region POST Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Collection == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Collection'  is null.");
            }
            var collection = await _context.Collection.FindAsync(id);
            if (collection != null)
            {
                _context.Collection.Remove(collection);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region private CollectionExits
        private bool CollectionExists(int id)
        {
          return (_context.Collection?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        #endregion
    }
}
