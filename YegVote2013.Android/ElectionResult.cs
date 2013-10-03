using Java.Sql;
using System.Collections;
using System.Collections.Generic;

namespace net.opgenorth.yegvote.droid
{
    using System;


	public class CandidateSorter : IComparer<Candidate>
	{
		public int Compare (Candidate x, Candidate y)
		{
			if (x.Acclaimed)
			{
				return 1;
			}
			if (y.Acclaimed)
			{
				return -1;
			}

			int result= y.Percentage.CompareTo(x.Percentage);
			if (result == 0)
			{
				result = x.Name.CompareTo(y.Name);
			}

			return result;
		}
	}
	public class Candidate 
	{
		public Guid UUID { get; set; }
		public string Name { get; set; }
		public int VotesReceived { get; set; }
		public Ward Ward { get; set; }
		public float Percentage { get; set; }
		public bool Acclaimed { get; set;}

	}
	public class Ward 
	{ 
		public string Name { get; set; }
		public int Reporting { get; set; }
		public int OutOf { get; set; }
		public int VotesCast { get; set; }
		public int RaceId { get; set; }
		public int Race { get; set;}
		public string Contest { get; set; }
		public Guid UUID { get; set; } 
		public List<Candidate> Candidates { get; private set; }

		public Candidate AddCandiate(ElectionResult electionResult)
		{
			var candidate = new Candidate();
			candidate.Ward = this;
			candidate.Name = electionResult.CandidateName;
			candidate.VotesReceived = electionResult.VotesReceived;
			candidate.Percentage = electionResult.Percentage;
			candidate.UUID = electionResult.UUID;
			candidate.Acclaimed = electionResult.Acclaimed;
			Candidates.Add(candidate);
			return candidate;
		}

		public static Ward NewInstance(ElectionResult electionResult) 
		{
			var ward = new Ward();
			ward.Race = electionResult.Race;
			ward.RaceId = electionResult.RaceId;
			ward.Name = electionResult.WardName;
			ward.VotesCast = electionResult.VotesCast;
			ward.Contest = electionResult.Contest;
			ward.OutOf = electionResult.OutOf;
			ward.Reporting = electionResult.Reporting;
			ward.UUID = electionResult.UUID;
			ward.Candidates = new List<Candidate>();
			ward.AddCandiate(electionResult);
			return ward;
		}
	}

    public class ElectionResult
    {
        public string Address { get; set; }
        public Guid UUID { get; set;  }
        public int Id { get; set; }
        public DateTime ReportedAt { get; set; }
        public int RaceId { get; set; }
        public string Contest { get; set; }
        public string WardName { get; set; }
        public bool Acclaimed { get; set; }
        public int Reporting { get; set; }
        public int OutOf { get; set; }
        public int VotesCast { get; set; }
        public int Race { get; set; }
        public string CandidateName { get; set; }
        public int VotesReceived { get; set; }
        public float Percentage { get; set; }
    }
}