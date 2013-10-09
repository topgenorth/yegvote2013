using System.Collections.Generic;

namespace YegVote2013.Droid.Model
{
    public interface IHaveElectionResults
    {
        List<ElectionResult> ElectionResults { get; }
    }
}
