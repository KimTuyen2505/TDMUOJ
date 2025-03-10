using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class ContestLeaderboardViewModel
	{
        public Contest Contest { get; set; }
        public List<ContestParticipantViewModel> Participants { get; set; }
        public List<ProblemViewModel> Problems { get; set; }
        public List<Submission> Submissions { get; set; }
    }
    //public class ContestParticipantViewModel
    //{
    //    public int Rank { get; set; }
    //    public int UserId { get; set; }
    //    public string Username { get; set; }
    //    public int Score { get; set; }
    //    public int Penalty { get; set; }
    //    public int? CurrentRating { get; set; }
    //    public int? RatingChange { get; set; }
    //    public List<int> SolvedProblems { get; set; }
    //}
}