using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class TestCaseSubmit
    {
        public int language_id { get; set; }
        public string stdin { get; set; }
        public string source_code { get; set; }
        public string expected_output { get; set; }
        public decimal cpu_time_limit { get; set; }
        public decimal memory_limit { get; set; }
    }
}