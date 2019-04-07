using System.Collections.Generic;

namespace SoftJail.DataProcessor.ExportDto
{
    public class PrisonersDto
    {
        public ICollection<PrisonerDto> Prisoners { get; set; }
    }
}