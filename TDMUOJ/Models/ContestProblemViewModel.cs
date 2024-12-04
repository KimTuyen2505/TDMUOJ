using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ContestProblemViewModel
    {
        public List<Contest> Contests { get; set; }
        public List<Problem> AllProblems { get; set; }
    }
}