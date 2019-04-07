using AutoMapper;
using CarDealer.Data;
using CarDealer.Dtos.Export;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        private const string CountMessage = "Successfully imported {0}";

        public static void Main(string[] args)
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<CarDealerProfile>();
            });

            //var inputXml = File.ReadAllText("../../../Datasets/cars.xml");

            using (CarDealerContext context = new CarDealerContext())
            {
                var result = GetSalesWithAppliedDiscount(context);

                Console.WriteLine(result);
            }
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSupplierDto[]), new XmlRootAttribute("Suppliers"));

            var suppliersDto = (ImportSupplierDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            var suppliers = new List<Supplier>();

            foreach (var supDto in suppliersDto)
            {
                var supplier = Mapper.Map<Supplier>(supDto);

                suppliers.Add(supplier);
            }

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return string.Format(CountMessage, suppliers.Count);
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPartDto[]), new XmlRootAttribute("Parts"));

            var partsDto = (ImportPartDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            var parts = new List<Part>();

            var suppliersIds = context.Suppliers
                .Select(x => x.Id)
                .ToArray();

            foreach (var partDto in partsDto)
            {
                if (suppliersIds.Contains(partDto.SupplierId))
                {
                    var part = Mapper.Map<Part>(partDto);

                    parts.Add(part);
                }
            }

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return string.Format(CountMessage, parts.Count);
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCarDto[]), new XmlRootAttribute("Cars"));

            var carsDto = (ImportCarDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            var validCarsDto = new List<ImportCarDto>();

            var partsCar = new List<PartCar>();

            var partsIds = context.Parts
                .Select(x => x.Id)
                .ToHashSet();

            foreach (var carDto in carsDto)
            {
                foreach (var currPartId in carDto.Parts)
                {
                    if (!partsIds.Contains(currPartId.PartId))
                    {
                        continue;
                    }
                }

                validCarsDto.Add(carDto);
            }

            for (int i = 0; i < validCarsDto.Count; i++)
            {
                var carTemp = new Car()
                {
                    Make = validCarsDto[i].Make,
                    Model = validCarsDto[i].Model,
                    TravelledDistance = validCarsDto[i].TravelledDistance,
                    PartsId = validCarsDto[i].Parts
                    .Select(x => x.PartId)
                    .ToList()
                };

                foreach (var partId in carTemp.PartsId.Distinct())
                {
                    carTemp.PartCars.Add(new PartCar() { CarId = i + 1, PartId = partId });
                }

                context.Cars.AddRange(carTemp);
            }
            var count = validCarsDto.Count();

            context.SaveChanges();

            return string.Format(CountMessage, count);
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCusotmerDto[]), new XmlRootAttribute("Customers"));

            var customersDto = (ImportCusotmerDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            var customers = new List<Customer>();

            foreach (var customerDto in customersDto)
            {
                var customer = Mapper.Map<Customer>(customerDto);

                customers.Add(customer);
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return string.Format(CountMessage, customers.Count);
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSaleDto[]), new XmlRootAttribute("Sales"));

            var carsIds = context.Cars
                    .Select(x => x.Id)
                    .ToHashSet();

            var sales = new List<Sale>();

            var salesDto = (ImportSaleDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            foreach (var saleDto in salesDto)
            {
                if (!carsIds.Contains(saleDto.CarId))
                {
                    continue;
                }

                var sale = Mapper.Map<Sale>(saleDto);

                sales.Add(sale);
            }

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return string.Format(CountMessage, sales.Count);
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            var cars = context.Cars
                    .Where(x => x.TravelledDistance >= 2000000)
                    .Select(x => new ExportCarsWithDistanceDto()
                    {
                        Make = x.Make,
                        Model = x.Model,
                        TravelledDistance = x.TravelledDistance
                    })
                    .OrderBy(x => x.Make)
                    .ThenBy(x => x.Model)
                    .Take(10)
                    .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarsWithDistanceDto[]), new XmlRootAttribute("cars"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            xmlSerializer.Serialize(new StringWriter(sb), cars, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars
                    .Where(x => x.Make == "BMW")
                    .Select(x => new ExportCarBMWDto()
                    {
                        Id = x.Id,
                        Model = x.Model,
                        TravelledDistance = x.TravelledDistance
                    })
                    .OrderBy(x => x.Model)
                    .ThenByDescending(x => x.TravelledDistance)
                    .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarBMWDto[]), new XmlRootAttribute("cars"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            xmlSerializer.Serialize(new StringWriter(sb), cars, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                    .Where(x => x.IsImporter == false)
                    .Select(x => new ExportLocalSuppliersDto()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        PartsCount = x.Parts.Count
                    })
                    .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportLocalSuppliersDto[]), new XmlRootAttribute("suppliers"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            xmlSerializer.Serialize(new StringWriter(sb), suppliers, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(x => new ExportCarWithPartsDto()
                {
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance,
                    Parts = x.PartCars
                                .Select(p => new PartsDto()
                                {
                                    Name = p.Part.Name,
                                    Price = p.Part.Price
                                })
                                .OrderByDescending(y => y.Price)
                                .ToArray()
                })
                .OrderByDescending(x => x.TravelledDistance)
                .ThenBy(x => x.Model)
                .Take(5)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarWithPartsDto[]), new XmlRootAttribute("cars"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
           {
                new XmlQualifiedName("","")
            });

            xmlSerializer.Serialize(new StringWriter(sb), cars, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(x => x.Sales.Any(y => y.Car != null))
                .Select(x => new ExportTotalSalesDto()
                {
                    FullName = x.Name,
                    BoughtCars = x.Sales
                        .Select(y => y.Car)
                        .Count(),
                    SpentMoney = x.Sales
                        .Sum(y => y.Car.PartCars
                        .Sum(p => p.Part.Price))
                })
                .OrderByDescending(x => x.SpentMoney)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportTotalSalesDto[]), new XmlRootAttribute("customers"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
           {
                new XmlQualifiedName("","")
            });

            xmlSerializer.Serialize(new StringWriter(sb), customers, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                    .Select(x => new ExportSaleWithDiscountDto()
                    {
                        Car = new CarDto()
                        {
                            Make = x.Car.Make,
                            Model = x.Car.Model,
                            TravelledDistance = x.Car.TravelledDistance
                        },
                        Discount = decimal.Parse(x.Discount.ToString("f0")),
                        CustomerName = x.Customer.Name,
                        Price = x.Car.PartCars.Sum(y => y.Part.Price),
                        PriceWithDiscount = (x.Car.PartCars
                                .Sum(pc => pc.Part.Price) -
                                (x.Car.PartCars.Sum(pc => pc.Part.Price) *
                                x.Discount / 100m))
                    })
                    .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportSaleWithDiscountDto[]), new XmlRootAttribute("sales"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
          {
                new XmlQualifiedName("","")
            });

            xmlSerializer.Serialize(new StringWriter(sb), sales, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}