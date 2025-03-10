using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class ContestParticipantViewModel
	{
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int Score { get; set; }
        public int Penalty { get; set; }
        public int CurrentRating { get; set; }
        public int RatingChange { get; set; }
        public List<int> SolvedProblems { get; set; }
    }
}