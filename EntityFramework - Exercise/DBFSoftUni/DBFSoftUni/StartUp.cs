using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        private static void Main(string[] args)
        {
            //GetEmployeesFullInformation(context);
            //GetEmployeesWithSalaryOver50000(context);
            //GetEmployeesFromResearchAndDevelopment(context);
            //AddNewAddressToEmployee(context);

            using (var context = new SoftUniContext())
            {
                //var result = GetEmployeesInPeriod(context);
                //var result = GetAddressesByTown(context);
                //var result = GetEmployee147(context);
                //var result = GetDepartmentsWithMoreThan5Employees(context);
                //var result = GetLatestProjects(context);
                //var result = GetEmployeesByFirstNameStartingWithSa(context);
                //var result = DeleteProjectById(context);
                var result = RemoveTown(context);

                Console.WriteLine(result);
            }
        }

        public static void GetEmployeesFullInformation(SoftUniContext context)
        {
            List<Employee> employees = context.Employees
                .OrderBy(e => e.EmployeeId)
                .ToList();

            foreach (Employee e in employees)
            {
                Console.WriteLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:f2}");
            }
        }

        public static void GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            List<Employee> employees = context.Employees
                .Where(e => e.Salary > 50000)
                .OrderBy(f => f.FirstName)
                .ToList();

            foreach (Employee e in employees)
            {
                Console.WriteLine($"{e.FirstName} - {e.Salary:f2}");
            }
        }

        public static void GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            List<Employee> employees = context.Employees
                .Include(e => e.Department)
                .Where(x => x.Department.Name == "Research and Development")
                .OrderBy(s => s.Salary)
                .ThenByDescending(f => f.FirstName)
                .ToList();

            foreach (Employee e in employees)
            {
                Console.WriteLine($"{e.FirstName} {e.LastName} from {e.Department.Name} - ${e.Salary:f2}");
            }
        }

        public static void AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            Employee employee = context.Employees
                .FirstOrDefault(e => e.LastName == "Nakov");

            if (employee != null)
            {
                employee.Address = newAddress;
                context.SaveChanges();
            }

            var employees = context.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText);

            foreach (var addrText in employees)
            {
                Console.WriteLine(addrText);
            }
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
               .Where(e => e.EmployeesProjects.Any(x => x.Project.StartDate.Year >= 2001 && x.Project.StartDate.Year <= 2003))
               .Select(x => new
               {
                   Fullname = x.FirstName + " " + x.LastName,
                   ManagerName = "Manager: " + x.Manager.FirstName + " " + x.Manager.LastName,
                   Projects = x.EmployeesProjects.Select(p => new
                   {
                       ProjectName = p.Project.Name,
                       StartDate = p.Project.StartDate,
                       EndDate = p.Project.EndDate
                   }),
               })
               .Take(10);

            var sb = new StringBuilder();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.Fullname} - {e.ManagerName}");

                foreach (var p in e.Projects)
                {
                    sb.AppendLine($" --{p.ProjectName} - {p.StartDate:M/d/yyyy h:mm:ss tt} AM - {p.EndDate:M/d/yyyy h:mm:ss tt} AM");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var addresses = context.Addresses
                .OrderByDescending(x => x.Employees.Count)
                .ThenBy(x => x.Town.Name)
                .ThenBy(x => x.AddressText)
                .Select(a => new
                {
                    AddressText = a.AddressText,
                    TownName = a.Town.Name,
                    EmployeeCount = a.Employees.Count
                })
                .Take(10)
                .ToList();

            foreach (var addr in addresses)
            {
                sb.AppendLine($"{addr.AddressText}, {addr.TownName} - {addr.EmployeeCount} employees");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employee174 = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(inf => new
                {
                    FullName = inf.FirstName + " " + inf.LastName,
                    JobTitle = inf.JobTitle,
                    Projects = inf.EmployeesProjects.Select(p => new
                    {
                        ProjectName = p.Project.Name
                    })
                    .OrderBy(p => p.ProjectName)
                });

            foreach (var e in employee174)
            {
                sb.AppendLine($"{e.FullName} - {e.JobTitle}");

                foreach (var p in e.Projects)
                {
                    sb.AppendLine($"{p.ProjectName}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .Select(m => new
                {
                    DepartmentName = m.Name,
                    ManagerFullName = m.Manager.FirstName + " " + m.Manager.LastName,
                    EmployeeCount = m.Employees.Count,
                    Employees = m.Employees.Select(e => new
                    {
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        EmployeeJobTitle = e.JobTitle
                    })
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                })
                .OrderBy(e => e.EmployeeCount)
                .ThenBy(d => d.DepartmentName);

            foreach (var m in departments)
            {
                sb.AppendLine($"{m.DepartmentName} - {m.ManagerFullName}");

                foreach (var e in m.Employees)
                {
                    sb.AppendLine($"{e.FirstName} {e.LastName} - {e.EmployeeJobTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var projects = context.Projects
                .Select(p => new
                {
                    ProjectName = p.Name,
                    StartDate = p.StartDate,
                    Description = p.Description
                })
                .OrderBy(p => p.ProjectName)
                .ThenByDescending(s => s.StartDate)
                .Take(10);

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.ProjectName}");
                sb.AppendLine($"{p.Description}");
                sb.AppendLine($"{p.StartDate:M/d/yyyy h:mm:ss tt} AM");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employees = context.Employees
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    JobTitle = e.JobTitle,
                    Salary = e.Salary
                })
                .Where(e => e.FirstName.StartsWith("Sa")
                ).OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName);

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var targetProject = context.Projects.FirstOrDefault(x => x.ProjectId == 2);

            var employeeProjects = context.EmployeesProjects.Where(x => x.ProjectId == 2)
                .ToList();

            context.EmployeesProjects.RemoveRange(employeeProjects);

            context.Projects.Remove(targetProject);

            context.SaveChanges();

            var projects = context.Projects.Select(x => new
            {
                Project = x.Name
            })
            .Take(10);

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.Project}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var town = context.Employees
                .Include(a => a.Address)
                .ThenInclude(t => t.Town)
                .First()
                .Address.Town.Name;
                

            Console.WriteLine(String.Join(' ', town));


            return sb.ToString().TrimEnd();
        }
    }
}