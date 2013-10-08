namespace net.opgenorth.yegvote.droid.Model
{
    using System;
    using System.Collections.Generic;

    public class Ward
    {
        public List<Candidate> Candidates { get; private set; }
        public string Contest { get; set; }
        public string Name { get; set; }
        public int OutOf { get; set; }
        public int Race { get; set; }
        public int RaceId { get; set; }
        public int Reporting { get; set; }
        public Guid UUID { get; set; }
        public int VotesCast { get; set; }
        public DateTime LastUpdatedAt { get; set; }

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

        public Candidate AddCandiate(ElectionResult electionResult)
        {
            var candidate = new Candidate();
            candidate.Ward = this;
            candidate.Name = electionResult.CandidateName;
            candidate.VotesReceived = electionResult.VotesReceived;
            candidate.Percentage = electionResult.Percentage;
            candidate.UUID = electionResult.UUID;
            candidate.Acclaimed = electionResult.Acclaimed;
            candidate.ReportedAt = electionResult.ReportedAt;

            Candidates.Add(candidate);
            return candidate;
        }
    }
}
