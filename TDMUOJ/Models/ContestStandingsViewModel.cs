using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ContestStandingsViewModel
    {
        public int ContestId { get; set; }
        public string ContestTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<ContestParticipantViewModel> Participants { get; set; }
        public List<ProblemViewModel> Problems { get; set; }
        public List<Submission> Submissions { get; set; }
    }
}