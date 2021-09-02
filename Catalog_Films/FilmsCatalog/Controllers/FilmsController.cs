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
        private readonly ApplicationDbContext _context;

        public FilmsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
        }

        // GET: Films
        public IActionResult Index(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var applicationDbContext = _context.Films.Include(f => f.Creator);
            return View(applicationDbContext);
        }
      
    }
}
