using System.Xml.Serialization;

namespace CarDealer.Dtos.Import
{
    [XmlType("PartCars")]
    public class ImportPartCarDto
    {
        [XmlElement("partId")]
        public int PartId { get; set; }

        [XmlElement("carId")]
        public int CarId { get; set; }
    }
}