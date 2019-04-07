using AutoMapper;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        private const string CountMessage = "Successfully imported {0}";

        public static void Main(string[] args)
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<ProductShopProfile>();
            });

            // var inputXml = File.ReadAllText("../../../Datasets/products.xml");

            using (ProductShopContext context = new ProductShopContext())
            {
                var result = GetUsersWithProducts(context);

                Console.WriteLine(result);
            }
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportUserDto[]), new XmlRootAttribute("Users"));

            var usersDto = (ImportUserDto[])serializer.Deserialize(new StringReader(inputXml));

            var users = new List<User>();

            foreach (var userDto in usersDto)
            {
                var user = Mapper.Map<User>(userDto);

                users.Add(user);
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return string.Format(CountMessage, users.Count);
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportProductDto[]), new XmlRootAttribute("Products"));

            var productsDto = (ImportProductDto[])serializer.Deserialize(new StringReader(inputXml));

            var products = new List<Product>();

            foreach (var productDto in productsDto)
            {
                var product = new Product()
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    SellerId = productDto.SellerId,
                    BuyerId = productDto.BuyerId
                };

                products.Add(product);
            }

            context.Products.AddRange(products);
            context.SaveChanges();

            return string.Format(CountMessage, products.Count);
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCategoryDto[]), new XmlRootAttribute("Categories"));

            var categoriesDto = (ImportCategoryDto[])serializer.Deserialize(new StringReader(inputXml));

            var categories = new List<Category>();

            foreach (var categoryDto in categoriesDto)
            {
                var category = Mapper.Map<Category>(categoryDto);

                categories.Add(category);
            }

            context.Categories.AddRange(categories.Where(x => x.Name != null).ToArray());

            context.SaveChanges();

            return string.Format(CountMessage, categories.Count);
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ImportCategoryProductDto[]), new XmlRootAttribute("CategoryProducts"));

            var categoriesProductsDto = (ImportCategoryProductDto[])serializer.Deserialize(new StringReader(inputXml));

            var categoriesProducts = new List<CategoryProduct>();

            var categories = context.Categories.Select(x => x.Id).ToList();
            var products = context.Products.Select(x => x.Id).ToList();

            foreach (var catProdDto in categoriesProductsDto)
            {
                var cId = catProdDto.CategoryId;
                var pId = catProdDto.ProductId;

                if (categories.Contains(cId) && products.Contains(pId))
                {
                    var catProd = Mapper.Map<CategoryProduct>(catProdDto);

                    categoriesProducts.Add(catProd);
                }
            }

            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();

            return string.Format(CountMessage, categoriesProducts.Count);
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new ExportProductsInRangeDto()
                {
                    Name = x.Name,
                    Price = x.Price,
                    Buyer = $"{x.Buyer.FirstName} {x.Buyer.LastName}" ?? null
                })
                .OrderBy(x => x.Price)
                .Take(10)
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportProductsInRangeDto[]), new XmlRootAttribute("Products"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            serializer.Serialize(new StringWriter(sb), products, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var soldProducts = context.Users
                .Where(x => x.ProductsSold.Any())
                .Select(x => new ExportUserSoldDto()
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SoldProducts = x.ProductsSold
                        .Select(y => new ExportSoldProductsDto()
                        {
                            Name = y.Name,
                            Price = decimal.Parse(y.Price.ToString("G29"))
                        })
                        .ToArray()
                })
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Take(5)
                .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportUserSoldDto[]), new XmlRootAttribute("Users"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            serializer.Serialize(new StringWriter(sb), soldProducts, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                    .Select(x => new ExportCategoriesDto()
                    {
                        Name = x.Name,
                        Count = x.CategoryProducts.Select(p => p.ProductId).Count(),
                        AveragePrice = x.CategoryProducts
                            .Average(y => y.Product.Price),
                        TotalRevenue = x.CategoryProducts
                            .Sum(y => y.Product.Price)
                    })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.TotalRevenue)
                    .ToArray();

            XmlSerializer serializer = new XmlSerializer(typeof(ExportCategoriesDto[]), new XmlRootAttribute("Categories"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            serializer.Serialize(new StringWriter(sb), categories, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersWithProducts = context.Users
                    .Where(x => x.ProductsSold.Any(y => y.Buyer != null))
                    .OrderByDescending(x => x.ProductsSold.Count(ps => ps.Buyer != null))
                    .Select(x => new ExportUsersProductsDto()
                    {
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Age = x.Age,
                        Products = new SoldProductsDto()
                        {
                            Count = x.ProductsSold.Count,
                            Products = x.ProductsSold
                                .Select(p => new ProductsDto()
                                {
                                    Name = p.Name,
                                    Price = p.Price
                                })
                                .OrderByDescending(n => n.Price)
                                .ToArray()
                        }
                    })
                    .Take(10)
                    .ToArray();
            
            var usersWithCount = new ExportUsersWithCountDto()
            {
                Count = context
                    .Users
                    .Count(x => x.ProductsSold.Any()),
                Users = usersWithProducts
            };
            
            XmlSerializer serializer = new XmlSerializer(typeof(ExportUsersWithCountDto), new XmlRootAttribute("Users"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            serializer.Serialize(new StringWriter(sb), usersWithCount, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}