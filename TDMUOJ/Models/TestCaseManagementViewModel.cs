using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class TestCaseManagementViewModel
	{
        public List<Problem> Problems { get; set; }
        public List<ProblemExample> Examples { get; set; }
    }
}