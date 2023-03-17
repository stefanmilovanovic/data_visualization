using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_DataVisualization.Models
{
    public class Employee
    {
        public string Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
    }
}