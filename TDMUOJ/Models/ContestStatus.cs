using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ContestStatus
    {
        public List<Contest> contestsUpComming { get; set; }
        public List<Contest> contestsRunning { get; set; }
        public List<ContestEndedCustom> contestsEnded { get; set; }
    }
}