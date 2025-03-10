using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ContestProblemDetailViewModel
    {
        public Contest Contest { get; set; }
        public int ContestId { get; set; }
        public string ContestTitle { get; set; }
        public int ProblemId { get; set; }
        public string ProblemTitle { get; set; }
        public string ProblemDescription { get; set; }
        public int TimeLimit { get; set; }
        public int MemoryLimit { get; set; }
        public string ProblemIndex { get; set; }
        public List<ProblemExampleViewModel> Examples { get; set; }
        public bool IsVirtualParticipant { get; set; }
        public DateTime? VirtualStartTime { get; set; }
        public DateTime? VirtualEndTime { get; set; }
        public bool IsAccepted { get; set; }
        public List<SubmissionViewModel> Submissions { get; set; }
    }
}