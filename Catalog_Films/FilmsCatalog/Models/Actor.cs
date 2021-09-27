using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class Actor
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string CreatorId { get; set; }

        public User Creator { get; set; }

        public string Name { get; set; }

        public string SurName { get; set; }

        public Int32 Height { get; set; }

        public DateTime Date_of_birth { get; set; }

        public string Number_of_films { get; set; }

        public string Path { get; set; }
    }
}
