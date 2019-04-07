namespace VaporStore.DataProcessor
{
    using Data;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.ImportDtos;

    public static class Deserializer
    {
        private const string FailureMessage = "Invalid Data";

        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            var gamesDto = JsonConvert.DeserializeObject<GameDto[]>(jsonString);

            var sb = new StringBuilder();
            var games = new List<Game>();
            var tags = new List<Tag>();
            var developers = new List<Developer>();
            var genres = new List<Genre>();

            foreach (var dto in gamesDto)
            {
                if (!IsValid(dto) || !dto.Tags.All(IsValid) || dto.Tags.Count == 0)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                DateTime dateTime;
                var isValidDate = DateTime.TryParseExact(dto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                if (!isValidDate)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var developer = context.Developers.SingleOrDefault(x => x.Name == dto.Developer);
                if (developer == null)
                {
                    developer = new Developer()
                    {
                        Name = dto.Developer
                    };

                    developers.Add(developer);
                }

                var genre = context.Genres.SingleOrDefault(x => x.Name == dto.Genre);
                if (genre == null)
                {
                    genre = new Genre()
                    {
                        Name = dto.Genre
                    };

                    genres.Add(genre);
                }

                var gameTags = new List<Tag>();

                foreach (var tagName in dto.Tags)
                {
                    var tag = tags.SingleOrDefault(x => x.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag()
                        {
                            Name = tagName
                        };

                        tags.Add(tag);
                    }

                    gameTags.Add(tag);
                }

                var game = new Game()
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    ReleaseDate = dateTime,
                    Developer = developer,
                    Genre = genre,
                    GameTags = gameTags.Select(x => new GameTag { Tag = x }).ToArray()
                };

                games.Add(game);
                sb.AppendLine($"Added {dto.Name} ({dto.Genre}) with {dto.Tags.Count} tags");
            }

            context.Games.AddRange(games);
            context.SaveChanges();
            ;
            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString) 
        {
            throw new NotImplementedException();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            throw new NotImplementedException();
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