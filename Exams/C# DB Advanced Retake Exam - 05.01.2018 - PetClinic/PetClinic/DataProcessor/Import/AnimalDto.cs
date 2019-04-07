using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PetClinic.DataProcessor.Import
{
    public class AnimalDto
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Type { get; set; }

        [Required]
        [Range(1, 100)]
        public int Age { get; set; }

        public PassportDto Passport { get; set; }
    }
}