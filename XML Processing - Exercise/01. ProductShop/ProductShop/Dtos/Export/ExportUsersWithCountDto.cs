using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    public class ExportUsersWithCountDto
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("users")]
        public ExportUsersProductsDto[] Users { get; set; }
    }
}