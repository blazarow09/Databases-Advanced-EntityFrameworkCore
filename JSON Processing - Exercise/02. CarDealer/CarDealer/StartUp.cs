using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO.Export;
using CarDealer.DTO.Import.Car;
using CarDealer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarDealer
{
    public class StartUp
    {
        private const string CountMessage = "Successfully imported {0}.";

        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            //var inputJson = File.ReadAllText(@"D:\Projects\CSharpDBFundamentals\Databases Advanced - Entity Framework\JSON Processing - Exercise\02. CarDealer\CarDealer\Datasets\sales.json");

            var result = GetTotalSalesByCustomer(context);

            Console.WriteLine(result);
        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var suppliers = JsonConvert.DeserializeObject<Supplier[]>(inputJson);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return string.Format(CountMessage, suppliers.Count());
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var parts = JsonConvert.DeserializeObject<Part[]>(inputJson);

            var supIds = context.Suppliers
                .Select(x => x.Id)
                .ToHashSet();

            List<Part> validParts = new List<Part>();

            foreach (var part in parts)
            {
                if (supIds.Contains(part.SupplierId))
                {
                    validParts.Add(part);
                }
            }

            context.Parts.AddRange(validParts);
            context.SaveChanges();

            return string.Format(CountMessage, validParts.Count());
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var carDtos = JsonConvert.DeserializeObject<ImpCarDto[]>(inputJson);

            List<ImpCarDto> validCarDtos = new List<ImpCarDto>();

            HashSet<int> partIds = context.Parts.Select(x => x.Id).ToHashSet();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ImpCarDto, Car>();
            });

            var mapper = config.CreateMapper();

            foreach (var car in carDtos)
            {
                bool isValid = false;

                foreach (var part in car.PartsId.Distinct())
                {
                    if (partIds.Contains(part))
                    {
                        isValid = true;
                        break;
                    }

                    isValid = false;
                    break;
                }

                if (isValid)
                {
                    validCarDtos.Add(car);
                }
            }

            for (int i = 0; i < validCarDtos.Count; i++)
            {
                var carTemp = mapper.Map<Car>(validCarDtos[i]);

                foreach (var part in validCarDtos[i].PartsId.Distinct())
                {
                    carTemp.PartCars.Add(new PartCar() { CarId = i + 1, PartId = part });
                }

                context.Cars.AddRange(carTemp);
            }

            var result = validCarDtos.Count();
            context.SaveChanges();

            return string.Format(CountMessage, validCarDtos.Count());
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var customers = JsonConvert.DeserializeObject<Customer[]>(inputJson);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return string.Format(CountMessage, customers.Count());
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var sales = JsonConvert.DeserializeObject<Sale[]>(inputJson);

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return string.Format(CountMessage, sales.Count());
        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context
                .Customers
                .OrderBy(x => x.BirthDate)
                .Select(x => new CustomerDto()
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToArray();

            var result = JsonConvert.SerializeObject(customers, Formatting.Indented);

            return result;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context
                    .Cars
                    .Where(x => x.Make == "Toyota")
                    .Select(x => new CarToyotaDto()
                    {
                        Id = x.Id,
                        Make = x.Make,
                        Model = x.Model,
                        TravelledDistance = x.TravelledDistance
                    })
                    .OrderBy(x => x.Model)
                    .ThenByDescending(x => x.TravelledDistance)
                    .ToArray();

            var result = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return result;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context
                        .Suppliers
                        .Where(x => x.IsImporter == false)
                        .Select(x => new LocalSupplierDto()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            PartsCount = x.Parts.Select(y => y.Name).Count()
                        }).ToArray();

            var result = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return result;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context
                   .Cars
                   .Select(x => new
                   {
                       car = new
                       {
                           Make = x.Make,
                           Model = x.Model,
                           TravelledDistance = x.TravelledDistance
                       },
                       parts = x.PartCars
                           .Select(p => new
                           {
                               Name = p.Part.Name,
                               Price = $"{p.Part.Price:f2}"
                           })
                   })
                   .ToArray();

            var result = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return result;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context
                    .Customers
                    .Where(x => x.Sales.Any())
                    .Select(x => new CustomerInfoDto()
                    {
                        FullName = x.Name,
                        BoughtCars = x.Sales.Count(),
                        SpentMoney = x.Sales
                        .Sum(s => s.Car.PartCars
                            .Sum(p => p.Part.Price))
                    })
                    .OrderByDescending(x => x.SpentMoney)
                    .ThenByDescending(x => x.BoughtCars)
                    .ToArray();

            DefaultContractResolver contractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var result = JsonConvert.SerializeObject(customers, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });

            return result;
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context
                .Sales
                .Select(s => new 
                {
                    car = new 
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },
                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("f2"),
                    price = s.Car.PartCars.Sum(pc => pc.Part.Price).ToString("f2"),
                    priceWithDiscount = (s.Car.PartCars.Sum(pc => pc.Part.Price) - (s.Car.PartCars.Sum(pc => pc.Part.Price) * s.Discount / 100m)).ToString("f2")
                })
                .Take(10)
                .ToArray();

            var result = JsonConvert.SerializeObject(sales, Formatting.Indented);

            return result;
        }
    }
}