using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ContestDetail
    {
        public int id { get; set; }
        public string title { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string rules { get; set; }
        public List<ProblemViewModel> problems { get; set; }
        public List<CommentViewModel> comments { get; set; }
        public bool IsParticipant { get; set; }
        public bool IsVirtualParticipant { get; set; }
        public DateTime? VirtualStartTime { get; set; }
        public DateTime? VirtualEndTime { get; set; }
    }
}