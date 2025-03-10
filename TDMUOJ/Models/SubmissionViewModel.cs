using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class SubmissionViewModel
	{
        public int Id { get; set; }
        public int ProblemId { get; set; }
        public string ProblemName { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int? ContestId { get; set; }
        public string Code { get; set; }
        public string Language { get; set; }
        public double? MaxTime { get; set; }
        public double? MaxMemory { get; set; }
        public string Result { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? TestCaseTotal { get; set; }
        public int? TestCaseAchieved { get; set; }
        public bool IsVirtual { get; set; }
        public DateTime? VirtualSubmissionTime { get; set; }
    }
}