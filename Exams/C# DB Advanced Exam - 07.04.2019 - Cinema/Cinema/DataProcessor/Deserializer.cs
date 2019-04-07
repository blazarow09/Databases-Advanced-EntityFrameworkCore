namespace Cinema.DataProcessor
{
    using Cinema.Data.Models;
    using Cinema.Data.Models.Enums;
    using Cinema.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
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
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportMovie
            = "Successfully imported {0} with genre {1} and rating {2}!";

        private const string SuccessfulImportHallSeat
            = "Successfully imported {0}({1}) with {2} seats!";

        private const string SuccessfulImportProjection
            = "Successfully imported projection {0} on {1}!";

        private const string SuccessfulImportCustomerTicket
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var moviesDto = JsonConvert.DeserializeObject<MovieDto[]>(jsonString);

            var sb = new StringBuilder();
            var movies = new List<Movie>();
            var titles = new List<string>();

            foreach (var dto in moviesDto)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Genre genre;
                var isValidGenre = Enum.TryParse<Genre>(dto.Genre, out genre);
                if (!isValidGenre)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                TimeSpan timeSpan;
                var isValidTime = TimeSpan.TryParse(dto.Duration, out timeSpan);
                if (!isValidTime)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (titles.Contains(dto.Title))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var movie = new Movie()
                {
                    Title = dto.Title,
                    Genre = genre,
                    Duration = timeSpan,
                    Rating = dto.Rating,
                    Director = dto.Director
                };

                movies.Add(movie);
                titles.Add(dto.Title);

                sb.AppendLine(string.Format(SuccessfulImportMovie, dto.Title, dto.Genre, dto.Rating.ToString("f2")));
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var hallSeatsDto = JsonConvert.DeserializeObject<HallSeatDto[]>(jsonString);

            var sb = new StringBuilder();
            var seats = new List<Seat>();

            foreach (var dto in hallSeatsDto)
            {
                if (!IsValid(dto) || dto.Seats < 1)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var hall = new Hall()
                {
                    Name = dto.Name,
                    Is4Dx = dto.Is4Dx,
                    Is3D = dto.Is3D,
                };

                context.Halls.Add(hall);
                context.SaveChanges();

                for (int i = 0; i < dto.Seats; i++)
                {
                    var seat = new Seat()
                    {
                        HallId = context.Halls.SingleOrDefault(x => x.Name == dto.Name).Id
                    };

                    seats.Add(seat);
                };
                ;

                context.AddRange(seats);
                context.SaveChanges();

                seats.RemoveRange(0, seats.Count);

                var types = new List<string>();
                if (dto.Is4Dx)
                {
                    types.Add("4Dx");
                }

                if (dto.Is3D)
                {
                    types.Add("3D");
                }

                if (!dto.Is3D && !dto.Is4Dx)
                {
                    types.Add("Normal");
                }

                var projectType = string.Join("/", types);

                sb.AppendLine(string.Format(SuccessfulImportHallSeat, dto.Name, projectType, dto.Seats));
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectionDto[]), new XmlRootAttribute("Projections"));

            var projectionsDto = (ProjectionDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();
            var projections = new List<Projection>();

            foreach (var dto in projectionsDto)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime dateTime;
                var isVaildDate = DateTime.TryParseExact(dto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                if (!isVaildDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var hall = context.Halls.SingleOrDefault(x => x.Id == dto.HallId);
                if (hall == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var movie = context.Movies.SingleOrDefault(x => x.Id == dto.MovieId);
                if (movie == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var projection = new Projection()
                {
                    MovieId = movie.Id,
                    HallId = hall.Id,
                    DateTime = dateTime
                };

                projections.Add(projection);

                sb.AppendLine(string.Format(SuccessfulImportProjection, movie.Title, dateTime.ToString("MM/dd/yyyy")));
            }

            context.Projections.AddRange(projections);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomerDto[]), new XmlRootAttribute("Customers"));

            var customersDto = (CustomerDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();
            var customers = new List<Customer>();

            foreach (var dto in customersDto)
            {
                if (!IsValid(dto) || !dto.Tickets.All(IsValid))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isVaildProjection = true;

                foreach (var ticket in dto.Tickets)
                {
                    var projection = context.Projections.SingleOrDefault(x => x.Id == ticket.ProjectionId);
                    if (projection == null)
                    {
                        isVaildProjection = false;
                        break;
                    }
                }

                if (!isVaildProjection)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var tickets = new List<Ticket>();
                foreach (var ticketDto in dto.Tickets)
                {
                    var ticket = new Ticket()
                    {
                        Price = ticketDto.Price,
                        ProjectionId = ticketDto.ProjectionId
                    };

                    tickets.Add(ticket);
                }

                var customer = new Customer()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Age = dto.Age,
                    Balance = dto.Balance,
                    Tickets = tickets
                };

                customers.Add(customer);

                sb.AppendLine(string.Format(SuccessfulImportCustomerTicket, customer.FirstName, customer.LastName, tickets.Count));
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

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