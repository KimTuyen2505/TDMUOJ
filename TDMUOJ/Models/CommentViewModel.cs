using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class CommentViewModel
	{
        public int id { get; set; }
        public string content { get; set; }
        public DateTime createdAt { get; set; }
        public int userId { get; set; }
        public string username { get; set; }
    }
}