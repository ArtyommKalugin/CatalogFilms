using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace FilmsCatalog.Models
{
    public class ActorEditViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }

        public string SurName { get; set; }

        public Int32 Height { get; set; }

        public DateTime Date_of_birth { get; set; }

        public string Number_of_films { get; set; }

        public IFormFile Photo { get; set; }
    }
}