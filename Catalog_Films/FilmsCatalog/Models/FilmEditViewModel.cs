using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace FilmsCatalog.Models
{
    public class FilmEditViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Year { get; set; }

        public string Producer { get; set; }

        public IFormFile Poster { get; set; }
    }
}