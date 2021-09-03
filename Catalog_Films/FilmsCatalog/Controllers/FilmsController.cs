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

namespace FilmsCatalog.Controllers
{
    public class FilmsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext _context;

        private readonly UserManager<User> userManager;

        private readonly IHostingEnvironment hostingEnvironment;

        public FilmsController(ApplicationDbContext context, UserManager<User> userManager, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Films
        public IActionResult Index(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var applicationDbContext = _context.Films.Include(f => f.Creator);
            return View(applicationDbContext);
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

    }
}
