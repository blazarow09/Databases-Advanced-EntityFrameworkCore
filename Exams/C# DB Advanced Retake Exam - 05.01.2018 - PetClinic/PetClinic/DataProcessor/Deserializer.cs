namespace PetClinic.DataProcessor
{
    using Newtonsoft.Json;
    using PetClinic.Data;
    using PetClinic.DataProcessor.Import;
    using PetClinic.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        public static string ImportAnimalAids(PetClinicContext context, string jsonString)
        {
            var animalAidsDto = JsonConvert.DeserializeObject<AnimalAidDto[]>(jsonString);

            var sb = new StringBuilder();

            var animalAids = new List<AnimalAid>();
            var addedAnimals = new List<string>();

            foreach (var animalAidDto in animalAidsDto)
            {
                if (IsValid(animalAidDto) == false)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                if (addedAnimals.Contains(animalAidDto.Name))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                var animalAid = new AnimalAid()
                {
                    Name = animalAidDto.Name,
                    Price = animalAidDto.Price
                };

                animalAids.Add(animalAid);
                addedAnimals.Add(animalAid.Name);

                sb.AppendLine($"Record {animalAid.Name} successfully imported.");
            }

            context.AnimalAids.AddRange(animalAids);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAnimals(PetClinicContext context, string jsonString)
        {
            AnimalDto[] animalDtos = JsonConvert.DeserializeObject<AnimalDto[]>(jsonString);
            StringBuilder sb = new StringBuilder();
            List<Animal> animals = new List<Animal>();

            foreach (AnimalDto dto in animalDtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                if (!IsValid(dto.Passport))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                DateTime dateTime;
                bool isValidDate = DateTime.TryParseExact(dto.Passport.RegistrationDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                if (!isValidDate)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                if (context.Passports.Any(p => p.SerialNumber == dto.Passport.SerialNumber))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                Passport passport = new Passport()
                {
                    SerialNumber = dto.Passport.SerialNumber,
                    OwnerName = dto.Passport.OwnerName,
                    OwnerPhoneNumber = dto.Passport.OwnerPhoneNumber,
                    RegistrationDate = dateTime
                };

                context.Passports.Add(passport);
                context.SaveChanges();

                Animal animal = new Animal()
                {
                    Name = dto.Name,
                    Type = dto.Type,
                    Age = dto.Age,
                    PassportSerialNumber = dto.Passport.SerialNumber
                };

                animals.Add(animal);
                sb.AppendLine($"Record {animal.Name} Passport №: {animal.PassportSerialNumber} successfully imported.");
            }

            context.Animals.AddRange(animals);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportVets(PetClinicContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(VetDto[]), new XmlRootAttribute("Vets"));

            var vetsDto = (VetDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();
            var vets = new List<Vet>();
            var vetsNumber = new List<string>();
            foreach (var vetDto in vetsDto)
            {
                if (!IsValid(vetDto))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                if (vetsNumber.Contains(vetDto.PhoneNumber) || vetDto.PhoneNumber == null)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                vetsNumber.Add(vetDto.PhoneNumber);

                var vet = new Vet()
                {
                    Name = vetDto.Name,
                    Profession = vetDto.Profession,
                    Age = vetDto.Age,
                    PhoneNumber = vetDto.PhoneNumber
                };

                vets.Add(vet);
                sb.AppendLine($"Record {vet.Name} successfully imported.");
            }

            context.Vets.AddRange(vets);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProcedures(PetClinicContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProcedureDto[]), new XmlRootAttribute("Procedures"));

            var proceduresDto = (ProcedureDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();
            var procedures = new List<Procedure>();

            foreach (var dto in proceduresDto)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                if (!context.Vets.Any(v => v.Name == dto.VetName))
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                var isValidSerial = context.Animals
                    .Any(x => x.Passport.SerialNumber == dto.AnimalSerial);

                if (!isValidSerial)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                DateTime dateTime;
                var isValidDateTime = DateTime.TryParseExact(dto.DateTime, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                if (!isValidDateTime)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                var isValidAnimalAid = true;
                foreach (var animalAid in dto.AnimalAids)
                {
                    var isValidAid = context.AnimalAids.Any(x => x.Name == animalAid.Name);

                    if (!isValidAid)
                    {
                        isValidAnimalAid = false;
                        break;
                    }
                }

                if (!isValidAnimalAid)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                bool isExistAnimalAid = false;
                List<string> animalAids = new List<string>();

                foreach (var animalAidDto in dto.AnimalAids)
                {
                    if (animalAids.Contains(animalAidDto.Name))
                    {
                        isExistAnimalAid = true;
                        break;
                    }

                    animalAids.Add(animalAidDto.Name);
                }

                if (isExistAnimalAid)
                {
                    sb.AppendLine("Error: Invalid data.");
                    continue;
                }

                var vetId = context.Vets.FirstOrDefault(v => v.Name == dto.VetName).Id;

                var animalId = context.Animals.FirstOrDefault(x => x.Passport.SerialNumber == dto.AnimalSerial).Id;

                Procedure procedure = new Procedure()
                {
                    VetId = vetId,
                    AnimalId = animalId,
                    DateTime = dateTime
                };

                context.Procedures.Add(procedure);
                context.SaveChanges();

                sb.AppendLine("Record successfully imported.");

                var procedureAnimalAids = new List<ProcedureAnimalAid>();

                foreach (var dtoAnimalAid in dto.AnimalAids)
                {
                    var procedureAnimalAid = new ProcedureAnimalAid()
                    {
                        Procedure = procedure,
                        AnimalAidId = context.AnimalAids.SingleOrDefault(x => x.Name == dtoAnimalAid.Name).Id
                    };
                }

                context.ProceduresAnimalAids.AddRange(procedureAnimalAids);
                context.SaveChanges();
            }

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object entity)
        {
            var validationContext = new ValidationContext(entity);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator
                .TryValidateObject(entity, validationContext, validationResult, true);

            return isValid;
        }
    }
}