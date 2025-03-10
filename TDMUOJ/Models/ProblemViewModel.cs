using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class ProblemViewModel
	{
        public int id { get; set; }
        public string title { get; set; }
        public string difficulty { get; set; }
        public int timeLimit { get; set; }
        public int memoryLimit { get; set; }
        public int numberOfAccepted { get; set; }
        public string problemIndex { get; set; } // A, B, C, ...
    }
}