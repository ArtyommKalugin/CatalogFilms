using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using FilmsCatalog.Services;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using X.PagedList;

namespace FilmsCatalog.Controllers
{
    public class ActorsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext _context;

        private readonly UserManager<User> userManager;

        private readonly IHostingEnvironment hostingEnvironment;

        public ActorsController(ApplicationDbContext context, UserManager<User> userManager, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Films/Create
        [Authorize]
        public IActionResult Create()
        {
            return this.View(new ActorCreateViewModel());
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ActorCreateViewModel model)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.Photo.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(fileExt))
            {
                ModelState.AddModelError(nameof(model.Photo), "This file type is prohibited");
            }

            if (ModelState.IsValid)
            {
                var _actor = new Actor
                {
                    Name = model.Name,
                    SurName = model.SurName,
                    Date_of_birth = model.Date_of_birth,
                    Number_of_films = model.Number_of_films,
                    Creator = user,
                    CreatorId = user.Id
                };

                var photoPath = Path.Combine(hostingEnvironment.WebRootPath, "attachments", _actor.Id.ToString("N") + fileExt);
                _actor.Path = $"/attachments/{_actor.Id:N}{fileExt}";
                using (var fileStream = new FileStream(photoPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.Photo.CopyToAsync(fileStream);
                }

                _context.Add(_actor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Actors/Details/5
        [Authorize]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

    }
}
