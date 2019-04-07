using System;
using System.Collections.Generic;
using System.Text;

namespace SoftJail.DataProcessor.ExportDto
{
    public class PrisonerDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CellNumber { get; set; }

        public ICollection<OfficerDto> Officers { get; set; }

        public decimal TotalOfficerSalary { get; set; }
    }

    public class OfficerDto
    {
        public string OfficerName { get; set; }

        public string Department { get; set; }
    }
}
