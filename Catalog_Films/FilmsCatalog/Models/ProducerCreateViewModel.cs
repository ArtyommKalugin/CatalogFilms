using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace FilmsCatalog.Models
{
    public class ProducerCreateViewModel
    {
        [Required(ErrorMessage = "Обязательное поле")]
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Date_of_birth { get; set; }

        public string Place_of_birth { get; set; }

        public string Genres { get; set; }

        public string Number_of_films { get; set; }

        public IFormFile Photo { get; set; }
    }
}