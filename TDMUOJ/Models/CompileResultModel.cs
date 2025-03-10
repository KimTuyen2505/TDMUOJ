using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class CompileResultModel
	{
        public string result { get; set; }
        public int testCaseAchieved { get; set; }
        public double maxTime { get; set; }
        public double maxMemory { get; set; }
    }
}