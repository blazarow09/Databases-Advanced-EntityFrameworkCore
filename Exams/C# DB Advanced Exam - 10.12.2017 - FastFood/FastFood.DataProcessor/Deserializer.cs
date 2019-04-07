using FastFood.Data;
using FastFood.DataProcessor.Dto.Export;
using FastFood.DataProcessor.Dto.Import;
using FastFood.Models;
using FastFood.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FastFood.DataProcessor
{
    public static class Deserializer
    {
        private const string FailureMessage = "Invalid data format.";
        private const string SuccessMessage = "Record {0} successfully imported.";

        public static string ImportEmployees(FastFoodDbContext context, string jsonString)
        {
            var employeesDto = JsonConvert.DeserializeObject<EmployeeDto[]>(jsonString);

            var sb = new StringBuilder();
            var employees = new List<Employee>();

            foreach (var dto in employeesDto)
            {
                if (!IsVaild(dto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                if (!context.Positions.Any(p => p.Name == dto.Position))
                {
                    Position position = new Position()
                    {
                        Name = dto.Position
                    };

                    context.Positions.Add(position);
                    context.SaveChanges();
                }

                var employee = new Employee()
                {
                    Name = dto.Name,
                    Age = dto.Age,
                    PositionId = context.Positions.SingleOrDefault(x => x.Name == dto.Position).Id
                };

                employees.Add(employee);

                sb.AppendLine(string.Format(SuccessMessage, employee.Name));
            }

            context.Employees.AddRange(employees);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportItems(FastFoodDbContext context, string jsonString)
        {
            var itemsDto = JsonConvert.DeserializeObject<Dto.Import.ItemDto[]>(jsonString);

            var sb = new StringBuilder();
            var items = new List<Item>();

            foreach (var dto in itemsDto)
            {
                if (!IsVaild(dto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

               if (!context.Categories.Any(c => c.Name == dto.Category))
                {
                    Category category = new Category()
                    {
                        Name = dto.Category
                    };

                    context.Categories.Add(category);
                    context.SaveChanges();
                }

                if (items.Any(i => i.Name == dto.Name))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var item = new Item()
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    CategoryId = context.Categories.SingleOrDefault(x => x.Name == dto.Category).Id
                };

                items.Add(item);
                sb.AppendLine(string.Format(SuccessMessage, item.Name));
            }

            context.Items.AddRange(items);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOrders(FastFoodDbContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(OrderDto[]), new XmlRootAttribute("Orders"));

            var ordersDto = (OrderDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var sb = new StringBuilder();
            var orders = new List<Order>();

            foreach (var dto in ordersDto)
            {
                 if (string.IsNullOrWhiteSpace(dto.Customer))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                if (!context.Employees.Any(x => x.Name == dto.Employee))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                DateTime dateTime;
                var isValidDateTime = DateTime.TryParseExact(dto.DateTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                if (!isValidDateTime)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                OrderType type;
                var isVaildType = Enum.TryParse<OrderType>(dto.Type, out type);
                if (!isVaildType)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isItemsValid = true;
                foreach (var itemDto in dto.Items)
                {
                    if (itemDto.Quantity <= 0 || context.Items.Any(x => x.Name == itemDto.Name))
                    {
                        isItemsValid = false;
                        break;
                    }
                }

                if (!isItemsValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                
                var order = new Order()
                {
                    Customer = dto.Customer,
                    DateTime = dateTime,
                    Type = type,
                    EmployeeId = context.Employees.SingleOrDefault(x => x.Name == dto.Employee).Id
                };

                context.Orders.Add(order);
                context.SaveChanges();

                sb.AppendLine($"Order for {order.Customer} on {order.DateTime} added");

                var ordersItems = new List<OrderItem>();

                foreach (var item in dto.Items)
                {
                    var orderItem = new OrderItem()
                    {
                        Order = order,
                        ItemId = context.Items.SingleOrDefault(x => x.Name == item.Name).Id,
                        Quantity = item.Quantity
                    };

                    ordersItems.Add(orderItem);
                }

                context.OrderItems.AddRange(ordersItems);
                context.SaveChanges();
            }

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