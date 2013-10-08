namespace net.opgenorth.yegvote.droid.Model
{
    using System;

    public class Candidate
    {
        public bool Acclaimed { get; set; }
        public string Name { get; set; }
        public float Percentage { get; set; }
        public Guid UUID { get; set; }
        public int VotesReceived { get; set; }
        public Ward Ward { get; set; }
        public DateTime ReportedAt { get; set; }
    }
}
