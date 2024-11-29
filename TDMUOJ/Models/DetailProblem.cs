using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class DetailProblem
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int timeLimit { get; set; }
        public int memoryLimit { get; set; }
        public string createdBy { get; set; }
        public List<ProblemExample> examples { get; set; }
    }
}