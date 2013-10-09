namespace YegVote2013.Droid.Model
{
    using System.Collections.Generic;

    public interface IHaveElectionResults
    {
        List<ElectionResult> ElectionResults { get; }
    }
}
