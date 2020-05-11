using Company.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Company.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly CompanyEntities db = new CompanyEntities();

        public ActionResult Index()
        {
            List<Employee> employeesList = db.Employee.ToList();

            foreach (Employee employee in employeesList)
            {
                Employee manager = employeesList.Where(s => s.Id == employee.Manager).FirstOrDefault();
                employee.ManagerName = manager.FirstName + ' ' + manager.LastName;
            }

            var searchEmployee = new SearchEmployeeModel
            {
                Employees = employeesList,
                Managers = new SelectList(LoadManagers(), "Manager"),
                DateFrom = employeesList.OrderBy(x => x.HireDate).FirstOrDefault().HireDate.ToString("dd'/'MM'/'yyyy"),
                DateTo = DateTime.Today.ToString("dd'/'MM'/'yyyy")
            };

            return View(searchEmployee);
        }

        [HttpPost]
        public ActionResult GetEmployees(string nameString, string dateFrom, string dateTo, string manager)
        {
            IQueryable<Employee> allEmployees = db.Employee;
            IEnumerable<Employee> selectedEmployees = allEmployees;
            List<Employee> employeesList = new List<Employee>();

            if (!string.IsNullOrEmpty(dateFrom))
            {
                DateTime dFrom = Convert.ToDateTime(dateFrom);
                selectedEmployees = selectedEmployees.Where(s => s.HireDate >= dFrom);
            }

            if (!string.IsNullOrEmpty(dateTo))
            {
                DateTime dTo = Convert.ToDateTime(dateTo);
                selectedEmployees = selectedEmployees.Where(s => s.HireDate <= dTo);
            }

            if (!string.IsNullOrEmpty(manager))
            {
                string firstName = manager.Split(' ')[0];
                string lastName = manager.Split(' ')[1];
                Employee m = allEmployees.Where(s => s.FirstName == firstName && s.LastName == lastName).FirstOrDefault();
                selectedEmployees = selectedEmployees.Where(s => s.Manager == m.Id);
            }

            if (!string.IsNullOrEmpty(nameString))
            {
                string firstTerm = Regex.Match(nameString, @"[^\s]*").Value;
                string lastTerm = Regex.Match(nameString, @"[\s][^,]*$").Value;
                if (lastTerm == "")
                {
                    selectedEmployees = selectedEmployees.Where(s => s.FirstName.Contains(firstTerm) || s.LastName.Contains(firstTerm));
                }
                else
                {
                    lastTerm = Regex.Match(lastTerm, @"[^\s]*$").Value;
                    selectedEmployees = selectedEmployees.Where(s => s.FirstName.Contains(firstTerm) && s.LastName.Contains(lastTerm));
                }
            }

            employeesList = selectedEmployees.ToList();

            if (selectedEmployees != null)
            {
                foreach (Employee employee in employeesList)
                {
                    Employee m = allEmployees.Where(s => s.Id == employee.Manager).FirstOrDefault();
                    employee.ManagerName = m.FirstName + ' ' + m.LastName;
                }
            }

            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var pageSize = length != null ? Convert.ToInt32(length) : 0;
            var skip = start != null ? Convert.ToInt32(start) : 0;

            return Json(new
            {
                data = employeesList.Skip(skip).Take(pageSize).ToList(),
                recordsFiltered = employeesList.Count(),
                recordsTotal = employeesList.Count()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteName(string term)
        {
            IQueryable<Employee> selectedEmployees = db.Employee;
            List<string> employees = new List<string>();

            string firstTerm = Regex.Match(term, @"[^,]*").Value;
            string lastTerm = Regex.Match(term, @"[\s][^,]*$").Value;

            if (lastTerm == "")
            {
                selectedEmployees = selectedEmployees.Where(s => s.FirstName.Contains(firstTerm) || s.LastName.Contains(firstTerm));
            }
            else
            {
                lastTerm = Regex.Match(lastTerm, @"[^\s]*$").Value;
                selectedEmployees = selectedEmployees.Where(s => s.FirstName.Contains(firstTerm) && s.LastName.Contains(lastTerm));
            }

            foreach (var employee in selectedEmployees.ToList())
            {
                employees.Add(employee.FirstName + " " + employee.LastName);
            }

            return Json(employees, JsonRequestBehavior.AllowGet);
        }

        public List<string> LoadManagers()
        {
            IQueryable<Employee> allEmployees = db.Employee;
            List<string> Managers = new List<string>();
            List<int> employeesId = (from employee in allEmployees select employee.Manager).ToList();
            List<Employee> result = allEmployees.Where(s => employeesId.Contains(s.Id)).ToList();

            Managers.Add(" ");
            foreach (Employee employee in result)
            {
                Managers.Add(employee.FirstName + " " + employee.LastName);
            }
            return Managers;
        }

    }
}
