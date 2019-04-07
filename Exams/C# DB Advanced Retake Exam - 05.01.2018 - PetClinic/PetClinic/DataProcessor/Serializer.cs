namespace PetClinic.DataProcessor
{
    using Newtonsoft.Json;
    using PetClinic.Data;
    using PetClinic.DataProcessor.Export;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportAnimalsByOwnerPhoneNumber(PetClinicContext context, string phoneNumber)
        {
            var animalsOwners = context.Animals
                    .Where(x => x.Passport.OwnerPhoneNumber == phoneNumber)
                    .Select(x => new
                    {
                        OwnerName = x.Passport.OwnerName,
                        AnimalName = x.Name,
                        Age = x.Age,
                        SerialNumber = x.PassportSerialNumber,
                        RegisteredOn = x.Passport.RegistrationDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
                    })
                    .OrderBy(x => x.Age)
                    .OrderBy(x => x.SerialNumber)
                    .ToArray();

            var result = JsonConvert.SerializeObject(animalsOwners);

            return result;
        }

        public static string ExportAllProcedures(PetClinicContext context)
        {
            var proceduresDto = context.Procedures
                .Select(x => new ProcedureXmlDto()
                {
                    Passport = x.Animal.PassportSerialNumber,
                    OwnerNumber = x.Animal.Passport.OwnerPhoneNumber,
                    DateTime = x.DateTime.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    AnimalAids = x.ProcedureAnimalAids
                        .Select(y => new AnimalAidsDto()
                        {
                            Name = y.AnimalAid.Name,
                            Price = y.AnimalAid.Price
                        })
                        .ToArray(),
                    TotalPrice = x.ProcedureAnimalAids.Select(y => y.AnimalAid.Price).Sum()
                })
                .OrderBy(p => DateTime.ParseExact(p.DateTime, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                .ThenBy(x => x.Passport)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProcedureXmlDto[]), new XmlRootAttribute("Procedures"));

            var sb = new StringBuilder();

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            xmlSerializer.Serialize(new StringWriter(sb), proceduresDto, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}