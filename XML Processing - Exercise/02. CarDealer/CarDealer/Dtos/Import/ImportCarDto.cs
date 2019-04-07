using System.Collections.Generic;
using System.Xml.Serialization;

namespace CarDealer.Dtos.Import
{
    [XmlType("Car")]
    public class ImportCarDto
    {
        [XmlElement("make")]
        public string Make { get; set; }

        [XmlElement("model")]
        public string Model { get; set; }

        [XmlElement("TraveledDistance")]
        public long TravelledDistance { get; set; }

        [XmlArray("parts")]
        [XmlArrayItem("partId")]
        public ImportIdPartDto[] Parts { get; set; }

        public List<int> PartsId = new List<int>();
    }

    public class ImportIdPartDto
    {
        [XmlAttribute("id")]
        public int PartId { get; set; }
    }
}