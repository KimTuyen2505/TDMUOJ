using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ProblemManagementCustom
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string difficulty { get; set; }
        public int timeLimit { get; set; }
        public int memoryLimit { get; set; }
        public Nullable<System.DateTime> createdAt { get; set; }
        public string createdBy { get; set; }
    }
}