using System.Xml.Serialization;

namespace PetClinic.DataProcessor.Import
{
    [XmlType("Procedure")]
    public class ProcedureDto
    {
        [XmlElement("Vet")]
        public string VetName { get; set; }

        [XmlElement("Animal")]
        public string AnimalSerial { get; set; }

        [XmlElement("DateTime")]
        public string DateTime { get; set; }

        [XmlArray("AnimalAids")]
        public AnimalAidXmlDto[] AnimalAids { get; set; }
    }

    [XmlType("AnimalAid")]
    public class AnimalAidXmlDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }
    }
}