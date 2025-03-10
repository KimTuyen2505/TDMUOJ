using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TDMUOJ.Models
{
	public class TopUsersViewModel
	{
        public List<AccountViewModel> AccountList { get; set; }
        public List<ProblemViewModel> NewProblemList { get; set; }
    }
}