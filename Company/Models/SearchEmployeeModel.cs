
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Company.Models
{
    public class SearchEmployeeModel
    {
        public List<Employee> Employees { get; set; }

        public SelectList Managers { get; set; }

        [DisplayName("Performance Manager")]
        public string Manager { get; set; }

        [DisplayName("From")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DateFrom { get; set; }

        [DisplayName("To")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DateTo { get; set; }

        [DisplayName("Name & Surname")]
        public string NameString { get; set; }
    }
}