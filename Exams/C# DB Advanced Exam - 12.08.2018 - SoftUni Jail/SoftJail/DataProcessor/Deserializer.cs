namespace SoftJail.DataProcessor
{
    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
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
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var departmentCellsDto = JsonConvert.DeserializeObject<DepartmentDto
              []>(jsonString);

            var sb = new StringBuilder();
            var departmentCells = new List<Department>();

            foreach (var departmentDto in departmentCellsDto)
            {
                if (IsVaild(departmentDto) == false)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var cells = new List<Cell>();
                var isValidCell = true;

                foreach (var cellDto in departmentDto.Cells)
                {
                    if(IsVaild(cellDto) == false)
                    {
                        sb.AppendLine("Invalid Data");
                        isValidCell = false;
                        break;
                    }

                    var cell = new Cell()
                    {
                        CellNumber = cellDto.CellNumber,
                        HasWindow = cellDto.HasWindow
                    };

                    cells.Add(cell);
                }

                if (isValidCell)
                {
                    var department = new Department()
                {
                    Name = departmentDto.Name,
                    Cells = cells
                };

                departmentCells.Add(department);

                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
                }
            }

            context.Departments.AddRange(departmentCells);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersDto = JsonConvert.DeserializeObject<ImportPrisonerDto[]>(jsonString);

            var sb = new StringBuilder();

            var prisonersMails = new List<Prisoner>();

            foreach (var prisonerDto in prisonersDto)
            {
                if(IsVaild(prisonerDto) == false)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                DateTime incarceration;
                if (!DateTime.TryParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out incarceration))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                DateTime release;
                if (prisonerDto.ReleaseDate != null)
                {
                    if (!DateTime.TryParseExact(prisonerDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out release))
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }
                }

                var isValidMail = true;
                var mails = new List<Mail>();
                    
                foreach (var mailDto in prisonerDto.Mails)
                {
                    if (IsVaild(mailDto))
                    {
                        sb.AppendLine("Invalid Data");
                        isValidMail = false;
                        break;
                    }

                    var mail = new Mail()
                    { 
                        Description = mailDto.Description,
                        Sender = mailDto.Sender,
                        Address = mailDto.Address,
                    };

                    mails.Add(mail);
                }

                if (isValidMail)
                {
                    var prisoner = new Prisoner()
                    {
                        FullName = prisonerDto.FullName,
                        Nickname = prisonerDto.Nickname,
                        Age = prisonerDto.Age,
                        IncarcerationDate = incarceration,
                        ReleaseDate = prisonerDto.ReleaseDate == null ? (DateTime?) null : DateTime.ParseExact(prisonerDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        Bail = prisonerDto.Bail,
                        CellId = prisonerDto.CellId,
                        Mails = mails
                    };

                    prisonersMails.Add(prisoner);

                    sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
                }
            }

            context.Prisoners.AddRange(prisonersMails);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(OfficerDto[]), new XmlRootAttribute("Officers"));

            var officersDto = (OfficerDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();
            var officers = new List<Officer>();

            foreach (var officerDto in officersDto)
            {
                if(IsVaild(officerDto) == false)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Position position;
                if(Enum.TryParse<Position>(officerDto.Position, out position) == false)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Weapon weapon;
                if(Enum.TryParse<Weapon>(officerDto.Weapon, out weapon) == false)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officersPrisoners = new List<OfficerPrisoner>();

                foreach (PrisonerXmlDto dto in officerDto.Prisoners)
                {
                    var officerPrisoner = new OfficerPrisoner()
                    {
                        PrisonerId = dto.Id,
                    };

                    officersPrisoners.Add(officerPrisoner);
                }

                Officer officer = new Officer()
                {
                    FullName = officerDto.Name,
                    Salary = officerDto.Salary,
                    Position = position,
                    Weapon = weapon,
                    DepartmentId = officerDto.DepartmentId,
                    OfficerPrisoners = officersPrisoners
                };

                officers.Add(officer);

                sb.AppendLine($"Imported {officerDto.Name} ({officerDto.Prisoners.Length} prisoners)");
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsVaild(object entity)
        {
            var validationContext = new ValidationContext(entity);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator
                .TryValidateObject(entity, validationContext, validationResult, true);

            return isValid;
        }
    }
}