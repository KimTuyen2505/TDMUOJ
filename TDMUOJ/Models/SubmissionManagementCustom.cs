using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
    public class SubmissionManagementCustom
    {
        public int id { get; set; }
        public string username { get; set; }
        public string problemTitle { get; set; }
        public string language { get; set; }
        public string status { get; set; }
        public string code { get; set; }
        public Nullable<System.DateTime> createdAt { get; set; }
    }
}