using System;

namespace YegVote2013.Droid.Model
{
    public class ElectionResult
    {
        public bool Acclaimed { get; set; }
        public string Address { get; set; }
        public string CandidateName { get; set; }
        public string Contest { get; set; }
        public int Id { get; set; }
        public int OutOf { get; set; }
        public float Percentage { get; set; }
        public int Race { get; set; }
        public int RaceId { get; set; }
        public DateTime ReportedAt { get; set; }
        public int Reporting { get; set; }
        public Guid UUID { get; set; }
        public int VotesCast { get; set; }
        public int VotesReceived { get; set; }
        public string WardName { get; set; }
    }
}
