using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class ProblemViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public int numberOfAccepted { get; set; }
    }
    public class CommentViewModel
    {
        public int id { get; set; }
        public string content { get; set; }
        public int userId { get; set; }
        public string username { get; set; }
        public DateTime createdAt { get; set; }
    }
    public class ContestDetail
    {
        public int id { get; set; }
        public string title { get; set; }
        public string rules { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public List<ProblemViewModel> problems { get; set; }
        public List<CommentViewModel> comments { get; set; }
    }
}