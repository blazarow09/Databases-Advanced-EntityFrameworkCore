using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ExportDto
{
    [XmlType("Prisoner")]
    public class PrisonerXmlDto
    {

        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("IncarcerationDate")]
        public string IncarcerationDate { get; set; }

        [XmlArray("EncryptedMessages")]
        public MessagesXmlDto[] EncryptedMessages { get; set; }
    }

    [XmlType("Message")]
    public class MessagesXmlDto
    {
        string description;

        [XmlElement("Description")]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                description =  string.Join("" ,value.ToCharArray().Reverse().ToArray());
            }
        }
    }
}
