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
    public class ProducersController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext _context;

        private readonly UserManager<User> userManager;

        private readonly IHostingEnvironment hostingEnvironment;

        public ProducersController(ApplicationDbContext context, UserManager<User> userManager, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Films/Create
        [Authorize]
        public IActionResult Create()
        {
            return this.View(new ProducerCreateViewModel());
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ProducerCreateViewModel model)
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
                var _producer = new Producer
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Date_of_birth = model.Date_of_birth,
                    Genres = model.Genres,
                    Number_of_films = model.Number_of_films,
                    Place_of_birth = model.Place_of_birth,
                    Creator = user,
                    CreatorId = user.Id
                };

                var photoPath = Path.Combine(hostingEnvironment.WebRootPath, "attachments", _producer.Id.ToString("N") + fileExt);
                _producer.Path = $"/attachments/{_producer.Id:N}{fileExt}";
                using (var fileStream = new FileStream(photoPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.Photo.CopyToAsync(fileStream);
                }

                _context.Add(_producer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Producers/Details/5
        [Authorize]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await _context.Producers
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }

            return View(producer);
        }

        // GET: Producers/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await this._context.Producers
                .SingleOrDefaultAsync(m => m.Id == id);
            if (producer == null)
            {
                return NotFound();
            }

            var model = new Producer
            {
                Name = producer.Name,
                Surname = producer.Surname,
                Date_of_birth = producer.Date_of_birth,
                Place_of_birth = producer.Place_of_birth,
                Genres = producer.Genres,
                Number_of_films = producer.Number_of_films
            };

            return this.View(model);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ProducerEditViewModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var producer = await this._context.Producers
                .SingleOrDefaultAsync(m => m.Id == id);
            if (producer == null)
            {
                return this.NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.Photo.ContentDisposition).FileName.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!AllowedExtensions.Contains(fileExt))
            {
                ModelState.AddModelError(nameof(model.Photo), "This file type is prohibited");
            }

            if (this.ModelState.IsValid)
            {
                producer.Name = model.Name;
                producer.Surname = model.Surname;
                producer.Genres = model.Genres;
                producer.Date_of_birth = model.Date_of_birth;
                producer.Place_of_birth = model.Place_of_birth;
                producer.Number_of_films = model.Number_of_films;

                if (model.Photo != null)
                {
                    var photoPath = Path.Combine(hostingEnvironment.WebRootPath, "attachments", producer.Id.ToString("N") + fileExt);
                    producer.Path = $"/attachments/{producer.Id:N}{fileExt}";
                    using (var fileStream = new FileStream(photoPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        await model.Photo.CopyToAsync(fileStream);
                    }
                }

                await this._context.SaveChangesAsync();
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

    }
}
