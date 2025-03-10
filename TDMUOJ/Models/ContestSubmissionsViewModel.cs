using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class ContestSubmissionsViewModel
	{
        public Contest Contest { get; set; }
        public bool IsParticipant { get; set; }
        public int ContestId { get; set; }
        public string ContestTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<SubmissionViewModel> Submissions { get; set; }
        public List<ProblemViewModel> Problems { get; set; }
    }
}