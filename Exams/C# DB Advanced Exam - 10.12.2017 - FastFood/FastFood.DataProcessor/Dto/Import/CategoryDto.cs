using System.ComponentModel.DataAnnotations;

namespace FastFood.DataProcessor.Dto.Import
{
    public class CategoryDto
    {
        [StringLength(30, MinimumLength = 3)]
        public string Name { get; set; }
    }
}