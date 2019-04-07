namespace SoftJail.DataProcessor
{
    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
            .Where(x => ids.Contains(x.Id))
            .Select(x => new PrisonerDto()
            {
                Id = x.Id,
                Name = x.FullName,
                CellNumber = x.Cell.CellNumber,
                Officers = x.PrisonerOfficers
                    .Select(y => new OfficerDto()
                    {
                        OfficerName = y.Officer.FullName,
                        Department = y.Officer.Department.Name
                    })
                    .OrderBy(p => p.OfficerName)
                    .ToArray(),
                TotalOfficerSalary = decimal.Parse(x.PrisonerOfficers.Sum(y => y.Officer.Salary).ToString("f2"))
            })
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToArray();

            var result = JsonConvert.SerializeObject(prisoners);

            return result;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var tokens = prisonersNames.Split(",").ToArray();

            var prisoners = context.Prisoners
                .Where(x => tokens.Contains(x.FullName))
                .Select(x => new PrisonerXmlDto()
                {
                    Id = x.Id,
                    Name = x.FullName,
                    IncarcerationDate = x.IncarcerationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EncryptedMessages = x.Mails.Select(y => new MessagesXmlDto()
                    { 
                        Description = y.Description
                    })
                    .ToArray()
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToArray();
            
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PrisonerXmlDto[]), new XmlRootAttribute("Prisoners"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("", "")
            });

            xmlSerializer.Serialize(new StringWriter(sb), prisoners, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}