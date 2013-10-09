using System;

namespace YegVote2013.Droid.Model
{
    public class Candidate
    {
        public bool Acclaimed { get; set; }
        public string Name { get; set; }
        public float Percentage { get; set; }
        public DateTime ReportedAt { get; set; }
        public Guid UUID { get; set; }
        public int VotesReceived { get; set; }
        public Ward Ward { get; set; }
    }
}
