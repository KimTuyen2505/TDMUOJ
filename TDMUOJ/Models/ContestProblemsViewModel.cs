using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class ContestProblemsViewModel
	{
        public List<Contest> Contests { get; set; }
        public List<Problem> AllProblems { get; set; }
        public Problem Problem { get; set; }
        public List<ProblemExample> Examples { get; set; }
        public int ContestId { get; set; }
        public Contest Contest { get; set; }
        public List<Submission> Submissions { get; set; }
        public bool IsAccepted { get; set; }
        public string ContestTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<ProblemViewModel> Problems { get; set; }
        public bool IsVirtualParticipant { get; set; }
        public DateTime? VirtualStartTime { get; set; }
        public DateTime? VirtualEndTime { get; set; }
    }
}