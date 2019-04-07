namespace Cinema.DataProcessor
{
    using Cinema.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var movies = context.Movies
                        .Where(x => x.Rating >= rating)
                        .Where(x => x.Projections.Any(y => y.Tickets.Any()))
                        .Select(x => new
                        {
                            MovieName = x.Title,
                            Rating = x.Rating.ToString("f2"),
                            TotalIncomes = x.Projections.Sum(y => y.Tickets.Sum(p => p.Price)).ToString("f2"),

                            Customers = x.Projections
                           .SelectMany(t => t.Tickets
                           .Select(p => p.Customer)
                           .Select(c => new
                           {
                               FirstName = c.FirstName,
                               LastName = c.LastName,
                               Balance = c.Balance.ToString("f2")
                           }))
                           .OrderByDescending(xx => xx.Balance)
                           .ThenBy(xx => xx.FirstName)
                           .ThenBy(xx => xx.LastName)
                           .ToArray()
                        })
                        .Take(10)
                        .OrderByDescending(x => decimal.Parse(x.Rating))
                        .ThenByDescending(x => decimal.Parse(x.TotalIncomes))
                        .ToArray();

            var result = JsonConvert.SerializeObject(movies);

            return result;
        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            var customers = context.Customers
                .Where(x => x.Age >= age)
                .Select(x => new CustomerXmlDto()
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SpentMoney = x.Tickets.Sum(t => t.Price).ToString("f2"),
                    SpentTime = new TimeSpan(x.Tickets.Sum(y => y.Projection.Movie.Duration.Ticks)).
                            ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                })
                .OrderByDescending(x => double.Parse(x.SpentMoney))
                .Take(10)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomerXmlDto[]), new XmlRootAttribute("Customers"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("", "")
            });

            xmlSerializer.Serialize(new StringWriter(sb), customers, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}