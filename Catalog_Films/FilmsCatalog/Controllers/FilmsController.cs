using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using FilmsCatalog.Services;

namespace FilmsCatalog.Controllers
{
    public class FilmsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext _context;

        private readonly UserManager<User> userManager;

        private readonly IHostingEnvironment hostingEnvironment;

        private readonly IUserPermissionsService userPermissions;

        public FilmsController(ApplicationDbContext context, UserManager<User> userManager, IUserPermissionsService userPermissions, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Films
        public IActionResult Index(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var applicationDbContext = _context.Films.Include(f => f.Creator);
            return View(applicationDbContext.ToPagedList(pageNumber, pageSize));
        }

        // GET: Films/Create
        [Authorize]
        public IActionResult Create()
        {
            return this.View(new FilmCreateViewModel());
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(FilmCreateViewModel model)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.Poster.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(fileExt))
            {
                ModelState.AddModelError(nameof(model.Poster), "This file type is prohibited");
            }

            if (ModelState.IsValid)
            {
                var _film = new Film
                {
                    Name = model.Name,
                    Description = model.Description,
                    Year = model.Year,
                    Producer = model.Producer,
                    Creator = user,
                    CreatorId = user.Id
                };

                var posterPath = Path.Combine(hostingEnvironment.WebRootPath, "attachments", _film.Id.ToString("N") + fileExt);
                _film.Path = $"/attachments/{_film.Id:N}{fileExt}";
                using (var fileStream = new FileStream(posterPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.Poster.CopyToAsync(fileStream);
                }

                _context.Add(_film);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Films/Details/5
        [Authorize]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            if (userPermissions.CanEditFilm(film))
            {
                ViewBag.permission = true;
            }
            else ViewBag.permission = false;

            return View(film);
        }

        // GET: Films/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await this._context.Films
                .SingleOrDefaultAsync(m => m.Id == id);
            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return NotFound();
            }

            var model = new FilmEditViewModel
            {
                Name = film.Name,
                Description = film.Description,
                Year = film.Year,
                Producer = film.Producer
            };

            return this.View(model);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, FilmEditViewModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this._context.Films
                .SingleOrDefaultAsync(m => m.Id == id);
            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.Poster.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(fileExt))
            {
                ModelState.AddModelError(nameof(model.Poster), "This file type is prohibited");
            }

            if (this.ModelState.IsValid)
            {
                film.Name = model.Name;
                film.Description = model.Description;
                film.Year = model.Year;
                film.Producer = model.Producer;

                if (model.Poster != null)
                {
                    var posterPath = Path.Combine(hostingEnvironment.WebRootPath, "attachments", film.Id.ToString("N") + fileExt);
                    film.Path = $"/attachments/{film.Id:N}{fileExt}";
                    using (var fileStream = new FileStream(posterPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        await model.Poster.CopyToAsync(fileStream);
                    }
                }

                await this._context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        // GET: Films/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return NotFound();
            }

            return View(film);
        }

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var film = await this._context.Films
                .SingleOrDefaultAsync(m => m.Id == id);

            if (film == null || !this.userPermissions.CanEditFilm(film))
            {
                return this.NotFound();
            }

            var posterPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", film.Id.ToString("N") + Path.GetExtension(film.Path));
            System.IO.File.Delete(posterPath);
            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
